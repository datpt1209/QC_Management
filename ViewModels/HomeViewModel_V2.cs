// (full file - only ViewChart start and ClearAllChartData changed as described)
using LiveCharts;
using LiveCharts.Configurations;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
using Microsoft.EntityFrameworkCore;
using QC_Management.Models;
using QC_Management.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Collections.Generic;

namespace QC_Management.ViewModels
{
    public class HomeViewModel_V2 : BaseViewModel, IDisposable
    {
        private bool _disposed = false;
        private Action _resultsUpdatedHandler;
        private QcManagmentContext _dbContext;

        // CancellationTokenSource để hủy ViewChart đang chạy khi user chuyển xét nghiệm mới
        private CancellationTokenSource _viewChartCts = new CancellationTokenSource();

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
        private Visibility _Visibility4;
        private bool _isLoading;
        private bool _hasNoData;
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
        private string _noDataTitle = "Không có dữ liệu";
        private string _noDataMessage = "Vui lòng chọn khoảng thời gian hoặc xét nghiệm khác";

        public string NoDataTitle { get => _noDataTitle; set { _noDataTitle = value; OnPropertyChanged(); } }
        public string NoDataMessage { get => _noDataMessage; set { _noDataMessage = value; OnPropertyChanged(); } }

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
        public Visibility Visibility4 { get => _Visibility4; set { _Visibility4 = value; OnPropertyChanged(); } }


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
        public bool HasNoData
        {
            get => _hasNoData;
            set { _hasNoData = value; OnPropertyChanged(); }
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


        // Chiều rộng thực tế hiển thị: max(totalWidth, chiều rộng vùng chart)
        // Dùng để chart luôn chiếm tối thiểu hết chiều ngang màn hình
        private float _effectiveWidth1;
        private float _effectiveWidth2;
        private float _effectiveWidth3;
        private float _effectiveWidth4;
        public float effectiveWidth1 { get => _effectiveWidth1; set { _effectiveWidth1 = value; OnPropertyChanged(); } }
        public float effectiveWidth2 { get => _effectiveWidth2; set { _effectiveWidth2 = value; OnPropertyChanged(); } }
        public float effectiveWidth3 { get => _effectiveWidth3; set { _effectiveWidth3 = value; OnPropertyChanged(); } }
        public float effectiveWidth4 { get => _effectiveWidth4; set { _effectiveWidth4 = value; OnPropertyChanged(); } }


        // Chiều rộng vùng chart (cột phải trừ panel thống kê 130px và margins)
        // Được cập nhật từ SizeChanged event trong code-behind
        private double _chartAreaWidth = 800;
        public double ChartAreaWidth
        {
            get => _chartAreaWidth;
            set { _chartAreaWidth = value; OnPropertyChanged(); }
        }

        // New dynamic SeriesCollections for each chart
        private SeriesCollection _SeriesCollection1;
        private SeriesCollection _SeriesCollection2;
        private SeriesCollection _SeriesCollection3;
        private SeriesCollection _SeriesCollection4;

        public SeriesCollection SeriesCollection1 { get => _SeriesCollection1; set { _SeriesCollection1 = value; OnPropertyChanged(); } }
        public SeriesCollection SeriesCollection2 { get => _SeriesCollection2; set { _SeriesCollection2 = value; OnPropertyChanged(); } }
        public SeriesCollection SeriesCollection3 { get => _SeriesCollection3; set { _SeriesCollection3 = value; OnPropertyChanged(); } }
        public SeriesCollection SeriesCollection4 { get => _SeriesCollection4; set { _SeriesCollection4 = value; OnPropertyChanged(); } }

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
            _dbContext = new QcManagmentContext();
            SelectedFilterOptions = FilterOptions[1];
            Visibility1 = Visibility.Collapsed;
            Visibility2 = Visibility.Collapsed;
            Visibility3 = Visibility.Collapsed;
            Visibility4 = Visibility.Collapsed;
            isCheck = false;

            // initialize empty series collections to avoid binding null in XAML
            SeriesCollection1 = new SeriesCollection();
            SeriesCollection2 = new SeriesCollection();
            SeriesCollection3 = new SeriesCollection();
            SeriesCollection4 = new SeriesCollection();

            // Fix ④: Đăng ký mapper 1 lần duy nhất tại đây, không đăng ký lại trong ViewChart()
            var mapper = Mappers.Xy<Result>()
                .X((value, index) => index)
                .Y(value =>
                {
                    if (!value.ZScore.HasValue) return 0;
                    if (value.ZScore >= 4) return 4;
                    if (value.ZScore <= -4) return -4;
                    return value.ZScore.Value;
                })
                .Fill((value, index) => (value.IsOutRange == true) ? Brushes.Red : null)
                .Stroke((value, index) => (value.IsOutRange == true) ? Brushes.Red : Brushes.Transparent);
            Charting.For<Result>(mapper, SeriesOrientation.Horizontal);

            LoadedCommand = new RelayCommand<Test>((p) =>
            {
                return true;

            }, async (p) =>
            {
                // load UI lists and preserve selected device/test when possible
                await LoadNew();

                // If a device and test are already selected ensure List is repopulated then draw charts.
                if (SelectedDevice != null && SelectedTest != null)
                {
                    var results = await UpdateLissResultAsync();
                    List = results;
                    await StartViewChart(results);
                }
            });

            appRangeCommand = new RelayCommand<Test>((p) =>
            {
                return true;

            }, async (p) =>
            {
                var results = await UpdateLissResultAsync();
                List = results;
                await StartViewChart(results);
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
                var results = await UpdateLissResultAsync();
                List = results;
                await StartViewChart(results);
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
                if (SelectedTest != null)
                {
                    var results = await UpdateLissResultAsync();
                    List = results;
                    if (SelectedTest.TestType == 2)
                    {
                        await StartViewChart(results);
                    }
                    else
                    {
                        // show overlay instead of MessageBox (overlay logic already in StartViewChart)
                        HasNoData = true;
                        NoDataTitle = "Xét nghiệm định tính";
                        NoDataMessage = "Xét nghiệm định tính không vẽ được biểu đồ";
                        Visibility1 = Visibility.Collapsed;
                        Visibility2 = Visibility.Collapsed;
                        Visibility3 = Visibility.Collapsed;
                        Visibility4 = Visibility.Collapsed;
                    }
                }
            });

            TestSelectionChangedCommand = new RelayCommand<CartesianChart>((p) =>
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
                // Không gọi Dispose() khi đổi selection — Dispose chỉ gọi khi View đóng
                var results = await UpdateLissResultAsync();
                List = results;
                if (SelectedTest.TestType == 2)
                {
                    await StartViewChart(results);
                }
                else
                {
                    HasNoData = true;
                    NoDataTitle = "Xét nghiệm định tính";
                    NoDataMessage = "Xét nghiệm định tính không vẽ được biểu đồ";
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
                var results = await UpdateLissResultAsync();
                List = results;
                await StartViewChart(results);
            });

            // Fix ①: Lưu handler vào field để có thể hủy đăng ký trong Dispose()
            // Nếu dùng lambda trực tiếp, không bao giờ hủy được → ViewModel cũ không được GC giải phóng
            // results updated notifier: use returned snapshot to update List + redraw
            _resultsUpdatedHandler = () =>
            {
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            var results = await UpdateLissResultAsync();
                            // update List on UI thread (or at least synchronize)
                            Application.Current.Dispatcher.Invoke(() => List = results);
                            await StartViewChart(results);
                        }
                        catch
                        {
                            // swallow, non-fatal
                        }
                    });
                }));
            };
            ResultChangeNotifier.ResultsUpdated += _resultsUpdatedHandler;

        }

        // ── Fix ① + ② + ③: Giải phóng toàn bộ tài nguyên khi View bị đóng ────────
        // Gọi method này trong code-behind của View:
        //   protected override void OnClosed(EventArgs e) {
        //       (DataContext as IDisposable)?.Dispose();
        //       base.OnClosed(e);
        //   }
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            // Hủy ViewChart đang chạy nếu có
            try
            {
                _viewChartCts?.Cancel();
            }
            catch (ObjectDisposedException) { }
            try
            {
                _viewChartCts?.Dispose();
            }
            catch { }
            _viewChartCts = null;

            // Fix ①: Hủy đăng ký event → ViewModel cũ sẽ được GC giải phóng
            ResultChangeNotifier.ResultsUpdated -= _resultsUpdatedHandler;

            // Fix ②: Dispose DbContext → giải phóng connection pool
            _dbContext?.Dispose();
            _dbContext = null;

            // Fix ③: Xóa dữ liệu chart → LiveCharts không còn giữ references
            ClearAllChartData();
        }

        private void ClearAllChartData()
        {
            try
            {
                // create new instances instead of Clear() to ensure LiveCharts releases old references
                ChartValues1 = new ChartValues<Result>();
                ChartValues2 = new ChartValues<Result>();
                ChartValues3 = new ChartValues<Result>();
                ChartValues4 = new ChartValues<Result>();

                MeanValues1 = new ChartValues<double>();
                MeanValues2 = new ChartValues<double>();
                MeanValues3 = new ChartValues<double>();
                MeanValues4 = new ChartValues<double>();

                PlusOneSDValues1 = new ChartValues<double>();
                PlusOneSDValues2 = new ChartValues<double>();
                PlusOneSDValues3 = new ChartValues<double>();
                PlusOneSDValues4 = new ChartValues<double>();

                MinusOneSDValues1 = new ChartValues<double>();
                MinusOneSDValues2 = new ChartValues<double>();
                MinusOneSDValues3 = new ChartValues<double>();
                MinusOneSDValues4 = new ChartValues<double>();

                PlusTwoSDValues1 = new ChartValues<double>();
                PlusTwoSDValues2 = new ChartValues<double>();
                PlusTwoSDValues3 = new ChartValues<double>();
                PlusTwoSDValues4 = new ChartValues<double>();

                MinusTwoSDValues1 = new ChartValues<double>();
                MinusTwoSDValues2 = new ChartValues<double>();
                MinusTwoSDValues3 = new ChartValues<double>();
                MinusTwoSDValues4 = new ChartValues<double>();

                PlusThreeSDValues1 = new ChartValues<double>();
                PlusThreeSDValues2 = new ChartValues<double>();
                PlusThreeSDValues3 = new ChartValues<double>();
                PlusThreeSDValues4 = new ChartValues<double>();

                MinusThreeSDValues1 = new ChartValues<double>();
                MinusThreeSDValues2 = new ChartValues<double>();
                MinusThreeSDValues3 = new ChartValues<double>();
                MinusThreeSDValues4 = new ChartValues<double>();

                SeriesCollection1?.Clear(); SeriesCollection2?.Clear();
                SeriesCollection3?.Clear(); SeriesCollection4?.Clear();
                SeriesCollection1 = new SeriesCollection();
                SeriesCollection2 = new SeriesCollection();
                SeriesCollection3 = new SeriesCollection();
                SeriesCollection4 = new SeriesCollection();
            }
            catch { /* ignore cleanup errors */ }
        }

        private async Task<ObservableCollection<Result>> UpdateLissResultAsync()
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
                    .Where(s => SelectedDevice != null && SelectedTest != null
                               && s.IdDevice == SelectedDevice.Id
                               && s.IdTest == SelectedTest.Id
                               && s.DateRun.Date >= StartDate.Date
                               && s.DateRun.Date <= EndDate.Date)
                    .OrderBy(s => s.DateRun.Year)
                    .ThenBy(s => s.DateRun.Month)
                    .ThenBy(s => s.DateRun.Day)
                    .ThenBy(s => s.IndexQc)
                    .ToListAsync());

                // Recompute MeanApp/SdApp per ControlInfoDetail group (for "Thống kê" mode)
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

                    // write back to the in-memory ControlInfoDetail instance so chart labels use it
                    foreach (var r in group)
                    {
                        r.IdControlDetailNavigation.MeanApp = mean;
                        r.IdControlDetailNavigation.SdApp = sd;
                    }
                }

                // Compute ZScore according to selected filter:
                foreach (var r in results)
                {
                    if (r.IdControlDetailNavigation != null && r.Result1.HasValue)
                    {
                        double? mean = null, sd = null;

                        if (string.Equals(SelectedFilterOptions, "Đang sử dụng", StringComparison.OrdinalIgnoreCase))
                        {
                            // If DB already stored ZScore, keep it
                            if (r.ZScore.HasValue)
                            {
                                // keep persisted value
                                continue;
                            }

                            // otherwise compute from current control values (prefer CurMean/CurSd)
                            mean = r.IdControlDetailNavigation.CurMean ?? r.IdControlDetailNavigation.MeanApp ?? r.IdControlDetailNavigation.MeanNsx;
                            sd = r.IdControlDetailNavigation.CurSd ?? r.IdControlDetailNavigation.SdApp ?? r.IdControlDetailNavigation.SdNsx;
                        }
                        else if (string.Equals(SelectedFilterOptions, "Nhà sản xuât", StringComparison.OrdinalIgnoreCase))
                        {
                            mean = r.IdControlDetailNavigation.MeanNsx;
                            sd = r.IdControlDetailNavigation.SdNsx;
                        }
                        else // "Thống kê" or other
                        {
                            mean = r.IdControlDetailNavigation.MeanApp;
                            sd = r.IdControlDetailNavigation.SdApp;
                        }

                        if (sd == 0) sd = 0.001; // avoid divide-by-zero
                        if (mean.HasValue && sd.HasValue && sd.Value != 0)
                        {
                            r.ZScore = Math.Round((r.Result1.Value - mean.Value) / sd.Value, 2);
                        }
                        else
                        {
                            r.ZScore = null;
                        }
                    }
                    else
                    {
                        r.ZScore = null;
                    }
                }

                // DO NOT assign to List here — return snapshot, caller will assign when appropriate
                return results;
            }
        }
        private async Task LoadNew()
        {
            IsLoading = true;
            try
            {
                _dbContext = await Task.Run(() => DataProvider.Ins.DB);
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

        // Hủy ViewChart đang chạy → chạy cái mới. Gọi thay vì ViewChart() trực tiếp.
        private async Task StartViewChart(ObservableCollection<Result> results)
        {
            if (_disposed) return;
            // Nếu chưa chọn xét nghiệm / thiết bị
            if (SelectedDevice == null || SelectedTest == null)
            {
                HasNoData = true;
                NoDataTitle = "Không có dữ liệu";
                NoDataMessage = "Vui lòng chọn thiết bị và xét nghiệm.";
                Visibility1 = Visibility.Collapsed;
                Visibility2 = Visibility.Collapsed;
                Visibility3 = Visibility.Collapsed;
                Visibility4 = Visibility.Collapsed;
                return;
            }
            // Nếu xét nghiệm định tính → không vẽ biểu đồ, hiển thị overlay thông báo
            if (SelectedTest.TestType != 2)
            {
                HasNoData = true;
                NoDataTitle = "Xét nghiệm định tính";
                NoDataMessage = "Xét nghiệm định tính không vẽ được biểu đồ";
                Visibility1 = Visibility.Collapsed;
                Visibility2 = Visibility.Collapsed;
                Visibility3 = Visibility.Collapsed;
                Visibility4 = Visibility.Collapsed;
                return;
            }

            // Hủy task cũ nếu đang chạy
            try
            {
                _viewChartCts?.Cancel();
            }
            catch (ObjectDisposedException) { /* ignore */ }

            try
            {
                _viewChartCts?.Dispose();
            }
            catch { /* ignore */ }

            _viewChartCts = new CancellationTokenSource();
            var token = _viewChartCts.Token;
            // reset NoData overlay before bắt đầu vẽ
            HasNoData = false;
            NoDataTitle = "Không có dữ liệu";
            NoDataMessage = "Vui lòng chọn khoảng thời gian hoặc xét nghiệm khác";


            await ViewChart(results, token);
        }

        private int GetLevelOrder(int levelKey)
        {
            // Đặt thứ tự ưu tiên: 1 (Low) -> 0, 2 (Normal) -> 1, 3 (High) -> 2, các level khác sau đó theo key tăng dần
            return levelKey switch
            {
                1 => 0,
                2 => 1,
                3 => 2,
                _ => 3 + levelKey
            };
        }

        //private async Task ViewChart(ObservableCollection<Result> results, CancellationToken token = default)
        //{
        //    Application.Current.Dispatcher.Invoke(() =>
        //    {
        //        Visibility1 = Visibility.Collapsed;
        //        Visibility2 = Visibility.Collapsed;
        //        Visibility3 = Visibility.Collapsed;
        //        Visibility4 = Visibility.Collapsed;

        //        // Replace ChartValues instances (do not Clear) to avoid LiveCharts keeping old refs
        //        ChartValues1 = new ChartValues<Result>();
        //        ChartValues2 = new ChartValues<Result>();
        //        ChartValues3 = new ChartValues<Result>();
        //        ChartValues4 = new ChartValues<Result>();

        //        // Reset SD/mean line collections too
        //        MeanValues1 = new ChartValues<double>();
        //        MeanValues2 = new ChartValues<double>();
        //        MeanValues3 = new ChartValues<double>();
        //        MeanValues4 = new ChartValues<double>();

        //        PlusOneSDValues1 = new ChartValues<double>();
        //        PlusOneSDValues2 = new ChartValues<double>();
        //        PlusOneSDValues3 = new ChartValues<double>();
        //        PlusOneSDValues4 = new ChartValues<double>();

        //        MinusOneSDValues1 = new ChartValues<double>();
        //        MinusOneSDValues2 = new ChartValues<double>();
        //        MinusOneSDValues3 = new ChartValues<double>();
        //        MinusOneSDValues4 = new ChartValues<double>();

        //        PlusTwoSDValues1 = new ChartValues<double>();
        //        PlusTwoSDValues2 = new ChartValues<double>();
        //        PlusTwoSDValues3 = new ChartValues<double>();
        //        PlusTwoSDValues4 = new ChartValues<double>();

        //        MinusTwoSDValues1 = new ChartValues<double>();
        //        MinusTwoSDValues2 = new ChartValues<double>();
        //        MinusTwoSDValues3 = new ChartValues<double>();
        //        MinusTwoSDValues4 = new ChartValues<double>();

        //        PlusThreeSDValues1 = new ChartValues<double>();
        //        PlusThreeSDValues2 = new ChartValues<double>();
        //        PlusThreeSDValues3 = new ChartValues<double>();
        //        PlusThreeSDValues4 = new ChartValues<double>();

        //        MinusThreeSDValues1 = new ChartValues<double>();
        //        MinusThreeSDValues2 = new ChartValues<double>();
        //        MinusThreeSDValues3 = new ChartValues<double>();
        //        MinusThreeSDValues4 = new ChartValues<double>();

        //        // SeriesCollection rỗng (không null) → LiveCharts render blank ngay
        //        SeriesCollection1 = new SeriesCollection();
        //        SeriesCollection2 = new SeriesCollection();
        //        SeriesCollection3 = new SeriesCollection();
        //        SeriesCollection4 = new SeriesCollection();
        //    });

        //    await Task.Yield();
        //    if (token.IsCancellationRequested) return;

        //    IsLoading = true;
        //    HasNoData = false;  // reset each time
        //    InitializeYAxisLabelFormatter();

        //    try
        //    {
        //        if (token.IsCancellationRequested) return;

        //        if (SelectedDevice == null || SelectedTest == null)
        //        {
        //            IsLoading = false;
        //            return;
        //        }

        //        if (results == null || !results.Any())
        //        {
        //            HasNoData = true;
        //            IsLoading = false;
        //            return;
        //        }

        //        HasNoData = false;
        //        var levelList = results.GroupBy(s => s.IdLevel).ToList();

        //        await Task.Run(() =>
        //        {
        //            foreach (var resultByLevel in levelList)
        //            {
        //                // Nếu user đã chọn xét nghiệm mới → hủy ngay, không render gì nữa
        //                if (token.IsCancellationRequested) return;

        //                var result = LoadChart1(resultByLevel);
        //                var chartValues = result.Item1;
        //                var visibility = result.Item2;
        //                var dates = result.Item3;
        //                var firstResult = resultByLevel.FirstOrDefault();

        //                const int TRAILING_PADDING = 3;
        //                float cmPerPoint = 2.0f;
        //                float pixelsPerPoint = CmToPixels(cmPerPoint);
        //                int numberOfPoints = chartValues.Count;
        //                float totalWidth = pixelsPerPoint * numberOfPoints;
        //                int minPointsForScreen = Math.Max((int)Math.Ceiling(ChartAreaWidth / pixelsPerPoint), numberOfPoints);
        //                int sdLineCount = minPointsForScreen + TRAILING_PADDING;
        //                float effectiveWidth = Math.Max(totalWidth, (float)ChartAreaWidth);

        //                var meanValues = new ChartValues<double>(Enumerable.Repeat(0.0, sdLineCount));
        //                var plusOneSDValues = new ChartValues<double>(Enumerable.Repeat(1.0, sdLineCount));
        //                var minusOneSDValues = new ChartValues<double>(Enumerable.Repeat(-1.0, sdLineCount));
        //                var plusTwoSDValues = new ChartValues<double>(Enumerable.Repeat(2.0, sdLineCount));
        //                var minusTwoSDValues = new ChartValues<double>(Enumerable.Repeat(-2.0, sdLineCount));
        //                var plusThreeSDValues = new ChartValues<double>(Enumerable.Repeat(3.0, sdLineCount));
        //                var minusThreeSDValues = new ChartValues<double>(Enumerable.Repeat(-3.0, sdLineCount));

        //                if (firstResult != null && firstResult.IdControlDetailNavigation != null)
        //                {
        //                    string levelName = firstResult.IdLevelNavigation?.Name ?? "";
        //                    double? mean = null;
        //                    double? sd = null;
        //                    switch (SelectedFilterOptions)
        //                    {
        //                        case "Nhà sản xuât":
        //                            mean = firstResult.IdControlDetailNavigation.MeanNsx;
        //                            sd = firstResult.IdControlDetailNavigation.SdNsx;
        //                            break;
        //                        case "Đang sử dụng":
        //                            mean = firstResult.IdControlDetailNavigation.CurMean;
        //                            sd = firstResult.IdControlDetailNavigation.CurSd;
        //                            break;
        //                        case "Thống kê":
        //                            mean = firstResult.IdControlDetailNavigation.MeanApp;
        //                            sd = firstResult.IdControlDetailNavigation.SdApp;
        //                            break;
        //                    }

        //                    var boundaryIndices = new List<int>();
        //                    double tol = 1e-6;
        //                    if (chartValues.Count > 0)
        //                    {
        //                        int prevControlId = chartValues[0].IdControlDetail ?? -1;
        //                        double? prevAppliedMean = chartValues[0].AppliedMean;
        //                        double? prevAppliedSd = chartValues[0].AppliedSd;
        //                        for (int i = 1; i < chartValues.Count; i++)
        //                        {
        //                            if (token.IsCancellationRequested) return;

        //                            var cur = chartValues[i];
        //                            int curControlId = cur.IdControlDetail ?? -1;
        //                            double? curAppliedMean = cur.AppliedMean;
        //                            double? curAppliedSd = cur.AppliedSd;

        //                            bool controlChanged = curControlId != prevControlId;

        //                            bool appliedMeanChanged = false;
        //                            if (prevAppliedMean.HasValue != curAppliedMean.HasValue)
        //                            {
        //                                appliedMeanChanged = true;
        //                            }
        //                            else if (prevAppliedMean.HasValue && curAppliedMean.HasValue)
        //                            {
        //                                if (Math.Abs(prevAppliedMean.Value - curAppliedMean.Value) > tol) appliedMeanChanged = true;
        //                            }

        //                            bool appliedSdChanged = false;
        //                            if (prevAppliedSd.HasValue != curAppliedSd.HasValue)
        //                            {
        //                                appliedSdChanged = true;
        //                            }
        //                            else if (prevAppliedSd.HasValue && curAppliedSd.HasValue)
        //                            {
        //                                if (Math.Abs(prevAppliedSd.Value - curAppliedSd.Value) > tol) appliedSdChanged = true;
        //                            }

        //                            if (controlChanged || appliedMeanChanged || appliedSdChanged)
        //                            {
        //                                boundaryIndices.Add(i);
        //                                prevControlId = curControlId;
        //                                prevAppliedMean = curAppliedMean;
        //                                prevAppliedSd = curAppliedSd;
        //                            }
        //                        }
        //                    }

        //                    var separators = boundaryIndices.Where(idx => idx > 0).Select(idx => idx - 0.5).ToList();

        //                    // Kiểm tra lần cuối trước khi ghi lên UI
        //                    if (token.IsCancellationRequested) return;

        //                    Application.Current.Dispatcher.Invoke(() =>
        //                    {
        //                        // Nếu bị cancel trong lúc chờ Dispatcher → bỏ qua
        //                        if (token.IsCancellationRequested) return;

        //                        switch (resultByLevel.Key)
        //                        {
        //                            case 1:
        //                            case 4:
        //                            case 7:
        //                                LevelName1 = levelName;
        //                                Mean1 = mean;
        //                                SD1 = sd;
        //                                Range1 = $"{(mean - 2 * sd):F2}  -  {(mean + 2 * sd):F2}";
        //                                TotalPoints1 = $"{chartValues.Count}";
        //                                MeanValues1 = meanValues;
        //                                PlusOneSDValues1 = plusOneSDValues;
        //                                MinusOneSDValues1 = minusOneSDValues;
        //                                PlusTwoSDValues1 = plusTwoSDValues;
        //                                MinusTwoSDValues1 = minusTwoSDValues;
        //                                PlusThreeSDValues1 = plusThreeSDValues;
        //                                MinusThreeSDValues1 = minusThreeSDValues;
        //                                ChartValues1 = chartValues;
        //                                Visibility1 = visibility;
        //                                Dates1 = dates;
        //                                totalWidth1 = effectiveWidth;

        //                                var sc1 = new SeriesCollection
        //                                {
        //                                    new LineSeries { Title="3SD", Values = PlusThreeSDValues1, Stroke = Brushes.Red, Fill = Brushes.Transparent, PointGeometry = null, StrokeThickness = 3 },
        //                                    new LineSeries { Title="+2SD", Values = PlusTwoSDValues1, Stroke = Brushes.Orange, Fill = Brushes.Transparent, PointGeometry = null, StrokeThickness = 3, IsHitTestVisible = false },
        //                                    new LineSeries { Title="1SD", Values = PlusOneSDValues1, Stroke = Brushes.Green, Fill = new SolidColorBrush(Color.FromArgb(0xFF,0xC4,0xEE,0xB4)), PointGeometry = null, StrokeThickness = 3, IsHitTestVisible = false },
        //                                    new LineSeries { Title="Mean", Values = MeanValues1, Stroke = Brushes.Green, Fill = Brushes.Transparent, PointGeometry = null, StrokeThickness = 3, IsHitTestVisible = false },
        //                                    new LineSeries { Title="-1SD", Values = MinusOneSDValues1, Stroke = Brushes.Green, Fill = Brushes.White, PointGeometry = null, StrokeThickness = 3, IsHitTestVisible = false },
        //                                    new LineSeries { Title="-2SD", Values = MinusTwoSDValues1, Stroke = Brushes.Orange, Fill = Brushes.Transparent, PointGeometry = null, StrokeThickness = 3 },
        //                                    new LineSeries { Title="-3SD", Values = MinusThreeSDValues1, Stroke = Brushes.Red, Fill = Brushes.Transparent, PointGeometry = null, StrokeThickness = 3 },
        //                                    new LineSeries { Title="result", Values = ChartValues1, Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1C8FC5")), Fill = Brushes.Transparent, LineSmoothness = 0, PointGeometrySize = 15, PointForeground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#222E31")), StrokeThickness = 4 }
        //                                };

        //                                foreach (var sx in separators)
        //                                {
        //                                    sc1.Add(new LineSeries
        //                                    {
        //                                        Title = "sep",
        //                                        Values = new ChartValues<ObservablePoint> { new ObservablePoint(sx, 4), new ObservablePoint(sx, -4) },
        //                                        Stroke = Brushes.Black,
        //                                        StrokeDashArray = new DoubleCollection { 4, 2 },
        //                                        StrokeThickness = 2,
        //                                        PointGeometry = null,
        //                                        Fill = Brushes.Transparent,
        //                                        IsHitTestVisible = false
        //                                    });
        //                                }

        //                                SeriesCollection1 = sc1;
        //                                break;
        //                            case 2:
        //                            case 5:
        //                            case 8:
        //                            case 9:
        //                                LevelName2 = levelName;
        //                                Mean2 = mean;
        //                                SD2 = sd;
        //                                Range2 = $"{(mean - 2 * sd):F2} - {(mean + 2 * sd):F2}";
        //                                TotalPoints2 = $"{chartValues.Count}";
        //                                MeanValues2 = meanValues;
        //                                PlusOneSDValues2 = plusOneSDValues;
        //                                MinusOneSDValues2 = minusOneSDValues;
        //                                PlusTwoSDValues2 = plusTwoSDValues;
        //                                MinusTwoSDValues2 = minusTwoSDValues;
        //                                PlusThreeSDValues2 = plusThreeSDValues;
        //                                MinusThreeSDValues2 = minusThreeSDValues;
        //                                ChartValues2 = chartValues;
        //                                Visibility2 = visibility;
        //                                Dates2 = dates;
        //                                totalWidth2 = effectiveWidth;

        //                                var sc2 = new SeriesCollection
        //                            {
        //                                new LineSeries { Title="3SD", Values = PlusThreeSDValues2, Stroke = Brushes.Red, Fill = Brushes.Transparent, PointGeometry = null, StrokeThickness = 3 },
        //                                new LineSeries { Title="+2SD", Values = PlusTwoSDValues2, Stroke = Brushes.Orange, Fill = Brushes.Transparent, PointGeometry = null, StrokeThickness = 3, IsHitTestVisible = false },
        //                                new LineSeries { Title="1SD", Values = PlusOneSDValues2, Stroke = Brushes.Green, Fill = new SolidColorBrush(Color.FromArgb(0xFF,0xC4,0xEE,0xB4)), PointGeometry = null, StrokeThickness = 3, IsHitTestVisible = false },
        //                                new LineSeries { Title="Mean", Values = MeanValues2, Stroke = Brushes.Green, Fill = Brushes.Transparent, PointGeometry = null, StrokeThickness = 3, IsHitTestVisible = false },
        //                                new LineSeries { Title="-1SD", Values = MinusOneSDValues2, Stroke = Brushes.Green, Fill = Brushes.White, PointGeometry = null, StrokeThickness = 3, IsHitTestVisible = false },
        //                                new LineSeries { Title="-2SD", Values = MinusTwoSDValues2, Stroke = Brushes.Orange, Fill = Brushes.Transparent, PointGeometry = null, StrokeThickness = 3 },
        //                                new LineSeries { Title="-3SD", Values = MinusThreeSDValues2, Stroke = Brushes.Red, Fill = Brushes.Transparent, PointGeometry = null, StrokeThickness = 3 },
        //                                new LineSeries { Title="result", Values = ChartValues2, Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1C8FC5")), Fill = Brushes.Transparent, LineSmoothness = 0, PointGeometrySize = 15, PointForeground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#222E31")), StrokeThickness = 4 }
        //                            };
        //                                foreach (var sx in separators)
        //                                {
        //                                    sc2.Add(new LineSeries
        //                                    {
        //                                        Title = "sep",
        //                                        Values = new ChartValues<ObservablePoint> { new ObservablePoint(sx, 4), new ObservablePoint(sx, -4) },
        //                                        Stroke = Brushes.Black,
        //                                        StrokeDashArray = new DoubleCollection { 4, 2 },
        //                                        StrokeThickness = 2,
        //                                        PointGeometry = null,
        //                                        Fill = Brushes.Transparent,
        //                                        IsHitTestVisible = false
        //                                    });
        //                                }
        //                                SeriesCollection2 = sc2;
        //                                break;
        //                            case 3:
        //                            case 6:
        //                            case 10:
        //                                LevelName3 = levelName;
        //                                Mean3 = mean;
        //                                SD3 = sd;
        //                                Range3 = $"{(mean - 2 * sd):F2} - {(mean + 2 * sd):F2}";
        //                                TotalPoints3 = $"{chartValues.Count}";
        //                                MeanValues3 = meanValues;
        //                                PlusOneSDValues3 = plusOneSDValues;
        //                                MinusOneSDValues3 = minusOneSDValues;
        //                                PlusTwoSDValues3 = plusTwoSDValues;
        //                                MinusTwoSDValues3 = minusTwoSDValues;
        //                                PlusThreeSDValues3 = plusThreeSDValues;
        //                                MinusThreeSDValues3 = minusThreeSDValues;
        //                                ChartValues3 = chartValues;
        //                                Visibility3 = visibility;
        //                                Dates3 = dates;
        //                                totalWidth3 = effectiveWidth;

        //                                var sc3 = new SeriesCollection
        //                            {
        //                                new LineSeries { Title="3SD", Values = PlusThreeSDValues3, Stroke = Brushes.Red, Fill = Brushes.Transparent, PointGeometry = null, StrokeThickness = 3 },
        //                                new LineSeries { Title="+2SD", Values = PlusTwoSDValues3, Stroke = Brushes.Orange, Fill = Brushes.Transparent, PointGeometry = null, StrokeThickness = 3, IsHitTestVisible = false },
        //                                new LineSeries { Title="1SD", Values = PlusOneSDValues3, Stroke = Brushes.Green, Fill = new SolidColorBrush(Color.FromArgb(0xFF,0xC4,0xEE,0xB4)), PointGeometry = null, StrokeThickness = 3, IsHitTestVisible = false },
        //                                new LineSeries { Title="Mean", Values = MeanValues3, Stroke = Brushes.Green, Fill = Brushes.Transparent, PointGeometry = null, StrokeThickness = 3, IsHitTestVisible = false },
        //                                new LineSeries { Title="-1SD", Values = MinusOneSDValues3, Stroke = Brushes.Green, Fill = Brushes.White, PointGeometry = null, StrokeThickness = 3, IsHitTestVisible = false },
        //                                new LineSeries { Title="-2SD", Values = MinusTwoSDValues3, Stroke = Brushes.Orange, Fill = Brushes.Transparent, PointGeometry = null, StrokeThickness = 3 },
        //                                new LineSeries { Title="-3SD", Values = MinusThreeSDValues3, Stroke = Brushes.Red, Fill = Brushes.Transparent, PointGeometry = null, StrokeThickness = 3 },
        //                                new LineSeries { Title="result", Values = ChartValues3, Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1C8FC5")), Fill = Brushes.Transparent, LineSmoothness = 0, PointGeometrySize = 15, PointForeground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#222E31")), StrokeThickness = 4 }
        //                            };
        //                                foreach (var sx in separators)
        //                                {
        //                                    sc3.Add(new LineSeries
        //                                    {
        //                                        Title = "sep",
        //                                        Values = new ChartValues<ObservablePoint> { new ObservablePoint(sx, 4), new ObservablePoint(sx, -4) },
        //                                        Stroke = Brushes.Black,
        //                                        StrokeDashArray = new DoubleCollection { 4, 2 },
        //                                        StrokeThickness = 2,
        //                                        PointGeometry = null,
        //                                        Fill = Brushes.Transparent,
        //                                        IsHitTestVisible = false
        //                                    });
        //                                }
        //                                SeriesCollection3 = sc3;
        //                                break;
        //                            case 11:
        //                                LevelName4 = levelName;
        //                                Mean4 = mean;
        //                                SD4 = sd;
        //                                Range4 = $"{(mean - 2 * sd):F2} - {(mean + 2 * sd):F2}";
        //                                TotalPoints4 = $"{chartValues.Count}";
        //                                MeanValues4 = meanValues;
        //                                PlusOneSDValues4 = plusOneSDValues;
        //                                MinusOneSDValues4 = minusOneSDValues;
        //                                PlusTwoSDValues4 = plusTwoSDValues;
        //                                MinusTwoSDValues4 = minusTwoSDValues;
        //                                PlusThreeSDValues4 = plusThreeSDValues;
        //                                MinusThreeSDValues4 = minusThreeSDValues;
        //                                ChartValues4 = chartValues;
        //                                Visibility4 = visibility;
        //                                Dates4 = dates;
        //                                totalWidth4 = effectiveWidth;

        //                                var sc4 = new SeriesCollection
        //                            {
        //                                new LineSeries { Title="3SD", Values = PlusThreeSDValues4, Stroke = Brushes.Red, Fill = Brushes.Transparent, PointGeometry = null, StrokeThickness = 3 },
        //                                new LineSeries { Title="+2SD", Values = PlusTwoSDValues4, Stroke = Brushes.Orange, Fill = Brushes.Transparent, PointGeometry = null, StrokeThickness = 3, IsHitTestVisible = false },
        //                                new LineSeries { Title="1SD", Values = PlusOneSDValues4, Stroke = Brushes.Green, Fill = new SolidColorBrush(Color.FromArgb(0xFF,0xC4,0xEE,0xB4)), PointGeometry = null, StrokeThickness = 3, IsHitTestVisible = false },
        //                                new LineSeries { Title="Mean", Values = MeanValues4, Stroke = Brushes.Green, Fill = Brushes.Transparent, PointGeometry = null, StrokeThickness = 3, IsHitTestVisible = false },
        //                                new LineSeries { Title="-1SD", Values = MinusOneSDValues4, Stroke = Brushes.Green, Fill = Brushes.White, PointGeometry = null, StrokeThickness = 3, IsHitTestVisible = false },
        //                                new LineSeries { Title="-2SD", Values = MinusTwoSDValues4, Stroke = Brushes.Orange, Fill = Brushes.Transparent, PointGeometry = null, StrokeThickness = 3 },
        //                                new LineSeries { Title="-3SD", Values = MinusThreeSDValues4, Stroke = Brushes.Red, Fill = Brushes.Transparent, PointGeometry = null, StrokeThickness = 3 },
        //                                new LineSeries { Title="result", Values = ChartValues4, Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1C8FC5")), Fill = Brushes.Transparent, LineSmoothness = 0, PointGeometrySize = 15, PointForeground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#222E31")), StrokeThickness = 4 }
        //                            };
        //                                foreach (var sx in separators)
        //                                {
        //                                    sc4.Add(new LineSeries
        //                                    {
        //                                        Title = "sep",
        //                                        Values = new ChartValues<ObservablePoint> { new ObservablePoint(sx, 4), new ObservablePoint(sx, -4) },
        //                                        Stroke = Brushes.Black,
        //                                        StrokeDashArray = new DoubleCollection { 4, 2 },
        //                                        StrokeThickness = 2,
        //                                        PointGeometry = null,
        //                                        Fill = Brushes.Transparent,
        //                                        IsHitTestVisible = false
        //                                    });
        //                                }
        //                                SeriesCollection4 = sc4;
        //                                break;
        //                        }
        //                    });
        //                }
        //            }
        //        });

        //        await LoadChartAsync();
        //    }
        //    catch (Exception ex)
        //    {
        //        // Nếu có lỗi không quan trọng, hiển thị nhưng không crash app
        //        MessageBox.Show($"Lỗi khi tải biểu đồ: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        //    }
        //    finally
        //    {
        //        IsLoading = false;
        //    }
        //}

        private async Task ViewChart(ObservableCollection<Result> results, CancellationToken token = default)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Visibility1 = Visibility.Collapsed;
                Visibility2 = Visibility.Collapsed;
                Visibility3 = Visibility.Collapsed;
                Visibility4 = Visibility.Collapsed;

                // Replace ChartValues instances (do not Clear) to avoid LiveCharts keeping old refs
                ChartValues1 = new ChartValues<Result>();
                ChartValues2 = new ChartValues<Result>();
                ChartValues3 = new ChartValues<Result>();
                ChartValues4 = new ChartValues<Result>();

                // Reset SD/mean line collections too
                MeanValues1 = new ChartValues<double>();
                MeanValues2 = new ChartValues<double>();
                MeanValues3 = new ChartValues<double>();
                MeanValues4 = new ChartValues<double>();

                PlusOneSDValues1 = new ChartValues<double>();
                PlusOneSDValues2 = new ChartValues<double>();
                PlusOneSDValues3 = new ChartValues<double>();
                PlusOneSDValues4 = new ChartValues<double>();

                MinusOneSDValues1 = new ChartValues<double>();
                MinusOneSDValues2 = new ChartValues<double>();
                MinusOneSDValues3 = new ChartValues<double>();
                MinusOneSDValues4 = new ChartValues<double>();

                PlusTwoSDValues1 = new ChartValues<double>();
                PlusTwoSDValues2 = new ChartValues<double>();
                PlusTwoSDValues3 = new ChartValues<double>();
                PlusTwoSDValues4 = new ChartValues<double>();

                MinusTwoSDValues1 = new ChartValues<double>();
                MinusTwoSDValues2 = new ChartValues<double>();
                MinusTwoSDValues3 = new ChartValues<double>();
                MinusTwoSDValues4 = new ChartValues<double>();

                PlusThreeSDValues1 = new ChartValues<double>();
                PlusThreeSDValues2 = new ChartValues<double>();
                PlusThreeSDValues3 = new ChartValues<double>();
                PlusThreeSDValues4 = new ChartValues<double>();

                MinusThreeSDValues1 = new ChartValues<double>();
                MinusThreeSDValues2 = new ChartValues<double>();
                MinusThreeSDValues3 = new ChartValues<double>();
                MinusThreeSDValues4 = new ChartValues<double>();

                // SeriesCollection rỗng (không null) → LiveCharts render blank ngay
                SeriesCollection1 = new SeriesCollection();
                SeriesCollection2 = new SeriesCollection();
                SeriesCollection3 = new SeriesCollection();
                SeriesCollection4 = new SeriesCollection();
            });

            await Task.Yield();
            if (token.IsCancellationRequested) return;

            IsLoading = true;
            HasNoData = false;  // reset each time
            InitializeYAxisLabelFormatter();

            try
            {
                if (token.IsCancellationRequested) return;

                if (SelectedDevice == null || SelectedTest == null)
                {
                    IsLoading = false;
                    return;
                }

                if (results == null || !results.Any())
                {
                    HasNoData = true;
                    IsLoading = false;
                    return;
                }

                HasNoData = false;
                // Nhóm theo level và sắp xếp theo thứ tự mong muốn (Low, Normal, High, ... )
                var levelGroups = results.GroupBy(s => s.IdLevel).ToList();
                var orderedGroups = levelGroups
                    .OrderBy(g => GetLevelOrder(g.Key))
                    .ToList();

                await Task.Run(() =>
                {
                    // Duyệt theo slot: 0 -> chart1, 1 -> chart2, 2 -> chart3, 3 -> chart4
                    for (int slot = 0; slot < orderedGroups.Count && slot < 4; slot++)
                    {
                        if (token.IsCancellationRequested) return;

                        var resultByLevel = orderedGroups[slot];
                        var result = LoadChart1(resultByLevel);
                        var chartValues = result.Item1;
                        var visibility = result.Item2;
                        var dates = result.Item3;
                        var firstResult = resultByLevel.FirstOrDefault();

                        const int TRAILING_PADDING = 1;
                        float cmPerPoint = 2.0f;
                        float pixelsPerPoint = CmToPixels(cmPerPoint);
                        int numberOfPoints = chartValues.Count;
                        float totalWidth = pixelsPerPoint * numberOfPoints;
                        int minPointsForScreen = Math.Max((int)Math.Ceiling(ChartAreaWidth / pixelsPerPoint), numberOfPoints);
                        int sdLineCount = minPointsForScreen + TRAILING_PADDING;
                        float effectiveWidth = Math.Max(totalWidth, (float)ChartAreaWidth);

                        var meanValues = new ChartValues<double>(Enumerable.Repeat(0.0, sdLineCount));
                        var plusOneSDValues = new ChartValues<double>(Enumerable.Repeat(1.0, sdLineCount));
                        var minusOneSDValues = new ChartValues<double>(Enumerable.Repeat(-1.0, sdLineCount));
                        var plusTwoSDValues = new ChartValues<double>(Enumerable.Repeat(2.0, sdLineCount));
                        var minusTwoSDValues = new ChartValues<double>(Enumerable.Repeat(-2.0, sdLineCount));
                        var plusThreeSDValues = new ChartValues<double>(Enumerable.Repeat(3.0, sdLineCount));
                        var minusThreeSDValues = new ChartValues<double>(Enumerable.Repeat(-3.0, sdLineCount));

                        if (firstResult == null || firstResult.IdControlDetailNavigation == null)
                        {
                            continue;
                        }

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

                        var boundaryIndices = new List<int>();
                        double tol = 1e-6;
                        if (chartValues.Count > 0)
                        {
                            int prevControlId = chartValues[0].IdControlDetail ?? -1;
                            double? prevAppliedMean = chartValues[0].AppliedMean;
                            double? prevAppliedSd = chartValues[0].AppliedSd;
                            for (int i = 1; i < chartValues.Count; i++)
                            {
                                if (token.IsCancellationRequested) return;

                                var cur = chartValues[i];
                                int curControlId = cur.IdControlDetail ?? -1;
                                double? curAppliedMean = cur.AppliedMean;
                                double? curAppliedSd = cur.AppliedSd;

                                bool controlChanged = curControlId != prevControlId;

                                bool appliedMeanChanged = false;
                                if (prevAppliedMean.HasValue != curAppliedMean.HasValue)
                                {
                                    appliedMeanChanged = true;
                                }
                                else if (prevAppliedMean.HasValue && curAppliedMean.HasValue)
                                {
                                    if (Math.Abs(prevAppliedMean.Value - curAppliedMean.Value) > tol) appliedMeanChanged = true;
                                }

                                bool appliedSdChanged = false;
                                if (prevAppliedSd.HasValue != curAppliedSd.HasValue)
                                {
                                    appliedSdChanged = true;
                                }
                                else if (prevAppliedSd.HasValue && curAppliedSd.HasValue)
                                {
                                    if (Math.Abs(prevAppliedSd.Value - curAppliedSd.Value) > tol) appliedSdChanged = true;
                                }

                                if (controlChanged || appliedMeanChanged || appliedSdChanged)
                                {
                                    boundaryIndices.Add(i);
                                    prevControlId = curControlId;
                                    prevAppliedMean = curAppliedMean;
                                    prevAppliedSd = curAppliedSd;
                                }
                            }
                        }

                        var separators = boundaryIndices.Where(idx => idx > 0).Select(idx => idx - 0.5).ToList();

                        // Kiểm tra lần cuối trước khi ghi lên UI
                        if (token.IsCancellationRequested) return;

                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            // Nếu bị cancel trong lúc chờ Dispatcher → bỏ qua
                            if (token.IsCancellationRequested) return;

                            // Gán theo slot cố định (slot 0->chart1, 1->chart2, 2->chart3, 3->chart4)
                            switch (slot)
                            {
                                case 0:
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
                                    totalWidth1 = effectiveWidth;

                                    var sc1 = new SeriesCollection
                            {
                                new LineSeries { Title="3SD", Values = PlusThreeSDValues1, Stroke = Brushes.Red, Fill = Brushes.Transparent, PointGeometry = null, StrokeThickness = 3 },
                                new LineSeries { Title="+2SD", Values = PlusTwoSDValues1, Stroke = Brushes.Orange, Fill = Brushes.Transparent, PointGeometry = null, StrokeThickness = 3, IsHitTestVisible = false },
                                new LineSeries { Title="1SD", Values = PlusOneSDValues1, Stroke = Brushes.Green, Fill = new SolidColorBrush(Color.FromArgb(0xFF,0xC4,0xEE,0xB4)), PointGeometry = null, StrokeThickness = 3, IsHitTestVisible = false },
                                new LineSeries { Title="Mean", Values = MeanValues1, Stroke = Brushes.Green, Fill = Brushes.Transparent, PointGeometry = null, StrokeThickness = 3, IsHitTestVisible = false },
                                new LineSeries { Title="-1SD", Values = MinusOneSDValues1, Stroke = Brushes.Green, Fill = Brushes.White, PointGeometry = null, StrokeThickness = 3, IsHitTestVisible = false },
                                new LineSeries { Title="-2SD", Values = MinusTwoSDValues1, Stroke = Brushes.Orange, Fill = Brushes.Transparent, PointGeometry = null, StrokeThickness = 3 },
                                new LineSeries { Title="-3SD", Values = MinusThreeSDValues1, Stroke = Brushes.Red, Fill = Brushes.Transparent, PointGeometry = null, StrokeThickness = 3 },
                                new LineSeries { Title="result", Values = ChartValues1, Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1C8FC5")), Fill = Brushes.Transparent, LineSmoothness = 0, PointGeometrySize = 15, PointForeground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#222E31")), StrokeThickness = 4 }
                            };

                                    foreach (var sx in separators)
                                    {
                                        sc1.Add(new LineSeries
                                        {
                                            Title = "sep",
                                            Values = new ChartValues<ObservablePoint> { new ObservablePoint(sx, 4), new ObservablePoint(sx, -4) },
                                            Stroke = Brushes.Black,
                                            StrokeDashArray = new DoubleCollection { 4, 2 },
                                            StrokeThickness = 2,
                                            PointGeometry = null,
                                            Fill = Brushes.Transparent,
                                            IsHitTestVisible = false
                                        });
                                    }

                                    SeriesCollection1 = sc1;
                                    break;

                                case 1:
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
                                    totalWidth2 = effectiveWidth;

                                    var sc2 = new SeriesCollection
                            {
                                new LineSeries { Title="3SD", Values = PlusThreeSDValues2, Stroke = Brushes.Red, Fill = Brushes.Transparent, PointGeometry = null, StrokeThickness = 3 },
                                new LineSeries { Title="+2SD", Values = PlusTwoSDValues2, Stroke = Brushes.Orange, Fill = Brushes.Transparent, PointGeometry = null, StrokeThickness = 3, IsHitTestVisible = false },
                                new LineSeries { Title="1SD", Values = PlusOneSDValues2, Stroke = Brushes.Green, Fill = new SolidColorBrush(Color.FromArgb(0xFF,0xC4,0xEE,0xB4)), PointGeometry = null, StrokeThickness = 3, IsHitTestVisible = false },
                                new LineSeries { Title="Mean", Values = MeanValues2, Stroke = Brushes.Green, Fill = Brushes.Transparent, PointGeometry = null, StrokeThickness = 3, IsHitTestVisible = false },
                                new LineSeries { Title="-1SD", Values = MinusOneSDValues2, Stroke = Brushes.Green, Fill = Brushes.White, PointGeometry = null, StrokeThickness = 3, IsHitTestVisible = false },
                                new LineSeries { Title="-2SD", Values = MinusTwoSDValues2, Stroke = Brushes.Orange, Fill = Brushes.Transparent, PointGeometry = null, StrokeThickness = 3 },
                                new LineSeries { Title="-3SD", Values = MinusThreeSDValues2, Stroke = Brushes.Red, Fill = Brushes.Transparent, PointGeometry = null, StrokeThickness = 3 },
                                new LineSeries { Title="result", Values = ChartValues2, Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1C8FC5")), Fill = Brushes.Transparent, LineSmoothness = 0, PointGeometrySize = 15, PointForeground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#222E31")), StrokeThickness = 4 }
                            };
                                    foreach (var sx in separators)
                                    {
                                        sc2.Add(new LineSeries
                                        {
                                            Title = "sep",
                                            Values = new ChartValues<ObservablePoint> { new ObservablePoint(sx, 4), new ObservablePoint(sx, -4) },
                                            Stroke = Brushes.Black,
                                            StrokeDashArray = new DoubleCollection { 4, 2 },
                                            StrokeThickness = 2,
                                            PointGeometry = null,
                                            Fill = Brushes.Transparent,
                                            IsHitTestVisible = false
                                        });
                                    }
                                    SeriesCollection2 = sc2;
                                    break;

                                case 2:
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
                                    totalWidth3 = effectiveWidth;

                                    var sc3 = new SeriesCollection
                            {
                                new LineSeries { Title="3SD", Values = PlusThreeSDValues3, Stroke = Brushes.Red, Fill = Brushes.Transparent, PointGeometry = null, StrokeThickness = 3 },
                                new LineSeries { Title="+2SD", Values = PlusTwoSDValues3, Stroke = Brushes.Orange, Fill = Brushes.Transparent, PointGeometry = null, StrokeThickness = 3, IsHitTestVisible = false },
                                new LineSeries { Title="1SD", Values = PlusOneSDValues3, Stroke = Brushes.Green, Fill = new SolidColorBrush(Color.FromArgb(0xFF,0xC4,0xEE,0xB4)), PointGeometry = null, StrokeThickness = 3, IsHitTestVisible = false },
                                new LineSeries { Title="Mean", Values = MeanValues3, Stroke = Brushes.Green, Fill = Brushes.Transparent, PointGeometry = null, StrokeThickness = 3, IsHitTestVisible = false },
                                new LineSeries { Title="-1SD", Values = MinusOneSDValues3, Stroke = Brushes.Green, Fill = Brushes.White, PointGeometry = null, StrokeThickness = 3, IsHitTestVisible = false },
                                new LineSeries { Title="-2SD", Values = MinusTwoSDValues3, Stroke = Brushes.Orange, Fill = Brushes.Transparent, PointGeometry = null, StrokeThickness = 3 },
                                new LineSeries { Title="-3SD", Values = MinusThreeSDValues3, Stroke = Brushes.Red, Fill = Brushes.Transparent, PointGeometry = null, StrokeThickness = 3 },
                                new LineSeries { Title="result", Values = ChartValues3, Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1C8FC5")), Fill = Brushes.Transparent, LineSmoothness = 0, PointGeometrySize = 15, PointForeground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#222E31")), StrokeThickness = 4 }
                            };
                                    foreach (var sx in separators)
                                    {
                                        sc3.Add(new LineSeries
                                        {
                                            Title = "sep",
                                            Values = new ChartValues<ObservablePoint> { new ObservablePoint(sx, 4), new ObservablePoint(sx, -4) },
                                            Stroke = Brushes.Black,
                                            StrokeDashArray = new DoubleCollection { 4, 2 },
                                            StrokeThickness = 2,
                                            PointGeometry = null,
                                            Fill = Brushes.Transparent,
                                            IsHitTestVisible = false
                                        });
                                    }
                                    SeriesCollection3 = sc3;
                                    break;

                                case 3:
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
                                    totalWidth4 = effectiveWidth;

                                    var sc4 = new SeriesCollection
                            {
                                new LineSeries { Title="3SD", Values = PlusThreeSDValues4, Stroke = Brushes.Red, Fill = Brushes.Transparent, PointGeometry = null, StrokeThickness = 3 },
                                new LineSeries { Title="+2SD", Values = PlusTwoSDValues4, Stroke = Brushes.Orange, Fill = Brushes.Transparent, PointGeometry = null, StrokeThickness = 3, IsHitTestVisible = false },
                                new LineSeries { Title="1SD", Values = PlusOneSDValues4, Stroke = Brushes.Green, Fill = new SolidColorBrush(Color.FromArgb(0xFF,0xC4,0xEE,0xB4)), PointGeometry = null, StrokeThickness = 3, IsHitTestVisible = false },
                                new LineSeries { Title="Mean", Values = MeanValues4, Stroke = Brushes.Green, Fill = Brushes.Transparent, PointGeometry = null, StrokeThickness = 3, IsHitTestVisible = false },
                                new LineSeries { Title="-1SD", Values = MinusOneSDValues4, Stroke = Brushes.Green, Fill = Brushes.White, PointGeometry = null, StrokeThickness = 3, IsHitTestVisible = false },
                                new LineSeries { Title="-2SD", Values = MinusTwoSDValues4, Stroke = Brushes.Orange, Fill = Brushes.Transparent, PointGeometry = null, StrokeThickness = 3 },
                                new LineSeries { Title="-3SD", Values = MinusThreeSDValues4, Stroke = Brushes.Red, Fill = Brushes.Transparent, PointGeometry = null, StrokeThickness = 3 },
                                new LineSeries { Title="result", Values = ChartValues4, Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1C8FC5")), Fill = Brushes.Transparent, LineSmoothness = 0, PointGeometrySize = 15, PointForeground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#222E31")), StrokeThickness = 4 }
                            };
                                    foreach (var sx in separators)
                                    {
                                        sc4.Add(new LineSeries
                                        {
                                            Title = "sep",
                                            Values = new ChartValues<ObservablePoint> { new ObservablePoint(sx, 4), new ObservablePoint(sx, -4) },
                                            Stroke = Brushes.Black,
                                            StrokeDashArray = new DoubleCollection { 4, 2 },
                                            StrokeThickness = 2,
                                            PointGeometry = null,
                                            Fill = Brushes.Transparent,
                                            IsHitTestVisible = false
                                        });
                                    }
                                    SeriesCollection4 = sc4;
                                    break;
                            }
                        });
                    }
                });

                await LoadChartAsync();
            }
            catch (Exception ex)
            {
                // Nếu có lỗi không quan trọng, hiển thị nhưng không crash app
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
            // Keep mapper for Result registered (already done in ViewChart). No additional work required here.
            await Task.CompletedTask;
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