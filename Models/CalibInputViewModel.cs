
using System;
using System.Collections.Generic;
using QC_Management.Models;

namespace QC_Management;

public class CalibInputViewModel : BaseViewModel
{
    public int IdTest { get; set; }
    public string? TestName { get; set; }
    public string? Lot { get; set; }
    public int? Level { get; set; }
    public int? CalDetailId { get; set; } = null!; // Thông tin chi tiết của phép đo
    public double? Min { get; set; }
    public double? Max { get; set; }
    public string? Comment { get; set; } = null!;
    private double? _Result;
    public double? Result 
    { 
        get => _Result;
        set
        {
            _Result = value;
            OnPropertyChanged();
            CheckIfOutOfRange();
        }
    } // Để nhập kết quả

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
    private void CheckIfOutOfRange()
    {
        if(Result != null && Min != null && Max != null)
        {
            IsOutRange = Result < Min || Result > Max;
        }
        else
        {
            IsOutRange = null;
        }
    }
}
