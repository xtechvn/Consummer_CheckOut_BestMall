using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.ViewModels.Products
{
    public class ProductMongoDbSummitModel: ProductMongoDbModel
    {
        public List<ProductDetailVariationMongoDbModel> variations { get; set; }


    }
}
