using System;
using System.Collections.Generic;

namespace HuloToys_Service.Models.Models;

public partial class AllotmentFund
{
    public int Id { get; set; }

    public int FundType { get; set; }

    public double AccountBalance { get; set; }

    public long? AccountClientId { get; set; }

    public DateTime? CreateDate { get; set; }

    public DateTime? UpdateTime { get; set; }

    public virtual ICollection<AllotmentUse> AllotmentUses { get; set; } = new List<AllotmentUse>();
}
