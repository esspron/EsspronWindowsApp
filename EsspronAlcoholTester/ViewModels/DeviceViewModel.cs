using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EsspronAlcoholTester.Services;
using EsspronAlcoholTester.Models;

namespace EsspronAlcoholTester.ViewModels
{
    public partial class DeviceViewModel : ObservableObject
    {
        private readonly IUsbDeviceService _usbDeviceService;
        private readonly ILicenseService _licenseService;

        [ObservableProperty]
        private DeviceInfo? _device;

        [ObservableProperty]
        private DeviceConnectionState _connectionState;

        [ObservableProperty]
        private int _importProgress;

        [ObservableProperty]
        private bool _isImporting;

        [ObservableProperty]
        private string _statusMessage = string.Empty;

        [ObservableProperty]
        private string _activationKey = string.Empty;

        [ObservableProperty]
        private bool _showActivationPanel;

        [ObservableProperty]
        private string _activationError = string.Empty;

        public DeviceViewModel()
        {
            _usbDeviceService = App.ServiceProvider.GetService(typeof(IUsbDeviceService)) as IUsbDeviceService
                ?? throw new InvalidOperationException("UsbDeviceService not found");
            _licenseService = App.ServiceProvider.GetService(typeof(ILicenseService)) as ILicenseService
                ?? throw new InvalidOperationException("LicenseService not found");

            _usbDeviceService.ConnectionStateChanged += (s, state) =>
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    ConnectionState = state;
                    UpdateStatusMessage();
                });
            };

            _usbDeviceService.DeviceDetected += (s, device) =>
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    Device = device;
                    ShowActivationPanel = !device.IsActivated;
                });
            };

            _usbDeviceService.ImportProgressChanged += (s, progress) =>
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    ImportProgress = progress;
                });
            };
        }

        private void UpdateStatusMessage()
        {
            StatusMessage = ConnectionState switch
            {
                DeviceConnectionState.Disconnected => "Please connect your Esspron Alcohol Tester device",
                DeviceConnectionState.Detecting => "Searching for device...",
                DeviceConnectionState.Found => Device?.IsActivated == true 
                    ? "Device found! Click 'Connect' to proceed" 
                    : "Device found! Please activate it first",
                DeviceConnectionState.Connecting => "Connecting to device...",
                DeviceConnectionState.Connected => "Connected! Ready to import data",
                DeviceConnectionState.ReadingData => "Reading data from device...",
                DeviceConnectionState.Importing => $"Importing data... {ImportProgress}%",
                DeviceConnectionState.ImportComplete => "Import complete!",
                DeviceConnectionState.Error => "An error occurred",
                _ => ""
            };
        }

        [RelayCommand]
        private async Task ActivateDeviceAsync()
        {
            if (Device == null || string.IsNullOrWhiteSpace(ActivationKey))
            {
                ActivationError = "Please enter a valid activation key";
                return;
            }

            ActivationError = string.Empty;

            try
            {
                var isValid = await _licenseService.ValidateLicenseKeyAsync(ActivationKey);
                
                if (!isValid)
                {
                    ActivationError = "Invalid license key. Please check and try again.";
                    return;
                }

                var success = await _licenseService.ActivateDeviceAsync(ActivationKey, Device.DeviceId);
                
                if (success)
                {
                    Device.IsActivated = true;
                    Device.ActivationKey = ActivationKey;
                    Device.ActivatedAt = DateTime.Now;
                    Device.LicenseExpiryDate = DateTime.Now.AddYears(1);
                    
                    ShowActivationPanel = false;
                    StatusMessage = "Device activated successfully!";
                }
                else
                {
                    ActivationError = "Activation failed. You may have reached your device limit.";
                }
            }
            catch (Exception ex)
            {
                ActivationError = $"Activation error: {ex.Message}";
            }
        }

        [RelayCommand]
        private async Task ConnectAsync()
        {
            if (Device == null || !Device.IsActivated)
            {
                StatusMessage = "Please activate the device first";
                return;
            }

            await _usbDeviceService.ConnectToDeviceAsync(Device);
        }

        [RelayCommand]
        private async Task ImportAsync()
        {
            if (ConnectionState != DeviceConnectionState.Connected)
            {
                StatusMessage = "Please connect to the device first";
                return;
            }

            IsImporting = true;
            ImportProgress = 0;

            try
            {
                await _usbDeviceService.ImportDataAsync();
            }
            finally
            {
                IsImporting = false;
            }
        }

        [RelayCommand]
        private async Task DisconnectAsync()
        {
            await _usbDeviceService.DisconnectAsync();
            Device = null;
        }

        [RelayCommand]
        private async Task RefreshAsync()
        {
            await _usbDeviceService.StopDetectionAsync();
            await _usbDeviceService.StartDetectionAsync();
        }
    }
}
