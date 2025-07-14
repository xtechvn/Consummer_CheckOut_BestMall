using Nest;
using System;
using System.Collections.Generic;


namespace APP_CHECKOUT.Models.Account
{
    public class AccountESModel
    {
        [PropertyName("Id")]
        public int Id { get; set; }

        [PropertyName("ClientId")]
        public long? ClientId { get; set; }

        [PropertyName("ClientType")]
        public int? ClientType { get; set; }
        [PropertyName("UserName")]
        public string UserName { get; set; }
        [PropertyName("Password")]
        public string Password { get; set; }
        [PropertyName("PasswordBackup")]
        public string PasswordBackup { get; set; }
        [PropertyName("ForgotPasswordToken")]
        public string ForgotPasswordToken { get; set; }
        [PropertyName("Status")]
        public byte? Status { get; set; }
        [PropertyName("GroupPermission")]
        public int? GroupPermission { get; set; }

    }
    public partial class AccountApiESModel
    {
        [PropertyName("Id")]

        public int Id { get; set; }
        [PropertyName("UserName")]

        public string UserName { get; set; }
        [PropertyName("Password")]

        public string Password { get; set; }
        [PropertyName("Status")]

        public int? Status { get; set; }

    }
}
