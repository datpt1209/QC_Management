using QC_Management.Models;
using QC_Management.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.AccessControl;
using System.Windows;
using System.Windows.Input;
using XAct.Library.Settings;

namespace QC_Management.ViewModels
{
    public class ViewResultViewModel : BaseViewModel
    {
        private ObservableCollection<Result> _List;
        public ObservableCollection<Result> List { get => _List; set { _List = value; OnPropertyChanged(); } }

        private ObservableCollection<Result> _ResultViewList;
        public ObservableCollection<Result> ResultViewList { get => _ResultViewList; set { _ResultViewList = value; OnPropertyChanged(); } }

        private ObservableCollection<Device> _DeviceList;
        public ObservableCollection<Device> DeviceList { get => _DeviceList; set { _DeviceList = value; OnPropertyChanged(); } }

        private List<int?> _IndexList;
        public List<int?> IndexList { get => _IndexList; set { _IndexList = value; OnPropertyChanged(); } }

        private ObservableCollection<DeviceTest> _TestList;
        public ObservableCollection<DeviceTest> TestList { get => _TestList; set { _TestList = value; OnPropertyChanged(); } }

        private ObservableCollection<LevelQc> _LevelList;
        public ObservableCollection<LevelQc> LevelList { get => _LevelList; set { _LevelList = value; OnPropertyChanged(); } }

        private ObservableCollection<ControlInfo> _ControlInfoList;
        public ObservableCollection<ControlInfo> ControlInfolList { get => _ControlInfoList; set { _ControlInfoList = value; OnPropertyChanged(); } }
        private ObservableCollection<User> _UserList;
        public ObservableCollection<User> UserList { get => _UserList; set { _UserList = value; OnPropertyChanged(); } }
 
        public ICommand ViewCommand { get; set; }
        public ICommand LoadedCommand { get; set; }
        public ICommand PrintCommand { get; set; }
        public ICommand EditCommand { get; set; }
        public ICommand DeleteCommand { get; set; }
        public ICommand LevelChangedCommand { get; set; }
        public ICommand DateChangedCommand { get; set; }
        public ICommand DeleteOneTestCommand { get; set; }
        public ICommand AddCommand { get; set; }


        private Visibility _IsVisibility;
        public Visibility IsVisibility
        {
            get => _IsVisibility;
            set
            {
                _IsVisibility = value;
                OnPropertyChanged();
            }
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
        public LevelQc SelectedLevel
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
            QcManagmentContext DB = DataProvider.Ins.DB;

            LoadedCommand = new RelayCommand<ControlInfoDetail>((p) =>
            {
                return true;

            }, (p) =>
            {
                LoadNew(DB);
            });

            ViewCommand = new RelayCommand<ControlInfoDetail>((p) =>
            {
                if (SelectedDevice == null || SelectedLevel == null || SelectedIndex == null) return false;
                else
                    return true;

            }, (p) =>
            {
                if( SelectedIndex == 0)
                {
                    ResultViewList = new ObservableCollection<Result>(List.Where(s => s.IdDevice == SelectedDevice.Id && s.IdLevel == SelectedLevel.Id && s.DateRun == SelectedDate).ToList());
                   
                }
                else
                {
                    ResultViewList = new ObservableCollection<Result>(List.Where(s => s.IdDevice == SelectedDevice.Id 
                    && s.IdLevel == SelectedLevel.Id 
                    && s.DateRun == SelectedDate && s.IndexQc == SelectedIndex).ToList());
                }
                if (ResultViewList.Count() == 0 || ResultViewList == null)
                {
                    MessageBox.Show("No data", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            });

            PrintCommand = new RelayCommand<object>((p) =>
            {
                if (ResultViewList.Count == 0 || ResultViewList == null) return false;
                else
                    return true;
            }, (p) =>
            {
                //ReportView rp = new ReportView(ResultViewList);
                //rp.ShowDialog();
                ReivewReportView rp = new ReivewReportView(ResultViewList.ToList());
                rp.ShowDialog();

            });


            EditCommand = new RelayCommand<object>((p) =>
            {
                if (ResultViewList.Count == 0 || ResultViewList == null) return false;
                else
                    return true;

            }, (p) =>
            {
                foreach(var item in ResultViewList)
                {
                    var editResult = List.Where(s => s.Id == item.Id).FirstOrDefault();
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
                        DataProvider.Ins.DB.RemoveRange(deleteItem);
                        DataProvider.Ins.DB.SaveChanges();
                        MessageBox.Show("Xóa thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                        Reload(DB);
                        FilterResults();

                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error: {ex}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Stop);
                    }
                }
                else return;

            });

            DeleteOneTestCommand = new RelayCommand<Result>((p) => 
            {
                if (SelectedDevice == null || SelectedLevel == null || SelectedIndex == null || SelectedDate == null) return false;
                else return true;
            }, (p) => 
            {
                DeleteResult(SelectedItem); 
            });

            LevelChangedCommand = new RelayCommand<ControlInfoDetail>((p) =>
            {
                IndexList = new List<int?>();
                if (SelectedDevice == null || SelectedLevel == null) return false;
                else return true;

            }, (p) =>
            {
                IndexList = LoadIndexList();
            });

            DateChangedCommand = new RelayCommand<ControlInfoDetail>((p) =>
            {
                return true;

            }, (p) =>
            {
                SelectedLevel = null;
            });

        }

        private List<int?> LoadIndexList()
        {
            var IndexList = new List<int?>();
            var listTest = List.Where(s => s.IdDevice == SelectedDevice.Id
                            && s.DateRun == SelectedDate
                            && s.IdLevel == SelectedLevel.Id);

            var list = List.Where(s => s.IdDevice == SelectedDevice.Id
            && s.DateRun == SelectedDate
            && s.IdLevel == SelectedLevel.Id)
            .GroupBy(s => s.IndexQc).Select(s => s.Key);
            if (list != null)
            {
                IndexList = list.ToList();
            }
            return IndexList;
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
            var addResultWindow = new AddResultWindow();
            var viewModel = new AddResultViewModel(SelectedDate, SelectedDevice, SelectedLevel, SelectedIndex, addResultWindow);
            addResultWindow.DataContext = viewModel;
            if (addResultWindow.ShowDialog() == true)
            {
                Reload(DataProvider.Ins.DB);
                FilterResults();
            }
        }
        private async void DeleteResult(Result result)
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
        private void FilterResults()
        {
            if (SelectedDevice == null || SelectedLevel == null ) return;

            if (SelectedIndex == 0)
            {
                ResultViewList = new ObservableCollection<Result>(List.Where(s => s.IdDevice == SelectedDevice.Id && s.IdLevel == SelectedLevel.Id && s.DateRun == SelectedDate).ToList());
            }
            else
            {
                ResultViewList = new ObservableCollection<Result>(List.Where(s => s.IdDevice == SelectedDevice.Id
                && s.IdLevel == SelectedLevel.Id
                && s.DateRun == SelectedDate && s.IndexQc == SelectedIndex).ToList());
            }

            if (ResultViewList.Count() == 0 || ResultViewList == null)
            {
                SelectedIndex = null;
                IndexList = LoadIndexList();
               
                MessageBox.Show("No data", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        public void LoadNew(QcManagmentContext DB)
        {
            if(UserManager.Instance.CurrentUser.Role == 1)
            {
                IsVisibility = Visibility.Visible;
            }
            else
            {
                IsVisibility = Visibility.Hidden;
            }
            List = new ObservableCollection<Result>(DB.Results);
            ResultViewList = new ObservableCollection<Result>();
            TestList = new ObservableCollection<DeviceTest>(DB.DeviceTests);
            LevelList = new ObservableCollection<LevelQc>(DB.LevelQcs);
            DeviceList = new ObservableCollection<Device>(DB.Devices);
            ControlInfolList = new ObservableCollection<ControlInfo>(DB.ControlInfos);
            UserList = new ObservableCollection<User>(DB.Users);
        }
        public void Reload(QcManagmentContext DB)
        {
            List = new ObservableCollection<Result>(DB.Results);
            //SelectedIndex = null;
            //SelectedDevice = null;
            //SelectedDate = DateTime.Now;
            ResultViewList = new ObservableCollection<Result>();
        }
    }
}
