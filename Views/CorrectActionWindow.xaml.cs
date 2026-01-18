using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using QC_Management.ViewModels;

namespace QC_Management.Views
{
    /// <summary>
    /// Interaction logic for CorrectActionWindow.xaml
    /// </summary>
    public partial class CorrectActionWindow : Window
    {
        public CorrectActionWindow()
        {
            InitializeComponent();
            DataContextChanged += CorrectActionWindow_DataContextChanged;

            // If DataContext already set before constructor call finishes, attach.
            if (DataContext is CorrectiveActionViewModel vm)
                AttachVm(vm);
        }

        private void CorrectActionWindow_DataContextChanged(object? sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is CorrectiveActionViewModel oldVm)
                DetachVm(oldVm);

            if (e.NewValue is CorrectiveActionViewModel newVm)
                AttachVm(newVm);
        }

        private void AttachVm(CorrectiveActionViewModel vm)
        {
            vm.RequestClose -= Vm_RequestClose;
            vm.RequestClose += Vm_RequestClose;
        }

        private void DetachVm(CorrectiveActionViewModel vm)
        {
            vm.RequestClose -= Vm_RequestClose;
        }

        private void Vm_RequestClose(bool dialogResult)
        {
            // Use Dispatcher to ensure UI thread and safe close
            Dispatcher.Invoke(() =>
            {
                try { DialogResult = dialogResult; } catch { /* ignore if not opened as dialog */ }
                Close();
            });
        }
    }
}
