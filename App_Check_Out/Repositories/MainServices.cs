using APP.READ_MESSAGES.Libraries;
using APP_CHECKOUT.DAL;
using APP_CHECKOUT.Helpers;
using APP_CHECKOUT.Interfaces;
using APP_CHECKOUT.MongoDb;
using Entities.Models;
using APP_CHECKOUT.Models.Models.Queue;
using APP_CHECKOUT.Utilities.constants;
using APP_CHECKOUT.Models.Location;
using Utilities.Contants;
using DAL;
using APP_CHECKOUT.RabitMQ;
using System.Configuration;
using Caching.Elasticsearch;
using Newtonsoft.Json;
using APP_CHECKOUT.Models.Orders;
using System.Net;
using ADAVIGO_FRONTEND.Models.Flights.TrackingVoucher;
using APP_CHECKOUT.Constants;
using System.Text;
using Nest;
using System.Net.Http;
using Caching.Elasticsearch.FlashSale;
using HuloToys_Service.Controllers.Product.Bussiness;

namespace APP_CHECKOUT.Repositories
{
    public class MainServices: IMainServices
    {
       // private readonly ILoggingService logging_service;
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

        public MainServices( /*ILoggingService loggingService*/) {

            //logging_service=loggingService;
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
            //nhanhVnService = new NhanhVnService(logging_service);
            workQueueClient = new WorkQueueClient();
            emailService = new EmailService(clientESService, accountClientESService, locationDAL);
            productDetailService=new ProductDetailService(clientESService,flashSaleESRepository,flashSaleProductESRepository,productDetailMongoAccess);
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
                                  Console.WriteLine("[APP CHECKOUT] CREATE_ORDER: Success " + data._id); 
                            }else{
                                Console.WriteLine("[APP CHECKOUT] CREATE_ORDER: That bai"); 
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
                //logging_service.InsertLogTelegramDirect(err);
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
                    Console.WriteLine("CreateOrder Get orderDetailMongoDbModel.FindById - NULL [" + order_detail_id+"]");
                    return null;
                }
                Order order_summit = new Order();
                List<OrderDetail> details = new List<OrderDetail>();
                //double total_price = 0;
                //double total_profit = 0;
                //double total_amount = 0;
                float total_weight = 0;
                foreach (var cart in order.carts)
                {
                    if (cart == null || cart.product == null) continue; 
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
                    //total_price += (cart.product.price * cart.quanity);
                    //total_profit += (cart.product.profit * cart.quanity);
                    //total_amount += (amount_product * cart.quanity);

                    ////cart.total_price = cart.product.price * cart.quanity;
                    ////cart.total_discount = cart.product.discount * cart.quanity;
                    ////cart.total_profit = cart.product.profit * cart.quanity;
                    ////cart.total_amount = amount_product * cart.quanity;

                    //cart.total_price = cart.product.price * cart.quanity;
                    //cart.total_profit = cart.product.profit * cart.quanity;
                    //cart.total_amount = amount_product * cart.quanity;
                    //cart.total_discount = cart.product.discount * cart.quanity;
                    total_weight += ((cart.product.weight == null ? 0 : (float)cart.product.weight) * cart.quanity / 1000);

                }
                var account_client = accountClientESService.GetById(order.account_client_id);
                //logging_service.InsertLogTelegramDirect(" accountClientESService.GetById("+ order.account_client_id + ") : "+ (account_client == null ? "NULL" : JsonConvert.SerializeObject(account_client)));

                var client = clientESService.GetById((long)account_client.ClientId);
               // logging_service.InsertLogTelegramDirect(" clientESService.GetById(" + (long)account_client.ClientId + ") : " + (client == null ? "NULL" : JsonConvert.SerializeObject(client)));

                AddressClientESModel address_client = addressClientESService.GetById(order.address_id, client.Id);
               // logging_service.InsertLogTelegramDirect(" addressClientESService.GetById(" + order.address_id + "," + client.Id + ") : " + (address_client == null ? "NULL" : JsonConvert.SerializeObject(address_client)));

                order_summit = new Order()
                {
                    Amount =order.total_amount,
                    ClientId = (long)account_client.ClientId,
                    CreatedDate = DateTime.Now,
                    Discount = 0,
                    IsDelete = 0,
                    Note = "",
                    OrderId = 0,
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
                List<Province> provinces = GetProvince();
                List<District> districts = GetDistrict();
                List<Ward> wards = GetWards();
                if (address_client != null && address_client.ProvinceId != null && address_client.DistrictId != null && address_client.WardId != null)
                {
                    if (address_client.ProvinceId.Trim() != "" && provinces != null && provinces.Count > 0)
                    {
                        var province = provinces.FirstOrDefault(x => x.ProvinceId == address_client.ProvinceId);
                        order_summit.ProvinceId = province != null ? province.Id : null;
                    }
                    if (address_client.DistrictId.Trim() != "" && districts != null && districts.Count > 0)
                    {
                        var district = districts.FirstOrDefault(x => x.DistrictId == address_client.DistrictId);
                        order_summit.DistrictId = district != null ? district.Id : null;
                    }
                    if (address_client.WardId.Trim() != "" && wards != null && wards.Count > 0)
                    {
                        var ward = wards.FirstOrDefault(x => x.WardId == address_client.WardId);
                        order_summit.WardId = ward != null ? ward.Id : null;
                    }
                    order_summit.ReceiverName = address_client.ReceiverName;
                    order_summit.Phone = address_client.Phone;
                    order_summit.Address = address_client.Address;
                }
                else
                {
                    var province = provinces.FirstOrDefault(x => x.ProvinceId == order.provinceid);
                    order_summit.ProvinceId = province != null ? province.Id : null;
                    var district = districts.FirstOrDefault(x => x.DistrictId == order.districtid);
                    order_summit.DistrictId = district != null ? district.Id : null;
                    var ward = wards.FirstOrDefault(x => x.WardId == order.wardid);
                    order_summit.WardId = ward != null ? ward.Id : null;
                    order_summit.ReceiverName = order.receivername;
                    order_summit.Phone = order.phone;
                    order_summit.Address = order.address;
                }
                //--apply voucher:
                #region Old version:
                //double total_discount = 0;
                //if (order.voucher_code != null && order.voucher_code.Trim() != "")
                //{
                //    var input = new TrackingVoucherRequest
                //    {
                //        total_order_amount_before = (double)order_summit.Amount,
                //        user_id = Convert.ToInt64(order.account_client_id),
                //        voucher_name = order.voucher_code,
                //        token = CommonHelpers.Encode("{\"user_name\":\"" + account_client.UserName + "\"}", ConfigurationManager.AppSettings["key_private"])
                //    };
                //    var voucher_apply = await ApplyVoucher(input);
                //    if (voucher_apply != null && voucher_apply.status == 0)
                //    {
                //        //    switch (voucher_apply.rule_type)
                //        //    {
                //        //        case (int)VoucherRuleType.ALL_PRODUCT:
                //        //            {

                //        //            }break;
                //        //        case (int)VoucherRuleType.SPECIFIC_PRODUCT:
                //        //            {

                //        //            }
                //        //            break;
                //        //        default:
                //        //            {
                //        //                double percent = Convert.ToDouble(voucher_apply.value);
                //        //                switch (voucher_apply.type)
                //        //                {
                //        //                    case "percent":
                //        //                        //Tinh số tiền giảm theo %
                //        //                        total_discount += ((double)order_summit.Amount * Convert.ToDouble(percent / 100));
                //        //                        break;
                //        //                    case "vnd":
                //        //                        total_discount += percent; //Math.Min(Convert.ToDouble(voucher.LimitTotalDiscount), total_fee_not_luxury) ;
                //        //                        break;

                //        //                    default: break;

                //        //                }
                //        //                voucher_apply.discount = total_discount;
                //        //                voucher_apply.total_order_amount_after = voucher_apply.total_order_amount_before - total_discount;
                //        //                order_summit.VoucherId = voucher_apply.voucher_id;
                //        //                order_summit.Discount = voucher_apply.discount;
                //        //                order_summit.Amount = voucher_apply.total_order_amount_after;
                //        //                order_summit.Profit -= order_summit.Discount;
                //        //            }break;

                //        //    }

                //        //}
                //        double percent = Convert.ToDouble(voucher_apply.value);
                //        switch (voucher_apply.type)
                //        {
                //            case "percent":
                //                total_discount += ((double)order_summit.Amount * Convert.ToDouble(percent / 100));
                //                break;
                //            case "vnd":
                //                total_discount += percent;
                //                break;

                //            default: break;

                //        }
                //        voucher_apply.discount = total_discount;
                //        voucher_apply.total_order_amount_after = voucher_apply.total_order_amount_before - total_discount;
                //        order_summit.VoucherId = voucher_apply.voucher_id;
                //        order_summit.Discount = voucher_apply.discount;
                //        order_summit.Amount = voucher_apply.total_order_amount_after;
                //        order_summit.Profit -= order_summit.Discount;
                //    }

                //}
                //order.total_discount = total_discount;
                #endregion


                var order_id = await orderDAL.CreateOrder(order_summit);
                Console.WriteLine("CreateOrder orderDAL.CreateOrder : [" + order_id + "]");
               // logging_service.InsertLogTelegramDirect("Order Created - " + order.order_no + " - " + total_amount);
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
                       // logging_service.InsertLogTelegramDirect("OrderDetail Created - " + detail.OrderId + ": " + detail.OrderDetailId);
                        //order.total_price = total_price;
                        //order.total_profit=total_profit;
                    }
                    //order.total_amount = (double)order_summit.Amount;
                    //order.total_profit = (double)order_summit.Profit;
                    //order.total_discount = (double)order_summit.Discount;

                    ////await nhanhVnService.PostToNhanhVN(order_summit,order, client, address_client);
                    //await orderDetailMongoDbModel.Update(order);

                }
                var extend_order = JsonConvert.DeserializeObject<OrderDetailMongoDbModelExtend>(JsonConvert.SerializeObject(order));
                if(extend_order!=null)
                {
                    extend_order.email = client.Email;
                }
                extend_order.created_date = time;
                return extend_order;
            }
            catch (Exception ex)
            {
                string err = "CreateOrder with ["+ order_detail_id+"] error: " + ex.ToString();
                Console.WriteLine(err);
                //logging_service.InsertLogTelegramDirect(err);

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
