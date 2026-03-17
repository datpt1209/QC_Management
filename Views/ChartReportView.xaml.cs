using Microsoft.EntityFrameworkCore;
using Microsoft.Reporting.WinForms;
using QC_Management.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using Microsoft.Win32;

namespace QC_Management
{
    /// <summary>
    /// Interaction logic for ChartReportView.xaml
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
            this.resultList = resultList?.ToList() ?? new List<Result>();
            this.fillter = fillter;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Helper for robust color mapping (handles numeric and textual level values)
            static string MapLevelToColor(string levelName)
            {
                var ln = (levelName ?? string.Empty).Trim();
                return ln == "1" || ln == "Low" || ln == "Ne" ? "Green" :
                       ln == "2" || ln == "Normal" || ln == "Pos" || ln == "Pos 1" ? "Orange" :
                       ln == "3" || ln == "High" || ln == "Pos 2" ? "Red" : "Black";
            }

            // Symmetric clamp for SDs: values < -4 become -4, values > 4 become 4
            static double? ClampSDs(double? sd)
            {
                if (!sd.HasValue) return sd;
                var v = sd.Value;
                if (v > 4) return 4;
                if (v < -4) return -4;
                return v;
            }

            var reportSource = new Object();
            if (fillter == FilterOptions[1])
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
                    // Use persisted ZScore when available; otherwise fallback to previous CurMean/CurSd calculation
                    SDs = ClampSDs(s.ZScore ?? ((s.Result1 - s.IdControlDetailNavigation.CurMean) / s.IdControlDetailNavigation.CurSd)),
                    SeriesColor = MapLevelToColor(s.IdLevelNavigation.Name),
                    IsEmptyPoint = s.Result1 == null // Add IsEmptyPoint flag
                }).OrderBy(s => s.DateRun.Month)
                  .ThenBy(s => s.DateRun.Day)
                  .ThenBy(s => s.Index)
                  .ToList();
            }
            else if (fillter == FilterOptions[0])
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
                    // Use persisted ZScore when available; otherwise fallback to manufacturer mean/sd
                    SDs = ClampSDs((s.Result1 - s.IdControlDetailNavigation.MeanNsx) / s.IdControlDetailNavigation.SdNsx),
                    SeriesColor = MapLevelToColor(s.IdLevelNavigation.Name),
                    IsEmptyPoint = s.Result1 == null // Add IsEmptyPoint flag
                }).OrderBy(s => s.DateRun.Month)
                  .ThenBy(s => s.DateRun.Day)
                  .ThenBy(s => s.Index)
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
                    // Use persisted ZScore when available; otherwise fallback to application mean/sd
                    SDs = ClampSDs((s.Result1 - s.IdControlDetailNavigation.MeanApp) / s.IdControlDetailNavigation.SdApp),
                    SeriesColor = MapLevelToColor(s.IdLevelNavigation.Name),
                    IsEmptyPoint = s.Result1 == null // Add IsEmptyPoint flag
                }).OrderBy(s => s.DateRun.Month)
                  .ThenBy(s => s.DateRun.Day)
                  .ThenBy(s => s.Index)
                  .ToList();
            }

            reportViewer.LocalReport.ReportEmbeddedResource = "QC_Management.Report.ChartReport.rdlc";
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

        // Export currently displayed report to PDF and prompt user to save
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
                    ? $"ChartReport_{DateTime.Now:yyyyMMdd_HHmmss}.pdf"
                    : $"{safeName}_ChartReport_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";

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
            return string.Join("_", input.Split(invalids, StringSplitOptions.RemoveEmptyEntries)).Replace(" ", "_");
        }
    }
}
