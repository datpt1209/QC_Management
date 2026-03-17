using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace QC_Management.Behaviors
{
    public static class DataGridColumnHelper
    {
        public static readonly DependencyProperty ColumnHeaderProperty =
            DependencyProperty.RegisterAttached(
                "ColumnHeader",
                typeof(string),
                typeof(DataGridColumnHelper),
                new PropertyMetadata(null));

        public static void SetColumnHeader(DependencyObject element, string value) => element.SetValue(ColumnHeaderProperty, value);
        public static string GetColumnHeader(DependencyObject element) => (string)element.GetValue(ColumnHeaderProperty);

        public static readonly DependencyProperty TargetVisibilityProperty =
            DependencyProperty.RegisterAttached(
                "TargetVisibility",
                typeof(Visibility),
                typeof(DataGridColumnHelper),
                new PropertyMetadata(Visibility.Visible, OnTargetVisibilityChanged));

        public static void SetTargetVisibility(DependencyObject element, Visibility value) => element.SetValue(TargetVisibilityProperty, value);
        public static Visibility GetTargetVisibility(DependencyObject element) => (Visibility)element.GetValue(TargetVisibilityProperty);

        private static void OnTargetVisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not DataGrid grid) return;

            if (grid.Columns == null || grid.Columns.Count == 0)
            {
                void onLoaded(object? s, RoutedEventArgs args)
                {
                    grid.Loaded -= onLoaded;
                    ApplyVisibility(grid, e);
                }
                grid.Loaded += onLoaded;
            }
            else
            {
                ApplyVisibility(grid, e);
            }
        }

        private static void ApplyVisibility(DataGrid grid, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                var header = GetColumnHeader(grid);
                if (string.IsNullOrEmpty(header)) return;

                var col = grid.Columns.FirstOrDefault(c => string.Equals(c.Header?.ToString(), header, StringComparison.OrdinalIgnoreCase));
                if (col == null) return;

                var vis = (Visibility)e.NewValue;
                if (vis == Visibility.Visible)
                {
                    col.Visibility = Visibility.Visible;
                    col.Width = new DataGridLength(1, DataGridLengthUnitType.Auto);
                }
                else
                {
                    col.Visibility = Visibility.Collapsed;
                    col.Width = new DataGridLength(0, DataGridLengthUnitType.Pixel);
                }
            }
            catch
            {
                // swallow UI errors
            }
        }
    }
}