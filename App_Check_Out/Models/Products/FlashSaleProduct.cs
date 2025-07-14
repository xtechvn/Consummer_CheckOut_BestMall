using System;
using System.Collections.Generic;

namespace HuloToys_Service.Models.Models;

public partial class FlashSaleProduct
{
    public long Id { get; set; }

    public int? CampaignId { get; set; }

    public string? ProductId { get; set; }

    public double? DiscountValue { get; set; }

    public int? ValueType { get; set; }

    public int? Status { get; set; }

    public int? Position { get; set; }

    public bool? SuperSale { get; set; }
}
