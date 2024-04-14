using System;
using System.Collections.Generic;

namespace QC_Management.Models;

public partial class ControlInfo : BaseViewModel
{

    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Lot { get; set; } = null!;

    public DateTime ProductionDate { get; set; }

    public DateTime ExpirationDate { get; set; }

    public bool Status { get; set; }

    public int? IdCategory { get; set; }

    public virtual ICollection<ControlInfoDetail> ControlInfoDetails { get; set; } = new List<ControlInfoDetail>();

    public virtual Category? IdCategoryNavigation { get; set; }

    public override string ToString()
    {
        return $"{this.Name} - {this.Lot}";
    }
}
