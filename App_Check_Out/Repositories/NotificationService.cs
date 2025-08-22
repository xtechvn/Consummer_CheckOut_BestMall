using APP_CHECKOUT.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APP_CHECKOUT.Repositories
{
    public class NotificationService
    {
        public async Task<int> SendMessage(string user_id_send, string user_receiver_id, string action_type, string Code, string link_redirect,string user_name="")
        {
            try
            {

                HttpClient httpClient = new HttpClient();
                var j_param = new Dictionary<string, object>
                {
                   {"user_name_send", user_name}, //tên người gửi
                    {"user_id_send", user_id_send}, //id người gửi
                    {"code", Code}, // mã đơn Hàng
                    {"link_redirect", link_redirect}, // Link mà khi người dùng click vào detail item notify sẽ chuyển sang đó
                    //{"module_type", module_type}, // loại module thực thi luồng notify. Ví dụ: Đơn hàng, khách hàng.......
                    {"action_type", action_type} // action thực hiện. Ví dụ: Duyệt, tạo mới, từ chối....
                   // {"role_type", role_type}, // quyền mà sẽ gửi tới
                   /// {"service_code", service_code}// mã dịch vụ
                    ,{"user_receiver_id", user_receiver_id} // id người nhận notify
                };
                var data_product = JsonConvert.SerializeObject(j_param);

                var token = Encode(data_product, "AAAAB3NzaC1yc2EAAAADAQABAAABAQC+6zVy2tuIFTDWo97E52chdG1QgzTnqEx8tItL+m5x39BzrWMv5RbZZJbB0qU3SMeUgyynrgBdqSsjGk6euV3+97F0dYT62cDP2oBCIKsETmpY3UUs2iNNxDVvpKzPDE4VV4oZXwwr1kxurCiy+8YC2Z0oYdNDlJxd7+80h87ecdYS3olv5huzIDaqxWeEyCvGDCopiMhr+eh8ikwUdTOEYmgQwQcWPCeYcDDZD8afgBMnB6ys2i51BbLAap16R/B83fB78y0N04qXs3rg4tWGhcVhVyWL1q5PmmweesledOWOVFowfO6QIwDSvBwz0n3TstjXWF4JPbdcAQ8VszUj");
                var request = new FormUrlEncodedContent(new[]
                    {
                    new KeyValuePair<string, string>("token",token)
                });
                var url = "http://api.best-mall.vn" + "/api/notify/message/send.json";
                var response = await httpClient.PostAsync(url, request);
                if (response.IsSuccessStatusCode)
                {
                    return 0;
                }

                return 1;
            }
            catch (Exception ex)
            {
                return 1;
            }
        }
        public static string Encode(string strString, string strKeyPhrase)
        {
            try
            {
                strString = KeyED(strString, strKeyPhrase);
                Byte[] byt = System.Text.Encoding.UTF8.GetBytes(strString);
                strString = Convert.ToBase64String(byt);
                return strString;
            }
            catch (Exception ex)
            {
                return string.Empty;
            }

        }
        private static string KeyED(string strString, string strKeyphrase)
        {
            int strStringLength = strString.Length;
            int strKeyPhraseLength = strKeyphrase.Length;

            System.Text.StringBuilder builder = new System.Text.StringBuilder(strString);

            for (int i = 0; i < strStringLength; i++)
            {
                int pos = i % strKeyPhraseLength;
                int xorCurrPos = (int)(strString[i]) ^ (int)(strKeyphrase[pos]);
                builder[i] = Convert.ToChar(xorCurrPos);
            }

            return builder.ToString();
        }

    }
}
