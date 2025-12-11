using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace EsspronAlcoholTester.Views
{
    public partial class SplashScreen : Window
    {
        private DispatcherTimer? _animationTimer;
        private int _dotIndex = 0;

        public SplashScreen()
        {
            InitializeComponent();
            StartLoadingAnimation();
        }

        private void StartLoadingAnimation()
        {
            _animationTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(300)
            };
            _animationTimer.Tick += AnimateDots;
            _animationTimer.Start();
        }

        private void AnimateDots(object? sender, EventArgs e)
        {
            // Reset all dots
            Dot1.Opacity = 0.3;
            Dot2.Opacity = 0.3;
            Dot3.Opacity = 0.3;

            // Highlight current dot
            switch (_dotIndex)
            {
                case 0:
                    Dot1.Opacity = 1;
                    break;
                case 1:
                    Dot2.Opacity = 1;
                    break;
                case 2:
                    Dot3.Opacity = 1;
                    break;
            }

            _dotIndex = (_dotIndex + 1) % 3;
        }

        public void StopAnimation()
        {
            _animationTimer?.Stop();
        }
    }
}
