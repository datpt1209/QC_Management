using System;
using System.Collections.Generic;

namespace QC_Management.Models;

public partial class UnitTable : BaseViewModel
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Test> Tests { get; set; } = new List<Test>();
}
