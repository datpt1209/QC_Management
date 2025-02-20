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
    public partial class CalibReportView : Window
    {
        List<CalResult> calResultList;

        public CalibReportView(List<CalResult> calResults)
        {
            InitializeComponent();
            this.calResultList = calResults.ToList();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ReportViewer reportViewer2 = new ReportViewer();
            var reportSource2 = new Object();
            reportSource2 = calResultList
                    .Select(s => new
                    {
                        NameDevice = s.IdDeviceNavigation?.Name,
                        LOTCAL = s.IdCalDetailNavigation.IdCalInforNavigation.CalLot,
                        NameTest = s.IdTestNavigation?.Name,
                        Level = s.Level,
                        Result = s.Result,
                        IndexCal = s.IndexCal,
                        UserName = s.IdUserNavigation?.DisplayName,
                        DateRun = s.DateRun,
                        Time = s.DateRun.Add(s.Time ?? TimeSpan.Zero).ToString("hh:mm:ss"),
                        MinValue = s.IdCalDetailNavigation?.MinValue,
                        MaxValue = s.IdCalDetailNavigation?.MaxValue,
                        Unit = s.IdTestNavigation?.IdUnitTableNavigation?.Name,
                        Comment = s.Comment,
                        CalResult = s.Result,
                        CALName = s.IdCalDetailNavigation.IdCalInforNavigation.IdCalTypeNavigation.CalTypeName,
                        ExpirationDate = s.IdCalDetailNavigation.IdCalInforNavigation.ExpirationDate,
                       
                    })
                .OrderBy(s => s.DateRun.Month)
                .ThenBy(s => s.DateRun.Day)
                .ToList();
            
            reportViewer2.LocalReport.ReportEmbeddedResource = "QC_Management.Report.CALResultsReport.rdlc";
            reportViewer2.LocalReport.DataSources.Clear();
            reportViewer2.LocalReport.DataSources.Add(new ReportDataSource("DataSet1", reportSource2));
            reportViewer2.SetDisplayMode(DisplayMode.PrintLayout);
            reportViewer2.RefreshReport();
            windowsFormsHost2.Child = reportViewer2;
        }
    }
}