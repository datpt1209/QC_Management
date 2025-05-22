using LiveCharts.Wpf;
using LiveCharts;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System;
using QC_Management.Models;
using System.Windows;


namespace QC_Management.Views
{
    /// <summary>
    /// Interaction logic for UserControl_CustomersTooltip.xaml
    /// </summary>
    public partial class CustomersTooltip : IChartTooltip
    {
        private TooltipData _data;
        public CustomersTooltip()
        {
            InitializeComponent();
            //DataContext = this;
        }
        public static readonly DependencyProperty SelectedFilterProperty =
            DependencyProperty.Register(
                nameof(SelectedFilter),
                typeof(string),
                typeof(CustomersTooltip),
                new PropertyMetadata(null));


        private string _selectedFilter;
        public string SelectedFilter
        {
            get => (string)GetValue(SelectedFilterProperty);
            set
            {
                SetValue(SelectedFilterProperty, value);
                OnPropertyChanged(nameof(SelectedFilter));
                OnPropertyChanged(nameof(FilteredPoints));
                OnPropertyChanged(nameof(DisplayPoints));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public TooltipData Data
        {
            get { return _data; }
            set
            {
                _data = value;
                OnPropertyChanged("Data");
                OnPropertyChanged("FilteredPoints");
                OnPropertyChanged(nameof(DisplayPoints));
            }
        }

        public TooltipSelectionMode? SelectionMode { get; set; }

        public IEnumerable<DataPointViewModel> FilteredPoints
        {
            get
            {
                if (_data == null) return null;
                return _data.Points.Where(p => p.Series.Title == "result");
            }
        }

        public IEnumerable<TooltipDisplayModel> DisplayPoints
        {
            get
            {
                if (_data == null)
                {
                    yield break; // End the iteration if _data is null
                }
                foreach (var point in _data.Points.Where(p => p.Series.Title == "result"))
                {
                    // Map DataPointViewModel to TooltipDisplayModel
                    var result = point.ChartPoint.Instance as Result;
                    var detail = result.IdControlDetailNavigation;
                    double? mean = null, sd = null;
                    switch (SelectedFilter)
                    {
                        case "Nhà sản xuât":
                            mean = detail?.MeanNsx;
                            sd = detail?.SdNsx;
                            break;
                        case "Đang sử dụng":
                            mean = detail?.CurMean;
                            sd = detail?.CurSd;
                            break;
                        case "Thống kê":
                            mean = detail?.MeanApp;
                            sd = detail?.SdApp;
                            break;
                    }
                    yield return new TooltipDisplayModel
                    {
                        Lot = detail?.Lot,
                        DateRun = result.DateRun,
                        Result1 = result.Result1,
                        Mean = mean,
                        SD = sd
                    };
                }
            }
        }

        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
    public class TooltipDisplayModel
    {
        public string Lot { get; set; }
        public DateTime DateRun { get; set; }
        public double? Result1 { get; set; }
        public double? Mean { get; set; }
        public double? SD { get; set; }
    }
}