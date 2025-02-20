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
    public partial class ReportView : Window
    {
        List<Result> resultList;
        private bool isCheck;

        public ReportView(List<Result> resultList, bool isCheck)
        {
            InitializeComponent();
            this.resultList = resultList.ToList();
            this.isCheck = isCheck;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var reportSource = new Object();
            if (isCheck)
            {
                reportSource = resultList
                    .Select(s => new
                    {
                        NameDevice = s.IdDeviceNavigation?.Name,
                        LOTQC = s.IdControlDetailNavigation?.Lot,
                        NameTest = s.IdTestNavigation?.Name,
                        Level = s.IdLevelNavigation?.Name,
                        Result = s.Result1,
                        Index = s.IndexQc,
                        UserName = s.IdUserNavigation?.DisplayName,
                        DateRun = s.DateRun,
                        Time = s.DateRun.Add(s.Time ?? TimeSpan.Zero).ToString("hh:mm:ss"),
                        Mean = s.IdControlDetailNavigation?.MeanNsx,
                        SD = s.IdControlDetailNavigation?.SdNsx,
                        Unit = s.IdTestNavigation?.IdUnitTableNavigation?.Name,
                        WestgardRule = s.WestgardRule,
                        Comment = s.Comment,
                        SDPXN = s.IdControlDetailNavigation?.SdApp ?? 0,
                        MeanPXN = s.IdControlDetailNavigation?.MeanApp ?? 0,
                        ExpirationDate = s.IdControlDetailNavigation?.IdControlInfoNavigation?.ExpirationDate,
                        ProductionDate = s.IdControlDetailNavigation?.IdControlInfoNavigation?.ProductionDate,
                        SDs = (s.Result1 - (s.IdControlDetailNavigation?.MeanApp ?? 0)) / (s.IdControlDetailNavigation?.SdApp ?? 1),
                    })
                .OrderBy(s => s.DateRun.Month)
                .ThenBy(s => s.DateRun.Day)
                .ThenBy(s => s.Index)
                .ToList();
            }
            else
            {
                reportSource = resultList
                    .Select(s => new
                    {
                        NameDevice = s.IdDeviceNavigation?.Name,
                        LOTQC = s.IdControlDetailNavigation?.Lot,
                        NameTest = s.IdTestNavigation?.Name,
                        Level = s.IdLevelNavigation?.Name,
                        Result = s.Result1,
                        Index = s.IndexQc,
                        UserName = s.IdUserNavigation?.DisplayName,
                        DateRun = s.DateRun,
                        Time = s.DateRun.Add(s.Time ?? TimeSpan.Zero).ToString("hh:mm:ss"),
                        Mean = s.IdControlDetailNavigation?.MeanNsx,
                        SD = s.IdControlDetailNavigation?.SdNsx,
                        Unit = s.IdTestNavigation?.IdUnitTableNavigation?.Name,
                        WestgardRule = s.WestgardRule,
                        Comment = s.Comment,
                        SDPXN = s.IdControlDetailNavigation?.SdApp ?? 0,
                        MeanPXN = s.IdControlDetailNavigation?.MeanApp ?? 0,
                        ExpirationDate = s.IdControlDetailNavigation?.IdControlInfoNavigation?.ExpirationDate,
                        ProductionDate = s.IdControlDetailNavigation?.IdControlInfoNavigation?.ProductionDate,
                        SDs = (s.Result1 - (s.IdControlDetailNavigation?.MeanNsx ?? 0)) / (s.IdControlDetailNavigation?.SdNsx ?? 1),
                    })
                .OrderBy(s => s.DateRun.Month)
                .ThenBy(s => s.DateRun.Day)
                .ThenBy(s => s.Index)
                .ToList();
            }

            // Register the RenderingComplete event
            reportViewer.LocalReport.ReportEmbeddedResource = "QC_Management.Report.ResultsReport.rdlc";
            ReportDataSource rds = new ReportDataSource
            {
                Name = "DataSet1",
                Value = reportSource
            };
            reportViewer.LocalReport.DataSources.Clear();
            reportViewer.LocalReport.DataSources.Add(rds);
            reportViewer.RefreshReport();
          
        }
       
    }
}