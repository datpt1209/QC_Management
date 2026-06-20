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

        // Changed CalDetail list to ObservableCollection so view updates like QC view
        private ObservableCollection<CalDetail> _calDetail_list;
        public ObservableCollection<CalDetail> CalDetail_List
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

        private CalInfor? _selectedCalInfor;
        public CalInfor? SelectedCalInfo
        {
            get => _selectedCalInfor;
            set
            {
                _selectedCalInfor = value;
                OnPropertyChanged();

                UpdateSelectedItemDetails();
                // When a CalInfo is selected, load its CalDetails (attached entities)
                LoadDetailsForSelectedItem();
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
                // when device changes update tests and filter details
                UpdateTestList();
                FilterDetailsByDevice();
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

        private ObservableCollection<LevelQc> _levelList = new();
        public ObservableCollection<LevelQc> LevelList
        {
            get => _levelList;
            set => SetProperty(ref _levelList, value);
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

        // New property to represent CalInfor.Status (left/master record)
        private bool _calInfoIsEnable = true;
        public bool CALInfoIsEnable
        {
            get => _calInfoIsEnable;
            set => SetProperty(ref _calInfoIsEnable, value);
        }

        private int _selectedLevel;
        private ObservableCollection<CalInfor> _CALInfoListDB;
        public int SelectedLevel
        {
            get => _selectedLevel;
            set => SetProperty(ref _selectedLevel, value);
        }
        public ObservableCollection<CalInfor> CALInfoListDB
        {
            get => _CALInfoListDB;
            set => SetProperty(ref _CALInfoListDB, value);
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
                    Status = CALInfoIsEnable,
                };

                try
                {
                    DataProvider.Ins.DB.CalInfors.Add(calInfor);
                    DataProvider.Ins.DB.SaveChanges();
                    MessageBox.Show("Thêm thông tin Cal thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
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
                return SelectedCalInfo != null &&
                       (SelectedCalInfo.ExpirationDate != CALExpirationDate ||
                        SelectedCalInfo.CalLot != CALLOT ||
                        SelectedCalInfo.IdCalType != SelectedCalibType?.Id ||
                        SelectedCalInfo.Status != CALInfoIsEnable);
            }, (p) =>
            {
                SelectedCalInfo.CalLot = CALLOT;
                SelectedCalInfo.ExpirationDate = CALExpirationDate;
                SelectedCalInfo.IdCalType = SelectedCalibType.Id;
                SelectedCalInfo.IdCalTypeNavigation = SelectedCalibType;
                SelectedCalInfo.Status = CALInfoIsEnable;

                try
                {
                    // propagate status change to related CalDetails
                    var details = DataProvider.Ins.DB.CalDetails.Where(cd => cd.IdCalInfor == SelectedCalInfo.Id).ToList();
                    foreach (var d in details)
                        d.Status = CALInfoIsEnable;

                    DataProvider.Ins.DB.SaveChanges();
                    MessageBox.Show("Cập nhật thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    RefreshCalInforList();
                    RefreshCalDetailList();
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

                RefreshCalInforList();
            });

            CALDeleteCommand = new RelayCommand<CalInfor>((p) =>
            {
                return SelectedCalInfo != null;
            }, (p) =>
            {
                if (SelectedCalInfo == null) return;

                var result = MessageBox.Show($"Bạn có muốn xóa thông tin Cal: {SelectedCalInfo.CalLot}?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        var details = DataProvider.Ins.DB.CalDetails.Where(cd => cd.IdCalInfor == SelectedCalInfo.Id).ToList();
                        DataProvider.Ins.DB.CalDetails.RemoveRange(details);
                        DataProvider.Ins.DB.CalInfors.Remove(SelectedCalInfo);
                        DataProvider.Ins.DB.SaveChanges();

                        MessageBox.Show("Xóa thông tin Cal thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
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
           SelectedCalInfo != null && SelectedDevice != null && SelectedTest != null && SelectedLevel != 0 && Max != 0;

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
                MessageBox.Show("Thêm CalDetail thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
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
            (SelectedCalDetail.IdCalInfor != SelectedCalInfo?.Id ||
            SelectedCalDetail.Level != SelectedLevel ||
            SelectedCalDetail.IdTestNavigation != SelectedTest ||
            SelectedCalDetail.MinValue != Min ||
            SelectedCalDetail.MaxValue != Max ||
            SelectedCalDetail.Status != CALIsEnable
            );

        private void Edit(CalDetail p)
        {
            if (SelectedCalDetail == null) return;

            SelectedCalDetail.IdCalInfor = SelectedCalInfo.Id;
            SelectedCalDetail.IdCalInforNavigation = SelectedCalInfo;
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
            if (SelectedCalDetail == null) return;

            var deleteItem = DataProvider.Ins.DB.CalDetails.FirstOrDefault(s => s.Id == SelectedCalDetail.Id);
            if (deleteItem == null) return;

            var result = MessageBox.Show($"Bạn có muốn xóa thông tin Cal: {SelectedCalDetail.IdCalInforNavigation?.CalLot} Level: {SelectedCalDetail.Level}?", "Confirmation", MessageBoxButton.YesNo);
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
            CALListDB = new ObservableCollection<CalInfor>(DataProvider.Ins.DB.CalInfors.Include(c => c.CalDetails));
            CALList = CALListDB.ToList();
            LevelList = new ObservableCollection<LevelQc>(DataProvider.Ins.DB.LevelQcs);
            // initialize empty details
            CalDetail_List = new ObservableCollection<CalDetail>();
            // default new cal info status
            CALInfoIsEnable = true;
        }

        private void UpdateSelectedItemDetails()
        {
            ResetCalDetailFields();
            if (SelectedCalInfo != null)
            {
                CALExpirationDate = SelectedCalInfo.ExpirationDate;
                CALLOT = SelectedCalInfo.CalLot;
                // reflect CalInfor status to UI
                CALInfoIsEnable = SelectedCalInfo.Status;
                // reflect CalInfor's type to the editor (so CalType combobox updates)
                SelectedCalibType = SelectedCalInfo.IdCalTypeNavigation;
                // load attached CalDetails and set observable
                var details = DataProvider.Ins.DB.CalDetails
                                    .Where(cd => cd.IdCalInfor == SelectedCalInfo.Id)
                                    .Include(cd => cd.IdTestNavigation)
                                    .Include(cd => cd.IdDeviceNavigation)
                                    .Include(cd => cd.IdCalInforNavigation).ToList();

                CalDetail_List = new ObservableCollection<CalDetail>(details);
            }
            else
            {
                CalDetail_List = new ObservableCollection<CalDetail>();
            }
        }

        private void UpdateSelectedCalDetails()
        {
            if (SelectedCalDetail == null) return;

            // cache values so setter side-effects won't change what we apply
            var level = SelectedCalDetail.Level ?? 0;
            var test = SelectedCalDetail.IdTestNavigation;
            var min = SelectedCalDetail.MinValue ?? 0;
            var max = SelectedCalDetail.MaxValue ?? 0;
            var device = SelectedCalDetail.IdDeviceNavigation;
            var status = SelectedCalDetail.Status;

            // apply simple properties first
            SelectedLevel = level;
            Min = min;
            Max = max;

            // set backing field directly for device to avoid triggering SelectedDevice setter
            _selectedDevice = device;
            OnPropertyChanged(nameof(SelectedDevice));

            // update tests for the device (without filtering details which would clear SelectedCalDetail)
            UpdateTestList();

            // now set selected test and status
            SelectedTest = test;
            CALIsEnable = status;
        }

        private void RefreshCalInforList()
        {
            if (SelectedCalibType == null)
            {
                CALList = new List<CalInfor>();
                CALInfoListDB = new ObservableCollection<CalInfor>();
                return;
            }
            CALInfoListDB = new ObservableCollection<CalInfor>(DataProvider.Ins.DB.CalInfors
                .Where(c => c.IdCalType == SelectedCalibType.Id)
                .Include(c => c.CalDetails));
            CALList = CALInfoListDB.ToList();
        }

        private void RefreshCalDetailList()
        {
            if (SelectedCalInfo != null)
            {
                var details = DataProvider.Ins.DB.CalDetails
                    .Where(cd => cd.IdCalInfor == SelectedCalInfo.Id)
                    .Include(cd => cd.IdTestNavigation)
                    .Include(cd => cd.IdDeviceNavigation)
                    .ToList();

                CalDetail_List = new ObservableCollection<CalDetail>(details);
            }
            else
            {
                CalDetail_List = new ObservableCollection<CalDetail>();
            }
        }

        private void ResetCalInforFields()
        {
            CALLOT = string.Empty;
            CALExpirationDate = DateTime.Now;
            CALInfoIsEnable = true;
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
            // after updating tests, keep selected test if it's still in the list
            if (SelectedTest != null && !TestList.Any(t => t.Id == SelectedTest.Id))
                SelectedTest = null;
        }

        // Filtering details by selected device (mirrors QC flow)
        private void FilterDetailsByDevice()
        {
            if (SelectedCalInfo == null)
            {
                CalDetail_List = new ObservableCollection<CalDetail>();
                return;
            }

            try
            {
                var query = DataProvider.Ins.DB.CalDetails
                    .Include(cd => cd.IdTestNavigation)
                    .Include(cd => cd.IdDeviceNavigation)
                    .Where(cd => cd.IdCalInfor == SelectedCalInfo.Id);

                if (SelectedDevice != null)
                    query = query.Where(cd => cd.IdDevice == SelectedDevice.Id);

                var details = query.OrderBy(cd => cd.IdDeviceNavigation.Name).ThenBy(cd => cd.Level).ThenBy(cd => cd.IdTest).ToList();
                CalDetail_List = new ObservableCollection<CalDetail>(details);
                SelectedCalDetail = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Filter cal detail failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void LoadDetailsForSelectedItem()
        {
            if (SelectedCalInfo != null)
            {
                var details = DataProvider.Ins.DB.CalDetails
                    .Where(cd => cd.IdCalInfor == SelectedCalInfo.Id)
                    .Include(cd => cd.IdTestNavigation)
                    .Include(cd => cd.IdDeviceNavigation)
                    .ToList();

                CalDetail_List = new ObservableCollection<CalDetail>(details);
            }
            else
            {
                CalDetail_List = new ObservableCollection<CalDetail>();
            }
        }
    }
}

