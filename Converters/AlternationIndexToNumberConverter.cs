using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace QC_Management.Converters
{
    public class AlternationIndexToNumberConverter : IMultiValueConverter
    {
        // values[0] = DataGridRow, values[1] = DataGrid
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values?.Length >= 2 &&
                values[0] is DataGridRow row &&
                values[1] is DataGrid dataGrid)
            {
                var item = row.Item;
                // Use DataGrid.Items.IndexOf to get the actual index in the grid (works regardless of ItemsSource concrete type)
                int index = dataGrid.Items.IndexOf(item);
                if (index >= 0)
                    return (index + 1).ToString(culture); // 1-based
            }

            return "0";
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}