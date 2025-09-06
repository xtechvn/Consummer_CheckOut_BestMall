using System;
using System.Collections.Generic;

namespace HuloToys_Service.Models.Models;

public partial class AllotmentUse
{
    public int Id { get; set; }

    /// <summary>
    /// Là lưu trữ id dịch vụ
    /// </summary>
    public long DataId { get; set; }

    /// <summary>
    /// Ngày tạo đơn hàng
    /// </summary>
    public DateTime CreateDate { get; set; }

    /// <summary>
    /// Số tiền đã sử dụng cho dịch vụ
    /// </summary>
    public double AmountUse { get; set; }

    /// <summary>
    /// Thông tin số tiền của quỹ đã được phân bổ
    /// </summary>
    public int AllotmentFundId { get; set; }

    public long AccountClientId { get; set; }

    public short ServiceType { get; set; }

    public long ClientId { get; set; }

    public virtual AllotmentFund AllomentFund { get; set; } = null!;
    public int PaymentStatus { get; set; }

}
