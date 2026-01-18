using System;
using System.Collections.Generic;

namespace QC_Management.Models
{
    public class InternalError
    {
        public int Id { get; set; }

        // Link to the erroneous Result (if available)
        public int? ErroneousResultId { get; set; }
        public Result? ErroneousResult { get; set; }

        public int? TestId { get; set; }
        public Test? Test { get; set; }

        public int? DeviceId { get; set; }
        public Device? Device { get; set; }

        public int? ControlInfoDetailId { get; set; }
        public ControlInfoDetail? ControlInfoDetail { get; set; }

        public string? Lot { get; set; }
        public string? WestgardDescription { get; set; }
        public string? RelatedResultsJson { get; set; }

        // Cause / root reason belongs to the InternalError (primary source)
        public string? Cause { get; set; }

        // NOTE: RangeMin/RangeMax/MeanApp/SdApp were removed.
        //public double? CurMean { get; set; }
        //public double? CurSd { get; set; }

        public bool IsResolved { get; set; } = false;
        public string? Status { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? CreatedBy { get; set; }

        // Navigation: corrective actions addressing this internal error
        public virtual ICollection<CorrectiveAction> CorrectiveActions { get; set; } = new List<CorrectiveAction>();
    }
}