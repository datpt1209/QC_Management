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

        private CalType _SelectedCalibType;
        public CalType SelectedCalibType
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

        private ObservableCollection<CalInfor> _CALInfoListDB;
        public ObservableCollection<CalInfor> CALInfoListDB
        {
            get => _CALInfoListDB;
            set => SetProperty(ref _CALInfoListDB, value);
        }


        private CalInfor _CALSelectedItem;
        public CalInfor CALSelectedItem
        {
            get => _CALSelectedItem;
            set
            {
                _CALSelectedItem = value;
                OnPropertyChanged();
                if (CALSelectedItem != null)
                {
                    CALExpirationDate = CALSelectedItem.ExpirationDate;
                    CALLOT = CALSelectedItem.CalLot;

                }
            }
        }

        public CAL_InforViewModel()
        {
            LoadedCommand = new RelayCommand<object>((p) =>
            {
                return true;

            }, (p) =>
            {
                LoadNew();
            });


            CALAddCommand = new RelayCommand<CalInfor>((p) =>
            {
                if (CALLOT == null || SelectedCalibType == null)
                    return false;
                else
                {
                    return true;
                }

            }, (p) =>
            {
                var CAL_Infor = new CalInfor()
                {
                    IdCalType = SelectedCalibType.Id,
                    IdCalTypeNavigation = SelectedCalibType,
                    CalLot = CALLOT,
                    ExpirationDate = CALExpirationDate,
                };

                try
                {
                    DataProvider.Ins.DB.CalInfors.Add(CAL_Infor);
                    DataProvider.Ins.DB.SaveChanges();
                    MessageBox.Show("Thêm thông tin QC thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    CALListDB.Add(CAL_Infor);
                    CALList = CALListDB.ToList();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Stop);
                }

            });

            CALEditCommand = new RelayCommand<CalInfor>((p) =>
            {
                if (CALSelectedItem == null)
                    return false;
                else if ( CALSelectedItem.ExpirationDate == CALExpirationDate
                && CALSelectedItem.CalLot == CALLOT)
                    return false;
                else
                    return true;

            }, (p) =>
            {
                CALSelectedItem.CalLot = CALLOT;
                CALSelectedItem.ExpirationDate = CALExpirationDate;
                CALSelectedItem.IdCalType = SelectedCalibType.Id;
                CALSelectedItem.IdCalTypeNavigation = SelectedCalibType;

                var calDetails = CALSelectedItem.CalDetails;
                foreach (var item in calDetails)
                {
                    item.IdCalInforNavigation = CALSelectedItem;
                    item.IdCalInfor = CALSelectedItem.Id;
                }
                try
                {
                    DataProvider.Ins.DB.SaveChanges();
                    MessageBox.Show("Cập nhật thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    CALList = CALListDB.ToList();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Stop);
                }
            });

            CalTypeSelectionChangedCommand = new RelayCommand<CalInfor>((p) =>
            {
                if (CalibTypeList == null)
                    return false;
               
                else
                    return true;

            }, (p) =>
            {
                CALList = CALListDB.Where(s => s.IdCalType == SelectedCalibType.Id).ToList();
            });

            //DeleteCommand = new RelayCommand<ControlInfo>((p) =>
            //{
            //    return SelectedItem != null;

            //}, (p) => {

            //    if (SelectedItem == null)
            //        return;
            //    try
            //    {
            //        DataProvider.Ins.DB.ControlInfos.Remove(SelectedItem);
            //        DataProvider.Ins.DB.SaveChanges();
            //        MessageBox.Show("Xóa thông tin QC thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);

            //        // Update the ListDB and List properties to refresh the ListView
            //        ListDB.Remove(SelectedItem);
            //        List = ListDB.Where(s => s.IdControlType == SelectedType.Id).ToList();
            //    }
            //    catch (Exception ex)
            //    {
            //        MessageBox.Show($"Error: {ex}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Stop);
            //    }

            //});
        } 
        private void LoadNew()
        {
            CalibTypeList = new ObservableCollection<CalType>(DataProvider.Ins.DB.CalTypes);
            CALListDB = new ObservableCollection<CalInfor>(DataProvider.Ins.DB.CalInfors);
            CALList = new List<CalInfor>(CALListDB);
        }

    }
}

