using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using QC_Management.ViewModels;

namespace QC_Management.Views
{
    /// <summary>
    /// Interaction logic for IncidentManagementView.xaml
    /// </summary>
    public partial class IncidentManagementView : UserControl
    {
        public IncidentManagementView()
        {
            InitializeComponent();
        }
        private void DataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            // Use GetIndex() so index stays correct with virtualization/sorting/filtering
            e.Row.Header = (e.Row.GetIndex() + 1).ToString();
        }

        private void DataGrid_UnloadingRow(object sender, DataGridRowEventArgs e)
        {
            // optional cleanup
            e.Row.Header = null;
        }

    }
}
