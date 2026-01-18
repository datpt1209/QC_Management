using System;
using System.Collections.Generic;

namespace QC_Management.Models;

public partial class ReResult:BaseViewModel
{
    public int Id { get; set; }

    public int IdLevel { get; set; }

    public int IdDevice { get; set; }

    public int IdTest { get; set; }

    public double? Result { get; set; }

    public string? QualitativeResult { get; set; }
    
    public DateTime Date { get; set; }

    public TimeSpan Time { get; set; }

    public int? Index { get; set; }

    public virtual Device IdDeviceNavigation { get; set; } = null!;

    public virtual LevelQc IdLevelNavigation { get; set; } = null!;

    public virtual Test IdTestNavigation { get; set; } = null!;


}
