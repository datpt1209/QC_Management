using Microsoft.EntityFrameworkCore;
using QC_Management.Models;
using QC_Management.Views;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.ComponentModel;
using System.Collections.Generic;
using QC_Management.Services; // <-- added for UserManager

namespace QC_Management.ViewModels
{
    public class IncidentManagementViewModel : BaseViewModel
    {
        public ObservableCollection<InternalError> InternalErrors { get; } = new();
        public ObservableCollection<CorrectiveAction> CorrectiveActions { get; } = new();

        // Devices for filter
        public ObservableCollection<Device> Devices { get; } = new();

        private InternalError? _selectedInternalError;
        public InternalError? SelectedInternalError { get => _selectedInternalError; set { _selectedInternalError = value; OnPropertyChanged(); } }

        private CorrectiveAction? _selectedCorrectiveAction;
        public CorrectiveAction? SelectedCorrectiveAction { get => _selectedCorrectiveAction; set { _selectedCorrectiveAction = value; OnPropertyChanged(); } }

        private Device? _selectedDevice;
        public Device? SelectedDevice
        {
            get => _selectedDevice;
            set
            {
                if (_selectedDevice == value) return;
                _selectedDevice = value;
                OnPropertyChanged();
            }
        }

        private DateTime? _selectedStartDate;
        public DateTime? SelectedStartDate
        {
            get => _selectedStartDate;
            set
            {
                if (_selectedStartDate == value) return;
                _selectedStartDate = value;
                OnPropertyChanged();
            }
        }

        private DateTime? _selectedEndDate;
        public DateTime? SelectedEndDate
        {
            get => _selectedEndDate;
            set
            {
                if (_selectedEndDate == value) return;
                _selectedEndDate = value;
                OnPropertyChanged();
            }
        }

        // Expose whether current user is a manager (used by the view to hide/show admin actions)
        private bool _isManagement;
        public bool IsManagement
        {
            get => _isManagement;
            private set
            {
                if (_isManagement == value) return;
                _isManagement = value;
                OnPropertyChanged();
            }
        }
        public ICommand ReloadInternalErrorsCommand { get; }
        public ICommand AddInternalErrorCommand { get; }
        public ICommand EditInternalErrorCommand { get; }
        public ICommand DeleteInternalErrorCommand { get; }
        public ICommand ReloadCorrectiveActionsCommand { get; }
        public ICommand AddCorrectiveActionCommand { get; }
        public ICommand EditCorrectiveActionCommand { get; }
        public ICommand DeleteCorrectiveActionCommand { get; }

        // New command: delete a single corrective action (row-level)
        public ICommand DeleteCorrectiveActionItemCommand { get; }

        // New command to open corrective action window for selected internal error
        public ICommand OpenCorrectiveActionCommand { get; }

        // New command to edit a specific corrective action item (per-row Edit)
        public ICommand EditCorrectiveActionItemCommand { get; }

        public ICommand FilterInternalErrorsCommand { get; }
        public ICommand ClearFiltersCommand { get; }
        public ICommand LoadedCommand { get; }
        public ICommand PrintInternalErrorsCommand { get; }

        // New: print single corrective action (button per action)
        public ICommand PrintCorrectiveActionCommand { get; }

        // Accept optional initial id so callers can open the management view with a preselected error.
        public IncidentManagementViewModel()
        {
            // Determine current user's management status by Role id: 1 or 3 are managers.
            try
            {
                var current = UserManager.Instance?.CurrentUser;
                IsManagement = current != null && (current.Role == 1 || current.Role == 3);
            }
            catch
            {
                IsManagement = false;
            }

            ReloadInternalErrorsCommand = new RelayCommand<object>(p => true, async p => await LoadInternalErrorsAsync());
            AddInternalErrorCommand = new RelayCommand<object>(p => true, p => MessageBox.Show("Add InternalError - implement UI"));
            EditInternalErrorCommand = new RelayCommand<object>(p => SelectedInternalError != null, p => MessageBox.Show("Edit InternalError - implement UI"));
            DeleteInternalErrorCommand = new RelayCommand<object>(p => SelectedInternalError != null, async p => await DeleteInternalErrorAsync());

            ReloadCorrectiveActionsCommand = new RelayCommand<object>(p => true, async p => await LoadCorrectiveActionsAsync());
            AddCorrectiveActionCommand = new RelayCommand<object>(p => true, p => MessageBox.Show("Add CorrectiveAction - implement UI"));
            EditCorrectiveActionCommand = new RelayCommand<object>(p => SelectedCorrectiveAction != null, p => MessageBox.Show("Edit CorrectiveAction - implement UI"));
            DeleteCorrectiveActionCommand = new RelayCommand<object>(p => SelectedCorrectiveAction != null, async p => await DeleteCorrectiveActionAsync());
            LoadedCommand = new RelayCommand<object>(p => true, async p => await Loaded());

            // New: open corrective action window for the selected internal error.
            OpenCorrectiveActionCommand = new RelayCommand<object>(
                p => SelectedInternalError != null,
                async p => await OpenCorrectiveActionAsync());

            // New: edit specific corrective action item (row-level edit)
            EditCorrectiveActionItemCommand = new RelayCommand<object>(
                p => p is CorrectiveAction,
                async p => await OpenCorrectiveActionForEditAsync(p as CorrectiveAction));

            // New: delete specific corrective action item (row-level delete)
            DeleteCorrectiveActionItemCommand = new RelayCommand<object>(
                p => p is CorrectiveAction,
                async p => await DeleteCorrectiveActionItemAsync(p as CorrectiveAction));

            FilterInternalErrorsCommand = new RelayCommand<object>(p => true, async p => await LoadInternalErrorsAsync());
            ClearFiltersCommand = new RelayCommand<object>(p => true, async p =>
            {
                SelectedDevice = null;
                SelectedStartDate = null;
                SelectedEndDate = null;
                await LoadInternalErrorsAsync();
            });

            PrintInternalErrorsCommand = new RelayCommand<object>(p => InternalErrors.Any(), p => PrintInternalErrors());

            // Initialize new command (async)
            PrintCorrectiveActionCommand = new RelayCommand<object>(
                p => p is CorrectiveAction,
                async p => await PrintCorrectiveActionAsync(p as CorrectiveAction));
        }

        private async Task Loaded()
        {
            await LoadDevicesAsync();

            // Default to last 1 month on initial load if no explicit filters set.
            if (!SelectedStartDate.HasValue && !SelectedEndDate.HasValue)
            {
                SelectedEndDate = DateTime.Today;
                SelectedStartDate = DateTime.Today.AddMonths(-1);
            }

            await LoadInternalErrorsAsync();
            await LoadCorrectiveActionsAsync();

            // After loading corrective actions, synchronize any completed corrective actions to mark internal errors resolved.
            await SyncResolvedInternalErrorsAsync();
        }

        private async Task LoadDevicesAsync()
        {
            try
            {
                Devices.Clear();
                using var db = new QcManagmentContext();
                var list = await db.Devices
                    .AsNoTracking()
                    .OrderBy(d => d.Name)
                    .ToListAsync();

                foreach (var d in list) Devices.Add(d);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Load devices failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadInternalErrorsAsync()
        {
            try
            {
                InternalErrors.Clear();
                using var db = new QcManagmentContext();

                // include ErroneousResult and its level navigation so Result and Level are available
                var query = db.InternalErrors
                    .AsNoTracking()
                    .Include(i => i.Test)
                    .Include(i => i.Device)
                    .Include(i => i.ControlInfoDetail)
                    .Include(i => i.ErroneousResult) // <-- ensure ErroneousResult is loaded
                        .ThenInclude(r => r.IdLevelNavigation) // <-- ensure level navigation is loaded
                                                               // include corrective actions and their resolving result + control detail for post-range
                    .Include(i => i.CorrectiveActions)
                        .ThenInclude(c => c.ResolvingResult)
                            .ThenInclude(r => r.IdControlDetailNavigation)
                    .OrderByDescending(i => i.CreatedAt)
                    .AsQueryable();

                if (SelectedDevice != null)
                {
                    query = query.Where(i => i.DeviceId.HasValue && i.DeviceId == SelectedDevice.Id);
                }

                if (SelectedStartDate.HasValue)
                {
                    var s = SelectedStartDate.Value.Date;
                    query = query.Where(i => i.CreatedAt >= s);
                }

                if (SelectedEndDate.HasValue)
                {
                    var eExclusive = SelectedEndDate.Value.Date.AddDays(1);
                    query = query.Where(i => i.CreatedAt < eExclusive);
                }

                var list = await query.Take(500).ToListAsync();

                foreach (var e in list) InternalErrors.Add(e);

                // inside LoadInternalErrorsAsync after populating InternalErrors
                for (int i = 0; i < InternalErrors.Count; i++)
                {
                    InternalErrors[i].RowNumber = i + 1;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Load InternalErrors failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadCorrectiveActionsAsync()
        {
            try
            {
                CorrectiveActions.Clear();
                using var db = new QcManagmentContext();
                var list = await db.CorrectiveActions
                    .AsNoTracking()
                    .Include(c => c.InternalError).ThenInclude(i => i.Test)
                    .Include(c => c.InternalError).ThenInclude(i => i.Device)
                    // include resolving result and its control detail for post-range
                    .Include(c => c.ResolvingResult)
                        .ThenInclude(r => r.IdControlDetailNavigation)
                    .OrderByDescending(c => c.CreatedAt)
                    .Take(500)
                    .ToListAsync();

                foreach (var c in list) CorrectiveActions.Add(c);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Load CorrectiveActions failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task DeleteInternalErrorAsync()
        {
            if (SelectedInternalError == null) return;
            if (MessageBox.Show("Xóa lỗi nội kiểm đã chọn?", "Xác nhận", MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;

            try
            {
                using var db = new QcManagmentContext();
                // use transaction to ensure atomicity
                using var tx = await db.Database.BeginTransactionAsync();

                var id = SelectedInternalError.Id;

                // Load the internal error with corrective actions and ErroneousResultId
                var ie = await db.InternalErrors
                    .Include(i => i.CorrectiveActions)
                    .FirstOrDefaultAsync(i => i.Id == id);

                if (ie == null)
                {
                    MessageBox.Show("Internal error not found in database.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // capture referenced result id (if any) before deleting
                var referencedResultId = ie.ErroneousResultId;

                // delete all corrective actions related to this internal error
                if (ie.CorrectiveActions != null && ie.CorrectiveActions.Any())
                {
                    db.CorrectiveActions.RemoveRange(ie.CorrectiveActions);
                }

                // remove the internal error itself
                db.InternalErrors.Remove(ie);

                // If the internal error referenced a Result, ensure we only clear the Result's Westgard / out-of-range flags
                // when no other InternalError references the same Result. When clearing, set IsCorrected = null (per request).
                if (referencedResultId.HasValue)
                {
                    // count other internal errors that still reference the same result (excluding this one)
                    var otherReferences = await db.InternalErrors
                        .AsNoTracking()
                        .CountAsync(i => i.ErroneousResultId == referencedResultId.Value && i.Id != id);

                    var result = await db.Results.FirstOrDefaultAsync(r => r.Id == referencedResultId.Value);
                    if (result != null)
                    {
                        if (otherReferences == 0)
                        {
                            // No other InternalError references this Result -> clear Westgard text and out-of-range flags.
                            result.WestgardRule = null;
                            result.IsOutRange = false;
                            // Per request: set IsCorrected to null when clearing the error marker
                            result.IsCorrected = null;
                            db.Results.Update(result);
                        }
                        else
                        {
                            // There are other InternalErrors referencing the same Result.
                            // Check whether any remaining InternalError still has a WestgardDescription (i.e. still marks an error).
                            var remainingErrors = await db.InternalErrors
                                .AsNoTracking()
                                .Where(i => i.ErroneousResultId == referencedResultId.Value && i.Id != id)
                                .Select(i => i.WestgardDescription)
                                .ToListAsync();

                            var anyRemainingWestgard = remainingErrors.Any(w => !string.IsNullOrWhiteSpace(w));

                            if (!anyRemainingWestgard)
                            {
                                // No remaining InternalErrors indicate Westgard violation -> clear Westgard/out-of-range on result.
                                result.WestgardRule = null;
                                result.IsOutRange = false;
                                result.IsCorrected = null; // clear corrected marker per request
                                db.Results.Update(result);
                            }
                            // else: keep existing flags because other internal errors still reference an active Westgard violation.
                        }
                    }
                }

                await db.SaveChangesAsync();
                await tx.CommitAsync();

                // Remove deleted corrective actions from in-memory collection
                var caToRemove = CorrectiveActions.Where(c => c.InternalErrorId == id).ToList();
                foreach (var ca in caToRemove) CorrectiveActions.Remove(ca);

                // Remove the internal error from in-memory collection and clear selection
                var ieInMemory = InternalErrors.FirstOrDefault(x => x.Id == id);
                if (ieInMemory != null) InternalErrors.Remove(ieInMemory);

                SelectedInternalError = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Delete failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task DeleteCorrectiveActionAsync()
        {
            if (SelectedCorrectiveAction == null) return;
            if (MessageBox.Show("Xóa phiếu khắc phục đã chọn?", "Xác nhận", MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;

            try
            {
                using var db = new QcManagmentContext();
                db.CorrectiveActions.Remove(new CorrectiveAction { Id = SelectedCorrectiveAction.Id });
                await db.SaveChangesAsync();
                CorrectiveActions.Remove(SelectedCorrectiveAction);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Delete failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task DeleteCorrectiveActionItemAsync(CorrectiveAction? ca)
        {
            if (ca == null) return;
            if (MessageBox.Show("Xóa phiếu khắc phục đã chọn?", "Xác nhận", MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;

            try
            {
                // Load CA with its internal error and resolving result
                await using var db = new QcManagmentContext();
                var caEntity = await db.CorrectiveActions
                    .Include(c => c.InternalError)
                    .FirstOrDefaultAsync(x => x.Id == ca.Id);

                if (caEntity == null)
                {
                    MessageBox.Show("Corrective action not found.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                int internalErrorId = caEntity.InternalErrorId;

                // Remove the corrective action
                db.CorrectiveActions.Remove(caEntity);
                await db.SaveChangesAsync();

                // Re-evaluate remaining corrective actions for the same internal error
                var remainingCAs = await db.CorrectiveActions
                    .AsNoTracking()
                    .Where(x => x.InternalErrorId == internalErrorId)
                    .ToListAsync();

                // Decide new InternalError state:
                bool anyCompleted = remainingCAs.Any(x => x.ActionCompletedAt != null);

                var ie = await db.InternalErrors.FindAsync(internalErrorId);
                if (ie != null)
                {
                    ie.IsResolved = anyCompleted;
                    ie.Status = anyCompleted ? "Hoàn thành" : "Đang chờ";
                    db.InternalErrors.Update(ie);

                    // Also update Erroneous Result.IsCorrected accordingly (if ErroneousResult exists)
                    if (ie.ErroneousResultId.HasValue)
                    {
                        var res = await db.Results.FindAsync(ie.ErroneousResultId.Value);
                        if (res != null)
                        {
                            res.IsCorrected = anyCompleted ? (bool?)true : null;
                            db.Results.Update(res);
                        }
                    }

                    await db.SaveChangesAsync();
                }

                // Reload UI collections to ensure full synchronization
                await LoadInternalErrorsAsync();
                await LoadCorrectiveActionsAsync();

                // Clear selection(s)
                SelectedCorrectiveAction = null;
                SelectedInternalError = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Xóa phiếu thất bại: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void PrintInternalErrors()
        {
            try
            {
                var rows = new List<InternalErrorReportRow>();

                // For each InternalError, emit one row per corrective action.
                // If none, emit one row with empty action fields so the error still appears.
                foreach (var ie in InternalErrors)
                {
                    // common internal error fields
                    var deviceName = ie.Device?.Name ?? string.Empty;
                    var testName = ie.Test?.Name ?? string.Empty;
                    var level = ie.ErroneousResult?.IdLevelNavigation?.Name ?? string.Empty;
                    var createdAt = ie.CreatedAt;
                    var createdBy = ie.CreatedBy ?? string.Empty;
                    var errorDescription = ie.WestgardDescription ?? string.Empty;
                    var cause = ie.Cause ?? string.Empty;
                    // Format numeric/text fields safely
                    var preCorrect = ie.ErroneousResult?.Result1?.ToString("0.###") ?? string.Empty;

                    // compute rangeBefore: if qualitative mean exists, show that string,
                    // otherwise compute numeric mean ± 2*sd as before.
                    string rangeBefore;
                    if (ie.ControlInfoDetail != null)
                    {
                        // prefer qualitative description when present
                        if (!string.IsNullOrWhiteSpace(ie.ControlInfoDetail.QualitativeMean))
                        {
                            rangeBefore = ie.ControlInfoDetail.QualitativeMean.Trim();
                        }
                        else
                        {
                            double? mean = ie.ControlInfoDetail.CurMean ?? ie.ControlInfoDetail.MeanNsx;
                            double? sd = ie.ControlInfoDetail.CurSd ?? ie.ControlInfoDetail.SdNsx;

                            if (mean.HasValue && sd.HasValue)
                            {
                                var lower = mean.Value - 2 * sd.Value;
                                var upper = mean.Value + 2 * sd.Value;
                                rangeBefore = $"{lower:0.###} - {upper:0.###}";
                            }
                            else
                            {
                                rangeBefore = string.Empty;
                            }
                        }
                    }
                    else
                    {
                        rangeBefore = string.Empty;
                    }


                    if (ie.CorrectiveActions != null && ie.CorrectiveActions.Any())
                    {
                        foreach (var ca in ie.CorrectiveActions.OrderBy(c => c.CreatedAt))
                        {
                            // compute ReferenceRangeAfter from corrective action's resolving result control detail if available
                            string rangeAfter = string.Empty;
                            var resolvingResult = ca.ResolvingResult;
                            ControlInfoDetail? afterControl = resolvingResult?.IdControlDetailNavigation;

                            if (afterControl != null)
                            {
                                // prefer qualitative description when present
                                if (!string.IsNullOrWhiteSpace(afterControl.QualitativeMean))
                                {
                                    rangeAfter = afterControl.QualitativeMean.Trim();
                                }
                                else
                                {
                                    double? meanA = afterControl.CurMean ?? afterControl.MeanNsx;
                                    double? sdA = afterControl.CurSd ?? afterControl.SdNsx;
                                    if (meanA.HasValue && sdA.HasValue)
                                    {
                                        var lowerA = meanA.Value - 2 * sdA.Value;
                                        var upperA = meanA.Value + 2 * sdA.Value;
                                        rangeAfter = $"{lowerA:0.###} - {upperA:0.###}";
                                    }
                                }
                            }
                            else
                            {
                                // fallback: if original error had a qualitative mean, show that
                                if (ie.ControlInfoDetail != null)
                                {
                                    if (!string.IsNullOrWhiteSpace(ie.ControlInfoDetail.QualitativeMean))
                                    {
                                        rangeAfter = ie.ControlInfoDetail.QualitativeMean.Trim();
                                    }
                                    else
                                    {
                                        double? meanA = ie.ControlInfoDetail.CurMean ?? ie.ControlInfoDetail.MeanNsx;
                                        double? sdA = ie.ControlInfoDetail.CurSd ?? ie.ControlInfoDetail.SdNsx;
                                        if (meanA.HasValue && sdA.HasValue)
                                        {
                                            var lowerA = meanA.Value - 2 * sdA.Value;
                                            var upperA = meanA.Value + 2 * sdA.Value;
                                            rangeAfter = $"{lowerA:0.###} - {upperA:0.###}";
                                        }
                                    }
                                }
                            }

                            // Post-correction result: take from corrective action's resolving Result (if available)
                            var postCorrect = resolvingResult?.Result1.HasValue == true
                                ? resolvingResult.Result1.Value.ToString("0.###")
                                : string.Empty;

                            rows.Add(new InternalErrorReportRow
                            {
                                InternalErrorId = ie.Id,
                                Device = deviceName,
                                TestName = testName,
                                Level = level,
                                CreatedAt = createdAt,
                                CreatedBy = createdBy,
                                ErrorDescription = errorDescription,
                                Cause = cause,
                                // corrective action fields (one row per action)
                                ActionDescription = ca.ActionDescription ?? string.Empty,
                                ActionOwner = ca.ActionOwner ?? string.Empty,
                                ActionCompleteAt = ca.ActionCompletedAt,
                                Outcome = ca.Outcome ?? string.Empty,
                                PreventiveAction = ca.PreventiveAction ?? string.Empty,
                                // optional fields from error
                                PreCorrectResult = preCorrect,
                                PostCorrectResult = postCorrect,
                                ReferenceRangeBefore = rangeBefore,
                                ReferenceRangeAfter = rangeAfter
                            });
                        }
                    }
                    else
                    {
                        // no corrective action -> single row with empty action columns
                        rows.Add(new InternalErrorReportRow
                        {
                            InternalErrorId = ie.Id,
                            Device = deviceName,
                            TestName = testName,
                            Level = level,
                            CreatedAt = createdAt,
                            CreatedBy = createdBy,
                            ErrorDescription = errorDescription,
                            Cause = cause,
                            ActionDescription = string.Empty,
                            ActionOwner = string.Empty,
                            ActionCompleteAt = null,
                            Outcome = string.Empty,
                            PreventiveAction = string.Empty,
                            PreCorrectResult = preCorrect,
                            PostCorrectResult = string.Empty,
                            ReferenceRangeBefore = rangeBefore,
                            ReferenceRangeAfter = string.Empty
                        });
                    }
                }

                var win = new InternalErrorsReportView(rows);

                // Guard before setting Owner: Application.Current.MainWindow can be null or not visible
                var main = System.Windows.Application.Current?.MainWindow;
                try
                {
                    // Ensure owner is a visible Window; set owner only in that case to avoid InvalidOperationException.
                    if (main != null && main.IsVisible)
                    {
                        // Ensure UI thread when setting Owner and showing dialog
                        System.Windows.Application.Current.Dispatcher.Invoke(() =>
                        {
                            win.Owner = main;
                            win.ShowDialog();
                        });
                    }
                    else
                    {
                        // Fall back: show dialog without setting Owner (safe)
                        System.Windows.Application.Current.Dispatcher.Invoke(() =>
                        {
                            win.ShowDialog();
                        });
                    }
                }
                catch (Exception ex)
                {
                    // Fall back again: try to show non-modal if dialog fails to avoid breaking UX.
                    try
                    {
                        System.Windows.Application.Current.Dispatcher.Invoke(() =>
                        {
                            if (!win.IsVisible)
                                win.Show();
                        });
                    }
                    catch
                    {
                        // final fallback: report the error
                        MessageBox.Show($"Show report failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Prepare report failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        // New: set InternalError.IsResolved = true and Status = "Hoàn thành"
        // for internal errors that have at least one completed CorrectiveAction (ActionCompletedAt != null).
        private async Task SyncResolvedInternalErrorsAsync()
        {
            try
            {
                using var db = new QcManagmentContext();

                // Find internal error IDs referenced by corrective actions that have a completion timestamp.
                // InternalErrorId is non-nullable int, so just select it when ActionCompletedAt is set.
                var completedInternalErrorIds = await db.CorrectiveActions
                    .AsNoTracking()
                    .Where(c => c.ActionCompletedAt != null)
                    .Select(c => c.InternalErrorId)
                    .Distinct()
                    .ToListAsync();

                if (!completedInternalErrorIds.Any()) return;

                // Load internal errors that are not yet marked resolved (IsResolved == false)
                var toUpdate = await db.InternalErrors
                    .Where(i => completedInternalErrorIds.Contains(i.Id) && !i.IsResolved)
                    .ToListAsync();

                if (!toUpdate.Any()) return;

                foreach (var ie in toUpdate)
                {
                    ie.IsResolved = true;
                    ie.Status = "Hoàn thành";
                }

                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // non-fatal: log / notify
                MessageBox.Show($"Sync resolved errors failed: {ex.Message}", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        // New: open corrective action window for the selected internal error and refresh lists after save.
        private async Task OpenCorrectiveActionAsync()
        {
            if (SelectedInternalError == null) return;

            try
            {
                // Load a fresh list of existing (unresolved) incidents for the corrective action dialog context.
                List<InternalError> existingErrors;
                using (var db = new QcManagmentContext())
                {
                    existingErrors = await db.InternalErrors
                        .AsNoTracking()
                        .OrderByDescending(i => i.CreatedAt)
                        .Take(200)
                        .ToListAsync();
                }

                // Compute range bounds from ControlInfoDetail if present
                double? rangeMin = null;
                double? rangeMax = null;
                var cid = SelectedInternalError.ControlInfoDetail;
                if (cid != null)
                {
                    double? mean = cid.CurMean ?? cid.MeanNsx;
                    double? sd = cid.CurSd ?? cid.SdNsx;
                    if (mean.HasValue && sd.HasValue)
                    {
                        rangeMin = mean.Value - 2 * sd.Value;
                        rangeMax = mean.Value + 2 * sd.Value;
                    }
                }

                var vm = new CorrectiveActionViewModel(
                    date: SelectedInternalError.CreatedAt,
                    reporter: SelectedInternalError.CreatedBy,
                    deviceName: SelectedInternalError.Device?.Name,
                    testName: SelectedInternalError.Test?.Name,
                    levey: SelectedInternalError.WestgardDescription,
                    // Định tính (TestType=1): dùng TempResult (chuỗi), định lượng: dùng Result1
                    resultValue: SelectedInternalError.ErroneousResult?.Result1?.ToString("0.###")
                                 ?? SelectedInternalError.ErroneousResult?.TempResult,
                    rangeMin: rangeMin,
                    rangeMax: rangeMax,
                    existingErrors: existingErrors,
                    resolvingResultId: null,
                    initialInternalErrorId: SelectedInternalError.Id,
                    // truyền TestType để CorrectiveActionViewModel biết đây là định tính hay định lượng
                    testType: SelectedInternalError.Test?.TestType
                );

                var win = new CorrectActionWindow
                {
                    DataContext = vm
                };

                try
                {
                    var mainWin = System.Windows.Application.Current?.MainWindow;
                    if (mainWin != null && mainWin.IsLoaded && mainWin.Visibility == Visibility.Visible && mainWin.Dispatcher == win.Dispatcher)
                    {
                        win.Owner = mainWin;
                    }
                }
                catch
                {
                    // ignore owner-setting failures
                }

                Action<bool>? handler = null;
                handler = async success =>
                {
                    try
                    {
                        if (handler != null) vm.RequestClose -= handler;

                        win.Dispatcher.Invoke(() =>
                        {
                            try { win.DialogResult = success; } catch { }
                            if (win.IsVisible) win.Close();
                        });

                        // If corrective action saved, refresh lists to show updated state.
                        if (success)
                        {
                            // First synchronize DB resolved flags based on corrective actions
                            await SyncResolvedInternalErrorsAsync();

                            // Then reload in UI thread
                            await LoadInternalErrorsAsync();
                            await LoadCorrectiveActionsAsync();
                        }
                    }
                    catch
                    {
                        // swallow
                    }
                };

                vm.RequestClose += handler;

                if (win.ShowDialog() == true)
                {
                    MessageBox.Show("Phiếu khắc phục đã được tạo/ cập nhật.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Open corrective action failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // New: open CorrectActionWindow pre-populated for editing a specific corrective action
        private async Task OpenCorrectiveActionForEditAsync(CorrectiveAction? ca)
        {
            if (ca == null) return;

            try
            {
                CorrectiveAction? caLoaded;
                List<InternalError> existingErrors;

                using (var db = new QcManagmentContext())
                {
                    caLoaded = await db.CorrectiveActions
                        .AsNoTracking()
                        .Include(c => c.InternalError).ThenInclude(i => i.Test)
                        .Include(c => c.InternalError).ThenInclude(i => i.Device)
                        .Include(c => c.InternalError).ThenInclude(i => i.ControlInfoDetail)
                        .Include(c => c.ResolvingResult).ThenInclude(r => r.IdControlDetailNavigation)
                        .Include(c => c.ResolvingResult).ThenInclude(r => r.IdLevelNavigation)
                        .Include(c => c.ResolvingResult).ThenInclude(r => r.IdUserNavigation)
                        .FirstOrDefaultAsync(x => x.Id == ca.Id);

                    existingErrors = await db.InternalErrors
                        .AsNoTracking()
                        .OrderByDescending(i => i.CreatedAt)
                        .Take(200)
                        .ToListAsync();
                }

                if (caLoaded == null)
                {
                    MessageBox.Show("Không tìm thấy phiếu hành động trong cơ sở dữ liệu.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // compute range bounds from resolving result control detail if present, otherwise from internal error control detail
                double? rangeMin = null;
                double? rangeMax = null;
                var cid = caLoaded.ResolvingResult?.IdControlDetailNavigation ?? caLoaded.InternalError?.ControlInfoDetail;
                if (cid != null)
                {
                    double? mean = cid.CurMean ?? cid.MeanNsx;
                    double? sd = cid.CurSd ?? cid.SdNsx;
                    if (mean.HasValue && sd.HasValue)
                    {
                        rangeMin = mean.Value - 2 * sd.Value;
                        rangeMax = mean.Value + 2 * sd.Value;
                    }
                }

                var vm = new CorrectiveActionViewModel(
                    date: caLoaded.CreatedAt,
                    reporter: caLoaded.CreatedBy,
                    deviceName: caLoaded.InternalError?.Device?.Name,
                    testName: caLoaded.InternalError?.Test?.Name,
                    levey: caLoaded.InternalError?.WestgardDescription,
                    // Định tính: dùng TempResult, định lượng: dùng Result1
                    resultValue: caLoaded.ResolvingResult?.Result1?.ToString("0.###")
                                 ?? caLoaded.ResolvingResult?.TempResult
                                 ?? caLoaded.InternalError?.ErroneousResult?.Result1?.ToString("0.###")
                                 ?? caLoaded.InternalError?.ErroneousResult?.TempResult,
                    rangeMin: rangeMin,
                    rangeMax: rangeMax,
                    existingErrors: existingErrors,
                    resolvingResultId: caLoaded.ResolvingResultId,
                    initialInternalErrorId: caLoaded.InternalErrorId,
                    editingCorrectiveActionId: caLoaded.Id,
                    testType: caLoaded.InternalError?.Test?.TestType
                );

                // Note: if CorrectiveActionViewModel exposes an "EditingCorrectiveActionId" or a load method,
                // you'd set it here. We pass resolvingResultId and initialInternalErrorId so the VM can initialize.
                var win = new CorrectActionWindow
                {
                    DataContext = vm
                };

                try
                {
                    var mainWin = System.Windows.Application.Current?.MainWindow;
                    if (mainWin != null && mainWin.IsLoaded && mainWin.Visibility == Visibility.Visible && mainWin.Dispatcher == win.Dispatcher)
                    {
                        win.Owner = mainWin;
                    }
                }
                catch
                {
                    // ignore owner-setting failures
                }

                Action<bool>? handler = null;
                handler = async success =>
                {
                    try
                    {
                        if (handler != null) vm.RequestClose -= handler;

                        win.Dispatcher.Invoke(() =>
                        {
                            try { win.DialogResult = success; } catch { }
                            if (win.IsVisible) win.Close();
                        });

                        // If corrective action saved, refresh lists to show updated state.
                        if (success)
                        {
                            await SyncResolvedInternalErrorsAsync();
                            await LoadInternalErrorsAsync();
                            await LoadCorrectiveActionsAsync();
                        }
                    }
                    catch
                    {
                        // swallow
                    }
                };

                vm.RequestClose += handler;

                if (win.ShowDialog() == true)
                {
                    MessageBox.Show("Phiếu khắc phục đã được tạo/ cập nhật.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Open corrective action (edit) failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Print a single CorrectiveAction using CorrectiveActionReport.rdlc
        private async Task PrintCorrectiveActionAsync(CorrectiveAction? ca)
        {
            if (ca == null)
            {
                MessageBox.Show("Không có phiếu hành động để in.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                // Load the corrective action with required navigations so the report can build ranges and fields
                CorrectiveAction? caLoaded;
                using (var db = new QcManagmentContext())
                {
                    caLoaded = await db.CorrectiveActions
                        .AsNoTracking()
                        .Include(c => c.InternalError).ThenInclude(i => i.Test).ThenInclude(u => u.IdUnitTableNavigation)
                        .Include(c => c.InternalError).ThenInclude(i => i.Device)
                        .Include(c => c.InternalError).ThenInclude(i => i.ControlInfoDetail).ThenInclude(d => d.IdLevelNavigation)
                        .Include(c => c.InternalError).ThenInclude(i => i.ErroneousResult).ThenInclude(r => r.IdControlDetailNavigation)
                        .Include(c => c.InternalError).ThenInclude(i => i.ErroneousResult).ThenInclude(r => r.IdLevelNavigation)
                        .Include(c => c.ResolvingResult).ThenInclude(r => r.IdControlDetailNavigation)
                        .Include(c => c.ResolvingResult).ThenInclude(r => r.IdLevelNavigation)
                        .Include(c => c.ResolvingResult).ThenInclude(r => r.IdUserNavigation)
                        .FirstOrDefaultAsync(x => x.Id == ca.Id);
                }

                if (caLoaded == null)
                {
                    MessageBox.Show("Không tìm thấy phiếu hành động trong cơ sở dữ liệu.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Create and show the report window (the window expects a collection)
                Application.Current?.Dispatcher.Invoke(() =>
                {
                    try
                    {
                        var reportWindow = new CorrectiveActionReportWindow(new[] { caLoaded });

                        // set owner (prefer main window)
                        var main = Application.Current?.MainWindow;
                        if (main != null && main.IsVisible)
                            reportWindow.Owner = main;

                        reportWindow.ShowDialog();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Không thể mở báo cáo: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tạo báo cáo: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}