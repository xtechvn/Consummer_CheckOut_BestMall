using Nest;

namespace entities.models;

public partial class OrderDetailESModel
{
    [PropertyName("OrderDetailId")]

    public long OrderDetailId { get; set; }
    [PropertyName("OrderId")]

    public long OrderId { get; set; }
    [PropertyName("ProductId")]

    public string ProductId { get; set; }
    [PropertyName("ProductCode")]

    public string ProductCode { get; set; }
    [PropertyName("Amount")]

    public double? Amount { get; set; }
    [PropertyName("Price")]

    public double? Price { get; set; }
    [PropertyName("Profit")]

    public double? Profit { get; set; }
    [PropertyName("Discount")]

    public double? Discount { get; set; }
    [PropertyName("Quantity")]

    public int? Quantity { get; set; }
    [PropertyName("TotalPrice")]

    public double? TotalPrice { get; set; }
    [PropertyName("TotalProfit")]

    public double? TotalProfit { get; set; }
    [PropertyName("TotalDiscount")]

    public double? TotalDiscount { get; set; }
    [PropertyName("TotalAmount")]

    public double? TotalAmount { get; set; }
    [PropertyName("ProductLink")]

    public string ProductLink { get; set; }
    [PropertyName("UserCreate")]

    public int? UserCreate { get; set; }
    [PropertyName("CreatedDate")]

    public DateTime? CreatedDate { get; set; }
    [PropertyName("UserUpdated")]

    public int? UserUpdated { get; set; }
    [PropertyName("UpdatedDate")]

    public DateTime? UpdatedDate { get; set; }
}
