using System;
using System.Collections.Generic;

namespace Entities.Models;

public partial class OrderMerge
{
    public long Id { get; set; }

    public long ClientId { get; set; }

    public string OrderNo { get; set; }

    public DateTime CreatedDate { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? UpdateLast { get; set; }

    public int? UserUpdateId { get; set; }

    public double? Price { get; set; }

    public double? Profit { get; set; }

    public double? Discount { get; set; }

    public double? Amount { get; set; }

    public int OrderStatus { get; set; }

    public short PaymentType { get; set; }

    public int PaymentStatus { get; set; }

    public string UtmSource { get; set; }

    public string UtmMedium { get; set; }

    /// <summary>
    /// Chính là label so với wiframe
    /// </summary>
    public string Note { get; set; }

    public string VoucherId { get; set; }

    public int? IsDelete { get; set; }

    public int? UserId { get; set; }

    public string UserGroupIds { get; set; }

    public string ReceiverName { get; set; }

    public string Phone { get; set; }

    public int? ProvinceId { get; set; }

    public int? DistrictId { get; set; }

    public int? WardId { get; set; }

    public string Address { get; set; }

    public int? RefundStatus { get; set; }

    public string RefundReason { get; set; }

    public DateTime? RefundDate { get; set; }
    public double? ShippingFee { get; set; }

}
