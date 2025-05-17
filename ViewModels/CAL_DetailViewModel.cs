using QC_Management.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace QC_Management.ViewModels
{
    public class CAL_DetailViewModel : BaseViewModel
    {
        private List<CalDetail> _list;
        public List<CalDetail> List
        {
            get => _list;
            set => SetProperty(ref _list, value);
        }

        private ObservableCollection<CalDetail> _listDB;
        public ObservableCollection<CalDetail> ListDB
        {
            get => _listDB;
            set => SetProperty(ref _listDB, value);
        }

        private ObservableCollection<CalType> _CalTypeList;
        public ObservableCollection<CalType> CalTypeList
        {
            get => _CalTypeList;
            set => SetProperty(ref _CalTypeList, value);
        }

        private CalType _SelectedCalType;
        public CalType SelectedCalType
        {
            get => _SelectedCalType;
            set => SetProperty(ref _SelectedCalType, value);
        }

        private ObservableCollection<Device> _deviceList;
        public ObservableCollection<Device> DeviceList
        {
            get => _deviceList;
            set => SetProperty(ref _deviceList, value);
        }

        private ObservableCollection<CalInfor> _calInfoList;
        public ObservableCollection<CalInfor> CalInfoList
        {
            get => _calInfoList;
            set => SetProperty(ref _calInfoList, value);
        }

        private ObservableCollection<DeviceTest> _DeviceTestList;
        public ObservableCollection<DeviceTest> DeviceTestList
        {
            get => _DeviceTestList;
            set => SetProperty(ref _DeviceTestList, value);
        }
      
        private ObservableCollection<CalInfor> _calInfoListDB;
        public ObservableCollection<CalInfor> CalInfoListDB
        {
            get => _calInfoListDB;
            set => SetProperty(ref _calInfoListDB, value);
        }

        private ObservableCollection<Test> _testList;
        public ObservableCollection<Test> TestList
        {
            get => _testList;
            set => SetProperty(ref _testList, value);
        }


        public ICommand AddCommand { get; set; }
        public ICommand EditCommand { get; set; }
        public ICommand DeleteCommand { get; set; }
        public ICommand LoadedCommand { get; set; }
        public ICommand CalInforSelectionChangedCommand { get; set; }
        public ICommand DeviceSelectionChangedCommand { get; set; }
        public ICommand CalTypeSelectionChangedCommand { get; set; }

        private double _min;
        public double Min
        {
            get => _min;
            set => SetProperty(ref _min, value);
        }

   
        private double _max;
        public double Max
        {
            get => _max;
            set => SetProperty(ref _max, value);
        }

        private bool _isCalEnble;
        public bool CALIsEnable
        {
            get => _isCalEnble;
            set => SetProperty(ref _isCalEnble, value);
        }

        private CalDetail _selectedItem;
        public CalDetail SelectedItem
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

        private CalInfor _selectedCalInfo;
        public CalInfor SelectedCalInfo
        {
            get => _selectedCalInfo;
            set => SetProperty(ref _selectedCalInfo, value);
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

        private int _selectedLevel;
        public int SelectedLevel
        {
            get => _selectedLevel;
            set => SetProperty(ref _selectedLevel, value);
        }

        public CAL_DetailViewModel()
        {
            InitializeCommands();
        }

        private void InitializeCommands()
        {
            AddCommand = new RelayCommand<CalDetail>(CanAdd, Add);
            EditCommand = new RelayCommand<CalDetail>(CanEdit, Edit);
            DeleteCommand = new RelayCommand<CalDetail>(CanDelete, Delete);
            LoadedCommand = new RelayCommand<CalDetail>(_ => true, _ => LoadNew());
            DeviceSelectionChangedCommand = new RelayCommand<Test>(_ => true, _ => UpdateTestList());
            CalInforSelectionChangedCommand = new RelayCommand<Test>(CanChangeQCInfo, _ => UpdateList());
            CalTypeSelectionChangedCommand = new RelayCommand<ControlInfo>(CanChangeQCType, _ => UpdateControlInfoList());
        }

        private bool CanAdd(CalDetail p) =>
            SelectedCalInfo != null && SelectedTest != null && SelectedLevel != null  && Max != 0;

        private void Add(CalDetail p)
        {
            var calDetail = new CalDetail
            {
                IdDevice = SelectedDevice.Id,
                IdDeviceNavigation = SelectedDevice,
                IdCalInfor = SelectedCalInfo.Id,
                IdCalInforNavigation = SelectedCalInfo,
                Level = SelectedLevel,
                IdTestNavigation = SelectedTest,
                IdTest = SelectedTest.Id,
                MinValue = Min,
                MaxValue = Max,
                Status = CALIsEnable,
            
            };

            try
            {
                DataProvider.Ins.DB.CalDetails.Add(calDetail);
                DataProvider.Ins.DB.SaveChanges();
                MessageBox.Show("Thêm thông tin QC thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                RefreshLists();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Stop);
            }
        }

        private bool CanEdit(CalDetail p) =>
            SelectedItem != null &&
            (SelectedItem.IdCalInforNavigation != SelectedCalInfo ||
            SelectedItem.Level != SelectedLevel ||
            SelectedItem.IdTestNavigation != SelectedTest ||
            SelectedItem.MinValue != Min ||
            SelectedItem.MaxValue != Max ||
            SelectedItem.Status != CALIsEnable
            );

        private void Edit(CalDetail p)
        {
            SelectedItem.IdCalInfor = SelectedCalInfo.Id;
            SelectedItem.IdCalInforNavigation = SelectedCalInfo;
            SelectedItem.Level = SelectedLevel;
            SelectedItem.IdTest = SelectedTest.Id;
            SelectedItem.MinValue = Min;
            SelectedItem.MaxValue = Max;
            SelectedItem.Status = CALIsEnable;
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

        private bool CanDelete(CalDetail p) => SelectedItem != null;

        private void Delete(CalDetail p)
        {
            var deleteItem = DataProvider.Ins.DB.CalDetails.FirstOrDefault(s => s.Id == SelectedItem.Id);

            if (deleteItem == null) return;

            var result = MessageBox.Show($"Bạn có muốn xóa thông tin QC: {SelectedItem.IdCalInforNavigation} LOT: {SelectedItem.IdCalInforNavigation.CalLot} Level: {SelectedItem.Level}?", "Confirmation", MessageBoxButton.YesNo);
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

        private bool CanChangeQCInfo(Test p) => SelectedDevice != null && SelectedCalInfo != null;

        private void UpdateList()
        {
            List = ListDB.Where(s => s.IdDevice == SelectedDevice.Id && s.IdCalInfor == SelectedCalInfo.Id).ToList();
        }

        private bool CanChangeQCType(ControlInfo p) => true;

        private void UpdateControlInfoList()
        {
            CalInfoList = new ObservableCollection<CalInfor>(DataProvider.Ins.DB.CalInfors.Where(s => s.IdCalType == SelectedCalType.Id).ToList());
        }

        private void UpdateTestList()
        {
               TestList = new ObservableCollection<Test>(DeviceTestList.Where(s => s.IdDevice == SelectedDevice.Id).Select(s => s.IdTestNavigation).OrderBy(s => s.Index));
                
        }

        private void UpdateSelectedItemDetails()
        {
            SelectedCalInfo = SelectedItem.IdCalInforNavigation;
            SelectedLevel = (int)SelectedItem.Level;
            SelectedTest = SelectedItem.IdTestNavigation;
            Min = (double)SelectedItem.MinValue;
            Max = (double)SelectedItem.MaxValue;
            SelectedDevice = SelectedItem.IdDeviceNavigation;
            CALIsEnable = (bool)SelectedItem.Status;
        }

        private void LoadNew()
        {
            List = new List<CalDetail>();
            ListDB = new ObservableCollection<CalDetail>(DataProvider.Ins.DB.CalDetails);
            TestList = new ObservableCollection<Test>(DataProvider.Ins.DB.Tests);
            CalInfoListDB = new ObservableCollection<CalInfor>(DataProvider.Ins.DB.CalInfors);
            CalInfoList = new ObservableCollection<CalInfor>(DataProvider.Ins.DB.CalInfors);
            DeviceList = new ObservableCollection<Device>(DataProvider.Ins.DB.Devices);
            DeviceTestList = new ObservableCollection<DeviceTest>(DataProvider.Ins.DB.DeviceTests);
            CalTypeList = new ObservableCollection<CalType>(DataProvider.Ins.DB.CalTypes);
          
        }

        private void RefreshLists()
        {
            ListDB = new ObservableCollection<CalDetail>(DataProvider.Ins.DB.CalDetails);
            List = ListDB.Where(s => s.IdCalInfor == SelectedCalInfo.Id && s.IdDevice == SelectedDevice.Id).ToList();
        }
    }
}
