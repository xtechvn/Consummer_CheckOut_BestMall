namespace ADAVIGO_FRONTEND.Models.Flights.TrackingVoucher
{
    public class TrackingVoucherResponse
    {
        public int status { get; set; }
        public string msg { get; set; }
        public float percent_decrease { get; set; }
        public string expire_date { get; set; }
        public string voucher_name { get; set; }
        public double total_order_amount_before { get; set; }
        public double discount { get; set; }
        public double total_order_amount_after { get; set; }
        public int voucher_id { get; set; }
        public double value { get; set; }
        public string type { get; set; }
        public int? rule_type { get; set; }
    }
}
