

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
                if(value > Max || value < Min)
                {
                    isOutOfRange = true;
                }
                else
                {
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
        public bool isOutOfRange { get; set; }
        public virtual ControlInfoDetail IdControlDetailNavigation { get; set; } = null!;


    }
}
