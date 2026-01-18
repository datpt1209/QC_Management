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
    public partial class ReportView : Window
    {
        List<Result> resultList;
        private string fillter;
        public ObservableCollection<string> FilterOptions { get; set; } = new()
         {
             "Nhà sản xuât",
             "Đang sử dụng",
             "Thống kê"
         };

        public ReportView(List<Result> resultList, string fillter)
        {
            InitializeComponent();
            this.resultList = resultList.ToList();
            this.fillter = fillter;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var reportSource = new Object();
            if (fillter == FilterOptions[1])
            {
                reportSource = resultList
                    .Select(s => new
                    {
                        ResultType = s.ResultType,
                        IsOutRange = s.IsOutRange,
                        QualitativeRange = s.IdControlDetailNavigation?.QualitativeMean,
                        QualitativeResult = s.QualitativeResult,
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
                        SDPXN = s.IdControlDetailNavigation?.CurSd ?? 0,
                        MeanPXN = s.IdControlDetailNavigation?.CurMean ?? 0,
                        ExpirationDate = s.IdControlDetailNavigation?.IdControlInfoNavigation?.ExpirationDate,
                        ProductionDate = s.IdControlDetailNavigation?.IdControlInfoNavigation?.ProductionDate,
                        SDs = (s.Result1 - (s.IdControlDetailNavigation?.CurMean ?? 0)) / (s.IdControlDetailNavigation?.CurSd ?? 1),
                        IsCorrected = s.IsCorrected,
                    })
                .OrderBy(s => s.DateRun.Month)
                .ThenBy(s => s.DateRun.Day)
                .ThenBy(s => s.Index)
                .ToList();
            }
            else if(fillter == FilterOptions[0])
            {
                reportSource = resultList
                    .Select(s => new
                    {
                        ResultType = s.ResultType,
                        IsOutRange = s.IsOutRange,
                        QualitativeRange = s.IdControlDetailNavigation?.QualitativeMean,
                        QualitativeResult = s.QualitativeResult,
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
                        SDPXN = s.IdControlDetailNavigation?.CurSd ?? 0,
                        MeanPXN = s.IdControlDetailNavigation?.CurMean ?? 0,
                        ExpirationDate = s.IdControlDetailNavigation?.IdControlInfoNavigation?.ExpirationDate,
                        ProductionDate = s.IdControlDetailNavigation?.IdControlInfoNavigation?.ProductionDate,
                        SDs = (s.Result1 - (s.IdControlDetailNavigation?.MeanNsx ?? 0)) / (s.IdControlDetailNavigation?.SdNsx ?? 1),
                        IsCorreted = s.IsCorrected,
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
                        ResultType = s.ResultType,
                        IsOutRange = s.IsOutRange,
                        QualitativeRange = s.IdControlDetailNavigation.QualitativeMean,
                        QualitativeResult = s.QualitativeResult,
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
                        SDPXN = s.IdControlDetailNavigation?.CurSd ?? 0,
                        MeanPXN = s.IdControlDetailNavigation?.CurMean ?? 0,
                        ExpirationDate = s.IdControlDetailNavigation?.IdControlInfoNavigation?.ExpirationDate,
                        ProductionDate = s.IdControlDetailNavigation?.IdControlInfoNavigation?.ProductionDate,
                        SDs = (s.Result1 - (s.IdControlDetailNavigation?.CurMean ?? 0)) / (s.IdControlDetailNavigation?.CurSd ?? 1),
                        IsCorreted = s.IsCorrected,
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
            reportViewer.SetDisplayMode(DisplayMode.PrintLayout);
            reportViewer.RefreshReport();
        }

        // Call this to export current report to PDF with default filename "Tên XN + ResultReport"
        private void BtnExportPdf_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (reportViewer.LocalReport == null)
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

        private static string MakeSafeFileName(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;
            var invalids = Path.GetInvalidFileNameChars();
            return string.Join("_", input.Split(invalids, StringSplitOptions.RemoveEmptyEntries)).Replace(" ", "_");
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}