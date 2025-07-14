using Entities.ViewModels;
using Entities.ViewModels.Products;
using HuloToys_Service.Models.ElasticSearch;

namespace APP_CHECKOUT.Models.Models
{
    public class ProductDetailResponseDbModel
    {
        public ProductMongoDbModel product_main { get; set; }
        public List<ProductMongoDbModel> product_sub { get; set; }
       

    }
    public class ProductDetailResponseModel
    {
       public ProductMongoDbModelFEResponse product_main { get; set; }
       public List<ProductMongoDbModelFEResponse> product_sub { get; set; }
        public ProductDetailResponseModelCertificate cert { get; set; }
        public ProductDetailResponseModelFavourite favourite { get; set; }
        public List<ProductMongoDbModelFEResponse> product_buy_with { get; set; }
        public List<ProductDetailResponseModelProductBuyWith> product_buy_with_output { get; set; }
        public List<GroupProductESModel> groups { get; set; }

    }
    public class ProductDetailResponseModelCertificate {
        public List<string> root_product { get; set; }
        public List<string> product { get; set; }
        public List<string> supply { get; set; }
        public List<string> confirm { get; set; }

    } 
    public class ProductDetailResponseModelFavourite
    {
       public bool is_favourite { get; set; }
       public long count { get; set; }

    }
    public class ProductDetailResponseModelProductBuyWith
    {
        public string _id { get; set; }
        public string code { get; set; }

        public double amount { get; set; }

        public string name { get; set; }

        public string avatar { get; set; }
        public string variation_detail { get; set; }
        public int? exists_flashsale_id { get; set; }
        public string exists_flashsale_name { get; set; }
        public double? amount_after_flashsale { get; set; }
        public DateTime? flash_sale_fromdate { get; set; }
        public DateTime? flash_sale_todate { get; set; }

    }
}
