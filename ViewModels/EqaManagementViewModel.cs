using Microsoft.EntityFrameworkCore;
using QC_Management.Models;
using QC_Management.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace QC_Management.ViewModels
{
    public class EqaManagementViewModel : BaseViewModel
    {
        public ObservableCollection<string> Programs { get; } = new();
        public ObservableCollection<int> Years { get; } = new();
        public ObservableCollection<string> Groups { get; } = new();
        public ObservableCollection<Device> Devices { get; } = new();
        public ObservableCollection<Test> TestList { get; } = new();

        private Test _selectedTest;
        public Test SelectedTest { get => _selectedTest; set { _selectedTest = value; OnPropertyChanged(); LoadDataCommand.Execute(null); } }

        public ObservableCollection<double> ChartPoints { get; } = new();
        public ObservableCollection<ExternalResultRow> ExternalResults { get; } = new();

        public ICommand LoadDataCommand { get; }
        public ICommand NewResultCommand { get; }
        public ICommand ExportCommand { get; }

        // filters
        public string SelectedProgram { get; set; }
        public int SelectedYear { get; set; } = DateTime.Now.Year;
        public string SelectedGroup { get; set; }
        public Device SelectedDevice { get; set; }
        public string TestSearch { get; set; }
        public string SearchText { get; set; }

        // summary
        public int SummaryTotal { get; set; }
        public string SummaryPassedText { get; set; }
        public string SummaryReviewText { get; set; }
        public string SummaryFailedText { get; set; }

        public EqaManagementViewModel()
        {
            LoadDataCommand = new RelayCommand<object>(_ => true, async _ => await LoadAsync());
            NewResultCommand = new RelayCommand<object>(_ => true, _ => MessageBox.Show("Open add external result dialog (implement)"));
            ExportCommand = new RelayCommand<object>(_ => ExternalResults.Count > 0, _ => MessageBox.Show("Export (implement)"));

            // fill simple lookup values
            Programs.Add("RIQAS");
            Programs.Add("Other");
            for (int y = DateTime.Now.Year; y >= DateTime.Now.Year - 5; y--) Years.Add(y);
            Groups.Add("Tất cả");

            // load devices/tests minimal
            TryPopulateDeviceAndTests();

            // initial load
            _ = LoadAsync();
        }

        private void TryPopulateDeviceAndTests()
        {
            try
            {
                using var db = new QcManagmentContext();
                var devs = db.Devices.AsNoTracking().OrderBy(d => d.Name).ToList();
                Devices.Clear();
                foreach (var d in devs) Devices.Add(d);

                var tests = db.Tests.AsNoTracking().OrderBy(t => t.Name).ToList();
                TestList.Clear();
                foreach (var t in tests) TestList.Add(t);
            }
            catch
            {
                // ignore
            }
        }

        private async Task LoadAsync()
        {
            try
            {
                ExternalResults.Clear();
                ChartPoints.Clear();

                // Example: load recent Results for selected test if present, otherwise show recent Results overall for year
                using var db = new QcManagmentContext();
                var start = new DateTime(SelectedYear, 1, 1);
                var end = start.AddYears(1);

                var q = db.Results
                    .AsNoTracking()
                    .Include(r => r.IdTestNavigation)
                    .Include(r => r.IdControlDetailNavigation)
                    .Where(r => r.DateRun >= start && r.DateRun < end);

                if (SelectedTest != null) q = q.Where(r => r.IdTest == SelectedTest.Id);
                if (SelectedDevice != null) q = q.Where(r => r.IdDevice == SelectedDevice.Id);

                var list = await q.OrderByDescending(r => r.DateRun).Take(200).ToListAsync();

                foreach (var r in list)
                {
                    ExternalResults.Add(new ExternalResultRow
                    {
                        Batch = r.IndexQc?.ToString() ?? "-",
                        Date = r.DateRun,
                        Level = r.IdLevelNavigation?.Name ?? (r.IdLevel != 0 ? r.IdLevel.ToString() : "-"),
                        LabValue = r.Result1?.ToString("0.###") ?? r.TempResult,
                        Unit = r.IdTestNavigation?.IdUnitTableNavigation?.Name ?? "",
                        Target = r.IdControlDetailNavigation != null ? (r.IdControlDetailNavigation.CurMean?.ToString("0.###") ?? "") : "",
                        SDI = r.ZScore?.ToString("0.###") ?? "",
                        BiasPercent = "", // compute if you have target
                        Evaluation = r.IsOutRange == true ? "Không đạt" : (string.IsNullOrWhiteSpace(r.WestgardRule) ? "Đạt" : "Cần xem xét"),
                        Note = r.Comment
                    });

                    // simple chart point from ZScore
                    if (r.ZScore.HasValue)
                        ChartPoints.Add((double)r.ZScore.Value);
                    else if (double.TryParse(r.TempResult, out var v))
                        ChartPoints.Add(v);
                }

                // compute summary counts
                SummaryTotal = ExternalResults.Count;
                var passed = ExternalResults.Count(x => x.Evaluation == "Đạt");
                var review = ExternalResults.Count(x => x.Evaluation == "Cần xem xét");
                var failed = ExternalResults.Count(x => x.Evaluation == "Không đạt");
                SummaryPassedText = $"{passed} ({(SummaryTotal == 0 ? 0 : Math.Round(100.0 * passed / SummaryTotal, 1))}%)";
                SummaryReviewText = $"{review} ({(SummaryTotal == 0 ? 0 : Math.Round(100.0 * review / SummaryTotal, 1))}%)";
                SummaryFailedText = $"{failed} ({(SummaryTotal == 0 ? 0 : Math.Round(100.0 * failed / SummaryTotal, 1))}%)";

                // notify UI
                OnPropertyChanged(nameof(ExternalResults));
                OnPropertyChanged(nameof(ChartPoints));
                OnPropertyChanged(nameof(SummaryTotal));
                OnPropertyChanged(nameof(SummaryPassedText));
                OnPropertyChanged(nameof(SummaryReviewText));
                OnPropertyChanged(nameof(SummaryFailedText));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Load EQA data failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public class ExternalResultRow
        {
            public string Batch { get; set; }
            public DateTime Date { get; set; }
            public string Level { get; set; }
            public string LabValue { get; set; }
            public string Unit { get; set; }
            public string Target { get; set; }
            public string SDI { get; set; }
            public string BiasPercent { get; set; }
            public string Evaluation { get; set; }
            public string Note { get; set; }
        }
    }
}