using System;
using System.Collections.Generic;

namespace QC_Management.Models;

public partial class CalDetail
{
    public int Id { get; set; }

    public int IdTest { get; set; }

    public int IdCalInfor { get; set; }

    public double? MinValue { get; set; }

    public double? MaxValue { get; set; }

    public bool Status { get; set; }

    public int IdDevice { get; set; }

    public int? Level { get; set; }

    public virtual ICollection<CalResult> CalResults { get; set; } = new List<CalResult>();

    public virtual CalInfor IdCalInforNavigation { get; set; } = null!;

    public virtual Device IdDeviceNavigation { get; set; } = null!;

    public virtual Test IdTestNavigation { get; set; } = null!;
}
