using APP_CHECKOUT;
using APP_CHECKOUT.Utilities.Lib;
using Newtonsoft.Json;
using System.Configuration;
using System.Text;

namespace HuloToys_Service.Controllers.Shipping.Business
{
    public class ViettelPostService
    {
        public readonly string DOMAIN = "https://partner.viettelpost.vn/v2/";

        private string token_temporary = "";
        private string token_ownerconnect = "";
        private readonly string USERNAME = "0962753455";
        private readonly string PASSWORD = "Lamtutam888";

        private readonly string API_LOGIN = "user/Login";
        private readonly string API_OWNERCONNECT = "user/ownerconnect";
        private readonly string API_GETPRICEALL = "order/getPriceAll";
        private readonly string API_GETPRICE = "order/getPrice";
        private readonly HttpClient _httpClient;
        private readonly RedisConn _redisService;
        private int exprire_time = 86400;
        public ViettelPostService(RedisConn redisService)
        {
            _httpClient = new HttpClient();

            _redisService = redisService;
            try
            {
                _redisService.Connect();
            }
            catch
            {

            }
            var result = GetTemporaryToken().Result;

        }
        public async Task<bool> GetTemporaryToken()
        {
            try
            {
                try
                {
                    var token = _redisService.Get("ViettelPostToken", Convert.ToInt32(ConfigurationManager.AppSettings["Redis_Database_db_common"]));
                    if (token != null && token.Trim() != "")
                    {
                        token_temporary = token.Trim();
                        return true;
                    }
                }
                catch {  }
            
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, DOMAIN + API_LOGIN);
                string json_content = "{\"USERNAME\":\"" + USERNAME + "\",\"PASSWORD\":\"" + PASSWORD + "\"}";
                StringContent content = new StringContent(json_content, null, "application/json");
                request.Content = content;
                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                if (responseBody == null || responseBody.Trim() == "")
                {
                    return false;

                }
                dynamic responseObject = JsonConvert.DeserializeObject(responseBody);

                // Kiểm tra xem token có tồn tại không
                if (responseObject != null && responseObject.data != null && responseObject.data.token != null)
                {
                    token_temporary = responseObject.data.token;
                    LogHelper.InsertLogTelegram("[APP.CHECKOUT] GetTemporaryToken - ViettelPostService: Token= [" + token_temporary + "] [" + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + "]");
                    try
                    {
                        _redisService.Set("ViettelPostToken", responseObject.data.token, DateTime.Now.AddSeconds(exprire_time), Convert.ToInt32(ConfigurationManager.AppSettings["Redis_Database_db_common"]));

                    }
                    catch { }
                    
                    return true;
                }
                else
                {
                    LogHelper.InsertLogTelegram("[APP.CHECKOUT] GetTemporaryToken - ViettelPostService: Token not found on API [" + (DOMAIN + API_LOGIN) + "] [" + json_content + "]");
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("[APP.CHECKOUT] GetTemporaryToken - ViettelPostService: error [" + (DOMAIN + API_LOGIN) + "] :" + ex);
            }
            return false;
        }
        private async Task<bool> GetOwnerConnectToken()
        {
            try
            {
                if (token_temporary == null || token_temporary.Trim() == "")
                {
                    await GetTemporaryToken();
                } 
                var request = new HttpRequestMessage(HttpMethod.Post, DOMAIN + API_OWNERCONNECT);
                request.Headers.Add("Token", token_temporary);
                request.Headers.Add("Cookie", "SERVERID=A");
                string json_content = "{\"USERNAME\":\"" + USERNAME + "\",\"PASSWORD\":\"" + PASSWORD + "\"}";
                StringContent content = new StringContent(json_content, null, "application/json");
                request.Content = content;
                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                if (responseBody == null || responseBody.Trim() == "")
                {
                    return false;

                }
                dynamic responseObject = JsonConvert.DeserializeObject(responseBody);

                if (responseObject != null && responseObject.data != null && responseObject.data.token != null)
                {
                    token_ownerconnect = responseObject.data.token;
                    return true;
                }
                else
                {
                    LogHelper.InsertLogTelegram("[APP.CHECKOUT] GetOwnerConnectToken - ViettelPostService: Token not found on API [" + (DOMAIN + API_OWNERCONNECT) + "] [" + json_content + "]");
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("[APP.CHECKOUT] GetOwnerConnectToken - ViettelPostService: error [" + (DOMAIN + API_OWNERCONNECT) + "] :" + ex);
            }
            return false;
        }
        public async Task<List<VTPGetPriceAllResponse>> GetShippingMethods(VTPGetPriceAllRequest requestData)
        {
            try
            {
                if (token_temporary==null || token_temporary.Trim()=="")
                {
                    await GetTemporaryToken();
                }
                var request = new HttpRequestMessage(HttpMethod.Post, DOMAIN + API_GETPRICEALL);
                request.Headers.Add("Token", token_temporary);
                request.Headers.Add("Cookie", "SERVERID=A");
                string jsonContent = JsonConvert.SerializeObject(requestData);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                request.Content = content;
                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();
                List<VTPGetPriceAllResponse> shippingServices = JsonConvert.DeserializeObject<List<VTPGetPriceAllResponse>>(responseBody);

                return shippingServices;
            }
            catch (HttpRequestException e)
            {
                LogHelper.InsertLogTelegram("[APP.CHECKOUT] GetShippingMethods - ViettelPostService : HttpCall error [" + (DOMAIN + API_GETPRICEALL) + "] [" + e.StatusCode + "][" + e.Message + "]");

                return null;
            }
            catch (JsonException e)
            {
                LogHelper.InsertLogTelegram("[APP.CHECKOUT] GetShippingMethods - ViettelPostService: JSON parse error [" + (DOMAIN + API_GETPRICEALL) + "] [" + e.Message + "]");
                return null;
            }
            catch (Exception e)
            {
                LogHelper.InsertLogTelegram("[APP.CHECKOUT] GetShippingMethods - ViettelPostService: error [" + (DOMAIN + API_GETPRICEALL) + "] [" + e.Message + "]");
                return null; 
            }
        }
        public async Task<VTPGetPriceResponse> CalculateShippingPrice( VTPGetPriceRequest requestData)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, DOMAIN + API_GETPRICE);
                request.Headers.Add("Token", token_temporary);
                request.Headers.Add("Cookie", "SERVERID=A");
                string jsonContent = JsonConvert.SerializeObject(requestData);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                request.Content = content;

                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();

                VTPGetPriceResponse shippingServices = JsonConvert.DeserializeObject<VTPGetPriceResponse>(responseBody);

                return shippingServices;
            }
            catch (HttpRequestException e)
            {
                LogHelper.InsertLogTelegram("[APP.CHECKOUT] CalculateShippingPrice - ViettelPostService: HttpCall error [" + (DOMAIN + API_GETPRICEALL) + "] [" + e.StatusCode + "][" + e.Message + "]");

                return null;
            }
            catch (JsonException e)
            {
                LogHelper.InsertLogTelegram("[APP.CHECKOUT] CalculateShippingPrice - ViettelPostService: JSON parse error [" + (DOMAIN + API_GETPRICEALL) + "] [" + e.Message + "]");
                return null;
            }
            catch (Exception e)
            {
                LogHelper.InsertLogTelegram("[APP.CHECKOUT] CalculateShippingPrice - ViettelPostService: error [" + (DOMAIN + API_GETPRICEALL) + "] [" + e.Message + "]");
                return null;
            }
        }
    }

    // --- INPUT MODEL ---
    public class VTPGetPriceAllRequest
    {
        [JsonProperty("SENDER_DISTRICT")]
        public int SenderDistrict { get; set; }

        [JsonProperty("SENDER_PROVINCE")]
        public int SenderProvince { get; set; }

        [JsonProperty("RECEIVER_DISTRICT")]
        public int ReceiverDistrict { get; set; }

        [JsonProperty("RECEIVER_PROVINCE")]
        public int ReceiverProvince { get; set; }

        [JsonProperty("PRODUCT_TYPE")]
        public string ProductType { get; set; }

        [JsonProperty("PRODUCT_WEIGHT")]
        public int ProductWeight { get; set; }

        [JsonProperty("PRODUCT_PRICE")]
        public long ProductPrice { get; set; }

        [JsonProperty("MONEY_COLLECTION")]
        public long MoneyCollection { get; set; }

        [JsonProperty("PRODUCT_LENGTH")]
        public int ProductLength { get; set; }

        [JsonProperty("PRODUCT_WIDTH")]
        public int ProductWidth { get; set; }

        [JsonProperty("PRODUCT_HEIGHT")]
        public int ProductHeight { get; set; }

        [JsonProperty("TYPE")]
        public int Type { get; set; }
    }

    // --- OUTPUT MODELS ---
    public class VTPExtraService
    {
        [JsonProperty("SERVICE_CODE")]
        public string ServiceCode { get; set; }

        [JsonProperty("SERVICE_NAME")]
        public string ServiceName { get; set; }

        [JsonProperty("DESCRIPTION")]
        public object Description { get; set; } // Có thể là null, nên dùng object
    }

    public class VTPGetPriceAllResponse
    {
        [JsonProperty("MA_DV_CHINH")]
        public string MaDvChinh { get; set; }

        [JsonProperty("TEN_DICHVU")]
        public string TenDichVu { get; set; }

        [JsonProperty("GIA_CUOC")]
        public int GiaCuoc { get; set; }

        [JsonProperty("THOI_GIAN")]
        public string ThoiGian { get; set; }

        [JsonProperty("EXCHANGE_WEIGHT")]
        public int ExchangeWeight { get; set; }

        [JsonProperty("EXTRA_SERVICE")]
        public List<VTPExtraService> ExtraService { get; set; }
    }
    public class VTPGetPriceRequest
    {
        [JsonProperty("PRODUCT_WEIGHT")]
        public int ProductWeight { get; set; }

        [JsonProperty("PRODUCT_PRICE")]
        public long ProductPrice { get; set; }

        [JsonProperty("MONEY_COLLECTION")]
        public long MoneyCollection { get; set; }

        [JsonProperty("ORDER_SERVICE_ADD")]
        public string OrderServiceAdd { get; set; } // Có thể là chuỗi rỗng

        [JsonProperty("ORDER_SERVICE")]
        public string OrderService { get; set; } // Ví dụ: "VCBO"

        [JsonProperty("SENDER_DISTRICT")]
        public int SenderDistrict { get; set; }

        [JsonProperty("SENDER_PROVINCE")]
        public int SenderProvince { get; set; }

        [JsonProperty("RECEIVER_DISTRICT")]
        public int ReceiverDistrict { get; set; }

        [JsonProperty("RECEIVER_PROVINCE")]
        public int ReceiverProvince { get; set; }

        [JsonProperty("PRODUCT_LENGTH")]
        public int ProductLength { get; set; }

        [JsonProperty("PRODUCT_WIDTH")]
        public int ProductWidth { get; set; }

        [JsonProperty("PRODUCT_HEIGHT")]
        public int ProductHeight { get; set; }

        [JsonProperty("PRODUCT_TYPE")]
        public string ProductType { get; set; }

        [JsonProperty("NATIONAL_TYPE")]
        public int NationalType { get; set; }
    }

    // --- OUTPUT MODELS for getPrice API ---
    public class VTPGetPriceData
    {
        [JsonProperty("MONEY_TOTAL_OLD")]
        public int MoneyTotalOld { get; set; }

        [JsonProperty("MONEY_TOTAL")]
        public int MoneyTotal { get; set; }

        [JsonProperty("MONEY_TOTAL_FEE")]
        public int MoneyTotalFee { get; set; }

        [JsonProperty("MONEY_FEE")]
        public int MoneyFee { get; set; }

        [JsonProperty("MONEY_COLLECTION_FEE")]
        public int MoneyCollectionFee { get; set; }

        [JsonProperty("MONEY_OTHER_FEE")]
        public int MoneyOtherFee { get; set; }

        [JsonProperty("MONEY_VAS")]
        public int MoneyVas { get; set; }

        [JsonProperty("MONEY_VAT")]
        public int MoneyVat { get; set; }

        [JsonProperty("KPI_HT")]
        public double KpiHt { get; set; }
    }

    public class VTPGetPriceResponse
    {
        [JsonProperty("status")]
        public int Status { get; set; }

        [JsonProperty("error")]
        public bool Error { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("data")]
        public VTPGetPriceData Data { get; set; }
    }
}
