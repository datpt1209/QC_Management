using Microsoft.EntityFrameworkCore;
using Microsoft.Reporting.WinForms;
using QC_Management.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows;
using Microsoft.Win32;

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
                ResultType = s.ResultType,
                IsOutRange = s.IsOutRange,
                QualitativeRange = s.IdControlDetailNavigation.QualitativeMean,
                QualitativeResult = s.QualitativeResult,
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
            reportViewer.SetDisplayMode(DisplayMode.PrintLayout);
            reportViewer.RefreshReport();
        }

        // Export PDF, default filename = "{TestName}_ResultReport_yyyyMMdd_HHmmss.pdf"
        private void BtnExportPdf_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (reportViewer?.LocalReport == null)
                {
                    MessageBox.Show("Report not loaded.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                string testName = string.Empty;
                if (resultList != null && resultList.Count > 0)
                    testName = resultList[0].IdTestNavigation?.Name ?? string.Empty;

                var safeName = MakeSafeFileName(testName);
                var defaultFileName = string.IsNullOrWhiteSpace(safeName)
                    ? $"ResultReport_{DateTime.Now:yyyyMMdd_HHmmss}.pdf"
                    : $"{safeName}_ResultReport_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";

                string mimeType, encoding, fileNameExtension;
                string[] streams;
                Warning[] warnings;
                var bytes = reportViewer.LocalReport.Render("PDF", null, out mimeType, out encoding, out fileNameExtension, out streams, out warnings);

                var dlg = new SaveFileDialog
                {
                    FileName = defaultFileName,
                    Filter = "PDF file|*.pdf"
                };

                if (dlg.ShowDialog() == true)
                {
                    File.WriteAllBytes(dlg.FileName, bytes);
                    MessageBox.Show("Lưu file PDF thành công.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Xuất PDF thất bại: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e) => Close();

        private static string MakeSafeFileName(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;
            var invalids = Path.GetInvalidFileNameChars();
            var cleaned = string.Join("_", input.Split(invalids, StringSplitOptions.RemoveEmptyEntries));
            return cleaned.Replace(" ", "_");
        }
    }
}
