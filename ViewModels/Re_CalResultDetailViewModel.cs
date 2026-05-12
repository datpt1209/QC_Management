using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using QC_Management.Models;
using QC_Management.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using XAct.Library.Settings;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;

namespace QC_Management.ViewModels
{
    public class Re_CalResultDetailViewModel : BaseViewModel
    {
        private ObservableCollection<ReCalResult> _ReCalResults;
        public ObservableCollection<ReCalResult> ReCalResults
        {
            get => _ReCalResults;
            set { _ReCalResults = value; OnPropertyChanged(); }
        }

        private ObservableCollection<ReCalResultReView> _CalResutlViewList;
        public ObservableCollection<ReCalResultReView> CalResutlViewList { get => _CalResutlViewList; set { _CalResutlViewList = value; OnPropertyChanged(); } }

        private ReCalResultReView _SelectedItem;
        public ReCalResultReView SelectedItem
        {
            get => _SelectedItem;
            set
            {
                _SelectedItem = value;
                OnPropertyChanged();
            }
        }

        // --- User selection fields ---
        private ObservableCollection<User> _UserList;
        public ObservableCollection<User> UserList { get => _UserList; set { _UserList = value; OnPropertyChanged(); } }

        private User _SelectedUser;
        public User SelectedUser { get => _SelectedUser; set { _SelectedUser = value; OnPropertyChanged(); } }

        private bool _isUserSelectionEnabled;
        public bool IsUserSelectionEnabled { get => _isUserSelectionEnabled; set { _isUserSelectionEnabled = value; OnPropertyChanged(); } }

        private System.Windows.Window _window;

        private string _Comment;
        public string Comment
        {
            get => _Comment;
            set
            {
                _Comment = value;
                OnPropertyChanged();
            }
        }
        private string _DeviceName;
        public string DeviceName
        {
            get => _DeviceName;
            set
            {
                _DeviceName = value;
                OnPropertyChanged();
            }
        }
        private string _LevelName;
        public string LevelName
        {
            get => _LevelName;
            set
            {
                _LevelName = value;
                OnPropertyChanged();
            }
        }

        private int _Level;
        public int Level
        {
            get => _Level;
            set
            {
                _Level = value;
                OnPropertyChanged();
            }
        }

        private int _index;
        public int Index
        {
            get => _index;
            set
            {
                _index = value;
                OnPropertyChanged();
            }
        }

        private DateTime _Date;
        public DateTime Date
        {
            get => _Date;
            set
            {
                _Date = value;
                OnPropertyChanged();
            }
        }
        private String? _Time;
        public String? Time
        {
            get => _Time;
            set
            {
                _Time = value;
                OnPropertyChanged();
            }
        }

        public ICommand SaveCommand { get; set; }

        public ICommand CancelCommand { get; set; }

        public ICommand DeleteCommand { get; set; }

        public ICommand LoadCommand { get; set; }

        public Re_CalResultDetailViewModel(CalGroup reCalResultGroup, System.Windows.Window window)
        {
            ReCalResults = reCalResultGroup.ReCalResults;
            _window = window;
            Index = 0;

            LoadCommand = new RelayCommand<ControlInfoDetail>((p) =>
            {
                return true;
            }, (p) =>
            {
                // Load users for the combo box
                LoadUsers(DataProvider.Ins.DB);

                DeviceName = reCalResultGroup.DeviceName;
                Date = reCalResultGroup.DateRun.Date;
                Time = string.Format("{0:D2}:{1:D2}:{2:D2}", reCalResultGroup.Time.Hours, reCalResultGroup.Time.Minutes, reCalResultGroup.Time.Seconds);
                CalResutlViewList = new ObservableCollection<ReCalResultReView>();

                foreach (var reCalResult in ReCalResults)
                {
                    var calInforDetail = DataProvider.Ins.DB.CalDetails.Include(cd => cd.IdCalInforNavigation).Where(s => s.Status == true
                      && s.IdDevice == reCalResult.IdDevice
                      && s.IdTest == reCalResult.IdTest).FirstOrDefault();

                    if (calInforDetail == null)
                    {
                        MessageBox.Show($"Không tìm thấy thông tin CAL {reCalResult.IdTestNavigation.Name}", "Thông báo", MessageBoxButton.OK);
                    }
                    else
                    {
                        CalResutlViewList.Add(new ReCalResultReView()
                        {
                            Id = reCalResult.Id,
                            IdTestNavigation = reCalResult.IdTestNavigation,
                            IdTest = reCalResult.IdTest,
                            Level = reCalResult.Level,
                            LOT = calInforDetail.IdCalInforNavigation.CalLot,
                            Max = calInforDetail.MaxValue,
                            Min = calInforDetail.MinValue,
                            Result = reCalResult.Result,
                            DateRun = reCalResult.DateRun,
                            Time = reCalResult.Time,
                            IndexCal = reCalResult.IndexCal,
                            IdDevice = reCalResult.IdDevice,
                            IdDeviceNavigation = reCalResult.IdDeviceNavigation,
                            IdCalDetailNavigation = calInforDetail,
                            isOutOfRange = reCalResult.Result < calInforDetail.MinValue || reCalResult.Result > calInforDetail.MaxValue,
                        });
                    }
                }

            });

            CancelCommand = new RelayCommand<CalResult>((p) => true, (p) => Cancel());
            DeleteCommand = new RelayCommand<Result>((p) => true, (p) => Delete());
            SaveCommand = new RelayCommand<ControlInfoDetail>((p) =>
            {
                if (ReCalResults == null) return false;
                else return true;

            }, (p) =>
            {

                var results = new ObservableCollection<CalResult>();
                foreach (var item in CalResutlViewList)
                {
                    if (item.Result != null)
                    {
                        CalResult result = new CalResult()
                        {
                            IdTest = (int)item.IdTest,
                            IdTestNavigation = item.IdTestNavigation,
                            IdDevice = (int)item.IdDevice,
                            Level = (int)item.Level,
                            DateRun = Date.Date,
                            Time = DateTime.Now.TimeOfDay,
                            // use SelectedUser if provided, otherwise fallback
                            IdUser = SelectedUser?.Id ?? UserManager.Instance.CurrentUser.Id,
                            IndexCal = Index,
                            IdCalDetail = item.IdCalDetailNavigation.Id,
                            IdCalDetailNavigation = item.IdCalDetailNavigation,
                            Comment = item.Comment,
                            Result = (double)item.Result,
                            isOutOfRange = item.isOutOfRange,

                        };
                        results.Add(result);
                    }
                }
                if (results.Count == 0)
                {
                    MessageBox.Show("Chưa nhập kết quả QC", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else
                {
                    SaveAsync(results);

                }
            });
        }

        private void LoadUsers(QcManagmentContext DB)
        {
            try
            {
                var users = DB.Users.Include(u => u.RoleNavigation).ToList();
                UserList = new ObservableCollection<User>(users);

                var current = UserManager.Instance?.CurrentUser;
                if (current != null)
                {
                    SelectedUser = UserList.FirstOrDefault(u => u.Id == current.Id) ?? current;
                    IsUserSelectionEnabled = current.IsAdmin == true;
                }
                else
                {
                    SelectedUser = UserList.FirstOrDefault();
                    IsUserSelectionEnabled = false;
                }
            }
            catch
            {
                UserList = new ObservableCollection<User>();
                SelectedUser = UserManager.Instance?.CurrentUser;
                IsUserSelectionEnabled = UserManager.Instance?.CurrentUser?.IsAdmin == true;
            }
        }

        private async Task SaveAsync(ObservableCollection<CalResult> results)
        {
            // Gọi hàm lưu dữ liệu
            bool isSaved = await SaveDataAsync(DataProvider.Ins.DB, results);

            // Hiển thị thông báo thành công hoặc thất bại
            if (isSaved)
            {
                DataProvider.Ins.DB.ReCalResults.RemoveRange(ReCalResults);
                await DataProvider.Ins.DB.SaveChangesAsync();
                ReCalResults.Clear();

                MessageBox.Show("Lưu kết quả thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                _window.DialogResult = true;
                _window.Close();
            }
            else
            {
                MessageBox.Show("Lưu dữ liệu thất bại. Vui lòng thử lại.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public async Task<bool> SaveDataAsync(QcManagmentContext DB, ObservableCollection<CalResult> results)
        {
            try
            {
                DB.CalResults.AddRange(results);
                await DB.SaveChangesAsync();

                return true; // Trả về true nếu lưu thành công
            }
            catch (Exception ex)
            {
                // Xử lý lỗi nếu có
                MessageBox.Show($"Có lỗi:{ex}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return false; // Trả về false nếu lưu thất bại
            }
        }

        private void Cancel()
        {
            _window.DialogResult = false;
            _window.Close();
        }

        private async void Delete()
        {
            var result = MessageBox.Show("Bạn có chắc chắn muốn xóa tất cả dữ liệu Calib này không?", "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    DataProvider.Ins.DB.ReCalResults.RemoveRange(ReCalResults);
                    await DataProvider.Ins.DB.SaveChangesAsync();
                    ReCalResults.Clear();

                    MessageBox.Show("Xóa tất cả dữ liệu ReResult thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Close the window
                    _window.DialogResult = true;
                    _window.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Có lỗi khi xóa dữ liệu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}