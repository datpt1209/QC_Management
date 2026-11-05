using Microsoft.EntityFrameworkCore;
using QC_Management.Models;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Data.SqlClient; // existing
using System.IO;
using ExcelDataReader;
using Microsoft.Win32;
using System.Text;
using System.Data;

namespace QC_Management.ViewModels
{
    public class ExternalResultViewModel : BaseViewModel
    {
        // Lookup collections
        public ObservableCollection<ExternalProgram> Programs { get; } = new();
        public ObservableCollection<Device> Devices { get; } = new();

        // Grid data uses row VMs
        public ObservableCollection<ExternalResultRow> NewResults { get; } = new();

        private ExternalProgram _selectedProgram;
        public ExternalProgram SelectedProgram
        {
            get => _selectedProgram;
            set { _selectedProgram = value; OnPropertyChanged(); }
        }

        private Device _selectedDevice;
        public Device SelectedDevice
        {
            get => _selectedDevice;
            set { _selectedDevice = value; OnPropertyChanged(); }
        }

        // Change the type of _selectedResult and SelectedResult to allow nulls
        private ExternalResultRow? _selectedResult;
        public ExternalResultRow? SelectedResult
        {
            get => _selectedResult;
            set { _selectedResult = value; OnPropertyChanged(); }
        }

        // Input helpers
        private string _batch;
        public string Batch { get => _batch; set { _batch = value; OnPropertyChanged(); } }

        // DateRun stored and used by models. Keep a DateTime that includes time for binding.
        private DateTime _dateRun = DateTime.UtcNow;
        private bool _suppressDateSync;
        private DateTime _selectedDateTime = DateTime.UtcNow;

        // Date+time for UI (bound to DatePicker + TimePicker). Changing this updates DateRun.
        public DateTime SelectedDateTime
        {
            get => _selectedDateTime;
            set
            {
                if (_selectedDateTime == value) return;
                _selectedDateTime = value;
                OnPropertyChanged();

                if (_suppressDateSync) return;
                try
                {
                    _suppressDateSync = true;
                    DateRun = _selectedDateTime;
                }
                finally { _suppressDateSync = false; }
            }
        }

        // Existing DateRun. Keep in sync with SelectedDateTime.
        public DateTime DateRun
        {
            get => _dateRun;
            set
            {
                if (_dateRun == value) return;
                _dateRun = value;
                OnPropertyChanged();

                if (_suppressDateSync) return;
                try
                {
                    _suppressDateSync = true;
                    // Preserve existing time component from SelectedDateTime if needed
                    _selectedDateTime = new DateTime(value.Year, value.Month, value.Day, _selectedDateTime.Hour, _selectedDateTime.Minute, _selectedDateTime.Second);
                    OnPropertyChanged(nameof(SelectedDateTime));
                }
                finally { _suppressDateSync = false; }
            }
        }

        // Shared inputs (apply to all rows in one entry)
        private string _sharedSample;
        public string SharedSample
        {
            get => _sharedSample;
            set
            {
                if (_sharedSample == value) return;
                _sharedSample = value;
                OnPropertyChanged();
                foreach (var r in NewResults) r.Sample = _sharedSample;
            }
        }

        private DateTime? _sharedReceivedAt;
        public DateTime? SharedReceivedAt
        {
            get => _sharedReceivedAt;
            set
            {
                if (_sharedReceivedAt == value) return;
                _sharedReceivedAt = value;
                OnPropertyChanged();
                foreach (var r in NewResults) r.ReceivedAt = _sharedReceivedAt;
            }
        }

        // Commands
        public ICommand LoadTestsCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand LoadedCommand { get; }
        public ICommand OpenProgramCommand { get; }
        public ICommand ImportExcelCommand { get; }
        public ICommand ExportCsvCommand { get; }

        public ExternalResultViewModel()
        {
            LoadedCommand = new RelayCommand<object>((p) => true, _ => { /* no auto-load */ });

            LoadTestsCommand = new RelayCommand<object>(
                (p) => SelectedDevice != null && SelectedProgram != null,
                (p) => LoadTestsForDevice());

            SaveCommand = new RelayCommand<object>(
                (p) => NewResults.Any(),
                async (p) => await SaveAsync());

            DeleteCommand = new RelayCommand<object>(
                (p) => SelectedResult != null,
                (p) => DeleteSelected());

            RefreshCommand = new RelayCommand<object>(
                (p) => true,
                async (p) => await LoadLookupsAsync());

            OpenProgramCommand = new RelayCommand<object>((p) => true, (p) => OpenProgramsWindow());

            ImportExcelCommand = new RelayCommand<object>((p) => true, (p) => ImportFromExcel());

          //  ExportCsvCommand = new RelayCommand<object>((p) => NewResults.Any(), (p) => ExportCsv());

            // initialize SelectedDateTime to match DateRun
            _selectedDateTime = _dateRun;
        }

        private async Task LoadLookupsAsync()
        {
            try
            {
                using var db = new QcManagmentContext();

                var programs = await db.ExternalPrograms
                    .AsNoTracking()
                    .Where(p => p.IsActive)
                    .OrderByDescending(p => p.Year)
                    .ThenBy(p => p.Name)
                    .ToListAsync();

                Programs.Clear();
                foreach (var p in programs) Programs.Add(p);

                var devices = await db.Devices
                    .AsNoTracking()
                    .OrderBy(d => d.Name)
                    .ToListAsync();

                Devices.Clear();
                foreach (var d in devices) Devices.Add(d);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Load lookup failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Build one row for each test on the selected device
        private void LoadTestsForDevice()
        {
            if (SelectedDevice == null || SelectedProgram == null)
                return;

            try
            {
                NewResults.Clear();

                var tests = DataProvider.Ins.DB.DeviceTests
                    .Where(d => d.IdDevice == SelectedDevice.Id)
                    .Select(d => d.IdTestNavigation)
                    .OrderBy(t => t.Index)
                    .ToList();

                foreach (var test in tests)
                {
                    var model = new ExternalResult
                    {
                        ExternalProgramId = SelectedProgram.Id,
                        ExternalProgram = SelectedProgram,
                        Batch = Batch,
                        DateRun = DateRun,
                        IdDevice = SelectedDevice.Id,
                        IdDeviceNavigation = SelectedDevice,
                        IdTest = test.Id,
                        IdTestNavigation = test,
                        // apply shared values
                        Sample = SharedSample,
                        ReceivedAt = SharedReceivedAt
                    };

                    var row = new ExternalResultRow(model);
                    NewResults.Add(row);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Load tests failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteSelected()
        {
            if (SelectedResult == null) return;
            if (MessageBox.Show("Delete selected result?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                return;

            NewResults.Remove(SelectedResult);
            SelectedResult = null;
        }

        // SaveAsync unchanged (keeps evaluation before detach)...
        private async Task SaveAsync()
        {
            if (!NewResults.Any())
            {
                MessageBox.Show("No results to save.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                var pairsToSave = new System.Collections.Generic.List<(ExternalResultRow Row, ExternalResult Model)>();
                var partialRows = new System.Collections.Generic.List<string>();

                foreach (var row in NewResults)
                {
                    var m = row.Model;
                    bool hasResult = !string.IsNullOrWhiteSpace(m.TempResult);
                    bool hasReference = !string.IsNullOrWhiteSpace(Convert.ToString(m.ReferenceValue, CultureInfo.InvariantCulture));

                    if (!hasResult && !hasReference)
                        continue;

                    var testType = m.IdTestNavigation?.TestType ?? 0;

                    if (testType == 2)
                    {
                        bool hasSigma = !string.IsNullOrWhiteSpace(Convert.ToString(m.SigmaP, CultureInfo.InvariantCulture));
                        bool parsable = double.TryParse(m.TempResult, NumberStyles.Any, CultureInfo.CurrentCulture, out _)
                                        && double.TryParse(Convert.ToString(m.ReferenceValue, CultureInfo.InvariantCulture), NumberStyles.Any, CultureInfo.CurrentCulture, out _)
                                        && double.TryParse(Convert.ToString(m.SigmaP, CultureInfo.InvariantCulture), NumberStyles.Any, CultureInfo.CurrentCulture, out var sigma) && Math.Abs(sigma) > double.Epsilon;

                        if (hasResult && hasReference && hasSigma && parsable)
                            pairsToSave.Add((row, m));
                        else
                        {
                            var id = $"{row.TestName ?? $"TestId:{m.IdTest}"} ({row.DeviceName ?? $"Device:{m.IdDevice}"})";
                            partialRows.Add($"{id} - Quantitative requires numeric Result, Reference and non-zero σp");
                        }
                    }
                    else
                    {
                        if (hasResult && hasReference)
                            pairsToSave.Add((row, m));
                        else
                        {
                            var id = $"{row.TestName ?? $"TestId:{m.IdTest}"} ({row.DeviceName ?? $"Device:{m.IdDevice}"})";
                            partialRows.Add($"{id} - Qualitative requires both Result and Reference text");
                        }
                    }
                }

                if (partialRows.Any())
                {
                    MessageBox.Show($"Cannot save. The following rows are missing or invalid and must be completed or cleared before saving:\n\n{string.Join("\n", partialRows)}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (!pairsToSave.Any())
                {
                    MessageBox.Show("No results to save.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                using var db = new QcManagmentContext();

                var models = pairsToSave.Select(p =>
                {
                    var mm = p.Model;

                    mm.EvaluatedBy = UserManager.Instance?.CurrentUser?.DisplayName;
                    mm.ResultSavedAt = DateTime.UtcNow;

                    // Recompute evaluation while navigation props still present
                    mm.ApplyReferenceEvaluation();

                    // detach nav props
                    mm.ExternalProgram = default!;
                    mm.IdDeviceNavigation = null;
                    mm.IdTestNavigation = default!;

                    return mm;
                }).ToList();

                db.ExternalResults.AddRange(models);
                await db.SaveChangesAsync();

                foreach (var p in pairsToSave.ToList())
                {
                    NewResults.Remove(p.Row);
                }

                MessageBox.Show("External results saved successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Save failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OpenProgramsWindow()
        {
            try
            {
                var view = new Views.ExternalProgramView();
                var vm = new ExternalProgramViewModel();

                // set DataContext first
                view.DataContext = vm;

                // explicitly invoke VM's loaded command to initialize lookups safely
                try
                {
                    if (vm.LoadedCommand != null && vm.LoadedCommand.CanExecute(null))
                        vm.LoadedCommand.Execute(null);
                }
                catch
                {
                    // swallow any VM load exceptions here; the VM will show its own MessageBox on failure
                }

                var win = new Window
                {
                    Title = "Manage External Programs",
                    Content = view,
                    Width = 900,
                    Height = 600,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                };

                // Determine the hosting/owner window safely
                try
                {
                    // the window that currently hosts the view (if any)
                    var hostWindow = Window.GetWindow(view);

                    // fallback to the active application window if no host found
                    var appMain = Application.Current?.MainWindow;
                    var owner = hostWindow ?? appMain;

                    // Only set Owner if it's non-null and not the same as the window we're creating
                    if (owner != null && !ReferenceEquals(owner, win))
                        win.Owner = owner;
                }
                catch
                {
                    // ignore owner assignment errors — show window without owner instead
                }

                win.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unable to open Programs screen: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ------------------- Excel import -------------------
        private void ImportFromExcel()
        {
            try
            {
                if (SelectedProgram == null)
                {
                    MessageBox.Show("Please select Program first.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var dlg = new Microsoft.Win32.OpenFileDialog
                {
                    Filter = "Excel files (*.xlsx;*.xls)|*.xlsx;*.xls",
                    Title = "Select Excel file"
                };

                if (dlg.ShowDialog() != true)
                    return;

                var file = dlg.FileName;
                System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

                using var stream = System.IO.File.Open(file, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite);
                using var reader = ExcelDataReader.ExcelReaderFactory.CreateReader(stream);
                var ds = reader.AsDataSet(new ExcelDataReader.ExcelDataSetConfiguration()
                {
                    ConfigureDataTable = (_) => new ExcelDataReader.ExcelDataTableConfiguration() { UseHeaderRow = true }
                });

                if (ds.Tables.Count == 0)
                {
                    MessageBox.Show("Excel file contains no sheets.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var table = ds.Tables[0];

                // preload tests from DB for matching (include AlinityName if present)
                var allTests = DataProvider.Ins.DB.Tests.ToList();

                NewResults.Clear();
                var unmatched = new System.Collections.Generic.List<string>();
                var created = 0;

                for (int r = 0; r < table.Rows.Count; r++)
                {
                    var row = table.Rows[r];
                    // expected headers: A=Thông số, B=Đơn vị đo, C=Kết quả đơn vị, D=Giá trị ấn định, E=σp, F=z-score
                    var rawTest = row.ItemArray.Length > 0 ? row[0]?.ToString()?.Trim() : null;
                    var resultCell = row.ItemArray.Length > 2 ? row[2]?.ToString()?.Trim() : null;
                    var refCell = row.ItemArray.Length > 3 ? row[3]?.ToString()?.Trim() : null;
                    var sigmaCell = row.ItemArray.Length > 4 ? row[4]?.ToString()?.Trim() : null;

                    if (string.IsNullOrWhiteSpace(rawTest))
                        continue;

                    // find matching Test entity (prefer AlinityName)
                    var matchedTest = allTests.FirstOrDefault(t =>
                    {
                        var al = t.GetType().GetProperty("ParameterName")?.GetValue(t)?.ToString()?.Trim();
                        if (!string.IsNullOrWhiteSpace(al))
                        {
                            if (string.Equals(al, rawTest, StringComparison.CurrentCultureIgnoreCase)) return true;
                            if (al.IndexOf(rawTest, StringComparison.CurrentCultureIgnoreCase) >= 0) return true;
                            if (rawTest.IndexOf(al, StringComparison.CurrentCultureIgnoreCase) >= 0) return true;
                        }

                        var name = t.Name?.Trim();
                        if (!string.IsNullOrWhiteSpace(name))
                        {
                            if (string.Equals(name, rawTest, StringComparison.CurrentCultureIgnoreCase)) return true;
                            if (name.IndexOf(rawTest, StringComparison.CurrentCultureIgnoreCase) >= 0) return true;
                            if (rawTest.IndexOf(name, StringComparison.CurrentCultureIgnoreCase) >= 0) return true;
                        }

                        return false;
                    });

                    if (matchedTest == null)
                    {
                        unmatched.Add(rawTest);
                        continue;
                    }

                    // avoid duplicates: if NewResults already contains same test id skip (keep first occurrence)
                    if (NewResults.Any(nr => nr.Model.IdTest == matchedTest.Id))
                        continue;

                    var model = new ExternalResult
                    {
                        ExternalProgramId = SelectedProgram.Id,
                        ExternalProgram = SelectedProgram,
                        Batch = Batch,
                        DateRun = DateRun,
                        IdDevice = SelectedDevice?.Id,
                        IdDeviceNavigation = SelectedDevice,
                        IdTest = matchedTest.Id,
                        IdTestNavigation = matchedTest,
                        Sample = SharedSample,
                        ReceivedAt = SharedReceivedAt,
                        TempResult = resultCell,
                        ReferenceValue = double.TryParse(refCell, NumberStyles.Any, CultureInfo.CurrentCulture, out var refVal) ? refVal : (double?)null,
                        SigmaP = double.TryParse(sigmaCell, NumberStyles.Any, CultureInfo.CurrentCulture, out var sigmaVal) ? sigmaVal : (double?)null
                    };

                    // ensure ZScore/status computed
                    model.ApplyReferenceEvaluation();

                    var rowVm = new ExternalResultRow(model);
                    NewResults.Add(rowVm);
                    created++;
                }

                var msg = $"Imported {created} test rows from Excel.";
                if (unmatched.Any())
                    msg += $"\nUnmatched tests: {string.Join(", ", unmatched.Take(20))}" + (unmatched.Count > 20 ? $" (+{unmatched.Count-20} more)" : "");

                MessageBox.Show(msg, "Import Excel", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Import failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}