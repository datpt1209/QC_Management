using QC_Management.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace QC_Management.ViewModels
{
    public class TestViewModel : BaseViewModel
    {
        private ObservableCollection<Test> _List;
        public ObservableCollection<Test> List { get => _List; set { _List = value; OnPropertyChanged(); } }

        private ObservableCollection<Category> _CategoryList;
        public ObservableCollection<Category> CategoryList { get => _CategoryList; set { _CategoryList = value; OnPropertyChanged(); } }

        private ObservableCollection<TestType> _TestTypeList;
        public ObservableCollection<TestType> TestTypeList { get => _TestTypeList; set { _TestTypeList = value; OnPropertyChanged(); } }

        private ObservableCollection<Category> _CategoryByTestList;
        public ObservableCollection<Category> CategoryByTestList { get => _CategoryByTestList; set { _CategoryByTestList = value; OnPropertyChanged(); } }

        private ObservableCollection<UnitTable> _UnitList;
        public ObservableCollection<UnitTable> UnitList { get => _UnitList; set { _UnitList = value; OnPropertyChanged(); } }

        private ObservableCollection<Device> _DeviceList;
        public ObservableCollection<Device> DeviceList { get => _DeviceList; set { _DeviceList = value; OnPropertyChanged(); } }

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

        private TestType _SelectedTestType;
        public TestType SelectedTestType
        {
            get => _SelectedTestType;
            set
            {
                _SelectedTestType = value;
                OnPropertyChanged();

            }
        }

        private Category _SelectedCategoryByTest;
        public Category SelectedCategoryByTest
        {
            get => _SelectedCategoryByTest;
            set
            {
                _SelectedCategoryByTest = value;
                OnPropertyChanged();
            }
        }

        private Test _SelectedItem;
        public Test SelectedItem
        {
            get => _SelectedItem;
            set
            {
                _SelectedItem = value;
                OnPropertyChanged();
                if (SelectedItem != null)
                {
                    if (SelectedItem.Index != null) Index = (int)SelectedItem.Index;
                    DisplayName = SelectedItem.Name;
                    SelectedUnitTable = SelectedItem.IdUnitTableNavigation;
                    SelectedCategory = SelectedItem.IdCategoryNavigation;
                    SelectedCategoryByTest = SelectedItem.IdCategoryNavigation; // sync combobox
                    SelectedTestType = SelectedItem.TestTypeNavigation;
                    ParameterName = SelectedItem.ParameterName;
                    TEaPercent = SelectedItem.TEaPercent;
                    OnlineNumber = SelectedItem.OnlineNumber;

                    // simple integer check per requirement
                    IsQualitative = SelectedItem.TestType == 1;

                    // refresh buttons
                    System.Windows.Input.CommandManager.InvalidateRequerySuggested();
                }
            }
        }
        private UnitTable _SelectedUnitTable;
        public UnitTable SelectedUnitTable
        {
            get => _SelectedUnitTable;
            set
            {
                _SelectedUnitTable = value;

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
        public ICommand AddCommand { get; set; }
        public ICommand AddTestDeviceCommand { get; set; }
        public ICommand CategoryChangedCommand { get; set; }
        public ICommand EditCommand { get; set; }
        public ICommand DeleteCommand { get; set; }
        public ICommand LoadedCommand { get; set; }
        public ICommand CategoryAddCommand { get; set; }
        public ICommand CategoryEditCommand { get; set; }
        public ICommand CategoryDeleteCommand { get; set; }
        public ICommand RefreshCommand { get; set; }

        private string _DisplayName;
        public string DisplayName { get => _DisplayName; set { _DisplayName = value; OnPropertyChanged(); } }

        private int _Index;
        public int Index { get => _Index; set { _Index = value; OnPropertyChanged(); } }

        private string _CategoryDisplayName;
        public string CategoryDisplayName { get => _CategoryDisplayName; set { _CategoryDisplayName = value; OnPropertyChanged(); } }


        // add field/property
        private double? _TEaPercent;
        public double? TEaPercent { get => _TEaPercent; set { _TEaPercent = value; OnPropertyChanged(); } }

        // add property
        private int? _OnlineNumber;
        public int? OnlineNumber { get => _OnlineNumber; set { _OnlineNumber = value; OnPropertyChanged(); } }

        // field/property (rename)
        private string _ParameterName;
        public string ParameterName { get => _ParameterName; set { _ParameterName = value; OnPropertyChanged(); } }



        // fields
        private int? QualitativeTypeId;
        private int? QuantitativeTypeId;

        private bool _IsQualitative;
        public bool IsQualitative
        {
            get => _IsQualitative;
            set
            {
                if (_IsQualitative == value) return;
                _IsQualitative = value;
                OnPropertyChanged();

                // keep SelectedTestType consistent with the checkbox
                if (TestTypeList != null)
                {
                    var id = value ? QualitativeTypeId : QuantitativeTypeId;
                    SelectedTestType = TestTypeList.FirstOrDefault(t => t.Id == id) ?? SelectedTestType;
                }

                // update commands' enabled state immediately
                System.Windows.Input.CommandManager.InvalidateRequerySuggested();
            }
        }

        public TestViewModel()
        {
            var DB = DataProvider.Ins.DB;

            // at top of constructor, after var DB = DataProvider.Ins.DB;
            QualitativeTypeId = 1;
            QuantitativeTypeId = 2;

            // optionally ensure TestTypeList is present (not required if you only use ints)
            TestTypeList = new ObservableCollection<TestType>(DB.TestTypes.ToList());

            LoadedCommand = new RelayCommand<Object>((p) =>
            {
                return true;
            }, (p) =>
            {
                List = new ObservableCollection<Test>(DB.Tests.OrderBy(s => s.Index));
                UnitList = new ObservableCollection<UnitTable>(DataProvider.Ins.DB.UnitTables);
                DeviceList = new ObservableCollection<Device>(DB.Devices);
                CategoryList = new ObservableCollection<Category>(DB.Categories);
                CategoryByTestList = new ObservableCollection<Category>(DB.Categories);


                // Prefer explicit mapping -- if you know Qualitative has Id == 1 use that,
                // otherwise match by name.
                QualitativeTypeId = DB.TestTypes.FirstOrDefault(t => t.Id == 1)?.Id
                    ?? DB.TestTypes.FirstOrDefault(t => (t.Name ?? "").ToLower().Contains("qual"))?.Id;

                QuantitativeTypeId = DB.TestTypes.FirstOrDefault(t => t.Id != QualitativeTypeId)?.Id
                    ?? DB.TestTypes.FirstOrDefault(t => (t.Name ?? "").ToLower().Contains("quant"))?.Id;
            });
            RefreshCommand = new RelayCommand<Object>((p) =>
            {
                return true;
            }, (p) =>
            {
                ReLoad(DB);

            });

            AddCommand = new RelayCommand<Test>((p) =>
            {
                if (SelectedUnitTable == null || string.IsNullOrEmpty(DisplayName) || SelectedCategoryByTest == null)
                    return false;
                return true;
            }, (p) =>
            {
                var chosenTypeId = (IsQualitative ? QualitativeTypeId : QuantitativeTypeId)
                                   ?? SelectedTestType?.Id
                                   ?? QualitativeTypeId  // final fallback if you prefer 1
                                   ;

                var test = new Test()
                {
                    Index = Index,
                    Name = DisplayName,
                    IdCategory = SelectedCategoryByTest.Id,
                    IdUnitTable = SelectedUnitTable.Id,
                    TestType = chosenTypeId,
                    ParameterName = ParameterName,
                    TEaPercent = TEaPercent,
                    OnlineNumber = OnlineNumber
                };

                try
                {
                    DataProvider.Ins.DB.Tests.Add(test);
                    DataProvider.Ins.DB.SaveChanges();
                    MessageBox.Show("Thêm Test thành công!");
                    List.Add(test);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex}");
                }
            });

            // EditCommand - CanExecute
            EditCommand = new RelayCommand<Test>((p) =>
            {
                // require selection + required references
                if (SelectedItem == null || SelectedUnitTable == null || SelectedCategoryByTest == null)
                    return false;

                // detect any difference (null-safe comparisons)
                bool changed =
                    SelectedItem.Index != Index ||
                    (SelectedItem.Name ?? "") != (DisplayName ?? "") ||
                    SelectedItem.IdUnitTable != SelectedUnitTable?.Id ||
                    SelectedItem.IdCategory != SelectedCategoryByTest?.Id ||
                    SelectedItem.TestType != SelectedTestType?.Id ||
                    (SelectedItem.ParameterName ?? "") != (ParameterName ?? "") ||
                    SelectedItem.TEaPercent != TEaPercent ||
                    SelectedItem.OnlineNumber != OnlineNumber;

                return changed;
            }, (p) =>
            {
                var testEditor = DataProvider.Ins.DB.Tests.Where(x => x.Id == SelectedItem.Id).SingleOrDefault();
                if(testEditor == null)
                {
                    MessageBox.Show("Không tìm thấy Test để cập nhật.");
                    return;
                }
                testEditor.Index = Index;
                testEditor.Name = DisplayName;
                testEditor.IdUnitTable = SelectedUnitTable.Id;
                testEditor.IdCategory = SelectedCategoryByTest.Id;
                testEditor.TestType = IsQualitative ? QualitativeTypeId : QuantitativeTypeId;
                testEditor.ParameterName = ParameterName;
                testEditor.TEaPercent = TEaPercent;
                testEditor.OnlineNumber = OnlineNumber;

                try
                {
                    DataProvider.Ins.DB.SaveChanges();
                    MessageBox.Show("Cập nhật Test thành công!");
                    List = new ObservableCollection<Test>(DB.Tests.Where(s=> s.IdCategory == SelectedCategory.Id));

                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex}");
                }

            });

            DeleteCommand = new RelayCommand<Test>((p) =>
            {
                if (SelectedItem == null)
                    return false;
                else
                {
                    return true;
                }

            }, (p) =>
            {
                MessageBoxResult result = MessageBox.Show($"Bạn có muốn xóa Test: {SelectedItem.Name}?", "Confirmation", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        DB.Tests.Remove(SelectedItem);
                        DataProvider.Ins.DB.SaveChanges();
                        List.Remove(SelectedItem);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error: {ex}");
                    }
                }
                else return;
            });

            CategoryChangedCommand = new RelayCommand<Category>((p) =>
            {
                if (SelectedCategory == null) return false;
                else
                    return true;

            }, (p) =>
            {
                List = new ObservableCollection<Test>(DataProvider.Ins.DB.Tests.Where(x => x.IdCategory == SelectedCategory.Id));
                SelectedCategoryByTest = SelectedCategory;
                CategoryDisplayName = SelectedCategory.Name;
            });

            CategoryAddCommand = new RelayCommand<Category>((p) =>
            {
                if (string.IsNullOrEmpty(CategoryDisplayName))
                    return false;
                else
                    return true;

            }, (p) =>
            {
                var category = new Category() { Name = CategoryDisplayName };

                try
                {
                    DB.Categories.Add(category);
                    DB.SaveChanges();
                    MessageBox.Show("Thêm Nhóm xét nghiệm thành công!");
                    CategoryList.Add(category);

                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex}");
                }
            });

            CategoryEditCommand = new RelayCommand<Category>((p) =>
            {
                if (SelectedCategory == null)
                    return false;

                else

                    return true;

            }, (p) =>
            {
                var category = DataProvider.Ins.DB.Categories.Where(x => x.Id == SelectedCategory.Id).SingleOrDefault();
                // Before assigning, check for null
                if (category != null)
                {
                    category.Name = CategoryDisplayName;
                }

                try
                {
                    DB.SaveChanges();
                    MessageBox.Show("Cập nhật Nhóm thành công!");
                    CategoryList = new ObservableCollection<Category>(DB.Categories);
                    CategoryByTestList = new ObservableCollection<Category>(DB.Categories);
                    if (category != null)
                    {
                        SelectedCategoryByTest = category;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex}");
                }
            });

            CategoryDeleteCommand = new RelayCommand<Category>((p) =>
            {
                if (SelectedCategory == null)
                    return false;

                else

                    return true;

            }, (p) =>
            {

                MessageBoxResult result = MessageBox.Show($"Bạn có muốn xóa nhóm: {SelectedCategory.Name}?", "Confirmation", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        List = new ObservableCollection<Test>(DB.Tests);

                        DB.Categories.Remove(SelectedCategory);
                        DB.SaveChanges();
                        CategoryByTestList.Remove(SelectedCategory);
                        CategoryList = new ObservableCollection<Category>(DB.Categories);

                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error: {ex}");
                    }
                }
                else return;
            });
        }
        public void ReLoad(QcManagmentContext DB)
        {
            List = new ObservableCollection<Test>(DB.Tests.OrderBy(s => s.Index));
            UnitList = new ObservableCollection<UnitTable>(DataProvider.Ins.DB.UnitTables);
            DeviceList = new ObservableCollection<Device>(DB.Devices);
            CategoryList = new ObservableCollection<Category>(DB.Categories);
            CategoryByTestList = new ObservableCollection<Category>(DB.Categories);

            // populate test types
            var types = DB.TestTypes.ToList();
            TestTypeList = new ObservableCollection<TestType>(types);

            // Prefer explicit mapping -- if you know Qualitative has Id == 1 use that,
            // otherwise match by name.
            QualitativeTypeId = types.FirstOrDefault(t => t.Id == 1)?.Id
                ?? types.FirstOrDefault(t => (t.Name ?? "").ToLower().Contains("qual"))?.Id;

            QuantitativeTypeId = types.FirstOrDefault(t => t.Id != QualitativeTypeId)?.Id
                ?? types.FirstOrDefault(t => (t.Name ?? "").ToLower().Contains("quant"))?.Id;
        }
    }
}
