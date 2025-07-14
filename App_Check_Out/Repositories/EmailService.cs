using APP_CHECKOUT.Model.Orders;
using APP_CHECKOUT.Models.Location;
using APP_CHECKOUT.Models.Orders;
using Caching.Elasticsearch;
using DAL;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Net.Mail;
using System.Text;

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
                // Log lỗi chi tiết hơn ở đây
                return false;
            }
        }
        private string ReadEmailTemplateAndPopulate(OrderDetailMongoDbModelExtend order)
        {
            try
            {
                string templatePath = Environment.CurrentDirectory + EmailTemplatePath;
                string htmlContent = File.ReadAllText(templatePath);
                var account_client = accountClientESService.GetById(order.account_client_id);
                var client = clientESService.GetById((long)account_client.ClientId);

                htmlContent = htmlContent.Replace("{clientname}", client.ClientName);
                htmlContent = htmlContent.Replace("{clientcode}", client.ClientCode);
                htmlContent = htmlContent.Replace("{orderno}", order.order_no);
                htmlContent = htmlContent.Replace("{created_date}", order.created_date.ToString("dd/MM/yyyy hh:mm"));
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
                htmlContent = htmlContent.Replace("{amount}", order.total_amount.ToString("N0"));
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
                    if (cart.product.flash_sale_todate >= DateTime.Now && cart.product.amount_after_flashsale != null && cart.product.amount_after_flashsale > 0)
                    {
                        amount_product = (double)cart.product.amount_after_flashsale;

                    }
                    product_html += template.Replace("{image}", cart.product.avatar)
                        .Replace("{image}", cart.product.avatar)
                        .Replace("{name}", cart.product.name)
                        .Replace("{quanity}", cart.quanity.ToString("N0"))
                        .Replace("{amount}", (amount_product * cart.quanity).ToString("N0"))
                        .Replace("{code}", cart.product.code)
                        ;
                }
                htmlContent = htmlContent.Replace("{products}", product_html);
                htmlContent = htmlContent.Replace("{total_discount}", (order.total_discount==null?"":"- "+((double)order.total_discount).ToString("N0")+" đ"));

                return htmlContent;
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine($"Lỗi: Không tìm thấy file template email tại: {EmailTemplatePath}. Hãy đảm bảo file đã được đặt trong thư mục đầu ra và thuộc tính 'Copy to Output Directory' đã được thiết lập.");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi đọc hoặc xử lý template email: {ex.Message}");
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
