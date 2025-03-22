using Microsoft.EntityFrameworkCore;
using QC_Management.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using XAct.Library.Settings;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace QC_Management.ViewModels
{

public class AddResultViewModel : BaseViewModel
    {
        private DateTime _selectedDate;
        private Device _selectedDevice;
        private LevelQc _selectedLevel;
        private Test _selectedTest;
        private int _selectedIndex;
        private System.Windows.Window _window;
        private ObservableCollection<Result> _newResults;
        private string _comment;
        private bool _isOutOfRange;
        private bool _isOut2SD;
        private string _resultString;
        private double _result;

        public string ResultString
        {
            get => _resultString;
            set
            {
                if (_resultString != value)
                {
                    _resultString = value;
                    OnPropertyChanged(nameof(ResultString));
                    // Try to convert the string to double
                    if (double.TryParse(_resultString, out double result))
                    {
                        Result = result;
                    }
                }
            }
        }

        public double Result
        {
            get => _result;
            set
            {
                if (_result != value)
                {
                    _result = value;
                    OnPropertyChanged(nameof(Result));
                }
            }
        }

        private ObservableCollection<Test> _TestList;
        public ObservableCollection<Test> TestList { get => _TestList; set { _TestList = value; OnPropertyChanged(); } }

        public DateTime SelectedDate
        {
            get => _selectedDate;
            set
            {
                _selectedDate = value;
                OnPropertyChanged();
            }
        }

        public bool isOutOfRange
        {
            get => _isOutOfRange;
            set
            {
                _isOutOfRange = value;
                OnPropertyChanged();
            }
        }

        public bool isOut2SD
        {
            get => _isOut2SD;
            set
            {
                _isOut2SD = value;
                OnPropertyChanged();
            }
        }
        public string Comment
        {
            get => _comment;
            set
            {
                _comment = value;
                OnPropertyChanged();
            }
        }
        public Test SelectedTest
        {
            get => _selectedTest;
            set
            {
                _selectedTest = value;
                OnPropertyChanged();
            }
        }

        public Device SelectedDevice
        {
            get => _selectedDevice;
            set
            {
                _selectedDevice = value;
                OnPropertyChanged();
                LoadTestList();
            }
        }

        public LevelQc SelectedLevel
        {
            get => _selectedLevel;
            set
            {
                _selectedLevel = value;
                OnPropertyChanged();
            }
        }

        public int SelectedIndex
        {
            get => _selectedIndex;
            set
            {
                _selectedIndex = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<Result> NewResults
        {
            get => _newResults;
            set
            {
                _newResults = value;
                OnPropertyChanged();
            }
        }

        public ICommand SaveCommand { get; }
        public ICommand AddResultCommand { get; }
        public ICommand CancelCommand { get; }

        public AddResultViewModel(DateTime selectedDate, Device selectedDevice, LevelQc selectedLevel, int? selectedIndex, System.Windows.Window window)
        {
            SelectedDate = selectedDate;
            SelectedDevice = selectedDevice;
            SelectedLevel = selectedLevel;
            SelectedIndex = selectedIndex ?? 0;
            NewResults = new ObservableCollection<Result>();
            SaveCommand = new RelayCommand<Result>((p) => true, (p) => SaveAsync());
            CancelCommand = new RelayCommand<Result>((p) => true, (p) => Cancel());
            AddResultCommand = new RelayCommand<Result>((p) => true, (p) => AddResult());
            _window = window;
            LoadTestList();
        }

        private async Task SaveAsync()
        {
            if (NewResults.Count == 0)
            {
                MessageBox.Show("Chưa nhập kết quả QC", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else
            {
                using (var DB = new QcManagmentContext())
                {
                    // Gọi hàm lưu dữ liệu
                    bool isSaved = await SaveDataAsync(DB, NewResults);

                    // Hiển thị thông báo thành công hoặc thất bại
                    if (isSaved)
                    {
                        MessageBox.Show("Lưu kết quả thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                        _window.DialogResult = true;
                        _window.Close();
                    }
                    else
                    {
                        MessageBox.Show("Lưu dữ liệu thất bại. Vui lòng thử lại.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void Cancel()
        {
            _window.DialogResult = false;
            _window.Close();
        }

        private void LoadTestList()
        {
            if (SelectedDevice != null)
            {
                using (var DB = new QcManagmentContext())
                {
                    TestList = new ObservableCollection<Test>(DB.DeviceTests
                        .Include(s => s.IdTestNavigation)
                        .Where(s => s.IdDevice == SelectedDevice.Id)
                        .Select(s => s.IdTestNavigation)
                        .OrderBy(s => s.Index));
                }
            }
        }

        private void AddResult()
        {
            if (SelectedTest != null && Result != null)
            {
                using (var DB = new QcManagmentContext())
                {
                    var qcInfor = DB.ControlInfoDetails
                        .Where(s =>
                             s.IdLevel == SelectedLevel.Id
                             && s.IdTest == SelectedTest.Id
                            && s.Status == true
                            && s.IdDevice == SelectedDevice.Id).FirstOrDefault();

                    if (qcInfor != null)
                    {
                        var newResult = new Result
                        {
                            IdTest = SelectedTest.Id,
                            ResultType = SelectedTest.TestType,
                            IdTestNavigation = SelectedTest,
                            IdDevice = SelectedDevice.Id,
                            IdLevel = SelectedLevel.Id,
                            DateRun = SelectedDate,
                            Time = DateTime.Now.TimeOfDay,
                            IdUser = UserManager.Instance.CurrentUser.Id,
                            IndexQc = SelectedIndex,
                            IdControlDetail = qcInfor.Id,
                            IdControlDetailNavigation = qcInfor,
                            Comment = Comment,
                            TempResult = ResultString,
                        };
                        NewResults.Add(newResult);
                        ResultString = null; // Clear the result input
                    }
                    else
                    {
                        MessageBox.Show("Không tìm thấy thông tin kiểm soát.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        public async Task<bool> SaveDataAsync(QcManagmentContext DB, ObservableCollection<Result> results)
        {
            try
            {
                foreach (var result in results)
                {
                    // Kiểm tra và gắn thực thể ControlInfoDetail nếu chưa được theo dõi
                    if (DB.Entry(result.IdControlDetailNavigation).State == EntityState.Detached)
                    {
                        var existingControlDetail = await DB.ControlInfoDetails.FindAsync(result.IdControlDetail);
                        if (existingControlDetail != null)
                        {
                            DB.Entry(existingControlDetail).State = EntityState.Unchanged;
                            result.IdControlDetailNavigation = existingControlDetail;
                        }
                    }

                    // Kiểm tra và gắn thực thể Test nếu chưa được theo dõi
                    if (DB.Entry(result.IdTestNavigation).State == EntityState.Detached)
                    {
                        var existingTest = await DB.Tests.FindAsync(result.IdTest);
                        if (existingTest != null)
                        {
                            DB.Entry(existingTest).State = EntityState.Unchanged;
                            result.IdTestNavigation = existingTest;
                        }
                    }

                    //// Kiểm tra và gắn thực thể Device nếu chưa được theo dõi
                    //if (DB.Entry(result.IdDeviceNavigation).State == EntityState.Detached)
                    //{
                    //    var existingDevice = await DB.Devices.FindAsync(result.IdDevice);
                    //    if (existingDevice != null)
                    //    {
                    //        DB.Entry(existingDevice).State = EntityState.Unchanged;
                    //        result.IdDeviceNavigation = existingDevice;
                    //    }
                    //}

                    //// Kiểm tra và gắn thực thể LevelQc nếu chưa được theo dõi
                    //if (DB.Entry(result.IdLevelNavigation).State == EntityState.Detached)
                    //{
                    //    var existingLevel = await DB.LevelQcs.FindAsync(result.IdLevel);
                    //    if (existingLevel != null)
                    //    {
                    //        DB.Entry(existingLevel).State = EntityState.Unchanged;
                    //        result.IdLevelNavigation = existingLevel;
                    //    }
                    //}

                    // Thêm hoặc cập nhật thực thể Result
                    var existingResult = await DB.Results.FindAsync(result.Id);
                    if (existingResult == null)
                    {
                        DB.Results.Add(result);
                    }
                    else
                    {
                        DB.Entry(existingResult).CurrentValues.SetValues(result);
                    }
                }

                await DB.SaveChangesAsync();
                return true; // Trả về true nếu lưu thành công
            }
            catch (Exception ex)
            {
                // Xử lý lỗi nếu có
                MessageBox.Show($"Có lỗi: {ex}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return false; // Trả về false nếu lưu thất bại
            }
        }
    }
}
