using ADAVIGO_FRONTEND.Models.Flights.TrackingVoucher;
using APP.READ_MESSAGES.Libraries;
using APP_CHECKOUT.Constants;
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
using Caching.Elasticsearch;
using Caching.Elasticsearch.FlashSale;
using DAL;
using Entities.Models;
using HuloToys_Service.Controllers.Product.Bussiness;
using HuloToys_Service.Controllers.Shipping.Business;
using Nest;
using Newtonsoft.Json;
using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Text;
using Utilities.Contants;

namespace APP_CHECKOUT.Repositories
{
    public class MainServices: IMainServices
    {
        private readonly ILoggingService logging_service;
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
        public MainServices( ILoggingService loggingService) {

            logging_service=loggingService;
            orderDetailMongoDbModel = new OrderMongodbService();
            productDetailMongoAccess = new ProductDetailMongoAccess();
            orderDAL = new OrderDAL(ConfigurationManager.AppSettings["ConnectionString"]);
            locationDAL = new LocationDAL(ConfigurationManager.AppSettings["ConnectionString"]);
            orderDetailDAL = new OrderDetailDAL(ConfigurationManager.AppSettings["ConnectionString"]);
            accountClientESService = new AccountClientESService(ConfigurationManager.AppSettings["Elastic_Host"]);
            clientESService = new ClientESService(ConfigurationManager.AppSettings["Elastic_Host"]);
            addressClientESService = new AddressClientESService(ConfigurationManager.AppSettings["Elastic_Host"]);
            flashSaleESRepository = new FlashSaleESRepository(ConfigurationManager.AppSettings["Elastic_Host"]);
            flashSaleProductESRepository = new FlashSaleProductESRepository(ConfigurationManager.AppSettings["Elastic_Host"]);
            _supplierESRepository = new SupplierESRepository(ConfigurationManager.AppSettings["Elastic_Host"]);
            nhanhVnService = new NhanhVnService(logging_service);
            workQueueClient = new WorkQueueClient(loggingService);
            emailService = new EmailService(clientESService, accountClientESService, locationDAL, loggingService);
            productDetailService=new ProductDetailService(clientESService,flashSaleESRepository,flashSaleProductESRepository,productDetailMongoAccess);
            _viettelPostService = new ViettelPostService();
            _productDetailMongoAccess = new ProductDetailMongoAccess();
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
                            if (data != null && data._id != null && data._id.Trim() != "")
                            {
                                emailService.SendOrderConfirmationEmail(data.email, data);
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

                        }
                        break;
                }
            }
            catch (Exception ex) {
                string err = "MainServices: " + ex.ToString();
                Console.WriteLine(err);
                logging_service.InsertLogTelegramDirect(err);
            }
        }
        private async Task<OrderDetailMongoDbModelExtend> CreateOrder(string order_detail_id)
        {
            try
            {
                var time = DateTime.Now;
                var order = await orderDetailMongoDbModel.FindById(order_detail_id);
                if (order == null || order.carts == null || order.carts.Count <= 0)
                {
                    return null;
                }
                Order order_summit = new Order();
                List<OrderDetail> details = new List<OrderDetail>();
                double total_price = 0;
                double total_profit = 0;
                double total_amount = 0;
                float total_weight = 0;
                var list_supplier = new List<int>();
                var list_cart = new List<CartItemMongoDbModel>();
                logging_service.InsertLogTelegramDirect("CreateOrder : variable");

                foreach (var cart in order.carts)
                {
                    if (cart == null || cart.product == null) continue;
                    list_cart.Add(cart);
                    logging_service.InsertLogTelegramDirect("CreateOrder : listcart");

                    string name_url = CommonHelpers.RemoveUnicode(cart.product.name);
                    name_url = CommonHelpers.RemoveSpecialCharacters(name_url);
                    name_url = name_url.Replace(" ", "-").Trim();
                    string parent_product_id = cart.product._id;
                    //try
                    //{
                    //    var product = await productDetailService.GetByID(cart.product._id);
                    if (cart.product != null && cart.product.parent_product_id != null && cart.product.parent_product_id.Trim() != "")
                    {
                        parent_product_id = cart.product.parent_product_id;
                    }
                    //}
                    //catch { }
                    var amount_product = cart.product.amount;
                    if (cart.product.flash_sale_todate >= DateTime.Now && cart.product.amount_after_flashsale != null && cart.product.amount_after_flashsale > 0)
                    {
                        amount_product = (double)cart.product.amount_after_flashsale;

                    }
                    logging_service.InsertLogTelegramDirect("CreateOrder : amount_product");

                    details.Add(new OrderDetail()
                    {
                        CreatedDate = time,
                        Discount = cart.product.discount,
                        OrderDetailId = 0,
                        OrderId = 0,
                        Price = cart.product.price,
                        Profit = cart.product.profit,
                        Quantity = cart.quanity,
                        Amount = amount_product,
                        ProductCode = cart.product.code,
                        ProductId = cart.product._id,
                        ProductLink = ConfigurationManager.AppSettings["Setting_Domain"] + "/san-pham/" + name_url + "--" + cart.product._id,
                        TotalPrice = cart.product.price * cart.quanity,
                        TotalProfit = cart.product.profit * cart.quanity,
                        TotalAmount = amount_product * cart.quanity,
                        TotalDiscount = cart.product.discount * cart.quanity,
                        UpdatedDate = time,
                        UserCreate = Convert.ToInt32(ConfigurationManager.AppSettings["BOT_UserID"]),
                        UserUpdated = Convert.ToInt32(ConfigurationManager.AppSettings["BOT_UserID"]),
                        ParentProductId=parent_product_id
                    });
                    total_price += (cart.product.price * cart.quanity);
                    total_profit += (cart.product.profit * cart.quanity);
                    total_amount += (amount_product * cart.quanity);

                    //cart.total_price = cart.product.price * cart.quanity;
                    //cart.total_discount = cart.product.discount * cart.quanity;
                    //cart.total_profit = cart.product.profit * cart.quanity;
                    //cart.total_amount = amount_product * cart.quanity;

                    cart.total_price = cart.product.price * cart.quanity;
                    cart.total_profit = cart.product.profit * cart.quanity;
                    cart.total_amount = amount_product * cart.quanity;
                    cart.total_discount = cart.product.discount * cart.quanity;
                    total_weight += ((cart.product.weight == null ? 0 : (float)cart.product.weight) * cart.quanity / 1000);
                    if (!list_supplier.Contains(cart.product.supplier_id))
                    {
                        list_supplier.Add(cart.product.supplier_id);
                    }
                    logging_service.InsertLogTelegramDirect("CreateOrder : details");

                }
                var account_client = accountClientESService.GetById(order.account_client_id);
                logging_service.InsertLogTelegramDirect("CreateOrder : account_client");

                var client = clientESService.GetById((long)account_client.ClientId);
                logging_service.InsertLogTelegramDirect("CreateOrder : client");

                AddressClientESModel address_client = addressClientESService.GetById(order.address_id, client.Id);
                logging_service.InsertLogTelegramDirect("CreateOrder : address_client");

                order_summit = new Order()
                {
                    Amount = total_amount + order.shipping_fee,
                    ClientId = (long)account_client.ClientId,
                    CreatedDate = DateTime.Now,
                    Discount = 0,
                    IsDelete = 0,
                    Note = "",
                    OrderId = 0,
                    OrderNo = order.order_no,
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
                    

                };
                logging_service.InsertLogTelegramDirect("CreateOrder : order_summit");

                List<Province> provinces = GetProvince();
                List<District> districts = GetDistrict();
                List<Ward> wards = GetWards();
                logging_service.InsertLogTelegramDirect("CreateOrder : provinces");

                if (address_client != null && address_client.ProvinceId != null && address_client.DistrictId != null && address_client.WardId != null)
                {
                    if (address_client.ProvinceId.Trim() != "" && provinces != null && provinces.Count > 0)
                    {
                        var province = provinces.FirstOrDefault(x => x.Id == Convert.ToInt32(address_client.ProvinceId));
                        order_summit.ProvinceId = province != null ? province.Id : null;
                    }
                    if (address_client.DistrictId.Trim() != "" && districts != null && districts.Count > 0)
                    {
                        var district = districts.FirstOrDefault(x => x.Id == Convert.ToInt32(address_client.DistrictId));
                        order_summit.DistrictId = district != null ? district.Id : null;
                    }
                    if (address_client.WardId.Trim() != "" && wards != null && wards.Count > 0)
                    {
                        var ward = wards.FirstOrDefault(x => x.Id == Convert.ToInt32(address_client.WardId));
                        order_summit.WardId = ward != null ? ward.Id : null;
                    }
                    order_summit.ReceiverName = address_client.ReceiverName;
                    order_summit.Phone = address_client.Phone;
                    order_summit.Address = address_client.Address;
                }
                else
                {
                    var province = provinces.FirstOrDefault(x => x.Id == Convert.ToInt32(order.provinceid));
                    order_summit.ProvinceId = province != null ? province.Id : null;
                    var district = districts.FirstOrDefault(x => x.Id == Convert.ToInt32(order.districtid));
                    order_summit.DistrictId = district != null ? district.Id : null;
                    var ward = wards.FirstOrDefault(x => x.Id == Convert.ToInt32(order.wardid));
                    order_summit.WardId = ward != null ? ward.Id : null;
                    order_summit.ReceiverName = order.receivername;
                    order_summit.Phone = order.phone;
                    order_summit.Address = order.address;
                }
                //--apply voucher:
                double total_discount = 0;
                if (order.voucher_code != null && order.voucher_code.Trim() != "")
                {
                    var input = new TrackingVoucherRequest
                    {
                        total_order_amount_before = (double)order_summit.Amount,
                        user_id = Convert.ToInt64(order.account_client_id),
                        voucher_name = order.voucher_code,
                        token = CommonHelpers.Encode("{\"user_name\":\"" + account_client.UserName + "\"}", ConfigurationManager.AppSettings["key_private"])
                    };
                    var voucher_apply = await ApplyVoucher(input);
                    if (voucher_apply != null && voucher_apply.status == 0)
                    {
                        //    switch (voucher_apply.rule_type)
                        //    {
                        //        case (int)VoucherRuleType.ALL_PRODUCT:
                        //            {

                        //            }break;
                        //        case (int)VoucherRuleType.SPECIFIC_PRODUCT:
                        //            {

                        //            }
                        //            break;
                        //        default:
                        //            {
                        //                double percent = Convert.ToDouble(voucher_apply.value);
                        //                switch (voucher_apply.type)
                        //                {
                        //                    case "percent":
                        //                        //Tinh số tiền giảm theo %
                        //                        total_discount += ((double)order_summit.Amount * Convert.ToDouble(percent / 100));
                        //                        break;
                        //                    case "vnd":
                        //                        total_discount += percent; //Math.Min(Convert.ToDouble(voucher.LimitTotalDiscount), total_fee_not_luxury) ;
                        //                        break;

                        //                    default: break;

                        //                }
                        //                voucher_apply.discount = total_discount;
                        //                voucher_apply.total_order_amount_after = voucher_apply.total_order_amount_before - total_discount;
                        //                order_summit.VoucherId = voucher_apply.voucher_id;
                        //                order_summit.Discount = voucher_apply.discount;
                        //                order_summit.Amount = voucher_apply.total_order_amount_after;
                        //                order_summit.Profit -= order_summit.Discount;
                        //            }break;

                        //    }

                        //}
                        double percent = Convert.ToDouble(voucher_apply.value);
                        switch (voucher_apply.type)
                        {
                            case "percent":
                                total_discount += ((double)order_summit.Amount * Convert.ToDouble(percent / 100));
                                break;
                            case "vnd":
                                total_discount += percent;
                                break;

                            default: break;

                        }
                        voucher_apply.discount = total_discount;
                        voucher_apply.total_order_amount_after = voucher_apply.total_order_amount_before - total_discount;
                        order_summit.VoucherId = voucher_apply.voucher_id;
                        order_summit.Discount = voucher_apply.discount;
                        order_summit.Amount = voucher_apply.total_order_amount_after;
                        order_summit.Profit -= order_summit.Discount;
                    }

                }
                logging_service.InsertLogTelegramDirect("CreateOrder : voucher_apply");

                order.total_discount = total_discount;
                //-- Shipping fee
                if (order.delivery_detail != null && order.delivery_detail.carrier_id > 0)
                {
                    switch (order.delivery_detail.carrier_id)
                    {

                        case 1: { } break;
                        case 2: { } break;
                        //---- ViettelPost
                        case 3:
                            {
                                logging_service.InsertLogTelegramDirect("CreateOrder : carrier_id ViettelPost");

                                List<VTPOrderRequestModel> model = new List<VTPOrderRequestModel>();
                                if (order.delivery_detail.shipping_service_code != null && order.delivery_detail.shipping_service_code.Trim() != "")
                                {
                                    foreach (var supplier in list_supplier)
                                    {
                                        var cart_belong_to_supplier = list_cart.Where(x => x.product.supplier_id == supplier);
                                        var detail_supplier = await _supplierESRepository.GetByIdAsync(supplier);
                                        int package_weight = 0;
                                        int package_width = 0;
                                        int package_height = 0;
                                        int package_depth = 0;
                                        int total_quanity = 0;
                                        double amount = 0;
                                        foreach (var c in cart_belong_to_supplier)
                                        {
                                            var selected = list_cart.First(x => x._id == c._id);
                                            package_weight += Convert.ToInt32(((c.product.weight <= 0 ? 0 : c.product.weight) * selected.quanity));
                                            package_width += Convert.ToInt32(((c.product.package_width <= 0 ? 0 : c.product.package_width) * selected.quanity));
                                            package_height += Convert.ToInt32(((c.product.package_height <= 0 ? 0 : c.product.package_height) * selected.quanity));
                                            package_depth += Convert.ToInt32(((c.product.package_depth <= 0 ? 0 : c.product.package_depth) * selected.quanity));
                                            amount += Convert.ToInt32(((c.product.amount_after_flashsale == null ? c.product.amount : c.product.amount_after_flashsale) * selected.quanity));
                                            total_quanity += selected.quanity;
                                        }
                                        var response_item = await _viettelPostService.GetShippingMethods(new VTPGetPriceAllRequest()
                                        {
                                            MoneyCollection = 0,
                                            ProductHeight = package_height,
                                            ProductLength = package_depth,
                                            ProductPrice = Convert.ToInt64(amount),
                                            ProductType = "HH",
                                            ProductWeight = package_weight,
                                            ProductWidth = package_width,
                                            SenderDistrict = detail_supplier.districtid == null ? 4 : (int)detail_supplier.districtid,
                                            SenderProvince = (int)detail_supplier.provinceid == null ? 1 : (int)detail_supplier.provinceid,
                                            ReceiverDistrict = Convert.ToInt32(order.districtid),
                                            ReceiverProvince = Convert.ToInt32(order.provinceid),
                                            Type = 1
                                        });
                                        if (response_item != null && response_item.Count > 0)
                                        {
                                            var match_service = response_item.Where(x => x.MaDvChinh.Trim().ToUpper() == order.delivery_detail.shipping_service_code.Trim().ToUpper());
                                            order.shipping_fee += (match_service == null || match_service.Count() <= 0) ? 0 : (match_service.Sum(x => x.GiaCuoc));
                                        }
                                        string package_name = string.Join(",", cart_belong_to_supplier.Select(x => x.product.name));
                                        VTPOrderRequestModel item = new VTPOrderRequestModel()
                                        {
                                            ORDER_NUMBER=order_summit.OrderNo,
                                            CHECK_UNIQUE=false,
                                            CUS_ID=0,
                                            DELIVERY_DATE=DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"),
                                            EXTRA_MONEY=0,
                                            GROUPADDRESS_ID=0,
                                            LIST_ITEM=new List<VTPOrderRequestListItem>(),
                                            MONEY_COLLECTION=0,
                                            ORDER_NOTE="Cho xem hàng, không cho thử",
                                            ORDER_PAYMENT =1,
                                            ORDER_SERVICE= order.delivery_detail.shipping_service_code,
                                            ORDER_SERVICE_ADD="",
                                            ORDER_VOUCHER="",
                                            PRODUCT_DESCRIPTION ="Trong đơn chứa tổng cộng "+ total_quanity + " sản phẩm",
                                             PRODUCT_HEIGHT= package_height,
                                             PRODUCT_LENGTH=package_width,
                                             PRODUCT_NAME = package_name,
                                             PRODUCT_PRICE= Convert.ToInt32(amount),
                                             PRODUCT_QUANTITY= total_quanity,
                                             PRODUCT_TYPE="HH",
                                             PRODUCT_WEIGHT=package_weight,
                                             PRODUCT_WIDTH=package_width,
                                             RECEIVER_ADDRESS= order.address,
                                             RECEIVER_DISTRICT=Convert.ToInt32(order.districtid),
                                             RECEIVER_EMAIL="",
                                             RECEIVER_FULLNAME=order.receivername,
                                             RECEIVER_LATITUDE=0,
                                             RECEIVER_LONGITUDE=0,
                                             RECEIVER_PHONE=order.phone,
                                             RECEIVER_PROVINCE= Convert.ToInt32(order.provinceid),
                                             RECEIVER_WARD = Convert.ToInt32(order.wardid),
                                             SENDER_ADDRESS=detail_supplier.address,
                                             SENDER_DISTRICT= detail_supplier.districtid==null?0:(int)detail_supplier.districtid,
                                             SENDER_EMAIL= "",
                                             SENDER_FULLNAME= detail_supplier.fullname,
                                             SENDER_LATITUDE=0,
                                             SENDER_LONGITUDE=0,
                                             SENDER_PHONE=detail_supplier.phone,
                                             SENDER_PROVINCE= detail_supplier.provinceid == null ? 0 : (int)detail_supplier.provinceid,
                                            SENDER_WARD = detail_supplier.wardid==null?0:(int)detail_supplier.wardid,
                                        };
                                        model.Add(item);
                                    }
                                }
                                order_summit.ShippingToken = JsonConvert.SerializeObject(model);
                            }
                            break;
                        default:
                            {

                            }
                            break;
                    }
                    order_summit.ShippingTypeCode = order.delivery_detail.shipping_service_code == null ? "" : order.delivery_detail.shipping_service_code;
                    order_summit.ShippingFee = order.shipping_fee;
                   
                }
                logging_service.InsertLogTelegramDirect("CreateOrder : CreateOrder");


                var order_id = await orderDAL.CreateOrder(order_summit);
                logging_service.InsertLogTelegramDirect("Order Created - " + order.order_no + " - " + total_amount);
                workQueueClient.SyncES(order_id, "SP_GetOrder", "hulotoys_sp_getorder", Convert.ToInt16(ProjectType.HULOTOYS));
                if (order_id > 0)
                {
                    order.order_id = order_id;
                    order.order_no = order_summit.OrderNo;
                    foreach (var detail in details)
                    {   
                        detail.OrderId = order_id;
                        await orderDetailDAL.CreateOrderDetail(detail);
                        Console.WriteLine("Created OrderDetail - " + detail.OrderId + ": " + detail.OrderDetailId);
                        logging_service.InsertLogTelegramDirect("OrderDetail Created - " + detail.OrderId + ": " + detail.OrderDetailId);
                        order.total_price = total_price;
                        order.total_profit=total_profit;
                        logging_service.InsertLogTelegramDirect("CreateOrder : CreateOrderDetail");

                    }
                    order.total_amount = (double)order_summit.Amount;
                    order.total_profit = (double)order_summit.Profit;
                    order.total_discount = (double)order_summit.Discount;

                    //await nhanhVnService.PostToNhanhVN(order_summit,order, client, address_client);
                    await orderDetailMongoDbModel.Update(order);

                }
                var extend_order = JsonConvert.DeserializeObject<OrderDetailMongoDbModelExtend>(JsonConvert.SerializeObject(order));
                if(extend_order!=null)
                {
                    extend_order.email = client.Email;
                }
                logging_service.InsertLogTelegramDirect("CreateOrder : extend_order");

                extend_order.created_date = time;
                try{
                    foreach (var detail in details)
                    {
                        await _productDetailMongoAccess.UpdateQuantityOfStock(detail.ProductId, (int)detail.Quantity);

                    }
                }
                catch
                {

                }
                return extend_order;
            }
            catch (Exception ex)
            {
                string err = "CreateOrder with ["+ order_detail_id+"] error: " + ex.ToString();
                Console.WriteLine(err);
                logging_service.InsertLogTelegramDirect(err);

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
        private async Task<TrackingVoucherResponse> ApplyVoucher(TrackingVoucherRequest input)
        {
            TrackingVoucherResponse result = new TrackingVoucherResponse();
            try
            {
                HttpClient _HttpClient = new HttpClient(new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (message, certificate2, arg3, arg4) => true
                })
                {
                    BaseAddress = new Uri(ConfigurationManager.AppSettings["API_Domain"])
                };
                string url = ConfigurationManager.AppSettings["API_Get_Voucher"];
                //HttpClient client = new HttpClient();

                //var token = CommonHelpers.Encode(JsonConvert.SerializeObject(input), ConfigurationManager.AppSettings["key_private"]);
                //var content_2 = new FormUrlEncodedContent(new[]
                //{
                //       new KeyValuePair<string, string>("token", token),
                //});
               // var content = new StringContent(JsonConvert.SerializeObject(input), Encoding.UTF8, "application/json");
                string token = CommonHelpers.Encode(JsonConvert.SerializeObject(input), ConfigurationManager.AppSettings["key_private"]);
                var request_message = new HttpRequestMessage(HttpMethod.Post, url);
                //request_message.Headers.Add("Authorization", "Bearer " + TOKEN);
                var content = new StringContent("{\"token\":\"" + token + "\"}", Encoding.UTF8, "application/json");
                request_message.Content = content;
                var response = await _HttpClient.SendAsync(request_message);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    result = JsonConvert.DeserializeObject<TrackingVoucherResponse>(response.Content.ReadAsStringAsync().Result);

                }
            }
            catch (Exception ex)
            {

            }
            return result;
        }
       
    }
}
