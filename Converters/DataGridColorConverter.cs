using QC_Management.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace QC_Management.Converters
{
    public class DataGridColorConverter: IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            /*        double result = (double)value;
                    double mean = (double)parameter;
                    double sd = (double)parameter;

                    double lowerLimit = mean - 2 * sd;
                    double upperLimit = mean + 2 * sd;

                    if (result < lowerLimit || result > upperLimit)
                    {
                        return Brushes.Red;
                    }
                    else
                    {
                        return Brushes.Black;
                    }*/
            double result;
            double mean = (double)parameter;
            double sd = (double)parameter;
            if (double.TryParse((string?)value, out result))
            {
                double lowerLimit = mean - 2 * sd;
                double upperLimit = mean + 2 * sd;
                if (result < lowerLimit || result > upperLimit)
                {
                    return Brushes.Red;
                }
                else
                {
                    return Brushes.Black;
                }
            }
            else
            {
                MessageBox.Show("Số vừa nhập không đúng định dạng, Vui lòng nhập lại");
                return null;
            }


        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
