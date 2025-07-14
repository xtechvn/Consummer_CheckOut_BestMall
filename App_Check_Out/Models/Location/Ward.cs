using Nest;
using System;
using System.Collections.Generic;

namespace APP_CHECKOUT.Models.Location;

public class Ward
{
    [PropertyName("Id")]

    public int Id { get; set; }
    [PropertyName("WardId")]

    public string WardId { get; set; }
    [PropertyName("Name")]

    public string Name { get; set; }
    [PropertyName("NameNonUnicode")]

    public string NameNonUnicode { get; set; }
    [PropertyName("Type")]

    public string Type { get; set; }
    [PropertyName("Location")]

    public string Location { get; set; }
    [PropertyName("DistrictId")]

    public string DistrictId { get; set; }
    [PropertyName("Status")]

    public short? Status { get; set; }
    [PropertyName("CreatedDate")]

    public DateTime? CreatedDate { get; set; }

}