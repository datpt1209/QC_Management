using System;
using System.Collections.Generic;

namespace QC_Management.Models;

public partial class ControlInfo
{
    public int Id { get; set; }

    public string Lot { get; set; } = null!;

    public DateTime ProductionDate { get; set; }

    public DateTime ExpirationDate { get; set; }

    public bool Status { get; set; }

    public int? IdControlType { get; set; }

    public virtual ICollection<ControlInfoDetail> ControlInfoDetails { get; set; } = new List<ControlInfoDetail>();

    public virtual ControlType? IdControlTypeNavigation { get; set; }
    public override string ToString()
    {
        return $"{Lot}";
    }
}
