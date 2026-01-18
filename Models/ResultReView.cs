using Microsoft.Xaml.Behaviors.Core;
using QC_Management.ViewModels;
using System.Windows.Forms;

namespace QC_Management.Models
{
    public class ResultReView : BaseViewModel
    {
        public int? id { get; set; }
        public string TestName { get; set; }

        private string? _TempResult;
        public string? TempResult
        {
            get =>_TempResult;
            set
            {
                _TempResult = value;
                OnPropertyChanged();
                //CheckIfOutOfRange();
            }
        }

        private void CheckIfOutOfRange()
        {
            if (ResultType == 1 && !string.IsNullOrEmpty(_TempResult))
            {
               isOutOfRange = !IdControlDetailNavigation.IsQualitativeResultAcceptable(_TempResult);
            }
            else if (ResultType == 2 && !string.IsNullOrEmpty(_TempResult))
            {
                // Parse TempResult to double and set Result
                if (double.TryParse(_TempResult, out double resultValue))
                {
                    if (double.TryParse(MeanApp, out double meanApp))
                    {
                        isOut2SD = resultValue > meanApp + 2 * SdApp || resultValue < meanApp - 2 * SdApp;
                    }
                    else
                    {
                        MessageBox.Show("MeanApp is not a number");
                    }
                }
                else
                {
                    MessageBox.Show("Please enter a valid number");
                }
            }
        }

        public int? ResultType { get; set; }
        public Test? Test { get; set; } 
        public string? LOT { get; set; }
        public int idTest { get; set; }
        public string? MeanApp { get; set; }
        public string? QualitativeMean { get; set; }
        public double? SdApp { get; set; }
        public double? MeanNSX { get; set; }
        public double? SdNSX { get; set; }
        public double? Min { get; set; }
        public double? Max { get; set; }
        public string? Comment { get; set; }

        // New: store detected Westgard rule / error directly on the view item
        private string? _WestgardRule;
        public string? WestgardRule
        {
            get => _WestgardRule;
            set
            {
                _WestgardRule = value;
                OnPropertyChanged();
            }
        }

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
