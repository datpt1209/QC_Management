using System;

namespace QC_Management.Models;

public class ExternalResult
{
    public int Id { get; set; }
    public int ExternalProgramId { get; set; }
    public virtual ExternalProgram ExternalProgram { get; set; } = null!;
    public string? Batch { get; set; }
    public DateTime DateRun { get; set; }
    public int? IdDevice { get; set; }
    public virtual Device? IdDeviceNavigation { get; set; }
    public int IdTest { get; set; }
    public virtual Test IdTestNavigation { get; set; } = null!;

    // User-entered values stored as strings
    public string? TempResult { get; set; }         // user-entered result text
    public string? ReferenceValue { get; set; }     // stored as text (user-entered)
    public string? SigmaP { get; set; }             // σp stored as text (user-entered)

    // Computed/persistent fields
    public double? ZScore { get; set; }
    public bool? IsDefect { get; set; }

    // Who evaluated / notes
    public string? EvaluatedBy { get; set; }
    public string? Notes { get; set; }

    // Sample and dates
    public string? Sample { get; set; }                  // "Mẫu"
    public DateTime? ReceivedAt { get; set; }            // "Ngày nhận kết quả"
    public DateTime? ResultSavedAt { get; set; }         // "Ngày lưu kết quả" (set on save)

    /// <summary>
    /// Evaluate ZScore / acceptance using user-supplied SigmaP or direct comparison for qualitative tests.
    /// - Quantitative (TestType == 2): ZScore = (Result - Reference) / SigmaP. If parsing fails or sigma==0 => ZScore=null.
    ///   IsDefect = |ZScore| >= defectZThreshold (default 2.0).
    /// - Qualitative (other types): ZScore = null; IsDefect = TempResult.Trim() != ReferenceValue.Trim() (case-insensitive).
    /// </summary>
    public void ApplyReferenceEvaluation(double defectZThreshold = 2.0)
    {
        ZScore = null;
        IsDefect = null;

        int? testType = IdTestNavigation?.TestType;

        if (testType == 2)
        {
            if (double.TryParse(TempResult, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.CurrentCulture, out var res)
                && double.TryParse(ReferenceValue, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.CurrentCulture, out var reference)
                && double.TryParse(SigmaP, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.CurrentCulture, out var sigma)
                && Math.Abs(sigma) > double.Epsilon)
            {
                var z = (res - reference) / sigma;
                // Round to 3 decimal places per request
                ZScore = Math.Round(z, 3);
                IsDefect = Math.Abs(z) >= defectZThreshold;
            }
            else
            {
                ZScore = null;
                IsDefect = null;
            }
        }
        else
        {
            if (!string.IsNullOrWhiteSpace(TempResult) && !string.IsNullOrWhiteSpace(ReferenceValue))
            {
                var t = TempResult.Trim();
                var r = ReferenceValue.Trim();
                IsDefect = !string.Equals(t, r, StringComparison.CurrentCultureIgnoreCase);
            }
            else
            {
                IsDefect = null;
            }
            ZScore = null;
        }
    }
}
