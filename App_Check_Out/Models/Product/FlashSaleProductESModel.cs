using System;

public class FlashSaleProductESModel
{

   public long id { get; set; } // ID ElasticSearch
   public long flashsale_productid { get; set; } 
    public int? flashsale_id { get; set; }

    public string productid { get; set; }

    public double? discountvalue { get; set; }

    public int? valuetype { get; set; }

    public int? status { get; set; }

    public int? position { get; set; }
    public bool? supersale { get; set; }

}