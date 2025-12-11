using System.Windows;
using EsspronAlcoholTester.Services;
using EsspronAlcoholTester.Views;
using Microsoft.Extensions.DependencyInjection;

namespace EsspronAlcoholTester
{
    public partial class App : Application
    {
        public static IServiceProvider ServiceProvider { get; private set; } = null!;

        private async void Application_Startup(object sender, StartupEventArgs e)
        {
            // Configure services
            var services = new ServiceCollection();
            ConfigureServices(services);
            ServiceProvider = services.BuildServiceProvider();

            // Show splash screen
            var splashScreen = new Views.SplashScreen();
            splashScreen.Show();

            // Simulate loading (initialize services, check for updates, etc.)
            await Task.Delay(2500);

            // Create and show main window
            var mainWindow = new MainWindow();
            mainWindow.Show();

            // Close splash screen
            splashScreen.StopAnimation();
            splashScreen.Close();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // Services
            services.AddSingleton<ISupabaseService, SupabaseService>();
            services.AddSingleton<IUsbDeviceService, UsbDeviceService>();
            services.AddSingleton<ILicenseService, LicenseService>();
            services.AddSingleton<IDataService, DataService>();

            // ViewModels
            services.AddTransient<ViewModels.LoginViewModel>();
            services.AddTransient<ViewModels.MainViewModel>();
            services.AddTransient<ViewModels.DeviceViewModel>();
            services.AddTransient<ViewModels.DataViewModel>();
            services.AddTransient<ViewModels.LicenseViewModel>();
        }
    }
}
