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
                // Tính toán phần chiều cao bị chiếm bởi các control khác (header, margin, padding...)
                double headerHeight = 130; // ví dụ: chiều cao header hoặc các control phía trên
                double margin = 20;       // ví dụ: tổng margin trên/dưới
                double availableHeight = e.NewSize.Height - headerHeight - margin;

                // Chia đều cho 3 biểu đồ   
                vm.ChartHeight = availableHeight / 3;
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
    }
}
