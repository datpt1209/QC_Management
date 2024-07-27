using Microsoft.EntityFrameworkCore.Metadata.Internal;
using QC_Management.Models;
using QC_Management.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using XAct.Library.Settings;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;

namespace QC_Management.ViewModels
{
    public class Re_ResultDetailViewModel:BaseViewModel
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

        public ICommand LoadCommand { get; set; }

        public Re_ResultDetailViewModel(ReResultGroup reResultGroup)
        {
            Results = reResultGroup.Results;
            int index = 0;

            LoadCommand = new RelayCommand<ControlInfoDetail>((p) =>
            {
                    return true;
            }, (p) =>
            {
                DeviceName = reResultGroup.DeviceName;
                LevelName = reResultGroup.LevelName;
                Date = reResultGroup.DateTime.Date;
                Time = DateTime.Now.ToString("HH:mm:ss");
                var indexList = DataProvider.Ins.DB.Results.Where(s => s.IdDevice == 20 && s.DateRun.Date == reResultGroup.DateTime.Date && s.IdLevelNavigation.Id == reResultGroup.IdLevel).GroupBy(s => s.IndexQc).Select(s => s.Key).ToList();

                if (indexList == null || indexList.Count() == 0)
                {
                    index = 1;
                }
                else
                {
                   index = (int)(indexList.Max() + 1);
                }

                ResutlViewList = new ObservableCollection<ResultReView>();
                
                foreach (var item in Results)
                {
                    var qcInfor = item.IdTestNavigation.ControlInfoDetails.Where(s =>
                       s.IdLevel == item.IdLevel
                       && s.Status == true
                       && s.IdDevice == 20).FirstOrDefault();

                    if (qcInfor == null)
                    {
                        MessageBox.Show($"Không tìm thấy thông tin QC {item.IdTestNavigation.Name}", "Thông báo", MessageBoxButton.OK);
                    }
                    else
                    {
                        ResutlViewList.Add(new ResultReView()
                        {                            
                            TestName = item.IdTestNavigation.Name,
                            idTest = item.IdTest,
                            QCName = qcInfor.IdControlInfoNavigation.Name,
                            LOT = qcInfor.Lot,
                            MeanApp = qcInfor.MeanApp,
                            SdApp = qcInfor.SdApp,
                            MeanNSX = qcInfor.MeanNsx,
                            SdNSX = qcInfor.SdNsx,
                            Max = qcInfor.MeanApp + 3 * qcInfor.SdApp,
                            Min = qcInfor.MeanApp - 3 * qcInfor.SdApp,
                            IdControlDetailNavigation = qcInfor,
                            Result = item.Result,
                        });
                    }
                }
            });

            SaveCommand = new RelayCommand<ControlInfoDetail>((p) =>
            {
                if (ResutlViewList == null) return false;
                else return true;

            }, (p) =>
            {
                var results = new ObservableCollection<Result>();
                foreach (var item in ResutlViewList)
                {
                    if (item.Result != null)
                    {
                        Result result = new Result()
                        {
                            IdTest = item.idTest,
                            IdDevice = 20,
                            IdLevel = reResultGroup.IdLevel,
                            DateRun = reResultGroup.DateTime.Date,
                            Time = DateTime.Now.TimeOfDay,
                            IdUser = UserManager.Instance.CurrentUser.Id,
                            IndexQc = index,
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
                   SaveResults(results);

                }
            });
        }

        private void SaveResults(ObservableCollection<Result> reResults)
        {
            try
            {
                QcManagmentContext DB = DataProvider.Ins.DB; 

                DB.Results.AddRange(reResults);

                // Save changes to the database
                int rowsAffected = DB.SaveChanges();

                if (rowsAffected > 0)
                {
                    // Delete the saved ReResult entries
                    DB.ReResults.RemoveRange(Results);

                    // Save changes to the database
                    DB.SaveChanges();

                    // Clear the Results collection
                    Results.Clear();

                    // Hiển thị thông báo thành công
                    MessageBox.Show("Lưu dữ liệu thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Đóng cửa sổ Re_Resultdetail
                    Application.Current.Windows.OfType<Window>().SingleOrDefault(w => w.DataContext == this)?.Close();
                }
                else
                {
                    // Hiển thị thông báo lỗi
                    MessageBox.Show("Lưu dữ liệu thất bại!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                // Hiển thị thông báo lỗi
                MessageBox.Show($"Lỗi khi lưu dữ liệu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }
    }
}