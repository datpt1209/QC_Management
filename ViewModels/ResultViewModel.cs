using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using QC_Management.Models;
using QC_Management.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;
using XAct.Library.Settings;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;
using System.Text.Json;
using QC_Management.Services;

namespace QC_Management.ViewModels
{
    public class ResultViewModel : BaseViewModel
    {
        private ObservableCollection<Result> _List;
        public ObservableCollection<Result> List { get => _List; set { _List = value; OnPropertyChanged(); } }

        private ObservableCollection<ResultReView> _ResutlViewList;
        public ObservableCollection<ResultReView>? ResutlViewList { get => _ResutlViewList; set { _ResutlViewList = value; OnPropertyChanged(); } }

        private ObservableCollection<CalResult>? _CalList;
        public ObservableCollection<CalResult>? CalList { get => _CalList; set { _CalList = value; OnPropertyChanged(); } }

        private ObservableCollection<CalType>? _CalTypeList;
        public ObservableCollection<CalType>? CalTypeList { get => _CalTypeList; set { _CalTypeList = value; OnPropertyChanged(); } }

        private ObservableCollection<Device> _DeviceList;
        public ObservableCollection<Device> DeviceList { get => _DeviceList; set { _DeviceList = value; OnPropertyChanged(); } }
        private List<int?> _IndexList;
        public List<int?> IndexList { get => _IndexList; set { _IndexList = value; OnPropertyChanged(); } }

        private List<LevelQc> _LevelList;
        public List<LevelQc> LevelList { get => _LevelList; set { _LevelList = value; OnPropertyChanged(); } }

        private ObservableCollection<ReResultGroup> _GroupedReResults;
        public ObservableCollection<ReResultGroup> GroupedReResults
        {
            get => _GroupedReResults;
            set { _GroupedReResults = value; OnPropertyChanged(); }
        }
        private ReResultGroup _SelectedReResultGroup;
        public ReResultGroup SelectedReResultGroup
        {
            get => _SelectedReResultGroup;
            set
            {
                _SelectedReResultGroup = value;
                OnPropertyChanged();
            }
        }

        private CalType? _SelectedCalType;
        public CalType? SelectedCalType
        {
            get => _SelectedCalType;
            set
            {
                _SelectedCalType = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<CalGroup> _CalGroupResult;
        public ObservableCollection<CalGroup> CalGroupResult
        {
            get => _CalGroupResult;
            set { _CalGroupResult = value; OnPropertyChanged(); }
        }
        private CalGroup _SelectedCalGroup;
        public CalGroup SelectedCalGroup
        {
            get => _SelectedCalGroup;
            set
            {
                _SelectedCalGroup = value;
                OnPropertyChanged();
            }
        }
        public ICommand ShowDetailCommand { get; set; }
        public ICommand ShowCalDetailCommand { get; set; }
        public ICommand AddCommand { get; set; }
        public ICommand InputCommand { get; set; }
        public ICommand LoadedCommand { get; set; }
        public ICommand CheckRangeCommand { get; set; }
        public ICommand DeviceSelectionChanged { get; set; }
        public ICommand OpenIncidentCommand { get; set; }

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

        private bool _isOutRange;
        public bool isOutRange
        {
            get => _isOutRange;
            set
            {
                _isOutRange = SelectedItem.isOutRange;
                OnPropertyChanged();
            }

        }

        private ObservableCollection<CalibInputViewModel> _CalibInputList;
        public ObservableCollection<CalibInputViewModel>? CalibInputList
        {
            get => _CalibInputList;
            set { _CalibInputList = value; OnPropertyChanged(); }
        }

        private int? _SelectedIndex;
        public int? SelectedIndex
        {
            get => _SelectedIndex;
            set
            {
                _SelectedIndex = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(SelectedIndexDisplay));
            }
        }

        // Display-only index text for UI (bind TextBox to this, non-editable)
        public string SelectedIndexDisplay => SelectedIndex?.ToString() ?? string.Empty;

        private CalibInputViewModel _SelectedCalibInput;
        public CalibInputViewModel SelectedCalibInput
        {
            get => _SelectedCalibInput;
            set
            {
                _SelectedCalibInput = value;
                OnPropertyChanged();
            }
        }

        private Device _SelectedDevice;
        public Device SelectedDevice
        {
            get => _SelectedDevice;
            set
            {
                _SelectedDevice = value;
                OnPropertyChanged();

                // Set visibility for Qualitative column.
                // Current rule: visible only when device name equals "UC" (case-insensitive).
                // If you prefer "contains" or "starts with" (e.g. "UC-01"), change Equals to Contains/StartsWith.
                if (_SelectedDevice != null && !string.IsNullOrWhiteSpace(_SelectedDevice.Name)
                    && _SelectedDevice.Id==21)
                {
                    ShowQualitative = Visibility.Visible;
                    WidthColum = 120;
                }
                else
                {
                    ShowQualitative = Visibility.Collapsed;
                    WidthColum = 0;
                }
            }
        }

        private LevelQc _SelectedLevel;
        public LevelQc? SelectedLevel
        {
            get => _SelectedLevel;
            set
            {
                _SelectedLevel = value;
                OnPropertyChanged();
            }
        }

        private DateTime _SelectedDate = DateTime.Now;
        public DateTime SelectedDate
        {
            get => _SelectedDate;
            set
            {
                _SelectedDate = value.Date;
                OnPropertyChanged();

                // keep SelectedDateTime in sync (preserve time)
                if (!_suppressSync)
                {
                    _suppressSync = true;
                    SelectedDateTime = _selectedDateTime.Date == default ? DateTime.Now.Date.Add(SelectedTime) : new DateTime(_SelectedDate.Year, _SelectedDate.Month, _SelectedDate.Day, SelectedDateTime.Hour, SelectedDateTime.Minute, 0);
                    _suppressSync = false;
                }
            }
        }

        private bool _suppressSync = false;

        // Combined selected date+time (used by TimePicker). Defaults to now.
        private DateTime _selectedDateTime = DateTime.Now;
        public DateTime SelectedDateTime
        {
            get => _selectedDateTime;
            set
            {
                if (_selectedDateTime == value) return;
                _selectedDateTime = value;
                OnPropertyChanged();

                if (!_suppressSync)
                {
                    _suppressSync = true;
                    // update SelectedDate and SelectedTime when SelectedDateTime changes
                    SelectedDate = _selectedDateTime.Date;
                    SelectedTime = _selectedDateTime.TimeOfDay;
                    _suppressSync = false;
                }
            }
        }

        // Expose SelectedTime (TimeSpan) for TimePicker binding if control expects TimeSpan
        private TimeSpan _selectedTime = DateTime.Now.TimeOfDay;
        public TimeSpan SelectedTime
        {
            get => _selectedTime;
            set
            {
                if (_selectedTime == value) return;
                _selectedTime = value;
                OnPropertyChanged();
            }
        }

        private string _SelectedResultType;
        public string SelectedResultType
        {
            get => _SelectedResultType;
            set
            {
                if (_SelectedResultType != value)
                {
                    _SelectedResultType = value;
                    OnPropertyChanged();
                    UpdateDataGridSource();
                }
            }
        }

        public ObservableCollection<string> ResultTypes { get; set; }

        private Visibility _Visibility1;
        public Visibility Visibility1 { get => _Visibility1; set { _Visibility1 = value; OnPropertyChanged(); } }

        private Visibility _Visibility2;
        public Visibility Visibility2 { get => _Visibility2; set { _Visibility2 = value; OnPropertyChanged(); } }

        // new property to control whether Qualitative column is visible
        private Visibility _showQualitative;
        public Visibility ShowQualitative
        {
            get => _showQualitative;
            set
            {
                if (_showQualitative == value) return;
                _showQualitative = value;
                OnPropertyChanged();
            }
        }
        // new property to control whether Qualitative column is visible
        private int _WidthColum;
        public int WidthColum
        {
            get => _WidthColum;
            set
            {
                if (_WidthColum == value) return;
                _WidthColum = value;
                OnPropertyChanged();
            }
        }

        // --- Inserted fields and helper types ---
        private readonly object _historyCacheLock = new();
        private readonly Dictionary<(int testId, int deviceId), CacheEntry> _historyCache = new();
        private readonly TimeSpan _historyCacheTtl = TimeSpan.FromSeconds(30); // adjust TTL as needed

        private class CacheEntry
        {
            public DateTimeOffset FetchedAt { get; set; }
            public List<Result> CrossLevelRecent { get; set; } = new();
        }

        public ResultViewModel()
        {
            QcManagmentContext DB = DataProvider.Ins.DB;
            ResultTypes = new ObservableCollection<string> { "CALIB", "QC" };
            SelectedResultType = "QC";
            GroupedReResults = new ObservableCollection<ReResultGroup>();
            ShowDetailCommand = new RelayCommand<object>((p) =>
            {
                if (SelectedReResultGroup == null) return false;
                else return true;
            }, (p) =>
            {
                OpenResultDetailWindow();
            });

            ShowCalDetailCommand = new RelayCommand<object>((p) =>
            {
                if (SelectedCalGroup == null) return false;
                else return true;
            }, (p) =>
            {
                OpenCalResultDetailWindow();
            });

            CalGroupResult = new ObservableCollection<CalGroup>();

            LoadedCommand = new RelayCommand<ControlInfoDetail>((p) =>
            {
                return true;

            }, (p) =>
            {
                LoadNew(DB);
                LoadReResults(DB);
                LoadCalGroup(DB);
            });

            CheckRangeCommand = new RelayCommand<ControlInfoDetail>((p) =>
            {
                return true;

            }, (p) =>
            {
                isOutRange = SelectedItem.isOutRange;
            });

            DeviceSelectionChanged = new RelayCommand<LevelQc>((p) =>
            {
                if (SelectedDevice == null) return false;
                else return true;

            }, async (p) =>
            {
                List<LevelQc> result = await GetLevelsByDeviceAsync(SelectedDevice.Id);
                LevelList = result;
            });


            InputCommand = new RelayCommand<ControlInfoDetail>((p) =>
            {
                // Adjusted: allow Load with only SelectedDevice for QC (SelectedLevel optional).
                if (SelectedResultType == "CALIB")
                {
                    if (SelectedDevice == null || SelectedCalType == null) return false;
                    else
                        return true;
                }
                else
                {
                    // only require device selected; date is always present
                    return SelectedDevice != null;
                }

            }, async (p) =>
            {
                if (SelectedResultType == "CALIB")
                {
                    if (SelectedDevice == null || SelectedCalType == null) return;

                    // Lấy các CalDetail theo Device và CalType
                    var calDetails = DB.CalDetails
                        .Include(cd => cd.IdTestNavigation)
                        .Include(cd => cd.IdCalInforNavigation)
                        .Where(cd => cd.IdDevice == SelectedDevice.Id
                                  && cd.IdCalInforNavigation.IdCalType == SelectedCalType.Id
                                  && cd.Status == true)
                        .ToList();

                    if (calDetails == null || calDetails.Count() == 0)
                    {
                        MessageBox.Show($"Không tìm thấy giá trị {SelectedCalType.CalTypeName} cho {SelectedDevice.Name}", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                        ReLoad();
                        return;
                    }

                    CalibInputList = new ObservableCollection<CalibInputViewModel>(
                        calDetails.Select(cd => new CalibInputViewModel
                        {
                            IdTest = cd.IdTest,
                            CalDetailId = cd.Id,
                            TestName = cd.IdTestNavigation.Name,
                            Lot = cd.IdCalInforNavigation.CalLot,
                            Level = cd.Level,
                            Min = cd.MinValue,
                            Max = cd.MaxValue,
                            Result = null // Để nhập kết quả
                        })
                    );

                }
                else
                {
                    // QC path: SelectedDevice required; SelectedLevel optional.
                    if (SelectedDevice == null) return;

                    // If level not chosen, try to auto-select first available level for device/date
                    if (SelectedLevel == null)
                    {
                        var levels = await GetLevelsByDeviceAsync(SelectedDevice.Id);
                        if (levels != null && levels.Any())
                        {
                            SelectedLevel = levels.First();
                            LevelList = levels;
                        }
                        else
                        {
                            MessageBox.Show($"Không tìm thấy Level hợp lệ cho thiết bị {SelectedDevice.Name} vào ngày {SelectedDate:d}.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                    }

                    IndexList = new List<int?>();

                    // Use a date range so we match any Result.DateRun on the selected day (time included)
                    var start = SelectedDate.Date;
                    var end = start.AddDays(1);

                    var results = new ObservableCollection<Result>(DB.Results
                            .Where(s => s.IdDevice == SelectedDevice.Id
                                       && s.DateRun >= start && s.DateRun < end
                                       && s.IdLevel == SelectedLevel.Id
                                       ));

                    List = results;

                    // Build index list from the already-filtered results (no direct DateRun equality)
                    var indexList = results
                        .GroupBy(s => s.IndexQc)
                        .Select(s => s.Key).ToList();

                    if (indexList == null || indexList.Count() == 0)
                    {
                        IndexList.Add(1);
                        SelectedIndex = (int)IndexList[IndexList.Count() - 1];
                    }
                    else
                    {
                        foreach (var item in indexList)
                        {
                            IndexList.Add(item);
                        }
                        IndexList.Add(indexList.Max() + 1);
                        SelectedIndex = (int)IndexList[IndexList.Count() - 1];
                    }

                    ResutlViewList = new ObservableCollection<ResultReView>();
                    var view = DB.DeviceTests
                                    .Include(s => s.IdTestNavigation.ControlInfoDetails)
                                    .Where(s => s.IdDevice == SelectedDevice.Id)
                                    .Select(s => s.IdTestNavigation)
                                    .OrderBy(s => s.Index).ToList();
                    foreach (var item in view)
                    {
                        var qcInfor = item.ControlInfoDetails.Where(s =>
                        s.IdLevel == SelectedLevel.Id
                        && s.Status == true
                        && s.IdDevice == SelectedDevice.Id).FirstOrDefault();
                        if (qcInfor != null)
                        {
                            ResutlViewList.Add(new ResultReView()
                            {
                                ResultType = item.TestType,
                                QualitativeMean = qcInfor.QualitativeMean,
                                TempResult = null,
                                TestName = item.Name,
                                Test = item,
                                idTest = item.Id,
                                LOT = qcInfor.Lot,
                                MeanApp = qcInfor.CurMean.ToString(),
                                SdApp = qcInfor.CurSd,
                                MeanNSX = qcInfor.MeanNsx,
                                SdNSX = qcInfor.SdNsx,
                                Max = qcInfor.CurMean + 2 * qcInfor.SdApp,
                                Min = qcInfor.CurMean - 2 * qcInfor.SdApp,
                                IdControlDetailNavigation = qcInfor
                            });
                        }
                    }

                }
            });

            AddCommand = new RelayCommand<ControlInfoDetail>((p) =>
            {
                if (SelectedResultType == "CALIB")
                    return CalibInputList != null && CalibInputList.Any(x => x.Result != null);
                else
                    return ResutlViewList != null && ResutlViewList.Any(x => !string.IsNullOrEmpty(x.TempResult));
            }, async (p) =>
            {
                var DB = DataProvider.Ins.DB;
                bool isSaved = false;
                if (SelectedResultType == "CALIB")
                {
                    isSaved = await SaveCal();
                    if (isSaved)
                    {
                        MessageBox.Show("Lưu kết quả Calib thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                        ReLoad();
                    }
                    else
                    {
                        MessageBox.Show("Lưu dữ liệu Calib thất bại. Vui lòng thử lại.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }       
                else
                {
                    isSaved = await SaveQC();
                    if (isSaved)
                    {
                        MessageBox.Show("Lưu kết quả QC thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                        ReLoad();
                    }
                    else
                    {
                        MessageBox.Show("Lưu dữ liệu QC thất bại. Vui lòng thử lại.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            });

            //OpenIncidentCommand = new RelayCommand<ResultReView>((p) =>
            //{
            //    return p != null;
            //}, (p) =>
            //{
            //    OpenIncidentWindow(p);
            //});

            SelectedDateTime = DateTime.Now;               // default to now (date+time)
            SelectedDate = SelectedDateTime.Date;          // keep date in sync
            SelectedTime = SelectedDateTime.TimeOfDay;     // if you also use SelectedTime (TimeSpan)

            // default qualitative column hidden until a device selected
            ShowQualitative = Visibility.Collapsed;
        }

        private void UpdateDataGridSource()
        {
            if (SelectedResultType == "QC")
            {
                Visibility1 = Visibility.Visible;
                Visibility2 = Visibility.Collapsed;
            }
            else
            {
                Visibility1 = Visibility.Collapsed;
                Visibility2 = Visibility.Visible;
            }
        }


        public async Task<List<LevelQc>> GetLevelsByDeviceAsync(int deviceId)
        {
            using (var dbContext = new QcManagmentContext())
            {
                var levels = await dbContext.ControlInfoDetails
                                            .Where(c => c.IdDevice == deviceId && c.Status == true)
                                            .Select(c => new LevelQc
                                            {
                                                Id = c.IdLevel,
                                                Name = c.IdLevelNavigation.Name
                                            })
                                            .Distinct()
                                            .ToListAsync();
                return levels;
            }
        }
        private void LoadReResults(QcManagmentContext DB)
        {
            var reResults = DB.ReResults.Include(s => s.IdTestNavigation).ToList();
            var groupedResults = reResults
                .GroupBy(r => new { r.IdDevice, r.IdLevel, r.Date, r.Index })
                .Select(g => new ReResultGroup
                {
                    DeviceName = DB.Devices.FirstOrDefault(d => d.Id == g.Key.IdDevice)?.Name ?? "Unknown Device",
                    IdDevice = DB.Devices.FirstOrDefault(d => d.Id == g.Key.IdDevice)?.Id ?? 0,
                    LevelName = DB.LevelQcs.FirstOrDefault(l => l.Id == g.Key.IdLevel)?.Name ?? "Unknown Level",
                    IdLevel = DB.LevelQcs.FirstOrDefault(l => l.Id == g.Key.IdLevel)?.Id ?? 0,
                    Index = (int)g.Key.Index,
                    DateTime = g.Key.Date,
                    Time = g.FirstOrDefault()?.Time ?? TimeSpan.Zero, // Lấy thời gian đầu tiên trong nhóm
                    Results = new ObservableCollection<ReResult>(g.ToList())
                })
                .ToList();

            GroupedReResults = new ObservableCollection<ReResultGroup>(groupedResults);
        }

        private void LoadCalGroup(QcManagmentContext DB)
        {
            var reCalResults = DB.ReCalResults.Include(s => s.IdTestNavigation).ToList();
            var groupedCalResults = reCalResults
                .GroupBy(r => new { r.IdDevice, r.DateRun, r.IndexCal })
                .Select(g => new CalGroup
                {
                    DeviceName = DB.Devices.FirstOrDefault(d => d.Id == g.Key.IdDevice)?.Name ?? "Unknown Device",
                    Index = (int)g.Key.IndexCal,
                    DateRun = g.Key.DateRun.Value,
                    Time = g.FirstOrDefault()?.Time ?? TimeSpan.Zero, // Lấy thời gian đầu tiên trong nhóm
                    ReCalResults = new ObservableCollection<ReCalResult>(g.ToList())
                })
                .ToList();

            CalGroupResult = new ObservableCollection<CalGroup>(groupedCalResults);
        }

        private async Task<bool> SaveQC()
        {
            if (ResutlViewList == null || !ResutlViewList.Any(x => !string.IsNullOrEmpty(x.TempResult)))
            {
                MessageBox.Show("Chưa nhập kết quả QC", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (SelectedDevice == null || SelectedLevel == null)
            {
                MessageBox.Show("Thiết bị hoặc level chưa được chọn", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            var DB = DataProvider.Ins.DB;
            var results = new ObservableCollection<Result>();
            var mapping = new List<(Result result, ResultReView view)>();

            foreach (var item in ResutlViewList)
            {
                if (string.IsNullOrEmpty(item.TempResult)) continue;

                // Use user-selected date+time (SelectedDateTime) rather than current time
                var combinedDateTime = SelectedDateTime;

                var result = new Result
                {
                    IdTest = item.idTest,
                    ResultType = item.ResultType,
                    IdTestNavigation = item.Test,
                    IdDevice = SelectedDevice.Id,
                    IdLevel = SelectedLevel.Id,
                    DateRun = combinedDateTime,
                    Time = combinedDateTime.TimeOfDay,
                    IdUser = UserManager.Instance.CurrentUser.Id,
                    IndexQc = SelectedIndex,
                    IdControlDetail = item.IdControlDetailNavigation.Id,
                    IdControlDetailNavigation = item.IdControlDetailNavigation,
                    Comment = item.Comment,
                    TempResult = item.TempResult,
                };

                var ctrl = item.IdControlDetailNavigation;
                if (result.ResultType == 2)
                {
                    if (double.TryParse(item.TempResult, out var parsed))
                    {
                        try { result.Result1 = parsed; } catch { }
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
                        result.ZScore = null;
                    }
                }
                else
                {
                    if (ctrl != null && !string.IsNullOrEmpty(result.TempResult))
                    {
                        try
                        {
                            result.IsOutRange = !ctrl.IsQualitativeResultAcceptable(result.TempResult);
                        }
                        catch
                        {
                            result.IsOutRange = null;
                        }
                    }
                    else
                    {
                        result.IsOutRange = null;
                    }

                    result.ZScore = null;
                }

                // copy UI-detected Westgard state into entity prior to save
                result.WestgardRule = string.IsNullOrWhiteSpace(item.WestgardRule) ? null : item.WestgardRule;
                result.IsOutRange = item.isOutRange;

                result.IsCorrected = (!string.IsNullOrWhiteSpace(result.WestgardRule) || result.IsOutRange == true)
                    ? (bool?)false
                    : null;
                results.Add(result);
                mapping.Add((result, item));
            }

            if (!results.Any())
            {
                MessageBox.Show("Chưa có kết quả để lưu", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            // Persist results first so they have IDs for InternalError linking
            bool isSaved = await SaveDataAsync(DB, results);
            if (!isSaved)
            {
                return false;
            }

            // mark UI items as not-corrected for problematic ones so user sees status immediately
            foreach (var (r, view) in mapping)
            {
                if (r.IsOutRange == true || !string.IsNullOrEmpty(r.WestgardRule))
                {
                    view.isOutRange = (bool)r.IsOutRange;
                    view.WestgardRule = r.WestgardRule;
                }
            }

            // Invalidate history cache so subsequent checks use fresh data
            ClearHistoryCache();

            return true;
        }

        private async Task<bool> SaveCal()
        {
            if (CalibInputList == null || !CalibInputList.Any(x => x.Result != null))
            {
                MessageBox.Show("Chưa nhập kết quả Calib", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            var DB = DataProvider.Ins.DB;
            var calResults = new ObservableCollection<CalResult>();

            foreach (var item in CalibInputList)
            {
                if (item.Result != null)
                {
                    // Use user-selected date+time instead of current time
                    var combinedDateTime = SelectedDateTime;

                    var calResult = new CalResult
                    {
                        IdDevice = SelectedDevice.Id,
                        IdCalDetail = item.CalDetailId,
                        IdTest = item.IdTest,
                        DateRun = combinedDateTime,
                        Time = combinedDateTime.TimeOfDay,
                        Result = item.Result,
                        Comment = item.Comment,
                        IdUser = UserManager.Instance.CurrentUser.Id,
                        IndexCal = 1, // Cần bổ sung logic nếu cần
                        isOutOfRange = item.IsOutRange ?? false
                    };
                    calResults.Add(calResult);
                }
            }

            bool isSaved = await SaveCalibDataAsync(DB, calResults);
            return isSaved;
        }

        public async Task<bool> SaveDataAsync(QcManagmentContext DB, ObservableCollection<Result> results, bool createInternalErrors = true)
        {
            try
            {
                // Ensure AppliedMean/AppliedSd/AppliedAt set for new results before persisting
                var now = DateTime.UtcNow;
                foreach (var r in results)
                {
                    ControlInfoDetail ctrl = null;
                    if (r.IdControlDetailNavigation != null)
                    {
                        ctrl = r.IdControlDetailNavigation;
                        DB.Entry(ctrl).State = EntityState.Unchanged;
                    }
                    else if (r.IdControlDetail.HasValue)
                    {
                        ctrl = await DB.ControlInfoDetails.FindAsync(r.IdControlDetail.Value);
                        if (ctrl != null)
                        {
                            r.IdControlDetailNavigation = ctrl;
                            DB.Entry(ctrl).State = EntityState.Unchanged;
                        }
                    }

                    if (!r.AppliedAt.HasValue && ctrl != null)
                    {
                        double? meanToApply = ctrl.CurMean ?? ctrl.MeanApp ?? ctrl.MeanNsx;
                        double? sdToApply = ctrl.CurSd ?? ctrl.SdApp ?? ctrl.SdNsx;
                        if (meanToApply.HasValue && sdToApply.HasValue)
                        {
                            r.AppliedMean = meanToApply;
                            r.AppliedSd = sdToApply;
                            r.AppliedAt = now;

                            if (r.Result1.HasValue)
                            {
                                var sdVal = sdToApply.Value == 0 ? 0.0001 : sdToApply.Value;
                                r.ZScore = Math.Round((r.Result1.Value - meanToApply.Value) / sdVal, 4);
                            }
                        }
                    }
                }

                // Persist results
                DB.AddRange(results);
                await DB.SaveChangesAsync();

                if (createInternalErrors)
                {
                    try
                    {
                        var problematic = results.Where(r => (r.IsOutRange == true) || !string.IsNullOrEmpty(r.WestgardRule)).ToList();
                        if (problematic.Any())
                        {
                            var newErrors = new List<InternalError>();
                            foreach (var r in problematic)
                            {
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
                                    Status = "Đang chờ",
                                    CreatedAt = r.DateRun,
                                    CreatedBy = UserManager.Instance?.CurrentUser?.DisplayName ?? UserManager.Instance?.CurrentUser?.Id.ToString()
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

        public async Task<bool> SaveCalibDataAsync(QcManagmentContext DB, ObservableCollection<CalResult> results)
        {
            try
            {
                DB.AddRange(results);
                await DB.SaveChangesAsync();

                return true; // Trả về true nếu lưu thành công
            }
            catch (Exception ex)
            {
                // Xử lý lỗi nếu có
                MessageBox.Show($"Có lỗi:{ex}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return false; // Trả về false nếu lưu thất bại
            }
        }
        public void LoadNew(QcManagmentContext DB)
        {
            DeviceList = new ObservableCollection<Device>(DB.Devices);
            CalTypeList = new ObservableCollection<CalType>(DB.CalTypes);
        }

        private void OpenResultDetailWindow()
        {
            var resultDetailWindow = new Re_ResultDetailView();
            var viewModel = new Re_ResultDetailViewModel(SelectedReResultGroup, resultDetailWindow);
            resultDetailWindow.DataContext = viewModel;
            if (resultDetailWindow.ShowDialog() == true)
            {
                ReLoad();
            }
        }

        private void OpenCalResultDetailWindow()
        {
            var calresultDetailWindow = new Re_CalResultDetailView();
            var viewModel = new Re_CalResultDetailViewModel(SelectedCalGroup, calresultDetailWindow);
            calresultDetailWindow.DataContext = viewModel;
            if (calresultDetailWindow.ShowDialog() == true)
            {
                ReLoad();
            }
        }
        private void ReLoad()
        {
            QcManagmentContext DB = DataProvider.Ins.DB;
            IndexList = new List<int?>();
            ResutlViewList = null;
            CalibInputList = null;
            SelectedLevel = null;
            SelectedIndex = null;
            SelectedCalType = null;
            LoadReResults(DB);
            LoadCalGroup(DB);
        }
        // Add this method into the ResultViewModel class
        public async Task CheckWestgardForItemAsync(ResultReView item)
        {
            if (item == null) return;
            if (SelectedDevice == null) return;
            if (SelectedLevel == null) return;

            int testId = item.idTest;
            int deviceId = SelectedDevice.Id;
            int levelId = SelectedLevel.Id;

            // Load history (existing code)
            var (sameLevelPrev, crossLevelPrev) = await GetRecentHistoryAsync(testId, deviceId, levelId, take: 10);

            // Use user-selected date+time (SelectedDateTime) for the temporary current result
            var combinedDateTime = SelectedDateTime;

            var current = new Result
            {
                IdTest = item.idTest,
                ResultType = item.ResultType,
                IdTestNavigation = item.Test,
                IdDevice = deviceId,
                IdLevel = levelId,
                DateRun = combinedDateTime,
                Time = combinedDateTime.TimeOfDay,
                IdUser = UserManager.Instance.CurrentUser.Id,
                IndexQc = SelectedIndex,
                IdControlDetail = item.IdControlDetailNavigation?.Id,
                IdControlDetailNavigation = item.IdControlDetailNavigation,
                TempResult = item.TempResult
            };

            // compute ZScore if quantitative and numeric (existing logic)
            if (current.ResultType == 2)
            {
                if (double.TryParse(item.TempResult, out var parsed))
                {
                    current.Result1 = parsed;
                    var ctrl = item.IdControlDetailNavigation;
                    if (ctrl != null && ctrl.CurMean.HasValue && ctrl.CurSd.HasValue && ctrl.CurSd.Value != 0)
                    {
                        current.ZScore = Math.Round((parsed - ctrl.CurMean.Value) / ctrl.CurSd.Value, 4);
                    }
                    else current.ZScore = null;
                }
                else current.ZScore = null;
            }
            else current.ZScore = null;

            // Load per-device/test enabled rules from DeviceTest.WestgardRulesJson (if any)
            IEnumerable<string>? enabledRules = null;
            try
            {
                using var db = new QcManagmentContext();
                var dt = await db.DeviceTests
                                 .AsNoTracking()
                                 .Where(d => d.IdDevice == deviceId && d.IdTest == testId)
                                 .Select(d => new { d.WestgardRulesJson })
                                 .FirstOrDefaultAsync();

                if (dt != null && !string.IsNullOrWhiteSpace(dt.WestgardRulesJson))
                {
                    try
                    {
                        var parsed = System.Text.Json.JsonSerializer.Deserialize<List<string>>(dt.WestgardRulesJson);
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

            // Update UI item
            item.isOutRange = levey.IsOutRange;
            item.WestgardRule = levey.ViolatedRules.Count > 0 ? string.Join(", ", levey.ViolatedRules) : null;
        }

        // Clears the whole cache (call after saving new results or when device/level changes)
        public void ClearHistoryCache()
        {
            lock (_historyCacheLock)
            {
                _historyCache.Clear();
            }
        }

        // Get recent history (uses cache). Returns (sameLevelList, crossLevelList) newest-first.
        private async Task<(List<Result> sameLevel, List<Result> crossLevel)> GetRecentHistoryAsync(int testId, int deviceId, int levelId, int take = 10)
        {
            var key = (testId, deviceId);
            CacheEntry? entry = null;

            lock (_historyCacheLock)
            {
                if (_historyCache.TryGetValue(key, out var e))
                {
                    // check TTL
                    if (DateTimeOffset.UtcNow - e.FetchedAt < _historyCacheTtl)
                    {
                        entry = e;
                    }
                    else
                    {
                        // expired: remove so we will reload
                        _historyCache.Remove(key);
                    }
                }
            }

            if (entry == null)
            {
                try
                {
                    var DB = DataProvider.Ins.DB;
                    var recent = await DB.Results
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
                    // non-fatal: return empty lists
                    return (new List<Result>(), new List<Result>());
                }
            }

            var cross = entry.CrossLevelRecent ?? new List<Result>();
            var sameLevel = cross.Where(r => r.IdLevel == levelId).ToList();
            return (sameLevel, cross);
        }
    }
}