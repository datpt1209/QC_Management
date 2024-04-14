namespace QC_Management.Models
{
    public class ResultView : BaseViewModel
    {
        public int? id { get; set; }
        public string TestName { get; set; }
        public string? QCName { get; set; }
        public double? Result { get; set; }
        public string? LOT { get; set; }
        public int idTest { get; set; }
        public double? Mean { get; set; }
        public double? Sd { get; set; }
        public double? Min { get; set; }
        public double? Max { get; set; }
        public virtual ControlInfoDetail IdControlDetailNavigation { get; set; } = null!;


    }
}
