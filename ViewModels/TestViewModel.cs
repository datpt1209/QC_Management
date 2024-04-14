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
                    if (SelectedItem.Index != null)
                    {
                        Index = (int)SelectedItem.Index;
                    }
                    DisplayName = SelectedItem.Name;
                    SelectedUnitTable = SelectedItem.IdUnitTableNavigation;
                    SelectedCategory = SelectedItem.IdCategoryNavigation;
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

        public TestViewModel()
        {
            var DB = DataProvider.Ins.DB;

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
                if (SelectedUnitTable == null || DisplayName == null || SelectedCategoryByTest == null)
                    return false;
                else
                {
                    return true;
                }

            }, (p) =>
            {
                var test = new Test()
                {
                    Index = Index,
                    Name = DisplayName,
                    IdCategoryNavigation = SelectedCategoryByTest,
                    IdCategory = SelectedCategoryByTest.Id,
                    IdUnitTable = SelectedUnitTable.Id,
                    IdUnitTableNavigation = SelectedUnitTable,
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

            EditCommand = new RelayCommand<Test>((p) =>
            {
                if (SelectedItem == null)
                    return false;

                else if (SelectedItem.Index == Index
                && SelectedItem.Name == DisplayName
                && SelectedItem.IdUnitTable == SelectedUnitTable.Id
                && SelectedItem.IdCategory == SelectedCategoryByTest.Id)
                    return false;

                return true;

            }, (p) =>
            {
                var testEditor = DataProvider.Ins.DB.Tests.Where(x => x.Id == SelectedItem.Id).SingleOrDefault();
                testEditor.Index = Index;
                testEditor.Name = DisplayName;
                testEditor.IdUnitTableNavigation = SelectedUnitTable;
                testEditor.IdCategoryNavigation = SelectedCategoryByTest;
                testEditor.IdCategory = SelectedCategoryByTest.Id;
                testEditor.IdUnitTable = SelectedUnitTable.Id;

                try
                {
                    DataProvider.Ins.DB.SaveChanges();
                    MessageBox.Show("Cập nhật Test thành công!");
                    List = new ObservableCollection<Test>(DB.Tests);

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
                category.Name = CategoryDisplayName;

                try
                {
                    DB.SaveChanges();
                    MessageBox.Show("Cập nhật Nhóm thành công!");
                    CategoryList = new ObservableCollection<Category>(DB.Categories);
                    CategoryByTestList = new ObservableCollection<Category>(DB.Categories);
                    SelectedCategoryByTest = category;
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
                        SelectedCategoryByTest = null;
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
            DisplayName = null;
            SelectedUnitTable = null;
            SelectedItem = null;
            List = new ObservableCollection<Test>(DB.Tests.OrderBy(s => s.Index));
            UnitList = new ObservableCollection<UnitTable>(DataProvider.Ins.DB.UnitTables);
            DeviceList = new ObservableCollection<Device>(DB.Devices);
            CategoryList = new ObservableCollection<Category>(DB.Categories);
            CategoryByTestList = new ObservableCollection<Category>(DB.Categories);

        }

    }
}
