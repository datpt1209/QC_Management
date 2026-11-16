using Microsoft.EntityFrameworkCore;
using QC_Management.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace QC_Management.Services
{
    public static class SigmaCalculator
    {
        public static string Grade(double s) =>
            s >= 6 ? "Đẳng cấp thế giới" : s >= 5 ? "Xuất sắc" : s >= 4 ? "Tốt"
            : s >= 3 ? "Biên" : "Không đạt";

        // Sample SD (n-1). Lưu ý HomeViewModel_V2 đang chia n (population SD)
        public static double? CvPercent(IReadOnlyList<double> v)
        {
            if (v.Count < 2) return null;
            var mean = v.Average();
            if (mean == 0) return null;
            var sd = Math.Sqrt(v.Sum(x => (x - mean) * (x - mean)) / (v.Count - 1));
            return sd / Math.Abs(mean) * 100.0;
        }

        static double? ParseNum(string? s) =>
            double.TryParse(s, NumberStyles.Float, CultureInfo.CurrentCulture, out var d) ||
            double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out d) ? d : null;

        // Rule Westgard đề xuất theo Sigma
        public static string[] SuggestRules(double sigma) =>
            sigma >= 6 ? new[] { "1_3S" } :
            sigma >= 5 ? new[] { "1_3S", "2_2S", "R-4s" } :
            sigma >= 4 ? new[] { "1_3S", "2_2S", "R-4s", "4_1S" } :
            sigma >= 3 ? new[] { "1_3S", "2_2S", "R-4s", "4_1S", "10X" } :
                         new[] { "1_2S", "1_3S", "2_2S", "R-4s", "4_1S", "10X" };

        public static List<SigmaMetric> Compute(QcManagmentContext db, int deviceId, int testId,
                                                DateTime from, DateTime to)
        {
            var list = new List<SigmaMetric>();
            var test = db.Tests.AsNoTracking().First(t => t.Id == testId);
            if (test.TestType != 2) return list;                      // chỉ định lượng

            var dt = db.DeviceTests.AsNoTracking()
                       .FirstOrDefault(d => d.IdDevice == deviceId && d.IdTest == testId);
            double? tea = dt?.TEaPercentOverride ?? test.TEaPercent;
            if (tea == null) return list;

            // --- Bias từ ngoại kiểm ---
            var biases = new List<double>();
            var eqa = db.ExternalResults.AsNoTracking()
                .Where(r => r.IdDevice == deviceId && r.IdTest == testId
                         && r.DateRun >= from && r.DateRun < to
                         && r.ReferenceValue != null && r.ReferenceValue != 0)
                .ToList();
            foreach (var r in eqa)
            {
                var x = ParseNum(r.TempResult);
                if (x.HasValue) biases.Add((x.Value - r.ReferenceValue!.Value) / r.ReferenceValue.Value * 100.0);
            }
            if (biases.Count == 0) return list;
            var meanBias = biases.Average();
            var absBias = Math.Abs(meanBias);

            // --- CV từ nội kiểm, tách theo level ---
            var iqc = db.Results.AsNoTracking()
                .Where(r => r.IdDevice == deviceId && r.IdTest == testId
                         && r.DateRun >= from && r.DateRun < to
                         && r.Result1 != null && r.IsExclude != true)
                .Select(r => new { r.IdLevel, Value = r.Result1.Value })
                .ToList();

            foreach (var g in iqc.GroupBy(x => x.IdLevel))
            {
                var vals = g.Select(x => x.Value).ToList();
                var cv = CvPercent(vals);
                if (cv is null or <= 0) continue;

                var sigma = (tea.Value - absBias) / cv.Value;
                list.Add(new SigmaMetric
                {
                    IdDevice = deviceId,
                    IdTest = testId,
                    IdLevel = g.Key,
                    PeriodStart = from,
                    PeriodEnd = to.Date.AddDays(-1),
                    TEaPercent = tea.Value,
                    BiasPercent = absBias,
                    BiasN = biases.Count,
                    CvPercent = cv.Value,
                    CvN = vals.Count,
                    Sigma = Math.Round(sigma, 2),
                    Qgi = Math.Round(absBias / (1.5 * cv.Value), 2),
                    Grade = Grade(sigma),
                    ComputedAt = DateTime.Now,
                    ComputedBy = UserManager.Instance?.CurrentUser?.DisplayName
                });
            }
            return list;
        }
    }
}