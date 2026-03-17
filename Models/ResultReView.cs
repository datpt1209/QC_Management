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

        private bool _isOutRange;
        public bool isOutRange
        {
            get => _isOutRange;
            set
            {
                _isOutRange = value;
                OnPropertyChanged();
            }
        }

        public virtual ControlInfoDetail IdControlDetailNavigation { get; set; } = null!;
    }
}
