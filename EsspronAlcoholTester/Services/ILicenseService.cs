using EsspronAlcoholTester.Models;

namespace EsspronAlcoholTester.Services
{
    public interface ILicenseService
    {
        Task<bool> ValidateLicenseKeyAsync(string licenseKey);
        Task<License?> GetLicenseDetailsAsync(string licenseKey);
        Task<bool> ActivateDeviceAsync(string licenseKey, string deviceId);
        Task<bool> DeactivateDeviceAsync(string licenseKey, string deviceId);
        Task<int> GetRemainingActivationsAsync(string licenseKey);
        bool IsLicenseExpired(License license);
    }
}
