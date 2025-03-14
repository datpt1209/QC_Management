using LiveCharts;
using LiveCharts.Configurations;
using LiveCharts.Defaults;
using LiveCharts.Definitions.Charts;
using LiveCharts.Wpf;
using MaterialDesignThemes.Wpf.Converters;
using Microsoft.EntityFrameworkCore;
using QC_Management.Models;
using QC_Management.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using XAct;
using XAct.UI.Views;

namespace QC_Management.ViewModels
{
    public class HomeViewModel_V2 : BaseViewModel
    {
        private readonly QcManagmentContext _dbContext;
        private ObservableCollection<Result> _List;
        private Func<double, string> _yAxisLabelFormatter;
        private ObservableCollection<Device> _DeviceList;
        private ObservableCollection<ControlInfo> _ControlInfoList;
        private ObservableCollection<ControlInfoDetail> _ControlInfoDetailList;
        private ObservableCollection<Test> _TestListDB;
        private ObservableCollection<User> _UserList;
        private ObservableCollection<UnitTable> _UnitList;
        private ObservableCollection<Test> _TestList;
        private ObservableCollection<DeviceTest> _DeviceTestList;
        private ObservableCollection<LevelQc> _LevelList;
        private ObservableCollection<string> _Dates3;
        private ObservableCollection<string> _Dates1;
        private ObservableCollection<string> _Dates;
        private ObservableCollection<string> _Dates2;
        private Visibility _Visibility1;
        private Visibility _Visibility2;
        private Visibility _Visibility3;
        private bool _isLoading;
        private ChartValues<Result> _ChartValues1;
        private ChartValues<Result> _ChartValues2;
        private ChartValues<Result> _ChartValues3;
        private ChartValues<double> _MeanValues1;
        private ChartValues<double> _MeanValues2;
        private ChartValues<double> _MeanValues3;
        private ChartValues<double> _PlusOneSDValues1;
        private ChartValues<double> _PlusOneSDValues2;
        private ChartValues<double> _PlusOneSDValues3;
        private ChartValues<double> _MinusOneSDValues1;
        private ChartValues<double> _MinusOneSDValues2;
        private ChartValues<double> _MinusOneSDValues3;
        private ChartValues<double> _PlusTwoSDValues2;
        private ChartValues<double> _PlusTwoSDValues1;
        private ChartValues<double> _PlusTwoSDValues3;
        private ChartValues<double> _MinusTwoSDValues1;
        private ChartValues<double> _MinusTwoSDValues2;
        private ChartValues<double> _MinusTwoSDValues3;
        private ChartValues<double> _PlusThreeSDValues1;
        private ChartValues<double> _PlusThreeSDValues2;
        private ChartValues<double> _PlusThreeSDValues3;
        private ChartValues<double> _MinusThreeSDValues3;
        private ChartValues<double> _MinusThreeSDValues2;
        private ChartValues<double> _MinusThreeSDValues1;
        private float _totalWidth1;
        private float _totalWidth2;
        private float _totalWidth3;
        private string _DisplayName;
        private DateTime _StartDate = DateTime.Now.AddDays(-14);
        private DateTime _EndDate = DateTime.Now;
        private string _LOT;
        private bool _isCheck;
        private Test _SelectedTest;
        private Device _SelectedDevice;
        private ControlInfo _SelectedControlInfo;
        private ControlInfoDetail _SelectedControlInfoDetail;

        public ObservableCollection<Result> List { get => _List; set { _List = value; OnPropertyChanged(); } }
        public ChartValues<ObservablePoint> LineAtOneValues { get; set; }
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

        public ObservableCollection<ControlInfo> ControlInfoList { get => _ControlInfoList; set { _ControlInfoList = value; OnPropertyChanged(); } }
        public ObservableCollection<ControlInfoDetail> ControlInfoDetailList { get => _ControlInfoDetailList; set { _ControlInfoDetailList = value; OnPropertyChanged(); } }
        public ObservableCollection<Test> TestListDB { get => _TestListDB; set { _TestListDB = value; OnPropertyChanged(); } }
        public ObservableCollection<User> UserList { get => _UserList; set { _UserList = value; OnPropertyChanged(); } }
        public ObservableCollection<UnitTable> UnitList { get => _UnitList; set { _UnitList = value; OnPropertyChanged(); } }
        public ObservableCollection<Test> TestList { get => _TestList; set { _TestList = value; OnPropertyChanged(); } }
        public ObservableCollection<DeviceTest> DeviceTestList { get => _DeviceTestList; set { _DeviceTestList = value; OnPropertyChanged(); } }
        public ObservableCollection<LevelQc> LevelList { get => _LevelList; set { _LevelList = value; OnPropertyChanged(); } }
        public ObservableCollection<string> Dates3 { get => _Dates3; set { _Dates3 = value; OnPropertyChanged(); } }
        public ObservableCollection<string> Dates1 { get => _Dates1; set { _Dates1 = value; OnPropertyChanged(); } }
        public ObservableCollection<string> Dates { get => _Dates; set { _Dates = value; OnPropertyChanged(); } }
        public ObservableCollection<string> Dates2 { get => _Dates2; set { _Dates2 = value; OnPropertyChanged(); } }
        public Visibility Visibility1 { get => _Visibility1; set { _Visibility1 = value; OnPropertyChanged(); } }
        public Visibility Visibility2 { get => _Visibility2; set { _Visibility2 = value; OnPropertyChanged(); } }
        public Visibility Visibility3 { get => _Visibility3; set { _Visibility3 = value; OnPropertyChanged(); } }
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
        public ChartValues<double> MeanValues1 { get => _MeanValues1; set { _MeanValues1 = value; OnPropertyChanged(); } }
        public ChartValues<double> MeanValues2 { get => _MeanValues2; set { _MeanValues2 = value; OnPropertyChanged(); } }
        public ChartValues<double> MeanValues3 { get => _MeanValues3; set { _MeanValues3 = value; OnPropertyChanged(); } }
        public ChartValues<double> PlusOneSDValues1 { get => _PlusOneSDValues1; set { _PlusOneSDValues1 = value; OnPropertyChanged(); } }
        public ChartValues<double> PlusOneSDValues2 { get => _PlusOneSDValues2; set { _PlusOneSDValues2 = value; OnPropertyChanged(); } }
        public ChartValues<double> PlusOneSDValues3 { get => _PlusOneSDValues3; set { _PlusOneSDValues3 = value; OnPropertyChanged(); } }
        public ChartValues<double> MinusOneSDValues1 { get => _MinusOneSDValues1; set { _MinusOneSDValues1 = value; OnPropertyChanged(); } }
        public ChartValues<double> MinusOneSDValues2 { get => _MinusOneSDValues2; set { _MinusOneSDValues2 = value; OnPropertyChanged(); } }
        public ChartValues<double> MinusOneSDValues3 { get => _MinusOneSDValues3; set { _MinusOneSDValues3 = value; OnPropertyChanged(); } }
        public ChartValues<double> PlusTwoSDValues2 { get => _PlusTwoSDValues2; set { _PlusTwoSDValues2 = value; OnPropertyChanged(); } }
        public ChartValues<double> PlusTwoSDValues1 { get => _PlusTwoSDValues1; set { _PlusTwoSDValues1 = value; OnPropertyChanged(); } }
        public ChartValues<double> PlusTwoSDValues3 { get => _PlusTwoSDValues3; set { _PlusTwoSDValues3 = value; OnPropertyChanged(); } }
        public ChartValues<double> MinusTwoSDValues1 { get => _MinusTwoSDValues1; set { _MinusTwoSDValues1 = value; OnPropertyChanged(); } }
        public ChartValues<double> MinusTwoSDValues2 { get => _MinusTwoSDValues2; set { _MinusTwoSDValues2 = value; OnPropertyChanged(); } }
        public ChartValues<double> MinusTwoSDValues3 { get => _MinusTwoSDValues3; set { _MinusTwoSDValues3 = value; OnPropertyChanged(); } }
        public ChartValues<double> PlusThreeSDValues1 { get => _PlusThreeSDValues1; set { _PlusThreeSDValues1 = value; OnPropertyChanged(); } }
        public ChartValues<double> PlusThreeSDValues2 { get => _PlusThreeSDValues2; set { _PlusThreeSDValues2 = value; OnPropertyChanged(); } }
        public ChartValues<double> PlusThreeSDValues3 { get => _PlusThreeSDValues3; set { _PlusThreeSDValues3 = value; OnPropertyChanged(); } }
       public ChartValues<double> MinusThreeSDValues3 { get => _MinusThreeSDValues3; set { _MinusThreeSDValues3 = value; OnPropertyChanged(); } }
        public ChartValues<double> MinusThreeSDValues2 { get => _MinusThreeSDValues2; set { _MinusThreeSDValues2 = value; OnPropertyChanged(); } }
         public ChartValues<double> MinusThreeSDValues1 { get => _MinusThreeSDValues1; set { _MinusThreeSDValues1 = value; OnPropertyChanged(); } }
        public float totalWidth1 { get => _totalWidth1; set { _totalWidth1 = value; OnPropertyChanged(); } }
        public float totalWidth2 { get => _totalWidth2; set { _totalWidth2 = value; OnPropertyChanged(); } }
        public float totalWidth3 { get => _totalWidth3; set { _totalWidth3 = value; OnPropertyChanged(); } }

        public ICommand PrintCommand { get; set; }
        public ICommand PrintCalibCommand { get; set; }
        public ICommand PrintChartCommand { get; set; }
        public ICommand ViewCommand { get; set; }
        public ICommand EditCommand { get; set; }
        public ICommand DeleteCommand { get; set; }
        public ICommand LoadedCommand { get; set; }
        public ICommand DeviceSelectionChangedCommand { get; set; }
        public ICommand TestSelectionChangedCommand { get; set; }
        public ICommand appRangeCommand { get; set; }
        public ICommand ScrollViewer_LoadedCommand { get; set; }
        public string DisplayName { get => _DisplayName; set { _DisplayName = value; OnPropertyChanged(); } }
        public DateTime StartDate { get => _StartDate; set { _StartDate = value; OnPropertyChanged(); } }
        public DateTime EndDate { get => _EndDate; set { _EndDate = value; OnPropertyChanged(); } }
        public string LOT { get => _LOT; set { _LOT = value; OnPropertyChanged(); } }
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
        public ControlInfo SelectedControlInfo
        {
            get => _SelectedControlInfo;
            set
            {
                _SelectedControlInfo = value;
                OnPropertyChanged();
            }
        }
        public ControlInfoDetail SelectedControlInfoDetail
        {
            get => _SelectedControlInfoDetail;
            set
            {
                _SelectedControlInfoDetail = value;
                OnPropertyChanged();
            }
        }
        public ICommand LoadDataCommand { get; set; }
        public HomeViewModel_V2()
        {
            _dbContext  = new QcManagmentContext();

            LoadDataCommand = new RelayCommand<object>((p) => true, async (p) => await LoadNew());

            LoadedCommand = new RelayCommand<Test>((p) =>
            {
                return true;

            }, async (p) =>
            {
                await LoadNew();
            });

            appRangeCommand = new RelayCommand<Test>((p) =>
            {
                return true;

            },async (p) =>
            {
               await ViewChart();
            });

            //ScrollViewer_LoadedCommand = new RelayCommand<ScrollViewer>((p) =>
            //{
            //    return true;

            //}, (p) =>
            //{
            //    OnScrollViewerLoaded(p);
            //});

            PrintCommand = new RelayCommand<object>((p) =>
            {
                if (SelectedTest == null || SelectedDevice == null) return false;
                else
                    return true;

            }, (p) =>
            {
                var results = List.Where(s => s.IdDevice == SelectedDevice.Id && s.IdTest == SelectedTest.Id && s.DateRun >= StartDate && s.DateRun <= EndDate).ToList();
                ReportView rp = new ReportView(results, isCheck);
                rp.ShowDialog();

            });

            PrintCalibCommand = new RelayCommand<object>((p) =>
            {
                if (SelectedTest == null || SelectedDevice == null) return false;
                else
                    return true;

            }, (p) =>
            {
                    var calresults = DataProvider.Ins.DB.CalResults
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
                if (SelectedTest == null || SelectedDevice == null)
                    return false;
                else
                    return true;

            }, (p) =>
            {
                // Thiết lập dữ liệu cho báo cáo
                var results = List.Where(s => s.IdDevice == SelectedDevice.Id && s.IdTest == SelectedTest.Id && s.DateRun >= StartDate && s.DateRun <= EndDate).ToList();
                ChartReportView rp = new ChartReportView(results, isCheck);
                rp.ShowDialog();

            });

            DeviceSelectionChangedCommand = new RelayCommand<ControlInfo>((p) =>
            {
                return true;

            }, (p) =>
            {

                if (SelectedDevice != null)
                {
                    // Update the TestList based on the selected device
                    var newTestList = new ObservableCollection<Test>(DeviceTestList
                        .Where(s => s.IdDevice == SelectedDevice.Id && s.IdTestNavigation.TestType == 2)
                        .Select(s => s.IdTestNavigation)
                        .OrderBy(s => s.Index));

                    // Check if the new list is different from the current one or if the SelectedTest is not in the new list
                    if (!TestList.SequenceEqual(newTestList) || !newTestList.Contains(SelectedTest))
                    {
                        TestList = newTestList;
                        OnPropertyChanged(nameof(TestList));

                        // Set SelectedTest to the first test in the updated list or null if the list is empty
                        //SelectedTest = TestList.FirstOrDefault();
                    }
                    else
                    {
                        // Force refresh of SelectedTest even if the list hasn't changed
                        var tempTest = SelectedTest;
                        SelectedTest = null;
                        OnPropertyChanged(nameof(SelectedTest));
                        SelectedTest = tempTest;
                    }

                    OnPropertyChanged(nameof(SelectedTest));
                }
            });

            TestSelectionChangedCommand = new RelayCommand<CartesianChart>((p) =>
            {
                return true;

            }, async (p) =>
            {
                await ViewChart();
            });

        }

        private async Task LoadNew()
        {
            IsLoading = true;
            try
            {
                var DB = await Task.Run(() =>  DataProvider.Ins.DB);
                List = new ObservableCollection<Result>(DB.Results.OrderBy(s => s.DateRun));
                UserList = new ObservableCollection<User>(DB.Users);
                LevelList = new ObservableCollection<LevelQc>(DB.LevelQcs);
                DeviceList = new ObservableCollection<Device>(DB.Devices);
                DeviceTestList = new ObservableCollection<DeviceTest>(DB.DeviceTests);
                UnitList = new ObservableCollection<UnitTable>(DB.UnitTables);
                TestListDB = new ObservableCollection<Test>(DB.Tests);
                if (SelectedDevice == null)
                {
                    TestList = new ObservableCollection<Test>();
                }
                else
                {
                    TestList = new ObservableCollection<Test>(DeviceTestList.Where(s => s.IdDevice == SelectedDevice.Id).Select(s => s.IdTestNavigation).OrderBy(s => s.Index));
                }
                ControlInfoDetailList = new ObservableCollection<ControlInfoDetail>(DB.ControlInfoDetails);
                ControlInfoList = new ObservableCollection<ControlInfo>(DB.ControlInfos);
                //SelectedTest = null;
                Visibility1 = Visibility.Collapsed;
                Visibility2 = Visibility.Collapsed;
                Visibility3 = Visibility.Collapsed;
                isCheck = false;
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

        //private void OnScrollViewerLoaded(ScrollViewer scrollViewer)
        //{
           
        //    if (scrollViewer != null)
        //    {
        //        MessageBox.Show("Loaded event has been called!");

        //        scrollViewer.ScrollToHorizontalOffset(100);
        //    }
        //}

        private async Task ViewChart()
        {
            InitializeYAxisLabelFormatter();
            try
            {
                if (SelectedDevice == null || SelectedTest == null)
                {
                    return;
                }

                Visibility1 = Visibility.Collapsed;
                Visibility2 = Visibility.Collapsed;
                Visibility3 = Visibility.Collapsed;

                IsLoading = true;
                OnPropertyChanged(nameof(IsLoading));

               

                var results = List.Where(s => s.IdDevice == SelectedDevice.Id && s.IdTest == SelectedTest.Id && s.DateRun >= StartDate && s.DateRun <= EndDate)
                    .OrderBy(s => s.DateRun.Year)
                    .ThenBy(s => s.DateRun.Month)
                    .ThenBy(s => s.DateRun.Day)
                    .ThenBy(s => s.IndexQc)
                    .ToList();
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

                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            switch (resultByLevel.Key)
                            {
                                case 1:
                                case 4:
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
                            }
                        });
                    }
                });

                await LoadChartAsync(isCheck);
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

        /* Chart 2
        private CartesianChart LoadChart2(IGrouping<int, Result> results)
        {
            var kqline = new ChartValues<Result>();
            Visibility visibility = new Visibility();
            Dates = new ObservableCollection<string>();
            YAxisLabels = new List<string>();


            foreach (var item in results)
            {
                kqline.Add(item);
                Dates.Add(item.DateRun.ToShortDateString());

            }
            if (kqline == null || kqline.Count == 0)
            {
                visibility = Visibility.Collapsed;
            }
            else visibility = Visibility.Visible;

            CartesianMapper<Result> Mapper = Mappers.Xy<Result>()
                .X((value, index) => index)
                .Y((value, index) => (value.Result1 - value.IdControlDetailNavigation.MeanNsx) / value.IdControlDetailNavigation.SdNsx)
                .Fill((value, index) => ((value.Result1 - value.IdControlDetailNavigation.MeanNsx) / value.IdControlDetailNavigation.SdNsx) > 2
                || ((value.Result1 - value.IdControlDetailNavigation.MeanNsx) / value.IdControlDetailNavigation.SdNsx) < -2 ? Brushes.Red : null)
                .Stroke(item => Brushes.Transparent);
            var seriesViews = new SeriesCollection
                    {
                        new LineSeries(Mapper)
                        {
                            Title = "Result",
                            PointForeground = Brushes.Blue,
                            StrokeThickness = 3,
                            Values = kqline,
                            LineSmoothness = 0,
                            Stroke = Brushes.LightGray,
                            Fill = Brushes.Transparent,
                            PointGeometry = DefaultGeometries.Circle,
                            PointGeometrySize = 15,
                        }
                    };
            CartesianChart cartesianChart = new CartesianChart();
            cartesianChart.Series = seriesViews;
            AxesCollection axesX = new AxesCollection();
            Axis axisX = new Axis();
            axisX.Labels = Dates;
            axisX.LabelsRotation = 0;
            axisX.Separator.Step = 1;
            axesX.Add(axisX);
            Axis axisY = new Axis();
            axisY.MaxValue = 3;
            axisY.MinValue = -3;
            axisY.Separator.Step = 1;
            AxesCollection axesY = new AxesCollection();
            axesY.Add(axisY);
            cartesianChart.AxisY = axesY;
            cartesianChart.AxisX = axesX;
            cartesianChart.Visibility = visibility;
            cartesianChart.DataTooltip.Visibility = Visibility.Hidden;

            return cartesianChart;
        }
        */

        //private void LoadNew()
        //{
        //    var DB = DataProvider.Ins.DB;
        //    List = new ObservableCollection<Result>(DB.Results.OrderBy(s => s.DateRun));
        //    UserList = new ObservableCollection<User>(DB.Users);
        //    LevelList = new ObservableCollection<LevelQc>(DB.LevelQcs);
        //    DeviceList = new ObservableCollection<Device>(DB.Devices);
        //    DeviceTestList = new ObservableCollection<DeviceTest>(DB.DeviceTests);
        //    UnitList = new ObservableCollection<UnitTable>(DB.UnitTables);
        //    TestListDB = new ObservableCollection<Test>(DB.Tests);
        //    if(SelectedDevice == null)
        //    {
        //        TestList = new ObservableCollection<Test>();
        //    }
        //    else
        //    {
        //        TestList = new ObservableCollection<Test>(DeviceTestList.Where(s => s.IdDevice == SelectedDevice.Id).Select(s => s.IdTestNavigation).OrderBy(s => s.Index));
        //    }
        //    ControlInfoDetailList = new ObservableCollection<ControlInfoDetail>(DB.ControlInfoDetails);
        //    ControlInfoList = new ObservableCollection<ControlInfo>(DB.ControlInfos);
        //    //SelectedTest = null;
        //    Visibility1 = Visibility.Collapsed;
        //    Visibility2 = Visibility.Collapsed;
        //    Visibility3 = Visibility.Collapsed;
        //    isCheck = false;
        //}
        private double CalculateMean(ObservableCollection<double> values)
        {
            double sum = 0;
            foreach (var value in values)
            {
                sum += value;
            }
            return sum / values.Count;
        }

        private double CalculateStandardDeviation(ObservableCollection<double> values, double mean)
        {
            double sumSquaredDifference = 0;
            foreach (var value in values)
            {
                sumSquaredDifference += Math.Pow(value - mean, 2);
            }
            double variance = sumSquaredDifference / values.Count;
            return Math.Sqrt(variance);
        }

        private async Task LoadChartAsync(bool isCheck)
        {
            var mapper1 = Mappers.Xy<Result>()
                  .X((value, index) => index) // lets use the position of the item as X
                  .Y(value => Math.Round((double)((value.Result1 - value.IdControlDetailNavigation.CurMean) / value.IdControlDetailNavigation.CurSd), 2))
                  .Fill((value, index) => ((value.Result1 - value.IdControlDetailNavigation.CurMean) / value.IdControlDetailNavigation.CurSd > 2 || (value.Result1 - value.IdControlDetailNavigation.CurMean) / value.IdControlDetailNavigation.CurSd < -2) ? Brushes.Red : null)
                  .Stroke(item => Brushes.Transparent);//and PurchasedItems property as Y

            var mapper2 = Mappers.Xy<Result>()
               .X((value, index) => index) // lets use the position of the item as X
               .Y(value => Math.Round((double)((value.Result1 - value.IdControlDetailNavigation.MeanApp) / value.IdControlDetailNavigation.SdApp), 2))
               .Fill((value, index) => ((value.Result1 - value.IdControlDetailNavigation.MeanApp) / value.IdControlDetailNavigation.SdApp > 2 || (value.Result1 - value.IdControlDetailNavigation.MeanApp) / value.IdControlDetailNavigation.SdApp < -2) ? Brushes.Red : null)
               .Stroke(item => Brushes.Transparent);//and PurchasedItems property as Y

            await Task.Run(() =>
            {
                if (isCheck == false)
                {
                    Charting.For<Result>(mapper1, SeriesOrientation.Horizontal);
                }
                else
                {
                    Charting.For<Result>(mapper2, SeriesOrientation.Horizontal);
                }
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



