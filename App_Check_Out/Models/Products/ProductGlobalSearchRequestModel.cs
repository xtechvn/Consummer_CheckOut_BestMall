namespace Entities.ViewModels.Products
{
    public class ProductGlobalSearchRequestModel
    {
        public string keyword { get; set; }
        public string token { get; set; }
        //--Additional search:
        public int? stars { get; set; }
        public string? group_product_id { get; set; }
        public string? brands { get; set; }
        public int? page_index { get; set; }
        public int? page_size { get; set; }
    }
}
