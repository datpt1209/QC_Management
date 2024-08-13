using LiveCharts;
using LiveCharts.Configurations;
using LiveCharts.Defaults;
using LiveCharts.Definitions.Charts;
using LiveCharts.Wpf;
using MaterialDesignThemes.Wpf.Converters;
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
        private ObservableCollection<Result> _List;
        public ObservableCollection<Result> List { get => _List; set { _List = value; OnPropertyChanged(); } }
        public ChartValues<ObservablePoint> LineAtOneValues { get; set; }

        private Func<double, string> _yAxisLabelFormatter;
        public Func<double, string> YAxisLabelFormatter
        {
            get { return _yAxisLabelFormatter; }
            set
            {
                _yAxisLabelFormatter = value;
                OnPropertyChanged(nameof(YAxisLabelFormatter));
            }
        }

        private ObservableCollection<Device> _DeviceList;
        public ObservableCollection<Device> DeviceList { get => _DeviceList; set { _DeviceList = value; OnPropertyChanged(); } }

        private ObservableCollection<ControlInfo> _ControlInfoList;
        public ObservableCollection<ControlInfo> ControlInfoList { get => _ControlInfoList; set { _ControlInfoList = value; OnPropertyChanged(); } }

        private ObservableCollection<ControlInfoDetail> _ControlInfoDetailList;
        public ObservableCollection<ControlInfoDetail> ControlInfoDetailList { get => _ControlInfoDetailList; set { _ControlInfoDetailList = value; OnPropertyChanged(); } }

        private ObservableCollection<Test> _TestListDB;
        public ObservableCollection<Test> TestListDB { get => _TestListDB; set { _TestListDB = value; OnPropertyChanged(); } }

        private ObservableCollection<User> _UserList;
        public ObservableCollection<User> UserList { get => _UserList; set { _UserList = value; OnPropertyChanged(); } }

        private ObservableCollection<UnitTable> _UnitList;
        public ObservableCollection<UnitTable> UnitList { get => _UnitList; set { _UnitList = value; OnPropertyChanged(); } }

        private ObservableCollection<Test> _TestList;
        public ObservableCollection<Test> TestList { get => _TestList; set { _TestList = value; OnPropertyChanged(); } }

        private ObservableCollection<DeviceTest> _DeviceTestList;
        public ObservableCollection<DeviceTest> DeviceTestList { get => _DeviceTestList; set { _DeviceTestList = value; OnPropertyChanged(); } }

        private ObservableCollection<LevelQc> _LevelList;
        public ObservableCollection<LevelQc> LevelList { get => _LevelList; set { _LevelList = value; OnPropertyChanged(); } }

        private ObservableCollection<string> _Dates3;
        public ObservableCollection<string> Dates3 { get => _Dates3; set { _Dates3 = value; OnPropertyChanged(); } }

        private ObservableCollection<string> _Dates1;
        public ObservableCollection<string> Dates1 { get => _Dates1; set { _Dates1 = value; OnPropertyChanged(); } }

        private ObservableCollection<string> _Dates;
        public ObservableCollection<string> Dates { get => _Dates; set { _Dates = value; OnPropertyChanged(); } }
        private ObservableCollection<string> _Dates2;
        public ObservableCollection<string> Dates2 { get => _Dates2; set { _Dates2 = value; OnPropertyChanged(); } }

        private Visibility _Visibility1;
        public Visibility Visibility1 { get => _Visibility1; set { _Visibility1 = value; OnPropertyChanged(); } }

        private Visibility _Visibility2;
        public Visibility Visibility2 { get => _Visibility2; set { _Visibility2 = value; OnPropertyChanged(); } }

        private Visibility _Visibility3;
        public Visibility Visibility3 { get => _Visibility3; set { _Visibility3 = value; OnPropertyChanged(); } }


        private ChartValues<Result> _ChartValues1;
        public ChartValues<Result> ChartValues1 { get => _ChartValues1; set { _ChartValues1 = value; OnPropertyChanged(); } }

        private ChartValues<Result> _ChartValues2;
        public ChartValues<Result> ChartValues2 { get => _ChartValues2; set { _ChartValues2 = value; OnPropertyChanged(); } }

        private ChartValues<Result> _ChartValues3;
        public ChartValues<Result> ChartValues3 { get => _ChartValues3; set { _ChartValues3 = value; OnPropertyChanged(); } }

        private ChartValues<double> _MeanValues1;
        public ChartValues<double> MeanValues1 { get => _MeanValues1; set { _MeanValues1 = value; OnPropertyChanged();} }

        private ChartValues<double> _MeanValues2;
        public ChartValues<double> MeanValues2 { get => _MeanValues2; set { _MeanValues2 = value; OnPropertyChanged(); } }
        private ChartValues<double> _MeanValues3;
        public ChartValues<double> MeanValues3 { get => _MeanValues3; set { _MeanValues3 = value; OnPropertyChanged(); } }

        private ChartValues<double> _PlusOneSDValues1;
        public ChartValues<double> PlusOneSDValues1 { get => _PlusOneSDValues1; set { _PlusOneSDValues1 = value; OnPropertyChanged(); } }

        private ChartValues<double> _PlusOneSDValues2;
        public ChartValues<double> PlusOneSDValues2 { get => _PlusOneSDValues2; set { _PlusOneSDValues2 = value; OnPropertyChanged(); } }

        private ChartValues<double> _PlusOneSDValues3;
        public ChartValues<double> PlusOneSDValues3 { get => _PlusOneSDValues3; set { _PlusOneSDValues3 = value; OnPropertyChanged(); } }

        private ChartValues<double> _MinusOneSDValues1;
        public ChartValues<double> MinusOneSDValues1 { get => _MinusOneSDValues1; set { _MinusOneSDValues1 = value; OnPropertyChanged(); } }

        private ChartValues<double> _MinusOneSDValues2;
        public ChartValues<double> MinusOneSDValues2 { get => _MinusOneSDValues2; set { _MinusOneSDValues2 = value; OnPropertyChanged(); } }

        private ChartValues<double> _MinusOneSDValues3;
        public ChartValues<double> MinusOneSDValues3 { get => _MinusOneSDValues3; set { _MinusOneSDValues3 = value; OnPropertyChanged(); } }


        private ChartValues<double> _PlusTwoSDValues2;
        public ChartValues<double> PlusTwoSDValues2 { get => _PlusTwoSDValues2; set { _PlusTwoSDValues2 = value; OnPropertyChanged(); } }

        private ChartValues<double> _PlusTwoSDValues1;
        public ChartValues<double> PlusTwoSDValues1 { get => _PlusTwoSDValues1; set { _PlusTwoSDValues1 = value; OnPropertyChanged(); } }

        private ChartValues<double> _PlusTwoSDValues3;
        public ChartValues<double> PlusTwoSDValues3 { get => _PlusTwoSDValues3; set { _PlusTwoSDValues3 = value; OnPropertyChanged(); } }

        private ChartValues<double> _MinusTwoSDValues1;
        public ChartValues<double> MinusTwoSDValues1 { get => _MinusTwoSDValues1; set { _MinusTwoSDValues1 = value; OnPropertyChanged(); } }

        private ChartValues<double> _MinusTwoSDValues2;
        public ChartValues<double> MinusTwoSDValues2 { get => _MinusTwoSDValues2; set { _MinusTwoSDValues2 = value; OnPropertyChanged(); } }

        private ChartValues<double> _MinusTwoSDValues3;
        public ChartValues<double> MinusTwoSDValues3 { get => _MinusTwoSDValues3; set { _MinusTwoSDValues3 = value; OnPropertyChanged(); } }


        private ChartValues<double> _PlusThreeSDValues1;
        public ChartValues<double> PlusThreeSDValues1 { get => _PlusThreeSDValues1; set { _PlusThreeSDValues1 = value; OnPropertyChanged(); } }

        private ChartValues<double> _PlusThreeSDValues2;
        public ChartValues<double> PlusThreeSDValues2 { get => _PlusThreeSDValues2; set { _PlusThreeSDValues2 = value; OnPropertyChanged(); } }

        private ChartValues<double> _PlusThreeSDValues3;
        public ChartValues<double> PlusThreeSDValues3 { get => _PlusThreeSDValues3; set { _PlusThreeSDValues3 = value; OnPropertyChanged(); } }



        private ChartValues<double> _MinusThreeSDValues3;
        public ChartValues<double> MinusThreeSDValues3 { get => _MinusThreeSDValues3; set { _MinusThreeSDValues3 = value; OnPropertyChanged(); } }

        private ChartValues<double> _MinusThreeSDValues2;
        public ChartValues<double> MinusThreeSDValues2 { get => _MinusThreeSDValues2; set { _MinusThreeSDValues2 = value; OnPropertyChanged(); } }

        private ChartValues<double> _MinusThreeSDValues1;
        public ChartValues<double> MinusThreeSDValues1 { get => _MinusThreeSDValues1; set { _MinusThreeSDValues1 = value; OnPropertyChanged(); } }

        private float _totalWidth1;
        public float totalWidth1 { get => _totalWidth1; set { _totalWidth1 = value; OnPropertyChanged(); } }
        private float _totalWidth2;
        public float totalWidth2 { get => _totalWidth2; set { _totalWidth2 = value; OnPropertyChanged(); } }
        private float _totalWidth3;
        public float totalWidth3 { get => _totalWidth3; set { _totalWidth3 = value; OnPropertyChanged(); } }

        public ICommand PrintCommand { get; set; }
        public ICommand PrintChartCommand { get; set; }
        public ICommand ViewCommand { get; set; }
        public ICommand EditCommand { get; set; }
        public ICommand DeleteCommand { get; set; }
        public ICommand LoadedCommand { get; set; }
        public ICommand DeviceSelectionChangedCommand { get; set; }
        public ICommand TestSelectionChangedCommand { get; set; }
        public ICommand appRangeCommand { get; set; }
        public ICommand ScrollViewer_LoadedCommand { get; set; }

        private string _DisplayName;
        public string DisplayName { get => _DisplayName; set { _DisplayName = value; OnPropertyChanged(); } }

        private DateTime _StartDate = DateTime.Now.AddDays(-14);
        public DateTime StartDate { get => _StartDate; set { _StartDate = value; OnPropertyChanged(); } }

        private DateTime _EndDate = DateTime.Now;
        public DateTime EndDate { get => _EndDate; set { _EndDate = value; OnPropertyChanged(); } }


        private string _LOT;
        public string LOT { get => _LOT; set { _LOT = value; OnPropertyChanged(); } }

        private bool _isCheck;
        public bool isCheck { get => _isCheck; set { _isCheck = value; OnPropertyChanged(); } }


        private Test _SelectedTest;
        public Test SelectedTest
        {
            get => _SelectedTest;
            set
            {
                _SelectedTest = value;
                OnPropertyChanged();
            }
        }

        private double _Min;
        public double Min
        {
            get => _Min;
            set
            {
                _Min = value;
                OnPropertyChanged();
            }
        }
        public int max1 { get; set; }
        public int max2 { get; set; }
        public int max3 { get; set; }
        private double _Max;
        public double Max
        {
            get => _Max;
            set
            {
                _Max = value;
                OnPropertyChanged();
            }
        }

        private Device _SelectedDevice;
        public Device SelectedDevice
        {
            get => _SelectedDevice;
            set
            {
                _SelectedDevice = value;
                OnPropertyChanged();
            }
        }
        private double _horizontalOffset;
        public double HorizontalOffset
        {
            get => _horizontalOffset;
            set
            {
                _horizontalOffset = value;
                OnPropertyChanged();
            }
        }

        private ControlInfo _SelectedControlInfo;
        public ControlInfo SelectedControlInfo
        {
            get => _SelectedControlInfo;
            set
            {
                _SelectedControlInfo = value;
                OnPropertyChanged();
            }
        }

        private ControlInfoDetail _SelectedControlInfoDetail;
        public ControlInfoDetail SelectedControlInfoDetail
        {
            get => _SelectedControlInfoDetail;
            set
            {
                _SelectedControlInfoDetail = value;
                OnPropertyChanged();
            }
        }

        private LineSeries _oneSDLine;
        public LineSeries oneSDLine
        {
            get => _oneSDLine;
            set
            {
                _oneSDLine = value;
                OnPropertyChanged();
            }
        }

        private LineSeries _twoSDLine;
        public LineSeries twoSDLine
        {
            get => _twoSDLine;
            set
            {
                _twoSDLine = value;
                OnPropertyChanged();
            }
        }

        
        public Brush OneToTwoSDFill { get; set; }
        public Brush TwoToThreeSDFill { get; set; }
        public HomeViewModel_V2()
        {
           
            InitializeYAxisLabelFormatter();
        LoadedCommand = new RelayCommand<Test>((p) =>
            {
                return true;

            }, (p) =>
            {
                LoadNew();
            });

            appRangeCommand = new RelayCommand<Test>((p) =>
            {
                return true;

            }, (p) =>
            {
                ViewChart();
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
                    var newTestList = new ObservableCollection<Test>(DeviceTestList.Where(s => s.IdDevice == SelectedDevice.Id).Select(s => s.IdTestNavigation).OrderBy(s => s.Index));

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

            }, (p) =>
            {
                ViewChart();
            });

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

        private async void ViewChart()
        {
            try
            {
                if (SelectedDevice == null || SelectedTest == null)
                {
                    return;
                }
                    Visibility1 = Visibility.Collapsed;
                    Visibility2 = Visibility.Collapsed;
                    Visibility3 = Visibility.Collapsed;
                    var results = List.Where(s => s.IdDevice == SelectedDevice.Id && s.IdTest == SelectedTest.Id && s.DateRun >= StartDate && s.DateRun <= EndDate);
                    if (results.Count() == 0 || results == null)
                    {
                        MessageBox.Show("Không có dữ liệu", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Error);

                        return;
                    }

                await Task.Run(() =>
                {
                        var levelList = results.GroupBy(s => s.IdLevel);

                        foreach (var resultByLevel in levelList)
                        {
                        if (resultByLevel.Key == 1 || resultByLevel.Key == 4)
                            {
                                MeanValues1 = new ChartValues<double>();
                            PlusOneSDValues1 = new ChartValues<double>();   
                                MinusOneSDValues1 = new ChartValues<double>();
                            PlusTwoSDValues1 = new ChartValues<double>();
                                MinusTwoSDValues1 = new ChartValues<double>();
                                PlusThreeSDValues1 = new ChartValues<double>();
                                MinusThreeSDValues1 = new ChartValues<double>();
                                var result = LoadChart1(resultByLevel);
                                ChartValues1 = result.Item1;
                                Visibility1 = result.Item2;
                                Dates1 = result.Item3;
                            for(int i = 0; i < result.Item1.Count + 1; i++)
                            {
                                PlusThreeSDValues1.Add(3);
                                PlusTwoSDValues1.Add(2);
                                PlusOneSDValues1.Add(1);
                                MeanValues1.Add(0);
                                MinusOneSDValues1.Add(-1);
                                MinusTwoSDValues1.Add(-2);
                                MinusThreeSDValues1.Add(-3);
                            }
                            float cmPerPoint = 2.0f; // 1 cm
                            float pixelsPerPoint = CmToPixels(cmPerPoint);
                            int numberOfPoints = result.Item1.Count;
                            totalWidth1 = pixelsPerPoint * numberOfPoints;
                        }
                            if (resultByLevel.Key == 2 || resultByLevel.Key == 5)
                            {
                                MeanValues2 = new ChartValues<double>();
                                PlusOneSDValues2 = new ChartValues<double>();
                                MinusOneSDValues2 = new ChartValues<double>();
                                    PlusTwoSDValues2 = new ChartValues<double>();
                            MinusTwoSDValues2 = new ChartValues<double>();
                            PlusThreeSDValues2 = new ChartValues<double>();
                            MinusThreeSDValues2 = new ChartValues<double>();
                                var result = LoadChart1(resultByLevel);
                                ChartValues2 = result.Item1;
                            for (int i = 0; i < result.Item1.Count + 1; i++)
                            {
                                MeanValues2.Add(0);
                                PlusOneSDValues2.Add(1);
                                MinusOneSDValues2.Add(-1);
                                PlusTwoSDValues2.Add(2);
                                MinusTwoSDValues2.Add(-2);
                                PlusThreeSDValues2.Add(3);
                                MinusThreeSDValues2.Add(-3);
                            }
                            Visibility2 = result.Item2;
                                Dates2 = result.Item3;
                            float cmPerPoint = 2.0f; // 1 cm
                            float pixelsPerPoint = CmToPixels(cmPerPoint);
                            int numberOfPoints = result.Item1.Count;
                            totalWidth2 = pixelsPerPoint * numberOfPoints;
                        }

                            if (resultByLevel.Key == 3 || resultByLevel.Key == 6)
                            {
                            MeanValues3 = new ChartValues<double>();
                            PlusOneSDValues3 = new ChartValues<double>();
                                MinusOneSDValues3 = new ChartValues<double>();
                            PlusTwoSDValues3 = new ChartValues<double>();
                            MinusTwoSDValues3 = new ChartValues<double>();  
                                PlusThreeSDValues3 = new ChartValues<double>();
                                MinusThreeSDValues3 = new ChartValues<double>();
                                    
                                var result = LoadChart1(resultByLevel);
                                ChartValues3 = result.Item1;
                            for (int i = 0; i < result.Item1.Count + 1; i++)
                            {
                                MeanValues3.Add(0);
                                PlusOneSDValues3.Add(1);
                                MinusOneSDValues3.Add(-1);
                                PlusTwoSDValues3.Add(2);
                                MinusTwoSDValues3.Add(-2);
                                PlusThreeSDValues3.Add(3);
                                MinusThreeSDValues3.Add(-3);
                            }
                            Visibility3 = result.Item2;
                                Dates3 = result.Item3;
                            float cmPerPoint = 2.0f; // 1 cm
                            float pixelsPerPoint = CmToPixels(cmPerPoint);
                            int numberOfPoints = result.Item1.Count;
                            totalWidth3 = pixelsPerPoint * numberOfPoints;
                        }

                        }
                       
                    // Các xử lý khác
                });
                LoadChart(isCheck);

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải biểu đồ: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            
        }

        private Tuple<ChartValues<Result>, Visibility, ObservableCollection<string>> LoadChart1(IGrouping<int, Result> results)
        {
            var visibility = new Visibility();
            var dataPoints = new ChartValues<Result>();
            var dates = new ObservableCollection<string>();
 
            foreach (var item in results)
            {
                
                dataPoints.Add(item);
                dates.Add(item.DateRun.ToString("dd/MM"));
            }
            if (dataPoints == null || dataPoints.Count == 0)
            {
                visibility = Visibility.Collapsed;
            }
            else visibility = Visibility.Visible;
           
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

        private void LoadNew()
        {
            var DB = DataProvider.Ins.DB;
            List = new ObservableCollection<Result>(DB.Results.OrderBy(s => s.DateRun));
            UserList = new ObservableCollection<User>(DB.Users);
            LevelList = new ObservableCollection<LevelQc>(DB.LevelQcs);
            DeviceList = new ObservableCollection<Device>(DB.Devices);
            DeviceTestList = new ObservableCollection<DeviceTest>(DB.DeviceTests);
            UnitList = new ObservableCollection<UnitTable>(DB.UnitTables);
            TestListDB = new ObservableCollection<Test>(DB.Tests);
            if(SelectedDevice == null)
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

        private void LoadChart(bool isCheck)
        {
            var mapper1 = Mappers.Xy<Result>()
                  .X((value, index) => index) // lets use the position of the item as X
                  .Y(value => Math.Round((value.Result1 - value.IdControlDetailNavigation.MeanNsx) / value.IdControlDetailNavigation.SdNsx, 2))
                  .Fill((value, index) => ((value.Result1 - value.IdControlDetailNavigation.MeanNsx) / value.IdControlDetailNavigation.SdNsx > 2 || (value.Result1 - value.IdControlDetailNavigation.MeanNsx) / value.IdControlDetailNavigation.SdNsx < -2) ? Brushes.Red : null)
                  .Stroke(item => Brushes.Transparent);//and PurchasedItems property as Y

            var mapper2 = Mappers.Xy<Result>()
               .X((value, index) => index) // lets use the position of the item as X
               .Y(value => Math.Round((double)((value.Result1 - value.IdControlDetailNavigation.MeanApp) / value.IdControlDetailNavigation.SdApp), 2))
               .Fill((value, index) => ((value.Result1 - value.IdControlDetailNavigation.MeanApp) / value.IdControlDetailNavigation.SdApp > 2 || (value.Result1 - value.IdControlDetailNavigation.MeanApp) / value.IdControlDetailNavigation.SdApp < -2) ? Brushes.Red : null)
               .Stroke(item => Brushes.Transparent);//and PurchasedItems property as Y

            if (isCheck == false)
            {
               Charting.For<Result>(mapper1, SeriesOrientation.Horizontal);
            }
            else
            {
                Charting.For<Result>(mapper2, SeriesOrientation.Horizontal);
            }
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



