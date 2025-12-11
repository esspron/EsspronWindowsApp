using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EsspronAlcoholTester.Services;
using EsspronAlcoholTester.Models;
using System.Collections.ObjectModel;

namespace EsspronAlcoholTester.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly ISupabaseService _supabaseService;
        private readonly IUsbDeviceService _usbDeviceService;
        private readonly ILicenseService _licenseService;
        private readonly IDataService _dataService;

        public event EventHandler? LogoutRequested;

        [ObservableProperty]
        private User? _currentUser;

        [ObservableProperty]
        private License? _currentLicense;

        [ObservableProperty]
        private string _currentView = "Dashboard";

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private DeviceConnectionState _deviceState = DeviceConnectionState.Disconnected;

        [ObservableProperty]
        private DeviceInfo? _connectedDevice;

        [ObservableProperty]
        private int _importProgress;

        [ObservableProperty]
        private ObservableCollection<AlcoholTestRecord> _testRecords = new();

        [ObservableProperty]
        private ObservableCollection<AlcoholTestRecord> _recentRecords = new();

        [ObservableProperty]
        private int _totalTests;

        [ObservableProperty]
        private int _passedTests;

        [ObservableProperty]
        private int _warningTests;

        [ObservableProperty]
        private int _failedTests;

        [ObservableProperty]
        private string _statusMessage = "Ready";

        public MainViewModel()
        {
            _supabaseService = App.ServiceProvider.GetService(typeof(ISupabaseService)) as ISupabaseService
                ?? throw new InvalidOperationException("SupabaseService not found");
            _usbDeviceService = App.ServiceProvider.GetService(typeof(IUsbDeviceService)) as IUsbDeviceService
                ?? throw new InvalidOperationException("UsbDeviceService not found");
            _licenseService = App.ServiceProvider.GetService(typeof(ILicenseService)) as ILicenseService
                ?? throw new InvalidOperationException("LicenseService not found");
            _dataService = App.ServiceProvider.GetService(typeof(IDataService)) as IDataService
                ?? throw new InvalidOperationException("DataService not found");

            // Subscribe to USB device events
            _usbDeviceService.ConnectionStateChanged += OnConnectionStateChanged;
            _usbDeviceService.DeviceDetected += OnDeviceDetected;
            _usbDeviceService.DataImported += OnDataImported;
            _usbDeviceService.ImportProgressChanged += OnImportProgressChanged;
        }

        public async Task InitializeAsync()
        {
            IsLoading = true;
            
            try
            {
                CurrentUser = await _supabaseService.GetCurrentUserAsync();
                
                if (CurrentUser != null)
                {
                    CurrentLicense = await _supabaseService.GetUserLicenseAsync(CurrentUser.Id);
                    var records = await _supabaseService.GetTestRecordsAsync(CurrentUser.Id);
                    
                    TestRecords = new ObservableCollection<AlcoholTestRecord>(records);
                    RecentRecords = new ObservableCollection<AlcoholTestRecord>(records.Take(10));
                    
                    await UpdateStatisticsAsync();
                }

                // Start USB device detection
                await _usbDeviceService.StartDetectionAsync();
            }
            catch (Exception ex)
            {
                StatusMessage = $"Initialization error: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void OnConnectionStateChanged(object? sender, DeviceConnectionState state)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                DeviceState = state;
                StatusMessage = GetStatusMessage(state);
            });
        }

        private void OnDeviceDetected(object? sender, DeviceInfo device)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                ConnectedDevice = device;
                StatusMessage = $"Device found: {device.DeviceName}";
            });
        }

        private void OnDataImported(object? sender, List<AlcoholTestRecord> records)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(async () =>
            {
                foreach (var record in records)
                {
                    record.UserId = CurrentUser?.Id ?? string.Empty;
                    TestRecords.Add(record);
                }
                
                RecentRecords = new ObservableCollection<AlcoholTestRecord>(TestRecords.TakeLast(10).Reverse());
                await UpdateStatisticsAsync();
                
                // Save to cloud
                if (CurrentUser != null)
                {
                    await _supabaseService.SaveTestRecordsAsync(records);
                }
                
                StatusMessage = $"Successfully imported {records.Count} records";
            });
        }

        private void OnImportProgressChanged(object? sender, int progress)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                ImportProgress = progress;
            });
        }

        private string GetStatusMessage(DeviceConnectionState state)
        {
            return state switch
            {
                DeviceConnectionState.Disconnected => "No device connected",
                DeviceConnectionState.Detecting => "Searching for devices...",
                DeviceConnectionState.Found => "Device found! Click Connect to proceed.",
                DeviceConnectionState.Connecting => "Connecting to device...",
                DeviceConnectionState.Connected => "Device connected successfully",
                DeviceConnectionState.ReadingData => "Reading data from device...",
                DeviceConnectionState.Importing => $"Importing data... {ImportProgress}%",
                DeviceConnectionState.ImportComplete => "Data import complete!",
                DeviceConnectionState.Error => "An error occurred. Please try again.",
                _ => "Ready"
            };
        }

        private async Task UpdateStatisticsAsync()
        {
            var stats = await _dataService.GetStatisticsAsync(TestRecords.ToList());
            TotalTests = stats.Total;
            PassedTests = stats.Passed;
            WarningTests = stats.Warning;
            FailedTests = stats.Failed;
        }

        [RelayCommand]
        private void NavigateTo(string view)
        {
            CurrentView = view;
        }

        [RelayCommand]
        private async Task ConnectDeviceAsync()
        {
            if (ConnectedDevice == null) return;

            if (!ConnectedDevice.IsActivated)
            {
                // Need to activate device first
                CurrentView = "License";
                return;
            }

            await _usbDeviceService.ConnectToDeviceAsync(ConnectedDevice);
        }

        [RelayCommand]
        private async Task ImportDataAsync()
        {
            if (DeviceState != DeviceConnectionState.Connected) return;
            
            ImportProgress = 0;
            await _usbDeviceService.ImportDataAsync();
        }

        [RelayCommand]
        private async Task RefreshDeviceAsync()
        {
            await _usbDeviceService.StopDetectionAsync();
            await _usbDeviceService.StartDetectionAsync();
        }

        [RelayCommand]
        private async Task DisconnectDeviceAsync()
        {
            await _usbDeviceService.DisconnectAsync();
            ConnectedDevice = null;
        }

        [RelayCommand]
        private async Task ExportDataAsync()
        {
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                DefaultExt = ".csv",
                Filter = "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*",
                FileName = $"AlcoholTests_{DateTime.Now:yyyyMMdd_HHmmss}"
            };

            if (dialog.ShowDialog() == true)
            {
                var success = await _dataService.ExportToCsvAsync(TestRecords.ToList(), dialog.FileName);
                StatusMessage = success ? "Data exported successfully" : "Export failed";
            }
        }

        [RelayCommand]
        private async Task LogoutAsync()
        {
            await _usbDeviceService.StopDetectionAsync();
            await _supabaseService.SignOutAsync();
            LogoutRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}
