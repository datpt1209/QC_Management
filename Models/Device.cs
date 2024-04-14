using System.Collections.Generic;

namespace QC_Management.Models;

public partial class Device : BaseViewModel
{

    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public int? IdCategory { get; set; }

    public virtual ICollection<ControlInfoDetail> ControlInfoDetails { get; set; } = new List<ControlInfoDetail>();

    public virtual ICollection<DeviceTest> DeviceTests { get; set; } = new List<DeviceTest>();

    public virtual Category? IdCategoryNavigation { get; set; }

    public virtual ICollection<Result> Results { get; set; } = new List<Result>();
}
