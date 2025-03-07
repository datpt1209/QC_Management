using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace QC_Management.Converters
{
    public class TestTypeTextConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] == null || values[1] == null)
                return string.Empty;

            var result = values[0].ToString();
            var testType = values[1].ToString();

            if (testType == "1") // Qualitative
            {
                return result;
            }
            else if (testType == "2") // Quantitative
            {
                if (double.TryParse(result, out double numericResult))
                {
                    return numericResult;
                }
                else
                {
                    return DependencyProperty.UnsetValue;
                }
            }

            return string.Empty;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            var result = value?.ToString();
            var testType = targetTypes[1].Name;

            if (testType == "String")
            {
                return new object[] { result, "Qualitative" };
            }
            else if (testType == "Double")
            {
                if (double.TryParse(result, out double numericResult))
                {
                    return new object[] { numericResult, "Quantitative" };
                }
                else
                {
                    MessageBox.Show("Nhập sai định dạng số, vui lòng nhập lại", "Lỗi định dạng", MessageBoxButton.OK, MessageBoxImage.Error);
                    return new object[] { DependencyProperty.UnsetValue, "Quantitative" };
                }
            }

            return new object[] { string.Empty, testType };
        }
    }
}
