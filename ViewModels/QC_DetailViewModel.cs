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
        private List<ControlInfoDetail> _list;
        public List<ControlInfoDetail> List
        {
            get => _list;
            set => SetProperty(ref _list, value);
        }

        private ObservableCollection<ControlInfoDetail> _listDB;
        public ObservableCollection<ControlInfoDetail> ListDB
        {
            get => _listDB;
            set => SetProperty(ref _listDB, value);
        }

        private ObservableCollection<ControlType> _listType;
        public ObservableCollection<ControlType> ListType
        {
            get => _listType;
            set => SetProperty(ref _listType, value);
        }

        private ObservableCollection<Device> _deviceList;
        public ObservableCollection<Device> DeviceList
        {
            get => _deviceList;
            set => SetProperty(ref _deviceList, value);
        }

        private ObservableCollection<ControlInfo> _controlInfoList;
        public ObservableCollection<ControlInfo> ControlInfoList
        {
            get => _controlInfoList;
            set => SetProperty(ref _controlInfoList, value);
        }

        private ObservableCollection<ControlInfo> _controlInfoListDB;
        public ObservableCollection<ControlInfo> ControlInfoListDB
        {
            get => _controlInfoListDB;
            set => SetProperty(ref _controlInfoListDB, value);
        }

        private ObservableCollection<Test> _testList;
        public ObservableCollection<Test> TestList
        {
            get => _testList;
            set => SetProperty(ref _testList, value);
        }

        private ObservableCollection<LevelQc> _levelList;
        public ObservableCollection<LevelQc> LevelList
        {
            get => _levelList;
            set => SetProperty(ref _levelList, value);
        }

        private ObservableCollection<DeviceTest> _deviceTestList;
        public ObservableCollection<DeviceTest> DeviceTestList
        {
            get => _deviceTestList;
            set => SetProperty(ref _deviceTestList, value);
        }

        public ICommand AddCommand { get; set; }
        public ICommand EditCommand { get; set; }
        public ICommand DeleteCommand { get; set; }
        public ICommand LoadedCommand { get; set; }
        public ICommand QC_InfoSelectionChangedCommand { get; set; }
        public ICommand DeviceSelectionChangedCommand { get; set; }
        public ICommand QCTypeSelectionChangedCommand { get; set; }

        private double _meanNSX;
        public double MeanNSX
        {
            get => _meanNSX;
            set => SetProperty(ref _meanNSX, value);
        }

        private ControlType? _selectedType;
        public ControlType? SelectedType
        {
            get => _selectedType;
            set => SetProperty(ref _selectedType, value);
        }

        private double _sdNSX;
        public double SDNSX
        {
            get => _sdNSX;
            set => SetProperty(ref _sdNSX, value);
        }

        private double _meanPXN;
        public double MeanPXN
        {
            get => _meanPXN;
            set => SetProperty(ref _meanPXN, value);
        }

        private double _curMean;
        public double CurMean
        {
            get => _curMean;
            set => SetProperty(ref _curMean, value);
        }

        private double _curSd;
        public double CurSd
        {
            get => _curSd;
            set => SetProperty(ref _curSd, value);
        }

        private bool _isChecked;
        public bool IsChecked
        {
            get => _isChecked;
            set => SetProperty(ref _isChecked, value);
        }

        private double _sdPXN;
        public double SdPXN
        {
            get => _sdPXN;
            set => SetProperty(ref _sdPXN, value);
        }

        private string _lot;
        public string LOT
        {
            get => _lot;
            set => SetProperty(ref _lot, value);
        }

        private ControlInfoDetail _selectedItem;
        public ControlInfoDetail SelectedItem
        {
            get => _selectedItem;
            set
            {
                if (SetProperty(ref _selectedItem, value) && _selectedItem != null)
                {
                    UpdateSelectedItemDetails();
                }
            }
        }

        private ControlInfo _selectedControlInfo;
        public ControlInfo SelectedControlInfo
        {
            get => _selectedControlInfo;
            set => SetProperty(ref _selectedControlInfo, value);
        }

        private Device _selectedDevice;
        public Device SelectedDevice
        {
            get => _selectedDevice;
            set => SetProperty(ref _selectedDevice, value);
        }

        private Test _selectedTest;
        public Test SelectedTest
        {
            get => _selectedTest;
            set => SetProperty(ref _selectedTest, value);
        }

        private LevelQc _selectedLevel;
        public LevelQc SelectedLevel
        {
            get => _selectedLevel;
            set => SetProperty(ref _selectedLevel, value);
        }

        public QC_DetailViewModel()
        {
            InitializeCommands();
        }

        private void InitializeCommands()
        {
            AddCommand = new RelayCommand<ControlInfoDetail>(CanAdd, Add);
            EditCommand = new RelayCommand<ControlInfoDetail>(CanEdit, Edit);
            DeleteCommand = new RelayCommand<ControlInfo>(CanDelete, Delete);
            LoadedCommand = new RelayCommand<ControlInfoDetail>(_ => true, _ => LoadNew());
            DeviceSelectionChangedCommand = new RelayCommand<Test>(_ => true, _ => UpdateTestList());
            QC_InfoSelectionChangedCommand = new RelayCommand<Test>(CanChangeQCInfo, _ => UpdateList());
            QCTypeSelectionChangedCommand = new RelayCommand<ControlInfo>(CanChangeQCType, _ => UpdateControlInfoList());
        }

        private bool CanAdd(ControlInfoDetail p) =>
            SelectedControlInfo != null && SelectedTest != null && SelectedLevel != null && MeanNSX != 0 && SDNSX != 0;

        private void Add(ControlInfoDetail p)
        {
            var qcInfo = new ControlInfoDetail
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
                CurSd = CurSd,
                CurMean = CurMean,
                Lot = LOT
            };

            try
            {
                DataProvider.Ins.DB.ControlInfoDetails.Add(qcInfo);
                DataProvider.Ins.DB.SaveChanges();
                MessageBox.Show("Thêm thông tin QC thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                RefreshLists();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Stop);
            }
        }

        private bool CanEdit(ControlInfoDetail p) =>
            SelectedItem != null &&
            (SelectedItem.IdControlInfoNavigation != SelectedControlInfo ||
            SelectedItem.IdLevelNavigation != SelectedLevel ||
            SelectedItem.IdTestNavigation != SelectedTest ||
            SelectedItem.MeanNsx != MeanNSX ||
            SelectedItem.SdNsx != SDNSX ||
            SelectedItem.MeanApp != MeanPXN ||
            SelectedItem.SdApp != SdPXN ||
            SelectedItem.Lot != LOT ||
            SelectedItem.Status != IsChecked ||
            SelectedItem.CurMean != CurMean ||
            SelectedItem.CurSd != CurSd);

        private void Edit(ControlInfoDetail p)
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
            SelectedItem.Status = IsChecked;
            SelectedItem.CurMean = CurMean;
            SelectedItem.CurSd = CurSd;

            try
            {
                DataProvider.Ins.DB.SaveChanges();
                MessageBox.Show("Cập nhật thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                RefreshLists();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Stop);
            }
        }

        private bool CanDelete(ControlInfo p) => SelectedItem != null;

        private void Delete(ControlInfo p)
        {
            var deleteItem = DataProvider.Ins.DB.ControlInfoDetails.FirstOrDefault(s => s.Id == SelectedItem.Id);

            if (deleteItem == null) return;

            var result = MessageBox.Show($"Bạn có muốn xóa thông tin QC: {SelectedItem.IdControlInfoNavigation.Name} LOT: {SelectedItem.IdControlInfoNavigation.Lot} Level: {SelectedItem.IdLevelNavigation.Name}?", "Confirmation", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    DataProvider.Ins.DB.Remove(deleteItem);
                    DataProvider.Ins.DB.SaveChanges();
                    MessageBox.Show("Xóa thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    RefreshLists();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Stop);
                }
            }
        }

        private bool CanChangeQCInfo(Test p) => SelectedDevice != null && SelectedControlInfo != null && SelectedType != null;

        private void UpdateList()
        {
            List = ListDB.Where(s => s.IdDevice == SelectedDevice.Id && s.IdControlInfo == SelectedControlInfo.Id).ToList();
        }

        private bool CanChangeQCType(ControlInfo p) => SelectedType != null;

        private void UpdateControlInfoList()
        {
            ControlInfoList = new ObservableCollection<ControlInfo>(DataProvider.Ins.DB.ControlInfos.Where(s => s.IdControlType == SelectedType.Id).ToList());
        }

        private void UpdateTestList()
        {
            TestList = new ObservableCollection<Test>(DeviceTestList.Where(s => s.IdDevice == SelectedDevice.Id).Select(s => s.IdTestNavigation).OrderBy(s => s.Index));
            ListType = new ObservableCollection<ControlType>(DataProvider.Ins.DB.ControlTypes.Where(x => x.IdCategory == SelectedDevice.IdCategory));
        }

        private void UpdateSelectedItemDetails()
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
            IsChecked = (bool)SelectedItem.Status;
            CurMean = (double)SelectedItem.CurMean;
            CurSd = (double)SelectedItem.CurSd;
        }

        private void LoadNew()
        {
            List = new List<ControlInfoDetail>();
            ListDB = new ObservableCollection<ControlInfoDetail>(DataProvider.Ins.DB.ControlInfoDetails);
            TestList = new ObservableCollection<Test>(DataProvider.Ins.DB.Tests);
            LevelList = new ObservableCollection<LevelQc>(DataProvider.Ins.DB.LevelQcs);
            ControlInfoListDB = new ObservableCollection<ControlInfo>(DataProvider.Ins.DB.ControlInfos);
            DeviceList = new ObservableCollection<Device>(DataProvider.Ins.DB.Devices);
            DeviceTestList = new ObservableCollection<DeviceTest>(DataProvider.Ins.DB.DeviceTests);
        }

        private void RefreshLists()
        {
            ListDB = new ObservableCollection<ControlInfoDetail>(DataProvider.Ins.DB.ControlInfoDetails);
            List = ListDB.Where(s => s.IdControlInfo == SelectedControlInfo.Id && s.IdDevice == SelectedDevice.Id).ToList();
        }
    }
}
