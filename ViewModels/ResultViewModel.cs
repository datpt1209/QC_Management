using QC_Management.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;

namespace QC_Management.ViewModels
{
    public class ResultViewModel : BaseViewModel
    {     

        private ObservableCollection<Result> _List;
        public ObservableCollection<Result> List { get => _List; set { _List = value; OnPropertyChanged(); } }

        private ObservableCollection<ResultView> _ResutlViewList;
        public ObservableCollection<ResultView> ResutlViewList { get => _ResutlViewList; set { _ResutlViewList = value; OnPropertyChanged(); } }

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

        public ICommand AddCommand { get; set; }
        public ICommand InputCommand { get; set; }
        public ICommand LoadedCommand { get; set; }
        public ICommand DateChangedCommand { get; set; }
        public ICommand CheckRangeCommand { get; set; }

        private ResultView _SelectedItem;
        public ResultView SelectedItem
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

        public ResultViewModel()
        {
            
            QcManagmentContext DB = DataProvider.Ins.DB;
            LoadedCommand = new RelayCommand<ControlInfoDetail>((p) =>
            {
                return true;

            }, (p) =>
            {
                LoadNew(DB);
            });


            DateChangedCommand = new RelayCommand<ControlInfoDetail>((p) =>
            {
                if (SelectedDevice == null || SelectedLevel == null) return false;
                else return true;

            }, (p) =>
            {
                var list = List.Where(s => s.IdDevice == SelectedDevice.Id && s.DateRun == SelectedDate && s.IdLevel == SelectedLevel.Id).GroupBy(s => s.IndexQc).Select(s => s.Key);
                if(list != null)
                {
                   IndexList = list.ToList();
                }
            });

            CheckRangeCommand = new RelayCommand<ControlInfoDetail>((p) =>
            {
               return true;

            }, (p) =>
            {
                isOutOfRange = SelectedItem.isOutOfRange;
            });

            InputCommand = new RelayCommand<ControlInfoDetail>((p) =>
            {
                if (SelectedDevice == null || SelectedLevel == null) return false;
                else
                    return true;
            }, (p) =>
            {
                IndexList = new List<int?>();
                int index = 0;
                var indexList = List.Where(s => s.IdDevice == SelectedDevice.Id && s.DateRun == SelectedDate && s.IdLevel == SelectedLevel.Id).GroupBy(s => s.IndexQc).Select(s => s.Key).ToList();
                if (indexList == null || indexList.Count() == 0)
                {
                    IndexList.Add(1);
                    SelectedIndex = (int)IndexList[index];
                    index++;
                }
                else
                {
                    foreach(var item in indexList)
                    {
                        IndexList.Add(item);
                        index++;
                    }
                    IndexList.Add(index + 1);
                    SelectedIndex = (int)IndexList[index];
                }

                ResutlViewList = new ObservableCollection<ResultView>();
                var view = TestList.Where(s => s.IdDevice == SelectedDevice.Id).Select(s => s.IdTestNavigation).OrderBy(s => s.Index).ToList();
                foreach (var item in view)
                {
                    var qcInfor = item.ControlInfoDetails.Where(s =>
                    s.IdLevel == SelectedLevel.Id 
                    && s.Status == true
                    && s.IdDevice == SelectedDevice.Id).FirstOrDefault();
                    if (qcInfor == null)
                    {
                        MessageBox.Show($"Không tìm thấy thông tin QC {item.Name}", "Thông báo", MessageBoxButton.OK);
                    }
                    else
                    {
                        ResutlViewList.Add(new ResultView()
                        {
                            Result = null,
                            TestName = item.Name,
                            idTest = item.Id,
                            QCName = qcInfor.IdControlInfoNavigation.Name,
                            LOT = qcInfor.IdControlInfoNavigation.Lot,
                            Mean = qcInfor.MeanNsx,
                            Sd = qcInfor.SdNsx,
                            Max = qcInfor.MeanNsx + 2 * qcInfor.SdNsx,
                            Min = qcInfor.MeanNsx - 2 * qcInfor.SdNsx,
                            IdControlDetailNavigation = qcInfor
                        }) ;
                    }

                }
            });

            AddCommand = new RelayCommand<ControlInfoDetail>((p) =>
            {
                if (ResutlViewList == null) return false;
                else return true;

            }, (p) =>
            {
                var results = new ObservableCollection<Result>();
                foreach (var item in ResutlViewList)
                {
                    if (item.Result != null)
                    {
                        Result result = new Result()
                        {
                            
                            IdTest = item.idTest,
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
                            Result1 = (double)item.Result,
                        };
                        results.Add(result);
                    }
                }
                if (results.Count == 0)
                {
                    MessageBox.Show("Chưa nhập kết quả QC", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else
                {
                    try
                    {
                        DB.AddRange(results);
                        DB.SaveChanges();
                        MessageBox.Show("Lưu kết quả thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                        LoadNew(DB);

                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Có lỗi:{ex}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    }

                }
            });
        }
        public void LoadNew(QcManagmentContext DB)
        {
            List = new ObservableCollection<Result>(DB.Results);
            TestList = new ObservableCollection<DeviceTest>(DB.DeviceTests);
            LevelList = new ObservableCollection<LevelQc>(DB.LevelQcs);
            DeviceList = new ObservableCollection<Device>(DB.Devices);
            ControlInfolList = new ObservableCollection<ControlInfo>(DB.ControlInfos);
            IndexList =new List<int?>();
            ResutlViewList = null;
            SelectedLevel = null;
            SelectedIndex = null;
        }
        public void ReLoad()
        {
         
        }

    }
}
