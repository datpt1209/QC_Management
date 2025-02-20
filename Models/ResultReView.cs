

using Microsoft.Xaml.Behaviors.Core;
using QC_Management.ViewModels;

namespace QC_Management.Models
{
    public class ResultReView : BaseViewModel
    {
        public int? id { get; set; }
        public string TestName { get; set; }
        private double? _Result;
        public double? Result
        {
            get => _Result;
            set
            {
                _Result = value;

                if(value > Max || value < Min)
                {
                    isOutOfRange = true;
                }
                else
                {
                    isOutOfRange = false;
                    if(value > MeanApp + 2*SdApp || value < MeanApp - 2 * SdApp)
                    {
                        isOut2SD = true;
                    }
                    else
                    {
                        isOut2SD = false;
                    }
                }
            }
        }
        public string? LOT { get; set; }
        public int idTest { get; set; }
        public double? MeanApp { get; set; }
        public double? SdApp { get; set; }
        public double? MeanNSX { get; set; }
        public double? SdNSX { get; set; }
        public double? Min { get; set; }
        public double? Max { get; set; }
        public string? Comment { get; set; }

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
        private bool _isOut2SD;
        public bool isOut2SD
        {
            get => _isOut2SD;

            set
            {
                _isOut2SD = value;
                OnPropertyChanged();
            }
        }
        public virtual ControlInfoDetail IdControlDetailNavigation { get; set; } = null!;


    }
}
