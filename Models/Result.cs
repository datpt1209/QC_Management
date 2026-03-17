using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using QC_Management.Services;

namespace QC_Management.Models;

public partial class Result : BaseViewModel
{
    public int Id { get; set; }

    // Persisted ZScore (stored in DB)
    private double? _zScore;
    public double? ZScore
    {
        get => _zScore;
        set { _zScore = value; OnPropertyChanged(nameof(ZScore)); }
    }

    public int IdTest { get; set; }
    public int? ResultType { get; set; }

    private double? _Result1;
    public double? Result1
    {
        get => _Result1;
        set
        {
            _Result1 = value;
            OnPropertyChanged();
            // Do not evaluate here; call ApplyLeveyJennings(...) when history is available
        }
    }

    private string? _TempResult;
    public string? TempResult
    {
        get => _TempResult;
        set
        {
            _TempResult = value;
            OnPropertyChanged();
            if (ResultType == 2)
            {
                if (double.TryParse(_TempResult, out double result))
                {
                    Result1 = result;
                    OnPropertyChanged();
                }
                else
                {
                    Result1 = null;
                }
            }
            else
            {
                QualitativeResult = _TempResult;
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

    private string? _westgardRule;
    public string? WestgardRule
    {
        get => _westgardRule;
        set => SetProperty(ref _westgardRule, value);
    }

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

    // New: flag indicating this Result has been resolved by a corrective action
    private bool? _isCorrected;
    public bool? IsCorrected
    {
        get => _isCorrected;
        set
        {
            _isCorrected = value;
            OnPropertyChanged();
        }
    }

    private string? _QualitativeResult;
    public string? QualitativeResult
    {
        get => _QualitativeResult;
        set
        {
            _QualitativeResult = value;
            OnPropertyChanged();
        }
    }
    public bool? IsExclude { get; set; } = false;

    public virtual ControlInfoDetail? IdControlDetailNavigation { get; set; }

    public virtual Device IdDeviceNavigation { get; set; } = null!;

    public virtual LevelQc IdLevelNavigation { get; set; } = null!;

    public virtual Test IdTestNavigation { get; set; } = null!;

    public virtual User IdUserNavigation { get; set; } = null!;

    // --- Persisted fields to record which mean/sd were used and when ---
    private double? _appliedMean;
    public double? AppliedMean
    {
        get => _appliedMean;
        set { _appliedMean = value; OnPropertyChanged(nameof(AppliedMean)); }
    }

    private double? _appliedSd;
    public double? AppliedSd
    {
        get => _appliedSd;
        set { _appliedSd = value; OnPropertyChanged(nameof(AppliedSd)); }
    }

    private DateTime? _appliedAt;
    public DateTime? AppliedAt
    {
        get => _appliedAt;
        set { _appliedAt = value; OnPropertyChanged(nameof(AppliedAt)); }
    }
}
