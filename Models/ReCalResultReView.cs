using System;
using System.Collections.Generic;

namespace QC_Management.Models;

public partial class ReCalResultReView:BaseViewModel
{
    public int Id { get; set; }

    public int? IdDevice { get; set; }

    public int? IdTest { get; set; }

    public int? Level { get; set; }

    public DateTime? DateRun { get; set; }

    public TimeSpan? Time { get; set; }

    public string? Comment { get; set; }
    public int? IndexCal { get; set; }
    public virtual Device? IdDeviceNavigation { get; set; }
    public virtual Test? IdTestNavigation { get; set; }
    public virtual CalDetail? IdCalDetailNavigation { get; set; }
    public double? Min { get; set; }
    public double? Max { get; set; }
    public string? LOT { get; set; }

    private bool _isOutOfRange;
    public bool isOutOfRange
    {
        get => _isOutOfRange;

        set
        {
            _isOutOfRange = value;
            OnPropertyChanged();
        }
    }

    private double? _Result;
    public double? Result
    {
        get => _Result;
        set
        {
            _Result = value;

            if (value > Max || value < Min)
            {
                isOutOfRange = true;
            }
            else
            {
                isOutOfRange = false;
            }
        }
    }
}
