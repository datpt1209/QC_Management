using System;
using System.Collections.Generic;

namespace QC_Management.Models;

public partial class Result : BaseViewModel
{
    public int Id { get; set; }

    public int IdTest { get; set; }

    private double? _Result1;
    public double? Result1
    {
        get => _Result1;
         set
        {
            _Result1 = value;
            if (value > (IdControlDetailNavigation.CurMean + 2 * IdControlDetailNavigation.CurSd) || value < (IdControlDetailNavigation.CurMean - 2 * IdControlDetailNavigation.CurSd))
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

    public virtual ControlInfoDetail? IdControlDetailNavigation { get; set; }

    public virtual Device IdDeviceNavigation { get; set; } = null!;

    public virtual LevelQc IdLevelNavigation { get; set; } = null!;

    public virtual Test IdTestNavigation { get; set; } = null!;

    public virtual User IdUserNavigation { get; set; } = null!;

    // Method to set the result value based on the test type
    public void SetResult(object result)
    {
        if (IdTestNavigation.TestTypeNavigation.Id == 2 )
        {
            if (result is double quantitativeResult)
            {
                Result1 = quantitativeResult;
            }
            else
            {
                throw new ArgumentException("Invalid result type for quantitative test.");
            }
        }
        else if (IdTestNavigation.TestTypeNavigation.Id == 1)
        {
            if (result is string qualitativeResult)
            {
                QualitativeResult = qualitativeResult;
            }
            else
            {
                throw new ArgumentException("Invalid result type for qualitative test.");
            }
        }
        else
        {
            throw new InvalidOperationException("Unknown test type.");
        }
    }

    // Method to validate the result based on the test type
    public bool ValidateResult()
    {
        if (IdTestNavigation.TestTypeNavigation.Id == 2)
        {
            return Result1 != default;
        }
        else if (IdTestNavigation.TestTypeNavigation.Id == 1)
        {
            return !string.IsNullOrEmpty(QualitativeResult);
        }
        else
        {
            throw new InvalidOperationException("Unknown test type.");
        }
    }
}
