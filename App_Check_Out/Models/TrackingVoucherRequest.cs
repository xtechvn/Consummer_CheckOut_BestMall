namespace ADAVIGO_FRONTEND.Models.Flights.TrackingVoucher
{
    public class TrackingVoucherRequest
    {
        public string voucher_name { get; set; }
        public long user_id { get; set; }
        public string service_id { get; set; }
        public string token { get; set; }
        public int project_type { get; set; }
        public double total_order_amount_before { get; set; }

    }
}
