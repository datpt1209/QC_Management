using System;
using System.Collections.Generic;
using System.Linq;
using QC_Management.Models;

namespace QC_Management.Services
{
    public class LeveyResult
    {
        public List<string> ViolatedRules { get; } = new();
        public bool IsOutRange { get; set; }
        public bool IsOut2SD { get; set; }
    }

    public static class LeveyJenningsChecker
    {
        // Public helper: evaluate only a single named rule (main or detailed key).
        // Returns LeveyResult that contains only violations discovered for that rule.
        // This avoids running all checks when caller only wants one rule.
        public static LeveyResult EvaluateSingleRule(
            Result current,
            IEnumerable<Result>? sameLevelPrev,
            IEnumerable<Result>? crossLevelPrev,
            string ruleKey)
        {
            if (current == null) return new LeveyResult();

            // Normalize key to main/detailed canonical form used by internal checks
            var key = (ruleKey ?? string.Empty).Trim();

            // Build history lists (newest-first) but do not compute unnecessary aggregates here.
            var sameList = new List<Result> { current };
            if (sameLevelPrev != null) sameList.AddRange(sameLevelPrev);

            var crossList = new List<Result> { current };
            if (crossLevelPrev != null) crossList.AddRange(crossLevelPrev);

            var res = new LeveyResult();

            // --- Qualitative tests handling ---
            // If this is a qualitative test (ResultType != 2) only qualitative checks apply.
            if (current.ResultType != 2)
            {
                // Acceptable rule keys for qualitative check
                if (string.Equals(key, "QUAL", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(key, "QUAL_FAIL", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(key, "QUALITATIVE", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        var cid = current.IdControlDetailNavigation;
                        if (cid != null && !string.IsNullOrEmpty(current.TempResult))
                        {
                            bool acceptable = false;
                            try
                            {
                                acceptable = cid.IsQualitativeResultAcceptable(current.TempResult);
                            }
                            catch
                            {
                                // treat exceptions as unknown -> do not flag
                                acceptable = true;
                            }

                            if (!acceptable)
                            {
                                res.ViolatedRules.Add("QUAL");
                                res.IsOutRange = true; 
                                res.IsOut2SD = false;
                            }
                        }
                    }
                    catch
                    {
                        // non-fatal: leave res empty
                    }

                    return res;
                }
                // otherwise numeric Westgard rules do not apply to qualitative tests
                return res;
            }

            // single-point rules (quantitative)
            if (string.Equals(key, "1_3S", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(key, "1-3S", StringComparison.OrdinalIgnoreCase))
            {
                if (Check1_3S(current)) res.ViolatedRules.Add("1_3S");
                res.IsOut2SD = CheckIsOut2SD(current);
                res.IsOutRange = res.ViolatedRules.Any();
                return res;
            }

            if (string.Equals(key, "1_2S", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(key, "1-2S", StringComparison.OrdinalIgnoreCase))
            {
                // If current already violates 1_3S, skip 1_2S (no need to run redundant check).
                if (Check1_3S(current))
                {
                    res.IsOut2SD = CheckIsOut2SD(current);
                    res.IsOutRange = res.ViolatedRules.Any();
                    return res;
                }

                // Treat 1_2S as the generic >2SD check
                if (CheckIsOut2SD(current)) res.ViolatedRules.Add("1_2S");
                res.IsOut2SD = CheckIsOut2SD(current);
                res.IsOutRange = res.ViolatedRules.Any();
                return res;
            }

            // 2-2S main key expands to variants
            if (string.Equals(key, "2_2S", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(key, "2-2S", StringComparison.OrdinalIgnoreCase))
            {
                var v = Check2of2Variants(sameList, crossList);
                res.ViolatedRules.AddRange(v);
                res.IsOut2SD = CheckIsOut2SD(current);
                res.IsOutRange = res.ViolatedRules.Any();
                return res;
            }

            // R-4s
            if (string.Equals(key, "R-4s", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(key, "R-4S", StringComparison.OrdinalIgnoreCase))
            {
                if (CheckR4s(current, crossList)) res.ViolatedRules.Add("R-4s");
                res.IsOut2SD = CheckIsOut2SD(current);
                res.IsOutRange = res.ViolatedRules.Any();
                return res;
            }

            // 4-of-5 main key expands to variants
            if (string.Equals(key, "4_1S", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(key, "4-1S", StringComparison.OrdinalIgnoreCase))
            {
                var v = Check4of5Variants(sameList, crossList, current);
                res.ViolatedRules.AddRange(v);
                res.IsOut2SD = CheckIsOut2SD(current);
                res.IsOutRange = res.ViolatedRules.Any();
                return res;
            }

            // 10X main key expands to variants
            if (string.Equals(key, "10X", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(key, "10-X", StringComparison.OrdinalIgnoreCase))
            {
                var v = Check10xVariants(sameList, crossList, current);
                res.ViolatedRules.AddRange(v);
                res.IsOut2SD = CheckIsOut2SD(current);
                res.IsOutRange = res.ViolatedRules.Any();
                return res;
            }

            // If caller passed a detailed key (e.g. "4_1S (+) (L)"), map to the specific check
            switch (key.ToUpperInvariant())
            {
                case "2_2S (+) (L)":
                case "2_2S (-) (L)":
                case "2_2S (+) (CROSS)":
                case "2_2S (-) (CROSS)":
                    var v2 = Check2of2Variants(sameList, crossList);
                    if (v2.Contains(key, StringComparer.OrdinalIgnoreCase)) res.ViolatedRules.Add(key);
                    res.IsOut2SD = CheckIsOut2SD(current);
                    res.IsOutRange = res.ViolatedRules.Any();
                    return res;

                case "4_1S (+) (L)":
                case "4_1S (-) (L)":
                case "4_1S (L)":
                case "4_1S (+) (CROSS)":
                case "4_1S (-) (CROSS)":
                case "4_1S (CROSS)":
                    var v4 = Check4of5Variants(sameList, crossList, current);
                    if (v4.Any(x => string.Equals(x, key, StringComparison.OrdinalIgnoreCase))) res.ViolatedRules.Add(key);
                    res.IsOut2SD = CheckIsOut2SD(current);
                    res.IsOutRange = res.ViolatedRules.Any();
                    return res;

                case "10X (+) (L)":
                case "10X (-) (L)":
                case "10X (+) (CROSS)":
                case "10X (-) (CROSS)":
                    var v10 = Check10xVariants(sameList, crossList, current);
                    if (v10.Any(x => string.Equals(x, key, StringComparison.OrdinalIgnoreCase))) res.ViolatedRules.Add(key);
                    res.IsOut2SD = CheckIsOut2SD(current);
                    res.IsOutRange = res.ViolatedRules.Any();
                    return res;

                default:
                    // Unknown rule -> return empty result
                    return res;
            }
        }

        // Backwards-compatible Evaluate: if enabledRules is null runs all checks.
        // If enabledRules provided, iterate enabled main/detailed keys and aggregate results,
        // but each rule check will only run the logic necessary for that rule.
        public static LeveyResult Evaluate(
            Result current,
            IEnumerable<Result>? sameLevelPrev,
            IEnumerable<Result>? crossLevelPrev,
            IEnumerable<string>? enabledRules = null)
        {
            // --- Qualitative: if current is qualitative, only qualitative check applies ---
            if (current?.ResultType != 2)
            {
                var result = new LeveyResult();
                try
                {
                    var cid = current.IdControlDetailNavigation;
                    if (cid != null && !string.IsNullOrEmpty(current.TempResult))
                    {
                        bool acceptable = true;
                        try
                        {
                            acceptable = cid.IsQualitativeResultAcceptable(current.TempResult);
                        }
                        catch
                        {
                            // treat exceptions as acceptable (do not false-positive)
                            acceptable = true;
                        }

                        if (!acceptable)
                        {
                            result.ViolatedRules.Add("QUAL");
                            result.IsOutRange = true;
                        }
                    }
                }
                catch
                {
                    // swallow and return empty result
                }

                result.IsOut2SD = false;
                return result;
            }

            // If enabledRules is null: preserve previous behavior (evaluate all numeric rules).
            if (enabledRules == null)
            {
                // run each rule once and aggregate
                var result = new LeveyResult();

                // single-points: check 1_3S first; if violated, skip 1_2S
                if (Check1_3S(current)) result.ViolatedRules.Add("1_3S");
                else if (CheckIsOut2SD(current)) result.ViolatedRules.Add("1_2S");

                // 2-2S
                result.ViolatedRules.AddRange(Check2of2Variants(new List<Result> { current }.Concat(sameLevelPrev ?? Enumerable.Empty<Result>()).ToList(),
                                                                new List<Result> { current }.Concat(crossLevelPrev ?? Enumerable.Empty<Result>()).ToList()));

                // R-4s
                if (CheckR4s(current, new List<Result> { current }.Concat(crossLevelPrev ?? Enumerable.Empty<Result>()).ToList()))
                    result.ViolatedRules.Add("R-4s");

                // 4-of-5
                result.ViolatedRules.AddRange(Check4of5Variants(new List<Result> { current }.Concat(sameLevelPrev ?? Enumerable.Empty<Result>()).ToList(),
                                                               new List<Result> { current }.Concat(crossLevelPrev ?? Enumerable.Empty<Result>()).ToList(),
                                                               current));

                // 10X
                result.ViolatedRules.AddRange(Check10xVariants(new List<Result> { current }.Concat(sameLevelPrev ?? Enumerable.Empty<Result>()).ToList(),
                                                               new List<Result> { current }.Concat(crossLevelPrev ?? Enumerable.Empty<Result>()).ToList(),
                                                               current));

                // dedupe (preserve original insertion order; do not reorder or relocate 1_2S)
                var ordered = result.ViolatedRules
                                  .Distinct(StringComparer.OrdinalIgnoreCase)
                                  .ToList();

                result.ViolatedRules.Clear();
                result.ViolatedRules.AddRange(ordered);

                result.IsOut2SD = CheckIsOut2SD(current);
                // Consider only non-1_2S rules as out-of-range for IsOutRange
                result.IsOutRange = result.ViolatedRules.Any(r => !string.Equals(r, "1_2S", StringComparison.OrdinalIgnoreCase));
                return result;
            }

            // If enabledRules provided: call EvaluateSingleRule for each key and aggregate.
            var agg = new LeveyResult();
            foreach (var k in enabledRules)
            {
                var part = EvaluateSingleRule(current, sameLevelPrev, crossLevelPrev, k);
                foreach (var v in part.ViolatedRules)
                {
                    if (!agg.ViolatedRules.Contains(v, StringComparer.OrdinalIgnoreCase))
                        agg.ViolatedRules.Add(v);
                }
                agg.IsOut2SD = agg.IsOut2SD || part.IsOut2SD;
                agg.IsOutRange = agg.IsOutRange || part.IsOutRange;
            }

            // keep reported order as discovered, but remove duplicates
            var orderedAgg = agg.ViolatedRules.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            agg.ViolatedRules.Clear();
            agg.ViolatedRules.AddRange(orderedAgg);

            return agg;
        }

        // --- Individual rule check helpers (do minimal work) ---

        private static bool CheckIsOut2SD(Result current)
        {
            if (current?.ZScore == null) return false;
            return Math.Abs(current.ZScore.Value) > 2;
        }

        private static bool Check1_3S(Result current)
        {
            if (current?.ZScore == null) return false;
            return Math.Abs(current.ZScore.Value) > 3;
        }

        // returns detailed 2-of-2 variant keys that are violated
        private static List<string> Check2of2Variants(List<Result> sameList, List<Result> crossList)
        {
            var violations = new List<string>();
            // choose same-level if available otherwise cross-level
            var list = sameList.Count >= 2 ? sameList : crossList;
            if (list.Count < 2) return violations;

            var z = list.Take(2).Select(r => r?.ZScore ?? double.NaN).ToList();
            if (z.Any(double.IsNaN)) return violations;

            if (z.All(v => v > 2)) violations.Add("2_2S (+) (L)".Replace(" (L)", list == sameList ? " (L)" : " (cross)"));
            if (z.All(v => v < -2)) violations.Add("2_2S (-) (L)".Replace(" (L)", list == sameList ? " (L)" : " (cross)"));

            // ensure correct tag names:
            for (int i = 0; i < violations.Count; i++)
            {
                if (violations[i].EndsWith("(L)") == false && list == sameList) violations[i] = violations[i].Replace("(cross)", "(L)");
                if (violations[i].EndsWith("(cross)") == false && list != sameList) violations[i] = violations[i].Replace("(L)", "(cross)");
            }

            return violations;
        }

        private static bool CheckR4s(Result current, List<Result> crossList)
        {
            if (crossList == null || crossList.Count < 2) return false;
            var immediatePrev = crossList.ElementAtOrDefault(1);
            if (immediatePrev == null) return false;
            if (immediatePrev.DateRun.Date != current.DateRun.Date) return false;
            if (immediatePrev.IndexQc != current.IndexQc) return false;
            if (immediatePrev.IdLevel == current.IdLevel) return false;
            var zPeer = immediatePrev?.ZScore;
            if (zPeer == null || current.ZScore == null) return false;
            return Math.Abs(current.ZScore.Value - zPeer.Value) > 4;
        }

        // returns detailed 4-of-5 variant keys that are violated (may be empty)
        private static List<string> Check4of5Variants(List<Result> sameList, List<Result> crossList, Result current)
        {
            var violations = new List<string>();
            int sign = current?.ZScore >= 0 ? 1 : -1;

            // helper to evaluate a list of nearest 5
            List<string> evalFor(List<Result> list, string tag)
            {
                var outList = new List<string>();
                if (list.Count < 5) return outList;
                var last4 = list.Take(4).Select(r => r?.ZScore ?? double.NaN).ToList();
                if (last4.Any(double.IsNaN)) return outList;

                var pos = last4.Count(z => z > 1);
                var neg = last4.Count(z => z < -1);

                if (sign > 0)
                {
                    if (pos >= 4) outList.Add($"4_1S (+) {tag}");
                }
                else
                {
                    if (neg >= 4) outList.Add($"4_1S (-) {tag}");
                }

                return outList;
            }

            // prefer same-level
            var sameCandidates = evalFor(sameList, "(L)");
            if (sameCandidates.Any()) { violations.AddRange(sameCandidates); return violations; }

            var crossCandidates = evalFor(crossList, "(cross)");
            if (crossCandidates.Any()) violations.AddRange(crossCandidates);

            return violations;
        }

        // returns detailed 10X variant keys that are violated (may be empty)
        private static List<string> Check10xVariants(List<Result> sameList, List<Result> crossList, Result current)
        {
            var violations = new List<string>();
            int sign = current?.ZScore >= 0 ? 1 : -1;

            List<string> evalFor(List<Result> list, string tag)
            {
                var outList = new List<string>();
                if (list.Count < 11) return outList;
                var last11 = list.Take(10).Select(r => r?.ZScore ?? double.NaN).ToList();
                if (last11.Any(double.IsNaN)) return outList;

                var pos = last11.Count(z => z > 0);
                var neg = last11.Count(z => z < 0);

                if (sign > 0)
                {
                    if (pos >= 10) outList.Add($"10X (+) {tag}");
                }
                else
                {
                    if (neg >= 10) outList.Add($"10X (-) {tag}");
                }

                return outList;
            }

            var sameCandidates = evalFor(sameList, "(L)");
            if (sameCandidates.Any()) { violations.AddRange(sameCandidates); return violations; }

            var crossCandidates = evalFor(crossList, "(cross)");
            if (crossCandidates.Any()) violations.AddRange(crossCandidates);

            return violations;
        }
    }
}