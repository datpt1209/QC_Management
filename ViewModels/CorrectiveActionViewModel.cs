using Microsoft.EntityFrameworkCore;
using QC_Management.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace QC_Management.ViewModels
{
    public class CorrectiveActionViewModel : BaseViewModel
    {
        // Use InternalError directly (no intermediate view item)
        public ObservableCollection<InternalError> ExistingIncidents { get; set; } = new();

        // New: resolving results (post-error results) to choose from
        public ObservableCollection<Result> ResolvingResults { get; } = new();

        public ObservableCollection<string> ActionOptions { get; } = new()
        {
            "Rửa/ làm sạch",
            "Đổi LOT",
            "Hiệu chuẩn lại",
            "Đào tạo nhân viên",
            "Khác"
        };

        public ObservableCollection<string> CorrectiveActionOptions { get; } = new ObservableCollection<string>
        {
            "Rửa máy",
            "Đổi LOT",
            "Hiệu chuẩn lại",
            "Thực hiện lại QC",
            "Bảo trì thiết bị",
            "Cập nhật Range mới",
            "Thay thế phụ tùng",
            "Cài đặt lại phần mềm",
            "Đào tạo nhân viên",
            "Thay đổi quy trình",
            "Kiểm tra chất lượng LOT",
            "Khác"
        };

        // Cause categories + details (you can extend this dictionary)
        private readonly Dictionary<string, List<string>> _causeDetails = new()
        {
            { "Thao tác", new List<string> { "Nhầm vị trí QC", "Nhầm nồng độ QC", "Không theo quy trình" } },
            { "Thuốc thử / hóa chất", new List<string> { "Hết date", "Hết hạn Onboard", "Biến tính", "Còn ít", "Đổi LOT thuốc thử" } },
            { "Mẫu QC", new List<string> { "Hòa nguyên không đúng", "Đổi LOT chưa cập nhật Range", "Hết Date","Mẫu QC còn ít" } },
            { "Mẫu Calib", new List<string> {"Hoàn nguyên không đúng","Đổi LOT", "Hết Date", "Hết hạn Calib", "Mẫu Calib bị hư" } },
            { "Thiết bị", new List<string> { "Bảo trì/Bảo dưỡng không đúng hạn", "Lỗi phần cứng", "Cần bảo trì" } },
            { "Điều kiện môi trường", new List<string> { "Nhiệt độ/ độ ẩm không đạt", "Lỗi lọc nữa" } },
            { "Khác", new List<string> { "Khác" } }
        };

        // Category combo
        public ObservableCollection<string> CauseCategoryOptions { get; } = new ObservableCollection<string>();
        // Detail combo populated when category selected
        public ObservableCollection<string> CauseDetailOptions { get; } = new ObservableCollection<string>();

        // suppression flag to avoid appending when we set selected items programmatically
        private bool _suppressCauseDetailAppend;

        // suppression flag to avoid appending preventive selection when set programmatically
        private bool _suppressPreventiveAppend;

        private string? _selectedCauseCategory;
        public string? SelectedCauseCategory
        {
            get => _selectedCauseCategory;
            set
            {
                if (_selectedCauseCategory == value) return;
                _selectedCauseCategory = value;
                OnPropertyChanged();

                // populate details for this category
                CauseDetailOptions.Clear();
                if (!string.IsNullOrEmpty(_selectedCauseCategory) && _causeDetails.TryGetValue(_selectedCauseCategory, out var details))
                {
                    foreach (var d in details) CauseDetailOptions.Add(d);
                }

                // reset selected detail without triggering append
                _suppressCauseDetailAppend = true;
                SelectedCauseDetail = CauseDetailOptions.FirstOrDefault();
                _suppressCauseDetailAppend = false;

                // If this change was performed by the user (not suppressed),
                // ensure a new line header "Category:" exists in ErrorReason.
                if (!_suppressCauseDetailAppend && !string.IsNullOrWhiteSpace(_selectedCauseCategory))
                {
                    ErrorReason = EnsureCategoryHeader(ErrorReason, _selectedCauseCategory);
                }
            }
        }

        private string? _selectedCauseDetail;
        public string? SelectedCauseDetail
        {
            get => _selectedCauseDetail;
            set
            {
                var previous = _selectedCauseDetail;
                if (previous == value) return;
                _selectedCauseDetail = value;
                OnPropertyChanged();

                // When user selects a detail (and not suppressed),
                // append that detail into the existing "Category: ..." line,
                // or create the line if missing. Multiple details for the same
                // category are stored comma-separated on the same line.
                if (!_suppressCauseDetailAppend && !string.IsNullOrWhiteSpace(_selectedCauseDetail))
                {
                    var category = SelectedCauseCategory?.Trim() ?? string.Empty;
                    var detail = _selectedCauseDetail.Trim();

                    if (string.IsNullOrEmpty(category))
                    {
                        // fallback: append as a simple line if no category
                        ErrorReason = AppendOrAddLine(ErrorReason, detail);
                    }
                    else
                    {
                        ErrorReason = AppendDetailToCategory(ErrorReason, category, detail);
                    }
                }
            }
        }

        private InternalError? _selectedExistingIncident;
        public InternalError? SelectedExistingIncident
        {
            get => _selectedExistingIncident;
            set
            {
                _selectedExistingIncident = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(SelectedExistingIncidentRange));
                OnPropertyChanged(nameof(SelectedExistingIncidentCreatedAt));
                OnPropertyChanged(nameof(SelectedExistingIncidentCreatedBy));

                // Attempt immediate compute from whatever navigation is present
                var (computedMin, computedMax) = ComputeRangeFromIncident(_selectedExistingIncident);
                RangeMin = computedMin;
                RangeMax = computedMax;

                // If ErroneousResult navigation is not populated but ErroneousResultId exists,
                // load it in background so UI bindings (ErroneousResult.TempResult etc.) work.
                if (_selectedExistingIncident != null && _selectedExistingIncident.ErroneousResult == null && _selectedExistingIncident.ErroneousResultId.HasValue)
                {
                    // fire-and-forget background load (will update UI when done)
                    _ = EnsureErroneousResultLoadedAsync(_selectedExistingIncident);
                }

                // If ControlInfoDetail navigation is not populated but id exists, load it (background)
                if (_selectedExistingIncident != null && _selectedExistingIncident.ControlInfoDetail == null && _selectedExistingIncident.ControlInfoDetailId.HasValue)
                {
                    _ = EnsureControlInfoDetailLoadedAsync(_selectedExistingIncident);
                }

                // Load available resolving results for this incident (fire-and-forget)
                if (_selectedExistingIncident != null)
                {
                    _ = LoadResolvingResultsAsync(_selectedExistingIncident);
                }
                else
                {
                    ResolvingResults.Clear();
                    SelectedResolvingResult = null;
                }

                // Load cause (primary) from InternalError.Cause when available; otherwise clear or show context actions.
                if (_selectedExistingIncident != null)
                {
                    // try to parse stored Cause into category/detail if possible
                    var cause = _selectedExistingIncident.Cause ?? string.Empty;
                    if (!string.IsNullOrWhiteSpace(cause))
                    {
                        // if stored string matches any detail, set category accordingly (programmatic only)
                        var matched = false;
                        foreach (var kv in _causeDetails)
                        {
                            if (kv.Value.Any(d => string.Equals(d, cause, StringComparison.OrdinalIgnoreCase)))
                            {
                                _suppressCauseDetailAppend = true;
                                SelectedCauseCategory = kv.Key;
                                SelectedCauseDetail = kv.Value.FirstOrDefault(d => string.Equals(d, cause, StringComparison.OrdinalIgnoreCase));
                                _suppressCauseDetailAppend = false;
                                // set textbox to existing cause (keep as list)
                                ErrorReason = cause;
                                matched = true;
                                break;
                            }
                        }

                        if (!matched)
                        {
                            // if stored string contains " - " assume "Category - Detail"
                            if (cause.Contains(" - "))
                            {
                                var parts = cause.Split(new[] { " - " }, StringSplitOptions.RemoveEmptyEntries);
                                if (parts.Length >= 2)
                                {
                                    var cat = parts[0].Trim();
                                    var det = parts[1].Trim();
                                    if (_causeDetails.ContainsKey(cat))
                                    {
                                        _suppressCauseDetailAppend = true;
                                        SelectedCauseCategory = cat;
                                        SelectedCauseDetail = _causeDetails[cat].FirstOrDefault(d => string.Equals(d, det, StringComparison.OrdinalIgnoreCase)) ?? det;
                                        _suppressCauseDetailAppend = false;
                                        ErrorReason = cause;
                                    }
                                    else
                                    {
                                        // fallback: set custom detail into ErrorReason
                                        _suppressCauseDetailAppend = true;
                                        SelectedCauseCategory = _causeDetails.Keys.FirstOrDefault();
                                        SelectedCauseDetail = null;
                                        _suppressCauseDetailAppend = false;
                                        ErrorReason = cause;
                                    }
                                }
                                else
                                {
                                    _suppressCauseDetailAppend = true;
                                    SelectedCauseCategory = _causeDetails.Keys.FirstOrDefault();
                                    SelectedCauseDetail = null;
                                    _suppressCauseDetailAppend = false;
                                    ErrorReason = cause;
                                }
                            }
                            else
                            {
                                // treat entire cause as custom detail (put into textbox)
                                _suppressCauseDetailAppend = true;
                                SelectedCauseCategory = _causeDetails.Keys.FirstOrDefault();
                                SelectedCauseDetail = null;
                                _suppressCauseDetailAppend = false;
                                ErrorReason = cause;
                            }
                        }
                    }
                    else
                    {
                        // no stored cause
                        ErrorReason = string.Empty;
                        _suppressCauseDetailAppend = true;
                        SelectedCauseCategory = _causeDetails.Keys.FirstOrDefault();
                        SelectedCauseDetail = CauseDetailOptions.FirstOrDefault();
                        _suppressCauseDetailAppend = false;
                    }

                    // still populate previous corrective/preventive action text for context (no Reason stored on CorrectiveAction)
                    var latestCa = _selectedExistingIncident.CorrectiveActions?.OrderByDescending(c => c.CreatedAt).FirstOrDefault();
                    CorrectiveAction = latestCa?.ActionDescription ?? string.Empty;
                    PreventiveAction = latestCa?.PreventiveAction ?? string.Empty;

                    SelectedAction = ActionOptions.Contains(CorrectiveAction) ? CorrectiveAction : "Khác";

                    // set SelectedPreventiveAction programmatically without triggering append
                    _suppressPreventiveAppend = true;
                    SelectedPreventiveAction = ActionOptions.Contains(PreventiveAction) ? PreventiveAction : null;
                    _suppressPreventiveAppend = false;
                }
                else
                {
                    ErrorReason = string.Empty;
                    CorrectiveAction = string.Empty;
                    PreventiveAction = string.Empty;
                    SelectedAction = "Khác";
                    // set SelectedPreventiveAction programmatically without triggering append
                    _suppressPreventiveAppend = true;
                    SelectedPreventiveAction = ActionOptions.Contains(PreventiveAction) ? PreventiveAction : null;
                    _suppressPreventiveAppend = false;

                    _suppressCauseDetailAppend = true;
                    SelectedCauseCategory = _causeDetails.Keys.FirstOrDefault();
                    SelectedCauseDetail = null;
                    _suppressCauseDetailAppend = false;
                }

                // Evaluate current ResultValue against the newly selected incident's range
                EvaluateCorrectiveResult();

                // If evaluation yields true (in-range), auto-check MarkResolved.
                MarkResolved = IsCorrectiveResultInRange == true;
            }
        }

        // Selected resolving Result (post-error)
        private Result? _selectedResolvingResult;
        public Result? SelectedResolvingResult
        {
            get => _selectedResolvingResult;
            set
            {
                if (_selectedResolvingResult == value) return;
                _selectedResolvingResult = value;
                OnPropertyChanged();

                // Re-evaluate status when user changes resolving result selection
                EvaluateResolvingResult();

                // Auto-check checkbox when resolving result is in-range
                if (IsCorrectiveResultInRange == true)
                    MarkResolved = true;
            }
        }

        // Ensure ErroneousResult and its navigations are loaded for the provided InternalError.
        private async Task EnsureErroneousResultLoadedAsync(InternalError ie)
        {
            try
            {
                if (ie == null) return;
                if (ie.ErroneousResult != null) return;
                if (!ie.ErroneousResultId.HasValue) return;

                using var db = new QcManagmentContext();
                var res = await db.Results
                    .AsNoTracking()
                    .Include(r => r.IdControlDetailNavigation)
                    .Include(r => r.IdLevelNavigation)
                    .Include(r => r.IdTestNavigation)
                    .Include(r => r.IdDeviceNavigation)
                    .FirstOrDefaultAsync(r => r.Id == ie.ErroneousResultId.Value);

                if (res != null)
                {
                    // assign navigation onto the existing InternalError instance on UI thread
                    Application.Current?.Dispatcher.Invoke(() =>
                    {
                        ie.ErroneousResult = res;
                        // notify UI that SelectedExistingIncident and dependent properties changed
                        OnPropertyChanged(nameof(SelectedExistingIncident));
                        OnPropertyChanged(nameof(SelectedExistingIncidentRange));
                    });
                }
            }
            catch
            {
                // swallow any errors — loading navigation is best-effort
            }
        }

        // Ensure ControlInfoDetail navigation is loaded for the given InternalError instance.
        // When loaded, update RangeMin/RangeMax on the UI thread so bindings refresh.
        private async Task EnsureControlInfoDetailLoadedAsync(InternalError ie)
        {
            try
            {
                if (ie == null) return;
                if (ie.ControlInfoDetail != null) return;
                if (!ie.ControlInfoDetailId.HasValue) return;

                using var db = new QcManagmentContext();
                var cid = await db.ControlInfoDetails
                    .AsNoTracking()
                    .Include(c => c.IdControlInfoNavigation)
                    .Include(c => c.IdDeviceNavigation)
                    .Include(c => c.IdLevelNavigation)
                    .Include(c => c.IdTestNavigation)
                    .FirstOrDefaultAsync(c => c.Id == ie.ControlInfoDetailId.Value);

                if (cid != null)
                {
                    Application.Current?.Dispatcher.Invoke(() =>
                    {
                        ie.ControlInfoDetail = cid;
                        // recompute and apply range for the currently selected incident
                        if (ReferenceEquals(ie, SelectedExistingIncident))
                        {
                            var (min, max) = ComputeRangeFromIncident(ie);
                            RangeMin = min;
                            RangeMax = max;
                            OnPropertyChanged(nameof(SelectedExistingIncidentRange));
                        }
                    });
                }
            }
            catch
            {
                // swallow — best-effort load
            }
        }

        // load candidates for resolving result (post-error results) related to the incident's test/device
        private async Task LoadResolvingResultsAsync(InternalError incident)
        {
            try
            {
                ResolvingResults.Clear();
                if (incident == null) return;

                using var db = new QcManagmentContext();
                // start query and eagerly include navigations we'll need for evaluation/reporting
                var q = db.Results
                          .AsNoTracking()
                          .Include(r => r.IdControlDetailNavigation)
                          .Include(r => r.IdLevelNavigation)
                          .Include(r => r.IdTestNavigation)
                          .Include(r => r.IdDeviceNavigation)
                          .Include(r => r.IdUserNavigation)
                          .AsQueryable();

                // same Test
                if (incident.TestId.HasValue)
                    q = q.Where(r => r.IdTest == incident.TestId.Value);

                // same Device
                if (incident.DeviceId.HasValue)
                    q = q.Where(r => r.IdDevice == incident.DeviceId.Value);

                // determine level from ErroneousResult if available (try navigation first, otherwise query DB)
                int? levelId = null;
                if (incident.ErroneousResult != null)
                {
                    levelId = incident.ErroneousResult.IdLevel;
                }
                else if (incident.ErroneousResultId.HasValue)
                {
                    levelId = await db.Results
                        .AsNoTracking()
                        .Where(r => r.Id == incident.ErroneousResultId.Value)
                        .Select(r => (int?)r.IdLevel)
                        .FirstOrDefaultAsync();
                }

                // same Level (if known)
                if (levelId.HasValue)
                    q = q.Where(r => r.IdLevel == levelId.Value);

                // prefer results on/after incident date and most recent first
                var startDate = incident.CreatedAt.Date;
                var list = await q
                    .Where(r => r.DateRun >= startDate)
                    .OrderByDescending(r => r.DateRun)
                    .ThenByDescending(r => r.Id)
                    .Take(100)
                    .ToListAsync();

                // populate collection on UI thread to avoid cross-thread issues
                Application.Current?.Dispatcher.Invoke(() =>
                {
                    foreach (var r in list) ResolvingResults.Add(r);

                    // select initial resolving result when available
                    if (_resolvingResultId.HasValue)
                        SelectedResolvingResult = ResolvingResults.FirstOrDefault(x => x.Id == _resolvingResultId.Value);
                    else
                        SelectedResolvingResult = ResolvingResults.FirstOrDefault();
                });
            }
            catch
            {
                // ignore resolution load failures
            }
        }

        public string SelectedExistingIncidentRange
        {
            get
            {
                var s = SelectedExistingIncident;
                if (s == null) return string.Empty;

                // prefer ControlInfoDetail's current mean/sd, fallback to InternalError's stored mean/sd
                var cid = s.ControlInfoDetail;
                double? mean = cid?.CurMean ;
                double? sd = cid?.CurSd;
                if (mean.HasValue && sd.HasValue)
                {
                    var lower = mean.Value - 2 * sd.Value;
                    var upper = mean.Value + 2 * sd.Value;
                    return $"{lower.ToString("N2")} - {upper.ToString("N2")}";
                }
                return string.Empty;
            }
        }

        public string SelectedExistingIncidentCreatedBy => SelectedExistingIncident?.CreatedBy ?? string.Empty;

        public DateTime? SelectedExistingIncidentCreatedAt
        {
            get
            {
                var dt = SelectedExistingIncident?.CreatedAt;
                if (!dt.HasValue) return null;
                if (dt.Value == default(DateTime)) return null;
                return dt;
            }
        }

        // helper: compute range bounds (mean ± 2*sd) from an InternalError
        // now prefers: InternalError.ControlInfoDetail -> InternalError.ErroneousResult.IdControlDetailNavigation
        private static (double? min, double? max) ComputeRangeFromIncident(InternalError? ie)
        {
            if (ie == null) return (null, null);

            // prefer the explicit ControlInfoDetail navigation when available
            ControlInfoDetail? cid = ie.ControlInfoDetail;

            // fallback to ErroneousResult's control detail navigation if present
            if (cid == null && ie.ErroneousResult != null)
                cid = ie.ErroneousResult.IdControlDetailNavigation;

            if (cid == null) return (null, null);

            // prefer current mean/sd, fallback to manufacturer or app means if present
            double? mean = cid.CurMean ?? cid.MeanNsx ?? cid.MeanApp;
            double? sd = cid.CurSd ?? cid.SdNsx ?? cid.SdApp;

            if (mean.HasValue && sd.HasValue)
            {
                var lower = mean.Value - 2 * sd.Value;
                var upper = mean.Value + 2 * sd.Value;
                return (lower, upper);
            }

            return (null, null);
        }

        private bool _markResolved;
        public bool MarkResolved
        {
            get => _markResolved;
            set { _markResolved = value; OnPropertyChanged(); }
        }

        public bool MarkAsResolved
        {
            get => MarkResolved;
            set => MarkResolved = value;
        }

        private DateTime _date;
        public DateTime? Date
        {
            get => _date == default ? (DateTime?)null : _date;
            set { _date = value ?? DateTime.Now; OnPropertyChanged(); }
        }

        private string _reporter = string.Empty;
        public string Reporter
        {
            get => _reporter;
            set { _reporter = value; OnPropertyChanged(); }
        }

        private string _deviceName = string.Empty;
        public string DeviceName
        {
            get => _deviceName;
            set { _deviceName = value; OnPropertyChanged(); }
        }

        private string _testName = string.Empty;
        public string TestName
        {
            get => _testName;
            set { _testName = value; OnPropertyChanged(); }
        }

        private string _leveyJenningsError = string.Empty;
        public string LeveyJenningsError
        {
            get => _leveyJenningsError;
            set { _leveyJenningsError = value; OnPropertyChanged(); }
        }

        private string _resultValue = string.Empty;
        public string ResultValue
        {
            get => _resultValue;
            set
            {
                _resultValue = value;
                OnPropertyChanged();
                EvaluateCorrectiveResult();
            }
        }

        public string RangeText
        {
            get
            {
                if (RangeMin.HasValue || RangeMax.HasValue)
                {
                    if (RangeMin.HasValue && RangeMax.HasValue)
                        return $"{RangeMin.Value.ToString("N2")} - {RangeMax.Value.ToString("N2")}";
                    if (RangeMin.HasValue)
                        return RangeMin.Value.ToString("N2");
                    return RangeMax.Value.ToString("N2");
                }
                return string.Empty;
            }
        }

        private double? _rangeMin;
        public double? RangeMin
        {
            get => _rangeMin;
            set
            {
                _rangeMin = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(RangeText));
                EvaluateCorrectiveResult();

                // When range changes re-evaluate selected resolving result as well
                EvaluateResolvingResult();
            }
        }

        private double? _rangeMax;
        public double? RangeMax
        {
            get => _rangeMax;
            set
            {
                _rangeMax = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(RangeText));
                EvaluateCorrectiveResult();

                // When range changes re-evaluate selected resolving result as well
                EvaluateResolvingResult();
            }
        }

        private string _correctiveAction = string.Empty;
        public string CorrectiveAction
        {
            get => _correctiveAction;
            set { _correctiveAction = value; OnPropertyChanged(); }
        }

        // preventive action: new UI-bound field
        private string _preventiveAction = string.Empty;
        public string PreventiveAction
        {
            get => _preventiveAction;
            set { _preventiveAction = value; OnPropertyChanged(); }
        }

        // user-editable reason (textbox) - now stores newline-separated "Category: detail1, detail2" lines
        private string _errorReason = string.Empty;
        public string ErrorReason
        {
            get => _errorReason;
            set { _errorReason = value; OnPropertyChanged(); }
        }

        // Helper: check if textbox already contains the token as a whole item (comma- or newline-separated)
        private bool ContainsToken(string? field, string token)
        {
            if (string.IsNullOrWhiteSpace(token)) return false;
            if (string.IsNullOrWhiteSpace(field)) return false;

            var parts = field.Split(new[] { ',', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                             .Select(p => p.Trim())
                             .Where(p => !string.IsNullOrEmpty(p))
                             .ToList();
            return parts.Any(p => string.Equals(p, token.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        // Helper: append token with comma separator, keep existing text, trim extra separators
        private string AppendToken(string? field, string token)
        {
            var t = token?.Trim();
            if (string.IsNullOrEmpty(t)) return field ?? string.Empty;

            var baseText = field?.Trim() ?? string.Empty;

            // remove trailing commas/spaces/newlines
            baseText = baseText.TrimEnd(',', ' ', '\n', '\r');

            if (string.IsNullOrEmpty(baseText))
                return t;

            return baseText + ", " + t;
        }

        // Helpers to manage "Category: detail1, detail2" lines inside ErrorReason

        // Ensure there's a header line "Category:" present (no detail). If missing, append as new line.
        private string EnsureCategoryHeader(string? field, string category)
        {
            var baseText = field?.Trim() ?? string.Empty;
            var header = $"{category}:";

            var lines = string.IsNullOrEmpty(baseText)
                ? new List<string>()
                : baseText.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                          .Select(l => l.Trim())
                          .Where(l => !string.IsNullOrEmpty(l))
                          .ToList();

            if (!lines.Any(l => l.StartsWith(header, StringComparison.OrdinalIgnoreCase)))
            {
                if (string.IsNullOrEmpty(baseText))
                    return header;
                return baseText + Environment.NewLine + header;
            }

            return baseText;
        }

        // Append a detail to an existing category line (comma-separated) or create the line if missing.
        private string AppendDetailToCategory(string? field, string category, string detail)
        {
            var baseText = field?.Trim() ?? string.Empty;
            var header = $"{category}:";

            var lines = string.IsNullOrEmpty(baseText)
                ? new List<string>()
                : baseText.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                          .Select(l => l.Trim())
                          .Where(l => !string.IsNullOrEmpty(l))
                          .ToList();

            var idx = lines.FindIndex(l => l.StartsWith(header, StringComparison.OrdinalIgnoreCase));

            if (idx >= 0)
            {
                var line = lines[idx];
                var after = line.Substring(header.Length).Trim(); // existing details portion

                if (string.IsNullOrEmpty(after))
                {
                    // no details yet -> add single detail
                    lines[idx] = $"{header} {detail}";
                }
                else
                {
                    // split existing details by comma and normalize
                    var existing = after.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                        .Select(x => x.Trim())
                                        .Where(x => !string.IsNullOrEmpty(x))
                                        .ToList();

                    // add only if not duplicate
                    if (!existing.Any(e => string.Equals(e, detail, StringComparison.OrdinalIgnoreCase)))
                    {
                        existing.Add(detail);
                        lines[idx] = $"{header} {string.Join(", ", existing)}";
                    }
                }
            }
            else
            {
                // no header line -> add with detail
                lines.Add($"{header} {detail}");
            }

            return string.Join(Environment.NewLine, lines);
        }

        // Fallback: append a plain line when no category is available
        private string AppendOrAddLine(string? field, string line)
        {
            var baseText = field?.Trim() ?? string.Empty;
            if (string.IsNullOrEmpty(baseText)) return line;
            return baseText + Environment.NewLine + line;
        }

        private string? _selectedAction;
        public string? SelectedAction
        {
            get => _selectedAction;
            set
            {
                _selectedAction = value;
                OnPropertyChanged();
                if (!string.IsNullOrWhiteSpace(_selectedAction) && _selectedAction != "Khác")
                {
                    // only append when textbox doesn't already contain the token
                    if (!ContainsToken(CorrectiveAction, _selectedAction))
                        CorrectiveAction = AppendToken(CorrectiveAction, _selectedAction);
                }
            }
        }

        // Selected preventive placeholder (bound in XAML)
        private string? _selectedPreventiveAction;
        public string? SelectedPreventiveAction
        {
            get => _selectedPreventiveAction;
            set
            {
                if (_selectedPreventiveAction == value) return;
                _selectedPreventiveAction = value;
                OnPropertyChanged();

                // do not append when set programmatically
                if (_suppressPreventiveAppend) return;

                // Append selected preventive action into PreventiveAction textbox (avoid duplicates)
                if (!string.IsNullOrWhiteSpace(_selectedPreventiveAction) && _selectedPreventiveAction != "Khác")
                {
                    if (!ContainsToken(PreventiveAction, _selectedPreventiveAction))
                        PreventiveAction = AppendToken(PreventiveAction, _selectedPreventiveAction);
                }
            }
        }

        // result evaluation
        private bool? _isCorrectiveResultInRange;
        public bool? IsCorrectiveResultInRange
        {
            get => _isCorrectiveResultInRange;
            private set
            {
                if (_isCorrectiveResultInRange != value)
                {
                    _isCorrectiveResultInRange = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(CorrectiveResultStatus));
                }
            }
        }

        public string CorrectiveResultStatus
            => IsCorrectiveResultInRange.HasValue ? (IsCorrectiveResultInRange.Value ? "Đạt" : "Không đạt") : string.Empty;

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public event Action<bool>? RequestClose;

        private readonly int? _resolvingResultId;
        private readonly int? _initialInternalErrorId;

        // New: id of corrective action being edited (when set, Save updates instead of creating)
        private readonly int? _editingCorrectiveActionId;

        // Accept InternalError collection directly — no wrapper class
        public CorrectiveActionViewModel(
            DateTime? date = null,
            string? reporter = null,
            string? deviceName = null,
            string? testName = null,
            string? levey = null,
            string? resultValue = null,
            double? rangeMin = null,
            double? rangeMax = null,
            IEnumerable<InternalError>? existingErrors = null,
            int? resolvingResultId = null,
            int? initialInternalErrorId = null,
            int? editingCorrectiveActionId = null) // added parameter
        {
            Date = date ?? DateTime.Now;
            Reporter = reporter ?? (UserManager.Instance?.CurrentUser?.DisplayName ?? "Unknown");
            DeviceName = deviceName ?? string.Empty;
            TestName = testName ?? string.Empty;
            LeveyJenningsError = levey ?? string.Empty;
            ResultValue = resultValue ?? string.Empty;
            RangeMin = rangeMin;
            RangeMax = rangeMax;
            _resolvingResultId = resolvingResultId;
            _initialInternalErrorId = initialInternalErrorId;
            _editingCorrectiveActionId = editingCorrectiveActionId;

            // populate cause categories
            foreach (var k in _causeDetails.Keys) CauseCategoryOptions.Add(k);
            _suppressCauseDetailAppend = true;
            SelectedCauseCategory = _causeDetails.Keys.FirstOrDefault();
            _suppressCauseDetailAppend = false;

            if (existingErrors != null)
            {
                foreach (var e  in existingErrors.OrderByDescending(x => x.CreatedAt).Take(200))
                    ExistingIncidents.Add(e);
                if (_initialInternalErrorId.HasValue)
                    SelectedExistingIncident = ExistingIncidents.FirstOrDefault(i => i.Id == _initialInternalErrorId.Value);
            }

            // if editing an existing corrective action, load its details (fire-and-forget)
            if (_editingCorrectiveActionId.HasValue)
            {
                _ = LoadEditingCorrectiveActionAsync(_editingCorrectiveActionId.Value);
            }

            // initial evaluation in case constructor provided values
            EvaluateCorrectiveResult();

            // auto-check when in range
            if (IsCorrectiveResultInRange == true)
                MarkResolved = true;

            SelectedAction = ActionOptions.Contains(CorrectiveAction) ? CorrectiveAction : "Khác";

            SaveCommand = new RelayCommand<object>(p => true, async p => await SaveAsync());

            CancelCommand = new RelayCommand<object>(p => true,
                p =>
                {
                    try
                    {
                        RequestClose?.Invoke(false);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error while cancelling: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                });
        }

        // Load corrective action being edited and populate UI fields (best-effort, non-blocking)
        private async Task LoadEditingCorrectiveActionAsync(int editingId)
        {
            try
            {
                using var db = new QcManagmentContext();
                var ca = await db.CorrectiveActions
                    .AsNoTracking()
                    .Include(c => c.InternalError).ThenInclude(i => i.Test)
                    .Include(c => c.InternalError).ThenInclude(i => i.Device)
                    .Include(c => c.ResolvingResult).ThenInclude(r => r.IdControlDetailNavigation)
                    .Include(c => c.ResolvingResult).ThenInclude(r => r.IdLevelNavigation)
                    .Include(c => c.ResolvingResult).ThenInclude(r => r.IdUserNavigation)
                    .FirstOrDefaultAsync(c => c.Id == editingId);

                if (ca == null) return;

                // Make sure the internal error is present in ExistingIncidents so SelectedExistingIncident can reference it
                Application.Current?.Dispatcher.Invoke(() =>
                {
                    if (ca.InternalError != null)
                    {
                        var existing = ExistingIncidents.FirstOrDefault(x => x.Id == ca.InternalError.Id);
                        if (existing == null)
                        {
                            // add to top for context
                            ExistingIncidents.Insert(0, ca.InternalError);
                        }
                        SelectedExistingIncident = ExistingIncidents.FirstOrDefault(x => x.Id == ca.InternalError.Id);
                    }

                    // populate cause from InternalError if present
                    if (ca.InternalError != null && !string.IsNullOrWhiteSpace(ca.InternalError.Cause))
                    {
                        ErrorReason = ca.InternalError.Cause;
                        // try parsing cause into category/detail if possible (reuse selection logic)
                        var cause = ca.InternalError.Cause;
                        var matched = false;
                        foreach (var kv in _causeDetails)
                        {
                            if (kv.Value.Any(d => string.Equals(d, cause, StringComparison.OrdinalIgnoreCase)))
                            {
                                _suppressCauseDetailAppend = true;
                                SelectedCauseCategory = kv.Key;
                                SelectedCauseDetail = kv.Value.FirstOrDefault(d => string.Equals(d, cause, StringComparison.OrdinalIgnoreCase));
                                _suppressCauseDetailAppend = false;
                                matched = true;
                                break;
                            }
                        }
                        if (!matched)
                        {
                            _suppressCauseDetailAppend = true;
                            SelectedCauseCategory = _causeDetails.Keys.FirstOrDefault();
                            SelectedCauseDetail = null;
                            _suppressCauseDetailAppend = false;
                        }
                    }

                    // populate corrective/preventive action fields from corrective action entity
                    CorrectiveAction = ca.ActionDescription ?? string.Empty;
                    PreventiveAction = ca.PreventiveAction ?? string.Empty;
                    SelectedAction = ActionOptions.Contains(CorrectiveAction) ? CorrectiveAction : "Khác";

                    // mark resolved state based on ActionCompletedAt
                    MarkResolved = ca.ActionCompletedAt.HasValue;

                    // set reporter/date from the corrective action (edit context)
                    Reporter = ca.CreatedBy ?? Reporter;
                    Date = ca.CreatedAt != default ? ca.CreatedAt : Date;

                    // set resolving result id so LoadResolvingResultsAsync will pick it when resolving list loads
                    // (constructor already saved _resolvingResultId from parameter; if not provided, set SelectedResolvingResult after load)
                });

                // evaluate resolving result numeric into ResultValue if present
                var resolving = ca.ResolvingResult;
                if (resolving != null)
                {
                    if (resolving.Result1.HasValue)
                        ResultValue = resolving.Result1.Value.ToString("0.###");
                    else if (!string.IsNullOrWhiteSpace(resolving.TempResult))
                        ResultValue = resolving.TempResult;
                }
            }
            catch
            {
                // ignore load errors (best effort)
            }
        }

        private void EvaluateCorrectiveResult()
        {
            bool? newVal = null;

            if (string.IsNullOrWhiteSpace(ResultValue))
            {
                IsCorrectiveResultInRange = null;
                // when we cannot determine, ensure checkbox is unchecked
                MarkResolved = false;
                return;
            }

            if (!double.TryParse(ResultValue, System.Globalization.NumberStyles.Float | System.Globalization.NumberStyles.AllowThousands, CultureInfo.CurrentCulture, out var parsed))
            {
                IsCorrectiveResultInRange = null;
                MarkResolved = false;
                return;
            }

            if (!RangeMin.HasValue && !RangeMax.HasValue)
            {
                newVal = null;
            }
            else
            {
                var min = RangeMin;
                var max = RangeMax;
                bool meetsMin = !min.HasValue || parsed >= min.Value;
                bool meetsMax = !max.HasValue || parsed <= max.Value;
                newVal = meetsMin && meetsMax;
            }

            IsCorrectiveResultInRange = newVal;
            // update checkbox: check only when explicitly in-range; otherwise uncheck
            MarkResolved = IsCorrectiveResultInRange == true;
        }

        // Evaluate the currently selected resolving result (SelectedResolvingResult).
        // Uses Result1 if present, otherwise tries to parse TempResult.
        private void EvaluateResolvingResult()
        {
            var r = SelectedResolvingResult;
            if (r == null)
            {
                // no selection -> fall back to evaluating the manual ResultValue textbox
                EvaluateCorrectiveResult();
                return;
            }

            double? numeric = null;

            if (r.Result1.HasValue)
            {
                numeric = r.Result1.Value;
            }
            else if (!string.IsNullOrWhiteSpace(r.TempResult))
            {
                // try parse TempResult
                if (double.TryParse(r.TempResult.Trim(), NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.CurrentCulture, out var parsed))
                    numeric = parsed;
            }

            if (!numeric.HasValue)
            {
                // cannot determine numeric value -> set unknown and uncheck
                IsCorrectiveResultInRange = null;
                MarkResolved = false;
                return;
            }

            // Determine range to use for evaluation
            double? minRange = null;
            double? maxRange = null;

            // 1) prefer explicit range provided to the view (RangeMin/RangeMax)
            if (RangeMin.HasValue || RangeMax.HasValue)
            {
                minRange = RangeMin;
                maxRange = RangeMax;
            }
            else
            {
                // 2) try resolving result's control detail navigation
                var resCtrl = r.IdControlDetailNavigation;
                if (resCtrl != null)
                {
                    var mean = resCtrl.CurMean ?? resCtrl.MeanNsx;
                    var sd = resCtrl.CurSd ?? resCtrl.SdNsx;
                    if (mean.HasValue && sd.HasValue)
                    {
                        minRange = mean.Value - 2 * sd.Value;
                        maxRange = mean.Value + 2 * sd.Value;
                    }
                }

                // 3) fallback: try selected incident's control detail
                if (!minRange.HasValue && !maxRange.HasValue && SelectedExistingIncident?.ControlInfoDetail != null)
                {
                    var cid = SelectedExistingIncident.ControlInfoDetail;
                    var mean2 = cid?.CurMean ?? cid?.MeanNsx;
                    var sd2 = cid?.CurSd ?? cid?.SdNsx;
                    if (mean2.HasValue && sd2.HasValue)
                    {
                        minRange = mean2.Value - 2 * sd2.Value;
                        maxRange = mean2.Value + 2 * sd2.Value;
                    }
                }
            }

            // If we still don't have a range, treat as unknown
            if (!minRange.HasValue && !maxRange.HasValue)
            {
                IsCorrectiveResultInRange = null;
                MarkResolved = false;
                return;
            }

            bool meetsMin = !minRange.HasValue || numeric.Value >= minRange.Value;
            bool meetsMax = !maxRange.HasValue || numeric.Value <= maxRange.Value;
            IsCorrectiveResultInRange = meetsMin && meetsMax;

            // Set checkbox: checked when in-range, unchecked otherwise (including unknown/null)
            MarkResolved = IsCorrectiveResultInRange == true;
        }

        private async Task SaveAsync()
        {
            try
            {
                int? savedCaId = null;

                // Require existing InternalError selection — do not auto-create
                if (SelectedExistingIncident == null)
                {
                    MessageBox.Show("Vui lòng chọn một lỗi nội kiểm trước khi lưu.", "Yêu cầu", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                using (var db = new QcManagmentContext())
                {
                    var internalError = await db.InternalErrors.FirstOrDefaultAsync(e => e.Id == SelectedExistingIncident.Id);
                    if (internalError == null)
                    {
                        MessageBox.Show("Không tìm thấy lỗi nội kiểm đã chọn trong cơ sở dữ liệu.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    if (MarkResolved)
                    {
                        internalError.IsResolved = true;
                        internalError.Status = "Hoàn thành";
                    }

                    // update WestgardDescription as before
                    internalError.WestgardDescription = string.IsNullOrWhiteSpace(internalError.WestgardDescription) ? LeveyJenningsError : internalError.WestgardDescription;

                    // determine cause to save: prefer manual ErrorReason, else selected detail/category
                    var causeToSave = !string.IsNullOrWhiteSpace(ErrorReason)
                        ? ErrorReason
                        : (!string.IsNullOrWhiteSpace(SelectedCauseDetail) ? $"{SelectedCauseCategory}: {SelectedCauseDetail}" : SelectedCauseCategory);

                    internalError.Cause = causeToSave ?? internalError.Cause;

                    db.InternalErrors.Update(internalError);

                    CorrectiveAction caEntity;

                    if (_editingCorrectiveActionId.HasValue)
                    {
                        // update existing corrective action
                        caEntity = await db.CorrectiveActions.FirstOrDefaultAsync(c => c.Id == _editingCorrectiveActionId.Value);
                        if (caEntity == null)
                        {
                            MessageBox.Show("Không tìm thấy phiếu khắc phục để cập nhật.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                        // update fields
                        caEntity.InternalErrorId = internalError.Id;
                        caEntity.ResolvingResultId = SelectedResolvingResult?.Id ?? _resolvingResultId;
                        caEntity.ActionDescription = !string.IsNullOrWhiteSpace(CorrectiveAction)
                                                    ? CorrectiveAction
                                                    : (SelectedAction != null && SelectedAction != "Khác" ? SelectedAction : caEntity.ActionDescription);

                        caEntity.ActionOwner = SelectedResolvingResult?.IdUserNavigation?.DisplayName ?? Reporter;
                        caEntity.ActionCompletedAt = SelectedResolvingResult?.DateRun ?? (MarkResolved ? (DateTime?)DateTime.Now : null);
                        caEntity.Outcome = MarkResolved ? "Hoàn thành" : "Chưa hoàn thành";
                        caEntity.PreventiveAction = !string.IsNullOrWhiteSpace(PreventiveAction) ? PreventiveAction : caEntity.PreventiveAction;

                        // Ensure CreatedAt reflects the internal error timestamp (per request)
                        caEntity.CreatedAt = internalError.CreatedAt;
                        caEntity.CreatedBy = Reporter;

                        db.CorrectiveActions.Update(caEntity);
                        savedCaId = caEntity.Id;
                    }
                    else
                    {
                        // create new corrective action (existing behavior) but populate fields from SelectedResolvingResult when available
                        var ca = new CorrectiveAction
                        {
                            InternalErrorId = internalError.Id,
                            // Prefer user's selected resolving result if present, otherwise use constructor-provided id
                            ResolvingResultId = SelectedResolvingResult?.Id ?? _resolvingResultId,
                            // Prefer user-typed CorrectiveAction (textbox). If empty, fall back to selected option (unless "Khác").
                            ActionDescription = !string.IsNullOrWhiteSpace(CorrectiveAction)
                                                ? CorrectiveAction
                                                : (SelectedAction != null && SelectedAction != "Khác" ? SelectedAction : null),
                            // Prefer resolving result's actor if a resolving result is selected; otherwise use Reporter
                            ActionOwner = SelectedResolvingResult?.IdUserNavigation?.DisplayName ?? Reporter,
                            // Prefer resolving result's DateRun for ActionCompletedAt if provided; otherwise use MarkResolved flag/time
                            ActionCompletedAt = SelectedResolvingResult?.DateRun ?? (MarkResolved ? (DateTime?)DateTime.Now : null),
                            Outcome = MarkResolved ? "Hoàn thành" : "Chưa hoàn thành",
                            // CreatedAt: use the internal error's CreatedAt so CA matches the error timestamp
                            CreatedAt = internalError.CreatedAt,
                            CreatedBy = Reporter,
                            PreventiveAction = !string.IsNullOrWhiteSpace(PreventiveAction) ? PreventiveAction : null
                        };

                        db.CorrectiveActions.Add(ca);
                        await db.SaveChangesAsync(); // save here so ca.Id is populated
                        savedCaId = ca.Id;
                    }

                    if (internalError.ErroneousResultId.HasValue)
                    {
                        var erroneous = await db.Results.FirstOrDefaultAsync(r => r.Id == internalError.ErroneousResultId.Value);
                        if (erroneous != null)
                        {
                            erroneous.IsCorrected = true;
                            db.Results.Update(erroneous);
                        }
                    }

                    await db.SaveChangesAsync();
                } // dispose DB

                // If we saved/updated a corrective action, ask user then load it with navigation properties and show report.
                if (savedCaId.HasValue)
                {
                    // prompt user on UI thread and proceed only when they press OK
                    MessageBoxResult userChoice = MessageBoxResult.None;
                    Application.Current?.Dispatcher.Invoke(() =>
                    {
                        userChoice = MessageBox.Show("Lưu phiếu khắc phục thành công. Bạn có muốn xem báo cáo?", "Thông báo", MessageBoxButton.OKCancel, MessageBoxImage.Information);
                    });

                    if (userChoice == MessageBoxResult.OK)
                    {
                        try
                        {
                            // load fresh from DB with related navigation properties
                            using var db2 = new QcManagmentContext();
                            var savedCa = await db2.CorrectiveActions
                                // internal error and its test/device
                                .Include(c => c.InternalError).ThenInclude(i => i.Test)
                                .Include(c => c.InternalError).ThenInclude(i => i.Device)
                                // include InternalError.Cause by eager-loading InternalError
                                .Include(c => c.InternalError).ThenInclude(i => i.ErroneousResult)
                                    .ThenInclude(r => r.IdControlDetailNavigation)
                                .Include(c => c.InternalError).ThenInclude(i => i.ErroneousResult)
                                    .ThenInclude(r => r.IdTestNavigation)
                                .Include(c => c.InternalError).ThenInclude(i => i.ErroneousResult)
                                    .ThenInclude(r => r.IdDeviceNavigation)
                                // ensuring level is loaded on erroneous result if present
                                .Include(c => c.InternalError).ThenInclude(i => i.ErroneousResult)
                                    .ThenInclude(r => r.IdLevelNavigation)
                                // resolving result and its relationships
                                .Include(c => c.ResolvingResult).ThenInclude(r => r.IdTestNavigation)
                                .Include(c => c.ResolvingResult).ThenInclude(r => r.IdDeviceNavigation)
                                .Include(c => c.ResolvingResult).ThenInclude(r => r.IdControlDetailNavigation)
                                .Include(c => c.ResolvingResult).ThenInclude(r => r.IdLevelNavigation)
                                .Include(c => c.ResolvingResult).ThenInclude(r => r.IdUserNavigation)
                                .FirstOrDefaultAsync(c => c.Id == savedCaId.Value);

                            if (savedCa != null)
                            {
                                // show report window on UI thread, using the current corrective dialog as owner when possible
                                Application.Current?.Dispatcher.Invoke(() =>
                                {
                                    try
                                    {
                                        var ownerWindow = Application.Current?.Windows
                                            .OfType<System.Windows.Window>()
                                            .FirstOrDefault(w => w.DataContext == this) // prefer the window hosting this VM
                                            ?? System.Windows.Application.Current?.MainWindow; // fallback

                                        var reportWindow = new QC_Management.Views.CorrectiveActionReportWindow(new[] { savedCa });
                                        reportWindow.Owner = ownerWindow;
                                        reportWindow.ShowDialog();
                                    }
                                    catch
                                    {
                                        // swallow UI exceptions to avoid breaking flow
                                    }
                                });
                            }

                            // Close the corrective-action window (request close) AFTER the report was shown (or attempted).
                            RequestClose?.Invoke(true);
                            return;
                        }
                        catch
                        {
                            // best effort: if report generation fails, just close dialog
                            RequestClose?.Invoke(true);
                            return;
                        }
                    }
                    else
                    {
                        // user chose not to see report — just close
                        RequestClose?.Invoke(true);
                        return;
                    }
                }

                // If nothing to report, just close
                RequestClose?.Invoke(true);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi lưu phiếu khắc phục: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // list of "Cause: action1; action2" lines
        public ObservableCollection<string> CauseActionPairs { get; } = new();

        // Combined editable text for the details textbox (two-way)
        public string CauseActionDetails
        {
            get => string.Join(Environment.NewLine, CauseActionPairs);
            set
            {
                var newValue = value ?? string.Empty;
                // parse lines into collection
                var lines = newValue
                    .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(l => l.Trim())
                    .Where(l => !string.IsNullOrEmpty(l))
                    .ToList();

                CauseActionPairs.Clear();
                foreach (var l in lines) CauseActionPairs.Add(l);

                OnPropertyChanged();
            }
        }

        private ICommand? _addCauseActionPairCommand;
        public ICommand AddCauseActionPairCommand => _addCauseActionPairCommand ??= new RelayCommand<object>(
            _ => true,
            param =>
            {
                // param expected to be string: "cause" or "action"
                var who = (param as string) ?? string.Empty;

                var cause = SelectedCauseCategory?.Trim() ?? string.Empty;
                var action = SelectedAction?.Trim() ?? string.Empty;

                if (string.IsNullOrEmpty(cause)) return;

                if (who.Equals("cause", StringComparison.OrdinalIgnoreCase))
                {
                    // On cause change: add a header line "Cause:" if last line isn't the same cause
                    var header = $"{cause}:";
                    if (!CauseActionPairs.Any() || !CauseActionPairs.Last().StartsWith(header, StringComparison.OrdinalIgnoreCase))
                    {
                        CauseActionPairs.Add(header);
                        OnPropertyChanged(nameof(CauseActionDetails));
                    }
                    return;
                }

                // default: treat as action selection event -> append action into last line for same cause
                if (string.IsNullOrEmpty(action)) return;

                if (!CauseActionPairs.Any())
                {
                    CauseActionPairs.Add($"{cause}: {action}");
                    OnPropertyChanged(nameof(CauseActionDetails));
                    return;
                }

                var lastIndex = CauseActionPairs.Count - 1;
                var last = CauseActionPairs[lastIndex];
                var prefix = $"{cause}:";

                if (last.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    var existing = last.Substring(prefix.Length).Trim();
                    // split existing by semicolon to check duplicates
                    var existingActions = existing.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                                                  .Select(a => a.Trim())
                                                  .Where(a => !string.IsNullOrEmpty(a))
                                                  .ToList();

                    if (!existingActions.Any(a => string.Equals(a, action, StringComparison.OrdinalIgnoreCase)))
                    {
                        var newActions = string.IsNullOrEmpty(existing) ? action : existing + "; " + action;
                        CauseActionPairs[lastIndex] = $"{prefix} {newActions}";
                        OnPropertyChanged(nameof(CauseActionDetails));
                    }
                }
                else
                {
                    // last line is different cause -> add new line for current cause
                    CauseActionPairs.Add($"{cause}: {action}");
                    OnPropertyChanged(nameof(CauseActionDetails));
                }
            });

    }
}