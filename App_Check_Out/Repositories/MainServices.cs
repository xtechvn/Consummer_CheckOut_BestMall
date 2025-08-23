using ADAVIGO_FRONTEND.Models.Flights.TrackingVoucher;
using APP_CHECKOUT.DAL;
using APP_CHECKOUT.Helpers;
using APP_CHECKOUT.Interfaces;
using APP_CHECKOUT.Model.Orders;
using APP_CHECKOUT.Models.Location;
using APP_CHECKOUT.Models.Models.Queue;
using APP_CHECKOUT.Models.Orders;
using APP_CHECKOUT.Models.ViettelPost;
using APP_CHECKOUT.MongoDb;
using APP_CHECKOUT.RabitMQ;
using APP_CHECKOUT.Utilities.constants;
using APP_CHECKOUT.Utilities.Lib;
using Caching.Elasticsearch;
using Caching.Elasticsearch.FlashSale;
using DAL;
using Entities.Models;
using HuloToys_Service.Controllers.Product.Bussiness;
using HuloToys_Service.Controllers.Shipping.Business;
using HuloToys_Service.RedisWorker;
using HuloToys_Service.Utilities.lib;
using Nest;
using Newtonsoft.Json;
using StackExchange.Redis;
using System.Configuration;
using System.Net;
using System.Text;
using Telegram.Bot.Types;
using Utilities.Contants;
using static MongoDB.Driver.WriteConcern;

namespace APP_CHECKOUT.Repositories
{
    public class MainServices: IMainServices
    {
        private readonly OrderMongodbService orderDetailMongoDbModel;
        private readonly ProductDetailMongoAccess productDetailMongoAccess;
        private readonly OrderDAL orderDAL;
        private readonly LocationDAL locationDAL;
        private readonly OrderDetailDAL orderDetailDAL;
        private readonly AccountClientESService accountClientESService;
        private readonly ClientESService clientESService;
        private readonly AddressClientESService addressClientESService;
        private readonly NhanhVnService nhanhVnService;
        private readonly WorkQueueClient workQueueClient;
        private readonly EmailService emailService;
        private readonly FlashSaleESRepository flashSaleESRepository;
        private readonly FlashSaleProductESRepository flashSaleProductESRepository;
        private readonly ProductDetailService productDetailService;
        private readonly ViettelPostService _viettelPostService;
        private readonly SupplierESRepository _supplierESRepository;
        private readonly ProductDetailMongoAccess _productDetailMongoAccess;
        private readonly RedisConn _redisConn;
        private readonly OrderMergeDAL orderMergeDAL;
        private readonly BesmalPriceFormulaManager besmalPriceFormulaManager;
        private readonly VoucherDAL voucherDAL;
        private readonly NotificationService notificationService;

        public MainServices( ViettelPostService viettelPostService) {

            orderDetailMongoDbModel = new OrderMongodbService();
            productDetailMongoAccess = new ProductDetailMongoAccess();
            orderDAL = new OrderDAL(ConfigurationManager.AppSettings["ConnectionString"]);
            orderMergeDAL = new OrderMergeDAL(ConfigurationManager.AppSettings["ConnectionString"]);
            locationDAL = new LocationDAL(ConfigurationManager.AppSettings["ConnectionString"]);
            orderDetailDAL = new OrderDetailDAL(ConfigurationManager.AppSettings["ConnectionString"]);
            accountClientESService = new AccountClientESService(ConfigurationManager.AppSettings["Elastic_Host"]);
            clientESService = new ClientESService(ConfigurationManager.AppSettings["Elastic_Host"]);
            addressClientESService = new AddressClientESService(ConfigurationManager.AppSettings["Elastic_Host"]);
            flashSaleESRepository = new FlashSaleESRepository(ConfigurationManager.AppSettings["Elastic_Host"]);
            flashSaleProductESRepository = new FlashSaleProductESRepository(ConfigurationManager.AppSettings["Elastic_Host"]);
            _supplierESRepository = new SupplierESRepository(ConfigurationManager.AppSettings["Elastic_Host"]);
            nhanhVnService = new NhanhVnService();
            workQueueClient = new WorkQueueClient();
            emailService = new EmailService(clientESService, accountClientESService, locationDAL);
            productDetailService=new ProductDetailService(clientESService,flashSaleESRepository,flashSaleProductESRepository,productDetailMongoAccess);
            _viettelPostService = viettelPostService;
            _productDetailMongoAccess = new ProductDetailMongoAccess();
            try
            {
                _redisConn = new RedisConn();
                _redisConn.Connect();
            }
            catch { }
            besmalPriceFormulaManager=new BesmalPriceFormulaManager();
            voucherDAL = new VoucherDAL(ConfigurationManager.AppSettings["ConnectionString"]);
            notificationService = new NotificationService();
        }
        public async Task Excute(CheckoutQueueModel request)
        {
            try
            {
                if (request == null || request.event_id<0) {
                    return;
                }
                switch (request.event_id)
                {
                    case (int)CheckoutEventID.CREATE_ORDER:
                        {
                           var data=  await CreateOrder(request.order_mongo_id);
                            if (data != null && data.data_mongo != null&& data.data_mongo._id != null && data.data_mongo._id.Trim() != "")
                            {
                                await notificationService.SendMessage((data.order_merge.UserId==null?0:(int)data.order_merge.UserId).ToString(),"", "0", data.order_merge.OrderNo, "/Order/");
                                emailService.SendOrderConfirmationEmail(data.data_mongo.email, data);
                            }
                        }break;
                    case (int)CheckoutEventID.UPDATE_ORDER:
                        {

                        }
                        break;
                    case (int)CheckoutEventID.DELETE_ORDER:
                        {

                        }
                        break;
                   default:
                        {
                            LogHelper.InsertLogTelegram("[APP.CHECKOUT] MainServices - excute: fail case: ["+JsonConvert.SerializeObject(request) +"]" );

                        }
                        break;
                }
            }
            catch (Exception ex) {
                string err = "MainServices: " + ex;
                Console.WriteLine(err);
                LogHelper.InsertLogTelegram("[APP.CHECKOUT] MainServices - err:"+ err);

            }
        }
        private async Task<OrderMergeSummitModel> CreateOrder(string order_detail_id)
        {
            OrderMergeSummitModel result = new OrderMergeSummitModel()
            {
                data_mongo = new OrderDetailMongoDbModelExtend(),
                detail = new List<OrderMergeSummitOrder>(),
                order_merge = new OrderMerge()
            };
            try
            {
                var time = DateTime.Now;
                var order = await orderDetailMongoDbModel.FindById(order_detail_id);
                if (order == null || order.carts == null || order.carts.Count <= 0)
                {
                    return null;
                }
                var account_client = accountClientESService.GetById(order.account_client_id);
                var client = clientESService.GetById((long)account_client.ClientId);
                AddressClientESModel address_client = addressClientESService.GetById(order.address_id, client.Id);
                order.total_price = 0;
                order.total_profit= 0;
                var supplier_ids = order.carts.Select(x => x.product.supplier_id).GroupBy(x=>x).Select(x=>x.First());
                supplier_ids = supplier_ids.Distinct();
                int sub_order_id = 0;
                double voucher_total_discount = 0;

                //-- voucher:
                double shipper_voucher_total_discount = 0;
                if (order.voucher_apply != null && order.voucher_apply.Count > 0)
                {
                    var shipper_voucher = order.voucher_apply.FirstOrDefault(x => x.RuleType == 1);
                    if (shipper_voucher != null && shipper_voucher.PriceSales != null)
                    {
                        shipper_voucher_total_discount = shipper_voucher.TotalDiscount;

                        voucher_total_discount += shipper_voucher.TotalDiscount;

                    }
                }
                double product_total_discount = 0;
                if (order.voucher_apply != null && order.voucher_apply.Count > 0)
                {
                    var shipper_voucher = order.voucher_apply.FirstOrDefault(x => x.RuleType == 0);
                    if (shipper_voucher != null && shipper_voucher.PriceSales != null)
                    {

                        product_total_discount = shipper_voucher.TotalDiscount;
                        voucher_total_discount += shipper_voucher.TotalDiscount;
                    }
                }

                //split by supplier
                foreach (var supplier in supplier_ids)
                {
                    OrderMergeSummitOrder result_item = new OrderMergeSummitOrder()
                    {
                        order = new Entities.Models.Order(),
                        order_detail = new List<OrderDetail>()
                    };
                    var cart_belong_to_supplier = order.carts.Where(x => x.product.supplier_id == supplier);
                    sub_order_id++;
                    int total_weight = 0;
                    var list_supplier = new List<int>();
                    var list_cart = new List<CartItemMongoDbModel>();
                    int total_product_quantity = 0;
                    double total_profit = 0;
                    double total_price = 0;
                    double total_amount = 0;
                    foreach (var cart in cart_belong_to_supplier)
                    {
                        if (cart == null || cart.product == null) continue;
                        list_cart.Add(cart);
                        string name_url = CommonHelpers.RemoveUnicode(cart.product.name);
                        name_url = CommonHelpers.RemoveSpecialCharacters(name_url);
                        name_url = name_url.Replace(" ", "-").Trim();
                        string parent_product_id = cart.product._id;
                        if (cart.product != null && cart.product.parent_product_id != null && cart.product.parent_product_id.Trim() != "")
                        {
                            parent_product_id = cart.product.parent_product_id;
                        }
                        var product_amount_after_sale = cart.product.amount;
                        if (cart.product.amount_after_flashsale != null && cart.product.amount_after_flashsale > 0)
                        {
                            product_amount_after_sale = (double)cart.product.amount_after_flashsale;


                        }
                        var product = await productDetailMongoAccess.GetByID(cart.product._id);
                        double amount_per_unit = cart.total_amount / cart.quanity;
                        //double order_detail_profit = CalculateTotalProfitProduct(amount_per_unit, product.profit, product.price, cart.quanity,order.payment_type,order.utm_medium
                        //    , Convert.ToDouble((cart!=null && cart.product!=null && cart.product.profit_affliate!=null) ?cart.product.profit_affliate:0)
                        //    , Convert.ToDouble((order != null && order.profit_vnpay != null && order.profit_vnpay > 0) ? order.profit_vnpay : 0)

                        //    );
                        double base_profit_value = Convert.ToDouble(cart.product.profit_value == null ? 0 : cart.product.profit_value);
                        double profit_value = base_profit_value;
                        if (cart.product.profit_value_type != null && cart.product.profit_value_type == 0)
                        {
                            profit_value = Math.Round(base_profit_value / product.amount * 100, 0);
                        }

                        double base_profit_supplier_value = Convert.ToDouble(cart.product.profit_supplier == null ? 0 : cart.product.profit_supplier);
                        double profit_supplier_value = base_profit_supplier_value;
                        if (cart.product.profit_supplier_type != null && cart.product.profit_supplier_type == 0)
                        {
                            profit_supplier_value = Math.Round(base_profit_supplier_value / product.amount * 100, 0);
                        }
                        double flashsale_percent = (cart.product.flash_sale_price_sales == null || cart.product.amount_after_flashsale == null || cart.product.amount_after_flashsale <=0) ?0: Convert.ToDouble(cart.product.flash_sale_price_sales);

                        if ((cart.product.flash_sale_unit == null && flashsale_percent>0) || cart.product.flash_sale_unit == 0)
                        {
                            flashsale_percent= Math.Round(Convert.ToDouble(cart.product.flash_sale_price_sales) / product.amount * 100,0);
                        }
                      
                        
                        //-- calculate price:
                        var order_detail_price = besmalPriceFormulaManager.tinh_gia_nhap(
                          Convert.ToDecimal(product.amount)
                           , Convert.ToDecimal(profit_supplier_value / 100)
                          , Convert.ToDecimal(profit_value / 100)
                          );
                        var order_detail_profit = besmalPriceFormulaManager.tinh_loi_nhuan_tam_tinh_sau_sale(
                            Convert.ToDecimal(product.amount)
                            , Convert.ToDecimal(profit_value / 100)
                            , Convert.ToDecimal(profit_supplier_value / 100)
                            , Convert.ToDecimal(flashsale_percent/100)
                            , cart.quanity);
                        var order_detail_final_profit = besmalPriceFormulaManager.tinh_loi_nhuan_rong_sau_sale_v2(
                            Convert.ToDecimal(product.amount)
                            , Convert.ToDecimal(profit_value/100)
                            , Convert.ToDecimal(profit_supplier_value / 100)
                            , Convert.ToDecimal(flashsale_percent / 100)
                            , cart.quanity
                            ,order.utm_medium!=null && order.utm_medium.Trim()!=""? Convert.ToDecimal(cart.product.profit_affliate / 100) :0
                            , order.payment_type != null && order.payment_type==3 ? Convert.ToDecimal(order.profit_vnpay / 100) : 0
                            , Convert.ToDecimal(Math.Ceiling(shipper_voucher_total_discount / order.carts.Count))
                            , Convert.ToDecimal(Math.Ceiling(product_total_discount / order.carts.Count))
                            , 0
                            , 0
                            ,Convert.ToDecimal(order.total_amount)
                            );
                        //LogHelper.InsertLogTelegram(@"[APP.CHECKOUT] MainServices - order_detail_profit = besmalPriceFormulaManager.tinh_loi_nhuan_tam_tinh_sau_sale(
                        //    " + Convert.ToDecimal(product.amount) + @"
                        //    , " + Convert.ToDecimal(profit_value / 100) + @"
                        //     , " + Convert.ToDecimal(profit_supplier_value / 100) + @"
                        //     , " + Convert.ToDecimal(flashsale_percent / 100) + @"
                        //     , " + cart.quanity + @"

                        //    );: [" + order_detail_profit + "]");
                        LogHelper.InsertLogTelegram(@"[APP.CHECKOUT] MainServices - order_detail_final_profit = besmalPriceFormulaManager.tinh_loi_nhuan_rong_sau_sale_v2(
                            " + Convert.ToDecimal(product.amount) + @"
                            , " + Convert.ToDecimal(profit_value / 100) + @"
                             , " + Convert.ToDecimal(profit_supplier_value / 100) + @"
                             , " + Convert.ToDecimal(flashsale_percent / 100) + @"
                             , " + cart.quanity + @"
                            , " + (order.utm_medium != null && order.utm_medium.Trim() != "" ? Convert.ToDecimal(cart.product.profit_affliate / 100) : 0) + @"
                            , " + (order.payment_type != null && order.payment_type == 3 ? Convert.ToDecimal(order.profit_vnpay / 100) : 0) + @"
                             , " + Convert.ToDecimal(Math.Ceiling(shipper_voucher_total_discount / order.carts.Count)) + @"
                             , " + Convert.ToDecimal(Math.Ceiling(product_total_discount / order.carts.Count)) + @"
                             , " + 0 + @"
                             , " + 0 + @"
                             , " + Convert.ToDecimal(product_amount_after_sale) + @"
                            );: [" + order_detail_final_profit + "]");
                        //order_detail_profit = StringHelper.RoundUp(order_detail_profit);
                        // order_detail_final_profit = StringHelper.RoundUp(order_detail_final_profit);
                        var order_detail = new OrderDetail()
                        {
                            CreatedDate = time,
                            Discount = 0,
                            OrderDetailId = 0,
                            OrderId = 0,
                            Price = Convert.ToDouble(order_detail_price),
                            Profit = Convert.ToDouble(order_detail_profit)/ cart.quanity,
                            Quantity = cart.quanity,
                            Amount = amount_per_unit,
                            ProductCode = cart.product.code,
                            ProductId = cart.product._id,
                            ProductLink = ConfigurationManager.AppSettings["Setting_Domain"] + "/san-pham/" + name_url + "--" + cart.product._id,
                            TotalPrice = Convert.ToDouble(order_detail_price) * cart.quanity,
                            TotalProfit = Convert.ToDouble(order_detail_profit),
                            TotalAmount = cart.total_amount,
                            TotalDiscount = 0,
                            UpdatedDate = time,
                            UserCreate = Convert.ToInt32(ConfigurationManager.AppSettings["BOT_UserID"]),
                            UserUpdated = Convert.ToInt32(ConfigurationManager.AppSettings["BOT_UserID"]),
                            ParentProductId = parent_product_id,
                            FinalProfit = Convert.ToDouble(order_detail_final_profit)
                        };
                        result_item.order_detail.Add(order_detail);
                        total_product_quantity += cart.quanity;
                        cart.total_price = Convert.ToDouble(order_detail_price) * cart.quanity;
                        cart.total_profit = Convert.ToDouble(order_detail_final_profit);
                        cart.product.amount = amount_per_unit;
                        total_profit += Convert.ToDouble(order_detail_final_profit);
                        total_price += Convert.ToDouble(order_detail_price) * cart.quanity;
                        total_amount += cart.total_amount;
                        if (!list_supplier.Contains(cart.product.supplier_id))
                        {
                            list_supplier.Add(cart.product.supplier_id);
                        }
                    }

                    order.total_price += total_price;
                    order.total_profit += total_profit;
                    string order_no = (supplier_ids == null || supplier_ids.Count() <= 1  ? order.order_no : order.order_no + "-" + sub_order_id);
                    double shipping_fee_supplier = 0;
                    //--shipping fee
                    if (order.delivery_order!=null && order.delivery_order.Count > 0)
                    {
                        var shipping_supplier = order.delivery_order.FirstOrDefault(x => x.SupplierId == supplier);
                        if (shipping_supplier != null)
                        {
                            shipping_fee_supplier=shipping_supplier.shipping_fee==null?0:(double)shipping_supplier.shipping_fee;
                        }
                    }
                    //LogHelper.InsertLogTelegram("result_item.order - ["+ total_amount + "]["+ shipping_fee_supplier + "]["+ voucher_total_discount + "]");

                    result_item.order = new Entities.Models.Order()
                    {
                        Amount = total_amount+ shipping_fee_supplier - voucher_total_discount,
                        ClientId = (long)account_client.ClientId,
                        CreatedDate = DateTime.Now,
                        Discount = voucher_total_discount,
                        IsDelete = 0,
                        Note = "",
                        OrderId = 0,
                        OrderNo = order_no,
                        PaymentStatus = 0,
                        PaymentType = Convert.ToInt16(order.payment_type),
                        Price = total_amount,
                        Profit = total_profit,
                        OrderStatus = 0,
                        UpdateLast = time,
                        UserGroupIds = "",
                        UserId = Convert.ToInt32(ConfigurationManager.AppSettings["BOT_UserID"]),
                        UtmMedium = order.utm_medium,
                        UtmSource = order.utm_source,
                        VoucherId = order.voucher_id,
                        CreatedBy = Convert.ToInt32(ConfigurationManager.AppSettings["BOT_UserID"]),
                        UserUpdateId = Convert.ToInt32(ConfigurationManager.AppSettings["BOT_UserID"]),
                        Address = order.address,
                        ReceiverName = order.receivername,
                        Phone = order.phone,
                        ShippingFee = shipping_fee_supplier,
                        CarrierId = order.delivery_detail.carrier_id,
                        ShippingCode = "",
                        ShippingType = order.delivery_detail.shipping_type,
                        ShippingStatus = 0,
                        PackageWeight = total_weight,
                        ShippingTypeCode = order.delivery_detail.shipping_service_code == null ? "" : order.delivery_detail.shipping_service_code,
                        
                    };

                    List<Province> provinces = GetProvince();
                    List<District> districts = GetDistrict();
                    List<Ward> wards = GetWards();
                    if (address_client != null && address_client.ProvinceId != null && address_client.DistrictId != null && address_client.WardId != null)
                    {
                        if (address_client.ProvinceId.Trim() != "" && provinces != null && provinces.Count > 0)
                        {
                            var province = provinces.FirstOrDefault(x => x.Id == Convert.ToInt32(address_client.ProvinceId));
                            result_item.order.ProvinceId = province != null ? province.Id : null;
                        }
                        if (address_client.DistrictId.Trim() != "" && districts != null && districts.Count > 0)
                        {
                            var district = districts.FirstOrDefault(x => x.Id == Convert.ToInt32(address_client.DistrictId));
                            result_item.order.DistrictId = district != null ? district.Id : null;
                        }
                        if (address_client.WardId.Trim() != "" && wards != null && wards.Count > 0)
                        {
                            var ward = wards.FirstOrDefault(x => x.Id == Convert.ToInt32(address_client.WardId));
                            result_item.order.WardId = ward != null ? ward.Id : null;
                        }
                        result_item.order.ReceiverName = address_client.ReceiverName;
                        result_item.order.Phone = address_client.Phone;
                        result_item.order.Address = address_client.Address;
                    }
                    else
                    {
                        var province = provinces.FirstOrDefault(x => x.Id == Convert.ToInt32(order.provinceid));
                        result_item.order.ProvinceId = province != null ? province.Id : null;
                        var district = districts.FirstOrDefault(x => x.Id == Convert.ToInt32(order.districtid));
                        result_item.order.DistrictId = district != null ? district.Id : null;
                        var ward = wards.FirstOrDefault(x => x.Id == Convert.ToInt32(order.wardid));
                        result_item.order.WardId = ward != null ? ward.Id : null;
                        result_item.order.ReceiverName = order.receivername;
                        result_item.order.Phone = order.phone;
                        result_item.order.Address = order.address;
                    }
                    //-- Shipping token
                    if (order.delivery_detail != null && order.delivery_detail.carrier_id > 0)
                    {
                        switch (order.delivery_detail.carrier_id)
                        {

                            case 1:
                                {

                                }
                                break;
                            case 2: { } break;
                            //---- ViettelPost
                            case 3:
                                {
                                    List<VTPOrderRequestModel> model = new List<VTPOrderRequestModel>();
                                    if (order.delivery_detail.shipping_service_code != null && order.delivery_detail.shipping_service_code.Trim() != "")
                                    {
                                        var detail_supplier = await _supplierESRepository.GetByIdAsync(supplier);
                                        int package_weight = 0;
                                        foreach (var c in cart_belong_to_supplier)
                                        {
                                            var selected = list_cart.First(x => x._id == c._id);
                                            package_weight += Convert.ToInt32(((c.product.weight <= 0 ? 0 : c.product.weight) * selected.quanity));

                                        }
                                        string package_name = string.Join(",", cart_belong_to_supplier.Select(x => x.product.name));
                                        VTPOrderRequestModel item = new VTPOrderRequestModel()
                                        {
                                            ORDER_NUMBER = result_item.order.OrderNo,
                                            CHECK_UNIQUE = false,
                                            CUS_ID = 0,
                                            DELIVERY_DATE = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"),
                                            EXTRA_MONEY = 0,
                                            GROUPADDRESS_ID = 0,
                                            LIST_ITEM = new List<VTPOrderRequestListItem>(),
                                            MONEY_COLLECTION = 0,
                                            ORDER_NOTE = "Cho xem hàng, không cho thử",
                                            ORDER_PAYMENT = 1,
                                            ORDER_SERVICE = order.delivery_detail.shipping_service_code,
                                            ORDER_SERVICE_ADD = "",
                                            ORDER_VOUCHER = "",
                                            PRODUCT_DESCRIPTION = "Trong đơn chứa tổng cộng " + total_product_quantity + " sản phẩm",
                                            PRODUCT_HEIGHT = 0,
                                            PRODUCT_LENGTH = 0,
                                            PRODUCT_NAME = package_name,
                                            PRODUCT_PRICE = Convert.ToInt32(order.total_amount),
                                            PRODUCT_QUANTITY = total_product_quantity,
                                            PRODUCT_TYPE = "HH",
                                            PRODUCT_WEIGHT = package_weight,
                                            PRODUCT_WIDTH = 0,
                                            RECEIVER_ADDRESS = order.address,
                                            RECEIVER_DISTRICT = Convert.ToInt32(order.districtid),
                                            RECEIVER_EMAIL = "",
                                            RECEIVER_FULLNAME = order.receivername,
                                            RECEIVER_LATITUDE = 0,
                                            RECEIVER_LONGITUDE = 0,
                                            RECEIVER_PHONE = order.phone,
                                            RECEIVER_PROVINCE = Convert.ToInt32(order.provinceid),
                                            RECEIVER_WARD = Convert.ToInt32(order.wardid),
                                            SENDER_ADDRESS = detail_supplier.address,
                                            SENDER_DISTRICT = detail_supplier.districtid == null ? 0 : (int)detail_supplier.districtid,
                                            SENDER_EMAIL = "",
                                            SENDER_FULLNAME = detail_supplier.fullname,
                                            SENDER_LATITUDE = 0,
                                            SENDER_LONGITUDE = 0,
                                            SENDER_PHONE = detail_supplier.phone,
                                            SENDER_PROVINCE = detail_supplier.provinceid == null ? 0 : (int)detail_supplier.provinceid,
                                            SENDER_WARD = detail_supplier.wardid == null ? 0 : (int)detail_supplier.wardid,
                                        };
                                        model.Add(item);
                                    }
                                    result_item.order.ShippingToken = JsonConvert.SerializeObject(model);
                                    
                                }
                                break;
                            default:
                                {

                                }
                                break;
                        }
                        

                    }
                    result_item.order.SupplierId = supplier;
                    //--Payment Type:
                    if (result_item.order.PaymentType == 1)
                    {
                        result_item.order.OrderStatus = 1;
                    }
                    result.detail.Add(result_item);
                }


                var extend_order = JsonConvert.DeserializeObject<OrderDetailMongoDbModelExtend>(JsonConvert.SerializeObject(order));
                if (extend_order != null)
                {
                    extend_order.email = client.Email;
                    extend_order.created_date = time;
                    result.data_mongo = extend_order;
                }
                result.order_merge = new OrderMerge()
                {
                    Amount = order.total_amount,
                    ClientId = (long)account_client.ClientId,
                    CreatedDate = DateTime.Now,
                    Discount = 0,
                    IsDelete = 0,
                    Note = "",
                    Id = 0,
                    OrderNo = order.order_no,
                    PaymentStatus = 0,
                    PaymentType = Convert.ToInt16(order.payment_type),
                    Price = order.total_price,
                    Profit = order.total_profit,
                    OrderStatus = 0,
                    UpdateLast = time,
                    UserGroupIds = "",
                    UserId = Convert.ToInt32(ConfigurationManager.AppSettings["BOT_UserID"]),
                    UtmMedium = order.utm_medium,
                    UtmSource = order.utm_source,
                    VoucherId = order.voucher_id==null?"":string.Join(",", order.voucher_id),
                    CreatedBy = Convert.ToInt32(ConfigurationManager.AppSettings["BOT_UserID"]),
                    UserUpdateId = Convert.ToInt32(ConfigurationManager.AppSettings["BOT_UserID"]),
                    Address = order.address,
                    ReceiverName = order.receivername,
                    Phone = order.phone,
                    ShippingFee= (order.delivery_order != null && order.delivery_order.Count > 0)? order.delivery_order.Sum(x => x.shipping_fee) : 0,
                    
                };
                if (result.detail.First().order.PaymentType == 1)
                {
                    result.order_merge.OrderStatus = 1;
                }
                if (result.detail.First().order.ProvinceId >0)
                {
                    result.order_merge.ProvinceId = result.detail.First().order.ProvinceId;
                }
                if (result.detail.First().order.DistrictId > 0)
                {
                    result.order_merge.DistrictId = result.detail.First().order.DistrictId;
                }
                if (result.detail.First().order.WardId > 0)
                {
                    result.order_merge.WardId = result.detail.First().order.WardId;
                }
                if (order.voucher_apply != null && order.voucher_apply.Count > 0)
                {
                    result.order_merge.VoucherId=string.Join(",", order.voucher_apply);
                }
                var order_merge_id = await orderMergeDAL.InsertOrderMerge(result.order_merge);
                result.order_merge.Id = order_merge_id;
                LogHelper.InsertLogTelegram("OrderMerge Created - ["+ order_merge_id + "] " + result.order_merge.OrderNo + " - " + result.order_merge.Amount);
                workQueueClient.SyncES(order_merge_id, "SP_GetOrderMerge", "hulotoys_sp_getordermerge", Convert.ToInt16(ProjectType.HULOTOYS));
                foreach(var result_item in result.detail)
                {
                    result_item.order.OrderMergeId = order_merge_id;
                    var order_id = await orderDAL.CreateOrder(result_item.order);
                    LogHelper.InsertLogTelegram("Order Created - " + result_item.order.OrderNo + " - " + result_item.order.Amount);
                    workQueueClient.SyncES(order_id, "SP_GetOrder", "hulotoys_sp_getorder", Convert.ToInt16(ProjectType.HULOTOYS));
                    if (order_id > 0)
                    {
                        foreach (var detail in result_item.order_detail)
                        {
                            detail.OrderId = order_id;
                            detail.OrderMergeId = order_merge_id;
                            await orderDetailDAL.CreateOrderDetail(detail);
                            Console.WriteLine("Created OrderDetail - [" + detail.OrderId + "][ " + detail.OrderDetailId + "]");
                            LogHelper.InsertLogTelegram("OrderDetail Created - [" + detail.OrderId + "] [" + detail.OrderDetailId+"] [" + detail.Profit + "] - [" + detail.FinalProfit+"]");

                        }

                    }
                    foreach (var detail in result_item.order_detail)
                    {
                        try
                        {
                            await _productDetailMongoAccess.UpdateQuantityOfStock(detail.ProductId, (int)detail.Quantity);

                        }
                        catch { }
                        try
                        {
                            var cache_name = CacheType.PRODUCT_DETAIL + detail.ProductId;
                            _redisConn.clear(cache_name, Convert.ToInt32(ConfigurationManager.AppSettings["Redis_Database_db_search_result"]));
                            var product=await _productDetailMongoAccess.GetByID(detail.ProductId);
                            if (product != null && product.parent_product_id!=null && product.parent_product_id.Trim()!="")
                            {

                                 cache_name = CacheType.PRODUCT_DETAIL + product.parent_product_id;
                                _redisConn.clear(cache_name, Convert.ToInt32(ConfigurationManager.AppSettings["Redis_Database_db_search_result"]));
                            }
                            

                        }
                        catch { }
                    }
                }
                order.order_id = order_merge_id;
                await orderDetailMongoDbModel.Update(order);
                if (order.voucher_apply != null && order.voucher_apply.Count > 0)
                {
                    string cache_name = "VOUCHER";
                    _redisConn.clear(cache_name, Convert.ToInt32(ConfigurationManager.AppSettings["Redis_Database_db_search_result"]));
                    foreach (var voucher in order.voucher_apply)
                    {
                        var exists_voucher = await voucherDAL.FindByVoucherId(voucher.voucher_id);
                        if (exists_voucher != null && exists_voucher.Id>0) {
                            exists_voucher.LimitUse--;
                            if (exists_voucher.LimitUse <= 0)
                            {
                                exists_voucher.LimitUse = 0;
                            }
                            try
                            {
                                voucherDAL.UpdateVoucher(exists_voucher);
                                if (exists_voucher.GroupUserPriority != null && exists_voucher.GroupUserPriority.Trim() != "")
                                {
                                    List<long> client_ids = JsonConvert.DeserializeObject<List<long>>(exists_voucher.GroupUserPriority);
                                    if (client_ids != null && client_ids.Count > 0)
                                    {
                                        foreach (var client_id in client_ids)
                                        {
                                            cache_name = "VOUCHER" + client;
                                            _redisConn.clear(cache_name, Convert.ToInt32(ConfigurationManager.AppSettings["Redis_Database_db_search_result"]));
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                LogHelper.InsertLogTelegram("[APP.CHECKOUT] MainServices - CreateOrder - UpdateVoucher:" + ex);

                            }
                        }
                    }
                }


                return result;
            }
            catch (Exception ex)
            {
                string err = "CreateOrder with ["+ order_detail_id+"] error: " + ex.Message + "at "+ex.StackTrace;
                Console.WriteLine(err);
                LogHelper.InsertLogTelegram(err);
                LogHelper.InsertLogTelegram("[APP.CHECKOUT] MainServices - CreateOrder:" + err);

            }
            return null;
        }
        private List<Province> GetProvince()
        {
            List<Province> provinces = new List<Province>();
            string provinces_string = "";

            try
            {
                provinces = locationDAL.GetListProvinces();

            }
            catch
            {

            }
            return provinces;
        }
        private List<District> GetDistrict()
        {
            List<District> districts = new List<District>();
            string districts_string = "";

            try
            {
                districts = locationDAL.GetListDistrict();

            }
            catch
            {

            }
            return districts;
        }
        private List<Ward> GetWards()
        {
            List<Ward> wards = new List<Ward>();
            string wards_string = "";

            try
            {
                wards = locationDAL.GetListWard();

            }
            catch
            {

            }
            return wards;
        }
        private double CalculateTotalProfitProduct(double amount, double profit, double price,int quantity,int payment_type, string utm_medium, double affiliate_percent, double vnpay_percent)
        {
            var total_profit = profit * (quantity <= 0 ? 1 : quantity);
            try
            {
                double aff_fee = 0;
                if (utm_medium!=null && utm_medium.Trim() != "")
                {
                    aff_fee = Math.Ceiling(total_profit * affiliate_percent / 100);
                }
                double vnpay_fee = 0;
                if (payment_type == 3)
                {
                    vnpay_fee = Math.Ceiling(total_profit * vnpay_percent / 100);
                }
                total_profit = total_profit - aff_fee - vnpay_fee;
            }
            catch (Exception ex)
            {

            }
            return total_profit;
        }
       
    }
}
