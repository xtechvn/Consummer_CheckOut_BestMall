namespace APP_CHECKOUT.Models.Models.Queue
{
    public class CheckoutQueueModel
    {
        public int event_id { get; set; }
        public string order_mongo_id { get; set; }
        public string? utm_source { get; set; }
        public string? utm_medium { get; set; }
    }
}
