using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QC_Management.Models
{
    public class ReResultGroup
    {
        public string DeviceName { get; set; } = string.Empty;
        public int IdDevice { get; set; }
        public string LevelName { get; set; } = string.Empty;
        public int IdLevel { get; set; }
        public int Index { get; set; }
        public DateTime DateTime { get; set; }
        public ObservableCollection<ReResult> Results { get; set; } = new ObservableCollection<ReResult>();
    }
}