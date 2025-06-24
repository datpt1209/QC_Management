using Microsoft.EntityFrameworkCore;
using QC_Management.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace QC_Management.ViewModels
{
    public class CAL_InforViewModel : BaseViewModel
    {

        private List<CalInfor> _CALList;
        public List<CalInfor> CALList { get => _CALList; set { _CALList = value; OnPropertyChanged(); } }
        public ObservableCollection<CalInfor> CALListDB { get; set; }
        public ICommand CALAddCommand { get; set; }
        public ICommand CALEditCommand { get; set; }
        public ICommand CALDeleteCommand { get; set; }
        public ICommand LoadedCommand { get; set; }
        public ICommand CalTypeSelectionChangedCommand { get; set; }

        public ICommand CalDetailAddCommand { get; set; }
        public ICommand CalDetailEditCommand { get; set; }
        public ICommand CalDetailDeleteCommand { get; set; }
        public ICommand DeviceSelectionChangedCommand { get; set; }

        private DateTime _CALExpirationDate = DateTime.Now;
        public DateTime CALExpirationDate { get => _CALExpirationDate; set { _CALExpirationDate = value; OnPropertyChanged(); } }

        private string _CALLOT;
        public string CALLOT { get => _CALLOT; set { _CALLOT = value; OnPropertyChanged(); } }

        private ObservableCollection<CalType> _CalibTypeList;
        public ObservableCollection<CalType> CalibTypeList
        {
            get => _CalibTypeList;
            set => SetProperty(ref _CalibTypeList, value);
        }

        private CalType? _SelectedCalibType;
        public CalType? SelectedCalibType
        {
            get => _SelectedCalibType;
            set
            {
                _SelectedCalibType = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<CalInfor> _CALInfoList;
        public ObservableCollection<CalInfor> CALInfoList
        {
            get => _CALInfoList;
            set => SetProperty(ref _CALInfoList, value);
        }
        private CalInfor _selectedCalInfo;
        public CalInfor SelectedCalInfo
        {
            get => _selectedCalInfo;
            set => SetProperty(ref _selectedCalInfo, value);
        }

        private CalDetail? _selectedCalDetail;
        public CalDetail? SelectedCalDetail
        {
            get => _selectedCalDetail;
            set
            {
                if (SetProperty(ref _selectedCalDetail, value) && _selectedCalDetail != null)
                {
                    UpdateSelectedCalDetails();
                }
            }
        }
        private ObservableCollection<CalInfor> _CALInfoListDB;
        public ObservableCollection<CalInfor> CALInfoListDB
        {
            get => _CALInfoListDB;
            set => SetProperty(ref _CALInfoListDB, value);
        }

        private List<CalDetail> _calDetail_list;
        public List<CalDetail> CalDetail_List
        {
            get => _calDetail_list;
            set => SetProperty(ref _calDetail_list, value);
        }

        private ObservableCollection<Device> _deviceList;
        public ObservableCollection<Device> DeviceList
        {
            get => _deviceList;
            set => SetProperty(ref _deviceList, value);
        }

        private ObservableCollection<DeviceTest> _DeviceTestList;
        public ObservableCollection<DeviceTest> DeviceTestList
        {
            get => _DeviceTestList;
            set => SetProperty(ref _DeviceTestList, value);
        }

        private CalInfor? _CALSelectedItem;
        public CalInfor? CALSelectedItem
        {
            get => _CALSelectedItem;
            set
            {
                _CALSelectedItem = value;
                OnPropertyChanged();
                UpdateSelectedItemDetails();
               
            }
        }

        private Device? _selectedDevice;
        public Device? SelectedDevice
        {
            get => _selectedDevice;
            set
            {
                _selectedDevice = value;
                OnPropertyChanged();
            }
        }

        private Test? _selectedTest;
        public Test? SelectedTest
        {
            get => _selectedTest;
            set
            {
                _selectedTest = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<Test> _testList;
        public ObservableCollection<Test> TestList
        {
            get => _testList;
            set => SetProperty(ref _testList, value);
        }

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

        private int _selectedLevel;
        public int SelectedLevel
        {
            get => _selectedLevel;
            set => SetProperty(ref _selectedLevel, value);
        }
        public CAL_InforViewModel()
        {
            CalDetailAddCommand = new RelayCommand<CalDetail>(CanAdd, Add);
            CalDetailEditCommand = new RelayCommand<CalDetail>(CanEdit, Edit);
            CalDetailDeleteCommand = new RelayCommand<CalDetail>(CanDelete, Delete);
            DeviceSelectionChangedCommand = new RelayCommand<Test>(_ => true, _ => UpdateTestList());
            LoadedCommand = new RelayCommand<object>((p) =>
            {
                return true;

            }, (p) =>
            {
                LoadNew();
            });


            CALAddCommand = new RelayCommand<CalInfor>((p) =>
            {
                return !string.IsNullOrWhiteSpace(CALLOT) && SelectedCalibType != null;
            }, (p) =>
            {
                var calInfor = new CalInfor()
                {
                    IdCalType = SelectedCalibType.Id,
                    IdCalTypeNavigation = SelectedCalibType,
                    CalLot = CALLOT,
                    ExpirationDate = CALExpirationDate,
                };

                try
                {
                    DataProvider.Ins.DB.CalInfors.Add(calInfor);
                    DataProvider.Ins.DB.SaveChanges();
                    MessageBox.Show("Thêm thông tin QC thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    RefreshCalInforList();
                    ResetCalInforFields();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Stop);
                }
            });

            CALEditCommand = new RelayCommand<CalInfor>((p) =>
            {
                return CALSelectedItem != null &&
                       (CALSelectedItem.ExpirationDate != CALExpirationDate ||
                        CALSelectedItem.CalLot != CALLOT ||
                        CALSelectedItem.IdCalType != SelectedCalibType?.Id);
            }, (p) =>
            {
                CALSelectedItem.CalLot = CALLOT;
                CALSelectedItem.ExpirationDate = CALExpirationDate;
                CALSelectedItem.IdCalType = SelectedCalibType.Id;
                CALSelectedItem.IdCalTypeNavigation = SelectedCalibType;

                try
                {
                    DataProvider.Ins.DB.SaveChanges();
                    MessageBox.Show("Cập nhật thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    RefreshCalInforList();
                    ResetCalInforFields();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Stop);
                }
            });

            CalTypeSelectionChangedCommand = new RelayCommand<CalInfor>((p) =>
            {
            if (CalibTypeList == null || SelectedCalibType == null)
                    return false;
               
                else
                    return true;

            }, (p) =>
            {
                CALList = DataProvider.Ins.DB.CalInfors
                    .Where(s => s.IdCalType == SelectedCalibType.Id)
                    .Include(c => c.CalDetails)
                    .ToList();
            });

            CALDeleteCommand = new RelayCommand<CalInfor>((p) =>
            {
                return CALSelectedItem != null;
            }, (p) =>
            {
                if (CALSelectedItem == null) return;

                var result = MessageBox.Show($"Bạn có muốn xóa thông tin QC: {CALSelectedItem.CalLot}?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        // Xóa các CalDetail liên quan
                        var details = DataProvider.Ins.DB.CalDetails.Where(cd => cd.IdCalInfor == CALSelectedItem.Id).ToList();
                        DataProvider.Ins.DB.CalDetails.RemoveRange(details);

                        // Xóa CalInfor
                        DataProvider.Ins.DB.CalInfors.Remove(CALSelectedItem);
                        DataProvider.Ins.DB.SaveChanges();

                        MessageBox.Show("Xóa thông tin QC thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                        RefreshCalInforList();
                        ResetCalInforFields();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error: {ex}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Stop);
                    }
                }
            });
        }

        private bool CanAdd(CalDetail p) =>
           CALSelectedItem != null && SelectedDevice != null && SelectedTest != null && SelectedLevel != null && Max != 0;

        private void Add(CalDetail p)
        {
            var calDetail = new CalDetail
            {
                IdDevice = SelectedDevice.Id,
                IdDeviceNavigation = SelectedDevice,
                IdCalInfor = CALSelectedItem.Id,
                IdCalInforNavigation = CALSelectedItem,
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
                RefreshCalDetailList();
                ResetCalDetailFields();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Stop);
            }
        }

        private bool CanEdit(CalDetail p) =>
            SelectedCalDetail != null &&
            (SelectedCalDetail.IdCalInforNavigation != SelectedCalInfo ||
            SelectedCalDetail.Level != SelectedLevel ||
            SelectedCalDetail.IdTestNavigation != SelectedTest ||
            SelectedCalDetail.MinValue != Min ||
            SelectedCalDetail.MaxValue != Max ||
            SelectedCalDetail.Status != CALIsEnable
            );

        private void Edit(CalDetail p)
        {
            SelectedCalDetail.IdCalInfor = CALSelectedItem.Id;
            SelectedCalDetail.IdCalInforNavigation = CALSelectedItem;
            SelectedCalDetail.Level = SelectedLevel;
            SelectedCalDetail.IdTest = SelectedTest.Id;
            SelectedCalDetail.MinValue = Min;
            SelectedCalDetail.MaxValue = Max;
            SelectedCalDetail.Status = CALIsEnable;
            try
            {
                DataProvider.Ins.DB.SaveChanges();
                MessageBox.Show("Cập nhật thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                RefreshCalDetailList();
                ResetCalDetailFields();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Stop);
            }
        }

        private bool CanDelete(CalDetail p) => SelectedCalDetail != null;

        private void Delete(CalDetail p)
        {
            var deleteItem = DataProvider.Ins.DB.CalDetails.FirstOrDefault(s => s.Id == SelectedCalDetail.Id);

            if (deleteItem == null) return;

            var result = MessageBox.Show($"Bạn có muốn xóa thông tin QC: {SelectedCalDetail.IdCalInforNavigation} LOT: {SelectedCalDetail.IdCalInforNavigation.CalLot} Level: {SelectedCalDetail.Level}?", "Confirmation", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    DataProvider.Ins.DB.Remove(deleteItem);
                    DataProvider.Ins.DB.SaveChanges();
                    MessageBox.Show("Xóa thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    RefreshCalDetailList();
                    ResetCalDetailFields();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Stop);
                }
            }
        }
        private void LoadNew()
        {
            CalibTypeList = new ObservableCollection<CalType>(DataProvider.Ins.DB.CalTypes);
            DeviceList = new ObservableCollection<Device>(DataProvider.Ins.DB.Devices);
            DeviceTestList = new ObservableCollection<DeviceTest>();
            CALListDB = new ObservableCollection<CalInfor>();
            CALList = new List<CalInfor>();
        }
        private void UpdateSelectedItemDetails()
        {
            ResetCalDetailFields();
            if (CALSelectedItem != null)
            {
                CALExpirationDate = CALSelectedItem.ExpirationDate;
                CALLOT = CALSelectedItem.CalLot;
                CalDetail_List = DataProvider.Ins.DB.CalDetails
                                    .Where(cd => cd.IdCalInforNavigation.Id == CALSelectedItem.Id)
                                    .Include(cd => cd.IdTestNavigation)
                                    .Include(cd => cd.IdDeviceNavigation)
                                    .Include(cd => cd.IdCalInforNavigation).ToList();
            }
        }

        private void UpdateSelectedCalDetails()
        {
            SelectedLevel = (int)SelectedCalDetail.Level;
            SelectedTest = SelectedCalDetail.IdTestNavigation;
            Min = (double)SelectedCalDetail.MinValue;
            Max = (double)SelectedCalDetail.MaxValue;
            SelectedDevice = SelectedCalDetail.IdDeviceNavigation;
            CALIsEnable = (bool)SelectedCalDetail.Status;
        }
        private void RefreshCalInforList()
        {
            if (SelectedCalibType == null)
            {
                CALList = new List<CalInfor>();
                return;
            }
            CALListDB = new ObservableCollection<CalInfor>(DataProvider.Ins.DB.CalInfors
                .Where(c => c.IdCalType == SelectedCalibType.Id)
                .Include(c => c.CalDetails));
            CALList = CALListDB.ToList();
        }

        private void RefreshCalDetailList()
        {
            if (SelectedCalInfo != null)
            {
                CalDetail_List = DataProvider.Ins.DB.CalDetails
                    .Where(cd => cd.IdCalInfor == SelectedCalInfo.Id)
                    .Include(cd => cd.IdTestNavigation)
                    .Include(cd => cd.IdDeviceNavigation)
                    .ToList();
            }
            else if (CALSelectedItem != null)
            {
                CalDetail_List = DataProvider.Ins.DB.CalDetails
                    .Where(cd => cd.IdCalInfor == CALSelectedItem.Id)
                    .Include(cd => cd.IdTestNavigation)
                    .Include(cd => cd.IdDeviceNavigation)
                    .ToList();
            }
            else
            {
                CalDetail_List = new List<CalDetail>();
            }
        }

        private void ResetCalInforFields()
        {
            CALLOT = string.Empty;
            CALExpirationDate = DateTime.Now;
        }

        private void ResetCalDetailFields()
        {
            SelectedDevice = null;
            SelectedTest = null;
            SelectedLevel = 0;
            Min = 0;
            Max = 0;
            CALIsEnable = false;
            SelectedCalDetail = null;
        }
        private void UpdateTestList()
        {
            if (SelectedDevice == null)
            {
                TestList = new ObservableCollection<Test>();
                return;
            }
            TestList = new ObservableCollection<Test>(DataProvider.Ins.DB.DeviceTests
                .Where(s => s.IdDevice == SelectedDevice.Id)
                .Select(s => s.IdTestNavigation)
                .OrderBy(s => s.Index));
        }
    }
}

