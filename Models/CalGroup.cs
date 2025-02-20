using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QC_Management.Models
{
    public class CalGroup:BaseViewModel
    {
        public string DeviceName { get; set; } = string.Empty;
        public int Index { get; set; }
        public DateTime DateRun { get; set; }
        public TimeSpan Time { get; set; }
        public ObservableCollection<ReCalResult> ReCalResults { get; set; } = new ObservableCollection<ReCalResult>();
    }
}
