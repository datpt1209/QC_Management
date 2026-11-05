using Microsoft.EntityFrameworkCore;
using QC_Management.Models;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace QC_Management.ViewModels
{
    public class ExternalResultReviewViewModel : BaseViewModel
    {
        public ObservableCollection<ExternalProgram> Programs { get; } = new();
        public ObservableCollection<Device> Devices { get; } = new();
        public ObservableCollection<Test> Tests { get; } = new();
        public ObservableCollection<ExternalResultRow> Results { get; } = new();

        private ExternalResultRow _selectedRow;
        public ExternalResultRow SelectedRow { get => _selectedRow; set { _selectedRow = value; OnPropertyChanged(); } }

        public ExternalProgram SelectedProgram { get; set; }
        public Device SelectedDevice { get; set; }
        public Test SelectedTest { get; set; }

        public DateTime? FromDate { get; set; } = DateTime.Today.AddMonths(-1);
        public DateTime? ToDate { get; set; } = DateTime.Today;

        public ICommand LoadCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand LoadedCommand { get; }
        public ICommand ExportCsvCommand { get; }

        public ExternalResultReviewViewModel()
        {
            LoadCommand = new RelayCommand<object>(_ => true, _ => LoadAsync());
            RefreshCommand = new RelayCommand<object>(_ => true, _ => _ = LoadLookupsAsync());
            LoadedCommand = new RelayCommand<object>(_ => true, async (p) => await LoadLookupsAsync());
            ExportCsvCommand = new RelayCommand<object>(_ => Results.Any(), _ => ExportCsv());

            // Do NOT load at ctor time; load when view is ready (LoadedCommand)
        }

        private async Task LoadLookupsAsync()
        {
            try
            {
                using var db = new QcManagmentContext();

                // quick connectivity check
                if (!await db.Database.CanConnectAsync().ConfigureAwait(false))
                {
                    Application.Current?.Dispatcher?.Invoke(() =>
                        MessageBox.Show("Cannot connect to database. Check connection settings and SQL Server.", "DB Error", MessageBoxButton.OK, MessageBoxImage.Error)
                    );
                    return;
                }

                // Execute queries sequentially to avoid concurrent DbContext operations
                var progsTask = Task.Run(async () => { using var c = new QcManagmentContext(); return await c.ExternalPrograms.AsNoTracking().OrderByDescending(p=>p.Year).ThenBy(p=>p.Name).ToListAsync(); });
                var devsTask = Task.Run(async () => { using var c = new QcManagmentContext(); return await c.Devices.AsNoTracking().OrderBy(d => d.Name).ToListAsync(); });
                var testsTask = Task.Run(async () => { using var c = new QcManagmentContext(); return await c.Tests.AsNoTracking().OrderBy(t => t.Index).ThenBy(t => t.Name).ToListAsync(); });

                // marshal updates to UI
                UpdateCollectionOnUiThread(Programs, await progsTask);
                UpdateCollectionOnUiThread(Devices, await devsTask);
                UpdateCollectionOnUiThread(Tests, await testsTask);

                await Task.WhenAll(progsTask, devsTask, testsTask);
            }
            catch (Exception ex)
            {
                var detail = ex.ToString();
                if (Application.Current?.Dispatcher != null)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                        MessageBox.Show($"Load lookups failed:\n{detail}", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
                    );
                }
                else
                {
                    MessageBox.Show($"Load lookups failed:\n{detail}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // Replace or update LoadAsync to marshal Results updates via helper.
        // Use this version for LoadAsync
        private async void LoadAsync()
        {
            try
            {
                // load data from DB asynchronously (no UI modifications here)
                using var db = new QcManagmentContext();
                var start = FromDate ?? DateTime.MinValue;
                var end = (ToDate ?? DateTime.MaxValue).Date.AddDays(1);

                var q = db.ExternalResults
                    .AsNoTracking()
                    .Include(r => r.IdTestNavigation)
                    .Include(r => r.IdDeviceNavigation)
                    .Include(r => r.ExternalProgram)
                    .Where(r => r.DateRun >= start && r.DateRun < end);

                if (SelectedProgram != null) q = q.Where(r => r.ExternalProgram != null && r.ExternalProgram.Id == SelectedProgram.Id);
                if (SelectedDevice != null) q = q.Where(r => r.IdDevice == SelectedDevice.Id);
                if (SelectedTest != null) q = q.Where(r => r.IdTest == SelectedTest.Id);

                var list = await q.OrderByDescending(r => r.DateRun).Take(1000).ToListAsync().ConfigureAwait(false);

                // build row VMs on background thread
                var rows = list.Select(r => new ExternalResultRow(r)).ToList();

                // update ObservableCollection on UI thread
                UpdateCollectionOnUiThread(Results, rows);
            }
            catch (Exception ex)
            {
                if (Application.Current?.Dispatcher != null)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                        MessageBox.Show($"Load failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
                    );
                }
                else
                {
                    MessageBox.Show($"Load failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ExportCsv()
        {
            try
            {
                var dlg = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "CSV files (*.csv)|*.csv",
                    FileName = "external_results.csv"
                };

                if (dlg.ShowDialog() != true) return;

                var sb = new StringBuilder();
                sb.AppendLine("Date,Batch,Program,Device,Test,Result,Reference,Sigma,ZScore,Notes");

                foreach (var r in Results)
                {
                    var line = $"{r.DateRun:O},\"{Escape(r.Batch)}\",\"{Escape(r.ExternalProgramName)}\",\"{Escape(r.DeviceName)}\",\"{Escape(r.TestName)}\",\"{Escape(r.TempResult)}\",\"{Escape(r.ReferenceValue)}\",\"{Escape(r.SigmaP)}\",\"{Escape(r.ZScore?.ToString() ?? "")}\",\"{Escape(r.Notes)}\"";
                    sb.AppendLine(line);
                }

                File.WriteAllText(dlg.FileName, sb.ToString(), Encoding.UTF8);
                MessageBox.Show("Exported CSV successfully.", "Export", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Export failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static string Escape(string s) => (s ?? "").Replace("\"", "\"\"");

        // Add this helper near top of class (e.g. after fields)
        private void UpdateCollectionOnUiThread<T>(ObservableCollection<T> target, System.Collections.Generic.IEnumerable<T> source)
        {
            if (target == null) return;
            var dispatcher = Application.Current?.Dispatcher;
            if (dispatcher == null || dispatcher.CheckAccess())
            {
                target.Clear();
                if (source != null)
                {
                    foreach (var it in source) target.Add(it);
                }
            }
            else
            {
                dispatcher.Invoke(() =>
                {
                    target.Clear();
                    if (source != null)
                    {
                        foreach (var it in source) target.Add(it);
                    }
                });
            }
        }
    }
}