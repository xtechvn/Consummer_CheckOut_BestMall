using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.ViewModels.Products
{
    public class ProductAttributeMongoDbModel
    {
        public string _id { get; set; }
        public string name { get; set; }


    }
    public class ProductAttributeMongoDbModelItem
    {
        public string attribute_id { get; set; }
        public string img { get; set; }
        public string name { get; set; }
    }
}
