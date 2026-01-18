using QC_Management.Models;
using QC_Management.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace QC_Management.Views
{
    /// <summary>
    /// Interaction logic for Re_ResultDetailView.xaml
    /// </summary>
    public partial class Re_ResultDetailView : Window
    {
        public Re_ResultDetailView()
        {
            InitializeComponent();
        }

        private void DataGridTextColumn_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // reuse same validation as other view (optional)
            if (sender is TextBox textBox)
            {
                var viewModel = DataContext as Re_ResultDetailViewModel;
                if (viewModel?.SelectedItem != null)
                {
                    var testType = viewModel.SelectedItem.ResultType;
                    if (testType == 1)
                    {
                        e.Handled = !Regex.IsMatch(e.Text, @"^[a-zA-Z0-9\u00C0-\u017F]+$");
                    }
                    else if (testType == 2)
                    {
                        e.Handled = !Regex.IsMatch(e.Text, @"^[0-9.]+$");
                    }
                }
            }
        }

        private void DataGridTextColumn_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (sender is TextBox)
            {
                var viewModel = DataContext as Re_ResultDetailViewModel;
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
                        if (!Regex.IsMatch(clipboardText, @"^[a-zA-Z0-9\u00C0-\u017F]+$"))
                        {
                            e.CancelCommand();
                        }
                    }
                    else if (testType == 2)
                    {
                        if (!Regex.IsMatch(clipboardText, @"^[0-9.]+$"))
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

            try
            {
                // remember current column so we can move to same column on next row
                DataGridColumn currentColumn = dg.CurrentCell.Column;

                // Commit edits so binding updates the ResultReView.TempResult
                dg.CommitEdit(DataGridEditingUnit.Cell, true);
                dg.CommitEdit(DataGridEditingUnit.Row, true);

                // call VM check for the edited item
                if (dg.SelectedItem is ResultReView item && DataContext is Re_ResultDetailViewModel vm)
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

                // swallow key so WPF doesn't produce system ding; navigation already handled
                e.Handled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error while checking Westgard or moving to next row: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
