using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace QC_Management.Views
{
    /// <summary>
    /// Interaction logic for DeviceView.xaml
    /// </summary>
    public partial class DeviceView : UserControl
    {
        public DeviceView()
        {
            InitializeComponent();
        }
        private void ListView_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            // find the nearest enclosing ScrollViewer (the one wrapping the ListView)
            var origin = sender as DependencyObject;
            var scroll = FindParent<ScrollViewer>(origin);
            if (scroll != null)
            {
                // scroll amount: adjust divisor to taste (120 is standard Delta)
                scroll.ScrollToVerticalOffset(scroll.VerticalOffset - e.Delta / 3.0);
                e.Handled = true;
            }
        }

        private void InnerList_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var origin = sender as DependencyObject;
            var scroll = FindParent<ScrollViewer>(origin);
            if (scroll != null)
            {
                scroll.ScrollToVerticalOffset(scroll.VerticalOffset - e.Delta / 3.0);
                e.Handled = true;
            }
        }

        private static T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            if (child == null) return null;
            DependencyObject parent = VisualTreeHelper.GetParent(child);
            while (parent != null && !(parent is T))
            {
                parent = VisualTreeHelper.GetParent(parent);
            }
            return parent as T;
        }
    }
}
