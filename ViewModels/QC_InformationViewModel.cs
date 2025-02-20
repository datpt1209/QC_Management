using QC_Management.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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

        private bool _isChecked;
        public bool isChecked { get => _isChecked; set { _isChecked = value; OnPropertyChanged(); } }

        private DateTime _ProductionDate = DateTime.Now;
        public DateTime ProductionDate { get => _ProductionDate; set { _ProductionDate = value; OnPropertyChanged(); } }


        private DateTime _ExpirationDate = DateTime.Now;
        public DateTime ExpirationDate { get => _ExpirationDate; set { _ExpirationDate = value; OnPropertyChanged(); } }

        private string _LOT;
        public string LOT { get => _LOT; set { _LOT = value; OnPropertyChanged(); } }

        private ControlType? _SelectedType;
        public ControlType? SelectedType { get => _SelectedType; set { _SelectedType = value; OnPropertyChanged(); } }

       

        private ObservableCollection<Test> _testList;
        public ObservableCollection<Test> TestList
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
                    isChecked = SelectedItem.Status;
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
        public QC_InformationViewModel()
        {
            LoadedCommand = new RelayCommand<object>((p) =>
            {
                return true;

            }, (p) =>
            {
                LoadNew();
            });

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
                    Status = isChecked,
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
                && SelectedItem.Status == isChecked
                && SelectedItem.IdControlTypeNavigation == SelectedType
                )
                    return false;
                else
                    return true;

            }, (p) =>
            {
                SelectedItem.IdControlTypeNavigation = SelectedType;
                SelectedItem.IdControlType  = SelectedType.Id;
                SelectedItem.Status = isChecked;
                SelectedItem.Lot = LOT;
                SelectedItem.ProductionDate = ProductionDate;
                SelectedItem.ExpirationDate = ExpirationDate;

                var controlDetails = SelectedItem.ControlInfoDetails;
                foreach (var item in controlDetails)
                {
                    item.IdControlInfoNavigation = SelectedItem;
                    item.IdControlInfo = SelectedItem.Id;
                    item.Status = SelectedItem.Status;
                }
                try
                {
                    DataProvider.Ins.DB.SaveChanges();
                    MessageBox.Show("Cập nhật thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    List = ListDB.Where(s => s.IdControlType == SelectedType.Id).ToList();
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
        }
        public void ReLoad()
        {
            LOT = string.Empty;
            ListDB = new ObservableCollection<ControlInfo>(DataProvider.Ins.DB.ControlInfos);
            List = ListDB.ToList();
        }

    }
}
