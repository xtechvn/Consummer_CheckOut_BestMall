using APP.READ_MESSAGES.Libraries;
using APP_CHECKOUT.Elasticsearch;
using APP_CHECKOUT.Helpers;
using APP_CHECKOUT.Models.Client;
using APP_CHECKOUT.Models.NhanhVN;
using APP_CHECKOUT.Models.Orders;
using APP_CHECKOUT.Utilities.Lib;
using Caching.Elasticsearch;
using Entities.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System.Configuration;

namespace APP_CHECKOUT.Repositories
{
    public class NhanhVnService
    {
        private readonly LocationESService locationESService;

        public NhanhVnService()
        {

            locationESService = new LocationESService(ConfigurationManager.AppSettings["Elastic_Host"]);

        }
        public async Task PostToNhanhVN(Entities.Models.Order order_summit, OrderDetailMongoDbModel order, ClientESModel client, AddressClientESModel address_client)
        {
            try
            {

                var options = new RestClientOptions(ConfigurationManager.AppSettings["NhanhVN_API:Domain"]);
                var http_client = new RestClient(options);
                var request = new RestRequest(ConfigurationManager.AppSettings["NhanhVN_API:Domain"] + ConfigurationManager.AppSettings["NhanhVN_API:AddOrder"], Method.Post);
                request.AlwaysMultipartFormData = true;
                request.AddParameter("version", "2.0");
                request.AddParameter("appId", ConfigurationManager.AppSettings["NhanhVN_AppId"]);
                request.AddParameter("businessId", ConfigurationManager.AppSettings["NhanhVN_BussinessID"]);
                request.AddParameter("accessToken", ConfigurationManager.AppSettings["NhanhVN_AccessToken"]);
                var city = await GetLocationByType(0);
                string city_name = "";
                string district_name = "";
                string ward_name = "";
                var province = locationESService.GetProvincesByProvinceId(order.provinceid);
                var district = locationESService.GetDistrictByDistrictId(order.districtid);
                var ward = locationESService.GetWardsByWardId(order.wardid);
                var city_map = city.FirstOrDefault(x => CommonHelpers.RemoveUnicode(x.name.Trim().ToLower()).Contains(CommonHelpers.RemoveUnicode(province.Name.Trim().ToLower())));
                if(city_map!=null && city_map.id > 0)
                {
                    city_name=city_map.name;
                    var districts=await GetLocationByType(1, city_map.id);
                    if (districts != null && districts.Count > 0)
                    {
                        var district_map = districts.FirstOrDefault(x => CommonHelpers.RemoveUnicode(x.name.Trim().ToLower()).Contains(CommonHelpers.RemoveUnicode(district.Name.Trim().ToLower())));
                        if(district_map != null && district_map.name != null)
                        {
                            district_name = district_map.name;
                            var wards = await GetLocationByType(2, district_map.id);
                            if (wards != null && wards.Count > 0)
                            {
                                var ward_map = districts.FirstOrDefault(x => CommonHelpers.RemoveUnicode(x.name.Trim().ToLower()).Contains(CommonHelpers.RemoveUnicode(district.Name.Trim().ToLower())));
                                ward_name = ward_map == null || ward_map.name == null ? "" : ward_map.name;
                            }
                        }
                      
                    }
                }
                NhanhVNOrderAddRequestModel model = new NhanhVNOrderAddRequestModel()
                {
                    id = order_summit.OrderId.ToString(),
                    depotId = null,
                    type = "Shipping",
                    customerName = address_client==null?order.receivername: address_client.ReceiverName,
                    customerMobile = address_client == null ? order.phone : address_client.ReceiverName,
                    customerEmail = client.Email,
                    customerAddress = order_summit.Address,
                    customerCityName = city_name,
                    customerDistrictName= district_name,
                    customerWardLocationName=ward_name,
                    moneyDiscount=order_summit.Discount,
                    moneyTransfer= null,
                    moneyTransferAccountId=null,
                    moneyDeposit=null,
                    moneyDepositAccountId = null,
                    paymentMethod= "COD",
                    paymentCode = null,
                    paymentGateway=null,
                    carrierId=null,
                    carrierServiceId=null,
                    customerShipFee=Convert.ToInt32(order_summit.ShippingFee),
                    deliveryDate=DateTime.Now.AddDays(2).ToString("yyyy-MM-dd"),
                    status="New",
                    description = null,
                    privateDescription=null,
                    trafficSource= null,
                    couponCode=order.voucher_id.ToString(),
                    allowTest=1,
                    saleId=null,
                    autoSend=0,
                    //sendCarrierType=2,
                    //carrierAccountId=null,
                    //carrierShopId=null,
                    //carrierServiceCode= null,
                    utmCampaign= null,
                    utmMedium= null,
                    utmSource= null,
                    isPartDelivery=null,
                    usedPoint=null,
                    affiliate = null,
                    productList=new List<NhanhVNOrderProductRequestModel>()
                };
                foreach(var c in order.carts)
                {
                  
                    model.productList.Add(new NhanhVNOrderProductRequestModel()
                    {
                        id=c.product._id,
                        code=c.product.sku,
                        description="",
                        idNhanh=null,
                        imei=null,
                        name=c.product.name,
                        importPrice=Convert.ToInt32(c.product.price),
                        price= Convert.ToInt32(c.product.amount),
                        quantity=c.quanity,
                        weight = Convert.ToInt32(c.product.weight==null?0: (float)c.product.weight),
                        gifts=null,
                        type="Product"
                    });
                }

                request.AddParameter("data", JsonConvert.SerializeObject(model));

                RestResponse response = await http_client.ExecuteAsync(request);
                var jsonData = JObject.Parse(response.Content);
                var status = int.Parse(jsonData["code"].ToString());
                if (status == 1)
                {
                    Console.WriteLine(response.Content);
                    LogHelper.InsertLogTelegram("[" + order.order_no + "] -> Nhanh VN OrderCreated: " + jsonData["data"]["orderId"].ToString());


                }
                else
                {
                    string err = "PostToNhanhVN with [" + order._id + "] error: " + response.Content;
                    Console.WriteLine(err);
                    LogHelper.InsertLogTelegram(err);
                }


            }
            catch(Exception ex) {
            

            }


        }
        public async Task<List<NhanhVNLocationResponseLocation>> GetLocationByType(int type, int parent_id=0)
        {
            try
            {
                string type_string = "CITY";
                switch (type){
                    case 0:
                        {

                        }break;
                    case 1:
                        {
                            type_string = "DISTRICT";
                        }
                        break;
                    case 2:
                        {
                            type_string = "WARD";

                        }
                        break;
                    default:
                        {
                            return new List<NhanhVNLocationResponseLocation>();
                        }

                }
                var options = new RestClientOptions(ConfigurationManager.AppSettings["NhanhVN_API:Domain"]);
                var http_client = new RestClient(options);
                var request = new RestRequest(ConfigurationManager.AppSettings["NhanhVN_API:Domain"]+ ConfigurationManager.AppSettings["NhanhVN_API:Location"], Method.Post);
                request.AlwaysMultipartFormData = true;
                request.AddParameter("version", "2.0");
                request.AddParameter("appId", ConfigurationManager.AppSettings["NhanhVN_AppId"]);
                request.AddParameter("businessId", ConfigurationManager.AppSettings["NhanhVN_BussinessID"]);
                request.AddParameter("accessToken", ConfigurationManager.AppSettings["NhanhVN_AccessToken"]);
                request.AddParameter("data", "{\"type\":\""+ type_string + "\",\"parentId\":"+ parent_id + "}");
              
                RestResponse response = await http_client.ExecuteAsync(request);
                var jsonData = JObject.Parse(response.Content);
                var status = int.Parse(jsonData["code"].ToString());

                if (status == 1)
                {
                    var data= JsonConvert.DeserializeObject<List<NhanhVNLocationResponseLocation>>(jsonData["data"].ToString());
                    return data;
                }

            }
            catch (Exception ex)
            {
                string err = "PostToNhanhVN ->GetLocationByType with [" + type+"-"+parent_id + "] error: " + ex.ToString();
                Console.WriteLine(err);
                LogHelper.InsertLogTelegram(err);
            }
            return new List<NhanhVNLocationResponseLocation>();

        }
    }
}
