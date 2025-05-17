
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace QC_Management.Converters
{
    public class ResultTypeToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string selectedResultType && parameter is string targetResultType)
            {
                return selectedResultType == targetResultType ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
   