using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.ViewModels.Products
{
    public class ProductMongoDbModel
    {
        [BsonElement("_id")]
        public string _id { get; set; }
        public void GenID()
        {
            _id = ObjectId.GenerateNewId().ToString();
        }
        public string code { get; set; }

        public double price { get; set; }
        public double profit { get; set; }
        public double amount { get; set; }
        public int quanity_of_stock { get; set; }

        public double discount { get; set; }
        public List<string> images { get; set; }
        public string avatar { get; set; }
        public List<string> videos { get; set; }
        public string name { get; set; }
        public string group_product_id { get; set; }
        public string description { get; set; }
        public List<ProductDetailVariationAttributesMongoDbModel> variation_detail { get; set; }
        public List<ProductSpecificationDetailMongoDbModel> specification { get; set; }
        public List<ProductAttributeMongoDbModel> attributes { get; set; }
        public List<ProductAttributeMongoDbModelItem> attributes_detail { get; set; }
        public List<ProductDiscountOnGroupsBuyModel> discount_group_buy { get; set; }

        public int preorder_status { get; set; }
        public float star { get; set; }
        public int condition_of_product { get; set; }
        public string sku { get; set; }
        public DateTime created_date { get; set; }
        public DateTime updated_last { get; set; }
        public double? amount_max { get; set; }
        public double? amount_min { get; set; }

        public string parent_product_id { get; set; }
        public int status { get; set; }
        public bool? is_one_weight { get; set; }
        public float? weight { get; set; }
        public float? package_width { get; set; }
        public float? package_height { get; set; }
        public float? package_depth { get; set; }
        public int supplier_id { get; set; }
        public int label_id { get; set; }
        public double? old_price { get; set; }
        public string description_ingredients { get; set; }
        public string description_effect { get; set; }
        public string description_usepolicy { get; set; }
        public double? review_count { get; set; }
        public float? rating { get; set; }
        public long? total_sold { get; set; }
        public int? supplier_status { get; set; }
        public List<ProductMongoDbSpecification>? detail_specification { get; set; }
        public List<string> products_buy_with { get; set; }
        public float? profit_value { get; set; }
        public int? profit_value_type { get; set; }
        public string description_delivery { get; set; }
        public string description_refund { get; set; }
        public List<string>? attachment_root { get; set; }
        public List<string>? attachment_product { get; set; }
        public List<string>? attachment_supply { get; set; }
        public List<string>? attachment_confirm { get; set; }
        public int? flashsale_badge_type { get; set; }
        public float? profit_supplier { get; set; }
        public int? profit_supplier_type { get; set; }
    }
    public class ProductMongoDbSpecification
    {
        public string key { get; set; }
        public string value { get; set; }
    }
    public class ProductMongoDbModelFEResponse : ProductMongoDbModel
    {
        public int? exists_flashsale_id { get; set; }
        public string exists_flashsale_name { get; set; }
        public double? amount_after_flashsale { get; set; }
        public DateTime? flash_sale_fromdate { get; set; }
        public DateTime? flash_sale_todate { get; set; }
        public string supplier_name { get; set; } = null;

    }
    public class ProductMongoDbModelFEResponseCollection : ProductMongoDbModelFEResponse
    {
        public string _id { get; set; }
        public string code { get; set; }
        public string name { get; set; }
        public string avatar { get; set; }
        public double price { get; set; }
        public double amount { get; set; }
        public double? amount_max { get; set; }
        public double? amount_min { get; set; }
        public float? rating { get; set; }
        public float star { get; set; }
        public long? total_sold { get; set; }
        public double? review_count { get; set; }
        public double? old_price { get; set; }
        public double discount { get; set; }

    }
}
