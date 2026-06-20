using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace QC_Management.Helpers
{
    /// <summary>
    /// Attached property cho phép bind Width của DataGridColumn từ XAML.
    /// Dùng vì DataGridColumn.Width không phải DependencyProperty.
    /// 
    /// Cách dùng trong XAML:
    ///   helpers:DataGridColumnWidthHelper.BindableWidth="{Binding ShowQualitative,
    ///       Converter={StaticResource VisibilityToColumnWidthConverter}}"
    /// </summary>
    public static class DataGridColumnWidthHelper
    {
        public static readonly DependencyProperty BindableWidthProperty =
            DependencyProperty.RegisterAttached(
                "BindableWidth",
                typeof(DataGridLength),
                typeof(DataGridColumnWidthHelper),
                new PropertyMetadata(DataGridLength.Auto, OnBindableWidthChanged));

        public static DataGridLength GetBindableWidth(DependencyObject obj)
            => (DataGridLength)obj.GetValue(BindableWidthProperty);

        public static void SetBindableWidth(DependencyObject obj, DataGridLength value)
            => obj.SetValue(BindableWidthProperty, value);

        private static void OnBindableWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DataGridColumn col)
                col.Width = (DataGridLength)e.NewValue;
        }
    }

    /// <summary>
    /// Chuyển Visibility → DataGridLength:
    ///   Visible   → DataGridLength(1, Star)   — cột co dãn bình thường
    ///   Collapsed → DataGridLength(0)          — cột ẩn hoàn toàn (width = 0)
    /// </summary>
    public class VisibilityToColumnWidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is Visibility v && v == Visibility.Visible
                ? new DataGridLength(1, DataGridLengthUnitType.Star)
                : new DataGridLength(0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
