using System;
using System.Globalization;
using System.Windows.Data;

namespace QC_Management.Converters
{
    public class TestTypeToResultConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] is int testType)
            {
                if (testType == 2)
                {
                    return values[1].ToString(); // Result
                }
                else if (testType == 1)
                {
                    return values[2]; // QualitativeResult
                }
            }
            return null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

