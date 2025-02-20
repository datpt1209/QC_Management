using System;
using System.Collections.Generic;

namespace QC_Management.Models;

public partial class ReCalResult:BaseViewModel
{
    public int Id { get; set; }

    public int? IdDevice { get; set; }

    public int? IdTest { get; set; }

    public int? Level { get; set; }

    public DateTime? DateRun { get; set; }

    public TimeSpan? Time { get; set; }

    public int? IndexCal { get; set; }

    public double? Result { get; set; }

    public virtual Device? IdDeviceNavigation { get; set; }

    public virtual Test IdTestNavigation { get; set; } = null!;

}
