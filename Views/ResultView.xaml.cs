using QC_Management.Models;
using QC_Management.ViewModels;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace QC_Management.Views
{
    /// <summary>
    /// Interaction logic for ResultView.xaml
    /// </summary>
    public partial class ResultView : UserControl
    {
        public ResultView()
        {
            InitializeComponent();
        }

        private void DataGridTextColumn_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                var viewModel = DataContext as ResultViewModel;
                if (viewModel?.SelectedItem != null)
                {
                    var testType = viewModel.SelectedItem.ResultType;
                    if (testType == 1)
                    {
                        // Allow letters, numbers, Vietnamese characters, plus and minus
                        e.Handled = !Regex.IsMatch(e.Text, @"^[a-zA-Z0-9\u00C0-\u017F+\-]+$");
                    }
                    else if (testType == 2)
                    {
                        // Allow digits, decimal point, plus and minus while typing (per-character)
                        e.Handled = !Regex.IsMatch(e.Text, @"^[0-9.\+\-]+$");
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
                        // Allow letters, numbers, Vietnamese characters, plus and minus in pasted qualitative values
                        if (!Regex.IsMatch(clipboardText, @"^[a-zA-Z0-9\u00C0-\u017F+\-]+$"))
                        {
                            e.CancelCommand();
                        }
                    }
                    else if (testType == 2)
                    {
                        // For quantitative values validate full numeric format with optional leading +/-
                        // Accepts: 123, -123, +123, 123.45, -0.5, .5 (if desired; pattern below requires at least one digit)
                        if (!Regex.IsMatch(clipboardText, @"^[\+\-]?\d*\.?\d+$"))
                        {
                            e.CancelCommand();
                        }
                    }
                }
            }
        }

        // Commit edit, trigger VM check and move to next row on Enter
        private async void InputDataGrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;
            if (sender is not DataGrid dg) return;

            // swallow early to stop DataGrid default Enter-navigation which caused double-jump
            e.Handled = true;

            try
            {
                // remember current column so we can move to same column on next row
                DataGridColumn currentColumn = dg.CurrentCell.Column;

                // Commit edits so binding updates the ResultReView.TempResult
                dg.CommitEdit(DataGridEditingUnit.Cell, true);
                dg.CommitEdit(DataGridEditingUnit.Row, true);

                // call VM check for the edited item
                if (dg.SelectedItem is ResultReView item && DataContext is ResultViewModel vm)
                {
                    await vm.CheckWestgardForItemAsync(item);
                }

                // move selection to next row (if any)
                var items = dg.Items;
                int currentIndex = items.IndexOf(dg.SelectedItem);
                int nextIndex = currentIndex + 1;

                if (nextIndex >= 0 && nextIndex < items.Count)
                {
                    var nextItem = items[nextIndex];

                    // select and scroll into view
                    dg.SelectedItem = nextItem;
                    dg.ScrollIntoView(nextItem);

                    // set current cell to same column on next row (fallback to first editable column)
                    DataGridColumn targetColumn = currentColumn;
                    if (targetColumn == null)
                    {
                        foreach (var c in dg.Columns)
                        {
                            if (!c.IsReadOnly) { targetColumn = c; break; }
                        }
                    }

                    if (targetColumn != null)
                    {
                        dg.CurrentCell = new DataGridCellInfo(nextItem, targetColumn);
                        // Begin edit asynchronously to avoid timing issues
                        dg.Dispatcher.BeginInvoke((Action)(() =>
                        {
                            dg.Focus();
                            dg.BeginEdit();
                        }), DispatcherPriority.Background);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error while checking Westgard or moving to next row: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
