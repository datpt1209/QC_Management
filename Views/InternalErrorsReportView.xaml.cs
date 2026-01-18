using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Reporting.WinForms;
using QC_Management.Models;
using System.IO;
using System.Windows.Forms.Integration;

namespace QC_Management.Views
{
    /// <summary>
    /// Interaction logic for InternalErrorsReportView.xaml
    /// </summary>
    public partial class InternalErrorsReportView : Window
    {
        private readonly IEnumerable<InternalErrorReportRow> _rows;
        private byte[]? _lastPdfBytes;
        private string? _lastPdfPath;

        public InternalErrorsReportView(IEnumerable<InternalErrorReportRow> rows)
        {
            InitializeComponent();
            _rows = rows ?? Enumerable.Empty<InternalErrorReportRow>();
            Loaded += InternalErrorsReportView_Loaded;
        }

        private void InternalErrorsReportView_Loaded(object? sender, RoutedEventArgs e)
        {
            // Ensure the WinForms ReportViewer is configured
            PdfViewer.ProcessingMode = ProcessingMode.Local;

            // Locate rdlc file in output folder: Report/InternalErrorsReportrt1.rdlc
            var rdlcPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Report", "InternalErrorsReportrt1.rdlc");
            if (!File.Exists(rdlcPath))
            {
                MessageBox.Show($"Report file not found: {rdlcPath}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            PdfViewer.LocalReport.DataSources.Clear();
            PdfViewer.LocalReport.ReportPath = rdlcPath;

            var rds = new ReportDataSource("DataSet1", _rows);
            PdfViewer.LocalReport.DataSources.Add(rds);

            PdfViewer.RefreshReport();
            // Ensure the viewer shows Print Layout and fits page width AFTER refresh
            PdfViewer.SetDisplayMode(DisplayMode.PrintLayout);
            PdfViewer.ZoomMode = ZoomMode.PageWidth;
        }

        private void BtnSavePdf_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var report = PdfViewer.LocalReport;
                var mimeType = string.Empty;
                var encoding = string.Empty;
                var fileNameExtension = string.Empty;
                string[] streams;
                Warning[] warnings;

                var bytes = report.Render("PDF", null, out mimeType, out encoding, out fileNameExtension, out streams, out warnings);

                var dlg = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "PDF Files (*.pdf)|*.pdf",
                    FileName = $"InternalErrors_{DateTime.Now:yyyyMMddHHmmss}.pdf"
                };

                if (dlg.ShowDialog() == true)
                {
                    File.WriteAllBytes(dlg.FileName, bytes);
                    _lastPdfBytes = bytes;
                    _lastPdfPath = dlg.FileName;
                    MessageBox.Show("PDF saved.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Save PDF failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnOpenPdf_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_lastPdfPath != null && File.Exists(_lastPdfPath))
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(_lastPdfPath) { UseShellExecute = true });
                    return;
                }

                // if no saved file, render to temp and open
                var report = PdfViewer.LocalReport;
                string mimeType, encoding, fileNameExtension;
                string[] streams;
                Warning[] warnings;
                var bytes = report.Render("PDF", null, out mimeType, out encoding, out fileNameExtension, out streams, out warnings);

                var tmp = Path.Combine(Path.GetTempPath(), $"InternalErrors_{DateTime.Now:yyyyMMddHHmmss}.pdf");
                File.WriteAllBytes(tmp, bytes);
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(tmp) { UseShellExecute = true });
                _lastPdfPath = tmp;
                _lastPdfBytes = bytes;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Open PDF failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
