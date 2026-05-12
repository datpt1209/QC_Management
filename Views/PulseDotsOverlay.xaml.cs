using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace QC_Management.Views
{
    /// <summary>
    /// Overlay 3 chấm pulse — chỉ xử lý trạng thái IsLoading.
    /// Trạng thái "Không có dữ liệu" được hiển thị bởi panel riêng
    /// ở cấp ngoài trong HomeView_V2.xaml (bind HasNoData trực tiếp).
    /// </summary>
    public partial class PulseDotsOverlay : UserControl
    {
        public static readonly DependencyProperty IsLoadingProperty =
            DependencyProperty.Register(
                nameof(IsLoading),
                typeof(bool),
                typeof(PulseDotsOverlay),
                new PropertyMetadata(false, OnIsLoadingChanged));

        public bool IsLoading
        {
            get => (bool)GetValue(IsLoadingProperty);
            set => SetValue(IsLoadingProperty, value);
        }

        private Storyboard _pulseSb;
        private Storyboard _fadeInSb;
        private Storyboard _fadeOutSb;
        private bool _visible = false;

        public PulseDotsOverlay()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            _pulseSb = (Storyboard)Resources["PulseSb"];
            _fadeInSb = (Storyboard)Resources["FadeIn"];
            _fadeOutSb = (Storyboard)Resources["FadeOut"];

            if (IsLoading) ShowLoading();
        }

        private static void OnIsLoadingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var overlay = (PulseDotsOverlay)d;
            if (overlay._pulseSb == null) return;

            if ((bool)e.NewValue)
                overlay.ShowLoading();
            else
                overlay.HideOverlay();
        }

        private void ShowLoading()
        {
            LoadingPanel.Visibility = Visibility.Visible;

            if (!_visible)
            {
                _visible = true;
                Visibility = Visibility.Visible;
                _fadeOutSb.Stop(OverlayGrid);
                _fadeInSb.Begin(OverlayGrid, isControllable: true);
            }

            _pulseSb.Begin(OverlayGrid, isControllable: true);
        }

        private void HideOverlay()
        {
            if (!_visible) return;
            _pulseSb.Stop(OverlayGrid);
            _fadeInSb.Stop(OverlayGrid);
            _fadeOutSb.Begin(OverlayGrid, isControllable: true);
        }

        public void FadeOut_Completed(object sender, System.EventArgs e)
        {
            _visible = false;
            Visibility = Visibility.Collapsed;
        }
    }
}

