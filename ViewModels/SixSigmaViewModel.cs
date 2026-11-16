using Microsoft.EntityFrameworkCore;
using QC_Management.Models;
using QC_Management.Services;
using QC_Management.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Text.Json;

namespace QC_Management.ViewModels
{
    public class SixSigmaViewModel : BaseViewModel
    {
        public ObservableCollection<Device> Devices { get; } = new();

        public ObservableCollection<Test> Tests { get; } = new();

        public ObservableCollection<SigmaMetricRow> Metrics { get; } = new();

        private Device? _selectedDevice;

        public Device? SelectedDevice
        {
            get => _selectedDevice;
            set
            {
                if (_selectedDevice == value)
                    return;

                _selectedDevice = value;
                OnPropertyChanged();

                SelectedTest = null;
                _ = LoadTestsForDeviceAsync(value?.Id);
            }
        }

        private Test? _selectedTest;

        public Test? SelectedTest
        {
            get => _selectedTest;
            set
            {
                if (_selectedTest == value)
                    return;

                _selectedTest = value;
                OnPropertyChanged();   
            }
        }

        private DateTime _periodStart = new(
            DateTime.Today.Year,
            DateTime.Today.Month,
            1);

        public DateTime PeriodStart
        {
            get => _periodStart;
            set
            {
                if (_periodStart == value)
                    return;

                _periodStart = value.Date;
                OnPropertyChanged();
            }
        }

        private DateTime _periodEnd = DateTime.Today;

        public DateTime PeriodEnd
        {
            get => _periodEnd;
            set
            {
                if (_periodEnd == value)
                    return;

                _periodEnd = value.Date;
                OnPropertyChanged();
            }
        }

        private string _summaryText = string.Empty;

        public string SummaryText
        {
            get => _summaryText;
            private set
            {
                if (_summaryText == value)
                    return;

                _summaryText = value;
                OnPropertyChanged();
            }
        }

        private int _excellentCount;
        public int ExcellentCount
        {
            get => _excellentCount;
            private set
            {
                if (_excellentCount == value)
                    return;

                _excellentCount = value;
                OnPropertyChanged();
            }
        }

        private int _goodCount;
        public int GoodCount
        {
            get => _goodCount;
            private set
            {
                if (_goodCount == value)
                    return;

                _goodCount = value;
                OnPropertyChanged();
            }
        }

        private int _borderlineCount;
        public int BorderlineCount
        {
            get => _borderlineCount;
            private set
            {
                if (_borderlineCount == value)
                    return;

                _borderlineCount = value;
                OnPropertyChanged();
            }
        }

        private int _failedCount;
        public int FailedCount
        {
            get => _failedCount;
            private set
            {
                if (_failedCount == value)
                    return;

                _failedCount = value;
                OnPropertyChanged();
            }
        }

        private SigmaMetricRow? _selectedMetric;

        public SigmaMetricRow? SelectedMetric
        {
            get => _selectedMetric;
            set
            {
                if (_selectedMetric == value)
                    return;

                _selectedMetric = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(SelectedPointVisibility));

                CommandManager.InvalidateRequerySuggested();
            }
        }

        public ICommand CalculateCommand { get; }

        public ICommand LoadSavedCommand { get; }

        public ICommand SaveCommand { get; }

        public ICommand RefreshCommand { get; }

        public ICommand ApplySuggestedRulesCommand { get; }

        public ICommand ViewReportCommand { get; }

        public SixSigmaViewModel()
        {
            CalculateCommand = new RelayCommand<object>(
                parameter => true,
                parameter => Calculate());
            ViewReportCommand = new RelayCommand<object>(
    parameter => Metrics.Any(),
    parameter => ViewReport());

            LoadSavedCommand = new RelayCommand<object>(
                parameter => true,
                async parameter => await LoadSavedAsync());

            SaveCommand = new RelayCommand<object>(
                parameter => Metrics.Any(),
                async parameter => await SaveAsync());

            RefreshCommand = new RelayCommand<object>(
                parameter => true,
                async parameter => await LoadLookupsAsync());

            ApplySuggestedRulesCommand = new RelayCommand<object>(
                parameter => SelectedMetric != null,
                parameter => ApplySuggestedRules());

            // Không truy cập database khi Visual Studio đang mở XAML Designer.
            if (!DesignerProperties.GetIsInDesignMode(new DependencyObject()))
                _ = LoadLookupsAsync();
        }

        private async Task LoadLookupsAsync()
        {
            try
            {
                using var db = new QcManagmentContext();

                var devices = await db.Devices
                    .AsNoTracking()
                    .OrderBy(device => device.Name)
                    .ToListAsync();

                var selectedDeviceId = SelectedDevice?.Id;

                Devices.Clear();

                foreach (var device in devices)
                    Devices.Add(device);

                var selectedDevice = Devices.FirstOrDefault(
                    device => device.Id == selectedDeviceId);

                if (!ReferenceEquals(_selectedDevice, selectedDevice))
                {
                    _selectedDevice = selectedDevice;
                    OnPropertyChanged(nameof(SelectedDevice));
                }

                await LoadTestsForDeviceAsync(selectedDevice?.Id);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Không thể tải danh sách thiết bị: {ex.Message}",
                    "Six Sigma",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void Calculate()
        {
            try
            {
                if (PeriodEnd < PeriodStart)
                {
                    MessageBox.Show(
                        "Ngày kết thúc phải lớn hơn hoặc bằng ngày bắt đầu.",
                        "Six Sigma",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    return;
                }

                using var db = new QcManagmentContext();

                var deviceTests = db.DeviceTests
                    .AsNoTracking()
                    .Where(mapping =>
                        (SelectedDevice == null ||
                         mapping.IdDevice == SelectedDevice.Id) &&
                        (SelectedTest == null ||
                         mapping.IdTest == SelectedTest.Id))
                    .Select(mapping => new
                    {
                        mapping.IdDevice,
                        mapping.IdTest
                    })
                    .Distinct()
                    .OrderBy(mapping => mapping.IdDevice)
                    .ThenBy(mapping => mapping.IdTest)
                    .ToList();

                Metrics.Clear();
                SelectedMetric = null;

                var toExclusive = PeriodEnd.Date.AddDays(1);

                foreach (var deviceTest in deviceTests)
                {
                    var calculated = SigmaCalculator.Compute(
                        db,
                        deviceTest.IdDevice,
                        deviceTest.IdTest,
                        PeriodStart.Date,
                        toExclusive)
                        .OrderBy(metric => metric.Sigma)
                        .ToList();

                    foreach (var metric in calculated)
                    {
                        var device = Devices.FirstOrDefault(
                            item => item.Id == metric.IdDevice);

                        var test = Tests.FirstOrDefault(
                            item => item.Id == metric.IdTest);

                        Metrics.Add(new SigmaMetricRow(
                            metric,
                            device?.Name ?? $"Device {metric.IdDevice}",
                            test?.Name ?? $"Test {metric.IdTest}"));
                    }
                }

                UpdateSummary();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Tính Six Sigma thất bại: {ex.Message}",
                    "Six Sigma",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private async Task LoadSavedAsync()
        {
            try
            {
                if (PeriodEnd < PeriodStart)
                    return;

                using var db = new QcManagmentContext();

                var query = db.SigmaMetrics
                    .AsNoTracking()
                    .Where(metric =>
                        metric.PeriodStart >= PeriodStart.Date &&
                        metric.PeriodEnd <= PeriodEnd.Date);

                if (SelectedDevice != null)
                {
                    query = query.Where(metric =>
                        metric.IdDevice == SelectedDevice.Id);
                }

                if (SelectedTest != null)
                {
                    query = query.Where(metric =>
                        metric.IdTest == SelectedTest.Id);
                }

                var savedMetrics = await query
                    .OrderBy(metric => metric.Sigma)
                    .ThenBy(metric => metric.IdDevice)
                    .ThenBy(metric => metric.IdTest)
                    .ThenBy(metric => metric.IdLevel)
                    .ToListAsync();

                Metrics.Clear();

                foreach (var metric in savedMetrics)
                {
                    var device = Devices.FirstOrDefault(
                        item => item.Id == metric.IdDevice);

                    var test = Tests.FirstOrDefault(
                        item => item.Id == metric.IdTest);

                    Metrics.Add(new SigmaMetricRow(
                        metric,
                        device?.Name ?? $"Device {metric.IdDevice}",
                        test?.Name ?? $"Test {metric.IdTest}"));
                }

                UpdateSummary();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Không thể tải kết quả Six Sigma: {ex.Message}",
                    "Six Sigma",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private async Task SaveAsync()
        {
            try
            {
                if (!Metrics.Any())
                    return;

                using var db = new QcManagmentContext();

                var metrics = Metrics
                    .Select(row => row.Metric)
                    .ToList();

                foreach (var metric in metrics)
                {
                    var oldMetrics = await db.SigmaMetrics
                        .Where(item =>
                            item.IdDevice == metric.IdDevice &&
                            item.IdTest == metric.IdTest &&
                            item.IdLevel == metric.IdLevel &&
                            item.PeriodStart == metric.PeriodStart &&
                            item.PeriodEnd == metric.PeriodEnd)
                        .ToListAsync();

                    if (oldMetrics.Any())
                        db.SigmaMetrics.RemoveRange(oldMetrics);
                }

                db.SigmaMetrics.AddRange(metrics);

                await db.SaveChangesAsync();

                MessageBox.Show(
                    $"Đã lưu {metrics.Count} kết quả Six Sigma.",
                    "Six Sigma",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Lưu kết quả Six Sigma thất bại: {ex.Message}",
                    "Six Sigma",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void UpdateSummary()
        {
            if (Metrics.Count == 0)
            {
                ExcellentCount = 0;
                GoodCount = 0;
                BorderlineCount = 0;
                FailedCount = 0;

                OnPropertyChanged(nameof(ExcellentCount));
                OnPropertyChanged(nameof(GoodCount));
                OnPropertyChanged(nameof(BorderlineCount));
                OnPropertyChanged(nameof(FailedCount));

                SummaryText =
                    "Không có dữ liệu Six Sigma trong khoảng thời gian đã chọn.";

                return;
            }

            ExcellentCount = Metrics.Count(row => row.Metric.Sigma >= 5);

            GoodCount = Metrics.Count(row =>
                row.Metric.Sigma >= 4 &&
                row.Metric.Sigma < 5);

            BorderlineCount = Metrics.Count(row =>
                row.Metric.Sigma >= 3 &&
                row.Metric.Sigma < 4);

            FailedCount = Metrics.Count(row =>
                row.Metric.Sigma < 3);

            var validCount = Metrics.Count(row =>
                row.Metric.Sigma >= 0);

            OnPropertyChanged(nameof(ExcellentCount));
            OnPropertyChanged(nameof(GoodCount));
            OnPropertyChanged(nameof(BorderlineCount));
            OnPropertyChanged(nameof(FailedCount));

            SummaryText =
                $"Tổng: {Metrics.Count} | " +
                $"Đạt từ 4 Sigma: {ExcellentCount + GoodCount} | " +
                $"Dưới 3 Sigma: {FailedCount} | " +
                $"Có dữ liệu hợp lệ: {validCount}";
        }

        private void ApplySuggestedRules()
        {
            if (SelectedMetric == null)
                return;

            var metric = SelectedMetric.Metric;
            var rules = SigmaCalculator.SuggestRules(metric.Sigma);

            var ruleText = string.Join(
                ", ",
                rules.Select(rule => rule.Replace("_", "-")));

            var confirmResult = MessageBox.Show(
                $"Áp dụng các Rule sau cho xét nghiệm " +
                $"{SelectedMetric.TestName} trên thiết bị " +
                $"{SelectedMetric.DeviceName}?\n\n{ruleText}",
                "Xác nhận áp dụng Rule",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (confirmResult != MessageBoxResult.Yes)
                return;

            try
            {
                using var db = new QcManagmentContext();

                var deviceTest = db.DeviceTests
                    .FirstOrDefault(item =>
                        item.IdDevice == metric.IdDevice &&
                        item.IdTest == metric.IdTest);

                if (deviceTest == null)
                {
                    MessageBox.Show(
                        "Không tìm thấy cấu hình thiết bị và xét nghiệm tương ứng.",
                        "Six Sigma",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    return;
                }

                // Lưu key chuẩn để LeveyJenningsChecker có thể đọc được.
                deviceTest.WestgardRulesJson =
                    JsonSerializer.Serialize(rules);

                db.SaveChanges();

                MessageBox.Show(
                    $"Đã áp dụng Rule cho {SelectedMetric.TestName}:\n{ruleText}",
                    "Six Sigma",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Không thể áp dụng Rule đề xuất: {ex.Message}",
                    "Six Sigma",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private async Task LoadTestsForDeviceAsync(int? deviceId)
        {
            Tests.Clear();
            SelectedTest = null;

            if (!deviceId.HasValue)
            {
                OnPropertyChanged(nameof(Tests));
                return;
            }

            try
            {
                using var db = new QcManagmentContext();

                var testIds = db.DeviceTests
                    .AsNoTracking()
                    .Where(mapping => mapping.IdDevice == deviceId.Value)
                    .Select(mapping => mapping.IdTest)
                    .Distinct();

                var tests = await db.Tests
                    .AsNoTracking()
                    .Where(test =>
                        test.TestType == 2 &&
                        testIds.Contains(test.Id))
                    .OrderBy(test => test.Index)
                    .ThenBy(test => test.Name)
                    .ToListAsync();

                foreach (var test in tests)
                    Tests.Add(test);

                OnPropertyChanged(nameof(Tests));
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Không thể tải danh sách xét nghiệm theo thiết bị: {ex.Message}",
                    "Six Sigma",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void ViewReport()
        {
            if (!Metrics.Any())
            {
                MessageBox.Show(
                    "Chưa có dữ liệu Sigma để lập báo cáo.",
                    "Báo cáo Six Sigma",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            var reportItems = Metrics
                .Select(row => new SigmaReportItem
                {
                    DeviceName = row.DeviceName,
                    TestName = row.TestName,
                    Level = row.LevelText,
                    Tea = row.TeaText,
                    Bias = row.BiasText,
                    Cv = row.CvText,
                    Sigma = row.SigmaText,
                    Qgi = row.QgiText,
                    Grade = row.Metric.Grade ?? "-",
                    SavedDate = row.ComputedAtText,
                    Period = row.PeriodText,
                    QcRules = row.SuggestedRulesText
                })
                .ToList();

            var reportWindow = new SigmaReportWindow(
                reportItems,
                SelectedDevice?.Name ?? "Tất cả thiết bị",
                PeriodStart,
                PeriodEnd);

            reportWindow.Owner = Application.Current.Windows
                .OfType<Window>()
                .FirstOrDefault(window => window.IsActive);

            reportWindow.ShowDialog();
        }

        public Visibility SelectedPointVisibility =>
            SelectedMetric == null
                ? Visibility.Collapsed
                : Visibility.Visible;
    }

    public sealed class SigmaMetricRow
    {
        public SigmaMetric Metric { get; }

        public string DeviceName { get; }

        public string TestName { get; }

        public string LevelText =>
            Metric.IdLevel.HasValue
                ? Metric.IdLevel.Value.ToString()
                : "Tất cả";

        public string TeaText =>
            $"{Metric.TEaPercent:0.##}%";

        public string BiasText =>
            $"{Metric.BiasPercent:0.##}% ({Metric.BiasN})";

        public string CvText =>
            $"{Metric.CvPercent:0.##}% ({Metric.CvN})";

        public string SigmaText =>
            Metric.Sigma.ToString("0.##");

        public string QgiText =>
            Metric.Qgi?.ToString("0.##") ?? "-";

        public string PeriodText =>
            $"{Metric.PeriodStart:dd/MM/yyyy} - {Metric.PeriodEnd:dd/MM/yyyy}";

        public Brush SigmaBrush =>
            Metric.Sigma >= 5
                ? new SolidColorBrush(Color.FromRgb(76, 142, 52))
                : Metric.Sigma >= 4
                    ? new SolidColorBrush(Color.FromRgb(25, 118, 210))
                    : Metric.Sigma >= 3
                        ? new SolidColorBrush(Color.FromRgb(245, 166, 35))
                        : new SolidColorBrush(Color.FromRgb(214, 40, 40));

        public double SigmaBarWidth =>
            Math.Clamp(Metric.Sigma / 6.0 * 100.0, 0, 100);

        public Brush GradeBackground =>
            Metric.Sigma >= 5
                ? new SolidColorBrush(Color.FromRgb(220, 239, 209))
                : Metric.Sigma >= 4
                    ? new SolidColorBrush(Color.FromRgb(190, 224, 255))
                    : Metric.Sigma >= 3
                        ? new SolidColorBrush(Color.FromRgb(255, 224, 178))
                        : new SolidColorBrush(Color.FromRgb(255, 205, 205));

        public Brush GradeForeground =>
            Metric.Sigma >= 5
                ? new SolidColorBrush(Color.FromRgb(46, 125, 50))
                : Metric.Sigma >= 4
                    ? new SolidColorBrush(Color.FromRgb(21, 101, 192))
                    : Metric.Sigma >= 3
                        ? new SolidColorBrush(Color.FromRgb(164, 91, 0))
                        : new SolidColorBrush(Color.FromRgb(190, 30, 30));

        public string DetailTitle =>
            $"Chi tiết: {TestName} (Level {LevelText})";

        public string DetailBiasText =>
            $"Bias = ({Metric.BiasPercent:0.##}) " +
            $"({Metric.BiasN} đợt ngoại kiểm)";

        public string DetailSigmaText =>
            $"Sigma = (TEa - Bias) / CV = " +
            $"({Metric.TEaPercent:0.##} - {Metric.BiasPercent:0.##}) " +
            $"/ {Metric.CvPercent:0.##} = {Metric.Sigma:0.##}";

        public string DetailQgiText =>
            Metric.Qgi.HasValue
                ? $"QGI = Bias / (1.5 × CV) = {Metric.Qgi.Value:0.##}"
                : "QGI = chưa có dữ liệu";

        public string DetailGradeText =>
            $"Ưu tiên giám sát: {Metric.Grade ?? "-"}";

        public string SuggestedRulesText =>
            string.Join(
                "   ",
                SigmaCalculator
                    .SuggestRules(Metric.Sigma)
                    .Select(rule => rule.Replace("_", "-")));

        public double BiasChartX
        {
            get
            {
                if (Metric.TEaPercent <= 0)
                    return 45;

                var biasRatio = Metric.BiasPercent / Metric.TEaPercent * 100;

                // Biểu đồ: trục X từ 45 đến 300
                return 45 + Math.Clamp(biasRatio, 0, 100) * 2.55;
            }
        }

        public double CvChartY
        {
            get
            {
                if (Metric.TEaPercent <= 0)
                    return 205;

                var cvRatio = Metric.CvPercent / Metric.TEaPercent * 100;

                // Biểu đồ: trục Y từ 25 đến 205
                return 205 - Math.Clamp(cvRatio, 0, 100) * 1.8;
            }
        }

        public double SelectedBiasChartX =>
            BiasChartX - 4;

        public double SelectedCvChartY =>
            CvChartY - 4;

        public string ComputedAtText =>
            Metric.ComputedAt == default
                ? "-"
                : Metric.ComputedAt.ToString("dd/MM/yyyy HH:mm");

        public SigmaMetricRow(
            SigmaMetric metric,
            string deviceName,
            string testName)
        {
            Metric = metric;
            DeviceName = deviceName;
            TestName = testName;
        }
    }
}