using System.Windows.Controls;
using QC_Management.ViewModels;

namespace QC_Management.Views
{
    public partial class EqaManagementView : UserControl
    {
        public EqaManagementView()
        {
            InitializeComponent();
            DataContext = new EqaManagementViewModel();
        }
    }
}