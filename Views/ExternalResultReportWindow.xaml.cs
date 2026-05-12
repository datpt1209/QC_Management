using Microsoft.Reporting.WinForms;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms.Integration;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace QC_Management.Views
{
    /// <summary>
    /// Interaction logic for ExternalResultReportWindow.xaml
    /// </summary>
    public partial class ExternalResultReportWindow : Window
    {
        private ReportViewer _rv;

        public ExternalResultReportWindow()
        {
            InitializeComponent();

            // create ReportViewer and host it
            _rv = new ReportViewer();
            _rv.ProcessingMode = ProcessingMode.Local;
            var host = new WindowsFormsHost();
            host.Child = _rv;
            this.Content = host;
        }

        // Load report from a DataTable. RDLC expects DataSet Name="dsResult".
        // Prefer embedded RDLC: make sure Report\ExternalResulReport.rdlc Build Action = __Embedded Resource__.
        public void LoadReport(DataTable data, string embeddedResourcePath = "QC_Management.Report.ExternalResulReport.rdlc")
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            _rv.Reset();

            // Use embedded resource (safer for deployment). Fallback to ReportPath if necessary.
            try
            {
                _rv.LocalReport.ReportEmbeddedResource = embeddedResourcePath;
            }
            catch
            {
                // If embedded not found, caller can pass a file path by using ReportPath overload (not implemented here).
                throw new InvalidOperationException($"Report embedded resource not found: {embeddedResourcePath}");
            }

            _rv.LocalReport.DataSources.Clear();

            // IMPORTANT: name must match <DataSet Name="dsResult"> in the RDLC
            var rds = new ReportDataSource("dsResult", data);
            _rv.LocalReport.DataSources.Add(rds);

            _rv.SetDisplayMode(DisplayMode.PrintLayout);
            _rv.RefreshReport();
        }

        // Example helper: create DataTable from your NewResults collection and show the report.
        // Adapt callers to pass the real collection; this method is only illustrative.
        private void ShowExternalReportExample(System.Collections.IEnumerable newResults, string programName = "", string batch = "")
        {
            var table = new DataTable();
            table.Columns.Add("ProgramName", typeof(string));
            table.Columns.Add("Batch", typeof(string));
            table.Columns.Add("DateSent", typeof(string));
            table.Columns.Add("ReceivedAt", typeof(DateTime));
            table.Columns.Add("Sample", typeof(string));
            table.Columns.Add("DeviceName", typeof(string));
            table.Columns.Add("TestName", typeof(string));
            table.Columns.Add("Unit", typeof(string));
            table.Columns.Add("TempResult", typeof(string));
            table.Columns.Add("ReferenceValue", typeof(string));
            table.Columns.Add("SigmaP", typeof(string));
            table.Columns.Add("ZScore", typeof(double));
            table.Columns.Add("Status", typeof(string));
            table.Columns.Add("Notes", typeof(string));

            // newResults is expected to contain objects with Model-like properties used below.
            foreach (var item in newResults.Cast<dynamic>())
            {
                // handle nullable ReceivedAt and ZScore properly
                object receivedAt = item.Model.ReceivedAt.HasValue ? (object)item.Model.ReceivedAt.Value : DBNull.Value;
                object zscore = item.Model.ZScore.HasValue ? (object)item.Model.ZScore.Value : DBNull.Value;
                string statusText = item.Model.IsDefect == true ? "Không đạt" : (item.Model.IsDefect == false ? "Đạt" : "");

                table.Rows.Add(
                    programName,
                    batch,
                    item.Model.DateRun.ToString("dd/MM/yyyy"),
                    receivedAt,
                    item.Model.Sample ?? string.Empty,
                    item.Model.IdDeviceNavigation?.Name ?? string.Empty,
                    item.Model.IdTestNavigation?.Name ?? string.Empty,
                    item.Model.IdTestNavigation?.IdUnitTableNavigation?.Name ?? string.Empty,
                    item.Model.TempResult ?? string.Empty,
                    item.Model.ReferenceValue ?? string.Empty,
                    item.Model.SigmaP ?? string.Empty,
                    zscore,
                    statusText,
                    item.Model.Notes ?? string.Empty
                );
            }
            // Finally show report
            LoadReport(table);
        }
    }
}
