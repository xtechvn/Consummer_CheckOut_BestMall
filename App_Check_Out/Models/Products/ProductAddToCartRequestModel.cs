using Entities.ViewModels.Products;

namespace APP_CHECKOUT.Models.Models
{
    public class ProductAddToCartRequestModel
    {
        public string product_id { get; set; }
        public string token { get; set; }
        public int quanity { get; set; }
    }
}
