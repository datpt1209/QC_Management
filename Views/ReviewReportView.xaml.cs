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
    public partial class ReivewReportView : Window
    {
        List<Result> resultList;
        
        public ReivewReportView(List<Result> resultList)
        {
            InitializeComponent();
            this.resultList = resultList;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var reportSource = new Object();
        
                reportSource = resultList.Select(s => new
                {
                    NameDevice = s.IdDeviceNavigation.Name,
                    LOTQC = s.IdControlDetailNavigation.Lot,
                    NameTest = s.IdTestNavigation.Name,
                    Level = s.IdLevelNavigation.Name,
                    Result = s.Result1,
                    Index = s.IndexQc,
                    UserName = s.IdUserNavigation.DisplayName,
                    DateRun = s.DateRun.ToShortDateString(),
                    Time = s.DateRun.Add((System.TimeSpan)s.Time).ToString("HH:mm:ss"),
                    Mean = s.IdControlDetailNavigation.MeanNsx,
                    SD = s.IdControlDetailNavigation.SdNsx,
                    Unit = s.IdTestNavigation.IdUnitTableNavigation.Name,
                    WestgardRule = s.WestgardRule,
                    Comment = s.Comment,
                    SDPXN = s.IdControlDetailNavigation.SdApp,
                    MeanPXN = s.IdControlDetailNavigation.MeanApp,
                    SDs = (s.Result1 - s.IdControlDetailNavigation.MeanApp) / s.IdControlDetailNavigation.SdApp,

                });
          
            reportViewer.LocalReport.ReportEmbeddedResource = "QC_Management.Report.ReviewResultReport.rdlc";
            ReportDataSource rds = new ReportDataSource();
            rds.Name = "DataSet1";
            rds.Value = reportSource;
            reportViewer.LocalReport.DataSources.Clear();
            reportViewer.LocalReport.DataSources.Add(rds);
            reportViewer.RefreshReport();
        }

    }
}
