using System.Collections.ObjectModel;

namespace QC_Management.Models
{
    public class DataProvider : BaseViewModel
    {
        private static DataProvider _ins;
        public static DataProvider Ins { get { if (_ins == null) _ins = new DataProvider(); return _ins; } set { _ins = value; } }

        public QcManagmentContext DB { get; set; }

        private DataProvider()
        {
            DB = new QcManagmentContext();
        }

    }
}
