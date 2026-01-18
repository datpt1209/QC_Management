using System;

namespace QC_Management.Models
{
    // Flattened row used by the report (fields match DataSet1 fields in InternalErrorsReportrt1.rdlc)
    public class InternalErrorReportRow
    {
        public int InternalErrorId { get; set; }
        public string? Device { get; set; }
        public string? Level { get; set; }
        public string? ErrorDescription { get; set; }
        public string? Cause { get; set; }
        public string? ActionDescription { get; set; }
        public string? ActionOwner { get; set; }
        public DateTime? ActionCompleteAt { get; set; }
        public string? Outcome { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? TestName { get; set; }
        public string? ReferenceRangeBefore { get; set; }
        public string? PreCorrectResult { get; set; }
        public string? PostCorrectResult { get; set; }
        public string? ReferenceRangeAfter { get; set; }
        public string? PreventiveAction { get; set; }
    }
}