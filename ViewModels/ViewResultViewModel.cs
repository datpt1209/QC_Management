using Microsoft.EntityFrameworkCore;
using QC_Management.Models;
using QC_Management.Services;
using QC_Management.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Security.AccessControl;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using XAct.Library.Settings;

namespace QC_Management.ViewModels
{
    public class ViewResultViewModel : BaseViewModel
    {
        private QcManagmentContext _dbContext;

        private ObservableCollection<CalResult>? _CalList;
        public ObservableCollection<CalResult>? CalList { get => _CalList; set { _CalList = value; OnPropertyChanged(); } }

        private ObservableCollection<Result> _ResultViewList;
        public ObservableCollection<Result> ResultViewList { get => _ResultViewList; set { _ResultViewList = value; OnPropertyChanged(); RefreshCollectionView(); } }

        private ICollectionView _ResultViewCollection;
        public ICollectionView ResultViewCollection { get => _ResultViewCollection; set { _ResultViewCollection = value; OnPropertyChanged(); } }

        private ObservableCollection<Device> _DeviceList;
        public ObservableCollection<Device> DeviceList { get => _DeviceList; set { _DeviceList = value; OnPropertyChanged(); } }

        private List<int?> _IndexList = new List<int?>();
        public List<int?> IndexList { get => _IndexList; set { _IndexList = value; OnPropertyChanged(); } }

        private List<LevelQc> _LevelList;
        public List<LevelQc> LevelList { get => _LevelList; set { _LevelList = value; OnPropertyChanged(); } }

        private string _SelectedResultType;
        public string SelectedResultType
        {
            get => _SelectedResultType;
            set
            {
                _SelectedResultType = value;
                OnPropertyChanged();
                UpdateDataGridSource();
            }
        }

        private Visibility _Visibility1;
        public Visibility Visibility1 { get => _Visibility1; set { _Visibility1 = value; OnPropertyChanged(); } }

        private Visibility _Visibility2;
        public Visibility Visibility2 { get => _Visibility2; set { _Visibility2 = value; OnPropertyChanged(); } }
        public ObservableCollection<string> ResultTypes { get; set; }
        private void UpdateDataGridSource()
        {
            if (SelectedResultType == "QC")
            {
                Visibility1 = Visibility.Visible;
                Visibility2 = Visibility.Collapsed;
            }
            else
            {
                Visibility1 = Visibility.Collapsed;
                Visibility2 = Visibility.Visible;
            }
        }

        public ICommand ViewCommand { get; set; }
        public ICommand LoadedCommand { get; set; }
        public ICommand PrintCommand { get; set; }
        public ICommand EditCommand { get; set; }
        public ICommand DeleteCommand { get; set; }
        public ICommand LevelChangedCommand { get; set; }
        public ICommand DateChangedCommand { get; set; }
        public ICommand DeleteOneQCResultCommand { get; set; }
        public ICommand DeleteOneCalResultCommand { get; set; }
        public ICommand ResultTypeChangedCommand { get; set; }
        public ICommand AddCommand { get; set; }
        public ICommand DeviceSelectionChangedCommand { get; set; }
        public ICommand OpenIncidentCommand { get; set; }

        public ICommand EnableEditCommand { get; set; }
        public ICommand CancelEditCommand { get; set; }

        private bool _isEditing;
        public bool IsEditing
        {
            get => _isEditing;
            set
            {
                _isEditing = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsGridReadOnly));
            }
        }

        public bool IsGridReadOnly => !IsEditing;

        private int? _SelectedIndex;
        public int? SelectedIndex
        {
            get => _SelectedIndex;
            set
            {
                _SelectedIndex = value;
                OnPropertyChanged();
            }
        }

        private Result _SelectedItem;
        public Result SelectedItem
        {
            get => _SelectedItem;
            set
            {
                _SelectedItem = value;
                OnPropertyChanged();
            }
        }

        private CalResult _SelectedCalResult;
        public CalResult SelectedCalResult
        {
            get => _SelectedCalResult;
            set
            {
                _SelectedCalResult = value;
                OnPropertyChanged();
            }
        }

        private Device _SelectedDevice;
        public Device SelectedDevice
        {
            get => _SelectedDevice;
            set
            {
                _SelectedDevice = value;
                OnPropertyChanged();

                if (_SelectedDevice != null)
                {
                    SelectedLevel = null;
                    IndexList = new List<int?>();
                    _ = UpdateLevelsByDeviceAsync(_SelectedDevice.Id);
                }
                else
                {
                    LevelList = new List<LevelQc>();
                    IndexList = new List<int?>();
                }
            }
        }

        // Snapshot now includes the three flags (nullable bool)
        private record ResultSnapshot(string? TempResult, string? Comment, string? WestgardRule, bool? IsExclude, double? Result1, bool? IsOutRange, string? QualitativeResult, bool? IsReagentReplaced, bool? IsReagentLotChanged, bool? IsCalLotChanged);
        private readonly Dictionary<int, ResultSnapshot> _originalResultSnapshot = new();

        private LevelQc _SelectedLevel;
        public LevelQc? SelectedLevel
        {
            get => _SelectedLevel;
            set
            {
                _SelectedLevel = value;
                OnPropertyChanged();
            }
        }

        private DateTime _SelectedDate = DateTime.Now;
        public DateTime SelectedDate
        {
            get => _SelectedDate;
            set
            {
                _SelectedDate = value;
                OnPropertyChanged();

                if (SelectedDevice != null)
                {
                    SelectedLevel = null;
                    IndexList = new List<int?>();
                    _ = UpdateLevelsByDeviceAsync(SelectedDevice.Id);
                }
            }
        }
        public ViewResultViewModel()
        {
            ResultViewList = new ObservableCollection<Result>();
            ResultTypes = new ObservableCollection<string> { "CALIB", "QC" };
            SelectedResultType = "QC";
            _dbContext = new QcManagmentContext();

            ResultViewCollection = CollectionViewSource.GetDefaultView(ResultViewList ?? new ObservableCollection<Result>());

            IsEditing = false;

            EnableEditCommand = new RelayCommand<object>((p) =>
            {
                return ResultViewList != null && ResultViewList.Count > 0;
            }, (p) =>
            {
                _originalResultSnapshot.Clear();
                if (ResultViewList != null)
                {
                    foreach (var it in ResultViewList)
                    {
                        if (it == null) continue;
                        _originalResultSnapshot[it.Id] = new ResultSnapshot(
                            it.TempResult,
                            it.Comment,
                            it.WestgardRule,
                            it.IsExclude,
                            it.Result1,
                            it.IsOutRange,
                            it.QualitativeResult,
                            it.IsReagentReplaced,
                            it.IsReagentLotChanged,
                            it.IsCalLotChanged
                        );
                    }
                }

                IsEditing = true;
            });

            CancelEditCommand = new RelayCommand<object>((p) =>
            {
                return true;
            }, (p) =>
            {
                IsEditing = false;
                Reload();
            });

            LoadedCommand = new RelayCommand<ControlInfoDetail>((p) =>
            {
                return true;

            }, async (p) =>
            {
                await LoadNew();
            });

            ViewCommand = new RelayCommand<ControlInfoDetail>((p) =>
            {
                if (SelectedResultType == "CALIB")
                {
                    return SelectedDevice != null;
                }
                else
                {
                    return SelectedDevice != null && SelectedLevel != null;
                }
            }, (p) =>
            {
                using var db = new QcManagmentContext();

                if (SelectedResultType == "CALIB")
                {
                    CalList = new ObservableCollection<CalResult>(db.CalResults
                        .AsNoTracking()
                        .Include(s => s.IdDeviceNavigation)
                        .Include(s => s.IdTestNavigation)
                        .Include(s => s.IdUserNavigation)
                        .Include(s => s.IdCalDetailNavigation)
                        .Include(s => s.IdTestNavigation.IdUnitTableNavigation)
                        .Include(s => s.IdCalDetailNavigation.IdCalInforNavigation.IdCalTypeNavigation)
                        .Include(s => s.IdCalDetailNavigation.IdCalInforNavigation)
                        .Where(s => s.IdDevice == SelectedDevice.Id && s.DateRun == SelectedDate)
                        .ToList());
                    if (CalList == null || CalList.Count == 0)
                    {
                        MessageBox.Show("No data", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                else
                {
                    FilterResults(db);
                }
            });

            PrintCommand = new RelayCommand<object>((p) =>
            {
                if (SelectedResultType == "CALIB")
                {
                    return false;
                }
                else
                {
                    if (ResultViewList.Count == 0 || ResultViewList == null) return false;
                    else
                        return true;
                }
            }, (p) =>
            {
                if (SelectedResultType == "CALIB")
                {
                    CalibReportView rp = new CalibReportView(CalList.ToList());
                    rp.ShowDialog();
                }
                else
                {
                    ReivewReportView rp = new ReivewReportView(ResultViewList.ToList());
                    rp.ShowDialog();
                }
            });

            DeviceSelectionChangedCommand = new RelayCommand<object>((p) =>
            {
                if (SelectedResultType == "CALIB")
                {
                    return false;
                }
                else
                {
                    return SelectedDevice != null;
                }
            }, async (p) =>
            {
                await UpdateLevelsByDeviceAsync(SelectedDevice.Id);

                if (SelectedLevel != null)
                {
                    IndexList = LoadIndexList(_dbContext);
                }
            });

            ResultTypeChangedCommand = new RelayCommand<object>((p) =>
            {
                if (SelectedDevice == null) return false;
                return true;

            }, async (p) =>
            {
                await UpdateLevelsByDeviceAsync(SelectedDevice.Id);

            });

            EditCommand = new RelayCommand<object>((p) =>
            {
                if (SelectedResultType == "CALIB")
                {
                    if (SelectedDevice == null || SelectedCalResult == null) return false;
                    else
                        return true;
                }
                else
                {
                    if (SelectedDevice == null || SelectedLevel == null) return false;
                    else
                        return true;
                }
            }, (p) =>
            {
                bool saved = false;

                if (SelectedResultType == "CALIB")
                {
                    try
                    {
                        foreach (var item in CalList)
                        {
                            var editResult = DataProvider.Ins.DB.CalResults.Where(s => s.Id == item.Id).FirstOrDefault();
                            if (editResult != null)
                            {
                                editResult.Result = item.Result;
                                editResult.Comment = item.Comment;
                                editResult.isOutOfRange = item.isOutOfRange;
                            }
                        }

                        DataProvider.Ins.DB.SaveChanges();

                        try { RefreshCollectionView(); } catch { }

                        saved = true;
                        MessageBox.Show("Cập nhật thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error: {ex}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Stop);
                    }
                    finally
                    {
                        if (saved)
                        {
                            IsEditing = false;
                            _originalResultSnapshot.Clear();
                        }
                    }
                }
                else if (SelectedResultType == "QC")
                {
                    try
                    {
                        var anyChanged = false;

                        foreach (var item in ResultViewList)
                        {
                            if (item == null) continue;

                            var changed = true;
                            if (_originalResultSnapshot.TryGetValue(item.Id, out var orig))
                            {
                                bool seqEqual(string? a, string? b) => string.Equals(a ?? string.Empty, b ?? string.Empty, StringComparison.Ordinal);
                                changed = !(
                                    seqEqual(item.TempResult, orig.TempResult) &&
                                    seqEqual(item.Comment, orig.Comment) &&
                                    seqEqual(item.WestgardRule, orig.WestgardRule) &&
                                    item.IsExclude == orig.IsExclude &&
                                    NullableEquals(item.Result1, orig.Result1) &&
                                    NullableEquals(item.IsOutRange, orig.IsOutRange) &&
                                    seqEqual(item.QualitativeResult, orig.QualitativeResult) &&
                                    NullableEquals(item.IsReagentReplaced, orig.IsReagentReplaced) &&
                                    NullableEquals(item.IsReagentLotChanged, orig.IsReagentLotChanged) &&
                                    NullableEquals(item.IsCalLotChanged, orig.IsCalLotChanged)
                                );
                            }

                            if (!changed) continue;

                            anyChanged = true;

                            var editResult = DataProvider.Ins.DB.Results.Where(s => s.Id == item.Id).FirstOrDefault();
                            if (editResult != null)
                            {
                                editResult.TempResult = item.TempResult;
                                editResult.Comment = item.Comment;
                                editResult.WestgardRule = string.IsNullOrWhiteSpace(item.WestgardRule) ? null : item.WestgardRule;
                                editResult.IsExclude = item.IsExclude;

                                double? numeric = item.Result1;
                                if (!numeric.HasValue && !string.IsNullOrWhiteSpace(item.TempResult))
                                {
                                    if (double.TryParse(item.TempResult.Trim(), out var parsed))
                                        numeric = parsed;
                                }
                                editResult.Result1 = numeric;

                                editResult.QualitativeResult = item.QualitativeResult;

                                editResult.IsOutRange = item.IsOutRange;

                                // NEW: copy the three flags into tracked entity
                                editResult.IsReagentReplaced = item.IsReagentReplaced;
                                editResult.IsReagentLotChanged = item.IsReagentLotChanged;
                                editResult.IsCalLotChanged = item.IsCalLotChanged;

                                try
                                {
                                    ControlInfoDetail? ctrl = editResult.IdControlDetailNavigation;
                                    if (ctrl == null && editResult.IdControlDetail.HasValue)
                                    {
                                        ctrl = DataProvider.Ins.DB.ControlInfoDetails
                                                    .AsNoTracking()
                                                    .FirstOrDefault(c => c.Id == editResult.IdControlDetail.Value);
                                    }

                                    if (numeric.HasValue && ctrl != null)
                                    {
                                        double? meanToApply = ctrl.CurMean ?? ctrl.MeanApp ?? ctrl.MeanNsx;
                                        double? sdToApply = ctrl.CurSd ?? ctrl.SdApp ?? ctrl.SdNsx;
                                        if (meanToApply.HasValue && sdToApply.HasValue)
                                        {
                                            var sdVal = sdToApply.Value == 0 ? 0.0001 : sdToApply.Value;
                                            editResult.ZScore = Math.Round((numeric.Value - meanToApply.Value) / sdVal, 4);
                                        }
                                        else
                                        {
                                            editResult.ZScore = null;
                                        }
                                    }
                                    else
                                    {
                                        editResult.ZScore = null;
                                    }
                                }
                                catch
                                {
                                }

                                editResult.IsCorrected = (!string.IsNullOrWhiteSpace(editResult.WestgardRule) || editResult.IsOutRange == true)
                                    ? (bool?)false
                                    : null;

                                try
                                {
                                    var isProblematic = editResult.IsOutRange == true || !string.IsNullOrWhiteSpace(editResult.WestgardRule);
                                    if (isProblematic)
                                    {
                                        var exists = DataProvider.Ins.DB.InternalErrors.Any(i => i.ErroneousResultId == editResult.Id);
                                        if (!exists)
                                        {
                                            var cid = editResult.IdControlDetailNavigation;
                                            var ie = new InternalError
                                            {
                                                ErroneousResultId = editResult.Id,
                                                TestId = editResult.IdTest,
                                                DeviceId = editResult.IdDevice,
                                                ControlInfoDetailId = editResult.IdControlDetail,
                                                Lot = cid?.Lot,
                                                WestgardDescription = !string.IsNullOrWhiteSpace(editResult.WestgardRule)
                                                    ? editResult.WestgardRule
                                                    : (editResult.IsOutRange == true ? "Out-of-range" : null),
                                                RelatedResultsJson = System.Text.Json.JsonSerializer.Serialize(new { Id = editResult.Id, TestId = editResult.IdTest, TempResult = editResult.TempResult }),
                                                IsResolved = false,
                                                Status = "Đang chờ",
                                                CreatedAt = editResult.DateRun == default ? DateTime.UtcNow : editResult.DateRun,
                                                CreatedBy = UserManager.Instance?.CurrentUser?.DisplayName ?? UserManager.Instance?.CurrentUser?.Id.ToString()
                                            };

                                            DataProvider.Ins.DB.InternalErrors.Add(ie);
                                        }
                                    }
                                }
                                catch
                                {
                                }
                            }
                        }

                        if (!anyChanged)
                        {
                            MessageBox.Show("Không có thay đổi để lưu.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            DataProvider.Ins.DB.SaveChanges();

                            try { RefreshCollectionView(); } catch { }

                            saved = true;
                            MessageBox.Show("Cập nhật thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error: {ex}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Stop);
                    }
                    finally
                    {
                        if (saved)
                        {
                            IsEditing = false;
                            _originalResultSnapshot.Clear();
                        }
                    }
                }

                static bool NullableEquals<T>(T? a, T? b) where T : struct
                {
                    if (a.HasValue != b.HasValue) return false;
                    return !a.HasValue || a.Value.Equals(b.Value);
                }
            });


            AddCommand = new RelayCommand<Result>((p) =>
            {
                if (ResultViewList.Count == 0 || ResultViewList == null) return false;
                else
                    return true;
            },
            (p) =>
            {
                OpenAddResultWindow();
            }
            );

            DeleteCommand = new RelayCommand<object>((p) =>
            {
                if (ResultViewList.Count == 0 || ResultViewList == null) return false;
                else
                    return true;

            }, async (p) =>
            {
                var deleteItem = ResultViewList.ToList();
                MessageBoxResult result = MessageBox.Show($"Bạn có muốn xóa các kết quả máy: {SelectedDevice.Name}, Level: {SelectedLevel.Name}, Ngày: {SelectedDate.Date} Index: {SelectedIndex.ToString()}?", "Confirmation", MessageBoxButton.YesNo);
                if (result != MessageBoxResult.Yes) return;

                try
                {
                    var ids = deleteItem.Select(d => d.Id).ToList();

                    await using (var context = new QcManagmentContext())
                    {
                        // gather related entities (async)
                        var internalErrors = await context.InternalErrors
                            .Where(i => i.ErroneousResultId.HasValue && ids.Contains(i.ErroneousResultId.Value))
                            .ToListAsync();

                        var internalErrorIds = internalErrors.Select(i => i.Id).ToList();

                        // corrective actions referencing the results OR referencing those internal errors
                        var caByResolving = await context.CorrectiveActions
                            .Where(c => c.ResolvingResultId != null && ids.Contains(c.ResolvingResultId.Value))
                            .ToListAsync();

                        var caByInternalError = await context.CorrectiveActions
                            .Where(c => internalErrorIds.Contains(c.InternalErrorId)) // use property directly (no .Value)
                            .ToListAsync();

                        var correctiveActionsToDelete = caByResolving.Union(caByInternalError).ToList();

                        int totalCA = correctiveActionsToDelete.Count;
                        int totalIE = internalErrors.Count;

                        if (totalIE > 0 || totalCA > 0)
                        {
                            var msg = $"There are {ids.Count} result(s) to delete.\n" +
                                $"Linked InternalErrors: {totalIE}\n" +
                                $"Linked CorrectiveActions: {totalCA}\n\n" +
                                $"Do you want to delete these related InternalError/CorrectiveAction records as well? (Yes = delete related, No = cancel)";
                            var confirmRelated = MessageBox.Show(msg, "Confirm related deletion", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                            if (confirmRelated != MessageBoxResult.Yes)
                            {
                                // user cancelled deletion of related items -> abort whole delete
                                MessageBox.Show("Deletion cancelled.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                                return;
                            }

                            // Track internal error ids referenced by CAs we will remove
                            var affectedInternalErrorIds = correctiveActionsToDelete
                                .Select(c => (int?)c.InternalErrorId)
                                .Where(id => id.HasValue)
                                .Select(id => id.Value)
                                .Distinct()
                                .ToList();

                            await using var tx = await context.Database.BeginTransactionAsync();
                            try
                            {
                                // Delete corrective actions first
                                if (correctiveActionsToDelete.Any())
                                    context.CorrectiveActions.RemoveRange(correctiveActionsToDelete);

                                // Then delete internal errors
                                if (internalErrors.Any())
                                    context.InternalErrors.RemoveRange(internalErrors);

                                // Finally delete the results
                                var entitiesToDelete = await context.Results.Where(r => ids.Contains(r.Id)).ToListAsync();
                                if (entitiesToDelete.Any())
                                    context.Results.RemoveRange(entitiesToDelete);

                                await context.SaveChangesAsync();

                                // Re-evaluate remaining corrective actions for affected internal errors
                                var remainingAffectedInternalIds = affectedInternalErrorIds
                                    .Except(internalErrors.Select(i => i.Id))
                                    .ToList();

                                if (remainingAffectedInternalIds.Any())
                                {
                                    foreach (var ieId in remainingAffectedInternalIds)
                                    {
                                        var ie = await context.InternalErrors.FirstOrDefaultAsync(x => x.Id == ieId);
                                        if (ie == null) continue;

                                        // any corrective actions still reference this internal error?
                                        var remainingCAExists = await context.CorrectiveActions
                                            .AsNoTracking()
                                            .AnyAsync(c => c.InternalErrorId == ieId);

                                        ie.IsResolved = remainingCAExists;
                                        ie.Status = remainingCAExists ? "Đã khắc phục" : "Đang chờ";

                                        context.InternalErrors.Update(ie);
                                    }
                                    await context.SaveChangesAsync();
                                }

                                await tx.CommitAsync();
                            }
                            catch
                            {
                                try { await tx.RollbackAsync(); } catch { /* swallow */ }
                                throw;
                            }
                        }
                        else
                        {
                            // No related entities; proceed with safe delete
                            await using var tx2 = await context.Database.BeginTransactionAsync();
                            try
                            {
                                var entitiesToDelete = await context.Results.Where(r => ids.Contains(r.Id)).ToListAsync();
                                if (entitiesToDelete.Any())
                                    context.Results.RemoveRange(entitiesToDelete);

                                await context.SaveChangesAsync();
                                await tx2.CommitAsync();
                            }
                            catch
                            {
                                try { await tx2.RollbackAsync(); } catch { /* swallow */ }
                                throw;
                            }
                        }
                    }

                    MessageBox.Show("Xóa thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    Reload();
                    using var fresh = new QcManagmentContext();
                    FilterResults(fresh);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Stop);
                }
            });

            DeleteOneQCResultCommand = new RelayCommand<Result>((p) =>
            {
                if (SelectedItem == null) return false;
                else return true;
            }, async (p) =>
            {
                await DeleteQCResult(p ?? SelectedItem);
            });

            DeleteOneCalResultCommand = new RelayCommand<CalResult>((p) =>
            {
                if (SelectedCalResult == null) return false;
                else return true;
            }, (p) =>
            {
                DeleteCalResult(SelectedCalResult);
            });

            LevelChangedCommand = new RelayCommand<ControlInfoDetail>((p) =>
            {
                IndexList = new List<int?>();
                if (SelectedDevice == null || SelectedLevel == null) return false;
                else return true;

            }, (p) =>
            {
                IndexList = LoadIndexList(_dbContext);
            });

            DateChangedCommand = new RelayCommand<ControlInfoDetail>((p) =>
            {
                return true;

            }, async (p) =>
            {
                SelectedLevel = null;
                if (SelectedDevice != null)
                {
                    await UpdateLevelsByDeviceAsync(SelectedDevice.Id);
                }
            });
        }
        private List<int?> LoadIndexList(QcManagmentContext DB)
        {
            var IndexList = new List<int?>();
            var listTest = DB.Results.Where(s => s.IdDevice == SelectedDevice.Id
                            && s.DateRun.Date == SelectedDate.Date
                            && s.IdLevel == SelectedLevel.Id)
            .GroupBy(s => s.IndexQc).Select(s => s.Key).ToList();

            if (listTest != null)
            {
                IndexList = listTest;
            }
            return IndexList;
        }
        public async Task UpdateLevelsByDeviceAsync(int deviceId)
        {
            using (var dbContext = new QcManagmentContext())
            {
                var levels = await dbContext.Results
                                            .Where(c => c.IdDevice == deviceId && c.DateRun.Date == SelectedDate.Date)
                                            .Select(c => new LevelQc
                                            {
                                                Id = c.IdLevel,
                                                Name = c.IdLevelNavigation.Name
                                            })
                                            .Distinct()
                                            .ToListAsync();
                LevelList = levels;
            }
        }
        private void OpenAddResultWindow()
        {
            QcManagmentContext DB = DataProvider.Ins.DB;
            var addResultWindow = new AddResultWindow();
            var viewModel = new AddResultViewModel(SelectedDate, SelectedDevice, SelectedLevel, SelectedIndex, addResultWindow);
            addResultWindow.DataContext = viewModel;
            if (addResultWindow.ShowDialog() == true)
            {
                Reload();
                FilterResults(DB);
            }
        }
        private async Task DeleteQCResult(Result result)
        {
            if (result == null) return;

            // ask confirmation
            var messageBoxResult = MessageBox.Show("Are you sure you want to delete this item?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (messageBoxResult == MessageBoxResult.No) return;

            try
            {
                await using (var context = new QcManagmentContext())
                {
                    // find related entities (async)
                    var internalErrors = await context.InternalErrors
                        .Where(i => i.ErroneousResultId.HasValue && i.ErroneousResultId.Value == result.Id)
                        .ToListAsync();

                    var internalErrorIds = internalErrors.Select(i => i.Id).ToList();

                    var caByResolving = await context.CorrectiveActions
                        .Where(c => c.ResolvingResultId != null && c.ResolvingResultId.Value == result.Id)
                        .ToListAsync();

                    var caByInternalError = await context.CorrectiveActions
                        .Where(c => internalErrorIds.Contains(c.InternalErrorId)) // use property directly
                        .ToListAsync();

                    var correctiveActionsToDelete = caByResolving.Union(caByInternalError).ToList();

                    int totalCA = correctiveActionsToDelete.Count;
                    int totalIE = internalErrors.Count;

                    if (totalIE > 0 || totalCA > 0)
                    {
                        var msg = $"There is 1 result to delete.\nLinked InternalErrors: {totalIE}\nLinked CorrectiveActions: {totalCA}\n\nDo you want to delete these related records as well? (Yes = delete related, No = cancel)";
                        var confirmRelated = MessageBox.Show(msg, "Confirm related deletion", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                        if (confirmRelated != MessageBoxResult.Yes)
                        {
                            MessageBox.Show("Deletion cancelled.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                            return;
                        }

                        // track internal errors referenced by CA we'll delete
                        var affectedInternalErrorIds = correctiveActionsToDelete
                            .Select(c => (int?)c.InternalErrorId)
                            .Where(id => id.HasValue)
                            .Select(id => id.Value)
                            .Distinct()
                            .ToList();

                        await using var tx = await context.Database.BeginTransactionAsync();

                        try
                        {
                            // delete corrective actions
                            if (correctiveActionsToDelete.Any())
                                context.CorrectiveActions.RemoveRange(correctiveActionsToDelete);

                            // delete internal errors
                            if (internalErrors.Any())
                                context.InternalErrors.RemoveRange(internalErrors);

                            // delete the result
                            var entity = await context.Results.FindAsync(result.Id);
                            if (entity != null)
                                context.Results.Remove(entity);

                            await context.SaveChangesAsync();

                            // re-evaluate internal errors remaining (those affected but not removed)
                            var remainingAffectedInternalIds = affectedInternalErrorIds
                                .Except(internalErrors.Select(i => i.Id))
                                .ToList();

                            if (remainingAffectedInternalIds.Any())
                            {
                                foreach (var ieId in remainingAffectedInternalIds)
                                {
                                    var ie = await context.InternalErrors.FirstOrDefaultAsync(x => x.Id == ieId);
                                    if (ie == null) continue;

                                    var remainingCAExists = await context.CorrectiveActions
                                        .AsNoTracking()
                                        .AnyAsync(c => c.InternalErrorId == ieId);

                                    ie.IsResolved = remainingCAExists;
                                    ie.Status = remainingCAExists ? "Đã khắc phục" : "Đang chờ";

                                    context.InternalErrors.Update(ie);
                                }
                                await context.SaveChangesAsync();
                            }

                            await tx.CommitAsync();
                        }
                        catch
                        {
                            try { await tx.RollbackAsync(); } catch { /* swallow */ }
                            throw;
                        }
                    }
                    else
                    {
                        // no related items; delete result directly
                        await using var tx2 = await context.Database.BeginTransactionAsync();
                        try
                        {
                            var entity = await context.Results.FindAsync(result.Id);
                            if (entity != null)
                                context.Results.Remove(entity);

                            await context.SaveChangesAsync();
                            await tx2.CommitAsync();
                        }
                        catch
                        {
                            try { await tx2.RollbackAsync(); } catch { /* swallow */ }
                            throw;
                        }
                    }
                }

                // Remove from the ObservableCollection (UI) on UI thread
                Application.Current.Dispatcher.Invoke(() =>
                {
                    ResultViewList.Remove(result);
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting result: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private async void DeleteCalResult(CalResult calResult)
        {
            if (calResult == null) return;

            var messageBoxResult = MessageBox.Show("Are you sure you want to delete this item?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (messageBoxResult == MessageBoxResult.No) return;
            // Remove from the database
            try
            {
                using (var context = new QcManagmentContext())
                {
                    // CalResult is not referenced by InternalError/CorrectiveAction - safe to delete directly
                    var entity = await context.CalResults.FindAsync(calResult.Id);
                    if (entity != null)
                    {
                        context.CalResults.Remove(entity);
                        await context.SaveChangesAsync();
                    }
                }
                // Remove from the ObservableCollection
                CalList.Remove(calResult);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting result: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void FilterResults(QcManagmentContext DB)
        {
            if (SelectedDevice == null || SelectedLevel == null) return;
            // Build base query and include related navigation properties
            var query = DB.Results
                          .AsNoTracking()
                          .Include(s => s.IdTestNavigation)
                          .Include(s => s.IdDeviceNavigation)
                          .Include(s => s.IdUserNavigation)
                          .Include(s => s.IdLevelNavigation)
                          .Include(s => s.IdTestNavigation.IdUnitTableNavigation)
                          .Include(s => s.IdControlDetailNavigation.IdControlInfoNavigation)
                          .Where(s => s.IdDevice == SelectedDevice.Id
                                      && s.IdLevel == SelectedLevel.Id
                                      && s.DateRun.Date == SelectedDate.Date);

            // Apply index filter only when selected and non-zero (preserve existing semantics)
            if (SelectedIndex != null && SelectedIndex != 0)
            {
                query = query.Where(s => s.IndexQc == SelectedIndex);
            }

            // Order results primarily by IndexQc (ascending) then by Time so the DataGrid shows index rows in sequence.
            var list = query.OrderBy(s => s.IndexQc ?? 0).ThenBy(s => s.Time ?? TimeSpan.Zero).ToList();

            ResultViewList = new ObservableCollection<Result>(list);

            // Update IndexList for UI selection
            IndexList = LoadIndexList(DB);

            // If no index selected, enable grouping by Index in the ICollectionView so the UI shows an index "row"
            // (XAML DataGrid will display groups if configured).
            RefreshCollectionView();

            if (ResultViewList == null || ResultViewList.Count == 0)
            {
                SelectedIndex = null;
                IndexList = LoadIndexList(DB);
                MessageBox.Show("No data", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        private async Task LoadNew()
        {
            try
            {
                _dbContext = await Task.Run(() => DataProvider.Ins.DB);
                DeviceList = new ObservableCollection<Device>(_dbContext.Devices);
                ResultViewList = new ObservableCollection<Result>();
                // ensure collection view initialized
                RefreshCollectionView();
            }
            catch (Exception ex)
            {
                // Handle exceptions
            }
        }
        public void Reload()
        {
            QcManagmentContext DB = DataProvider.Ins.DB;
            ResultViewList = new ObservableCollection<Result>();
            CalList = new ObservableCollection<CalResult>();
            RefreshCollectionView();
        }
        // RefreshCollectionView: apply sorting and optional grouping by IndexQc when SelectedIndex is not chosen.
        private void RefreshCollectionView()
        {
            if (ResultViewList == null)
            {
                ResultViewCollection = CollectionViewSource.GetDefaultView(new ObservableCollection<Result>());
                return;
            }

            var view = CollectionViewSource.GetDefaultView(ResultViewList);
            if (view == null) return;

            // Clear existing sort and group
            view.SortDescriptions.Clear();
            view.GroupDescriptions.Clear();

            // Always sort by IndexQc then by Time for predictable ordering
            view.SortDescriptions.Add(new SortDescription(nameof(Result.IndexQc), ListSortDirection.Ascending));
            view.SortDescriptions.Add(new SortDescription(nameof(Result.Time), ListSortDirection.Ascending));

            // If no specific index is selected, group by IndexQc so the DataGrid can show index "rows"
            if (SelectedIndex == null)
            {
                // GroupDescription will use the raw IndexQc value (nulls will be shown as blank group)
                view.GroupDescriptions.Add(new PropertyGroupDescription(nameof(Result.IndexQc)));
            }

            ResultViewCollection = view;
        }

        // Add this helper method somewhere inside the ViewResultViewModel class (e.g. after RefreshCollectionView).
        // It mirrors the Westgard evaluation in ResultViewModel but operates on the UI's Result instance (detached).
        private async Task<(bool? isOutRange, string? westgardRule)> EvaluateWestgardForResultAsync(Result candidate)
        {
            if (candidate == null) return (null, null);

            try
            {
                // Use non-nullable ids from candidate (Result.IdTest/IdDevice/IdLevel are int)
                var testId = candidate.IdTest;
                var deviceId = candidate.IdDevice;
                var levelId = candidate.IdLevel;

                // Guard: require positive ids
                if (testId <= 0 || deviceId <= 0 || levelId <= 0)
                {
                    return (null, null);
                }

                // Prepare 'current' result object similar to ResultViewModel behavior
                var current = new Result
                {
                    IdTest = candidate.IdTest,
                    ResultType = candidate.ResultType,
                    IdTestNavigation = candidate.IdTestNavigation,
                    IdDevice = candidate.IdDevice,
                    IdLevel = candidate.IdLevel,
                    DateRun = candidate.DateRun == default ? DateTime.Now : candidate.DateRun,
                    Time = candidate.Time,
                    IdUser = candidate.IdUser,
                    IndexQc = candidate.IndexQc,
                    IdControlDetail = candidate.IdControlDetail,
                    IdControlDetailNavigation = candidate.IdControlDetailNavigation,
                    TempResult = candidate.TempResult
                };

                // compute ZScore if quantitative and numeric
                if (current.ResultType == 2)
                {
                    if (current.TempResult != null && double.TryParse(current.TempResult, out var parsed))
                    {
                        current.Result1 = parsed;
                        var ctrl = current.IdControlDetailNavigation;
                        if (ctrl != null && ctrl.CurMean.HasValue && ctrl.CurSd.HasValue && ctrl.CurSd.Value != 0)
                        {
                            current.ZScore = Math.Round((parsed - ctrl.CurMean.Value) / ctrl.CurSd.Value, 4);
                        }
                        else current.ZScore = null;
                    }
                    else current.ZScore = null;
                }
                else
                {
                    current.ZScore = null;
                }

                // Load recent history (same device/test). Use a fresh context for history query.
                List<Result> recent;
                using (var db = new QcManagmentContext())
                {
                    recent = await db.Results
                        .AsNoTracking()
                        .Include(r => r.IdControlDetailNavigation)
                        .Where(r => r.IdTest == testId && r.IdDevice == deviceId && r.IsExclude != true)
                        .OrderByDescending(r => r.DateRun)
                        .ThenByDescending(r => r.IndexQc ?? 0)
                        .ThenByDescending(r => r.Time ?? TimeSpan.Zero)
                        .Take(10)
                        .ToListAsync();
                }

                var sameLevelPrev = recent.Where(r => r.IdLevel == levelId).ToList();
                var crossLevelPrev = recent;

                // Load per-device/test enabled rules from DeviceTest.WestgardRulesJson (if any)
                IEnumerable<string>? enabledRules = null;
                try
                {
                    using var db2 = new QcManagmentContext();
                    var dt = await db2.DeviceTests
                                     .AsNoTracking()
                                     .Where(d => d.IdDevice == deviceId && d.IdTest == testId)
                                     .Select(d => new { d.WestgardRulesJson })
                                     .FirstOrDefaultAsync();

                    if (dt != null && !string.IsNullOrWhiteSpace(dt.WestgardRulesJson))
                    {
                        try
                        {
                            var parsed = System.Text.Json.JsonSerializer.Deserialize<List<string>>(dt.WestgardRulesJson);
                            if (parsed != null && parsed.Count > 0)
                            {
                                enabledRules = parsed.Select(s => s?.Trim()).Where(s => !string.IsNullOrEmpty(s)).ToList();
                            }
                            else enabledRules = null;
                        }
                        catch
                        {
                            enabledRules = null;
                        }
                    }
                }
                catch
                {
                    enabledRules = null;
                }

                var keysToEvaluate = (enabledRules != null)
                    ? enabledRules.ToList()
                    : new List<string> { "1_3S", "1_2S", "2_2S", "R-4s", "4_1S", "10X" };

                var aggViolations = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                bool aggIsOutRange = false;

                foreach (var rk in keysToEvaluate)
                {
                    try
                    {
                        var part = LeveyJenningsChecker.EvaluateSingleRule(current, sameLevelPrev, crossLevelPrev, rk);
                        if (part?.ViolatedRules != null)
                        {
                            foreach (var v in part.ViolatedRules)
                                aggViolations.Add(v);
                        }
                        aggIsOutRange = aggIsOutRange || part.IsOutRange;
                    }
                    catch
                    {
                        // ignore rule failures
                    }
                }

                var ordered = aggViolations.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
                if (ordered.Contains("1_2S") && ordered.Contains("1_3S"))
                {
                    ordered.Remove("1_2S");
                    ordered.Insert(ordered.IndexOf("1_3S") + 1, "1_2S");
                }

                var westgardRule = ordered.Count > 0 ? string.Join(", ", ordered) : null;

                // For qualitative tests, keep existing IsOutRange computed elsewhere (caller may have set),
                // but if Levey check indicates out-of-range we reflect that as well.
                return (aggIsOutRange ? (bool?)true : (bool?)false, westgardRule);
            }
            catch
            {
                return (null, null);
            }
        }
        // Update CheckWestgardForItemAsync: refresh the ICollectionView instead of replacing the collection item.
        public async Task CheckWestgardForItemAsync(Result item)
        {
            if (item == null) return;

            try
            {
                // 1) For qualitative results attempt quick acceptability check from control detail (best-effort)
                if (item.ResultType != 2)
                {
                    try
                    {
                        var ctrl = item.IdControlDetailNavigation;
                        if (ctrl == null && item.IdControlDetail.HasValue)
                        {
                            using var db = new QcManagmentContext();
                            ctrl = await db.ControlInfoDetails
                                           .AsNoTracking()
                                           .FirstOrDefaultAsync(c => c.Id == item.IdControlDetail.Value);
                        }

                        if (ctrl != null && !string.IsNullOrWhiteSpace(item.TempResult))
                        {
                            try
                            {
                                var acceptable = ctrl.IsQualitativeResultAcceptable(item.TempResult.Trim());
                                item.IsOutRange = !acceptable;
                            }
                            catch
                            {
                                // ignore acceptance errors, fall back to Levey evaluation below
                            }
                        }
                    }
                    catch
                    {
                        // swallow - best-effort
                    }
                }

                // 2) Run Levey/Jennings evaluation to compute WestgardRule/isOutRange (mirrors ResultViewModel logic)
                var eval = await EvaluateWestgardForResultAsync(item);

                // Apply evaluation results to the item on UI thread and refresh collection so DataGrid updates.
                Application.Current?.Dispatcher.Invoke(() =>
                {
                    if (eval.isOutRange.HasValue)
                        item.IsOutRange = eval.isOutRange.Value;

                    item.WestgardRule = string.IsNullOrWhiteSpace(eval.westgardRule) ? null : eval.westgardRule;

                    // Recompute IsCorrected display state: follow existing policy (not-corrected if there is a rule or out-of-range)
                    item.IsCorrected = (!string.IsNullOrWhiteSpace(item.WestgardRule) || item.IsOutRange == true) ? (bool?)false : null;

                    // Refresh the view instead of replacing the item to avoid losing selection/grouping state
                    try
                    {
                        ResultViewCollection?.Refresh();
                    }
                    catch
                    {
                        // fallback: update the observable collection element (only if refresh fails)
                        var idx = ResultViewList?.IndexOf(item) ?? -1;
                        if (idx >= 0)
                        {
                            ResultViewList[idx] = ResultViewList[idx];
                        }
                    }
                });
            }
            catch
            {
                // non-fatal
            }
        }
    }
}