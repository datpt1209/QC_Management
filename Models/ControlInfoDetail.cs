using QC_Management.Models;
using System;
using System.Collections.Generic;

namespace QC_Management;

public partial class ControlInfoDetail
{
    public int Id { get; set; }

    public int IdLevel { get; set; }

    public double MeanNsx { get; set; }

    public double SdNsx { get; set; }

    public double? MeanApp { get; set; }

    public double? SdApp { get; set; }

    public bool? Status { get; set; }

    public int IdTest { get; set; }

    public int IdControlInfo { get; set; }

    public int? IdDevice { get; set; }

    public string? Lot { get; set; }

    public virtual ControlInfo IdControlInfoNavigation { get; set; } = null!;

    public virtual Device? IdDeviceNavigation { get; set; }

    public virtual LevelQc IdLevelNavigation { get; set; } = null!;

    public virtual Test IdTestNavigation { get; set; } = null!;

    public virtual ICollection<Result> Results { get; set; } = new List<Result>();
}
