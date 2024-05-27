

using Microsoft.Xaml.Behaviors.Core;
using QC_Management.ViewModels;

namespace QC_Management.Models
{
    public class ResultView : BaseViewModel
    {
        public int? id { get; set; }
        public string TestName { get; set; }
        public string? QCName { get; set; }
        private double? _Result;
        public double? Result
        {
            get => _Result;
            set
            {
                _Result = value;
                if(value > Mean*2*Sd || value < Mean - 2 * Sd)
                {
                    isOut2SD = true;
                    if (value > Max || value < Min)
                    {
                        isOutOfRange = true;
                    }
                }
                else
                {
                    isOut2SD = false;
                    isOutOfRange = false;
                }
            }
        }
        public string? LOT { get; set; }
        public int idTest { get; set; }
        public double? Mean { get; set; }
        public double? Sd { get; set; }
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
            get => _isOutOfRange;

            set
            {
                _isOut2SD = value;
                OnPropertyChanged();
            }
        }
        public virtual ControlInfoDetail IdControlDetailNavigation { get; set; } = null!;


    }
}
