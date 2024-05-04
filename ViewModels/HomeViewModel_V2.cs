using LiveCharts;
using LiveCharts.Configurations;
using LiveCharts.Defaults;
using LiveCharts.Definitions.Charts;
using LiveCharts.Wpf;
using QC_Management.Models;
using QC_Management.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using XAct;

namespace QC_Management.ViewModels
{
    public class HomeViewModel_V2 : BaseViewModel
    {
        private ObservableCollection<Result> _List;
        public ObservableCollection<Result> List { get => _List; set { _List = value; OnPropertyChanged(); } }

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

        public ICommand PrintCommand { get; set; }
        public ICommand PrintChartCommand { get; set; }
        public ICommand ViewCommand { get; set; }
        public ICommand EditCommand { get; set; }
        public ICommand DeleteCommand { get; set; }
        public ICommand LoadedCommand { get; set; }
        public ICommand DeviceSelectionChangedCommand { get; set; }
        public ICommand TestSelectionChangedCommand { get; set; }
        public ICommand appRangeCommand { get; set; }

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
        public HomeViewModel_V2()
        {

            QcManagmentContext DB = LoadNew();

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
                    TestList = new ObservableCollection<Test>(DeviceTestList.Where(s => s.IdDevice == SelectedDevice.Id).Select(s => s.IdTestNavigation).OrderBy(s => s.Index));
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

        private async void ViewChart()
        {
            if (SelectedTest != null)
            {
                Visibility1 = Visibility.Collapsed;
                Visibility2 = Visibility.Collapsed;
                Visibility3 = Visibility.Collapsed;
                var results = List.Where(s => s.IdDevice == SelectedDevice.Id && s.IdTest == SelectedTest.Id && s.DateRun >= StartDate && s.DateRun <= EndDate);

                if (results.Count() == 0 || results == null)
                {
                    MessageBox.Show("Không có dữ liệu", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Error);
                 
                    return;
                }
                else
                {
                    var levelList = results.GroupBy(s => s.IdLevel);

                    foreach (var resultByLevel in levelList)
                    {
                        if (resultByLevel.Key == 1 || resultByLevel.Key == 4)
                        {
                            var result = LoadChart1(resultByLevel);
                            ChartValues1 = result.Item1;
                            Visibility1 = result.Item2;
                            Dates1 = result.Item3;
                        }
                        if (resultByLevel.Key == 2 || resultByLevel.Key == 5)
                        {
                            var result = LoadChart1(resultByLevel);
                            ChartValues2 = result.Item1;
                            Visibility2 = result.Item2;
                            Dates2 = result.Item3;
                        }

                        if (resultByLevel.Key == 3 || resultByLevel.Key == 6)
                        {
                            var result = LoadChart1(resultByLevel);
                            ChartValues3 = result.Item1;
                            Visibility3 = result.Item2;
                            Dates3 = result.Item3;
                        }

                    }
                    LoadChart(isCheck);
                }
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
                dates.Add(item.DateRun.ToShortDateString());
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

        private QcManagmentContext LoadNew()
        {
            var DB = DataProvider.Ins.DB;
            List = new ObservableCollection<Result>(DB.Results.OrderBy(s => s.DateRun));
            UserList = new ObservableCollection<User>(DB.Users);
            LevelList = new ObservableCollection<LevelQc>(DB.LevelQcs);
            DeviceList = new ObservableCollection<Device>(DB.Devices);
            DeviceTestList = new ObservableCollection<DeviceTest>(DB.DeviceTests);
            UnitList = new ObservableCollection<UnitTable>(DB.UnitTables);
            SelectedDevice = DeviceList.FirstOrDefault();
            TestListDB = new ObservableCollection<Test>(DB.Tests);
            TestList = new ObservableCollection<Test>(DeviceTestList.Where(s => s.IdDevice == SelectedDevice.Id).Select(s => s.IdTestNavigation));
            ControlInfoDetailList = new ObservableCollection<ControlInfoDetail>(DB.ControlInfoDetails);
            ControlInfoList = new ObservableCollection<ControlInfo>(DB.ControlInfos);
            SelectedTest = TestList.FirstOrDefault();
            Visibility1 = Visibility.Collapsed;
            Visibility2 = Visibility.Collapsed;
            Visibility3 = Visibility.Collapsed;
            isCheck = false;
            
            return DB;
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
    }

}



