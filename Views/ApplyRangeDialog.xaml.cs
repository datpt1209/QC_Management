using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Globalization;

namespace QC_Management.Views
{
    /// <summary>
    /// Interaction logic for ApplyRangeDialog.xaml
    /// </summary>
    public partial class ApplyRangeDialog : Window
    {
        public DateTime Start { get; private set; }
        public DateTime End { get; private set; }

        public ApplyRangeDialog()
        {
            InitializeComponent();

            // Set defaults
            StartDatePicker.SelectedDate = DateTime.Today;
            EndDatePicker.SelectedDate = DateTime.Today;
            StartTimeText.Text = "00:00";
            EndTimeText.Text = "23:59";
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            if (!StartDatePicker.SelectedDate.HasValue || !EndDatePicker.SelectedDate.HasValue)
            {
                MessageBox.Show("Vui lòng chọn ngày bắt đầu và ngày kết thúc.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var startDate = StartDatePicker.SelectedDate.Value.Date;
            var endDate = EndDatePicker.SelectedDate.Value.Date;

            if (!TimeSpanTryParse(StartTimeText.Text.Trim(), out var startTime))
            {
                MessageBox.Show("Giờ bắt đầu không đúng định dạng. Vui lòng dùng HH:mm (ví dụ 08:30).", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!TimeSpanTryParse(EndTimeText.Text.Trim(), out var endTime))
            {
                MessageBox.Show("Giờ kết thúc không đúng định dạng. Vui lòng dùng HH:mm (ví dụ 17:00).", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Start = startDate.Add(startTime);
            End = endDate.Add(endTime);

            if (Start > End)
            {
                MessageBox.Show("Ngày/Giờ bắt đầu phải nhỏ hơn hoặc bằng ngày/giờ kết thúc.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DialogResult = true;
            Close();
        }

        private bool TimeSpanTryParse(string text, out TimeSpan ts)
        {
            ts = TimeSpan.Zero;
            if (TimeSpan.TryParseExact(text, "hh\\:mm", CultureInfo.InvariantCulture, out ts)) return true;
            if (TimeSpan.TryParseExact(text, "h\\:mm", CultureInfo.InvariantCulture, out ts)) return true;
            // Allow parse via DateTime fallback
            if (DateTime.TryParseExact(text, "HH:mm", CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var dt))
            {
                ts = dt.TimeOfDay;
                return true;
            }
            return false;
        }
    }
}
