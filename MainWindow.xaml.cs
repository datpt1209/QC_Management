using QC_Management.Models;
using QC_Management.ViewModels;
using System.Windows;
using System.Windows.Input;

namespace QC_Management
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
       
        public MainWindow()
        {
            
            InitializeComponent();
        }

        private void Tg_Btn_Checked(object sender, RoutedEventArgs e)
        {
            menu.Visibility = Visibility.Visible;
        }
        private void Tg_Btn_Unchecked(object sender, RoutedEventArgs e)
        {
            menu.Visibility = Visibility.Hidden;
        }
        private void ListViewItem_MouseEnter(object sender, MouseEventArgs e)
        {
            // Set tooltip visibility

            if (Tg_Btn.IsChecked == true)
            {
                tt_home.Visibility = Visibility.Collapsed;
                tt_input.Visibility = Visibility.Collapsed;
                tt_device.Visibility = Visibility.Collapsed;
                tt_qcInfor.Visibility = Visibility.Collapsed;
                tt_range.Visibility = Visibility.Collapsed;
                tt_test.Visibility = Visibility.Collapsed;
                tt_unit.Visibility = Visibility.Collapsed;
                tt_user.Visibility = Visibility.Collapsed;
                tt_user_role.Visibility = Visibility.Collapsed;

            }
            else
            {
                tt_home.Visibility = Visibility.Visible;
                tt_input.Visibility = Visibility.Visible;
                tt_device.Visibility = Visibility.Visible;
                tt_qcInfor.Visibility = Visibility.Visible;
                tt_range.Visibility = Visibility.Visible;
                tt_test.Visibility = Visibility.Visible;
                tt_unit.Visibility = Visibility.Visible;
                tt_user.Visibility = Visibility.Visible;
                tt_user_role.Visibility = Visibility.Visible;
            }
        }
    }
}
