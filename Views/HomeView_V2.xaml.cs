using LiveCharts;
using LiveCharts.Configurations;
using LiveCharts.Dtos;
using QC_Management.Models;
using QC_Management.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace QC_Management.Views
{
    /// <summary>
    /// Interaction logic for HomeView.xaml
    /// </summary>
    public partial class HomeView_V2 : UserControl
    {
        public HomeView_V2()
        {
            InitializeComponent();
        }

        private void HomeView_SizeChanged(object sender, System.Windows.SizeChangedEventArgs e)
        {
            var vm = DataContext as HomeViewModel_V2;
            if (vm != null)
            {
                double headerHeight = 130;
                double margin = 20;
                double availableHeight = e.NewSize.Height - headerHeight - margin;
                vm.ChartHeight = availableHeight / 3;

                // Cập nhật chiều rộng vùng chart để ViewModel tính SD line length đúng.
                // Trừ: cột test list (130) + panel thống kê (130) + margins (~30)
                double chartAreaWidth = e.NewSize.Width - 130 - 130 - 30;
                if (chartAreaWidth > 0)
                    vm.ChartAreaWidth = chartAreaWidth;
            }
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

        // New: route mouse wheel on charts to the outer vertical ScrollViewer so vertical wheel scrolls the charts area.
        private void Chart_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            // Prefer the named outer ScrollViewer if available
            var outer = ChartsScrollViewer;
            if (outer == null)
            {
                // fallback: find nearest enclosing ScrollViewer
                outer = FindParent<ScrollViewer>(sender as DependencyObject);
            }

            if (outer != null)
            {
                outer.ScrollToVerticalOffset(outer.VerticalOffset - e.Delta / 3.0);
                e.Handled = true;
            }
        }
    }
}
