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

        private bool _isOutOfRange;
        public  bool isOutOfRange
        {
            get => _isOutOfRange;
            set
            {
                _isOutOfRange = SelectedItem.isOutOfRange;
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
            }
        }

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
                _SelectedDate = value;
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
                if(SelectedCalGroup == null) return false;
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
                isOutOfRange = SelectedItem.isOutOfRange;
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
                if (SelectedResultType == "CALIB")
                {
                if (SelectedDevice == null || SelectedCalType == null) return false;
                    else
                        return true;
                }
                else
                {
                    if (SelectedDevice == null || SelectedLevel == null) return false;
                    else
                        return true;
                }

            }, (p) =>
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

                    if(calDetails == null || calDetails.Count() == 0)
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
                    IndexList = new List<int?>();
                    //int index = 0;
                    var results = new ObservableCollection<Result>(DB.Results
                            //.AsNoTracking()
                            //.Include(s => s.IdControlDetailNavigation)
                            //.Include(s => s.IdUserNavigation)
                            //.Include(s => s.IdLevelNavigation)
                            //.Include(s => s.IdTestNavigation)
                            //.Include(s => s.IdDeviceNavigation)
                            //.Include(s => s.IdTestNavigation.IdUnitTableNavigation)
                            //.Include(s => s.IdControlDetailNavigation.IdControlInfoNavigation)
                            .Where(s => s.IdDevice == SelectedDevice.Id
                                       && s.DateRun == SelectedDate.Date
                                       && s.IdLevel == SelectedLevel.Id
                                       ));
                    List = results;

                    var indexList = List.Where(s => s.IdDevice == SelectedDevice.Id && s.DateRun == SelectedDate && s.IdLevel == SelectedLevel.Id)
                    .GroupBy(s => s.IndexQc).Select(s => s.Key).ToList();
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
                                Max = qcInfor.MeanApp + 3 * qcInfor.SdApp,
                                Min = qcInfor.MeanApp - 3 * qcInfor.SdApp,
                                IdControlDetailNavigation = qcInfor
                            });
                        }
                        else
                        {
                           MessageBox.Show($"Không tìm thấy giá trị {item.Name} cho {SelectedDevice.Name}", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                           return;
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

            var DB = DataProvider.Ins.DB;
            var results = new ObservableCollection<Result>();
            foreach (var item in ResutlViewList)
            {
                if (!string.IsNullOrEmpty(item.TempResult))
                {
                    Result result = new Result()
                    {
                        IdTest = item.idTest,
                        ResultType = item.ResultType,
                        IdTestNavigation = item.Test,
                        IdDevice = SelectedDevice.Id,
                        IdLevel = SelectedLevel.Id,
                        DateRun = SelectedDate,
                        Time = DateTime.Now.TimeOfDay,
                        IdUser = UserManager.Instance.CurrentUser.Id,
                        IndexQc = SelectedIndex,
                        IdControlDetail = item.IdControlDetailNavigation.Id,
                        IdControlDetailNavigation = item.IdControlDetailNavigation,
                        Comment = item.Comment,
                        IsOutRange = item.isOutOfRange,
                        TempResult = item.TempResult,
                    };
                    results.Add(result);
                }
            }

            bool isSaved = await SaveDataAsync(DB, results);
            return isSaved;
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
                    var calResult = new CalResult
                    {
                        IdDevice = SelectedDevice.Id,
                        IdCalDetail = item.CalDetailId,
                        IdTest = item.IdTest,
                        DateRun = SelectedDate,
                        Time = DateTime.Now.TimeOfDay,
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

        public async Task<bool> SaveDataAsync(QcManagmentContext DB, ObservableCollection<Result> results)
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

    }
}
