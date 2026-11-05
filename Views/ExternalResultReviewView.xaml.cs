using System;
using System.Windows;
using System.Windows.Controls;
using QC_Management.ViewModels;

namespace QC_Management.Views
{
    public partial class ExternalResultReviewView : UserControl
    {
        public ExternalResultReviewView()
        {
            InitializeComponent();
            // Do NOT set DataContext here when the view is shown via a DataTemplate.
            // The MainViewModel provides the ExternalResultReviewViewModel instance and WPF will assign it.
        }
    }
}