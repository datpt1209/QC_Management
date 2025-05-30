using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace QC_Management.Models;

public partial class Result : BaseViewModel
{
    public int Id { get; set; }

    [NotMapped]
    public double? ZScore { get; set; }
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
            CheckIfOutOfRange();
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
                if(double.TryParse(_TempResult, out double result))
                {
                    Result1 = result;
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

    private bool? _isOutOfRangeNSX;
    public bool? IsOutRangeNSX
    {
        get => _isOutOfRangeNSX;
        set
        {
            _isOutOfRangeNSX = value;
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
            CheckIfOutOfRange();
        }
    }
    public bool? IsExclude { get; set; } = false; // Default value is false

    public virtual ControlInfoDetail? IdControlDetailNavigation { get; set; }

    public virtual Device IdDeviceNavigation { get; set; } = null!;

    public virtual LevelQc IdLevelNavigation { get; set; } = null!;

    public virtual Test IdTestNavigation { get; set; } = null!;

    public virtual User IdUserNavigation { get; set; } = null!;
    private void CheckIfOutOfRange()
    {
        if (IdTestNavigation.TestType == 1 && !string.IsNullOrEmpty(_QualitativeResult))
        {
            IsOutRange = IsOutRangeNSX = !IdControlDetailNavigation.IsQualitativeResultAcceptable(_QualitativeResult);
        }
        if(IdTestNavigation.TestType == 2 && _Result1 != null)
        {
            IsOutRange = _Result1 > (IdControlDetailNavigation.CurMean + 2 * IdControlDetailNavigation.CurSd) 
                || _Result1 < (IdControlDetailNavigation.CurMean - 2 * IdControlDetailNavigation.CurSd);

            IsOutRangeNSX = _Result1 > (IdControlDetailNavigation.MeanNsx + 2 * IdControlDetailNavigation.SdNsx) 
                || _Result1 < (IdControlDetailNavigation.MeanNsx - 2 * IdControlDetailNavigation.SdNsx);
        }
    }
}
