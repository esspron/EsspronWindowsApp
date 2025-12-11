using System.Windows;
using EsspronAlcoholTester.ViewModels;

namespace EsspronAlcoholTester.Views
{
    public partial class MainWindow : Window
    {
        private LoginViewModel? _loginViewModel;
        private MainViewModel? _mainViewModel;

        public MainWindow()
        {
            InitializeComponent();
            InitializeLoginView();
        }

        private void InitializeLoginView()
        {
            _loginViewModel = new LoginViewModel();
            _loginViewModel.LoginSuccessful += OnLoginSuccessful;
            LoginViewControl.DataContext = _loginViewModel;
        }

        private async void OnLoginSuccessful(object? sender, EventArgs e)
        {
            LoginViewControl.Visibility = Visibility.Collapsed;
            DashboardViewControl.Visibility = Visibility.Visible;

            _mainViewModel = new MainViewModel();
            _mainViewModel.LogoutRequested += OnLogoutRequested;
            DashboardViewControl.DataContext = _mainViewModel;

            await _mainViewModel.InitializeAsync();
        }

        private void OnLogoutRequested(object? sender, EventArgs e)
        {
            DashboardViewControl.Visibility = Visibility.Collapsed;
            LoginViewControl.Visibility = Visibility.Visible;
            
            _mainViewModel = null;
            InitializeLoginView();
        }
    }
}
