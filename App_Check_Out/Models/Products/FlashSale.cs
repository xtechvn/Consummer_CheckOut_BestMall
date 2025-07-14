using System;
using System.Collections.Generic;

namespace HuloToys_Service.Models.Models;

public partial class FlashSale
{
    public int Id { get; set; }

    public DateTime FromDate { get; set; }

    public DateTime ToDate { get; set; }

    public int? ClientTypeId { get; set; }

    public byte Status { get; set; }

    public DateTime CreateDate { get; set; }

    public DateTime UpdateLast { get; set; }

    public long UserUpdateId { get; set; }

    public int UserCreateId { get; set; }

    public int? SupplierId { get; set; }

    public string? Name { get; set; }

    public string? Banner { get; set; }

}
