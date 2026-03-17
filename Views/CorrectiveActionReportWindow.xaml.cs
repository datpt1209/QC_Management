using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows;
using Microsoft.Reporting.WinForms;
using QC_Management.Models;

namespace QC_Management.Views
{
    /// <summary>
    /// Interaction logic for CorrectiveActionReportWindow.xaml
    /// </summary>
    public partial class CorrectiveActionReportWindow : Window
    {
        private readonly List<CorrectiveAction> _items;
        private string _lastPdfPath = string.Empty;

        public CorrectiveActionReportWindow(IEnumerable<CorrectiveAction> items)
        {
            InitializeComponent();
            _items = items?.ToList() ?? new List<CorrectiveAction>();
            RenderAndShow();
        }

        private void RenderAndShow()
        {
            try
            {
                var assembly = typeof(CorrectiveActionReportWindow).Assembly;
                // match embedded resource path to your project (Reports folder)
                using var stream = assembly.GetManifestResourceStream("QC_Management.Report.CorrectiveActionReport.rdlc")
                                   ?? assembly.GetManifestResourceStream("QC_Management.Reports.CorrectiveActionReport.rdlc");

                // Build DataSet1 (dtCorrectAction) - fields must match RDLC DataSet1 field names
                var dtCorrectAction = new DataTable("DataSet1");
                dtCorrectAction.Columns.Add("InternalErrorId", typeof(string));
                dtCorrectAction.Columns.Add("ResolvingResultId", typeof(string));
                dtCorrectAction.Columns.Add("ActionDescription", typeof(string));
                dtCorrectAction.Columns.Add("ActionOwner", typeof(string));
                dtCorrectAction.Columns.Add("ActionCompleteAt", typeof(DateTime));
                dtCorrectAction.Columns.Add("Outcome", typeof(string));
                dtCorrectAction.Columns.Add("Cause", typeof(string)); // use InternalError.Cause
                dtCorrectAction.Columns.Add("PreventiveAction", typeof(string));
                dtCorrectAction.Columns.Add("CreatedAt", typeof(DateTime));
                dtCorrectAction.Columns.Add("CreatedBy", typeof(string));
                dtCorrectAction.Columns.Add("Device", typeof(string));
                dtCorrectAction.Columns.Add("ErrorDescription", typeof(string));
                dtCorrectAction.Columns.Add("Level", typeof(string));
                dtCorrectAction.Columns.Add("TestName", typeof(string));

                // New fields: pre/post reference ranges and pre/post results
                dtCorrectAction.Columns.Add("PreCorrectResult", typeof(string));
                dtCorrectAction.Columns.Add("PostCorrectResult", typeof(string));
                dtCorrectAction.Columns.Add("ReferenceRangeBefore", typeof(string));
                dtCorrectAction.Columns.Add("ReferenceRangeAfter", typeof(string));
                dtCorrectAction.Columns.Add("Unit", typeof(string));


                // build range from control detail mean/sd using multiplier (2 by default)
                static string BuildRangeFromMeanSd(double? mean, double? sd, double multiplier = 2.0)
                {
                    if (mean.HasValue && sd.HasValue)
                    {
                        var min = (mean.Value - multiplier * sd.Value).ToString("F2");
                        var max = (mean.Value + multiplier * sd.Value).ToString("F2");
                        return $"{min} - {max}";
                    }
                    return string.Empty;
                }

                foreach (var ca in _items)
                {
                    var row = dtCorrectAction.NewRow();
                    row["InternalErrorId"] = ca.InternalErrorId.ToString();
                    row["ResolvingResultId"] = ca.ResolvingResultId?.ToString() ?? string.Empty;
                    row["ActionDescription"] = ca.ActionDescription ?? string.Empty;
                    row["ActionOwner"] = ca.ActionOwner ?? string.Empty;
                    row["ActionCompleteAt"] = ca.ActionCompletedAt ?? (object)DBNull.Value;
                    row["Outcome"] = ca.Outcome ?? string.Empty;

                    // Cause is stored on InternalError (not on CorrectiveAction)
                    row["Cause"] = ca.InternalError?.Cause ?? string.Empty;
                    row["PreventiveAction"] = ca.PreventiveAction ?? string.Empty;

                    // IMPORTANT: Use InternalError.CreatedAt / CreatedBy for "detection" (Ngày giờ phát hiện / Người phát hiện).
                    // If InternalError is missing or its CreatedAt is default, fall back to CorrectiveAction.CreatedAt/CreatedBy,
                    // otherwise set DBNull for CreatedAt if no meaningful value is available.
                    if (ca.InternalError != null && ca.InternalError.CreatedAt != default(DateTime))
                    {
                        row["CreatedAt"] = ca.InternalError.CreatedAt;
                    }
                    else if (ca.CreatedAt != default(DateTime))
                    {
                        row["CreatedAt"] = ca.CreatedAt;
                    }
                    else
                    {
                        row["CreatedAt"] = DBNull.Value;
                    }

                    row["CreatedBy"] = ca.InternalError?.CreatedBy ?? ca.CreatedBy ?? string.Empty;

                    // defaults
                    row["Device"] = string.Empty;
                    row["ErrorDescription"] = string.Empty;
                    row["Level"] = string.Empty;
                    row["TestName"] = string.Empty;
                    row["ReferenceRangeBefore"] = string.Empty;
                    row["ReferenceRangeAfter"] = string.Empty;
                    row["PreCorrectResult"] = string.Empty;
                    row["PostCorrectResult"] = string.Empty;
                    row["Unit"] = string.Empty;

                    // Compute Level value: prefer erroneous result level, then control-info-detail level, then resolving result level
                    string levelName = string.Empty;

                    // if there is an InternalError navigation, populate device/test/level/errordesc & Pre range/result
                    if (ca.InternalError != null)
                    {
                        row["Device"] = ca.InternalError.Device?.Name ?? string.Empty;
                        row["ErrorDescription"] = ca.InternalError.WestgardDescription ?? string.Empty;
                        row["TestName"] = ca.InternalError.Test?.Name ?? string.Empty;

                        // Unit: prefer InternalError.Test navigation, then ErroneousResult's test nav, then ResolvingResult's test nav
                        var unit = ca.InternalError?.Test?.IdUnitTableNavigation?.Name
                                   ?? ca.InternalError?.ErroneousResult?.IdTestNavigation?.IdUnitTableNavigation?.Name
                                   ?? ca.ResolvingResult?.IdTestNavigation?.IdUnitTableNavigation?.Name
                                   ?? string.Empty;
                        row["Unit"] = unit;

                        // Pre-correct result (erroneous result)
                        var preRes = ca.InternalError.ErroneousResult;
                        if (preRes != null)
                        {
                            // Pre result value
                            row["PreCorrectResult"] = preRes.Result1?.ToString("G") ?? preRes.TempResult ?? string.Empty;

                            // ReferenceRangeBefore: derive from pre result's control detail first,
                            // then fall back to InternalError.ControlInfoDetail (we no longer use InternalError.RangeMin/RangeMax)
                            var rr = BuildRangeFromMeanSd(preRes.IdControlDetailNavigation?.CurMean, preRes.IdControlDetailNavigation?.CurSd);
                            if (string.IsNullOrEmpty(rr))
                            {
                                rr = BuildRangeFromMeanSd(ca.InternalError.ControlInfoDetail?.CurMean, ca.InternalError.ControlInfoDetail?.CurSd);
                            }

                            row["ReferenceRangeBefore"] = rr;

                            // Level from pre result's level navigation (if available)
                            levelName = preRes.IdLevelNavigation?.Name ?? string.Empty;

                            // If unit wasn't found earlier, try to get it from resolving result's test navigation
                            if (string.IsNullOrEmpty(row["Unit"] as string))
                            {
                                var unitFromPost = preRes.IdTestNavigation?.IdUnitTableNavigation?.Name ?? string.Empty;
                                if (!string.IsNullOrEmpty(unitFromPost))
                                    row["Unit"] = unitFromPost;
                            }
                        }
                        else
                        {
                            // no explicit erroneous result: attempt to show range from InternalError's control detail
                            row["ReferenceRangeBefore"] = BuildRangeFromMeanSd(ca.InternalError.ControlInfoDetail?.CurMean, ca.InternalError.ControlInfoDetail?.CurSd);

                            // fallback: try control info detail level
                            levelName = ca.InternalError.ControlInfoDetail?.IdLevelNavigation?.Name ?? string.Empty;
                        }
                    }

                    // PostCorrectResult: prefer ResolvingResult on corrective action; compute ReferenceRangeAfter from that result
                    if (ca.ResolvingResult != null)
                    {
                        var post = ca.ResolvingResult;
                        row["PostCorrectResult"] = post.Result1?.ToString("G") ?? post.TempResult ?? string.Empty;

                        // ReferenceRangeAfter: derive from resolving result's control detail first; fallback to InternalError control detail
                        var rrAfter = BuildRangeFromMeanSd(post.IdControlDetailNavigation?.CurMean, post.IdControlDetailNavigation?.CurSd);
                        if (string.IsNullOrEmpty(rrAfter))
                        {
                            rrAfter = BuildRangeFromMeanSd(ca.InternalError?.ControlInfoDetail?.CurMean, ca.InternalError?.ControlInfoDetail?.CurSd);
                        }
                        row["ReferenceRangeAfter"] = rrAfter;

                        // If level not set yet, take level from resolving result
                        if (string.IsNullOrEmpty(levelName))
                            levelName = post.IdLevelNavigation?.Name ?? string.Empty;

                    }

                    // final assign level
                    row["Level"] = levelName ?? string.Empty;

                    dtCorrectAction.Rows.Add(row);
                }

                // Configure the WinForms ReportViewer hosted in XAML
                PdfViewer.Reset();
                PdfViewer.ProcessingMode = ProcessingMode.Local;

                if (stream != null)
                {
                    PdfViewer.LocalReport.LoadReportDefinition(stream);
                }
                else
                {
                    var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Report", "CorrectiveActionReport.rdlc");
                    using var fs = File.OpenRead(path);
                    PdfViewer.LocalReport.LoadReportDefinition(fs);
                }

                PdfViewer.LocalReport.DataSources.Clear();
                PdfViewer.LocalReport.DataSources.Add(new ReportDataSource("DataSet1", dtCorrectAction));
                // If you need DataSet2, add similarly:
                // PdfViewer.LocalReport.DataSources.Add(new ReportDataSource("DataSet2", dtResult));

                // Default the viewer to Print Layout and fit page width
                PdfViewer.SetDisplayMode(DisplayMode.PrintLayout);
                PdfViewer.ZoomMode = ZoomMode.PageWidth;

                // Refresh to apply data and layout
                PdfViewer.RefreshReport();

                // clear any cached PDF path — report is now displayed in viewer
                _lastPdfPath = string.Empty;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Không thể tạo báo cáo: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnSavePdf_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Ensure report is loaded
                if (PdfViewer.LocalReport == null || PdfViewer.LocalReport.ReportPath == null && PdfViewer.LocalReport.GetDefaultPageSettings() == null)
                    RenderAndShow();

                // Render PDF from the LocalReport
                string mimeType, encoding, fileNameExtension;
                string[] streams;
                Warning[] warnings;
                var bytes = PdfViewer.LocalReport.Render("PDF", null, out mimeType, out encoding, out fileNameExtension, out streams, out warnings);

                var dlg = new Microsoft.Win32.SaveFileDialog
                {
                    FileName = $"CorrectiveActions_{DateTime.Now:yyyyMMdd_HHmmss}.pdf",
                    Filter = "PDF file|*.pdf"
                };
                if (dlg.ShowDialog() == true)
                {
                    File.WriteAllBytes(dlg.FileName, bytes);
                    MessageBox.Show("Lưu file PDF thành công.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    _lastPdfPath = dlg.FileName;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lưu PDF thất bại: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnOpenPdf_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Render PDF from the LocalReport
                string mimeType, encoding, fileNameExtension;
                string[] streams;
                Warning[] warnings;
                var bytes = PdfViewer.LocalReport.Render("PDF", null, out mimeType, out encoding, out fileNameExtension, out streams, out warnings);

                var tmp = Path.Combine(Path.GetTempPath(), $"CorrectiveActions_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");
                File.WriteAllBytes(tmp, bytes);

                _lastPdfPath = tmp;
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(tmp) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Mở PDF thất bại: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e) => Close();
    }
}
