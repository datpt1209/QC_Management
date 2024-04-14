using System;
using System.Collections.Generic;

namespace QC_Management.Models;

public partial class Result
{
    public int Id { get; set; }

    public int IdTest { get; set; }

    public double Result1 { get; set; }

    public int IdDevice { get; set; }

    public int IdLevel { get; set; }

    public DateTime DateRun { get; set; }

    public int IdUser { get; set; }

    public int? IdControlDetail { get; set; }

    public TimeSpan? Time { get; set; }

    public int? IndexQc { get; set; }

    public string? WestgardRule { get; set; }

    public virtual ControlInfoDetail? IdControlDetailNavigation { get; set; }

    public virtual Device IdDeviceNavigation { get; set; } = null!;

    public virtual LevelQc IdLevelNavigation { get; set; } = null!;

    public virtual Test IdTestNavigation { get; set; } = null!;

    public virtual User IdUserNavigation { get; set; } = null!;
}
