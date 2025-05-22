using Microsoft.EntityFrameworkCore;
using Microsoft.Reporting.WinForms;
using QC_Management.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Windows;

namespace QC_Management
{
    /// <summary>
    /// Interaction logic for ReportView.xaml
    /// </summary>

    public partial class ChartReportView : Window
    {
        private List<Result> resultList;
        private string fillter;
        public ObservableCollection<string> FilterOptions { get; set; } = new()
         {
             "Nhà sản xuât",
             "Đang sử dụng",
             "Thống kê"
         };
        public ChartReportView(List<Result> resultList, string fillter)
        {
            CultureInfo cultureInfo = new CultureInfo("vi-VN");
            CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
            CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

            InitializeComponent();
            this.resultList = resultList.ToList();
            this.fillter = fillter;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var reportSource = new Object();
            if(fillter == FilterOptions[1])
            {
                 reportSource = resultList.Select(s => new 
                {
                     Id = s.Id,
                     NameDevice = s.IdDeviceNavigation.Name,
                     Index = s.IndexQc,
                     LOTQC = s.IdControlDetailNavigation.Lot,
                     NameTest = s.IdTestNavigation.Name,
                     Level = s.IdLevelNavigation.Name,
                     Result = s.Result1,
                     UserName = s.IdUserNavigation.DisplayName,
                     DateRun = s.DateRun,
                     DateRunString = s.DateRun.ToString("dd/MM/yy"),
                     Time = s.DateRun.Add((System.TimeSpan)s.Time).ToString("hh:mm:ss"),
                     Mean = s.IdControlDetailNavigation.MeanNsx,
                     SD = s.IdControlDetailNavigation.SdNsx,
                     Unit = s.IdTestNavigation.IdUnitTableNavigation.Name,
                     WestgardRule = s.WestgardRule,
                     SDPXN = s.IdControlDetailNavigation.CurSd,
                     MeanPXN = s.IdControlDetailNavigation.CurMean,
                     ExpirationDate = s.IdControlDetailNavigation.IdControlInfoNavigation.ExpirationDate,
                     ProductionDate = s.IdControlDetailNavigation.IdControlInfoNavigation.ProductionDate,
                     SDs = (s.Result1 - s.IdControlDetailNavigation.CurMean) / s.IdControlDetailNavigation.CurSd,
                     SeriesColor = s.IdLevelNavigation.Name == "1" || s.IdLevelNavigation.Name == "Low" ? "LightSeaGreen" :
                                    s.IdLevelNavigation.Name == "2" || s.IdLevelNavigation.Name =="Normal" ? "Salmon" :
                                     s.IdLevelNavigation.Name == "3" || s.IdLevelNavigation.Name =="High" ? "DimGray" : "Black",
                    IsEmptyPoint = s.Result1 == null // Add IsEmptyPoint flag
                 }).OrderBy(s => s.DateRun.Month)
                    .ThenBy(s => s.DateRun.Day)
                    .ThenBy(s=>s.Index)
                    .ToList();
            }
            else if(fillter == FilterOptions[0])
            {
                 reportSource = resultList.Select(s => new 
                {
                     Id = s.Id,
                     NameDevice = s.IdDeviceNavigation.Name,
                     Index = s.IndexQc,
                     LOTQC = s.IdControlDetailNavigation.Lot,
                     NameTest = s.IdTestNavigation.Name,
                     Level = s.IdLevelNavigation.Name,
                     Result = s.Result1,
                     UserName = s.IdUserNavigation.DisplayName,
                     DateRun = s.DateRun.Date,
                     DateRunString = s.DateRun.ToString("dd/MM/yy"),
                     Time = s.DateRun.Add((System.TimeSpan)s.Time).ToString("hh:mm:ss"),
                     Mean = s.IdControlDetailNavigation.MeanNsx,
                     SD = s.IdControlDetailNavigation.SdNsx,
                     Unit = s.IdTestNavigation.IdUnitTableNavigation.Name,
                     WestgardRule = s.WestgardRule,
                     SDPXN = (double)s.IdControlDetailNavigation.CurSd,
                     MeanPXN = (double)s.IdControlDetailNavigation.CurMean,
                     ExpirationDate = s.IdControlDetailNavigation.IdControlInfoNavigation.ExpirationDate,
                     ProductionDate = s.IdControlDetailNavigation.IdControlInfoNavigation.ProductionDate,
                     SDs = (s.Result1 - s.IdControlDetailNavigation.MeanNsx) / s.IdControlDetailNavigation.SdNsx,
                     SeriesColor = s.IdLevelNavigation.Name == "1" || s.IdLevelNavigation.Name == "Low" ? "LightSeaGreen" :
                                    s.IdLevelNavigation.Name == "2" || s.IdLevelNavigation.Name == "Normal" ? "Salmon" :
                                     s.IdLevelNavigation.Name == "3" || s.IdLevelNavigation.Name == "High" ? "DimGray" : "Black",
                     IsEmptyPoint = s.Result1 == null // Add IsEmptyPoint flag
                 }).OrderBy(s => s.DateRun.Month)
                    .ThenBy(s => s.DateRun.Day)
                    .ThenBy(s=>s.Index)
                    .ToList();
            }
            else
            {
                reportSource = resultList.Select(s => new
                {
                    Id = s.Id,
                    NameDevice = s.IdDeviceNavigation.Name,
                    Index = s.IndexQc,
                    LOTQC = s.IdControlDetailNavigation.Lot,
                    NameTest = s.IdTestNavigation.Name,
                    Level = s.IdLevelNavigation.Name,
                    Result = s.Result1,
                    UserName = s.IdUserNavigation.DisplayName,
                    DateRun = s.DateRun.Date,
                    DateRunString = s.DateRun.ToString("dd/MM/yy"),
                    Time = s.DateRun.Add((System.TimeSpan)s.Time).ToString("hh:mm:ss"),
                    Mean = s.IdControlDetailNavigation.MeanNsx,
                    SD = s.IdControlDetailNavigation.SdNsx,
                    Unit = s.IdTestNavigation.IdUnitTableNavigation.Name,
                    WestgardRule = s.WestgardRule,
                    SDPXN = (double)s.IdControlDetailNavigation.CurSd,
                    MeanPXN = (double)s.IdControlDetailNavigation.CurMean,
                    ExpirationDate = s.IdControlDetailNavigation.IdControlInfoNavigation.ExpirationDate,
                    ProductionDate = s.IdControlDetailNavigation.IdControlInfoNavigation.ProductionDate,
                    SDs = (s.Result1 - s.IdControlDetailNavigation.MeanApp) / s.IdControlDetailNavigation.SdApp,
                    SeriesColor = s.IdLevelNavigation.Name == "1" || s.IdLevelNavigation.Name == "Low" ? "LightSeaGreen" :
                                  s.IdLevelNavigation.Name == "2" || s.IdLevelNavigation.Name == "Normal" ? "Salmon" :
                                   s.IdLevelNavigation.Name == "3" || s.IdLevelNavigation.Name == "High" ? "DimGray" : "Black",
                    IsEmptyPoint = s.Result1 == null // Add IsEmptyPoint flag
                }).OrderBy(s => s.DateRun.Month)
                  .ThenBy(s => s.DateRun.Day)
                  .ThenBy(s => s.Index)
                  .ToList();
            }
                //var reportViewer = new ReportViewer();
                reportViewer.LocalReport.ReportEmbeddedResource = "QC_Management.Report.ChartReport.rdlc";
         
            ReportDataSource rds = new ReportDataSource();
            rds.Name = "DataSet1";
            rds.Value = reportSource;
            reportViewer.LocalReport.DataSources.Clear();
            reportViewer.LocalReport.DataSources.Add(rds);
            reportViewer.SetDisplayMode(DisplayMode.PrintLayout);
            reportViewer.RefreshReport();
        }
    }
}
