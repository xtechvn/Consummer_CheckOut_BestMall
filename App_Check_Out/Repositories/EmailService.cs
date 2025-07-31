using APP_CHECKOUT.Models.Location;
using APP_CHECKOUT.Models.Orders;
using APP_CHECKOUT.Utilities.Lib;
using Caching.Elasticsearch;
using DAL;
using HuloToys_Service.Utilities.lib;
using System.Configuration;
using System.Net;
using System.Net.Mail;

namespace APP_CHECKOUT.Repositories
{
    public class EmailService
    {
        private readonly string _host;
        private readonly int _port;
        private readonly string _username;
        private readonly string _password;
        private readonly string _cc;
        private readonly string _bcc;
        private readonly string _domain;
        private const string EmailTemplatePath = "\\EmailTemplates\\OrderConfirmationEmail.html"; // Đường dẫn tới file template
        private readonly ClientESService clientESService;
        private readonly AccountClientESService accountClientESService;
        private readonly LocationDAL locationDAL;
        private readonly string static_url = "https://static-image.adavigo.com";
        public EmailService(ClientESService _clientESService, AccountClientESService _accountClientESService, LocationDAL _locationDAL)
        {
            _host = ConfigurationManager.AppSettings["Email_HOST"];
            _port = int.Parse(ConfigurationManager.AppSettings["Email_PORT"]);
            _username = ConfigurationManager.AppSettings["Email_UserName"];
            _password = ConfigurationManager.AppSettings["Email_Password"];
            _cc = ConfigurationManager.AppSettings["Email_CC"];
            _bcc = ConfigurationManager.AppSettings["Email_BCC"];
            _domain = ConfigurationManager.AppSettings["Email_Domain"];
            clientESService = _clientESService;
            accountClientESService = _accountClientESService;
            locationDAL = _locationDAL;

        }

        public bool SendOrderConfirmationEmail(string recipientEmail, OrderDetailMongoDbModelExtend order)
        {
            try
            {
                using (SmtpClient client = new SmtpClient(_host, _port))
                {
                    client.EnableSsl = true;
                    client.UseDefaultCredentials = false;
                    client.Credentials = new NetworkCredential(_username, _password);
                    client.DeliveryMethod = SmtpDeliveryMethod.Network;

                    using (MailMessage mail = new MailMessage())
                    {
                        mail.From = new MailAddress(_username, "BestMall CSKH"); // Tên hiển thị là BestMall
                        mail.To.Add(recipientEmail);
                        mail.Subject = $"Xác nhận đơn hàng của bạn tại BestMall - #{order.order_no}";
                        mail.IsBodyHtml = true;

                        if (!string.IsNullOrEmpty(_cc))
                        {
                            mail.CC.Add(_cc);
                        }
                        if (!string.IsNullOrEmpty(_bcc))
                        {
                            mail.Bcc.Add(_bcc);
                        }

                        mail.Body = ReadEmailTemplateAndPopulate(order);

                        client.Send(mail);
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending email: {ex.Message}");
                LogHelper.InsertLogTelegram("[APP.CHECKOUT] EmailService - SendOrderConfirmationEmail:" + ex.Message);
                return false;
            }
        }
        private string ReadEmailTemplateAndPopulate(OrderDetailMongoDbModelExtend order)
        {
            try
            {
                string templatePath = Environment.CurrentDirectory + EmailTemplatePath;
                string htmlContent = "";
                try
                {
                    htmlContent = File.ReadAllText(templatePath);
                }
                catch
                {
                    LogHelper.InsertLogTelegram("[APP.CHECKOUT] EmailService - SendOrderConfirmationEmail: Không tìm thấy file template email tại:" + EmailTemplatePath);
                }
                if (htmlContent == null || htmlContent.Trim()=="") {
                    htmlContent = GetTemplateInFunction();
                
                }
                var account_client = accountClientESService.GetById(order.account_client_id);
                var client = clientESService.GetById((long)account_client.ClientId);

                htmlContent = htmlContent.Replace("{clientname}", client.ClientName);
                htmlContent = htmlContent.Replace("{clientcode}", client.ClientCode);
                htmlContent = htmlContent.Replace("{orderno}", order.order_no);
                htmlContent = htmlContent.Replace("{created_date}", StringHelper.GetCurrentTimeInUtcPlus7(order.created_date).ToString("dd/MM/yyyy HH:mm:ss"));
                htmlContent = htmlContent.Replace("{receiver_name}", order.receivername);
                htmlContent = htmlContent.Replace("{receiver_name}", order.receivername);
                List<Province> provinces = GetProvince();
                List<District> districts = GetDistrict();
                List<Ward> wards = GetWards();
                string full_address = "{address}, {wardid}, {district}, {province}";
                if (order != null && order.provinceid != null && order.districtid != null && order.wardid != null)
                {
                    var province = provinces.FirstOrDefault(x => x.ProvinceId == order.provinceid);
                    var district = districts.FirstOrDefault(x => x.DistrictId == order.districtid);
                    var ward = wards.FirstOrDefault(x => x.WardId == order.wardid);
                    full_address = order.address + ", " + (ward == null ? "" : ward.Name) + ", " + (district == null ? "" : district.Name) + ", " + (province == null ? "" : province.Name);
                }
                else
                {
                    full_address = order.address;
                }
                htmlContent = htmlContent.Replace("{address}", full_address);
                htmlContent = htmlContent.Replace("{phone}", order.phone);
                htmlContent = htmlContent.Replace("{amount}", (order.total_amount+(order.total_discount==null?0:(double)order.total_discount)).ToString("N0"));
                htmlContent = htmlContent.Replace("{total_amount}", order.total_amount.ToString("N0"));
                string template = @"
                                            <tr>
                                                <!-- Product Image -->
                                                <td width=""50"" valign=""top"">
                                                    <img src=""{image}"" width=""50"" height=""50""
                                                         alt=""Product"" style=""border-radius:4px;"">
                                                </td>

                                                <!-- Product Info -->
                                                <td valign=""top"" style=""padding-left:10px;"">
                                                    <div style=""font-size:14px; font-weight:bold; color:#002b5b;"">
                                                        {name}
                                                    </div>
                                                    <div style=""font-size:13px; color:#555;"">Mã sản phẩm: {code}</div>
                                                    <div style=""font-size:13px; color:#555;"">Số lượng: {quanity}</div>
                                                </td>

                                                <!-- Price -->
                                                <td align=""right"" valign=""top""
                                                    style=""font-size:14px; font-weight:bold; color:#f22; white-space:nowrap;"">
                                                    {amount} đ
                                                </td>
                                            </tr>


                ";
                string product_html = "";
                foreach (var cart in order.carts)
                {

                    var amount_product = cart.product.amount;
                    if (cart.product.flash_sale_todate >= StringHelper.GetCurrentTimeInUtcPlus7(order.created_date) && cart.product.amount_after_flashsale != null && cart.product.amount_after_flashsale > 0)
                    {
                        amount_product = (double)cart.product.amount_after_flashsale;

                    }
                    var url_fixed = cart.product.avatar;
                    if (!url_fixed.Contains(static_url)
                    && !url_fixed.Contains("base64")
                    && !url_fixed.Contains("data:video"))
                    {
                        url_fixed = static_url + cart.product.avatar;
                    }
                    product_html += template
                        .Replace("{image}", url_fixed)
                        .Replace("{name}", cart.product.name)
                        .Replace("{quanity}", cart.quanity.ToString("N0"))
                        .Replace("{amount}", (amount_product * cart.quanity).ToString("N0"))
                        .Replace("{code}", cart.product.code)
                        ;
                }
                htmlContent = htmlContent.Replace("{products}", product_html);
                htmlContent = htmlContent.Replace("{total_discount}", (order.total_discount==null?"":"- "+((double)order.total_discount).ToString("N0")+" đ"));
                string payment_type = "COD";
                switch (order.payment_type)
                {
                    default:
                        {
                        }
                        break;
                    case 2:
                        {
                            payment_type = "Chuyển khoản ngân hàng";
                        }
                        break;
                    case 3:
                        {
                            payment_type = "Thẻ VISA/Master Card";
                        }
                        break;
                    case 4:
                        {
                            payment_type = "Thanh toán QR/PAY";
                        }
                        break;
                    case 5:
                        {
                            payment_type = "Thanh toán tại văn phòng";
                        }
                        break;
                }
                htmlContent = htmlContent.Replace("{payment_type}", payment_type);

                string shipping_type = "Nhận hàng tại BestMall";
                switch (order.delivery_detail.carrier_id)
                {
                    default:
                        {
                        }
                        break;
                    case 2:
                        {
                            shipping_type = "Ninja Van";
                        }
                        break;
                    case 3:
                        {
                            shipping_type = "Viettel Post";
                        }
                        break;
                }
                htmlContent = htmlContent.Replace("{shipping_type}", shipping_type);
                htmlContent = htmlContent.Replace("{shipping_fee}", (order.total_discount == null ? 0 : (double)order.total_discount).ToString("N0") + " đ");

                return htmlContent;
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine($"Lỗi: Không tìm thấy file template email tại: {EmailTemplatePath}. Hãy đảm bảo file đã được đặt trong thư mục đầu ra và thuộc tính 'Copy to Output Directory' đã được thiết lập.");
                LogHelper.InsertLogTelegram("[APP.CHECKOUT] EmailService - SendOrderConfirmationEmail: Không tìm thấy file template email tại:" + EmailTemplatePath);

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi đọc hoặc xử lý template email: {ex.Message}");
                LogHelper.InsertLogTelegram("[APP.CHECKOUT] EmailService - SendOrderConfirmationEmail:" + ex.Message);

                return null;
            }
        }
        private List<Province> GetProvince()
        {
            List<Province> provinces = new List<Province>();
            string provinces_string = "";

            try
            {
                provinces = locationDAL.GetListProvinces();

            }
            catch(Exception ex)
            {
                LogHelper.InsertLogTelegram("[APP.CHECKOUT] EmailService - GetProvince:" + ex.Message);

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
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("[APP.CHECKOUT] EmailService - GetProvince:" + ex.Message);
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
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("[APP.CHECKOUT] EmailService - GetProvince:" + ex.Message);
            }
            return wards;
        }
        private string GetTemplateInFunction()
        {
            return @"
<!DOCTYPE html>
<html lang=""vi"">

<head>
    <meta charset=""UTF-8"">
    <title>Best Mall Email</title>
</head>

<body style=""margin:0; padding:0; background-color:#f2f2f2;"">
    <table width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0""
           style=""background-color:#f2f2f2; padding:20px 0; font-family:Arial, sans-serif;line-height: 1.4;"">
        <tr>
            <td align=""center"">
                <table width=""600"" cellpadding=""0"" cellspacing=""0"" border=""0""
                       style=""background-color:#ffffff; border:1px solid #dddddd; max-width:600px; width:100%;"">
                    <tr>
                        <td style=""padding:30px 20px 10px; text-align:center;"">
                            <h1 style=""font-size:22px; margin:0; color:#002b5b;"">
                                Xin chào
                                {clientname}
                            </h1>
                        </td>
                    </tr>
                    <tr>
                        <td style=""padding:0 20px 30px; text-align:center;"">
                            <h2 style=""font-size:20px; margin:0; color:#002b5b;"">
                                Cảm ơn
                                bạn đã ủng hộ Best Mall!
                            </h2>
                        </td>
                    </tr>
                    <tr>
                        <td style=""border-top:1px solid #E3EBF3;"">
                            <table width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""text-align:center;"">
                                <tr>
                                    <td width=""33.33%"" style=""padding:15px 5px; border-right:1px solid #E3EBF3;"">
                                        <div style=""font-size:15px; color:#002b5b; font-weight:600;  margin-bottom:5px;"">
                                            Mã khách hàng
                                        </div>
                                        <div style=""font-size:16px; color:#9c27b0; "">
                                            {clientcode}
                                        </div>
                                    </td>
                                    <td width=""33.33%"" style=""padding:15px 5px; border-right:1px solid #E3EBF3;"">
                                        <div style=""font-size:15px; color:#002b5b; font-weight:600;  margin-bottom:5px;"">
                                            Mã đơn hàng
                                        </div>
                                        <div style=""font-size:16px; color:#9c27b0; "">
                                            {orderno}
                                        </div>
                                    </td>
                                    <td width=""33.33%"" style=""padding:15px 5px;"">
                                        <div style=""font-size:15px; color:#002b5b; font-weight:600;  margin-bottom:5px;"">
                                            Ngày đặt hàng
                                        </div>
                                        <div style=""font-size:16px; color:#9c27b0; "">
                                            {created_date}
                                        </div>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                    <tr>
                        <td style=""padding:0 20px 10px; text-align:center; border-top:1px solid #E3EBF3;"">
                            <p>
                                Best Mall sẽ xử lý đơn hàng của bạn ngay lập tức, chúng tôi sẽ gửi email cho bạn để cập
                                nhật trạng thái đơn hàng
                            </p>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            <!-- Timeline Table with Line -->
                            <table width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""padding:10px 0;"">
                                <tr>
                                    <td align=""center"">
                                        <table width=""600"" cellpadding=""0"" cellspacing=""0"" border=""0""
                                               style=""width:100%; max-width:600px; text-align:center;"">
                                            <tr>
                                                <td colspan=""3"">
                                                    <img src=""https://static-image.adavigo.com/uploads/images/email/email-order-confirm.png"" width=""85%"" alt="""" />
                                                </td>
                                            </tr>
                                            <!-- Step Icons -->
                                            <tr>
                                                <!-- Step 1 -->
                                                <td width=""33.33%"" style=""vertical-align:top;"">

                                                    <div style=""font-size:14px; font-weight:bold; color:#002b5b; font-family:Arial, sans-serif;"">
                                                        Đơn hàng đã đặt
                                                    </div>
                                                    <div style=""font-size:13px; color:#789;"">{created_date}</div>
                                                </td>

                                                <!-- Step 2 -->
                                                <td width=""33.33%"" style=""vertical-align:top;"">
                                                    <div style=""font-size:14px; font-weight:bold; color:#002b5b; font-family:Arial, sans-serif;"">
                                                        Đang giao hàng
                                                    </div>
                                                    <div style=""font-size:13px; color:#789; display:none;"">{created_date}</div>
                                                </td>

                                                <!-- Step 3 -->
                                                <td width=""33.33%"" style=""vertical-align:top;"">
                                                    <div style=""font-size:14px; font-weight:bold; color:#002b5b; font-family:Arial, sans-serif;"">
                                                        Đã nhận được hàng
                                                    </div>
                                                    <div style=""font-size: 13px; color: #789; display: none;"">{created_date}</div>
                                                </td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                            </table>

                        </td>
                    </tr>
                    <tr>
                        <td>
                            <!-- Order Details -->
                            <table width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0""
                                   style=""max-width:600px; margin:0 auto; font-family:Arial, sans-serif; background:#fff; "">
                                <tr>
                                    <td colspan=""2"" align=""center""
                                        style=""padding:20px 0 10px 0; font-size:16px; font-weight:600; color:#002b5b;"">
                                        Chi tiết đơn hàng
                                    </td>
                                </tr>

                                <!-- Product Row -->
                                <tr>
                                    <td colspan=""2"" style=""padding:15px 20px 10px;border-top:1px solid #e5e5e5;"">
                                        <table width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"">
                                           {products}
                                        </table>
                                    </td>
                                </tr>

                                <!-- Customer + Payment Info -->
                                <tr>
                                    <td colspan=""2"" style=""padding:10px 20px; border-top:1px solid #e5e5e5;"">
                                        <table width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"">
                                            <tr>
                                                <!-- Customer Info -->
                                                <td valign=""top"" style=""width:50%;"">
                                                    <div style=""font-size:14px; font-weight:bold; color:#002b5b; padding-bottom:6px;"">
                                                        {receiver_name}
                                                    </div>
                                                    <div style=""font-size:13px; color:#555;"">
                                                        <strong>Địa chỉ:</strong>
                                                        {address}
                                                    </div>
                                                    <div style=""font-size:13px; color:#555;"">
                                                        <strong>
                                                            Điện
                                                            thoại:
                                                        </strong> <span style=""color:#004ba0; font-weight:bold;"">{phone}</span>
                                                    </div>
                                                </td>

                                                <!-- Payment Info -->
                                                <td valign=""top"" style=""width:50%;"">
                                                    <table cellpadding=""0"" cellspacing=""4"" border=""0""
                                                           style=""font-size:13px; color:#002b5b; width:100%;"">
                                                        <tr>
                                                            <td align=""right"" style=""padding-bottom:2px;"">
                                                                Hình thức
                                                                thanh toán:
                                                            </td>
                                                            <td align=""right"" style=""font-weight:bold;"">COD</td>
                                                        </tr>
                                                        <tr>
                                                            <td align=""right"" style=""padding-bottom:2px;"">
                                                                Hình thức
                                                                vận chuyển:
                                                            </td>
                                                            <td align=""right"" style=""font-weight:bold;"">{shipping_type}</td>
                                                        </tr>
                                                        <tr>
                                                            <td align=""right"">Tiền hàng:</td>
                                                            <td align=""right"">{amount} đ</td>
                                                        </tr>
                                                          <tr>
                                                            <td align=""right"" style=""padding-bottom:2px;"">
                                                                Phí vận chuyển:
                                                            </td>
                                                            <td align=""right"" style=""font-weight:bold;"">{shipping_fee}</td>
                                                        </tr>
                                                        <tr>
                                                            <td align=""right"" style="""">Giảm giá:</td>
                                                            <td align=""right"" style=""color: #f22; "">{total_discount}</td>
                                                        </tr>
                                                        <tr>
                                                            <td align=""right"" style=""font-weight:bold;"">Tổng tiền:</td>
                                                            <td align=""right""
                                                                style=""color:#f22; font-size:16px; font-weight:bold;"">
                                                                {total_amount} đ
                                                            </td>
                                                        </tr>
                                                    </table>
                                                </td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>

                                <!-- Hotline / Support -->
                                <tr>
                                    <td colspan=""2"" align=""center""
                                        style=""background:#f3f7fc; padding:20px 15px; font-size:13px; color:#002b5b;"">
                                        Quý khách có thể phản hồi trực tiếp ở email này hoặc liên hệ với chúng tôi qua
                                        hotline: <strong style=""color:#004ba0;"">0932.888.666</strong>
                                    </td>
                                </tr>
                            </table>

                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>

</html>";
        }
    }
}
