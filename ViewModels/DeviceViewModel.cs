using QC_Management.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

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
                if (SelectedItem != null)
                {
                    DisplayName = SelectedItem.Name;
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

        private DeviceTest _SelectedDeviceTest;
        public DeviceTest SelectedDeviceTest
        {
            get => _SelectedDeviceTest;
            set
            {
                _SelectedDeviceTest = value;
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

        private string _DisplayName;
        public string DisplayName { get => _DisplayName; set { _DisplayName = value; OnPropertyChanged(); } }
        public DeviceViewModel()
        {
            LoadedCommand = new RelayCommand<UserRole>((p) =>
            {
                return true;
            }, (p) =>
            {
                LoadNew();
            });

            AddCommand = new RelayCommand<UserRole>((p) =>
            {
                if (string.IsNullOrEmpty(DisplayName))
                    return false;

                return true;

            }, (p) =>
            {
                var device = new Device() { Name = DisplayName, IdCategory = SelectedCategory.Id, IdCategoryNavigation = SelectedCategory };

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
                else if (SelectedItem.Name == DisplayName) return false;
                else return true;

            }, (p) =>
            {
                SelectedItem.Name = DisplayName;
                SelectedItem.IdCategory = SelectedCategory.Id;
                SelectedItem.IdCategoryNavigation = SelectedCategory;

                try
                {
                    DataProvider.Ins.DB.SaveChanges();
                    MessageBox.Show("Cập nhật thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Stop);
                }
            });
            AddTestCommand = new RelayCommand<Device>((p) =>
            {
                return true;

            }, (p) =>
            {
                DeviceTest deviceTest = new DeviceTest
                {
                    IdDevice = SelectedItem.Id,
                    IdDeviceNavigation = SelectedItem,
                    IdTest = SelectedTest.Id,
                    IdTestNavigation = SelectedTest
                };
                DeviceTestList.Add(deviceTest);
                DataProvider.Ins.DB.DeviceTests.Add(deviceTest);
                DataProvider.Ins.DB.SaveChanges();
            });

            DeviceSelectionChangedCommand = new RelayCommand<Device>((p) =>
            {
                if (SelectedItem == null) return false;
                else return true;

            }, (p) =>
            {
                DeviceTestList = new ObservableCollection<DeviceTest>(DeviceTestListDB.Where(s => s.IdDevice == SelectedItem.Id));
                SelectedCategory = SelectedItem.IdCategoryNavigation;
            });

            CategorySelectionChangedCommand = new RelayCommand<Device>((p) =>
            {
                return true;

            }, (p) =>
            {
                List = new ObservableCollection<Device>(ListDB.Where(s => s.IdCategory == SelectedCategory.Id));
            });

            RomoveCommand = new RelayCommand<DeviceTest>((p) =>
            {
                if (SelectedDeviceTest == null) return false;
                else return true;

            }, (p) =>
            {
                MessageBoxResult result = MessageBox.Show($"Do you want remove Test: {SelectedDeviceTest.IdTestNavigation.Name}?", "Confirmation", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        DataProvider.Ins.DB.DeviceTests.Remove(SelectedDeviceTest);
                        DataProvider.Ins.DB.SaveChanges();
                        DeviceTestList.Remove(SelectedDeviceTest);
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
            CategorytList = new ObservableCollection<Category>(DataProvider.Ins.DB.Categories);
            ListDB = new ObservableCollection<Device>(DataProvider.Ins.DB.Devices);

            TestList = new ObservableCollection<Test>(DataProvider.Ins.DB.Tests);
            List = new ObservableCollection<Device>(DataProvider.Ins.DB.Devices);
            DeviceTestList = new ObservableCollection<DeviceTest>();
            DeviceTestListDB = new ObservableCollection<DeviceTest>(DataProvider.Ins.DB.DeviceTests);

        }

        public void Reload()
        {
            SelectedItem = null;
        }
    }
}
