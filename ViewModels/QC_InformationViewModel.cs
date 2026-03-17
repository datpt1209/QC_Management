using Microsoft.EntityFrameworkCore;
using QC_Management.Models;
using QC_Management.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace QC_Management.ViewModels
{
    public class QC_InformationViewModel : BaseViewModel
    {
        private List<ControlInfo> _List;
        public List<ControlInfo> List { get => _List; set { _List = value; OnPropertyChanged(); } }

        private ObservableCollection<ControlInfo> _ListDB;
        public ObservableCollection<ControlInfo> ListDB { get => _ListDB; set { _ListDB = value; OnPropertyChanged(); } }

        private ObservableCollection<ControlType> _ListType;
        public ObservableCollection<ControlType> ListType { get => _ListType; set { _ListType = value; OnPropertyChanged(); } }
        
        private ObservableCollection<Category> _CategoryList;
        public ObservableCollection<Category> CategoryList { get => _CategoryList; set { _CategoryList = value; OnPropertyChanged(); } }

        public ICommand AddCommand { get; set; }
        public ICommand EditCommand { get; set; }
        public ICommand DeleteCommand { get; set; }
        public ICommand LoadedCommand { get; set; }
        public ICommand CategorySelectionChangedCommand { get; set; }
        public ICommand QCTypeSelectionChangedCommand { get; set;}
        public ICommand DeviceSelectionChangedCommand { get; set; }
        public ICommand TestSelectionChangedCommand { get; set; }

        // commands for detail (range) actions
        public ICommand AddRangeCommand { get; set; }
        public ICommand EditRangeCommand { get; set; }
        public ICommand DeleteRangeCommand { get; set; }

        // apply range to results (copied from QC_DetailViewModel)
        public ICommand ApplyRangeAllCommand { get; set; }

        // --- New: collections/properties for QC detail area ---
        private ObservableCollection<ControlInfoDetail> _DetailList = new();
        public ObservableCollection<ControlInfoDetail> DetailList
        {
            get => _DetailList;
            set { _DetailList = value; OnPropertyChanged(); }
        }

        private ControlInfoDetail? _SelectedDetail;
        public ControlInfoDetail? SelectedDetail
        {
            get => _SelectedDetail;
            set
            {
                _SelectedDetail = value;
                OnPropertyChanged();
                if (_SelectedDetail != null)
                {
                    // When a range row is selected, update editor fields and
                    // set SelectedDevice/SelectedTest/SelectedLevel so the ComboBoxes follow.
                    // Suppress device filtering while we set these programmatically so we don't
                    // reload/filter DetailList prematurely.
                    _suppressDeviceFilter = true;
                    try
                    {
                        MeanNSX = _SelectedDetail.MeanNsx;
                        SDNSX = _SelectedDetail.SdNsx;
                        MeanPXN = _SelectedDetail.MeanApp;
                        SdPXN = _SelectedDetail.SdApp;
                        CurMean = _SelectedDetail.CurMean;
                        CurSd = _SelectedDetail.CurSd;
                        QualitativeMean = _SelectedDetail.QualitativeMean;

                        // pick device instance from DeviceList by id (so ComboBox.SelectedItem matches by reference)
                        if (DeviceList != null)
                        {
                            var matchDevice = DeviceList.FirstOrDefault(d => d.Id == _SelectedDetail.IdDevice)
                                              ?? DeviceList.FirstOrDefault(d => d.Id == _SelectedDetail.IdDeviceNavigation?.Id);
                            if (matchDevice != null)
                                SelectedDevice = matchDevice;
                            else
                                SelectedDevice = _SelectedDetail.IdDeviceNavigation;
                        }
                        else
                        {
                            SelectedDevice = _SelectedDetail.IdDeviceNavigation;
                        }

                        // ensure TestList contains tests for the SelectedDevice before setting SelectedTest
                        UpdateTestList();
                        if (TestList != null)
                        {
                            var matchTest = TestList.FirstOrDefault(t => t.Id == _SelectedDetail.IdTest)
                                           ?? TestList.FirstOrDefault(t => t.Id == _SelectedDetail.IdTestNavigation?.Id);
                            if (matchTest != null)
                                SelectedTest = matchTest;
                            else
                                SelectedTest = _SelectedDetail.IdTestNavigation;
                        }
                        else
                        {
                            SelectedTest = _SelectedDetail.IdTestNavigation;
                        }

                        // pick level instance from LevelList by id
                        if (LevelList != null)
                        {
                            var matchLevel = LevelList.FirstOrDefault(l => l.Id == _SelectedDetail.IdLevel)
                                            ?? LevelList.FirstOrDefault(l => l.Id == _SelectedDetail.IdLevelNavigation?.Id);
                            if (matchLevel != null)
                                SelectedLevel = matchLevel;
                            else
                                SelectedLevel = _SelectedDetail.IdLevelNavigation;
                        }
                        else
                        {
                            SelectedLevel = _SelectedDetail.IdLevelNavigation;
                        }

                        // update editor visibility based on the selected test
                        UpdateView();
                    }
                    finally
                    {
                        _suppressDeviceFilter = false;
                    }
                }
            }
        }

        private ObservableCollection<Device> _DeviceList = new();
        public ObservableCollection<Device> DeviceList
        {
            get => _DeviceList;
            set { _DeviceList = value; OnPropertyChanged(); }
        }

        private ObservableCollection<Test> _TestList = new();
        public ObservableCollection<Test> TestList
        {
            get => _TestList;
            set { _TestList = value; OnPropertyChanged(); }
        }

        private ObservableCollection<LevelQc> _LevelList = new();
        public ObservableCollection<LevelQc> LevelList
        {
            get => _LevelList;
            set { _LevelList = value; OnPropertyChanged(); }
        }

        private Device? _SelectedDevice;
        private bool _suppressDeviceFilter = false;
        public Device? SelectedDevice
        {
            get => _SelectedDevice;
            set
            {
                if (_SelectedDevice == value) return;
                _SelectedDevice = value;
                OnPropertyChanged();
                // Update the TestList so Test ComboBox reflects the selected device.
                UpdateTestList();

                // Only filter the DetailList when not suppressed (i.e. when user changes device in UI).
                if (!_suppressDeviceFilter && !_isLoadingDetails && SelectedItem != null)
                    FilterDetailsByDevice();
            }
        }

        private Test? _SelectedTest;
        public Test? SelectedTest
        {
            get => _SelectedTest;
            set
            {
                _SelectedTest = value;
                OnPropertyChanged();
            }
        }

        private LevelQc? _SelectedLevel;
        public LevelQc? SelectedLevel
        {
            get => _SelectedLevel;
            set
            {
                _SelectedLevel = value;
                OnPropertyChanged();
            }
        }
        // --- end new section ---

        private bool _isChecked_detail;
        // keep the original lowercase property (left-side list binding) and add a PascalCase alias (editor binding).
        // both share the same backing field and notify each other's name so XAML bindings remain valid.
        public bool ischecked_detail
        {
            get => _isChecked_detail;
            set
            {
                if (SetProperty(ref _isChecked_detail, value))
                {
                    OnPropertyChanged(nameof(ischecked_detail));
                }
            }
        }

        private bool _isChecked_Info;

        public bool isChecked_Info
        {
            get => _isChecked_Info;
            set
            {
                if (SetProperty(ref _isChecked_Info, value))
                {
                    OnPropertyChanged(nameof(isChecked_Info));
                }
            }
        }

        private DateTime _ProductionDate = DateTime.Now;
        public DateTime ProductionDate { get => _ProductionDate; set { _ProductionDate = value; OnPropertyChanged(); } }


        private DateTime _ExpirationDate = DateTime.Now;
        public DateTime ExpirationDate { get => _ExpirationDate; set { _ExpirationDate = value; OnPropertyChanged(); } }

        private string _LOT;
        public string LOT { get => _LOT; set { _LOT = value; OnPropertyChanged(); } }

        private ControlType? _SelectedType;
        public ControlType? SelectedType { get => _SelectedType; set { _SelectedType = value; OnPropertyChanged(); } }

        private ObservableCollection<Test> _testList;
        public ObservableCollection<Test> TestList_Short
        {
            get => _testList;
            set => SetProperty(ref _testList, value);
        }

        private ControlInfo _SelectedItem;
        public ControlInfo SelectedItem
        {
            get => _SelectedItem;
            set
            {
                _SelectedItem = value;
                OnPropertyChanged();
                if (SelectedItem != null)
                {
                    ProductionDate = SelectedItem.ProductionDate;
                    ExpirationDate = SelectedItem.ExpirationDate;
                    LOT = SelectedItem.Lot;
                    SelectedType = SelectedItem.IdControlTypeNavigation;
                    ischecked_detail = SelectedItem.Status; // keep compatibility with existing code

                    // when a LOT is selected, load its detail range list
                    // do NOT change SelectedDevice/SelectedTest/SelectedLevel here:
                    // ComboBoxes should only update when a Detail row is explicitly selected.
                    LoadDetailsForSelectedItem();

                    // clear any previously selected detail so combo boxes don't change implicitly
                    SelectedDetail = null;
                }
                else
                {
                    DetailList = new ObservableCollection<ControlInfoDetail>();
                    SelectedDetail = null;
                }
            }
        }

        private Category _SelectedCategory;
        public Category SelectedCategory
        {
            get => _SelectedCategory;
            set
            {
                _SelectedCategory = value;
                OnPropertyChanged();
            }
        }

        // --- New: editor properties for adding a range ---
        private double? _meanNsx;
        public double? MeanNSX { get => _meanNsx; set { _meanNsx = value; OnPropertyChanged(); } }

        private double? _sdNsx;
        public double? SDNSX { get => _sdNsx; set { _sdNsx = value; OnPropertyChanged(); } }

        private double? _meanPxn;
        public double? MeanPXN { get => _meanPxn; set { _meanPxn = value; OnPropertyChanged(); } }

        private double? _sdPxn;
        public double? SdPXN { get => _sdPxn; set { _sdPxn = value; OnPropertyChanged(); } }

        private double? _curMean;
        public double? CurMean { get => _curMean; set { _curMean = value; OnPropertyChanged(); } }

        private double? _curSd;
        public double? CurSd { get => _curSd; set { _curSd = value; OnPropertyChanged(); } }

        private string? _qualitativeMean;
        public string? QualitativeMean { get => _qualitativeMean; set { _qualitativeMean = value; OnPropertyChanged(); } }

        private bool _qualitativeMeanVisibility;
        public bool QualitativeMeanVisibility { get => _qualitativeMeanVisibility; set { _qualitativeMeanVisibility = value; OnPropertyChanged(); } }

        private bool _quantativeVisibility;
        public bool QuantativeVisibility { get => _quantativeVisibility; set { _quantativeVisibility = value; OnPropertyChanged(); } }

        // command to add a new ControlInfoDetail (range)
        // AddRangeCommand defined earlier as public property

        public QC_InformationViewModel()
        {
            // Preserve selection when view re-opens: reload lists but restore SelectedItem (by Id or LOT) and its details.
            LoadedCommand = new RelayCommand<object>((p) =>
            {
                return true;

            }, (p) =>
            {
                // capture current selection keys before reload
                var previousSelectedId = SelectedItem?.Id;
                var previousLot = SelectedItem?.Lot;
                var previousSelectedTypeId = SelectedType?.Id;

                // reload master lists and lookup tables
                LoadNew();

                // repopulate List to reflect current SelectedType if any
                if (previousSelectedTypeId.HasValue)
                    List = ListDB.Where(s => s.IdControlType == previousSelectedTypeId.Value).ToList();
                else
                    List = ListDB.ToList();

                // try to restore previous selection (prefer Id, then LOT)
                ControlInfo restored = null;
                if (previousSelectedId.HasValue)
                {
                    restored = ListDB.FirstOrDefault(c => c.Id == previousSelectedId.Value);
                }
                if (restored == null && !string.IsNullOrWhiteSpace(previousLot))
                {
                    restored = ListDB.FirstOrDefault(c => c.Lot == previousLot);
                }

                if (restored != null)
                {
                    // assign SelectedItem from the freshly loaded collection (keeps entity tracking consistent)
                    SelectedItem = restored;

                    // reload details for this LOT
                    LoadDetailsForSelectedItem();

                    // keep LOT field (SelectedItem setter sets LOT already)
                }
                else
                {
                    // no previous selection found — keep UI as fresh
                    SelectedItem = null;
                    DetailList = new ObservableCollection<ControlInfoDetail>();
                }
            });

            TestSelectionChangedCommand = new RelayCommand<Test>(CanChangeTest, _ => UpdateView());

            // initialize DeviceSelectionChangedCommand so Device selection in XAML triggers updates
            // Guard the command so programmatic device changes (from SelectedDetail) won't call FilterDetailsByDevice.
            DeviceSelectionChangedCommand = new RelayCommand<Device>(
                _ => true,
                _ =>
                {
                    if (_suppressDeviceFilter) return; // ignore UI-event command when suppression active
                    UpdateTestList();
                    FilterDetailsByDevice();
                });

            // initialize detail commands
            AddRangeCommand = new RelayCommand<object>(
                (p) => CanAddRange(),
                (p) => AddRange());

            EditRangeCommand = new RelayCommand<object>(
                (p) => CanEditRange(),
                (p) => EditRange());

            DeleteRangeCommand = new RelayCommand<object>(
                (p) => CanDeleteRange(),
                (p) => DeleteRange());

            // apply range from SelectedDetail to results (like QC_DetailViewModel)
            ApplyRangeAllCommand = new RelayCommand<object>(
                (p) => SelectedDetail != null && SelectedDetail.IdTestNavigation != null && SelectedDetail.IdTestNavigation.TestType == 2 && SelectedDetail.IdDevice != null,
                (p) =>
                {
                    try
                    {
                        // create dialog on UI thread
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            var dlg = new ApplyRangeDialog
                            {
                                WindowStartupLocation = WindowStartupLocation.CenterOwner
                            };

                            // pick active window as preferred owner, fallback to MainWindow
                            var owner = Application.Current?.Windows
                                          .OfType<Window>()
                                          .FirstOrDefault(w => w.IsActive) ?? Application.Current?.MainWindow;

                            // only set Owner when it's not the dialog itself
                            if (owner != null && !ReferenceEquals(owner, dlg))
                            {
                                dlg.Owner = owner;
                            }

                            bool? dialogResult;
                            try
                            {
                                dialogResult = dlg.ShowDialog();
                            }
                            catch (Exception showEx)
                            {
                                MessageBox.Show($"Không thể mở hộp thoại chọn khoảng thời gian: {showEx.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                                return;
                            }

                            if (dialogResult == true)
                            {
                                // run apply on background so UI stays responsive
                                Task.Run(() =>
                                {
                                    try
                                    {
                                        ApplyMeanSdToAllRelatedTests(dlg.Start, dlg.End);
                                    }
                                    catch (Exception exApply)
                                    {
                                        Application.Current.Dispatcher.Invoke(() =>
                                            MessageBox.Show($"Lỗi khi áp dụng range: {exApply.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error));
                                    }
                                });
                            }
                        });
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Lỗi khi khởi tạo hộp thoại: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                });

            // existing commands (Add/Edit/Delete for ControlInfo) unchanged...
            AddCommand = new RelayCommand<Test>((p) =>
            {
                if (LOT == null || SelectedCategory == null)
                    return false;
                else
                {
                    return true;
                }

            }, (p) =>
            {
                var QC_Infor = new ControlInfo()
                {
                    Lot = LOT,
                    ProductionDate = ProductionDate,
                    ExpirationDate = ExpirationDate,
                    Status = ischecked_detail,
                    IdControlType = SelectedType.Id,
                    IdControlTypeNavigation = SelectedType,
                };

                try
                {
                    DataProvider.Ins.DB.ControlInfos.Add(QC_Infor);
                    DataProvider.Ins.DB.SaveChanges();
                    MessageBox.Show("Thêm thông tin QC thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    ListDB.Add(QC_Infor);
                    List = ListDB.Where(s => s.IdControlType == SelectedType.Id).ToList();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Stop);
                }

            });

            EditCommand = new RelayCommand<ControlInfo>((p) =>
            {
                if (SelectedItem == null)
                    return false;
                else if (
                SelectedItem.ProductionDate == ProductionDate
                && SelectedItem.ExpirationDate == ExpirationDate
                && SelectedItem.Lot == LOT
                && SelectedItem.Status == isChecked_Info
                && SelectedItem.IdControlTypeNavigation == SelectedType
                )
                    return false;
                else
                    return true;

            }, (p) =>
            {
                SelectedItem.IdControlTypeNavigation = SelectedType;
                SelectedItem.IdControlType  = SelectedType.Id;
                SelectedItem.Status = isChecked_Info;
                SelectedItem.Lot = LOT;
                SelectedItem.ProductionDate = ProductionDate;
                SelectedItem.ExpirationDate = ExpirationDate;

                var controlDetails = SelectedItem.ControlInfoDetails;
                foreach (var item in controlDetails)
                {
                    item.IdControlInfoNavigation = SelectedItem;
                    item.IdControlInfo = SelectedItem.Id;
                    // ensure detail Status follows parent's Status
                    item.Status = SelectedItem.Status;
                }

                try
                {
                    DataProvider.Ins.DB.SaveChanges();

                    // --- New: reload the updated ControlInfo (with its details) from DB to ensure UI shows fresh values ---
                    var db = DataProvider.Ins.DB;
                    var reloaded = db.ControlInfos
                        .Include(ci => ci.ControlInfoDetails)
                            .ThenInclude(d => d.IdDeviceNavigation)
                        .Include(ci => ci.ControlInfoDetails)
                            .ThenInclude(d => d.IdTestNavigation)
                        .Include(ci => ci.ControlInfoDetails)
                            .ThenInclude(d => d.IdLevelNavigation)
                        .Include(ci => ci.IdControlTypeNavigation)
                        .FirstOrDefault(ci => ci.Id == SelectedItem.Id);

                    if (reloaded != null)
                    {
                        // replace item in ListDB so bound collection reflects current entity state
                        var index = ListDB.IndexOf(ListDB.FirstOrDefault(x => x.Id == reloaded.Id));
                        if (index >= 0)
                            ListDB[index] = reloaded;

                        // set SelectedItem to the freshly loaded entity so its navigation props are up-to-date
                        SelectedItem = reloaded;
                    }

                    // reload details for UI (respects any selected device filter)
                    LoadDetailsForSelectedItem();

                    MessageBox.Show("Cập nhật thành công! (Status đã được đồng bộ cho các thông số chi tiết)", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);

                    // refresh master list view according to selected type
                    if (SelectedType != null)
                        List = ListDB.Where(s => s.IdControlType == SelectedType.Id).ToList();
                    else
                        List = ListDB.ToList();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Stop);
                }
            });

            DeleteCommand = new RelayCommand<ControlInfo>((p) =>
            {
                return SelectedItem != null;

            }, (p) => {

                if (SelectedItem == null)
                    return;
                try
                {
                    DataProvider.Ins.DB.ControlInfos.Remove(SelectedItem);
                    DataProvider.Ins.DB.SaveChanges();
                    MessageBox.Show("Xóa thông tin QC thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Update the ListDB and List properties to refresh the ListView
                    ListDB.Remove(SelectedItem);
                    List = ListDB.Where(s => s.IdControlType == SelectedType.Id).ToList();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Stop);
                }

            });

            QCTypeSelectionChangedCommand = new RelayCommand<ControlInfo>((p) =>
            {
              if(SelectedCategory == null || SelectedType == null) return false;
              else return true;

            }, (p) => 
            {
                List = ListDB.Where(s => s.IdControlType == SelectedType.Id).ToList() ;
            });


            CategorySelectionChangedCommand = new RelayCommand<ControlInfo>((p) =>
            {
                return true;

            }, (p) =>
            {
                SelectedType = null;
                LOT = string.Empty;

                ListType = new ObservableCollection<ControlType>(DataProvider.Ins.DB.ControlTypes.Where(x => x.IdCategory == SelectedCategory.Id));
            });
        }

        private  void LoadNew()
        {
            ListDB = new ObservableCollection<ControlInfo>(DataProvider.Ins.DB.ControlInfos);
            List = new List<ControlInfo>();
            CategoryList = new ObservableCollection<Category>(DataProvider.Ins.DB.Categories);

            // populate devices/tests/levels so detail area can bind immediately
            DeviceList = new ObservableCollection<Device>(DataProvider.Ins.DB.Devices);
            TestList = new ObservableCollection<Test>(DataProvider.Ins.DB.Tests);
            LevelList = new ObservableCollection<LevelQc>(DataProvider.Ins.DB.LevelQcs);
        }

        private bool CanChangeTest(Test p) => SelectedType != null && SelectedTest != null;

        public void ReLoad()
        {
            LOT = string.Empty;
            ListDB = new ObservableCollection<ControlInfo>(DataProvider.Ins.DB.ControlInfos);
            List = ListDB.ToList();
            DetailList = new ObservableCollection<ControlInfoDetail>();
            SelectedDetail = null;
        }

        // New: load the ControlInfoDetail list for currently selected ControlInfo (LOT)
        private bool _isLoadingDetails = false;

        private void LoadDetailsForSelectedItem()
        {
            if (SelectedItem == null)
            {
                DetailList = new ObservableCollection<ControlInfoDetail>();
                return;
            }

            try
            {
                _isLoadingDetails = true;

                // Use the shared context from DataProvider so entities are attached to the same context
                var db = DataProvider.Ins.DB;

                // NOTE: do NOT use AsNoTracking() here — we want the returned entities attached
                var details = db.ControlInfoDetails
                                .Include(d => d.IdDeviceNavigation)
                                .Include(d => d.IdTestNavigation)
                                .Include(d => d.IdLevelNavigation)
                                .Where(d => d.IdControlInfo == SelectedItem.Id)
                                .OrderBy(d => d.IdDeviceNavigation.Name)
                                .ThenBy(d => d.IdLevel)
                                .ThenBy(d => d.IdTest)
                                .ToList();

                DetailList = new ObservableCollection<ControlInfoDetail>(details);

                // Important: do NOT set SelectedDevice/SelectedTest/SelectedLevel here.
                // The requirement is that ComboBoxes update only when a Detail row is explicitly selected.
                // Clear any selected detail so ComboBoxes remain unchanged until user picks a row.
                SelectedDetail = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Load detail ranges failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                _isLoadingDetails = false;
            }
        }

        // New: update TestList when device changes (invoked by DeviceSelectionChangedCommand)
        private void UpdateTestList()
        {
            if (SelectedDevice != null)
            {
                TestList = new ObservableCollection<Test>(
                    DataProvider.Ins.DB.DeviceTests
                        .Where(s => s.IdDevice == SelectedDevice.Id)
                        .Select(s => s.IdTestNavigation)
                        .OrderBy(s => s.Index)
                        .ToList());
            }
            else
            {
                TestList = new ObservableCollection<Test>(DataProvider.Ins.DB.Tests);
            }
        }

        // New: filter DetailList by selected device (if any)
        private void FilterDetailsByDevice()
        {
            if (SelectedItem == null)
            {
                DetailList = new ObservableCollection<ControlInfoDetail>();
                return;
            }

            try
            {
                // Use the shared context instance so SaveChanges acts on the same tracked entities
                var db = DataProvider.Ins.DB;

                var query = db.ControlInfoDetails
                              .Include(d => d.IdDeviceNavigation)
                              .Include(d => d.IdTestNavigation)
                              .Include(d => d.IdLevelNavigation)
                              .Where(d => d.IdControlInfo == SelectedItem.Id);

                if (SelectedDevice != null)
                {
                    query = query.Where(d => d.IdDevice == SelectedDevice.Id);
                }

                // Do not use AsNoTracking() so entities remain tracked by the shared context
                var details = query.OrderBy(d => d.IdDeviceNavigation.Name)
                                   .ThenBy(d => d.IdLevel)
                                   .ThenBy(d => d.IdTest)
                                   .ToList();

                DetailList = new ObservableCollection<ControlInfoDetail>(details);

                // keep SelectedDetail cleared so combo boxes don't change unexpectedly
                SelectedDetail = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Filter detail ranges failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // New: update editor visibility when test changes
        private void UpdateView()
        {
            if (SelectedTest == null) return;

            if (SelectedTest.TestType == 1)
            {
                QualitativeMeanVisibility = true;
                QuantativeVisibility = false;
            }
            else
            {
                QualitativeMeanVisibility = false;
                QuantativeVisibility = true;
            }
        }

        // New: determine whether AddRange can execute
        private bool CanAddRange()
        {
            // require selected LOT, device, test, level
            if (SelectedItem == null) return false;
            if (SelectedDevice == null) return false;
            if (SelectedTest == null) return false;
            if (SelectedLevel == null) return false;

            // require either qualitative value (test type 1) or mean/sd for quantitative (test type 2)
            if (SelectedTest.TestType == 1)
            {
                return !string.IsNullOrWhiteSpace(QualitativeMean);
            }
            else
            {
                return MeanNSX.HasValue || CurMean.HasValue;
            }
        }

        // New: determine whether EditRange can execute
        private bool CanEditRange()
        {
            return SelectedDetail != null &&
                   (SelectedDetail.IdDevice != SelectedDevice?.Id ||
                    SelectedDetail.IdLevel != SelectedLevel?.Id ||
                    SelectedDetail.IdTest != SelectedTest?.Id ||
                    SelectedDetail.QualitativeMean != QualitativeMean ||
                    SelectedDetail.MeanNsx != MeanNSX ||
                    SelectedDetail.SdNsx != SDNSX ||
                    SelectedDetail.MeanApp != MeanPXN ||
                    SelectedDetail.SdApp != SdPXN ||
                    SelectedDetail.CurMean != CurMean ||
                    SelectedDetail.CurSd != CurSd ||
                    SelectedDetail.Lot != LOT ||
                    SelectedDetail.Status != ischecked_detail);
        }

        // Edit selected range (ControlInfoDetail) - mirrors QC_DetailViewModel.Edit for details
        private void EditRange()
        {
            if (SelectedDetail == null) return;

            if (SelectedDetail.IdTestNavigation != null && SelectedDetail.IdTestNavigation.TestType == 1)
            {
                SelectedDetail.IdDevice = SelectedDevice?.Id;
                SelectedDetail.IdDeviceNavigation = SelectedDevice;
                SelectedDetail.IdLevelNavigation = SelectedLevel;
                SelectedDetail.IdLevel = SelectedLevel.Id;
                SelectedDetail.IdTest = SelectedTest.Id;
                SelectedDetail.Status = ischecked_detail;
                SelectedDetail.Lot = LOT;
                SelectedDetail.QualitativeMean = QualitativeMean;
            }
            else
            {
                SelectedDetail.IdDevice = SelectedDevice?.Id;
                SelectedDetail.IdDeviceNavigation = SelectedDevice;
                SelectedDetail.IdLevelNavigation = SelectedLevel;
                SelectedDetail.IdLevel = SelectedLevel.Id;
                SelectedDetail.IdTest = SelectedTest.Id;
                SelectedDetail.MeanNsx = MeanNSX;
                SelectedDetail.SdNsx = SDNSX;
                SelectedDetail.MeanApp = MeanPXN;
                SelectedDetail.SdApp = SdPXN;
                SelectedDetail.Status = ischecked_detail;
                SelectedDetail.Lot = LOT;
                SelectedDetail.CurSd = CurSd;
                SelectedDetail.CurMean = CurMean;
            }

            try
            {
                SelectedDetail.MeanSdUpdatedAt = DateTime.UtcNow;
                DataProvider.Ins.DB.SaveChanges();
                MessageBox.Show("Cập nhật range thành công. Kết quả hiện có KHÔNG thay đổi tự động. Để áp dụng lên kết quả cũ, dùng 'Áp dụng Range'.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);

                // reload UI list respecting current device filter
                FilterDetailsByDevice();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating range: {ex}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Stop);
            }
        }

        private bool CanDeleteRange()
        {
            return SelectedDetail != null;
        }

        private void DeleteRange()
        {
            if (SelectedDetail == null) return;

            var deleteItem = DataProvider.Ins.DB.ControlInfoDetails.FirstOrDefault(s => s.Id == SelectedDetail.Id);
            if (deleteItem == null) return;

            var result = MessageBox.Show($"Bạn có muốn xóa range: Test={SelectedDetail.IdTestNavigation?.Name} Device={SelectedDetail.IdDeviceNavigation?.Name} Level={SelectedDetail.IdLevelNavigation?.Name} LOT={SelectedDetail.Lot} ?", "Confirmation", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    DataProvider.Ins.DB.Remove(deleteItem);
                    DataProvider.Ins.DB.SaveChanges();
                    MessageBox.Show("Xóa range thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);

                    // reload lists
                    FilterDetailsByDevice();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting range: {ex}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Stop);
                }
            }
        }

        // Apply mean/sd to ALL related Test results (same Device + Test + Level) within user-specified datetime range
        private void ApplyMeanSdToAllRelatedTests(DateTime rangeStart, DateTime rangeEnd)
        {
            if (SelectedDetail == null) return;

            try
            {
                double? mean = SelectedDetail.CurMean ?? SelectedDetail.MeanApp ?? SelectedDetail.MeanNsx;
                double? sd = SelectedDetail.CurSd ?? SelectedDetail.SdApp ?? SelectedDetail.SdNsx;

                if (!mean.HasValue || !sd.HasValue)
                {
                    MessageBox.Show("Không có Mean/SD hợp lệ để áp dụng.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (sd.Value == 0) sd = 0.001;

                // Select results that share same Device, Test and Level and fall within the inclusive datetime range
                var relatedResults = DataProvider.Ins.DB.Results
                    .Where(r => r.IdDevice == SelectedDetail.IdDevice
                                && r.IdTest == SelectedDetail.IdTest
                                && r.IdLevel == SelectedDetail.IdLevel
                                && r.DateRun >= rangeStart
                                && r.DateRun <= rangeEnd)
                    .ToList();

                if (relatedResults.Count == 0)
                {
                    MessageBox.Show($"Không tìm thấy kết quả liên quan trong khoảng {rangeStart} - {rangeEnd}.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var confirm = MessageBox.Show($"Sẽ cập nhật {relatedResults.Count} kết quả (từ {rangeStart} đến {rangeEnd}) — gán AppliedMean/AppliedSd và tính lại ZScore. Tiếp tục?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (confirm != MessageBoxResult.Yes) return;

                var now = DateTime.UtcNow;
                foreach (var r in relatedResults)
                {
                    r.AppliedMean = mean;
                    r.AppliedSd = sd;
                    r.AppliedAt = now;

                    if (r.Result1.HasValue)
                    {
                        r.ZScore = Math.Round((r.Result1.Value - mean.Value) / sd.Value, 2);
                    }
                    else
                    {
                        r.ZScore = null;
                    }
                }

                DataProvider.Ins.DB.SaveChanges();

                // notify chart(s) to refresh
                QC_Management.Services.ResultChangeNotifier.Notify();

                MessageBox.Show("Áp dụng Range cho các kết quả liên quan thành công.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);

                // refresh detail list
                FilterDetailsByDevice();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi áp dụng Range tất cả: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Add this method inside the QC_InformationViewModel class
        private void AddRange()
        {
            if (SelectedItem == null) return;
            if (SelectedDevice == null || SelectedTest == null || SelectedLevel == null) return;

            ControlInfoDetail detail;
            if (SelectedTest.TestType == 1)
            {
                if (string.IsNullOrWhiteSpace(QualitativeMean))
                {
                    MessageBox.Show("Vui lòng nhập giá trị định tính!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Stop);
                    return;
                }

                detail = new ControlInfoDetail
                {
                    IdDevice = SelectedDevice.Id,
                    IdDeviceNavigation = SelectedDevice,
                    IdControlInfoNavigation = SelectedItem,
                    IdControlInfo = SelectedItem.Id,
                    IdLevelNavigation = SelectedLevel,
                    IdLevel = SelectedLevel.Id,
                    IdTestNavigation = SelectedTest,
                    IdTest = SelectedTest.Id,
                    QualitativeMean = QualitativeMean,
                    // Use user-selected status/LOT from the editor controls (IsChecked / LOT),
                    // not the SelectedItem (ControlInfo) values.
                    Status = ischecked_detail,
                    Lot = LOT
                };
            }
            else
            {
                detail = new ControlInfoDetail
                {
                    IdDevice = SelectedDevice.Id,
                    IdDeviceNavigation = SelectedDevice,
                    IdControlInfoNavigation = SelectedItem,
                    IdControlInfo = SelectedItem.Id,
                    IdLevelNavigation = SelectedLevel,
                    IdLevel = SelectedLevel.Id,
                    IdTestNavigation = SelectedTest,
                    IdTest = SelectedTest.Id,
                    MeanNsx = MeanNSX,
                    SdNsx = SDNSX,
                    MeanApp = MeanPXN,
                    SdApp = SdPXN,
                    // Use user-selected status/LOT from the editor controls (IsChecked / LOT),
                    // not the SelectedItem (ControlInfo) values.
                    Status = ischecked_detail,
                    CurSd = CurSd,
                    CurMean = CurMean,
                    Lot = LOT
                };
            }

            try
            {
                DataProvider.Ins.DB.ControlInfoDetails.Add(detail);
                DataProvider.Ins.DB.SaveChanges();

                MessageBox.Show("Thêm range thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);

                // reload details for UI (respecting any device selection)
                FilterDetailsByDevice();

                // clear editor fields
                MeanNSX = null;
                SDNSX = null;
                MeanPXN = null;
                SdPXN = null;
                CurMean = null;
                CurSd = null;
                QualitativeMean = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding range: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Stop);
            }
        }
    }
}
