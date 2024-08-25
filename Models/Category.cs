using System;
using System.Collections.Generic;

namespace QC_Management.Models;

public partial class Category : BaseViewModel
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Device> Devices { get; set; } = new List<Device>();

    public virtual ICollection<Test> Tests { get; set; } = new List<Test>();
    public virtual ICollection<ControlType> ControlTypes { get; set; } = new List<ControlType>();
}
