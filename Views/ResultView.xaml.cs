using QC_Management.Models;
using QC_Management.ViewModels;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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
