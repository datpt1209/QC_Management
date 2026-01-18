using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using QC_Management.Models;
using QC_Management.Services;
using QC_Management.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using XAct.Library.Settings;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;
using System.Text.Json;

namespace QC_Management.ViewModels
{
    public class Re_ResultDetailViewModel : BaseViewModel
    {
        private ObservableCollection<ReResult> _Results;
        public ObservableCollection<ReResult> Results
        {
            get => _Results;
            set { _Results = value; OnPropertyChanged(); }
        }

        private ObservableCollection<ResultReView> _ResutlViewList;
        public ObservableCollection<ResultReView> ResutlViewList { get => _ResutlViewList; set { _ResutlViewList = value; OnPropertyChanged(); } }

        private ResultReView _SelectedItem;
        public ResultReView SelectedItem
        {
            get => _SelectedItem;
            set
            {
                _SelectedItem = value;
                OnPropertyChanged();
            }
        }

        private System.Windows.Window _window;

        private string _Comment;
        public string Comment
        {
            get => _Comment;
            set
            {
                _Comment = value;
                OnPropertyChanged();
            }
        }
        private string _DeviceName;
        public string DeviceName
        {
            get => _DeviceName;
            set
            {
                _DeviceName = value;
                OnPropertyChanged();
            }
        }
        private string _LevelName;
        public string LevelName
        {
            get => _LevelName;
            set
            {
                _LevelName = value;
                OnPropertyChanged();
            }
        }

        private int _idLevel;
        public int IdLevel
        {
            get => _idLevel;
            set
            {
                _idLevel = value;
                OnPropertyChanged();
            }
        }

        private int _index;
        public int Index
        {
            get => _index;
            set
            {
                _index = value;
                OnPropertyChanged();
            }
        }

        private DateTime _Date;
        public DateTime Date
        {
            get => _Date;
            set
            {
                _Date = value;
                OnPropertyChanged();
            }
        }

        private TimeSpan? _Time;
        private DateTime? _TimeAsDateTime;
        private bool _suppressTimeSync;

        /// <summary>
        /// Existing VM Time (keeps model as TimeSpan?)
        /// </summary>
        public TimeSpan? Time
        {
            get => _Time;
            set
            {
                if (_Time == value) return;
                _Time = value;
                OnPropertyChanged();

                if (_suppressTimeSync) return;
                try
                {
                    _suppressTimeSync = true;
                    // reflect into DateTime? property for the TimePicker (use today's date)
                    TimeAsDateTime = _Time.HasValue ? DateTime.Today.Add(_Time.Value) : (DateTime?)null;
                }
                finally { _suppressTimeSync = false; }
            }
        }

        /// <summary>
        /// DateTime wrapper for binding to TimePicker.SelectedTime (no converter required)
        /// </summary>
        public DateTime? TimeAsDateTime
        {
            get => _TimeAsDateTime;
            set
            {
                if (_TimeAsDateTime == value) return;
                _TimeAsDateTime = value;
                OnPropertyChanged();

                if (_suppressTimeSync) return;
                try
                {
                    _suppressTimeSync = true;
                    Time = _TimeAsDateTime.HasValue ? _TimeAsDateTime.Value.TimeOfDay : (TimeSpan?)null;
                }
                finally { _suppressTimeSync = false; }
            }
        }

        public ICommand SaveCommand { get; set; }

        public ICommand CancelCommand { get; set; }

        public ICommand DeleteCommand { get; set; }

        public ICommand LoadCommand { get; set; }

        // store device id from ReResultGroup as fallback
        private readonly int _deviceId;

        // --- small in-memory history cache (reduces DB queries when evaluating many items) ---
        private readonly object _historyCacheLock = new();
        private readonly Dictionary<(int testId, int deviceId), CacheEntry> _historyCache = new();
        private readonly TimeSpan _historyCacheTtl = TimeSpan.FromSeconds(30);

        private class CacheEntry
        {
            public DateTimeOffset FetchedAt { get; set; }
            public List<Result> CrossLevelRecent { get; set; } = new();
        }

        // Clears history cache (call after saving/deleting if you need fresh data)
        public void ClearHistoryCache()
        {
            lock (_historyCacheLock)
            {
                _historyCache.Clear();
            }
        }

        // Cache-aware loader: returns (sameLevel, crossLevel) newest-first
        private async Task<(List<Result> sameLevel, List<Result> crossLevel)> GetRecentHistoryAsync(int testId, int deviceId, int levelId, int take = 10)
        {
            var key = (testId, deviceId);
            CacheEntry? entry = null;
            lock (_historyCacheLock)
            {
                if (_historyCache.TryGetValue(key, out var e))
                {
                    if (DateTimeOffset.UtcNow - e.FetchedAt < _historyCacheTtl)
                    {
                        entry = e;
                    }
                    else
                    {
                        _historyCache.Remove(key);
                    }
                }
            }

            if (entry == null)
            {
                try
                {
                    var db = DataProvider.Ins.DB;
                    var recent = await db.Results
                        .AsNoTracking()
                        .Include(r => r.IdControlDetailNavigation)
                        .Where(r => r.IdTest == testId && r.IdDevice == deviceId && r.IsExclude != true)
                        .OrderByDescending(r => r.DateRun)
                        .ThenByDescending(r => r.IndexQc ?? 0)
                        .ThenByDescending(r => r.Time ?? TimeSpan.Zero)
                        .Take(take)
                        .ToListAsync();

                    entry = new CacheEntry
                    {
                        FetchedAt = DateTimeOffset.UtcNow,
                        CrossLevelRecent = recent
                    };

                    lock (_historyCacheLock)
                    {
                        _historyCache[key] = entry;
                    }
                }
                catch
                {
                    return (new List<Result>(), new List<Result>());
                }
            }

            var cross = entry.CrossLevelRecent ?? new List<Result>();
            var same = cross.Where(r => r.IdLevel == levelId).ToList();
            return (same, cross);
        }

        public Re_ResultDetailViewModel(ReResultGroup reResultGroup, System.Windows.Window window)
        {
            Results = reResultGroup.Results;
            _window = window;
            Index = 0;

            // capture device id from group so we always have a device context
            _deviceId = reResultGroup.IdDevice;

            // Make LoadCommand async so we can query DB history and run Westgard checks.
            LoadCommand = new RelayCommand<ControlInfoDetail>((p) =>
            {
                return true;
            }, async (p) =>
            {
                try
                {
                    DeviceName = reResultGroup.DeviceName;
                    LevelName = reResultGroup.LevelName;
                    IdLevel = reResultGroup.IdLevel;
                    Date = reResultGroup.DateTime.Date;

                    // assign TimeSpan directly so TimePicker.SelectedTime (TimeSpan?) binds correctly
                    Time = reResultGroup.Time;

                    ResutlViewList = new ObservableCollection<ResultReView>();

                    var db = DataProvider.Ins.DB;

                    foreach (var item in Results)
                    {
                        var qcInfor = await db.ControlInfoDetails
                            .Include(s => s.IdControlInfoNavigation.IdControlTypeNavigation)
                            .Where(s =>
                                 s.IdLevel == item.IdLevel
                                 && s.IdTest == item.IdTest
                                 && s.IdDevice == item.IdDevice
                                 && s.Status == true)
                            .FirstOrDefaultAsync();

                        if (qcInfor == null)
                        {
                            MessageBox.Show($"Không tìm thấy thông tin QC {item.IdTestNavigation.Name}", "Thông báo", MessageBoxButton.OK);
                            continue;
                        }

                        var viewModelItem = new ResultReView
                        {
                            id = item.Id,
                            TestName = item.IdTestNavigation.Name,
                            ResultType = item.IdTestNavigation.TestType,
                            idTest = item.IdTest,
                            Test = item.IdTestNavigation,
                            LOT = qcInfor.Lot,
                            IdControlDetailNavigation = qcInfor
                        };

                        if (item.IdTestNavigation.TestType == 1)
                        {
                            viewModelItem.QualitativeMean = qcInfor.QualitativeMean;
                            viewModelItem.TempResult = item.QualitativeResult?.ToString();
                        }
                        else
                        {
                            viewModelItem.MeanApp = qcInfor.CurMean?.ToString();
                            viewModelItem.SdApp = qcInfor.CurSd;
                            viewModelItem.MeanNSX = qcInfor.MeanNsx;
                            viewModelItem.SdNSX = qcInfor.SdNsx;
                            // Use CurMean/CurSd or MeanApp/SdApp as appropriate — keep existing behavior for display
                            //viewModelItem.Max = qcInfor.MeanApp + 2 * (qcInfor.CurSd ?? qcInfor.SdNsx ?? 0);
                            //viewModelItem.Min = qcInfor.MeanApp - 2 * (qcInfor.CurSd ?? qcInfor.SdNsx ?? 0);
                            viewModelItem.TempResult = item.Result.HasValue ? item.Result.Value.ToString() : null;
                        }

                        // Run Westgard checks using the same algorithm as other places.
                        // Build a temporary Result object reflecting this ReResult so LeveyJenningsChecker can evaluate it.
                        try
                        {
                            var tempResult = new Result
                            {
                                IdTest = item.IdTest,
                                IdDevice = item.IdDevice,
                                IdLevel = item.IdLevel,
                                ResultType = item.IdTestNavigation.TestType,
                                Result1 = item.Result,
                                TempResult = item.Result.HasValue ? item.Result.Value.ToString() : item.QualitativeResult,
                                IdControlDetail = qcInfor.Id,
                                IdControlDetailNavigation = qcInfor,
                                DateRun = CombineDateAndTime(item.Date, item.Time), // <-- full date+time
                                Time = item.Time,
                                ZScore = item.Result.HasValue && qcInfor.CurMean.HasValue && qcInfor.CurSd.HasValue
                                    ? Math.Round((item.Result.Value - qcInfor.CurMean.Value) / qcInfor.CurSd.Value, 2)
                                    : null

                            };

                            // fetch histories using cache-aware helper (reduces per-item DB queries)
                            var (sameLevelPrev, crossLevelPrev) = await GetRecentHistoryAsync(tempResult.IdTest, tempResult.IdDevice, tempResult.IdLevel, take: 10);

                            // Load per-device/test enabled rules (if any) and evaluate only those rules via EvaluateSingleRule
                            IEnumerable<string>? enabled = null;
                            try
                            {
                                using var db2 = new QcManagmentContext();
                                var json = await db2.DeviceTests
                                                    .AsNoTracking()
                                                    .Where(d => d.IdDevice == tempResult.IdDevice && d.IdTest == tempResult.IdTest)
                                                    .Select(d => d.WestgardRulesJson)
                                                    .FirstOrDefaultAsync();

                                if (!string.IsNullOrWhiteSpace(json))
                                {
                                    try
                                    {
                                        var parsed = JsonSerializer.Deserialize<List<string>>(json);
                                        if (parsed != null && parsed.Count > 0)
                                            enabled = parsed.Select(s => s?.Trim()).Where(s => !string.IsNullOrEmpty(s)).ToList();
                                        else
                                            enabled = null; // empty -> treat as "all rules"
                                    }
                                    catch
                                    {
                                        enabled = null;
                                    }
                                }
                            }
                            catch
                            {
                                enabled = null;
                            }

                            LeveyResult levey;

                            // Instead of calling Evaluate with a list/expanded rules, always call EvaluateSingleRule per active key.
                            // If enabled == null => treat as "all rules" and evaluate the main rule keys individually (minimal expansion).
                            var keysToEvaluate = (enabled != null)
                                ? enabled.ToList()
                                : new List<string> { "1_3S", "1_2S", "2_2S", "R-4s", "4_1S", "10X" };

                            var aggViolations = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                            bool aggIsOut2SD = false;
                            bool aggIsOutRange = false;

                            foreach (var rk in keysToEvaluate)
                            {
                                try
                                {
                                    var part = LeveyJenningsChecker.EvaluateSingleRule(tempResult, sameLevelPrev, crossLevelPrev, rk);
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
                                    // non-fatal per-rule failures - continue with others
                                }
                            }

                            levey = new LeveyResult();
                            var ordered = aggViolations.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
                            if (ordered.Contains("1_2S") && ordered.Contains("1_3S"))
                            {
                                ordered.Remove("1_2S");
                                ordered.Insert(ordered.IndexOf("1_3S") + 1, "1_2S");
                            }
                            levey.ViolatedRules.AddRange(ordered);
                            levey.IsOut2SD = aggIsOut2SD;
                            levey.IsOutRange = aggIsOutRange;

                            viewModelItem.isOutOfRange = levey.IsOutRange;
                            // map NSX/out-2SD flag if available
                            viewModelItem.isOut2SD = levey.IsOut2SD;

                            // store violated rules / error directly on the ResultReView
                            if (levey.ViolatedRules != null && levey.ViolatedRules.Count > 0)
                            {
                                var rulesText = string.Join(", ", levey.ViolatedRules);
                                viewModelItem.WestgardRule = rulesText;
                                // primary storage for detected error on UI item
                                // optional: show in comment column too
                            }
                            else
                            {
                                viewModelItem.WestgardRule = null;
                            }
                        }
                        catch (Exception exEval)
                        {
                            // Do not block loading on evaluation errors; log/show warning
                            MessageBox.Show($"Lỗi khi kiểm tra Westgard cho {item.IdTestNavigation.Name}: {exEval.Message}", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }

                        ResutlViewList.Add(viewModelItem);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Load ReResult failed: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            });

            CancelCommand = new RelayCommand<Result>((p) => true, (p) => Cancel());
            DeleteCommand = new RelayCommand<Result>((p) => true, (p) => Delete());

            SaveCommand = new RelayCommand<ControlInfoDetail>((p) =>
            {
                if (ResutlViewList == null) return false;
                else return true;

            }, (p) =>
            {
                var indexList = DataProvider.Ins.DB.Results
                    .Where(s => s.IdDevice == reResultGroup.IdDevice && s.DateRun.Date == Date && s.IdLevelNavigation.Id == reResultGroup.IdLevel)
                    .GroupBy(s => s.IndexQc)
                    .Select(s => s.Key).ToList();

                if (indexList == null || indexList.Count() == 0)
                {
                    Index = 1;
                }
                else
                {
                    Index = (int)(indexList.Max() + 1);
                }

                var results = new ObservableCollection<Result>();

                foreach (var item in ResutlViewList)
                {
                    if (!string.IsNullOrEmpty(item.TempResult))
                    {
                        // find matching original ReResult by id (id was assigned during Load)
                        var original = Results?.FirstOrDefault(r => r.Id == item.id);

                        var runDate = original != null
                            ? CombineDateAndTime(original.Date, original.Time)
                            : CombineDateAndTime(Date, DateTime.Now.TimeOfDay);

                        var result = new Result()
                        {
                            IdTest = item.idTest,
                            ResultType = item.ResultType,
                            IdTestNavigation = item.Test,
                            IdDevice = reResultGroup.IdDevice,
                            IdLevel = reResultGroup.IdLevel,
                            DateRun = runDate, // <-- full date+time stored now
                            Time = runDate.TimeOfDay,
                            IdUser = UserManager.Instance.CurrentUser.Id,
                            IndexQc = Index,
                            IdControlDetail = item.IdControlDetailNavigation.Id,
                            IdControlDetailNavigation = item.IdControlDetailNavigation,
                            Comment = item.Comment,
                            IsOutRange = item.isOutOfRange,
                            TempResult = item.TempResult,
                            WestgardRule = string.IsNullOrWhiteSpace(item.WestgardRule) ? null : item.WestgardRule,
                            // Mark newly-detected problematic results as not-corrected; otherwise leave null
                            IsCorrected = (!string.IsNullOrWhiteSpace(item.WestgardRule) || item.isOutOfRange) ? (bool?)false : null
                        };

                        // Compute numeric Result1 and ZScore here (do not rely on VM transfer)
                        if (result.ResultType == 2)
                        {
                            if (double.TryParse(item.TempResult, System.Globalization.NumberStyles.Float | System.Globalization.NumberStyles.AllowThousands, System.Globalization.CultureInfo.CurrentCulture, out var parsed))
                            {
                                result.Result1 = parsed;
                                var ctrl = item.IdControlDetailNavigation;
                                if (ctrl != null && ctrl.CurMean.HasValue && ctrl.CurSd.HasValue && ctrl.CurSd.Value != 0)
                                {
                                    result.ZScore = Math.Round((parsed - ctrl.CurMean.Value) / ctrl.CurSd.Value, 2);
                                }
                                else
                                {
                                    result.ZScore = null;
                                }
                            }
                            else
                            {
                                result.Result1 = null;
                                result.ZScore = null;
                            }
                        }
                        else
                        {
                            // qualitative: ensure Result1/ZScore are null
                            result.Result1 = null;
                            result.ZScore = null;
                        }

                        results.Add(result);
                    }
                }

                if (results.Count == 0)
                {
                    MessageBox.Show("Chưa nhập kết quả QC", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else
                {
                    SaveAsync(results);
                }
            });
        }

        private async Task SaveAsync(ObservableCollection<Result> results)
        {
            var db = DataProvider.Ins.DB;

            // Do NOT re-evaluate Westgard rules here.
            // Rely on the pre-computed WestgardRule placed on Result (from ResultReView) and the IsOutRange flag.
            foreach (var r in results)
            {
                if (!string.IsNullOrEmpty(r.WestgardRule) || r.IsOutRange == true)
                {
                    // Keep detection made during Load; mark as not-corrected so downstream logic is consistent.
                    r.IsCorrected = false;
                }

                // Leave other fields (ZScore, IsOutRange, etc.) as provided by the UI mapping.
            }

            // Persist results directly
            bool isSaved = await SaveDataAsync(db, results);

            if (isSaved)
            {
                try
                {
                    db.ReResults.RemoveRange(Results);
                    await db.SaveChangesAsync();
                    Results.Clear();

                    // Invalidate cache so subsequent loads get fresh history
                    ClearHistoryCache();

                    MessageBox.Show("Lưu kết quả thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    _window.DialogResult = true;
                    _window.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lưu thành công nhưng lỗi khi xoá ReResults/refresh: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            else
            {
                MessageBox.Show("Lưu dữ liệu thất bại. Vui lòng thử lại.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public async Task<bool> SaveDataAsync(QcManagmentContext DB, ObservableCollection<Result> results, bool createInternalErrors = true)
        {
            try
            {
                // Persist results (ensure Ids exist for InternalError linking)
                DB.AddRange(results);
                await DB.SaveChangesAsync();

                if (createInternalErrors)
                {
                    try
                    {
                        var problematic = results
                            .Where(r => (r.IsOutRange == true) || !string.IsNullOrEmpty(r.WestgardRule))
                            .ToList();

                        if (problematic.Any())
                        {
                            var newErrors = new List<InternalError>();

                            foreach (var r in problematic)
                            {
                                // avoid duplicate InternalError for same Result
                                var exists = await DB.InternalErrors
                                    .AsNoTracking()
                                    .AnyAsync(i => i.ErroneousResultId == r.Id);

                                if (exists) continue;

                                var cid = r.IdControlDetailNavigation;

                                var error = new InternalError
                                {
                                    ErroneousResultId = r.Id,
                                    TestId = r.IdTest,
                                    DeviceId = r.IdDevice,
                                    ControlInfoDetailId = r.IdControlDetail,
                                    Lot = cid?.Lot,
                                    WestgardDescription = !string.IsNullOrEmpty(r.WestgardRule) ? r.WestgardRule : (r.IsOutRange == true ? "Out-of-range" : null),
                                    RelatedResultsJson = JsonSerializer.Serialize(new { r.Id, r.IdTest, r.TempResult }),
                                    IsResolved = false,
                                    // Vietnamese status
                                    Status = "Đang chờ",
                                    CreatedAt = DateTime.Now,
                                    CreatedBy = UserManager.Instance?.CurrentUser?.DisplayName ?? UserManager.Instance.CurrentUser?.Id.ToString()
                                };

                                newErrors.Add(error);
                            }

                            if (newErrors.Any())
                            {
                                DB.InternalErrors.AddRange(newErrors);
                                await DB.SaveChangesAsync();
                            }
                        }
                    }
                    catch (Exception exErr)
                    {
                        // Non-blocking: warn user but keep overall save successful
                        MessageBox.Show($"Tạo bản ghi lỗi nội kiểm thất bại: {exErr.Message}", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Có lỗi:{ex}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        private void Cancel()
        {
            _window.DialogResult = false;
            _window.Close();
        }

        private async void Delete()
        {
            var result = MessageBox.Show("Bạn có chắc chắn muốn xóa tất cả dữ liệu ReResult không?", "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    DataProvider.Ins.DB.ReResults.RemoveRange(Results);
                    await DataProvider.Ins.DB.SaveChangesAsync();
                    Results.Clear();

                    // Invalidate cache because underlying history changed
                    ClearHistoryCache();

                    MessageBox.Show("Xóa tất cả dữ liệu ReResult thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Close the window
                    _window.DialogResult = true;
                    _window.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Có lỗi khi xóa dữ liệu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // helper to combine date + timespan safely
        private DateTime CombineDateAndTime(DateTime date, TimeSpan time)
        {
            return date.Date.Add(time);
        }

        // New: Evaluate Westgard for a single UI item using per-rule EvaluateSingleRule to avoid expansion overhead.
        public async Task CheckWestgardForItemAsync(ResultReView item)
        {
            if (item == null) return;

            try
            {
                int testId = item.idTest;
                int deviceId = _deviceId;      // captured from group
                int levelId = IdLevel;         // captured from group

                // load cached history (newest-first)
                var (sameLevelPrev, crossLevelPrev) = await GetRecentHistoryAsync(testId, deviceId, levelId, take: 10);

                // build a temporary current Result
                TimeSpan groupTime = Time ?? TimeSpan.Zero;

                var dateRun = CombineDateAndTime(Date, groupTime);

                var current = new Result
                {
                    IdTest = testId,
                    ResultType = item.ResultType,
                    IdTestNavigation = item.Test,
                    IdDevice = deviceId,
                    IdLevel = levelId,
                    DateRun = dateRun,
                    Time = dateRun.TimeOfDay,
                    IdUser = UserManager.Instance?.CurrentUser?.Id ?? 0,
                    IndexQc = Index,
                    IdControlDetail = item.IdControlDetailNavigation?.Id,
                    IdControlDetailNavigation = item.IdControlDetailNavigation,
                    TempResult = item.TempResult
                };

                // compute ZScore if quantitative
                if (current.ResultType == 2)
                {
                    if (double.TryParse(item.TempResult, out var parsed))
                    {
                        current.Result1 = parsed;
                        var ctrl = item.IdControlDetailNavigation;
                        if (ctrl != null && ctrl.CurMean.HasValue && ctrl.CurSd.HasValue && ctrl.CurSd.Value != 0)
                        {
                            current.ZScore = Math.Round((parsed - ctrl.CurMean.Value) / ctrl.CurSd.Value, 2);
                        }
                        else current.ZScore = null;
                    }
                    else current.ZScore = null;
                }
                else current.ZScore = null;

                // load per-device/test enabled rules (DeviceTest.WestgardRulesJson)
                IEnumerable<string>? enabled = null;
                try
                {
                    using var db2 = new QcManagmentContext();
                    var json = await db2.DeviceTests
                                        .AsNoTracking()
                                        .Where(d => d.IdDevice == deviceId && d.IdTest == testId)
                                        .Select(d => d.WestgardRulesJson)
                                        .FirstOrDefaultAsync();

                    if (!string.IsNullOrWhiteSpace(json))
                    {
                        try
                        {
                            var parsed = JsonSerializer.Deserialize<List<string>>(json);
                            if (parsed != null && parsed.Count > 0)
                                enabled = parsed.Select(s => s?.Trim()).Where(s => !string.IsNullOrEmpty(s)).ToList();
                            else
                                enabled = null; // empty -> treat as "all rules"
                        }
                        catch
                        {
                            enabled = null;
                        }
                    }
                }
                catch
                {
                    enabled = null;
                }

                // decide keys to evaluate; if enabled==null evaluate canonical main keys
                var keysToEvaluate = (enabled != null)
                    ? enabled.ToList()
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
                        // non-fatal per-rule errors: continue
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

                // update UI item
                item.isOutOfRange = levey.IsOutRange;
                item.isOut2SD = levey.IsOut2SD;
                item.WestgardRule = levey.ViolatedRules != null && levey.ViolatedRules.Count > 0
                    ? string.Join(", ", levey.ViolatedRules)
                    : null;
            }
            catch (Exception ex)
            {
                // non-blocking: show minimal warning to keep UI responsive
                MessageBox.Show($"Lỗi khi kiểm tra Westgard: {ex.Message}", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}