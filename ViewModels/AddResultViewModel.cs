using QC_Management.Models;
using System;
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
        private ObservableCollection<DeviceTest> _testList;
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
                // Gọi hàm lưu dữ liệu
                bool isSaved = await SaveDataAsync(DataProvider.Ins.DB, NewResults);

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

        private void Cancel()
        {
            _window.DialogResult = false;
            _window.Close();
        }

        private void LoadTestList()
        {
            if (SelectedDevice != null)
            {
                var DeviceTestList = DataProvider.Ins.DB.DeviceTests.ToList();
                // Assuming DataProvider.Ins.DB is accessible here
                TestList = new ObservableCollection<Test>(DeviceTestList.Where(s => s.IdDevice == SelectedDevice.Id).Select(s => s.IdTestNavigation).OrderBy(s => s.Index));
               
            }
        }
        private void AddResult()
        {
            if (SelectedTest != null && Result != null)
            {
                var qcInfor = SelectedTest.ControlInfoDetails.Where(s =>
                      s.IdLevel == SelectedLevel.Id
                      && s.Status == true
                      && s.IdDevice == SelectedDevice.Id).FirstOrDefault();
                var newResult = new Result
                {
                    IdControlDetailNavigation = qcInfor,
                    IdDeviceNavigation = SelectedDevice,
                    IdLevelNavigation = SelectedLevel,
                    IdControlDetail = qcInfor.Id,
                    IdUser = UserManager.Instance.CurrentUser.Id,
                    IdTestNavigation = SelectedTest,
                    IdDevice = SelectedDevice.Id,
                    IdLevel = SelectedLevel.Id,
                    IndexQc = SelectedIndex,
                    IdTest = SelectedTest.Id,
                    ResultType = SelectedTest.TestType,
                    DateRun = SelectedDate,
                    Time = DateTime.Now.TimeOfDay,
                    Comment = Comment,
                    TempResult = ResultString,
                };
                NewResults.Add(newResult);
                ResultString = null; // Clear the result input
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

    }
}
