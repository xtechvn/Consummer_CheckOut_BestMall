using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APP_CHECKOUT.Models.NhanhVN
{
 
    public class NhanhVNLocationResponseLocation
    {
        //--General
        public int id { get; set; }
        public string name { get; set; }

        //--District and ward:
        public int? parentId { get; set; }

        //--District data only:
        public int? cityId { get; set; }
        public int? cityLocationId { get; set; }

        //--Ward Data only:
        public int? districtId { get; set; }
        public int? districtLocationId { get; set; }

    }
}
