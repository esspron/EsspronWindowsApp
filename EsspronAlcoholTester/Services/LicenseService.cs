using EsspronAlcoholTester.Models;

namespace EsspronAlcoholTester.Services
{
    public class LicenseService : ILicenseService
    {
        private readonly ISupabaseService _supabaseService;

        public LicenseService(ISupabaseService supabaseService)
        {
            _supabaseService = supabaseService;
        }

        public async Task<bool> ValidateLicenseKeyAsync(string licenseKey)
        {
            try
            {
                // TODO: Implement actual license validation against Supabase
                await Task.Delay(500);
                
                // For MVP, accept any key that matches pattern ESSPRON-XXXX-XXXX-XXXX
                if (string.IsNullOrWhiteSpace(licenseKey))
                    return false;

                var parts = licenseKey.Split('-');
                if (parts.Length != 4)
                    return false;

                if (parts[0] != "ESSPRON")
                    return false;

                return parts.Skip(1).All(p => p.Length == 4 && p.All(char.IsLetterOrDigit));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"License validation error: {ex.Message}");
                return false;
            }
        }

        public async Task<License?> GetLicenseDetailsAsync(string licenseKey)
        {
            try
            {
                await Task.Delay(500);
                
                if (!await ValidateLicenseKeyAsync(licenseKey))
                    return null;

                // TODO: Fetch actual license details from Supabase
                return new License
                {
                    Id = Guid.NewGuid().ToString(),
                    LicenseKey = licenseKey,
                    AllowedDevices = 5,
                    ActivatedDevices = 0,
                    CreatedAt = DateTime.Now,
                    ExpiryDate = DateTime.Now.AddYears(1),
                    IsActive = true,
                    MonthlyPrice = 2500,
                    PlanName = "Professional"
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Get license error: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> ActivateDeviceAsync(string licenseKey, string deviceId)
        {
            try
            {
                var license = await GetLicenseDetailsAsync(licenseKey);
                
                if (license == null || !license.IsValid)
                    return false;

                if (license.ActivatedDevices >= license.AllowedDevices)
                    return false;

                // TODO: Register device activation in Supabase
                await Task.Delay(500);
                
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Activate device error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeactivateDeviceAsync(string licenseKey, string deviceId)
        {
            try
            {
                // TODO: Remove device activation from Supabase
                await Task.Delay(500);
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Deactivate device error: {ex.Message}");
                return false;
            }
        }

        public async Task<int> GetRemainingActivationsAsync(string licenseKey)
        {
            try
            {
                var license = await GetLicenseDetailsAsync(licenseKey);
                return license?.RemainingDevices ?? 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Get remaining activations error: {ex.Message}");
                return 0;
            }
        }

        public bool IsLicenseExpired(License license)
        {
            return license.ExpiryDate < DateTime.Now;
        }
    }
}
