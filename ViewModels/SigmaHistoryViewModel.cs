using Microsoft.EntityFrameworkCore;
using QC_Management.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.ComponentModel;
using System.Windows.Input;

namespace QC_Management.ViewModels
{
    public class SigmaHistoryViewModel : BaseViewModel
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

        public ICommand LoadCommand { get; }

        public ICommand ClearCommand { get; }

        public SigmaHistoryViewModel()
        {
            LoadCommand = new RelayCommand<object>(
                parameter => true,
                async parameter => await LoadAsync());

            ClearCommand = new RelayCommand<object>(
                parameter => true,
                parameter => ClearFilters());

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
        private async Task LoadAsync()
        {
            try
            {
                if (PeriodEnd < PeriodStart)
                {
                    MessageBox.Show(
                        "Ngày kết thúc phải lớn hơn hoặc bằng ngày bắt đầu.",
                        "Lịch sử Six Sigma",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    return;
                }

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
                SelectedMetric = null;

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
                    $"Không thể tải kết quả Sigma đã lưu: {ex.Message}",
                    "Lịch sử Six Sigma",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void ClearFilters()
        {
            SelectedDevice = null;
            SelectedTest = null;
            PeriodStart = new DateTime(
                DateTime.Today.Year,
                DateTime.Today.Month,
                1);
            PeriodEnd = DateTime.Today;

            Metrics.Clear();
            SelectedMetric = null;
            UpdateSummary();
        }

        private void UpdateSummary()
        {
            ExcellentCount = Metrics.Count(
                row => row.Metric.Sigma >= 5);

            GoodCount = Metrics.Count(
                row => row.Metric.Sigma >= 4 &&
                       row.Metric.Sigma < 5);

            BorderlineCount = Metrics.Count(
                row => row.Metric.Sigma >= 3 &&
                       row.Metric.Sigma < 4);

            FailedCount = Metrics.Count(
                row => row.Metric.Sigma < 3);

            SummaryText =
                $"Tổng: {Metrics.Count} | " +
                $"Xuất sắc: {ExcellentCount} | " +
                $"Tốt: {GoodCount} | " +
                $"Biên: {BorderlineCount} | " +
                $"Không đạt: {FailedCount}";
        }

        public Visibility SelectedPointVisibility =>
            SelectedMetric == null
                ? Visibility.Collapsed
                : Visibility.Visible;
    }
}