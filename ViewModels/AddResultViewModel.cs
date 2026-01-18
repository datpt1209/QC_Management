using Microsoft.EntityFrameworkCore;
using QC_Management.Models;
using QC_Management.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using XAct.Library.Settings;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace QC_Management.ViewModels
{

public class AddResultViewModel : BaseViewModel
    {
        private DateTime _selectedDate;
        private Device _selectedDevice;
        private LevelQc _selectedLevel;
        private Test _selectedTest;
        private int _selectedIndex;
        private System.Windows.Window _window;
        private ObservableCollection<Result> _newResults;
        private string _comment;
        private bool _isOutOfRange;
        private bool _isOut2SD;
        private string _resultString;
        private double _result;

        // Added SelectedItem so DataGrid.SelectedItem binding has a target on the VM
        private Result? _selectedItem;
        public Result? SelectedItem
        {
            get => _selectedItem;
            set
            {
                _selectedItem = value;
                OnPropertyChanged();
            }
        }

        public string ResultString
        {
            get => _resultString;
            set
            {
                if (_resultString != value)
                {
                    _resultString = value;
                    OnPropertyChanged(nameof(ResultString));
                    // Try to convert the string to double
                    if (double.TryParse(_result_string_fallback(value), out double result))
                    {
                        Result = result;
                    }
                }
            }
        }

        // helper in case ResultString is null - keep behavior unchanged
        private static string _result_string_fallback(string? v) => v ?? string.Empty;

        public double Result
        {
            get => _result;
            set
            {
                if (_result != value)
                {
                    _result = value;
                    OnPropertyChanged(nameof(Result));
                }
            }
        }

        private ObservableCollection<Test> _TestList;
        public ObservableCollection<Test> TestList { get => _TestList; set { _TestList = value; OnPropertyChanged(); } }

        public DateTime SelectedDate
        {
            get => _selectedDate;
            set
            {
                _selectedDate = value;
                OnPropertyChanged();
            }
        }

        public bool isOutOfRange
        {
            get => _isOutOfRange;
            set
            {
                _isOutOfRange = value;
                OnPropertyChanged();
            }
        }

        public bool isOut2SD
        {
            get => _isOut2SD;
            set
            {
                _isOut2SD = value;
                OnPropertyChanged();
            }
        }
        public string Comment
        {
            get => _comment;
            set
            {
                _comment = value;
                OnPropertyChanged();
            }
        }
        public Test SelectedTest
        {
            get => _selectedTest;
            set
            {
                _selectedTest = value;
                OnPropertyChanged();
            }
        }

        public Device SelectedDevice
        {
            get => _selectedDevice;
            set
            {
                _selectedDevice = value;
                OnPropertyChanged();
                LoadTestList();
            }
        }

        public LevelQc SelectedLevel
        {
            get => _selectedLevel;
            set
            {
                _selectedLevel = value;
                OnPropertyChanged();
            }
        }

        public int SelectedIndex
        {
            get => _selectedIndex;
            set
            {
                _selectedIndex = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<Result> NewResults
        {
            get => _newResults;
            set
            {
                _newResults = value;
                OnPropertyChanged();
            }
        }

        public ICommand SaveCommand { get; }
        public ICommand AddResultCommand { get; }
        public ICommand CancelCommand { get; }

        public AddResultViewModel(DateTime selectedDate, Device selectedDevice, LevelQc selectedLevel, int? selectedIndex, System.Windows.Window window)
        {
            SelectedDate = selectedDate;
            SelectedDevice = selectedDevice;
            SelectedLevel = selectedLevel;
            SelectedIndex = selectedIndex ?? 0;
            NewResults = new ObservableCollection<Result>();
            SaveCommand = new RelayCommand<Result>((p) => true, (p) => SaveAsync());
            CancelCommand = new RelayCommand<Result>((p) => true, (p) => Cancel());
            AddResultCommand = new RelayCommand<Result>((p) => true, (p) => AddResult());
            _window = window;
            LoadTestList();
        }

        private async Task SaveAsync()
        {
            if (NewResults.Count == 0)
            {
                MessageBox.Show("Chưa nhập kết quả QC", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else
            {
                using (var DB = new QcManagmentContext())
                {
                    // Gọi hàm lưu dữ liệu
                    bool isSaved = await SaveDataAsync(DB, NewResults);

                    // Hiển thị thông báo thành công hoặc thất bại
                    if (isSaved)
                    {
                        MessageBox.Show("Lưu kết quả thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                        _window.DialogResult = true;
                        _window.Close();
                    }
                    else
                    {
                        MessageBox.Show("Lưu dữ liệu thất bại. Vui lòng thử lại.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void Cancel()
        {
            _window.DialogResult = false;
            _window.Close();
        }

        private void LoadTestList()
        {
            if (SelectedDevice != null)
            {
                using (var DB = new QcManagmentContext())
                {
                    TestList = new ObservableCollection<Test>(DB.DeviceTests
                        .Include(s => s.IdTestNavigation)
                        .Where(s => s.IdDevice == SelectedDevice.Id)
                        .Select(s => s.IdTestNavigation)
                        .OrderBy(s => s.Index));
                }
            }
        }

        // Changed to async void to allow awaiting DB/history checks inside (command handler)
        private async void AddResult()
        {
            if (SelectedTest != null && !string.IsNullOrEmpty(ResultString))
            {
                using (var DB = new QcManagmentContext())
                {
                    var qcInfor = DB.ControlInfoDetails
                        .Where(s =>
                             s.IdLevel == SelectedLevel.Id
                             && s.IdTest == SelectedTest.Id
                            && s.Status == true
                            && s.IdDevice == SelectedDevice.Id).FirstOrDefault();

                    if (qcInfor != null)
                    {
                        // combine date + current time so DateRun stores full date+time
                        var now = DateTime.Now;
                        var combinedDateTime = SelectedDate.Date.Add(now.TimeOfDay);

                        var newResult = new Result
                        {
                            IdTest = SelectedTest.Id,
                            ResultType = SelectedTest.TestType,
                            IdTestNavigation = SelectedTest,
                            IdDevice = SelectedDevice.Id,
                            IdLevel = SelectedLevel.Id,
                            DateRun = combinedDateTime,
                            Time = combinedDateTime.TimeOfDay,
                            IdUser = UserManager.Instance.CurrentUser.Id,
                            IndexQc = SelectedIndex,
                            IdControlDetail = qcInfor.Id,
                            IdControlDetailNavigation = qcInfor,
                            Comment = Comment,
                            TempResult = ResultString,
                        };

                        // Compute numeric Result1 and ZScore for quantitative tests (TestType == 2)
                        if (newResult.ResultType == 2)
                        {
                            if (double.TryParse(newResult.TempResult, out var parsed))
                            {
                                newResult.Result1 = parsed;
                                if (qcInfor != null && qcInfor.CurMean.HasValue && qcInfor.CurSd.HasValue && qcInfor.CurSd.Value != 0)
                                {
                                    newResult.ZScore = Math.Round((parsed - qcInfor.CurMean.Value) / qcInfor.CurSd.Value, 2);
                                }
                                else
                                {
                                    newResult.ZScore = null;
                                }
                            }
                            else
                            {
                                newResult.ZScore = null;
                            }
                        }
                        else
                        {
                            // qualitative: determine out-of-range based on control info
                            if (qcInfor != null && !string.IsNullOrEmpty(newResult.TempResult))
                            {
                                try
                                {
                                    newResult.IsOutRange = !qcInfor.IsQualitativeResultAcceptable(newResult.TempResult);
                                }
                                catch
                                {
                                    newResult.IsOutRange = null;
                                }
                            }
                            else
                            {
                                newResult.IsOutRange = null;
                            }
                            newResult.ZScore = null;
                        }

                        // Evaluate Westgard rules using recent history (same as in ResultViewModel)
                        try
                        {
                            var (sameLevelPrev, crossLevelPrev) = await GetRecentHistoryAsync(SelectedTest.Id, SelectedDevice.Id, SelectedLevel.Id, take: 10);

                            // Build a current Result object for checker (we already have newResult)
                            var current = newResult;

                            // Load per-device/test enabled rules
                            IEnumerable<string>? enabledRules = null;
                            try
                            {
                                var dt = await DB.DeviceTests
                                                 .AsNoTracking()
                                                 .Where(d => d.IdDevice == SelectedDevice.Id && d.IdTest == SelectedTest.Id)
                                                 .Select(d => new { d.WestgardRulesJson })
                                                 .FirstOrDefaultAsync();

                                if (dt != null && !string.IsNullOrWhiteSpace(dt.WestgardRulesJson))
                                {
                                    try
                                    {
                                        var parsed = JsonSerializer.Deserialize<List<string>>(dt.WestgardRulesJson);
                                        if (parsed != null && parsed.Count > 0)
                                        {
                                            enabledRules = parsed.Select(s => s?.Trim()).Where(s => !string.IsNullOrEmpty(s)).ToList();
                                        }
                                        else
                                        {
                                            enabledRules = null;
                                        }
                                    }
                                    catch
                                    {
                                        enabledRules = null;
                                    }
                                }
                            }
                            catch
                            {
                                enabledRules = null;
                            }

                            var keysToEvaluate = (enabledRules != null)
                                ? enabledRules.ToList()
                                : new List<string> { "1_3S", "1_2S", "2_2S", "R-4s", "4_1S", "10X" };

                            var aggViolations = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                            bool aggIsOut2SD = false;
                            bool aggIsOutRange = false;

                            foreach (var rk in keysToEvaluate)
                            {
                                try
                                {
                                    var part = LeveyJenningsChecker.EvaluateSingleRule(current, sameLevelPrev, crossLevelPrev, rk);
                                    if (part?.ViolatedRules != null)
                                    {
                                        foreach (var v in part.ViolatedRules)
                                            aggViolations.Add(v);
                                    }
                                    aggIsOut2SD = aggIsOut2SD || part.IsOut2SD;
                                    aggIsOutRange = aggIsOutRange || part.IsOutRange;
                                }
                                catch
                                {
                                    // ignore individual rule failures
                                }
                            }

                            var ordered = aggViolations.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
                            if (ordered.Contains("1_2S") && ordered.Contains("1_3S"))
                            {
                                ordered.Remove("1_2S");
                                ordered.Insert(ordered.IndexOf("1_3S") + 1, "1_2S");
                            }

                            var levey = new LeveyResult();
                            levey.ViolatedRules.AddRange(ordered);
                            levey.IsOut2SD = aggIsOut2SD;
                            levey.IsOutRange = aggIsOutRange;

                            // Apply results to entity
                            newResult.IsOutRange = levey.IsOutRange;
                            // set WestgardRule string
                            newResult.WestgardRule = levey.ViolatedRules.Count > 0 ? string.Join(", ", levey.ViolatedRules) : null;

                            // try to propagate 2SD flag into Result.IsOutRangeNSX if property exists
                            try
                            {
                                var prop = newResult.GetType().GetProperty("IsOutRangeNSX");
                                if (prop != null && prop.CanWrite)
                                {
                                    prop.SetValue(newResult, levey.IsOut2SD);
                                }
                            }
                            catch { /* ignore reflection failures */ }

                            // Set IsCorrected flag: if there's a problem mark as not corrected (false), otherwise null
                            newResult.IsCorrected = (!string.IsNullOrWhiteSpace(newResult.WestgardRule) || newResult.IsOutRange == true)
                                ? (bool?)false
                                : null;

                            // Update view-level flags for dialog display
                            this.isOutOfRange = newResult.IsOutRange == true;
                            this.isOut2SD = levey.IsOut2SD;
                        }
                        catch
                        {
                            // non-fatal: keep earlier computed fields
                        }

                        NewResults.Add(newResult);
                        // Clear input
                        ResultString = null;
                    }
                    else
                    {
                        MessageBox.Show("Không tìm thấy thông tin kiểm soát.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        public async Task<bool> SaveDataAsync(QcManagmentContext DB, ObservableCollection<Result> results)
        {
            try
            {
                foreach (var result in results)
                {
                    // Kiểm tra và gắn thực thể ControlInfoDetail nếu chưa được theo dõi
                    if (result.IdControlDetail.HasValue)
                    {
                        var existingControlDetail = await DB.ControlInfoDetails.FindAsync(result.IdControlDetail.Value);
                        if (existingControlDetail != null)
                        {
                            DB.Entry(existingControlDetail).State = EntityState.Unchanged;
                            result.IdControlDetailNavigation = existingControlDetail;
                        }
                    }

                    // Kiểm tra và gắn thực thể Test nếu chưa được theo dõi
                    if (result.IdTest != 0)
                    {
                        var existingTest = await DB.Tests.FindAsync(result.IdTest);
                        if (existingTest != null)
                        {
                            DB.Entry(existingTest).State = EntityState.Unchanged;
                            result.IdTestNavigation = existingTest;
                        }
                    }

                    // Thêm hoặc cập nhật thực thể Result
                    var existingResult = await DB.Results.FindAsync(result.Id);
                    if (existingResult == null)
                    {
                        DB.Results.Add(result);
                    }
                    else
                    {
                        DB.Entry(existingResult).CurrentValues.SetValues(result);
                    }
                }

                await DB.SaveChangesAsync();
                return true; // Trả về true nếu lưu thành công
            }
            catch (Exception ex)
            {
                // Xử lý lỗi nếu có
                MessageBox.Show($"Có lỗi: {ex}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return false; // Trả về false nếu lưu thất bại
            }
        }

        // Copied helper from ResultViewModel to provide recent history for Westgard checks
        private async Task<(List<Result> sameLevel, List<Result> crossLevel)> GetRecentHistoryAsync(int testId, int deviceId, int levelId, int take = 10)
        {
            try
            {
                using var db = new QcManagmentContext();
                var recent = await db.Results
                    .AsNoTracking()
                    .Include(r => r.IdControlDetailNavigation)
                    .Where(r => r.IdTest == testId && r.IdDevice == deviceId && r.IsExclude != true)
                    .OrderByDescending(r => r.DateRun)
                    .ThenByDescending(r => r.IndexQc ?? 0)
                    .ThenByDescending(r => r.Time ?? TimeSpan.Zero)
                    .Take(take)
                    .ToListAsync();

                var cross = recent ?? new List<Result>();
                var sameLevel = cross.Where(r => r.IdLevel == levelId).ToList();
                return (sameLevel, cross);
            }
            catch
            {
                return (new List<Result>(), new List<Result>());
            }
        }
    }
}
