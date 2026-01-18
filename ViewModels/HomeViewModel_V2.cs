using LiveCharts;
using LiveCharts.Configurations;
using LiveCharts.Wpf;
using Microsoft.EntityFrameworkCore;
using QC_Management.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace QC_Management.ViewModels
{
    public class HomeViewModel_V2 : BaseViewModel
    {
        private QcManagmentContext _dbContext;
        private ObservableCollection<Result> _List;
        private Func<double, string> _yAxisLabelFormatter;
        private ObservableCollection<Device> _DeviceList;
        private ObservableCollection<Test> _TestList;
        private ObservableCollection<string> _Dates3;
        private ObservableCollection<string> _Dates1;
        private ObservableCollection<string> _Dates2;
        private ObservableCollection<string> _Dates4;
        private Visibility _Visibility1;
        private Visibility _Visibility2;
        private Visibility _Visibility3;
        private bool _isLoading;
        private ChartValues<Result> _ChartValues1;
        private ChartValues<Result> _ChartValues2;
        private ChartValues<Result> _ChartValues3;
        private ChartValues<Result> _ChartValues4;
        private ChartValues<double> _MeanValues1;
        private ChartValues<double> _MeanValues2;
        private ChartValues<double> _MeanValues3;
        private ChartValues<double> _MeanValues4;
        private ChartValues<double> _PlusOneSDValues1;
        private ChartValues<double> _PlusOneSDValues2;
        private ChartValues<double> _PlusOneSDValues3;
        private ChartValues<double> _PlusOneSDValues4;
        private ChartValues<double> _MinusOneSDValues1;
        private ChartValues<double> _MinusOneSDValues2;
        private ChartValues<double> _MinusOneSDValues3;
        private ChartValues<double> _MinusOneSDValues4;
        private ChartValues<double> _PlusTwoSDValues2;
        private ChartValues<double> _PlusTwoSDValues1;
        private ChartValues<double> _PlusTwoSDValues3;
        private ChartValues<double> _PlusTwoSDValues4;
        private ChartValues<double> _MinusTwoSDValues1;
        private ChartValues<double> _MinusTwoSDValues2;
        private ChartValues<double> _MinusTwoSDValues3;
        private ChartValues<double> _MinusTwoSDValues4;
        private ChartValues<double> _PlusThreeSDValues1;
        private ChartValues<double> _PlusThreeSDValues2;
        private ChartValues<double> _PlusThreeSDValues3;
        private ChartValues<double> _PlusThreeSDValues4;
        private ChartValues<double> _MinusThreeSDValues3;
        private ChartValues<double> _MinusThreeSDValues2;
        private ChartValues<double> _MinusThreeSDValues1;
        private ChartValues<double> _MinusThreeSDValues4;
        private float _totalWidth1;
        private float _totalWidth2;
        private float _totalWidth3;
        private float _totalWidth4;
        private DateTime _StartDate = DateTime.Now.AddDays(-14);
        private DateTime _EndDate = DateTime.Now;
        private bool _isCheck;
        private Test _SelectedTest;
        private Device _SelectedDevice;
        public ObservableCollection<Result> List { get => _List; set { _List = value; OnPropertyChanged(); } }
        public Func<double, string> YAxisLabelFormatter
        {
            get { return _yAxisLabelFormatter; }
            set
            {
                _yAxisLabelFormatter = value;
                OnPropertyChanged(nameof(YAxisLabelFormatter));
            }
        }
        public ObservableCollection<Device> DeviceList { get => _DeviceList; set { _DeviceList = value; OnPropertyChanged(); } }
        public ObservableCollection<Test> TestList { get => _TestList; set { _TestList = value; OnPropertyChanged(); } }
        public ObservableCollection<string> Dates3 { get => _Dates3; set { _Dates3 = value; OnPropertyChanged(); } }
        public ObservableCollection<string> Dates1 { get => _Dates1; set { _Dates1 = value; OnPropertyChanged(); } }
        public ObservableCollection<string> Dates2 { get => _Dates2; set { _Dates2 = value; OnPropertyChanged(); } }
        public ObservableCollection<string> Dates4 { get => _Dates4; set { _Dates4 = value; OnPropertyChanged(); } }
        public Visibility Visibility1 { get => _Visibility1; set { _Visibility1 = value; OnPropertyChanged(); } }
        public Visibility Visibility2 { get => _Visibility2; set { _Visibility2 = value; OnPropertyChanged(); } }
        public Visibility Visibility3 { get => _Visibility3; set { _Visibility3 = value; OnPropertyChanged(); } }
        public Visibility Visibility4 { get => _Visibility3; set { _Visibility3 = value; OnPropertyChanged(); } }


        // For Chart 1
        private string _levelName1;
        public string LevelName1 { get => _levelName1; set { _levelName1 = value; OnPropertyChanged(); } }

        private double? _mean1;
        public double? Mean1 { get => _mean1; set { _mean1 = value; OnPropertyChanged(); } }

        private double? _sd1;
        public double? SD1 { get => _sd1; set { _sd1 = value; OnPropertyChanged(); } }

        private string _range1;
        public string Range1 { get => _range1; set { _range1 = value; OnPropertyChanged(); } }
        private string _totalPoints1;
        public string TotalPoints1
        {
            get => _totalPoints1;
            set
            {
                _totalPoints1 = value;
                OnPropertyChanged();
            }
        }

        // Repeat for Chart 2, 3, 4
        private string _levelName2;
        public string LevelName2 { get => _levelName2; set { _levelName2 = value; OnPropertyChanged(); } }
        private double? _mean2;
        public double? Mean2 { get => _mean2; set { _mean2 = value; OnPropertyChanged(); } }
        private double? _sd2;
        public double? SD2 { get => _sd2; set { _sd2 = value; OnPropertyChanged(); } }
        private string _range2;
        public string Range2 { get => _range2; set { _range2 = value; OnPropertyChanged(); } }
        private string _totalPoints2;

        public string TotalPoints2
        {
            get => _totalPoints2;
            set
            {
                _totalPoints2 = value;
                OnPropertyChanged();
            }
        }


        private string _levelName3;
        public string LevelName3 { get => _levelName3; set { _levelName3 = value; OnPropertyChanged(); } }
        private double? _mean3;
        public double? Mean3 { get => _mean3; set { _mean3 = value; OnPropertyChanged(); } }
        private double? _sd3;
        public double? SD3 { get => _sd3; set { _sd3 = value; OnPropertyChanged(); } }
        private string _range3;
        public string Range3 { get => _range3; set { _range3 = value; OnPropertyChanged(); } }

        private string _totalPoints3;
        public string TotalPoints3
        {
            get => _totalPoints3;
            set
            {
                _totalPoints3 = value;
                OnPropertyChanged();
            }
        }

        private string _levelName4;
        public string LevelName4 { get => _levelName4; set { _levelName4 = value; OnPropertyChanged(); } }
        private double? _mean4;
        public double? Mean4 { get => _mean4; set { _mean4 = value; OnPropertyChanged(); } }
        private double? _sd4;
        public double? SD4 { get => _sd4; set { _sd4 = value; OnPropertyChanged(); } }
        private string _range4;
        public string Range4 { get => _range4; set { _range4 = value; OnPropertyChanged(); } }
        private string _totalPoints4;
        public string TotalPoints4
        {
            get => _totalPoints4;
            set
            {
                _totalPoints4 = value;
                OnPropertyChanged();
            }
        }


        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }
        public ChartValues<Result> ChartValues1 { get => _ChartValues1; set { _ChartValues1 = value; OnPropertyChanged(); } }
        public ChartValues<Result> ChartValues2 { get => _ChartValues2; set { _ChartValues2 = value; OnPropertyChanged(); } }
        public ChartValues<Result> ChartValues3 { get => _ChartValues3; set { _ChartValues3 = value; OnPropertyChanged(); } }
        public ChartValues<Result> ChartValues4 { get => _ChartValues4; set { _ChartValues4 = value; OnPropertyChanged(); } }
        public ChartValues<double> MeanValues1 { get => _MeanValues1; set { _MeanValues1 = value; OnPropertyChanged(); } }
        public ChartValues<double> MeanValues2 { get => _MeanValues2; set { _MeanValues2 = value; OnPropertyChanged(); } }
        public ChartValues<double> MeanValues3 { get => _MeanValues3; set { _MeanValues3 = value; OnPropertyChanged(); } }
        public ChartValues<double> MeanValues4 { get => _MeanValues4; set { _MeanValues4 = value; OnPropertyChanged(); } }
        public ChartValues<double> PlusOneSDValues1 { get => _PlusOneSDValues1; set { _PlusOneSDValues1 = value; OnPropertyChanged(); } }
        public ChartValues<double> PlusOneSDValues2 { get => _PlusOneSDValues2; set { _PlusOneSDValues2 = value; OnPropertyChanged(); } }
        public ChartValues<double> PlusOneSDValues3 { get => _PlusOneSDValues3; set { _PlusOneSDValues3 = value; OnPropertyChanged(); } }
        public ChartValues<double> PlusOneSDValues4 { get => _PlusOneSDValues4; set { _PlusOneSDValues4 = value; OnPropertyChanged(); } }
        public ChartValues<double> MinusOneSDValues1 { get => _MinusOneSDValues1; set { _MinusOneSDValues1 = value; OnPropertyChanged(); } }
        public ChartValues<double> MinusOneSDValues2 { get => _MinusOneSDValues2; set { _MinusOneSDValues2 = value; OnPropertyChanged(); } }
        public ChartValues<double> MinusOneSDValues3 { get => _MinusOneSDValues3; set { _MinusOneSDValues3 = value; OnPropertyChanged(); } }
        public ChartValues<double> MinusOneSDValues4 { get => _MinusOneSDValues4; set { _MinusOneSDValues4 = value; OnPropertyChanged(); } }
        public ChartValues<double> PlusTwoSDValues2 { get => _PlusTwoSDValues2; set { _PlusTwoSDValues2 = value; OnPropertyChanged(); } }
        public ChartValues<double> PlusTwoSDValues1 { get => _PlusTwoSDValues1; set { _PlusTwoSDValues1 = value; OnPropertyChanged(); } }
        public ChartValues<double> PlusTwoSDValues3 { get => _PlusTwoSDValues3; set { _PlusTwoSDValues3 = value; OnPropertyChanged(); } }
        public ChartValues<double> PlusTwoSDValues4 { get => _PlusTwoSDValues4; set { _PlusTwoSDValues4 = value; OnPropertyChanged(); } }
        public ChartValues<double> MinusTwoSDValues1 { get => _MinusTwoSDValues1; set { _MinusTwoSDValues1 = value; OnPropertyChanged(); } }
        public ChartValues<double> MinusTwoSDValues2 { get => _MinusTwoSDValues2; set { _MinusTwoSDValues2 = value; OnPropertyChanged(); } }
        public ChartValues<double> MinusTwoSDValues3 { get => _MinusTwoSDValues3; set { _MinusTwoSDValues3 = value; OnPropertyChanged(); } }
        public ChartValues<double> MinusTwoSDValues4 { get => _MinusTwoSDValues4; set { _MinusTwoSDValues4 = value; OnPropertyChanged(); } }
        public ChartValues<double> PlusThreeSDValues1 { get => _PlusThreeSDValues1; set { _PlusThreeSDValues1 = value; OnPropertyChanged(); } }
        public ChartValues<double> PlusThreeSDValues2 { get => _PlusThreeSDValues2; set { _PlusThreeSDValues2 = value; OnPropertyChanged(); } }
        public ChartValues<double> PlusThreeSDValues3 { get => _PlusThreeSDValues3; set { _PlusThreeSDValues3 = value; OnPropertyChanged(); } }
        public ChartValues<double> PlusThreeSDValues4 { get => _PlusThreeSDValues4; set { _PlusThreeSDValues4 = value; OnPropertyChanged(); } }
        public ChartValues<double> MinusThreeSDValues3 { get => _MinusThreeSDValues3; set { _MinusThreeSDValues3 = value; OnPropertyChanged(); } }
        public ChartValues<double> MinusThreeSDValues2 { get => _MinusThreeSDValues2; set { _MinusThreeSDValues2 = value; OnPropertyChanged(); } }
        public ChartValues<double> MinusThreeSDValues1 { get => _MinusThreeSDValues1; set { _MinusThreeSDValues1 = value; OnPropertyChanged(); } }
        public ChartValues<double> MinusThreeSDValues4 { get => _MinusThreeSDValues4; set { _MinusThreeSDValues4 = value; OnPropertyChanged(); } }
        public float totalWidth1 { get => _totalWidth1; set { _totalWidth1 = value; OnPropertyChanged(); } }
        public float totalWidth2 { get => _totalWidth2; set { _totalWidth2 = value; OnPropertyChanged(); } }
        public float totalWidth3 { get => _totalWidth3; set { _totalWidth3 = value; OnPropertyChanged(); } }
        public float totalWidth4 { get => _totalWidth4; set { _totalWidth4 = value; OnPropertyChanged(); } }

        public ICommand PrintCommand { get; set; }
        public ICommand PrintCalibCommand { get; set; }
        public ICommand PrintChartCommand { get; set; }
        public ICommand LoadedCommand { get; set; }
        public ICommand DeviceSelectionChangedCommand { get; set; }
        public ICommand TestSelectionChangedCommand { get; set; }
        public ICommand appRangeCommand { get; set; }
        public ICommand DateSelectionChangedCommand { get; set; }
        public DateTime StartDate { get => _StartDate; set { _StartDate = value; OnPropertyChanged(); } }
        public DateTime EndDate { get => _EndDate; set { _EndDate = value; OnPropertyChanged(); } }

        public ObservableCollection<string> FilterOptions { get; set; } = new()
         {
             "Nhà sản xuât",
             "Đang sử dụng",
             "Thống kê"
         };

        private double _chartHeight = 300; // Default value
        public double ChartHeight
        {
            get => _chartHeight;
            set
            {
                if (_chartHeight != value)
                {
                    _chartHeight = value;
                    OnPropertyChanged(nameof(ChartHeight));
                }
            }
        }

        private string _SelectedFilterOptions;
        public string SelectedFilterOptions
        {
            get => _SelectedFilterOptions;
            set
            {
                _SelectedFilterOptions = value;
                OnPropertyChanged();
            }
        }

        public ICommand SeenChartTypeSelectionChangedCommand { get; set; }

        public bool isCheck { get => _isCheck; set { _isCheck = value; OnPropertyChanged(); } }
        public Test SelectedTest
        {
            get => _SelectedTest;
            set
            {
                _SelectedTest = value;
                OnPropertyChanged();
            }
        }
        public Device SelectedDevice
        {
            get => _SelectedDevice;
            set
            {
                _SelectedDevice = value;
                OnPropertyChanged();
            }
        }
        public HomeViewModel_V2()
        {
            _dbContext  = new QcManagmentContext();
            SelectedFilterOptions = FilterOptions[1];
            Visibility1 = Visibility.Collapsed;
            Visibility2 = Visibility.Collapsed;
            Visibility3 = Visibility.Collapsed;
            isCheck = false;

            LoadedCommand = new RelayCommand<Test>((p) =>
            {
                return true;

            }, async (p) =>
            {
                // load UI lists and preserve selected device/test when possible
                await LoadNew();

                // If a device and test are already selected (e.g. navigating back), ensure List is repopulated
                // before drawing charts to avoid the "No data" message.
                if (SelectedDevice != null && SelectedTest != null)
                {
                    await UpdateLissResultAsync();
                    await ViewChart(List);
                }
            });

            appRangeCommand = new RelayCommand<Test>((p) =>
            {
                return true;

            },async (p) =>
            {
               await ViewChart(List);
            });

            PrintCommand = new RelayCommand<object>((p) =>
            {
                if (SelectedTest == null || SelectedDevice == null || List.Count == 0) return false;
                else
                    return true;

            }, (p) =>
            {
                var results = List.ToList();
                ReportView rp = new ReportView(results, SelectedFilterOptions);
                rp.ShowDialog();

            });

            SeenChartTypeSelectionChangedCommand = new RelayCommand<object>((p) =>
            {
                return true;

            }, async (p) =>
            {
                await UpdateLissResultAsync();
                await ViewChart(List);
            });


            PrintCalibCommand = new RelayCommand<object>((p) =>
            {
                if (SelectedTest == null || SelectedDevice == null || SelectedTest.TestType == 1 || List.Count == 0) return false;
                else
                    return true;

            }, (p) =>
            {
                    var calresults = DataProvider.Ins.DB.CalResults
                        .Include(s => s.IdUserNavigation)
                        .Include(s => s.IdCalDetailNavigation)
                        .ThenInclude(cd => cd.IdCalInforNavigation)
                        .ThenInclude(ct => ct.IdCalTypeNavigation)
                        .Where(s => s.IdDevice == SelectedDevice.Id
                                        && s.IdTest == SelectedTest.Id
                                        && s.DateRun >= StartDate
                                        && s.DateRun <= EndDate)
                        .ToList();

                CalibReportView rp = new CalibReportView(calresults);
                rp.ShowDialog();

            });

            PrintChartCommand = new RelayCommand<object>((p) =>
            {
                if (SelectedTest == null || SelectedDevice == null || List.Count == 0 || SelectedTest.TestType == 1)
                    return false;
 
                else
                    return true;

            }, (p) =>
            {
                // Thiết lập dữ liệu cho báo cáo
                var results = List.ToList();
                ChartReportView rp = new ChartReportView(results, SelectedFilterOptions);
                rp.ShowDialog();

            });

            DeviceSelectionChangedCommand = new RelayCommand<ControlInfo>((p) => 
            {
                return true;

            }, async (p) =>
            {

                if (SelectedDevice != null)
                {
                    // Update the TestList based on the selected device
                     TestList = new ObservableCollection<Test>(_dbContext.DeviceTests
                        .Where(s => s.IdDevice == SelectedDevice.Id)
                        .Select(s => s.IdTestNavigation)
                        .OrderBy(s => s.Index));
                }
                if(SelectedTest != null)
                {
                    await UpdateLissResultAsync();
                    if (SelectedTest.TestType == 2)
                    {
                        await ViewChart(List);
                    }
                    else
                    {
                        MessageBox.Show("Xét nghiệm định tính tạm thời chưa có Biều đồ Levey-Jenning. Xin cảm ơn!");
                        Visibility1 = Visibility.Collapsed;
                        Visibility2 = Visibility.Collapsed;
                        Visibility3 = Visibility.Collapsed;
                        Visibility4 = Visibility.Collapsed;
                    }
                }
            });

            TestSelectionChangedCommand = new RelayCommand<CartesianChart>((p) =>
            {
                if(SelectedTest == null)
                {
                    return false;
                }
                else
                {
                    return true;
                }

            }, async (p) =>
            {
                await UpdateLissResultAsync();
                if (SelectedTest.TestType == 2)
                {
                    await ViewChart(List);
                }
                else
                {
                    MessageBox.Show("Xét nghiệm định tính tạm thời chưa có Biều đồ Levey-Jenning. Xin cảm ơn!");
                    Visibility1 = Visibility.Collapsed;
                    Visibility2 = Visibility.Collapsed;
                    Visibility3 = Visibility.Collapsed;
                    Visibility4 = Visibility.Collapsed;
                }

            });

            DateSelectionChangedCommand = new RelayCommand<CartesianChart>((p) =>
            {
                if (SelectedTest == null)
                {
                    return false;
                }
                else
                {
                    return true;
                }

            }, async (p) =>
            {
                await UpdateLissResultAsync();
                if (SelectedTest.TestType == 2)
                {
                    await ViewChart(List);
                }
                else
                {
                    MessageBox.Show("Xét nghiệm định tính tạm thời chưa có Biều đồ Levey-Jenning. Xin cảm ơn!");
                    Visibility1 = Visibility.Collapsed;
                    Visibility2 = Visibility.Collapsed;
                    Visibility3 = Visibility.Collapsed;
                    Visibility4 = Visibility.Collapsed;
                }

            });
        }

       private async Task UpdateLissResultAsync()
{
    using (var context = new QcManagmentContext())
    {
        var results = new ObservableCollection<Result>(await context.Results
            .AsNoTracking()
            .Include(s => s.IdControlDetailNavigation)
            .Include(s => s.IdUserNavigation)
            .Include(s => s.IdLevelNavigation)
            .Include(s => s.IdTestNavigation)
            .Include(s => s.IdDeviceNavigation)
            .Include(s => s.IdTestNavigation.IdUnitTableNavigation)
            .Include(s => s.IdControlDetailNavigation.IdControlInfoNavigation)
            .Where(s => s.IdDevice == SelectedDevice.Id
                       && s.IdTest == SelectedTest.Id
                       && s.DateRun.Date >= StartDate.Date
                       && s.DateRun.Date <= EndDate.Date)
            .OrderBy(s => s.DateRun.Year)
            .ThenBy(s => s.DateRun.Month)
            .ThenBy(s => s.DateRun.Day)
            .ThenBy(s => s.IndexQc)
            .ToListAsync());

        // Tính lại mean và sd cho từng nhóm IdControlDetail
        var controlDetailGroups = results
            .Where(r => r.IdControlDetailNavigation != null && r.Result1.HasValue)
            .GroupBy(r => r.IdControlDetailNavigation.Id);

        foreach (var group in controlDetailGroups)
        {
            var valueList = group.Select(r => r.Result1.Value).ToList();
            if (valueList.Count == 0) continue;

            double mean = Math.Round(valueList.Average(), 3);
            double sd = Math.Round(Math.Sqrt(valueList.Sum(v => Math.Pow(v - mean, 2)) / valueList.Count), 3);
            if (sd == 0) sd = 0.01;

            // Gán lại cho tất cả các Result trong nhóm
            foreach (var r in group)
            {
                r.IdControlDetailNavigation.MeanApp = mean;
                r.IdControlDetailNavigation.SdApp = sd;
            }
        }

        // Tính ZScore cho từng Result
        foreach (var r in results)
        {
            if (r.IdControlDetailNavigation != null && r.Result1.HasValue)
            {
                double? mean = null, sd = null;
                switch (SelectedFilterOptions)
                {
                    case "Nhà sản xuât":
                        mean = r.IdControlDetailNavigation.MeanNsx;
                        sd = r.IdControlDetailNavigation.SdNsx;
                        break;
                    case "Đang sử dụng":
                        mean = r.IdControlDetailNavigation.CurMean;
                        sd = r.IdControlDetailNavigation.CurSd;
                        break;
                    case "Thống kê":
                        mean = r.IdControlDetailNavigation.MeanApp;
                        sd = r.IdControlDetailNavigation.SdApp;
                        break;
                        }
                        if (sd == 0) sd = 0.001; // Tránh chia cho 0
                        if (mean.HasValue && sd.HasValue && sd.Value != 0)
                           r.ZScore = Math.Round((r.Result1.Value - mean.Value) / sd.Value, 2);
                        else
                            r.ZScore = null;
            }
            else
            {
                r.ZScore = null;
            }
        }

        List = results;
    }
}

        private async Task LoadNew()
        {
            IsLoading = true;
            try
            {
                _dbContext = await Task.Run(() =>  DataProvider.Ins.DB);
                DeviceList = new ObservableCollection<Device>(_dbContext.Devices);
                List = new ObservableCollection<Result>();
                if (SelectedDevice != null)
                {
                    TestList = new ObservableCollection<Test>(_dbContext.DeviceTests
                        .Where(s => s.IdDevice == SelectedDevice.Id)
                        .Select(s => s.IdTestNavigation)
                        .OrderBy(s => s.Index));
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions
            }
            finally
            {
                IsLoading = false;
            }
        }
        private void InitializeYAxisLabelFormatter()
        {
            YAxisLabelFormatter = value =>
            {
                switch (value)
                {
                    case 0: return "Mean";
                    case 1: return "1SD";
                    case 2: return "2SD";
                    case 3: return "3SD";
                    case -1: return "-1SD";
                    case -2: return "-2SD";
                    case -3: return "-3SD";
                    default: return value.ToString(); // Fallback for other values
                }
            };
        }
        private async Task ViewChart(ObservableCollection<Result> results)
        {
            Visibility1 = Visibility.Collapsed;
            Visibility2 = Visibility.Collapsed;
            Visibility3 = Visibility.Collapsed;
            Visibility4 = Visibility.Collapsed;

            IsLoading = true;
            OnPropertyChanged(nameof(IsLoading));
            InitializeYAxisLabelFormatter();
            try
            {
                if (SelectedDevice == null || SelectedTest == null)
                {
                    return;
                }
                              
                if (!results.Any())
                {
                    MessageBox.Show("Không có dữ liệu", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                var levelList = results.GroupBy(s => s.IdLevel).ToList();

                await Task.Run(() =>
                {
                    foreach (var resultByLevel in levelList)
                    {
                        var result = LoadChart1(resultByLevel);
                        var chartValues = result.Item1;
                        var visibility = result.Item2;
                        var dates = result.Item3;
                        var firstResult = resultByLevel.FirstOrDefault();

                        var meanValues = new ChartValues<double>(Enumerable.Repeat(0.0, chartValues.Count + 1));
                        var plusOneSDValues = new ChartValues<double>(Enumerable.Repeat(1.0, chartValues.Count + 1));
                        var minusOneSDValues = new ChartValues<double>(Enumerable.Repeat(-1.0, chartValues.Count + 1));
                        var plusTwoSDValues = new ChartValues<double>(Enumerable.Repeat(2.0, chartValues.Count + 1));
                        var minusTwoSDValues = new ChartValues<double>(Enumerable.Repeat(-2.0, chartValues.Count + 1));
                        var plusThreeSDValues = new ChartValues<double>(Enumerable.Repeat(3.0, chartValues.Count + 1));
                        var minusThreeSDValues = new ChartValues<double>(Enumerable.Repeat(-3.0, chartValues.Count + 1));

                        float cmPerPoint = 2.0f; // 1 cm
                        float pixelsPerPoint = CmToPixels(cmPerPoint);
                        int numberOfPoints = chartValues.Count;
                        float totalWidth = pixelsPerPoint * numberOfPoints;

                        if (firstResult != null && firstResult.IdControlDetailNavigation != null)
                        {
                            string levelName = firstResult.IdLevelNavigation?.Name ?? "";
                            double? mean = null;
                            double? sd = null;
                            switch (SelectedFilterOptions)
                            {
                                case "Nhà sản xuât":
                                    mean = firstResult.IdControlDetailNavigation.MeanNsx;
                                    sd = firstResult.IdControlDetailNavigation.SdNsx;
                                    break;
                                case "Đang sử dụng":
                                    mean = firstResult.IdControlDetailNavigation.CurMean;
                                    sd = firstResult.IdControlDetailNavigation.CurSd;
                                    break;
                                case "Thống kê":
                                    mean = firstResult.IdControlDetailNavigation.MeanApp;
                                    sd = firstResult.IdControlDetailNavigation.SdApp;
                                    break;
                            }
                            Application.Current.Dispatcher.Invoke(() =>
                        {
                            switch (resultByLevel.Key)
                            {
                                case 1:
                                case 4:
                                case 7:
                                    LevelName1 = levelName;
                                    Mean1 = mean;
                                    SD1 = sd;
                                    Range1 = $"{(mean - 2 * sd):F2}  -  {(mean + 2 * sd):F2}";
                                    TotalPoints1 = $"{chartValues.Count}";
                                    MeanValues1 = meanValues;
                                    PlusOneSDValues1 = plusOneSDValues;
                                    MinusOneSDValues1 = minusOneSDValues;
                                    PlusTwoSDValues1 = plusTwoSDValues;
                                    MinusTwoSDValues1 = minusTwoSDValues;
                                    PlusThreeSDValues1 = plusThreeSDValues;
                                    MinusThreeSDValues1 = minusThreeSDValues;
                                    ChartValues1 = chartValues;
                                    Visibility1 = visibility;
                                    Dates1 = dates;
                                    totalWidth1 = totalWidth;
                                    break;
                                case 2:
                                case 5:
                                case 8:
                                case 9:
                                    LevelName2 = levelName;
                                    Mean2 = mean;
                                    SD2 = sd;
                                    Range2 = $"{(mean - 2 * sd):F2} - {(mean + 2 * sd):F2}";
                                    TotalPoints2 = $"{chartValues.Count}";
                                    MeanValues2 = meanValues;
                                    PlusOneSDValues2 = plusOneSDValues;
                                    MinusOneSDValues2 = minusOneSDValues;
                                    PlusTwoSDValues2 = plusTwoSDValues;
                                    MinusTwoSDValues2 = minusTwoSDValues;
                                    PlusThreeSDValues2 = plusThreeSDValues;
                                    MinusThreeSDValues2 = minusThreeSDValues;
                                    ChartValues2 = chartValues;
                                    Visibility2 = visibility;
                                    Dates2 = dates;
                                    totalWidth2 = totalWidth;
                                    break;
                                case 3:
                                case 6:
                                case 10:
                                    LevelName3 = levelName;
                                    Mean3 = mean;
                                    SD3 = sd;
                                    Range3 = $"{(mean - 2 * sd):F2} - {(mean + 2 * sd):F2}";
                                    TotalPoints3 = $"{chartValues.Count}";
                                    MeanValues3 = meanValues;
                                    PlusOneSDValues3 = plusOneSDValues;
                                    MinusOneSDValues3 = minusOneSDValues;
                                    PlusTwoSDValues3 = plusTwoSDValues;
                                    MinusTwoSDValues3 = minusTwoSDValues;
                                    PlusThreeSDValues3 = plusThreeSDValues;
                                    MinusThreeSDValues3 = minusThreeSDValues;
                                    ChartValues3 = chartValues;
                                    Visibility3 = visibility;
                                    Dates3 = dates;
                                    totalWidth3 = totalWidth;
                                    break;
                                case 11:
                                    LevelName4 = levelName;
                                    Mean4 = mean;
                                    SD4 = sd;
                                    Range4 = $"{(mean - 2 * sd):F2} - {(mean + 2 * sd):F2}";
                                    TotalPoints4 = $"{chartValues.Count}";
                                    MeanValues4 = meanValues;
                                    PlusOneSDValues4 = plusOneSDValues;
                                    MinusOneSDValues4 = minusOneSDValues;
                                    PlusTwoSDValues4 = plusTwoSDValues;
                                    MinusTwoSDValues4 = minusTwoSDValues;
                                    PlusThreeSDValues4 = plusThreeSDValues;
                                    MinusThreeSDValues4 = minusThreeSDValues;
                                    ChartValues4 = chartValues;
                                    Visibility4 = visibility;
                                    Dates4 = dates;
                                    totalWidth4 = totalWidth;
                                    break;
                            }
                        });
                        }
                    }
                });

                await LoadChartAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải biểu đồ: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private Tuple<ChartValues<Result>, Visibility, ObservableCollection<string>> LoadChart1(IGrouping<int, Result> results)
        {
            var visibility = Visibility.Collapsed;
            var dataPoints = new ChartValues<Result>();
            var dates = new ObservableCollection<string>();

            foreach (var item in results)
            {
                dataPoints.Add(item);
                dates.Add($"{item.DateRun.ToString("dd/MM")} - {item.IndexQc}");
            }

            if (dataPoints.Count > 0)
            {
                visibility = Visibility.Visible;
            }

            return new Tuple<ChartValues<Result>, Visibility, ObservableCollection<string>>(dataPoints, visibility, dates);
        }

        private async Task LoadChartAsync()
        {
            var mapper = Mappers.Xy<Result>()
        .X((value, index) => index)
        .Y(value =>
        {
            if (!value.ZScore.HasValue) return 0;
            if (value.ZScore >= 4) return 4;
            if (value.ZScore <= -4) return -4;
            return value.ZScore.Value;
        })
//.Fill((value, index) => (value.ZScore.HasValue && (value.ZScore > 2 || value.ZScore < -2)) ? Brushes.Red : null)
// replace this line:
// .Fill((value, index) => (bool)(value.IsOutRange) ? Brushes.Red : null)
// with either of these:

// Option A — null -> true using null-coalescing
        .Fill((value, index) => (value.IsOutRange ?? true) ? Brushes.Red : null)

// Option B — null -> true using GetValueOrDefault
        .Fill((value, index) => value.IsOutRange.GetValueOrDefault(true) ? Brushes.Red : null)

// Option C — equivalent using explicit null check
        .Fill((value, index) => (value.IsOutRange == null || value.IsOutRange == true) ? Brushes.Red : null)
        .Stroke(item => Brushes.Transparent);

            await Task.Run(() =>
            {
                Charting.For<Result>(mapper, SeriesOrientation.Horizontal);
            });
        }
        public static float CmToPixels(float cm)
        {
            // 1 inch = 2.54 cm
            float inches = cm / 2.54f;

            // Get the DPI (Dots Per Inch) of the screen
            using (System.Drawing.Graphics g = System.Drawing.Graphics.FromHwnd(IntPtr.Zero))
            {
                float dpiX = g.DpiX;
                return inches * dpiX;
            }
        }
    }

}



