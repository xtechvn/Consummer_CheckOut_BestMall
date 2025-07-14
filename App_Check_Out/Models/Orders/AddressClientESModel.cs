using Nest;
using System;
using System.Collections.Generic;

namespace Entities.Models
{
    public partial class AddressClientESModel
    {
        [PropertyName("id")]

        public long Id { get; set; }
        [PropertyName("ClientId")]

        public long ClientId { get; set; }
        [PropertyName("ReceiverName")]

        public string ReceiverName { get; set; }
        [PropertyName("Phone")]

        public string Phone { get; set; }
        [PropertyName("ProvinceId")]

        public string ProvinceId { get; set; }
        [PropertyName("DistrictId")]

        public string DistrictId { get; set; }
        [PropertyName("WardId")]

        public string WardId { get; set; }
        [PropertyName("Address")]

        public string Address { get; set; }
        [PropertyName("Status")]

        public int? Status { get; set; }
        [PropertyName("IsActive")]

        public bool IsActive { get; set; }
        [PropertyName("CreatedOn")]

        public DateTime? CreatedOn { get; set; }
        [PropertyName("UpdateTime")]

        public DateTime? UpdateTime { get; set; }

    }


}
