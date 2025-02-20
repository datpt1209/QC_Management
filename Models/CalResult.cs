using System;
using System.Collections.Generic;
using QC_Management.Models;

namespace QC_Management;

public partial class CalResult
{
    public int Id { get; set; }

    public int IdDevice { get; set; }

    public int IdTest { get; set; }

    public int Level { get; set; }

    public DateTime DateRun { get; set; }

    public TimeSpan? Time { get; set; }

    public int? IndexCal { get; set; }

    public double Result { get; set; }

    public int? IdCalDetail { get; set; }

    public int? IdUser { get; set; }

    public string? Comment { get; set; } = null!;

    public virtual CalDetail? IdCalDetailNavigation { get; set; }

    public virtual Device IdDeviceNavigation { get; set; } = null!;

    public virtual Test IdTestNavigation { get; set; } = null!;

    public virtual User? IdUserNavigation { get; set; }
}
