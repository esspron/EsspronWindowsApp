using EsspronAlcoholTester.Models;

namespace EsspronAlcoholTester.Services
{
    public interface ISupabaseService
    {
        Task<bool> SignInAsync(string email, string password);
        Task<bool> SignUpAsync(string email, string password, string fullName, string companyName);
        Task SignOutAsync();
        Task<User?> GetCurrentUserAsync();
        Task<bool> IsAuthenticatedAsync();
        Task<List<AlcoholTestRecord>> GetTestRecordsAsync(string userId);
        Task<bool> SaveTestRecordsAsync(List<AlcoholTestRecord> records);
        Task<License?> GetUserLicenseAsync(string userId);
        Task<List<DeviceInfo>> GetUserDevicesAsync(string userId);
        Task<bool> SaveDeviceAsync(DeviceInfo device);
        Task<bool> ActivateDeviceAsync(string deviceId, string licenseKey);
    }
}
