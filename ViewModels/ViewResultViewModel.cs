using Microsoft.EntityFrameworkCore;
using QC_Management.Models;
using QC_Management.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.AccessControl;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using XAct.Library.Settings;

namespace QC_Management.ViewModels
{
    public class ViewResultViewModel : BaseViewModel
    {
        private QcManagmentContext _dbContext;

        private ObservableCollection<CalResult>? _CalList;
        public ObservableCollection<CalResult>? CalList { get => _CalList; set { _CalList = value; OnPropertyChanged(); } }

        private ObservableCollection<Result> _ResultViewList;
        public ObservableCollection<Result> ResultViewList { get => _ResultViewList; set { _ResultViewList = value; OnPropertyChanged(); } }

        private ObservableCollection<Device> _DeviceList;
        public ObservableCollection<Device> DeviceList { get => _DeviceList; set { _DeviceList = value; OnPropertyChanged(); } }

        private List<int?> _IndexList;
        public List<int?> IndexList { get => _IndexList; set { _IndexList = value; OnPropertyChanged(); } }

        private List<LevelQc> _LevelList;
        public List<LevelQc> LevelList { get => _LevelList; set { _LevelList = value; OnPropertyChanged(); } }

        private string _SelectedResultType;
        public string SelectedResultType
        {
            get => _SelectedResultType;
            set
            {
                _SelectedResultType = value;
                OnPropertyChanged();
                UpdateDataGridSource();
            }
        }

        private Visibility _Visibility1;
        public Visibility Visibility1 { get => _Visibility1; set { _Visibility1 = value; OnPropertyChanged(); } }

        private Visibility _Visibility2;
        public Visibility Visibility2 { get => _Visibility2; set { _Visibility2 = value; OnPropertyChanged(); } }
        public ObservableCollection<string> ResultTypes { get; set; }
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

        public ICommand ViewCommand { get; set; }
        public ICommand LoadedCommand { get; set; }
        public ICommand PrintCommand { get; set; }
        public ICommand EditCommand { get; set; }
        public ICommand DeleteCommand { get; set; }
        public ICommand LevelChangedCommand { get; set; }
        public ICommand DateChangedCommand { get; set; }
        public ICommand DeleteOneQCResultCommand { get; set; }
        public ICommand DeleteOneCalResultCommand { get; set; }
        public ICommand ResultTypeChangedCommand { get; set; }
        public ICommand AddCommand { get; set; }
        public ICommand DeviceSelectionChangedCommand { get; set; }

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

        private Result _SelectedItem;
        public Result SelectedItem
        {
            get => _SelectedItem;
            set
            {
                _SelectedItem = value;
                OnPropertyChanged();
            }
        }

        private CalResult _SelectedCalResult;
        public CalResult SelectedCalResult
        {
            get => _SelectedCalResult;
            set
            {
                _SelectedCalResult = value;
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

        public ViewResultViewModel()
        {
            ResultTypes = new ObservableCollection<string> { "CALIB", "QC" };
            SelectedResultType = "QC";
            _dbContext = new QcManagmentContext();
            LoadedCommand = new RelayCommand<ControlInfoDetail>((p) =>
            {
                return true;

            }, async (p) =>
            {
                await LoadNew();
            });

            ViewCommand = new RelayCommand<ControlInfoDetail>((p) =>
            {
                if(SelectedResultType == "CALIB")
                {
                    if(SelectedDevice == null ) return false;
                    else
                        return true;
                }
                else
                {
                    if (SelectedDevice == null || SelectedLevel == null || SelectedIndex == null) return false;
                    else
                        return true;
                }
            }, (p) =>
            {
                if (SelectedResultType == "CALIB")
                {
                    CalList = new ObservableCollection<CalResult>(_dbContext.CalResults
                        .Include(s => s.IdDeviceNavigation)
                        .Include(s => s.IdTestNavigation)
                        .Include(s => s.IdUserNavigation)
                        .Include(s => s.IdCalDetailNavigation)
                        .Include(s => s.IdTestNavigation.IdUnitTableNavigation)
                        .Include(s => s.IdCalDetailNavigation.IdCalInforNavigation.IdCalTypeNavigation)
                        .Include(s => s.IdCalDetailNavigation.IdCalInforNavigation)
                        .Where(s => s.IdDevice == SelectedDevice.Id && s.DateRun == SelectedDate).ToList());
                    if (CalList.Count() == 0 || CalList == null)
                    {
                        MessageBox.Show("No data", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                else
                {
                    ResultViewList = new ObservableCollection<Result>(_dbContext.Results
                               .Include(s => s.IdTestNavigation)
                               .Include(s => s.IdUserNavigation)
                               .Include(s => s.IdLevelNavigation)
                               .Include(s => s.IdTestNavigation.IdUnitTableNavigation)
                               .Include(s => s.IdControlDetailNavigation.IdControlInfoNavigation)
                               .Where(s => s.IdDevice == SelectedDevice.Id
                    && s.IdLevel == SelectedLevel.Id
                    && s.DateRun == SelectedDate && s.IndexQc == SelectedIndex).ToList());
                    if (ResultViewList.Count() == 0 || ResultViewList == null)
                    {
                        MessageBox.Show("No data", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                
            });

            PrintCommand = new RelayCommand<object>((p) =>
            {
                if(SelectedResultType == "CALIB")
                {
                    return false;
                }
                else
                {
                    if (ResultViewList.Count == 0 || ResultViewList == null) return false;
                    else
                        return true;
                }
            }, (p) =>
            {
                if(SelectedResultType == "CALIB")
                {
                    CalibReportView rp = new CalibReportView(CalList.ToList());
                    rp.ShowDialog();
                }
                else
                {
                    ReivewReportView rp = new ReivewReportView(ResultViewList.ToList());
                    rp.ShowDialog();
                }
            });

            DeviceSelectionChangedCommand = new RelayCommand<object>((p) =>
            {
                if(SelectedResultType == "CALIB")
                {
                    return false;
                }
                else
                {
                    if (SelectedDevice == null ) return false;
                    else
                        return true;
                }
            }, async (p) =>
            {
                await UpdateLevelsByDeviceAsync(SelectedDevice.Id);

                if (SelectedLevel != null)
                {
                    IndexList = LoadIndexList(_dbContext);
                }
            });

            ResultTypeChangedCommand = new RelayCommand<object>((p) =>
            {
               if(SelectedDevice == null) return false;   
               return true;
               
            }, async (p) =>
            {
                 await UpdateLevelsByDeviceAsync(SelectedDevice.Id);
                
            });


            EditCommand = new RelayCommand<object>((p) =>
            {
                if(SelectedResultType == "CALIB")
                {
                    if (SelectedDevice == null || SelectedCalResult == null) return false;
                    else
                        return true;
                }
                else
                {
                    if (SelectedDevice == null || SelectedLevel == null || SelectedIndex == null ) return false;
                    else
                        return true;
                }
            }, (p) =>
            {
                if (SelectedResultType == "CALIB")
                {
                    foreach (var item in CalList)
                    {
                        var editResult = CalList.Where(s => s.Id == item.Id).FirstOrDefault();
                        editResult = item;
                    }
                    try
                    {
                        DataProvider.Ins.DB.SaveChanges();
                        MessageBox.Show("Cập nhật thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error: {ex}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Stop);
                    }

                }
                if(SelectedResultType == "QC")
                {
                    foreach (var item in ResultViewList)
                    {
                        var editResult = DataProvider.Ins.DB.Results.Where(s => s.Id == item.Id).FirstOrDefault();
                        if (editResult != null)
                        {
                            editResult.TempResult = item.TempResult;
                            editResult.Comment = item.Comment;
                            editResult.WestgardRule = item.WestgardRule;
                            editResult.IsExclude = item.IsExclude; // Cập nhật Exclude
                            editResult.Result1 = item.Result1;
                            editResult.IsOutRange = item.IsOutRange;
                            editResult.IsOutRangeNSX = item.IsOutRangeNSX;
                            editResult.QualitativeResult = item.QualitativeResult;
                        }
                    }
                    try
                    {
                        DataProvider.Ins.DB.SaveChanges();
                        MessageBox.Show("Cập nhật thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error: {ex}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Stop);
                    }
                }
                
            });

            AddCommand = new RelayCommand<Result>((p) => 
            {
                if (ResultViewList.Count == 0 || ResultViewList == null) return false;
                else
                    return true;
            }, 
            (p) => 
            { 
                OpenAddResultWindow(); 
            }
            );


            DeleteCommand = new RelayCommand<object>((p) =>
            {
                if (ResultViewList.Count == 0 || ResultViewList == null) return false;
                else
                    return true;

            }, (p) =>
            {
                var deleteItem = ResultViewList.ToList();
                MessageBoxResult result = MessageBox.Show($"Bạn có muốn xóa các kết quả máy: {SelectedDevice.Name}, Level: {SelectedLevel.Name}, Ngày: {SelectedDate.Date} Index: {SelectedIndex.ToString()}?", "Confirmation", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        _dbContext.RemoveRange(deleteItem);
                        _dbContext.SaveChanges();
                        MessageBox.Show("Xóa thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                        Reload();
                        FilterResults(_dbContext);

                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error: {ex}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Stop);
                    }
                }
                else return;

            });

            DeleteOneQCResultCommand = new RelayCommand<Result>((p) => 
            {
            if (SelectedItem == null) return false;
                else return true;
            }, (p) => 
            {
                DeleteQCResult(SelectedItem); 
            });

            DeleteOneCalResultCommand = new RelayCommand<CalResult>((p) =>
            {
                if (SelectedCalResult == null) return false;
                else return true;
            }, (p) =>
            {
                DeleteCalResult(SelectedCalResult);
            });

            LevelChangedCommand = new RelayCommand<ControlInfoDetail>((p) =>
            {
                IndexList = new List<int?>();
                if (SelectedDevice == null || SelectedLevel == null) return false;
                else return true;

            }, (p) =>
            {
                IndexList = LoadIndexList(_dbContext);
            });

            DateChangedCommand = new RelayCommand<ControlInfoDetail>((p) =>
            {
                return true;

            }, async (p) =>
            {
                SelectedLevel = null;
                if(SelectedDevice != null)
                {
                   await UpdateLevelsByDeviceAsync(SelectedDevice.Id);
                }
            });

        }
        private List<int?> LoadIndexList(QcManagmentContext DB)
        {
            var IndexList = new List<int?>();
            var listTest =  DB.Results.Where(s => s.IdDevice == SelectedDevice.Id
                            && s.DateRun == SelectedDate
                            && s.IdLevel == SelectedLevel.Id)
            .GroupBy(s => s.IndexQc).Select(s => s.Key).ToList();

            if (listTest != null)
            {
                IndexList = listTest;
            }
            return IndexList;
        }

        public async Task UpdateLevelsByDeviceAsync(int deviceId)
        {
            using (var dbContext = new QcManagmentContext())
            {
                var levels = await dbContext.Results
                                            .Where(c => c.IdDevice == deviceId && c.DateRun == SelectedDate.Date)
                                            .Select(c => new LevelQc
                                            {
                                                Id = c.IdLevel,
                                                Name = c.IdLevelNavigation.Name
                                            })
                                            .Distinct()
                                            .ToListAsync();
                LevelList = levels;
            }
        }

        private void AddNewResult()
        {
            var newResult = new Result
            {
                // Initialize with default values if needed
                DateRun = DateTime.Now,
                IdDevice = SelectedDevice?.Id ?? 0,
                IdLevel = SelectedLevel?.Id ?? 0,
                IndexQc = SelectedIndex
            };
            ResultViewList.Add(newResult);
        }

        private void OpenAddResultWindow()
        {
            QcManagmentContext DB = DataProvider.Ins.DB;
            var addResultWindow = new AddResultWindow();
            var viewModel = new AddResultViewModel(SelectedDate, SelectedDevice, SelectedLevel, SelectedIndex, addResultWindow);
            addResultWindow.DataContext = viewModel;
            if (addResultWindow.ShowDialog() == true)
            {
                Reload();
                FilterResults(DB);
            }
        }
        private async void DeleteQCResult(Result result)
        {
            if (result == null) return;

            var messageBoxResult = MessageBox.Show("Are you sure you want to delete this item?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (messageBoxResult == MessageBoxResult.No) return;
            // Remove from the database
            try
            {
                using (var context = new QcManagmentContext())
                {
                    var entity = context.Results.Find(result.Id);
                    if (entity != null)
                    {
                        context.Results.Remove(entity);
                        await context.SaveChangesAsync();
                    }
                }

                // Remove from the ObservableCollection
                ResultViewList.Remove(result);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting result: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void DeleteCalResult(CalResult calResult)
        {
            if (calResult == null) return;

            var messageBoxResult = MessageBox.Show("Are you sure you want to delete this item?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (messageBoxResult == MessageBoxResult.No) return;
            // Remove from the database
            try
            {
                using (var context = new QcManagmentContext())
                {
                    var entity = context.CalResults.Find(calResult.Id);
                    if (entity != null)
                    {
                        context.CalResults.Remove(entity);
                        await context.SaveChangesAsync();
                    }
                }
                // Remove from the ObservableCollection
                CalList.Remove(calResult);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting result: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void FilterResults(QcManagmentContext DB)
        {
            if (SelectedDevice == null || SelectedLevel == null ) return;

            if (SelectedIndex == 0)
            {
                ResultViewList = new ObservableCollection<Result>(DB.Results.Where(s => s.IdDevice == SelectedDevice.Id && s.IdLevel == SelectedLevel.Id && s.DateRun == SelectedDate).ToList());
            }
            else
            {
                ResultViewList = new ObservableCollection<Result>(DB.Results.Where(s => s.IdDevice == SelectedDevice.Id
                && s.IdLevel == SelectedLevel.Id
                && s.DateRun == SelectedDate && s.IndexQc == SelectedIndex).ToList());
            }

            if (ResultViewList.Count() == 0 || ResultViewList == null)
            {
                SelectedIndex = null;
                IndexList = LoadIndexList(DB);
                MessageBox.Show("No data", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        //public void LoadNew(QcManagmentContext DB)
        //{
        //    if(UserManager.Instance.CurrentUser.Role == 1)
        //    {
        //        IsVisibility = Visibility.Visible;
        //    }
        //    else
        //    {
        //        IsVisibility = Visibility.Hidden;
        //    }
        //    //List = new ObservableCollection<Result>();
        //    //ResultViewList = new ObservableCollection<Result>();
        //    //TestList = new ObservableCollection<DeviceTest>();
        //    DeviceList = new ObservableCollection<Device>(DB.Devices);
        //    //ControlInfolList = new ObservableCollection<ControlInfo>();
           
        //}

        private async Task LoadNew()
        {
            try
            {
                _dbContext = await Task.Run(() => DataProvider.Ins.DB);
                DeviceList = new ObservableCollection<Device>(_dbContext.Devices);
          
            }
            catch (Exception ex)
            {
                // Handle exceptions
            }
        }

        public void Reload()
        {
            QcManagmentContext DB = DataProvider.Ins.DB;
            ResultViewList = new ObservableCollection<Result>();
            CalList = new ObservableCollection<CalResult>();
        }
    }
}
