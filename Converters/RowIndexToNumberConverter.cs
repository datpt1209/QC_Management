using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace QC_Management.Converters
{
    // Converts a DataGridRow instance to a 1-based row number string.
    public class RowIndexToNumberConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DataGridRow row)
            {
                // GetIndex returns the current row index in the DataGrid (accounts for virtualization)
                return (row.GetIndex() + 1).ToString(culture);
            }

            return "0";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}