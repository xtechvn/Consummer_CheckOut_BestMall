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
using Newtonsoft.Json;
using StackExchange.Redis;
using System.Configuration;
using System.Net;
using System.Text;
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
            OrderMergeSummitModel result = new OrderMergeSummitModel();
            try
            {
                var time = DateTime.Now;
                var order = await orderDetailMongoDbModel.FindById(order_detail_id);
                if (order == null || order.carts == null || order.carts.Count <= 0)
                {
                    return null;
                }
                LogHelper.InsertLogTelegram("[APP.CHECKOUT] MainServices - CreateOrder orderDetailMongoDbModel.FindById: ["+ order._id + "][" + (order.order_no==null ? "NULL" : order.order_no) + "]");
                var account_client = accountClientESService.GetById(order.account_client_id);
                var client = clientESService.GetById((long)account_client.ClientId);
                AddressClientESModel address_client = addressClientESService.GetById(order.address_id, client.Id);
                order.total_price = 0;
                order.total_profit= 0;
                var supplier_ids = order.carts.Select(x => x.product.supplier_id).GroupBy(x=>x).Select(x=>x.First());
                supplier_ids = supplier_ids.Distinct();
                int sub_order_id = 0;
                foreach (var supplier in supplier_ids)
                {
                    OrderMergeSummitOrder result_item = new OrderMergeSummitOrder()
                    {
                        order = new Order(),
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
                        var product = await productDetailMongoAccess.GetByID(parent_product_id);
                        double amount_per_unit = cart.total_amount / cart.quanity;
                        result_item.order_detail.Add(new OrderDetail()
                        {
                            CreatedDate = time,
                            Discount = 0,
                            OrderDetailId = 0,
                            OrderId = 0,
                            Price = product.price,
                            Profit = amount_per_unit - product.price,
                            Quantity = cart.quanity,
                            Amount = amount_per_unit,
                            ProductCode = cart.product.code,
                            ProductId = cart.product._id,
                            ProductLink = ConfigurationManager.AppSettings["Setting_Domain"] + "/san-pham/" + name_url + "--" + cart.product._id,
                            TotalPrice = product.price * cart.quanity,
                            TotalProfit = (amount_per_unit - product.price) * cart.quanity,
                            TotalAmount = cart.total_amount,
                            TotalDiscount = 0,
                            UpdatedDate = time,
                            UserCreate = Convert.ToInt32(ConfigurationManager.AppSettings["BOT_UserID"]),
                            UserUpdated = Convert.ToInt32(ConfigurationManager.AppSettings["BOT_UserID"]),
                            ParentProductId = parent_product_id
                        });
                        total_product_quantity += cart.quanity;
                        cart.product.price = product.price;
                        cart.product.profit = amount_per_unit - product.price;
                        cart.product.amount = amount_per_unit;
                        total_profit += (amount_per_unit - product.price) * cart.quanity;
                        total_price += product.price * cart.quanity;
                        total_amount += cart.total_amount;
                        if (!list_supplier.Contains(cart.product.supplier_id))
                        {
                            list_supplier.Add(cart.product.supplier_id);
                        }
                    }
                   
                    order.total_price += total_price;
                    order.total_profit += total_profit;
                    result_item.order = new Order()
                    {
                        Amount = total_amount,
                        ClientId = (long)account_client.ClientId,
                        CreatedDate = DateTime.Now,
                        Discount = 0,
                        IsDelete = 0,
                        Note = "",
                        OrderId = 0,
                        OrderNo = order.order_no+"-"+sub_order_id,
                        PaymentStatus = 0,
                        PaymentType = Convert.ToInt16(order.payment_type),
                        Price = total_price,
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
                        ShippingFee = order.shipping_fee,
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

                    result_item.order.VoucherId = order.voucher_id;
                    result_item.order.Discount = order.total_discount;
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
                    if(order.delivery_order!=null && order.delivery_order.Count > 0)
                    {
                        var delivery_selected = order.delivery_order.FirstOrDefault(x => x.SupplierId == supplier);
                        if (delivery_selected != null) {
                            result_item.order.Amount += delivery_selected.shipping_fee;
                            result_item.order.Profit -= delivery_selected.shipping_fee;
                            result_item.order.ShippingFee = delivery_selected.shipping_fee;
                            LogHelper.InsertLogTelegram("[APP.CHECKOUT] MainServices - CreateOrder [" + supplier + "] order.delivery_order:" + result_item.order.ShippingFee);
                        }

                    }
                    if (order.voucher_apply != null && order.voucher_apply.Count > 0)
                    {
                        var delivery_selected = order.voucher_apply.FirstOrDefault(x => x.SupplierId == supplier);
                        if (delivery_selected != null)
                        {
                            result_item.order.Amount -= delivery_selected.TotalDiscount;
                            result_item.order.Profit -= delivery_selected.TotalDiscount;
                            result_item.order.Discount = delivery_selected.TotalDiscount;
                            result_item.order.VoucherId = delivery_selected.voucher_id;
                            LogHelper.InsertLogTelegram("[APP.CHECKOUT] MainServices - CreateOrder [" + supplier + "] order.voucher_apply: [" + delivery_selected.voucher_id + "] ["+ delivery_selected.TotalDiscount + "]" );

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
                   ShippingFee= (order.delivery_order != null && order.delivery_order.Count > 0)? order.delivery_order.Sum(x => x.shipping_fee) : 0
                };
                var order_merge_id = await orderMergeDAL.InsertOrderMerge(result.order_merge);
                result.order_merge.Id = order_merge_id;
                LogHelper.InsertLogTelegram("OrderMerge Created - " + result.order_merge.OrderNo + " - " + result.order_merge.Amount);
                workQueueClient.SyncES(order_merge_id, "SP_GetOrderMerge", "hulotoys_sp_getordermerge", Convert.ToInt16(ProjectType.HULOTOYS));
                foreach(var result_item in result.detail)
                {
                    result_item.order.OrderMergeId = order_merge_id;
                    var order_id = await orderDAL.CreateOrder(result_item.order);
                    LogHelper.InsertLogTelegram("Order Created - " + order.order_no + " - " + result_item.order.Amount);
                    workQueueClient.SyncES(order_id, "SP_GetOrder", "hulotoys_sp_getorder", Convert.ToInt16(ProjectType.HULOTOYS));
                    if (order_id > 0)
                    {
                        order.order_id = order_id;
                        order.order_no = result_item.order.OrderNo;
                        foreach (var detail in result_item.order_detail)
                        {
                            detail.OrderId = order_id;
                            detail.OrderMergeId = order_merge_id;
                            await orderDetailDAL.CreateOrderDetail(detail);
                            Console.WriteLine("Created OrderDetail - " + detail.OrderId + ": " + detail.OrderDetailId);
                            LogHelper.InsertLogTelegram("OrderDetail Created - " + detail.OrderId + ": " + detail.OrderDetailId);
                        }
                        await orderDetailMongoDbModel.Update(order);

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

                        }
                        catch { }
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
      
       
    }
}
