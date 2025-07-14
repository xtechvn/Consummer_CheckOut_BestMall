using Nest;
using System;
using System.Collections.Generic;

namespace APP_CHECKOUT.Models.Location;

public class Province
{
    [PropertyName("Id")]

    public int Id { get; set; }
    [PropertyName("ProvinceId")]

    public string ProvinceId { get; set; }
    [PropertyName("Name")]

    public string Name { get; set; }
    [PropertyName("NameNonUnicode")]

    public string NameNonUnicode { get; set; }
    [PropertyName("Type")]

    public string Type { get; set; }
    [PropertyName("Status")]

    public short? Status { get; set; }
    [PropertyName("CreatedDate")]

    public DateTime? CreatedDate { get; set; }

}

