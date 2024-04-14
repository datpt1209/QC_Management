using QC_Management.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

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
        public ICommand LevelChangedCommand { get; set; }
        public ICommand DateChangedCommand { get; set; }

        private bool _IsReadOnly;
        public bool IsReadOnly
        {
            get => _IsReadOnly;
            set
            {
                _IsReadOnly = value;
                OnPropertyChanged();
            }
        }

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

        private int _SelectedIndex;
        public int SelectedIndex
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

        private Test _SelectedTest;
        public Test SelectedTest
        {
            get => _SelectedTest;
            set
            {
                _SelectedTest = value;
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
                if (SelectedDevice == null || SelectedLevel == null||SelectedDate == null) return false;
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

            LevelChangedCommand = new RelayCommand<ControlInfoDetail>((p) =>
            {
                IndexList = new List<int?>();
                if (SelectedDevice == null || SelectedLevel == null) return false;
                else return true;

            }, (p) =>
            {
                var list = List.Where(s => s.IdDevice == SelectedDevice.Id 
                && s.DateRun == SelectedDate 
                && s.IdLevel == SelectedLevel.Id)
                .GroupBy(s => s.IndexQc).Select(s => s.Key);
                if (list != null)
                {
                    IndexList = list.ToList();
                }
            });

            DateChangedCommand = new RelayCommand<ControlInfoDetail>((p) =>
            {
                return true;

            }, (p) =>
            {
                SelectedLevel = null;
            });

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

    }
}
