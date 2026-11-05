using LiveCharts;
using LiveCharts.Configurations;
using LiveCharts.Wpf;
using Microsoft.EntityFrameworkCore;
using QC_Management.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace QC_Management.ViewModels
{
    public class ExternalResultChartViewModel : BaseViewModel
    {
        private QcManagmentContext _db = new();

        public ObservableCollection<ExternalProgram> Programs { get; } = new();
        public ObservableCollection<Device> Devices { get; } = new();

        private ExternalProgram _selectedProgram;
        public ExternalProgram SelectedProgram { get => _selectedProgram; set { _selectedProgram = value; OnPropertyChanged(); } }

        private Device _selectedDevice;
        public Device SelectedDevice { get => _selectedDevice; set { _selectedDevice = value; OnPropertyChanged(); } }

        private DateTime _startDate = DateTime.Now.AddDays(-30);
        public DateTime StartDate { get => _startDate; set { _startDate = value; OnPropertyChanged(); } }

        private DateTime _endDate = DateTime.Now;
        public DateTime EndDate { get => _endDate; set { _endDate = value; OnPropertyChanged(); } }

        public SeriesCollection SeriesCollection { get; set; } = new SeriesCollection();
        public ObservableCollection<string> Labels { get; set; } = new ObservableCollection<string>();
        public Func<double, string> YAxisLabelFormatter { get; set; } = v => v.ToString("0.###");

        public ICommand LoadCommand { get; }
        public ICommand RefreshCommand { get; }

        public ExternalResultChartViewModel()
        {
            // Register mapper for ExternalResult once
            var mapper = Mappers.Xy<ExternalResult>()
                .X((value, index) => value.Id) // use Id as X position (discrete)
                .Y(v => ClampZScore(v.ZScore))
                .Fill((v, _) => v.IsDefect == true ? Brushes.Red : Brushes.Transparent)
                .Stroke((v, _) => v.IsDefect == true ? Brushes.Red : Brushes.DimGray);

            Charting.For<ExternalResult>(mapper, SeriesOrientation.Horizontal);

            LoadCommand = new RelayCommand<object>((p) => true, async (p) => await LoadAndRenderAsync());
            RefreshCommand = new RelayCommand<object>((p) => true, async (p) => await LoadLookupsAsync());

            // initial load of lookups
            _ = LoadLookupsAsync();
        }

        private static double ClampZScore(double? z)
        {
            if (!z.HasValue) return 0;
            if (z.Value > 4) return 4;
            if (z.Value < -4) return -4;
            return z.Value;
        }

        public async Task LoadLookupsAsync()
        {
            try
            {
                var progs = await _db.ExternalPrograms.AsNoTracking().Where(p => p.IsActive).OrderByDescending(p => p.Year).ThenBy(p => p.Name).ToListAsync();
                Programs.Clear();
                foreach (var p in progs) Programs.Add(p);

                var devs = await _db.Devices.AsNoTracking().OrderBy(d => d.Name).ToListAsync();
                Devices.Clear();
                foreach (var d in devs) Devices.Add(d);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Load lookups failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public async Task LoadAndRenderAsync()
        {
            try
            {
                var q = _db.ExternalResults.AsNoTracking()
                    .Include(er => er.IdTestNavigation)
                    .Include(er => er.IdDeviceNavigation)
                    .Include(er => er.ExternalProgram)
                    .Where(er => er.DateRun >= StartDate && er.DateRun <= EndDate.AddDays(1));

                if (SelectedProgram != null) q = q.Where(er => er.ExternalProgramId == SelectedProgram.Id);
                if (SelectedDevice != null) q = q.Where(er => er.IdDevice == SelectedDevice.Id);

                var list = await q.OrderBy(er => er.DateRun).ThenBy(er => er.Id).ToListAsync();

                // Evaluate ZScore/IsDefect on each model (in-memory)
                foreach (var er in list)
                {
                    try
                    {
                        er.ApplyReferenceEvaluation();
                    }
                    catch
                    {
                        // ignore per-item evaluation errors
                    }
                }

                RenderChart(list);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Load results failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RenderChart(System.Collections.Generic.List<ExternalResult> list)
        {
            SeriesCollection.Clear();
            Labels.Clear();

            if (list == null || list.Count == 0)
            {
                // empty series to avoid null binding
                SeriesCollection.Add(new ScatterSeries { Title = "No data", Values = new ChartValues<ExternalResult>() });
                return;
            }

            // Build ChartValues (type-mapped)
            var values = new ChartValues<ExternalResult>();
            int idx = 0;
            foreach (var r in list)
            {
                // set ChartIndex-like property via Id used in mapper; Id already set
                values.Add(r);
                Labels.Add($"{r.DateRun:MM-dd}\n{r.IdTestNavigation?.Name ?? r.IdTest.ToString()}");
                idx++;
            }

            var scatter = new ScatterSeries
            {
                Title = "External results",
                Values = values,
                MinPointShapeDiameter = 8,
                MaxPointShapeDiameter = 8
            };

            SeriesCollection.Add(scatter);

            // Add optional mean line if many quantitative values exist
            var quantitative = list.Where(r => r.ZScore.HasValue).ToList();
            if (quantitative.Any())
            {
                var mean = quantitative.Average(r => r.ZScore!.Value);
                var meanValues = new ChartValues<double>(quantitative.Select(r => ClampZScore(mean)));
                var line = new LineSeries
                {
                    Title = "Mean (Z)",
                    Values = meanValues,
                    Stroke = Brushes.Blue,
                    Fill = Brushes.Transparent,
                    PointGeometry = null
                };
                SeriesCollection.Add(line);
            }
        }
    }
}