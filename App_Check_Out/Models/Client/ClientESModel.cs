using Nest;
using System;
using System.Collections.Generic;


namespace APP_CHECKOUT.Models.Client
{
    public  class ClientESModel
    {
        [PropertyName("Id")]

        public long Id { get; set; }
        [PropertyName("ClientMapId")]

        public int? ClientMapId { get; set; }
        [PropertyName("SaleMapId")]

        public int? SaleMapId { get; set; }
        [PropertyName("ClientType")]

        public int? ClientType { get; set; }
        [PropertyName("ClientName")]

        public string ClientName { get; set; }
        [PropertyName("Email")]

        public string Email { get; set; }
        [PropertyName("Gender")]

        public int? Gender { get; set; }
        [PropertyName("Status")]

        public int Status { get; set; }
        [PropertyName("Note")]

        public string Note { get; set; }
        [PropertyName("Avartar")]

        public string Avartar { get; set; }
        [PropertyName("JoinDate")]

        public DateTime JoinDate { get; set; }
        [PropertyName("IsReceiverInfoEmail")]

        public bool? IsReceiverInfoEmail { get; set; }
        [PropertyName("Phone")]

        public string Phone { get; set; }
        [PropertyName("Birthday")]

        public DateTime? Birthday { get; set; }
        [PropertyName("UpdateTime")]

        public DateTime? UpdateTime { get; set; }
        [PropertyName("TaxNo")]

        public string TaxNo { get; set; }
        [PropertyName("AgencyType")]

        public int? AgencyType { get; set; }
        [PropertyName("PermisionType")]

        public int? PermisionType { get; set; }
        [PropertyName("BusinessAddress")]

        public string BusinessAddress { get; set; }
        [PropertyName("ExportBillAddress")]

        public string ExportBillAddress { get; set; }
        [PropertyName("ClientCode")]

        public string ClientCode { get; set; }
        [PropertyName("IsRegisterAffiliate")]

        public bool? IsRegisterAffiliate { get; set; }
        [PropertyName("ReferralId")]

        public string ReferralId { get; set; }
        [PropertyName("ParentId")]

        public int? ParentId { get; set; }

    }
}
