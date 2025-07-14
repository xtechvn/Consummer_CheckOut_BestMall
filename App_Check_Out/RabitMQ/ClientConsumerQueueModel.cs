using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Queue
{
    public class ClientConsumerQueueModel
    {
        public int type { get; set; } // phân biệt các data nhận về lấy từ queue
        public string data_push { get; set; }   // data queue từ nơi khác push về
    }
    public class AccountClientViewModel
    {
        public int Id { get; set; }
        public long? ClientId { get; set; }
        public int ClientType { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Name { get; set; }
        public string ClientName { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string ForgotPasswordToken { get; set; }
        public string GoogleToken { get; set; }
        public byte? Status { get; set; }
        public byte? isReceiverInfoEmail { get; set; }
        public string ClientCode { get; set; }

    }
}
