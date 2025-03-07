using System;
using System.Collections.Generic;

namespace QC_Management.Models;

public partial class Test
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public int? IdCategory { get; set; }

    public int IdUnitTable { get; set; }

    public int? Index { get; set; }

    public int? OnlineNumber { get; set; }

    public int? TestType { get; set; }

    public virtual ICollection<CalDetail> CalDetails { get; set; } = new List<CalDetail>();

    public virtual ICollection<CalResult> CalResults { get; set; } = new List<CalResult>();

    public virtual ICollection<ControlInfoDetail> ControlInfoDetails { get; set; } = new List<ControlInfoDetail>();

    public virtual ICollection<DeviceTest> DeviceTests { get; set; } = new List<DeviceTest>();

    public virtual Category? IdCategoryNavigation { get; set; }

    public virtual UnitTable IdUnitTableNavigation { get; set; } = null!;

    public virtual ICollection<ReCalResult> ReCalResults { get; set; } = new List<ReCalResult>();

    public virtual ICollection<ReResult> ReResults { get; set; } = new List<ReResult>();

    public virtual ICollection<Result> Results { get; set; } = new List<Result>();

    public virtual TestType? TestTypeNavigation { get; set; }
}
