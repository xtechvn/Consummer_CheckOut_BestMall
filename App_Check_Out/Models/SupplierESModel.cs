using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.ViewModels.ElasticSearch
{
    public class SupplierESModel
    {

        public long id { get; set; } // ID ElasticSearch
        public int supplierid { get; set; }

        public string suppliercode { get; set; }

        public string fullname { get; set; }

        public string shortname { get; set; }

        public string email { get; set; }

        public string phone { get; set; }

        public int? provinceid { get; set; }

        public string taxcode { get; set; }

        public int? establishedyear { get; set; }

        public int? ratingstar { get; set; }

        public string address { get; set; }

        public int? chainbrands { get; set; }

        public int? verifydate { get; set; }

        public int? salerid { get; set; }

        public string description { get; set; }

        public string servicetype { get; set; }

        public string residencetype { get; set; }

        public bool? isdisplaywebsite { get; set; }

        public int? createdby { get; set; }

        public DateTime? createddate { get; set; }

        public int? updatedby { get; set; }

        public DateTime? updateddate { get; set; }

        public int? status { get; set; }
        public int? districtid { get; set; }

        public int? wardid { get; set; }
        public string bannermain { get; set; }

        public string bannersub { get; set; }
    }
}
