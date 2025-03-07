using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace QC_Management.Converters
{
    public class TextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //convert the int to a string:
            if (value != null)
                return value.ToString();
            else
            {
                return string.Empty;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //convert the string back to an int here
            double result;
            if (value != null)
            {
                if (value == "")
                    return null;
                else if (double.TryParse(value.ToString(), out result))
                {
                    return result;
                }
                else
                    return MessageBox.Show("Nhập sai định dạnh số, vui lòng nhập lại");

            }
            else
            {
                return string.Empty;
            }

        }
    }
}