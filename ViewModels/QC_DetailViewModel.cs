using QC_Management.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace QC_Management.ViewModels
{
    public class QC_DetailViewModel : BaseViewModel
    {
        private ObservableCollection<ControlInfoDetail> _List;
        public ObservableCollection<ControlInfoDetail> List { get => _List; set { _List = value; OnPropertyChanged(); } }

        private ObservableCollection<ControlInfoDetail> _ListDB;
        public ObservableCollection<ControlInfoDetail> ListDB { get => _ListDB; set { _ListDB = value; OnPropertyChanged(); } }
        private ObservableCollection<Device> _DeviceList;
        public ObservableCollection<Device> DeviceList { get => _DeviceList; set { _DeviceList = value; OnPropertyChanged(); } }

        private ObservableCollection<ControlInfo> _ControlInfoList;
        public ObservableCollection<ControlInfo> ControlInfoList { get => _ControlInfoList; set { _ControlInfoList = value; OnPropertyChanged(); } }
        private ObservableCollection<ControlInfo> _ControlInfoListDB;
        public ObservableCollection<ControlInfo> ControlInfoListDB { get => _ControlInfoListDB; set { _ControlInfoListDB = value; OnPropertyChanged(); } }

        private ObservableCollection<Test> _TestList;
        public ObservableCollection<Test> TestList { get => _TestList; set { _TestList = value; OnPropertyChanged(); } }

        private ObservableCollection<LevelQc> _LevelList;
        public ObservableCollection<LevelQc> LevelList { get => _LevelList; set { _LevelList = value; OnPropertyChanged(); } }
        private ObservableCollection<DeviceTest> _DeviceTestList;
        public ObservableCollection<DeviceTest> DeviceTestList { get => _DeviceTestList; set { _DeviceTestList = value; OnPropertyChanged(); } }
        public ICommand AddCommand { get; set; }
        public ICommand EditCommand { get; set; }
        public ICommand DeleteCommand { get; set; }
        public ICommand LoadedCommand { get; set; }
        public ICommand QC_InfoSelectionChangedCommand { get; set; }
        public ICommand DeviceSelectionChangedCommand { get; set; }

        private double _MeanNSX;
        public double MeanNSX { get => _MeanNSX; set { _MeanNSX = value; OnPropertyChanged(); } }


        private double _SDNSX;
        public double SDNSX { get => _SDNSX; set { _SDNSX = value; OnPropertyChanged(); } }

        private double _MeanPXN;
        public double MeanPXN { get => _MeanPXN; set { _MeanPXN = value; OnPropertyChanged(); } }


        private double _SdPXN;
        public double SdPXN { get => _SdPXN; set { _SdPXN = value; OnPropertyChanged(); } }

        private string _LOT;
        public string LOT { get => _LOT; set { _LOT = value; OnPropertyChanged(); } }

        private ControlInfoDetail _SelectedItem;
        public ControlInfoDetail SelectedItem
        {
            get => _SelectedItem;
            set
            {
                _SelectedItem = value;
                OnPropertyChanged();
                if (SelectedItem != null)
                {
                    SelectedControlInfo = SelectedItem.IdControlInfoNavigation;
                    SelectedLevel = SelectedItem.IdLevelNavigation;
                    SelectedTest = SelectedItem.IdTestNavigation;
                    MeanNSX = SelectedItem.MeanNsx;
                    SDNSX = SelectedItem.SdNsx;
                    MeanPXN = (double)SelectedItem.MeanApp;
                    SdPXN = (double)SelectedItem.SdApp;
                    SelectedDevice = SelectedItem.IdDeviceNavigation;
                    LOT = SelectedItem.Lot;

                }
            }
        }

        private ControlInfo _SelectedControlInfo;
        public ControlInfo SelectedControlInfo
        {
            get => _SelectedControlInfo;
            set
            {
                _SelectedControlInfo = value;
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

        public QC_DetailViewModel()
        {

            AddCommand = new RelayCommand<ControlInfoDetail>((p) =>
             {
                 if (SelectedControlInfo == null || SelectedTest == null || SelectedLevel == null || MeanNSX == 0 || SDNSX == 0)
                     return false;
                 else
                 {
                     return true;
                 }

             }, (p) =>
             {
                 var QC_Infor = new ControlInfoDetail()
                 {
                     IdDevice = SelectedDevice.Id,
                     IdDeviceNavigation = SelectedDevice,
                     IdControlInfoNavigation = SelectedControlInfo,
                     IdControlInfo = SelectedControlInfo.Id,
                     IdLevelNavigation = SelectedLevel,
                     IdLevel = SelectedLevel.Id,
                     IdTestNavigation = SelectedTest,
                     IdTest = SelectedTest.Id,
                     MeanNsx = MeanNSX,
                     SdNsx = SDNSX,
                     MeanApp = MeanPXN,
                     SdApp = SdPXN,
                     Status = SelectedControlInfo.Status,
                     Lot = LOT
                 };

                 try
                 {
                     DataProvider.Ins.DB.ControlInfoDetails.Add(QC_Infor);
                     DataProvider.Ins.DB.SaveChanges();
                     MessageBox.Show("Thêm thông tin QC thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                     List.Add(QC_Infor);

                 }
                 catch (Exception ex)
                 {
                     MessageBox.Show($"Error: {ex}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Stop);
                 }
             });

            LoadedCommand = new RelayCommand<ControlInfoDetail>((p) =>
            {
                return true;
            }, (p) =>
            {
                LoadNew();
            });

            DeviceSelectionChangedCommand = new RelayCommand<Test>((p) =>
            {
                return true;
            }, (p) =>
            {
                ControlInfoList = new ObservableCollection<ControlInfo>(ControlInfoListDB);
                TestList = new ObservableCollection<Test>(DeviceTestList.Where(s => s.IdDevice == SelectedDevice.Id).Select(s => s.IdTestNavigation).OrderBy(s => s.Index));
                List = new ObservableCollection<ControlInfoDetail>(ListDB.Where(s => s.IdDevice == SelectedDevice.Id).ToList());
            });

            QC_InfoSelectionChangedCommand = new RelayCommand<Test>((p) =>
            {
                if (SelectedDevice == null || SelectedControlInfo == null) return false;
                else
                    return true;
            }, (p) =>
            {
                List = new ObservableCollection<ControlInfoDetail>(ListDB.Where(s => s.IdDevice == SelectedDevice.Id && s.IdControlInfo == SelectedControlInfo.Id).ToList());
            });

            EditCommand = new RelayCommand<ControlInfoDetail>((p) =>
            {
                if (SelectedItem == null)
                    return false;

                else if (
                SelectedItem.IdControlInfoNavigation == SelectedControlInfo 
                && SelectedItem.IdLevelNavigation == SelectedLevel 
                && SelectedItem.IdTestNavigation == SelectedTest 
                && SelectedItem.MeanNsx == MeanNSX 
                && SelectedItem.SdNsx == SDNSX
                && SelectedItem.MeanApp == MeanPXN
                && SelectedItem.SdApp == SdPXN
                && SelectedItem.Lot == LOT)
                    return false;

                return true;

            }, (p) =>
            {
                SelectedItem.IdControlInfo = SelectedControlInfo.Id;
                SelectedItem.IdControlInfoNavigation = SelectedControlInfo;
                SelectedItem.IdLevelNavigation = SelectedLevel;
                SelectedItem.IdLevel = SelectedLevel.Id;
                SelectedItem.IdTest = SelectedTest.Id;
                SelectedItem.MeanNsx = MeanNSX;
                SelectedItem.SdNsx = SDNSX;
                SelectedItem.MeanApp = MeanPXN;
                SelectedItem.SdApp = SdPXN;
                SelectedItem.Status = SelectedControlInfo.Status;
                SelectedItem.Lot = LOT;
                try
                {
                    DataProvider.Ins.DB.SaveChanges();
                    MessageBox.Show("Cập nhật thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    List =new ObservableCollection<ControlInfoDetail>(ListDB.Where(s =>
                    s.IdDevice == SelectedDevice.Id 
                    && s.IdControlInfo == SelectedControlInfo.Id).ToList());
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Stop);
                }
            });


            DeleteCommand = new RelayCommand<ControlInfo>((p) =>
            {
                if (SelectedItem == null)
                    return false;
                else
                {
                    return true;
                }
            }, (p) =>
            {
                var deleteItem = DataProvider.Ins.DB.ControlInfoDetails.Where(s => s.Id == SelectedItem.Id).FirstOrDefault();

                MessageBoxResult result = MessageBox.Show($"Bạn có muốn xóa thông tin QC: {SelectedItem.IdControlInfoNavigation.Name} LOT: {SelectedItem.IdControlInfoNavigation.Lot} Level: {SelectedItem.IdLevelNavigation.Name}  ?", "Confirmation", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        DataProvider.Ins.DB.Remove(deleteItem);
                        DataProvider.Ins.DB.SaveChanges();
                        MessageBox.Show("Xóa thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                        List.Remove(deleteItem);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error: {ex}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Stop);
                    }
                }
                else return;


            });

        }
        private void LoadNew()
        {
            List = new ObservableCollection<ControlInfoDetail> ();
            ListDB = new ObservableCollection<ControlInfoDetail>(DataProvider.Ins.DB.ControlInfoDetails);
            TestList = new ObservableCollection<Test>(DataProvider.Ins.DB.Tests);
            LevelList = new ObservableCollection<LevelQc>(DataProvider.Ins.DB.LevelQcs);
            ControlInfoListDB = new ObservableCollection<ControlInfo>(DataProvider.Ins.DB.ControlInfos);
            DeviceList = new ObservableCollection<Device>(DataProvider.Ins.DB.Devices);
            DeviceTestList = new ObservableCollection<DeviceTest>(DataProvider.Ins.DB.DeviceTests);

        }
        public void ReLoad()
        {


        }

    }
}
