using Microsoft.EntityFrameworkCore;
using QC_Management.Models;
using QC_Management.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

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

        public ICommand CalculateCommand { get; }

        public ICommand LoadSavedCommand { get; }

        public ICommand SaveCommand { get; }

        public ICommand RefreshCommand { get; }

        public SixSigmaViewModel()
        {
            CalculateCommand = new RelayCommand<object>(
                parameter => true,
                parameter => Calculate());

            LoadSavedCommand = new RelayCommand<object>(
                parameter => true,
                async parameter => await LoadSavedAsync());

            SaveCommand = new RelayCommand<object>(
                parameter => Metrics.Any(),
                async parameter => await SaveAsync());

            RefreshCommand = new RelayCommand<object>(
                parameter => true,
                async parameter => await LoadLookupsAsync());

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

                var tests = await db.Tests
                    .AsNoTracking()
                    .Where(test => test.TestType == 2)
                    .OrderBy(test => test.Index)
                    .ThenBy(test => test.Name)
                    .ToListAsync();

                Devices.Clear();

                foreach (var device in devices)
                    Devices.Add(device);

                Tests.Clear();

                foreach (var test in tests)
                    Tests.Add(test);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Không thể tải danh sách thiết bị và xét nghiệm: {ex.Message}",
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
                    .ToList();

                Metrics.Clear();

                var toExclusive = PeriodEnd.Date.AddDays(1);

                foreach (var deviceTest in deviceTests)
                {
                    var calculated = SigmaCalculator.Compute(
                        db,
                        deviceTest.IdDevice,
                        deviceTest.IdTest,
                        PeriodStart.Date,
                        toExclusive);

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
                    .OrderBy(metric => metric.IdDevice)
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
                SummaryText = "Không có dữ liệu Six Sigma trong khoảng thời gian đã chọn.";
                return;
            }

            var validCount = Metrics.Count(row => row.Metric != null && row.Metric.Sigma >= 0);
            var goodCount = Metrics.Count(row => row.Metric != null && row.Metric.Sigma >= 4);
            var failedCount = Metrics.Count(row => row.Metric.Sigma < 3);

            SummaryText =
                $"Tổng: {Metrics.Count} | " +
                $"Đạt từ 4 Sigma: {goodCount} | " +
                $"Dưới 3 Sigma: {failedCount} | " +
                $"Có dữ liệu hợp lệ: {validCount}";
        }
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
            $"{Metric.BiasPercent:0.##}%";

        public string CvText =>
            $"{Metric.CvPercent:0.##}%";

        public string SigmaText =>
            Metric.Sigma.ToString("0.##");

        public string QgiText =>
            Metric.Qgi?.ToString("0.##") ?? "-";

        public string PeriodText =>
            $"{Metric.PeriodStart:dd/MM/yyyy} - {Metric.PeriodEnd:dd/MM/yyyy}";

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