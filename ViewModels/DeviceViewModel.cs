using QC_Management.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Collections.Generic;
using System.Text.Json;
using System.ComponentModel;
using Microsoft.EntityFrameworkCore;

namespace QC_Management.ViewModels
{
    public class DeviceViewModel : BaseViewModel
    {
        private ObservableCollection<Device> _List;
        public ObservableCollection<Device> List { get => _List; set { _List = value; OnPropertyChanged(); } }
        private ObservableCollection<Device> _ListDB;
        public ObservableCollection<Device> ListDB { get => _ListDB; set { _ListDB = value; OnPropertyChanged(); } }

        private ObservableCollection<Test> _TestList;
        public ObservableCollection<Test> TestList { get => _TestList; set { _TestList = value; OnPropertyChanged(); } }

        private ObservableCollection<DeviceTest> _DeviceTestList;
        public ObservableCollection<DeviceTest> DeviceTestList { get => _DeviceTestList; set { _DeviceTestList = value; OnPropertyChanged(); } }
        private ObservableCollection<DeviceTest> _DeviceTestListDB;
        public ObservableCollection<DeviceTest> DeviceTestListDB { get => _DeviceTestListDB; set { _DeviceTestListDB = value; OnPropertyChanged(); } }

        private ObservableCollection<Category> _CategoryList;
        public ObservableCollection<Category> CategorytList { get => _CategoryList; set { _CategoryList = value; OnPropertyChanged(); } }
        public ICommand AddCommand { get; set; }
        public ICommand EditCommand { get; set; }
        public ICommand DeleteCommand { get; set; }
        public ICommand RomoveCommand { get; set; }
        public ICommand DeviceSelectionChangedCommand { get; set; }
        public ICommand CategorySelectionChangedCommand { get; set; }
        public ICommand AddTestCommand { get; set; }

        public ICommand LoadedCommand { get; set; }

        private Device _SelectedItem;
        public Device SelectedItem
        {
            get => _SelectedItem;
            set
            {
                _SelectedItem = value;
                OnPropertyChanged();
                // keep display name in sync
                if (SelectedItem != null)
                {
                    DisplayName = SelectedItem.Name;
                }

                // notify dependent view state
                OnPropertyChanged(nameof(IsDeviceSelected));
            }
        }

        // Exposed convenience property that UI can bind to for visibility
        public bool IsDeviceSelected => SelectedItem != null;

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

        private DeviceTest _SelectedDeviceTest;
        public DeviceTest SelectedDeviceTest
        {
            get => _SelectedDeviceTest;
            set
            {
                _SelectedDeviceTest = value;
                OnPropertyChanged();
                // Populate checkbox list when a DeviceTest is selected
                PopulateDeviceTestRules(_SelectedDeviceTest);
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

        private string _DisplayName;
        public string DisplayName { get => _DisplayName; set { _DisplayName = value; OnPropertyChanged(); } }

        // keep a snapshot of originally assigned Test Ids for the selected device
        private List<int> _originalAssignedTestIds = new();

        // --- NEW: Westgard rule selection support for DeviceTest ---
        public class WestgardRuleItem : INotifyPropertyChanged
        {
            public string Key { get; set; } = string.Empty;
            public string Display { get; set; } = string.Empty;

            private bool _isChecked;
            public bool IsChecked
            {
                get => _isChecked;
                set
                {
                    if (_isChecked == value) return;
                    _isChecked = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsChecked)));
                }
            }

            public event PropertyChangedEventHandler? PropertyChanged;
        }

        // UI shows only these main rules. Keys are machine keys saved in DB.
        private static readonly (string Key, string Display)[] _availableRules = new[]
        {
            ("1_2S", "1-2S"),
            ("1_3S", "1-3S"),
            ("2_2S", "2-2S"),
            ("R-4s", "R4S"),
            ("10X", "10X"),
            ("4_1S", "4-1S"),
            // Add qualitative check option so users can opt-in to qualitative acceptance checks
            ("QUAL", "Qualitative")
        };

        private ObservableCollection<WestgardRuleItem> _CurrentDeviceTestWestgardRuleItems = new();
        public ObservableCollection<WestgardRuleItem> CurrentDeviceTestWestgardRuleItems
        {
            get => _CurrentDeviceTestWestgardRuleItems;
            set { _CurrentDeviceTestWestgardRuleItems = value; OnPropertyChanged(); }
        }

        // New commands
        public ICommand SaveRulesCommand { get; set; }
        public ICommand ApplyRulesToAllCommand { get; set; }

        public DeviceViewModel()
        {
            LoadedCommand = new RelayCommand<UserRole>((p) =>
            {
                return true;
            }, (p) =>
            {
                LoadNew();
            });

            AddCommand = new RelayCommand<DeviceTest>((p) =>
            {
                if (string.IsNullOrEmpty(DisplayName))
                    return false;

                return true;

            }, (p) =>
            {
                var device = new Device() { Name = DisplayName, IdCategory = SelectedCategory?.Id, IdCategoryNavigation = SelectedCategory };

                try
                {
                    DataProvider.Ins.DB.Devices.Add(device);
                    DataProvider.Ins.DB.SaveChanges();
                    MessageBox.Show("Thêm thông tin thiết bị thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    List.Add(device);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Stop);
                }
            });


            EditCommand = new RelayCommand<Device>((p) =>
            {
                if (SelectedItem == null)
                    return false;
                else if (SelectedItem.Name == DisplayName && AreDeviceTestsUnchanged())
                    return false;
                else return true;

            }, (p) =>
            {
                try
                {
                    // update device fields
                    var deviceEditor = DataProvider.Ins.DB.Devices.Where(x => x.Id == SelectedItem.Id).SingleOrDefault();
                    if (deviceEditor == null) throw new InvalidOperationException("Device not found in database.");

                    deviceEditor.Name = DisplayName;
                    deviceEditor.IdCategory = SelectedCategory?.Id;
                    deviceEditor.IdCategoryNavigation = SelectedCategory;

                    // sync DeviceTests:
                    var currentTestIds = DeviceTestList.Select(d => d.IdTest).ToHashSet();

                    // find DB entries currently assigned to this device
                    var dbAssigned = DataProvider.Ins.DB.DeviceTests.Where(d => d.IdDevice == SelectedItem.Id).ToList();

                    // to remove: dbAssigned where test id not in currentTestIds
                    var toRemove = dbAssigned.Where(d => !currentTestIds.Contains(d.IdTest)).ToList();
                    foreach (var rem in toRemove)
                    {
                        DataProvider.Ins.DB.DeviceTests.Remove(rem);
                    }

                    // to add: items in DeviceTestList where original snapshot did not contain the test id
                    var toAdd = DeviceTestList.Where(d => !_originalAssignedTestIds.Contains(d.IdTest)).ToList();
                    foreach (var add in toAdd)
                    {
                        var newDt = new DeviceTest
                        {
                            IdDevice = SelectedItem.Id,
                            IdDeviceNavigation = SelectedItem,
                            IdTest = add.IdTest,
                            IdTestNavigation = add.IdTestNavigation,
                            WestgardRulesJson = add.WestgardRulesJson // persist selection
                        };
                        DataProvider.Ins.DB.DeviceTests.Add(newDt);
                    }

                    // update existing db-assigned entries with any changed WestgardRulesJson
                    var toUpdate = DeviceTestList.Where(d => d.Id != 0).ToList();
                    foreach (var upd in toUpdate)
                    {
                        var dbDt = DataProvider.Ins.DB.DeviceTests.FirstOrDefault(x => x.Id == upd.Id);
                        if (dbDt != null)
                        {
                            dbDt.WestgardRulesJson = upd.WestgardRulesJson;
                            DataProvider.Ins.DB.DeviceTests.Update(dbDt);
                        }
                    }

                    DataProvider.Ins.DB.SaveChanges();
                    MessageBox.Show("Cập nhật thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);

                    // refresh DB caches
                    DeviceTestListDB = new ObservableCollection<DeviceTest>(DataProvider.Ins.DB.DeviceTests.Include(dt => dt.IdTestNavigation));
                    // update snapshot
                    _originalAssignedTestIds = DeviceTestListDB.Where(d => d.IdDevice == SelectedItem.Id).Select(d => d.IdTest).ToList();
                    // reflect DB-assigned list (use DB items so they have correct Ids)
                    DeviceTestList = new ObservableCollection<DeviceTest>(DeviceTestListDB.Where(d => d.IdDevice == SelectedItem.Id));
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Stop);
                }
            });

            // allow the button to be clickable when a device and a test are selected;
            // Execute will show a message if the test is already assigned.
            AddTestCommand = new RelayCommand<Test>((p) =>
            {
                return SelectedItem != null && SelectedTest != null;
            }, (p) =>
            {
                if (SelectedItem == null || SelectedTest == null) return;

                if (DeviceTestList != null && DeviceTestList.Any(dt => dt.IdTest == SelectedTest.Id))
                {
                    MessageBox.Show($"Test '{SelectedTest.Name}' is already assigned to the selected device.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var deviceTest = new DeviceTest
                {
                    // Id left as 0 -> indicates new, not yet persisted
                    IdDevice = SelectedItem.Id,
                    IdDeviceNavigation = SelectedItem,
                    IdTest = SelectedTest.Id,
                    IdTestNavigation = SelectedTest
                };

                DeviceTestList.Add(deviceTest);
            });

            DeviceSelectionChangedCommand = new RelayCommand<Device>((p) =>
            {
                if (SelectedItem == null) return false;
                else return true;

            }, (p) =>
            {
                // Build an in-memory copy of DB-assigned DeviceTests for the selected device.
                var dbAssigned = DeviceTestListDB.Where(s => s.IdDevice == SelectedItem.Id).ToList();

                // snapshot original assigned test ids to detect additions/removals on save
                _originalAssignedTestIds = dbAssigned.Select(d => d.IdTest).ToList();

                DeviceTestList = new ObservableCollection<DeviceTest>(dbAssigned.Select(d => new DeviceTest
                {
                    Id = d.Id,
                    IdDevice = d.IdDevice,
                    IdDeviceNavigation = d.IdDeviceNavigation,
                    IdTest = d.IdTest,
                    IdTestNavigation = d.IdTestNavigation,
                    WestgardRulesJson = d.WestgardRulesJson
                }));

                // If navigation is not loaded, fall back to lookup from CategorytList
                SelectedCategory = SelectedItem.IdCategoryNavigation ?? CategorytList?.FirstOrDefault(c => c.Id == SelectedItem.IdCategory);
            });

            CategorySelectionChangedCommand = new RelayCommand<Device>((p) =>
            {
                return true;

            }, (p) =>
            {
                // Null-safe: if ListDB not loaded, avoid NRE
                if (ListDB == null)
                {
                    List = new ObservableCollection<Device>();
                    return;
                }

                // If no category selected, show all devices
                if (SelectedCategory == null)
                {
                    List = new ObservableCollection<Device>(ListDB);
                    return;
                }

                List = new ObservableCollection<Device>(ListDB.Where(s => s.IdCategory == SelectedCategory.Id));
            });

            RomoveCommand = new RelayCommand<DeviceTest>((p) =>
            {
                if (SelectedDeviceTest == null) return false;
                else return true;

            }, (p) =>
            {
                // Remove from in-memory assigned list only. Persist removals on Save (EditCommand).
                if (SelectedDeviceTest == null) return;
                DeviceTestList.Remove(SelectedDeviceTest);
            });

            // Initialize new commands
            SaveRulesCommand = new RelayCommand<object>((p) =>
            {
                return SelectedDeviceTest != null;
            }, (p) =>
            {
                SaveSelectedDeviceTestRules();
            });

            ApplyRulesToAllCommand = new RelayCommand<object>((p) =>
            {
                return SelectedItem != null;
            }, (p) =>
            {
                ApplySelectedRulesToAllDeviceTests();
            });
        }

        private bool AreDeviceTestsUnchanged()
        {
            var currentIds = DeviceTestList?.Select(d => d.IdTest).OrderBy(i => i).ToList() ?? new List<int>();
            var originalIds = _originalAssignedTestIds.OrderBy(i => i).ToList();
            return currentIds.SequenceEqual(originalIds);
        }

        private void LoadNew()
        {
            // Use a fresh DbContext to avoid reading stale tracked entities from a long-lived shared context.
            // This ensures UI shows the latest persisted WestgardRulesJson when navigating back to the view.
            try
            {
                using var db = new QcManagmentContext();

                CategorytList = new ObservableCollection<Category>(db.Categories.AsNoTracking().ToList());
                // Include category navigation so SelectedItem.IdCategoryNavigation is available after selection
                ListDB = new ObservableCollection<Device>(db.Devices.Include(d => d.IdCategoryNavigation).AsNoTracking().ToList());

                TestList = new ObservableCollection<Test>(db.Tests.AsNoTracking().ToList());
                List = new ObservableCollection<Device>(ListDB);

                DeviceTestListDB = new ObservableCollection<DeviceTest>(
                    db.DeviceTests
                      .AsNoTracking()
                      .Include(dt => dt.IdTestNavigation)
                      .ToList()
                );

                // clear any transient assigned list
                DeviceTestList = new ObservableCollection<DeviceTest>();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading device data: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void Reload()
        {
            SelectedItem = null;
            OnPropertyChanged(nameof(IsDeviceSelected));
        }

        // Populate CurrentDeviceTestWestgardRuleItems from SelectedDeviceTest.WestgardRulesJson
        // Accepts stored JSON that may contain either main keys or previously-stored detailed keys.
        private void PopulateDeviceTestRules(DeviceTest? dt)
        {
            // detach previous handlers
            foreach (var item in CurrentDeviceTestWestgardRuleItems)
            {
                item.PropertyChanged -= RuleItem_PropertyChanged;
            }

            CurrentDeviceTestWestgardRuleItems = new ObservableCollection<WestgardRuleItem>();

            if (dt == null)
            {
                OnPropertyChanged(nameof(CurrentDeviceTestWestgardRuleItems));
                return;
            }

            // Parse existing JSON (may contain main keys or expanded detailed keys).
            HashSet<string> stored = new(StringComparer.OrdinalIgnoreCase);
            if (!string.IsNullOrEmpty(dt.WestgardRulesJson))
            {
                try
                {
                    var parsed = JsonSerializer.Deserialize<List<string>>(dt.WestgardRulesJson);
                    if (parsed != null)
                    {
                        foreach (var s in parsed.Select(x => x?.Trim()).Where(x => !string.IsNullOrEmpty(x)))
                        {
                            stored.Add(s!);
                        }
                    }
                }
                catch
                {
                    stored.Clear();
                }
            }

            // Helper to decide whether stored contains any variant that maps to mainKey
            bool StoredContainsMain(string mainKey)
            {
                if (stored.Contains(mainKey, StringComparer.OrdinalIgnoreCase)) return true;

                // map main -> detailed variants that might be present in older saved data
                switch (mainKey)
                {
                    case "4_1S":
                        return stored.Overlaps(new[] {
                            "4_1S (+) (L)", "4_1S (-) (L)", "4_1S (L)",
                            "4_1S (+) (cross)", "4_1S (-) (cross)", "4_1S (cross)"
                        });
                    case "10X":
                        return stored.Overlaps(new[] {
                            "10X (+) (L)", "10X (-) (L)", "10X (+) (cross)", "10X (-) (cross)"
                        });
                    case "2_2S":
                        return stored.Overlaps(new[] {
                            "2_2S (+) (L)", "2_2S (-) (L)", "2_2S (+) (cross)", "2_2S (-) (cross)"
                        });
                    case "R-4s":
                        return stored.Overlaps(new[] { "R-4s" });
                    case "1_2S":
                        return stored.Overlaps(new[] { "1_2S" });
                    case "1_3S":
                        return stored.Overlaps(new[] { "1_3S" });
                    case "QUAL":
                        // Accept older variant names or synonyms
                        return stored.Overlaps(new[] { "QUAL", "QUAL_FAIL", "QUALITATIVE" });
                    default:
                        return false;
                }
            }

            foreach (var r in _availableRules)
            {
                var it = new WestgardRuleItem
                {
                    Key = r.Key,
                    Display = r.Display,
                    IsChecked = StoredContainsMain(r.Key)
                };
                it.PropertyChanged += RuleItem_PropertyChanged;
                CurrentDeviceTestWestgardRuleItems.Add(it);
            }

            OnPropertyChanged(nameof(CurrentDeviceTestWestgardRuleItems));
        }

        // When a checkbox toggles, update the SelectedDeviceTest.WestgardRulesJson (store main keys).
        private void RuleItem_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(WestgardRuleItem.IsChecked)) return;
            if (SelectedDeviceTest == null) return;

            var selectedKeys = CurrentDeviceTestWestgardRuleItems
                .Where(x => x.IsChecked)
                .Select(x => x.Key)
                .ToList();

            try
            {
                SelectedDeviceTest.WestgardRulesJson = JsonSerializer.Serialize(selectedKeys);
            }
            catch
            {
                SelectedDeviceTest.WestgardRulesJson = null;
            }
        }

        // Persist only the selected DeviceTest rules (save single DeviceTest)
        private void SaveSelectedDeviceTestRules()
        {
            if (SelectedDeviceTest == null)
            {
                MessageBox.Show("Vui lòng chọn một xét nghiệm (Assigned test) để lưu rule.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using var db = new QcManagmentContext();

                // If this is an existing DB row
                if (SelectedDeviceTest.Id != 0)
                {
                    var dbDt = db.DeviceTests.Include(dt => dt.IdTestNavigation).FirstOrDefault(x => x.Id == SelectedDeviceTest.Id);
                    if (dbDt != null)
                    {
                        dbDt.WestgardRulesJson = SelectedDeviceTest.WestgardRulesJson;
                        db.DeviceTests.Update(dbDt);
                        db.SaveChanges();

                        // refresh in-memory DB cache from fresh context to ensure newest values
                        DeviceTestListDB = new ObservableCollection<DeviceTest>(
                            db.DeviceTests.AsNoTracking().Include(dt => dt.IdTestNavigation).ToList()
                        );

                        // reflect DB-assigned list (use DB items so they have correct Ids)
                        if (SelectedItem != null)
                            DeviceTestList = new ObservableCollection<DeviceTest>(DeviceTestListDB.Where(d => d.IdDevice == SelectedItem.Id));
                    }
                    else
                    {
                        MessageBox.Show("Không tìm thấy bản ghi DeviceTest trong cơ sở dữ liệu.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }
                else
                {
                    // New in-memory DeviceTest -> persist as new row
                    if (SelectedItem == null)
                    {
                        MessageBox.Show("Cannot create DeviceTest without selecting a Device.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    var newDt = new DeviceTest
                    {
                        IdDevice = SelectedItem.Id,
                        IdTest = SelectedDeviceTest.IdTest,
                        WestgardRulesJson = SelectedDeviceTest.WestgardRulesJson
                    };

                    db.DeviceTests.Add(newDt);
                    db.SaveChanges();

                    // refresh in-memory lists with persisted row (refresh from fresh context)
                    DeviceTestListDB = new ObservableCollection<DeviceTest>(db.DeviceTests.AsNoTracking().Include(dt => dt.IdTestNavigation).ToList());
                    DeviceTestList = new ObservableCollection<DeviceTest>(DeviceTestListDB.Where(d => d.IdDevice == SelectedItem.Id));
                }

                MessageBox.Show("Lưu rule thành công.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi lưu rule: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Apply the currently selected rule set (from CurrentDeviceTestWestgardRuleItems) to all DeviceTest rows of the selected device
        private void ApplySelectedRulesToAllDeviceTests()
        {
            if (SelectedItem == null)
            {
                MessageBox.Show("Vui lòng chọn một thiết bị trước khi áp dụng.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var selectedKeys = CurrentDeviceTestWestgardRuleItems.Where(x => x.IsChecked).Select(x => x.Key).ToList();
            string json;
            try
            {
                json = JsonSerializer.Serialize(selectedKeys);
            }
            catch
            {
                MessageBox.Show("Không thể serialize danh sách rule.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                using var db = new QcManagmentContext();

                // Load DeviceTest rows for this device with test navigation
                var list = db.DeviceTests.Include(dt => dt.IdTestNavigation).Where(d => d.IdDevice == SelectedItem.Id).ToList();
                if (!list.Any())
                {
                    MessageBox.Show("Không tìm thấy xét nghiệm nào gán cho thiết bị này.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                foreach (var dt in list)
                {
                    dt.WestgardRulesJson = json;
                    db.DeviceTests.Update(dt);
                }

                db.SaveChanges();

                // refresh in-memory caches from fresh context (include navigation)
                DeviceTestListDB = new ObservableCollection<DeviceTest>(db.DeviceTests.AsNoTracking().Include(dt => dt.IdTestNavigation).ToList());
                DeviceTestList = new ObservableCollection<DeviceTest>(DeviceTestListDB.Where(d => d.IdDevice == SelectedItem.Id));

                MessageBox.Show("Áp dụng rule cho tất cả xét nghiệm thành công.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi áp dụng rule: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}