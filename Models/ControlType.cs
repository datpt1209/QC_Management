using System;
using System.Collections.Generic;

namespace QC_Management.Models;

public partial class ControlType : BaseViewModel
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public int IdCategory { get; set; }

    public virtual ICollection<ControlInfo> ControlInfos { get; set; } = new List<ControlInfo>();

    public virtual Category IdCategoryNavigation { get; set; } = null!;
}
