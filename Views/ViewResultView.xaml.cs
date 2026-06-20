using QC_Management.Models;
using QC_Management.ViewModels;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace QC_Management.Views
{
    /// <summary>
    /// Interaction logic for ViewResult.xaml
    /// </summary>
    public partial class ViewResultView : UserControl
    {
        public ViewResultView()
        {
            InitializeComponent();
        }
        // Handle Enter: commit cell edit only, trigger Westgard check for the edited item (await),
        // then move to same column next row and begin edit. Avoid replacing collection item.
        private async void TempResult_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;

            e.Handled = true;

            if (sender is not TextBox tb) return;

            var dg = FindAncestor<DataGrid>(tb);
            if (dg == null) return;

            // Commit current cell edit only (do not commit full row to avoid DataGrid collapsing/grouping side-effects)
            dg.CommitEdit(DataGridEditingUnit.Cell, true);

            // Find row and cell
            var cell = FindAncestor<DataGridCell>(tb);
            var row = FindAncestor<DataGridRow>(tb);
            if (row == null) return;

            // Trigger immediate Westgard evaluation for the row item and await it so UI updates before moving
            if (dg.DataContext is ViewResultViewModel vm && row.Item is Result item)
            {
                try
                {
                    await vm.CheckWestgardForItemAsync(item);
                }
                catch
                {
                    // swallow - best-effort
                }
            }

            // Move to next row in same column
            int currentColIndex = cell?.Column?.DisplayIndex ?? 0;
            int currentRowIndex = dg.Items.IndexOf(row.Item);
            int nextRowIndex = currentRowIndex + 1;

            if (nextRowIndex < dg.Items.Count)
            {
                var nextItem = dg.Items[nextRowIndex];
                dg.SelectedItem = nextItem;
                dg.ScrollIntoView(nextItem);

                // set current cell to same column
                var targetColumn = dg.Columns.FirstOrDefault(c => c.DisplayIndex == currentColIndex) ?? dg.Columns.FirstOrDefault();
                if (targetColumn != null)
                {
                    dg.CurrentCell = new DataGridCellInfo(nextItem, targetColumn);
                    dg.BeginEdit();

                    // try to focus TextBox inside the target cell
                    await Task.Delay(50); // let visual tree update
                    var nextRow = (DataGridRow)dg.ItemContainerGenerator.ContainerFromItem(nextItem);
                    var nextCell = GetCell(dg, nextRow, targetColumn);
                    if (nextCell != null)
                    {
                        var nextTb = FindVisualChild<TextBox>(nextCell);
                        if (nextTb != null)
                        {
                            nextTb.Focus();
                            nextTb.SelectAll();
                        }
                    }
                }
            }
            else
            {
                // last row: commit the cell and leave focus (no row replacement)
                dg.CommitEdit(DataGridEditingUnit.Cell, true);
            }
        }
        // Add these event handlers into Views\ViewResultView.xaml.cs (place them in the class body)
        private void DataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            // Defer refresh until the edit is committed to the underlying source
            if (sender is DataGrid dg && dg.DataContext is ViewResultViewModel vm)
            {
                // Use background dispatcher to allow DataGrid to apply the edit first
                dg.Dispatcher.BeginInvoke(new System.Action(() =>
                {
                    try { vm.ResultViewCollection?.Refresh(); } catch { /* ignore */ }
                }), System.Windows.Threading.DispatcherPriority.Background);
            }
        }

        private void DataGrid_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            // When a row edit completes (user clicks away), refresh sorting/grouping
            if (sender is DataGrid dg && dg.DataContext is ViewResultViewModel vm)
            {
                dg.Dispatcher.BeginInvoke(new System.Action(() =>
                {
                    try { vm.ResultViewCollection?.Refresh(); } catch { /* ignore */ }
                }), System.Windows.Threading.DispatcherPriority.Background);
            }
        }

        // Helpers: find ancestor in visual tree
        private static T? FindAncestor<T>(DependencyObject? child) where T : DependencyObject
        {
            if (child == null) return null;
            var parent = VisualTreeHelper.GetParent(child);
            while (parent != null)
            {
                if (parent is T t) return t;
                parent = VisualTreeHelper.GetParent(parent);
            }
            return null;
        }

        // Helpers: find first visual child of type T
        private static T? FindVisualChild<T>(DependencyObject? parent) where T : DependencyObject
        {
            if (parent == null) return null;
            int count = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T t) return t;
                var result = FindVisualChild<T>(child);
                if (result != null) return result;
            }
            return null;
        }

        // Get DataGridCell from row + column
        private static DataGridCell? GetCell(DataGrid grid, DataGridRow? row, DataGridColumn column)
        {
            if (row == null) return null;
            var presenter = FindVisualChild<DataGridCellsPresenter>(row);
            if (presenter == null)
            {
                grid.ScrollIntoView(row, column);
                presenter = FindVisualChild<DataGridCellsPresenter>(row);
                if (presenter == null) return null;
            }

            // Try to get container by column index
            for (int i = 0; i < presenter.ItemContainerGenerator.Items.Count; i++)
            {
                var container = presenter.ItemContainerGenerator.ContainerFromIndex(i) as DataGridCell;
                if (container != null && container.Column == column) return container;
            }

            // fallback: use container from generator
            var cellObj = presenter.ItemContainerGenerator.ContainerFromIndex(column.DisplayIndex) as DataGridCell;
            return cellObj;
        }

        private void DataGridTextColumn_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                var viewModel = DataContext as ViewResultViewModel;
                if (viewModel?.SelectedItem != null)
                {
                    var testType = viewModel.SelectedItem.ResultType;
                    if (testType == 1)
                    {
                        // Allow letters, numbers, and Vietnamese characters
                        e.Handled = !Regex.IsMatch(e.Text, @"^[a-zA-Z0-9\u00C0-\u017F]+$");
                    }
                    else if (testType == 2)
                    {
                        // Allow only numbers and dot
                        e.Handled = !Regex.IsMatch(e.Text, @"^[0-9.]+$");
                    }
                }
            }
        }

        private void DataGridTextColumn_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                var viewModel = DataContext as ResultViewModel;
                if (viewModel?.SelectedItem != null)
                {
                    var testType = viewModel.SelectedItem.ResultType;
                    var clipboardText = e.DataObject.GetData(typeof(string)) as string;
                    if (string.IsNullOrEmpty(clipboardText))
                    {
                        e.CancelCommand();
                        return;
                    }

                    if (testType == 1)
                    {
                        // Allow letters, numbers, and Vietnamese characters
                        if (!Regex.IsMatch(clipboardText, @"^[a-zA-Z0-9\u00C0-\u017F]+$"))
                        {
                            e.CancelCommand();
                        }
                    }
                    else if (testType == 2)
                    {
                        // Allow only numbers and dot
                        if (!Regex.IsMatch(clipboardText, @"^[0-9.]+$"))
                        {
                            e.CancelCommand();
                        }
                    }
                }
            }
        }

    }
}
