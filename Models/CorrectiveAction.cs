using System;

namespace QC_Management.Models
{
    public class CorrectiveAction
    {
        public int Id { get; set; }

        // Required link to the internal error being addressed
        public int InternalErrorId { get; set; }
        public InternalError? InternalError { get; set; }

        // Optional link to a resolving Result (existing saved Result)
        public int? ResolvingResultId { get; set; }
        public Result? ResolvingResult { get; set; }

        public string? ActionDescription { get; set; }
        public string? ActionOwner { get; set; }
        public DateTime? ActionCompletedAt { get; set; }
        public string? Outcome { get; set; } // e.g. "Pass", "Fail", "Resolved"

        // Note: Reason moved to InternalError.Cause (root cause stored on InternalError).
        // CorrectiveAction no longer stores Reason.

        // New: preventive action description (phòng ngừa)
        public string? PreventiveAction { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? CreatedBy { get; set; }
    }
}