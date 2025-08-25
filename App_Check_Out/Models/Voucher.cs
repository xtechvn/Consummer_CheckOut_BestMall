using System;
using System.Collections.Generic;

namespace HuloToys_Service.Models.Models;

public partial class Voucher
{
    public int Id { get; set; }

    public string Code { get; set; }

    public DateTime? Cdate { get; set; }

    public DateTime? Udate { get; set; }

    public DateTime? EDate { get; set; }

    public int LimitUse { get; set; }

    public decimal? PriceSales { get; set; }

    public string? Unit { get; set; }

    /// <summary>
    /// Trường này dùng để phân biệt voucher triển khai này chạy theo rule nào. Ví dụ: rule giảm giá với 1 số tiền vnđ trên toàn bộ đơn hàng. Giảm giá 20% phí first pound đầu tiên của nhãn hàng amazon. 1: triển khai rule giảm giá cho toàn bộ đơn hàng. 2 là rule áp dụng cho 20% phí first pound đầu tiên.
    /// </summary>
    public int? RuleType { get; set; }

    /// <summary>
    /// Trường này để lưu nhóm những user được áp dụng trên voucher này
    /// </summary>
    public string? GroupUserPriority { get; set; }

    /// <summary>
    /// Nêu set true thì hiểu voucher này được public cho các user thanh toán đơn hàng
    /// </summary>
    public bool? IsPublic { get; set; }

    public string? Description { get; set; }

    public bool? IsLimitVoucher { get; set; }

    public double? LimitTotalDiscount { get; set; }

    public string? StoreApply { get; set; }

    public bool? IsMaxPriceProduct { get; set; }

    public double? MinTotalAmount { get; set; }

    public int? CampaignId { get; set; }

    public string? Name { get; set; }

}

