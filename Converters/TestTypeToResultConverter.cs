using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Forms;

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
            if (value is string result && !string.IsNullOrWhiteSpace(result))
            {
                if (double.TryParse(result, out double number))
                {
                    // If the string can be parsed to a number, return 2, number, null
                    return new object[] { 2, number, null };
                }
                else
                {
                    // If the string cannot be parsed to a number, return 1, null, result
                    return new object[] { 1, null, result };
                }
            }
            else
            {
                return new object[] { null, null, null };
            }

        }
    }
}

