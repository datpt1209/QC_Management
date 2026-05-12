using Microsoft.EntityFrameworkCore;
using QC_Management.Models;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.Data.SqlClient; // add this using

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

        private ExternalResultRow _selectedResult;
        public ExternalResultRow SelectedResult
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

        public ExternalResultViewModel()
        {
            LoadedCommand = new RelayCommand<object>((p) => true, async (p) => await LoadLookupsAsync());

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
                    bool hasReference = !string.IsNullOrWhiteSpace(m.ReferenceValue);

                    if (!hasResult && !hasReference)
                        continue;

                    var testType = m.IdTestNavigation?.TestType ?? 0;

                    if (testType == 2)
                    {
                        bool hasSigma = !string.IsNullOrWhiteSpace(m.SigmaP);
                        bool parsable = double.TryParse(m.TempResult, NumberStyles.Any, CultureInfo.CurrentCulture, out _)
                                        && double.TryParse(m.ReferenceValue, NumberStyles.Any, CultureInfo.CurrentCulture, out _)
                                        && double.TryParse(m.SigmaP, NumberStyles.Any, CultureInfo.CurrentCulture, out var sigma) && Math.Abs(sigma) > double.Epsilon;

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
                    mm.ExternalProgram = null;
                    mm.IdDeviceNavigation = null;
                    mm.IdTestNavigation = null;

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
                view.DataContext = vm;

                var win = new Window
                {
                    Title = "Manage External Programs",
                    Content = view,
                    Owner = Application.Current?.MainWindow,
                    Width = 900,
                    Height = 600,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                };

                win.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unable to open Programs screen: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}