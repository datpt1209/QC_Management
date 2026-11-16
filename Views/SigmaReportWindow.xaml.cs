using Microsoft.Reporting.WinForms;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

namespace QC_Management.Views
{
    public partial class SigmaReportWindow : Window
    {
        private readonly IReadOnlyList<SigmaReportItem> _items;
        private readonly string _deviceName;
        private readonly DateTime _periodStart;
        private readonly DateTime _periodEnd;

        public SigmaReportWindow(
            IReadOnlyList<SigmaReportItem> items,
            string deviceName,
            DateTime periodStart,
            DateTime periodEnd)
        {
            InitializeComponent();

            _items = items;
            _deviceName = deviceName;
            _periodStart = periodStart;
            _periodEnd = periodEnd;

            Loaded += SigmaReportWindow_Loaded;
        }

        private void SigmaReportWindow_Loaded(
            object sender,
            RoutedEventArgs e)
        {
            ReportViewer.LocalReport.ReportEmbeddedResource =
                "QC_Management.Report.SigmaReport.rdlc";

            var dataSource = new ReportDataSource(
                "DataSet1",
                _items);

            ReportViewer.LocalReport.DataSources.Clear();
            ReportViewer.LocalReport.DataSources.Add(dataSource);

            ReportViewer.LocalReport.SetParameters(new[]
            {
                new ReportParameter(
                    "DeviceName",
                    _deviceName),

                new ReportParameter(
                    "Period",
                    $"{_periodStart:dd/MM/yyyy} - {_periodEnd:dd/MM/yyyy}"),

                new ReportParameter(
                    "ReportYear",
                    _periodEnd.Year.ToString())
            });

            ReportViewer.SetDisplayMode(DisplayMode.PrintLayout);
            ReportViewer.ZoomMode = ZoomMode.PageWidth;
            ReportViewer.RefreshReport();
        }

        private void PrintReport_Click(
            object sender,
            RoutedEventArgs e)
        {
            ReportViewer.PrintDialog();
        }

        private void ExportPdf_Click(
            object sender,
            RoutedEventArgs e)
        {
            try
            {
                var bytes = ReportViewer.LocalReport.Render(
                    "PDF",
                    null,
                    out _,
                    out _,
                    out _,
                    out _,
                    out _);

                var dialog = new SaveFileDialog
                {
                    FileName =
                        $"SigmaReport_{DateTime.Now:yyyyMMdd_HHmmss}.pdf",
                    Filter = "PDF file|*.pdf"
                };

                if (dialog.ShowDialog() == true)
                {
                    File.WriteAllBytes(dialog.FileName, bytes);

                    MessageBox.Show(
                        "Xuất báo cáo PDF thành công.",
                        "Báo cáo Six Sigma",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Xuất báo cáo thất bại: {ex.Message}",
                    "Báo cáo Six Sigma",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void Close_Click(
            object sender,
            RoutedEventArgs e)
        {
            Close();
        }
    }
}