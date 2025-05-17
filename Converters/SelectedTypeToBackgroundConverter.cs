using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace QC_Management.Converters
{
    public class SelectedTypeToBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var type = value as string;
            return type switch
            {
                "QC" => Brushes.LightGreen,
                "CALIB" => Brushes.LightGoldenrodYellow,
                _ => Brushes.AntiqueWhite
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
