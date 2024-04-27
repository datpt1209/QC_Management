using System;
using System.Collections.Generic;

namespace QC_Management.Models;

public partial class Result:BaseViewModel
{
    public int Id { get; set; }

    public int IdTest { get; set; }


    private double _Result1;
    public double Result1
    {
        get => _Result1;
        set
        {
            _Result1 = value;
            if (value > (IdControlDetailNavigation.MeanApp + 2*IdControlDetailNavigation.SdApp) || value < (IdControlDetailNavigation.MeanApp - 2 * IdControlDetailNavigation.SdApp))
            {
                IsOutRange = true;
            }
            else
            {
                IsOutRange = false;
            }
        }
    }

    public int IdDevice { get; set; }

    public int IdLevel { get; set; }

    public DateTime DateRun { get; set; }

    public int IdUser { get; set; }

    public int? IdControlDetail { get; set; }

    public TimeSpan? Time { get; set; }

    public int? IndexQc { get; set; }

    public string? WestgardRule { get; set; }

    public string? Comment { get; set; }


    private bool? _isOutOfRange;
    public bool? IsOutRange
    {
        get => _isOutOfRange;

        set
        {
            _isOutOfRange = value;
            OnPropertyChanged();
        }
    }

    public virtual ControlInfoDetail? IdControlDetailNavigation { get; set; }

    public virtual Device IdDeviceNavigation { get; set; } = null!;

    public virtual LevelQc IdLevelNavigation { get; set; } = null!;

    public virtual Test IdTestNavigation { get; set; } = null!;

    public virtual User IdUserNavigation { get; set; } = null!;
}
