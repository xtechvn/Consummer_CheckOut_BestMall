namespace HuloToys_Service.Models.Products
{
    public class LocationProductESModel
    {
        public long locationproductid { get; set; }
        public string productcode { get; set; }
        public int groupproductid { get; set; }
        public int orderno { get; set; }
        public DateTime createon { get; set; }
        public DateTime updatelast { get; set; }
        public int userid { get; set; }
    }
}
