using Microsoft.EntityFrameworkCore;
using Microsoft.Reporting.WinForms;
using QC_Management.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
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
        private bool isCheck;
        public ChartReportView(List<Result> resultList, bool isCheck)
        {
            InitializeComponent();
            this.resultList = resultList.ToList();
            this.isCheck = isCheck;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var reportSource = new Object();
            if(isCheck)
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
                     Time = s.DateRun.Add((System.TimeSpan)s.Time).ToString("hh:mm:ss"),
                     Mean = s.IdControlDetailNavigation.MeanNsx,
                     SD = s.IdControlDetailNavigation.SdNsx,
                     Unit = s.IdTestNavigation.IdUnitTableNavigation.Name,
                     WestgardRule = s.WestgardRule,
                     SDPXN = s.IdControlDetailNavigation.SdApp,
                     MeanPXN = s.IdControlDetailNavigation.MeanApp,
                     ExpirationDate = s.IdControlDetailNavigation.IdControlInfoNavigation.ExpirationDate,
                     ProductionDate = s.IdControlDetailNavigation.IdControlInfoNavigation.ProductionDate,
                     SDs = (s.Result1 - s.IdControlDetailNavigation.MeanApp) / s.IdControlDetailNavigation.SdApp,
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
                     DateRun = s.DateRun,
                     Time = s.DateRun.Add((System.TimeSpan)s.Time).ToString("hh:mm:ss"),
                     Mean = s.IdControlDetailNavigation.MeanNsx,
                     SD = s.IdControlDetailNavigation.SdNsx,
                     Unit = s.IdTestNavigation.IdUnitTableNavigation.Name,
                     WestgardRule = s.WestgardRule,
                     SDPXN = (double)s.IdControlDetailNavigation.SdApp,
                     MeanPXN = (double)s.IdControlDetailNavigation.MeanApp,
                     ExpirationDate = s.IdControlDetailNavigation.IdControlInfoNavigation.ExpirationDate,
                     ProductionDate = s.IdControlDetailNavigation.IdControlInfoNavigation.ProductionDate,
                     SDs = (s.Result1 - s.IdControlDetailNavigation.MeanNsx) / s.IdControlDetailNavigation.SdNsx,
                 }).OrderBy(s => s.DateRun.Month)
                    .ThenBy(s => s.DateRun.Day)
                    .ThenBy(s=>s.Index)
                    .ToList();
            }
           
           
            reportViewer.LocalReport.ReportEmbeddedResource = "QC_Management.Report.ChartReport.rdlc";
            ReportDataSource rds = new ReportDataSource();
            rds.Name = "DataSet1";
            rds.Value = reportSource;
            reportViewer.LocalReport.DataSources.Clear();
            reportViewer.LocalReport.DataSources.Add(rds);
            reportViewer.RefreshReport();
        }
    }
}
