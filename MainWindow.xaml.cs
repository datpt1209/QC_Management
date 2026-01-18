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

            // Drawer closed by default
            if (NavDrawer != null)
                NavDrawer.IsLeftDrawerOpen = false;

            if (MenuToggleButton != null)
                MenuToggleButton.IsChecked = false;
        }

        private void MenuClose_Click(object sender, RoutedEventArgs e)
        {
            if (NavDrawer != null) NavDrawer.IsLeftDrawerOpen = false;
            if (MenuToggleButton != null) MenuToggleButton.IsChecked = false;
        }

        // Close drawer when user selects a menu item.
        // SelectionChanged fires when user selects (clicks) a ListViewItem.
        private void LV_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            // If drawer is modal (small screens) or user wants it to auto-close, close it.
            if (NavDrawer != null)
            {
                NavDrawer.IsLeftDrawerOpen = false;
            }
            if (MenuToggleButton != null)
            {
                MenuToggleButton.IsChecked = false;
            }

            // Clear the ListView selection so the same item can be selected again later.
            // Use the sender where possible to avoid null reference to named LV.
            try
            {
                if (sender is System.Windows.Controls.ListView lv)
                {
                    lv.SelectedItem = null;
                    lv.SelectedIndex = -1;
                }
                else if (LV != null)
                {
                    LV.SelectedItem = null;
                    LV.SelectedIndex = -1;
                }
            }
            catch
            {
                // non-fatal - ignore clearing failures
            }
        }

        private void ListViewItem_MouseEnter(object sender, MouseEventArgs e)
        {
            var drawerOpen = NavDrawer != null && NavDrawer.IsLeftDrawerOpen;
            var setVisibility = drawerOpen ? Visibility.Collapsed : Visibility.Visible;

            if (tt_home != null) tt_home.Visibility = setVisibility;
            if (tt_input != null) tt_input.Visibility = setVisibility;
            if (tt_device != null) tt_device.Visibility = setVisibility;
            if (tt_qcInfor != null) tt_qcInfor.Visibility = setVisibility;
            if (tt_range != null) tt_range.Visibility = setVisibility;
            if (tt_test != null) tt_test.Visibility = setVisibility;
            if (tt_unit != null) tt_unit.Visibility = setVisibility;
            if (tt_user != null) tt_user.Visibility = setVisibility;
            if (tt_user_role != null) tt_user_role.Visibility = setVisibility;
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);

            MessageBoxResult result = MessageBox.Show("Do you really want to close the application?", "Confirmation", MessageBoxButton.OKCancel, MessageBoxImage.Question);
            if (result != MessageBoxResult.OK)
            {
                e.Cancel = true;
            }
        }
    }
}
