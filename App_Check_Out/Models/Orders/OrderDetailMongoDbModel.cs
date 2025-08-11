using APP_CHECKOUT.Model.Orders;
using APP_CHECKOUT.Models.NinjaVan;
using HuloToys_Service.Models.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace APP_CHECKOUT.Models.Orders
{
    public class OrderDetailMongoDbModelExtend : OrderDetailMongoDbModel
    {
        public string email { get; set; }
        public DateTime created_date { get; set; }

    }
    public class OrderDetailMongoDbModel
    {
        [BsonElement("_id")]
        public string _id { get; set; }
        public void GenID()
        {
            _id = ObjectId.GenerateNewId(DateTime.Now).ToString();
        }
        public long account_client_id { get; set; }
        public int payment_type { get; set; }
        public int delivery_type { get; set; }
        public long order_id { get; set; }
        public string order_no { get; set; }

        public double total_amount { get; set; }
        public double? total_price { get; set; }
        public double? total_profit { get; set; }
        public double? total_discount { get; set; }
        public List<CartItemMongoDbModel> carts { get; set; }
        public string utm_source { get; set; }
        public string utm_medium { get; set; }
        public int? voucher_id { get; set; }
        public string voucher_code { get; set; }

        public string receivername { get; set; }

        public string phone { get; set; }

        public string? provinceid { get; set; }

        public string? districtid { get; set; }

        public string? wardid { get; set; }

        public string address { get; set; }
        public long address_id { get; set; }
        public double? shipping_fee { get; set; } = 0;
        public ShippingFeeRequestModel delivery_detail { get; set; }
        public int? flashsale_badge_type { get; set; }
        public double? total_amount_product { get; set; }
        public List<OrderDetailMongoDbVoucherApply>? voucher_apply { get; set; }
        public List<OrderDetailMongoDbDelivery>? delivery_order { get; set; }
        public List<int> list_voucher_id { get; set; }
        public List<string> list_voucher_code { get; set; }
    }
    public class OrderDetailMongoDbVoucherApply
    {
        public int voucher_id { get; set; }
        public string voucher_code { get; set; }
        public int? RuleType { get; set; }
        public decimal? PriceSales { get; set; }
        public string? Unit { get; set; }
        public int? SupplierId { get; set; }

        public double TotalDiscount { get; set; }
    }
    public class OrderDetailMongoDbDelivery
    {
        public int? SupplierId { get; set; }
        public double? shipping_fee { get; set; } = 0;
        public int? package_weight { get; set; }


    }
}
