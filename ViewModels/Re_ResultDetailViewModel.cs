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
    public class Re_ResultDetailViewModel : BaseViewModel
    {
        private ObservableCollection<ReResult> _Results;
        public ObservableCollection<ReResult> Results
        {
            get => _Results;
            set { _Results = value; OnPropertyChanged(); }
        }

        private ObservableCollection<ResultReView> _ResutlViewList;
        public ObservableCollection<ResultReView> ResutlViewList { get => _ResutlViewList; set { _ResutlViewList = value; OnPropertyChanged(); } }

        private ResultReView _SelectedItem;
        public ResultReView SelectedItem
        {
            get => _SelectedItem;
            set
            {
                _SelectedItem = value;
                OnPropertyChanged();
            }
        }

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

        private int _idLevel;
        public int IdLevel
        {
            get => _idLevel;
            set
            {
                _idLevel = value;
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
        private String _Time;
        public String Time
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

        public Re_ResultDetailViewModel(ReResultGroup reResultGroup, System.Windows.Window window)
        {
            Results = reResultGroup.Results;
            _window = window;
            Index = 0;

            LoadCommand = new RelayCommand<ControlInfoDetail>((p) =>
            {
                return true;
            }, (p) =>
            {
                DeviceName = reResultGroup.DeviceName;
                LevelName = reResultGroup.LevelName;
                IdLevel = reResultGroup.IdLevel;
                Date = reResultGroup.DateTime.Date;
                Time = string.Format("{0:D2}:{1:D2}:{2:D2}", reResultGroup.Time.Hours, reResultGroup.Time.Minutes, reResultGroup.Time.Seconds);

                ResutlViewList = new ObservableCollection<ResultReView>();

                foreach (var item in Results)
                {
                    var qcInfor = item.IdTestNavigation.ControlInfoDetails.Where(s =>
                       s.IdLevel == item.IdLevel
                       && s.Status == true
                       && s.IdDevice == item.IdDevice).FirstOrDefault();

                    if (qcInfor == null)
                    {
                        MessageBox.Show($"Không tìm thấy thông tin QC {item.IdTestNavigation.Name}", "Thông báo", MessageBoxButton.OK);
                    }
                    else
                    {
                        ResutlViewList.Add(new ResultReView()
                        {
                            id = item.Id,
                            TestName = item.IdTestNavigation.Name,
                            idTest = item.IdTest,
                            QCName = qcInfor.IdControlInfoNavigation.Name,
                            LOT = qcInfor.Lot,
                            MeanApp = qcInfor.CurMean,
                            SdApp = qcInfor.CurSd,
                            MeanNSX = qcInfor.MeanNsx,
                            SdNSX = qcInfor.SdNsx,
                            Max = qcInfor.MeanApp + 3 * qcInfor.CurMean,
                            Min = qcInfor.MeanApp - 3 * qcInfor.CurSd,
                            IdControlDetailNavigation = qcInfor,
                            Result = item.Result,
                        });
                    }
                }
            });

            CancelCommand = new RelayCommand<Result>((p) => true, (p) => Cancel());
            DeleteCommand = new RelayCommand<Result>((p) => true, (p) => Delete());

            SaveCommand = new RelayCommand<ControlInfoDetail>((p) =>
            {
                if (ResutlViewList == null) return false;
                else return true;

            }, (p) =>
            {
                var indexList = DataProvider.Ins.DB.Results
                .Where(s => s.IdDevice == reResultGroup.IdDevice && s.DateRun.Date == Date && s.IdLevelNavigation.Id == reResultGroup.IdLevel)
                .GroupBy(s => s.IndexQc)
                .Select(s => s.Key).ToList();

                if (indexList == null || indexList.Count() == 0)
                {
                    Index = 1;
                }
                else
                {
                    Index = (int)(indexList.Max() + 1);
                }

                var results = new ObservableCollection<Result>();
                foreach (var item in ResutlViewList)
                {
                    if (item.Result != null)
                    {
                        Result result = new Result()
                        {
                            IdTest = item.idTest,
                            IdDevice = reResultGroup.IdDevice,
                            IdLevel = reResultGroup.IdLevel,
                            DateRun = Date.Date,
                            Time = DateTime.Now.TimeOfDay,
                            IdUser = UserManager.Instance.CurrentUser.Id,
                            IndexQc = Index,
                            IdControlDetail = item.IdControlDetailNavigation.Id,
                            IdControlDetailNavigation = item.IdControlDetailNavigation,
                            Comment = item.Comment,
                            IsOutRange = item.isOutOfRange,
                            Result1 = (double)item.Result,
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

        private async Task SaveAsync(ObservableCollection<Result> results)
        {
            // Gọi hàm lưu dữ liệu
            bool isSaved = await SaveDataAsync(DataProvider.Ins.DB, results);

            // Hiển thị thông báo thành công hoặc thất bại
            if (isSaved)
            {
                DataProvider.Ins.DB.ReResults.RemoveRange(Results);
                await DataProvider.Ins.DB.SaveChangesAsync();
                Results.Clear();

                MessageBox.Show("Lưu kết quả thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                _window.DialogResult = true;
                _window.Close();
            }
            else
            {
                MessageBox.Show("Lưu dữ liệu thất bại. Vui lòng thử lại.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public async Task<bool> SaveDataAsync(QcManagmentContext DB, ObservableCollection<Result> results)
        {
            try
            {
                DB.AddRange(results);
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
            var result = MessageBox.Show("Bạn có chắc chắn muốn xóa tất cả dữ liệu ReResult không?", "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    DataProvider.Ins.DB.ReResults.RemoveRange(Results);
                    await DataProvider.Ins.DB.SaveChangesAsync();
                    Results.Clear();

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