using System.Windows;

namespace QC_Management.Helpers
{
    /// <summary>
    /// Freezable proxy that enables binding to DataContext from places that are not in the visual tree
    /// (e.g. DataGridColumn). Use as a StaticResource and bind its Data to the UserControl's DataContext.
    /// </summary>
    public class BindingProxy : Freezable
    {
        public static readonly DependencyProperty DataProperty =
            DependencyProperty.Register(nameof(Data), typeof(object), typeof(BindingProxy), new PropertyMetadata(null));

        public object Data
        {
            get => GetValue(DataProperty);
            set => SetValue(DataProperty, value);
        }

        protected override Freezable CreateInstanceCore()
        {
            return new BindingProxy();
        }
    }
}