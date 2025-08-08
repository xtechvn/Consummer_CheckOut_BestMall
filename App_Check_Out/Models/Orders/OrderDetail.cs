using System;
using System.Collections.Generic;

namespace Entities.Models;

public partial class OrderDetail
{
    public long OrderDetailId { get; set; }

    public long OrderId { get; set; }

    public string ProductId { get; set; }

    public string ProductCode { get; set; }

    public double? Amount { get; set; }

    public double? Price { get; set; }

    public double? Profit { get; set; }

    public double? Discount { get; set; }

    public int? Quantity { get; set; }

    public double? TotalPrice { get; set; }

    public double? TotalProfit { get; set; }

    public double? TotalDiscount { get; set; }

    public double? TotalAmount { get; set; }

    public string ProductLink { get; set; }

    public int? UserCreate { get; set; }

    public DateTime? CreatedDate { get; set; }

    public int? UserUpdated { get; set; }

    public DateTime? UpdatedDate { get; set; }
    public string ParentProductId { get; set; }
    public long? OrderMergeId { get; set; }

}
