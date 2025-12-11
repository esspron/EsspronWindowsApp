using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EsspronAlcoholTester.Services;
using EsspronAlcoholTester.Models;
using System.Collections.ObjectModel;

namespace EsspronAlcoholTester.ViewModels
{
    public partial class LicenseViewModel : ObservableObject
    {
        private readonly ILicenseService _licenseService;
        private readonly ISupabaseService _supabaseService;

        [ObservableProperty]
        private License? _license;

        [ObservableProperty]
        private string _newLicenseKey = string.Empty;

        [ObservableProperty]
        private string _errorMessage = string.Empty;

        [ObservableProperty]
        private string _successMessage = string.Empty;

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private ObservableCollection<DeviceInfo> _activatedDevices = new();

        public LicenseViewModel()
        {
            _licenseService = App.ServiceProvider.GetService(typeof(ILicenseService)) as ILicenseService
                ?? throw new InvalidOperationException("LicenseService not found");
            _supabaseService = App.ServiceProvider.GetService(typeof(ISupabaseService)) as ISupabaseService
                ?? throw new InvalidOperationException("SupabaseService not found");
        }

        public async Task LoadLicenseAsync()
        {
            IsLoading = true;
            try
            {
                var user = await _supabaseService.GetCurrentUserAsync();
                if (user != null)
                {
                    License = await _supabaseService.GetUserLicenseAsync(user.Id);
                    var devices = await _supabaseService.GetUserDevicesAsync(user.Id);
                    ActivatedDevices = new ObservableCollection<DeviceInfo>(devices.Where(d => d.IsActivated));
                }
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task ActivateLicenseAsync()
        {
            if (string.IsNullOrWhiteSpace(NewLicenseKey))
            {
                ErrorMessage = "Please enter a license key";
                SuccessMessage = string.Empty;
                return;
            }

            IsLoading = true;
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;

            try
            {
                var isValid = await _licenseService.ValidateLicenseKeyAsync(NewLicenseKey);
                
                if (!isValid)
                {
                    ErrorMessage = "Invalid license key format. Expected: ESSPRON-XXXX-XXXX-XXXX";
                    return;
                }

                var licenseDetails = await _licenseService.GetLicenseDetailsAsync(NewLicenseKey);
                
                if (licenseDetails == null)
                {
                    ErrorMessage = "License key not found or expired.";
                    return;
                }

                License = licenseDetails;
                SuccessMessage = $"License activated! You can now activate up to {licenseDetails.AllowedDevices} devices.";
                NewLicenseKey = string.Empty;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Activation failed: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task DeactivateDeviceAsync(DeviceInfo device)
        {
            if (License == null) return;

            IsLoading = true;
            try
            {
                var success = await _licenseService.DeactivateDeviceAsync(License.LicenseKey, device.DeviceId);
                
                if (success)
                {
                    ActivatedDevices.Remove(device);
                    License.ActivatedDevices--;
                    SuccessMessage = "Device deactivated successfully";
                }
            }
            finally
            {
                IsLoading = false;
            }
        }

        public string LicenseStatus
        {
            get
            {
                if (License == null) return "No License";
                if (!License.IsActive) return "Inactive";
                if (License.ExpiryDate < DateTime.Now) return "Expired";
                return "Active";
            }
        }

        public string ExpiryInfo
        {
            get
            {
                if (License == null) return "N/A";
                var days = License.DaysRemaining;
                if (days < 0) return "Expired";
                if (days == 0) return "Expires today";
                if (days == 1) return "Expires tomorrow";
                return $"Expires in {days} days";
            }
        }
    }
}
