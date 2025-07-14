using Entities.ViewModels.Products;

namespace APP_CHECKOUT.Models.Models
{
    public class ProductListResponseModel
    {
        public List<ProductMongoDbModel> items { get; set; }
        public long count { get; set; }
    }
    public class ProductListResponseFEModel
    {
        public List<ProductMongoDbModelFEResponse> items { get; set; }
        public long count { get; set; }
    }
}
