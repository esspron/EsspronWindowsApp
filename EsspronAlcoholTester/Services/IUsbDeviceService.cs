using EsspronAlcoholTester.Models;

namespace EsspronAlcoholTester.Services
{
    public interface IUsbDeviceService
    {
        event EventHandler<DeviceConnectionState>? ConnectionStateChanged;
        event EventHandler<DeviceInfo>? DeviceDetected;
        event EventHandler<List<AlcoholTestRecord>>? DataImported;
        event EventHandler<int>? ImportProgressChanged;
        
        DeviceConnectionState CurrentState { get; }
        DeviceInfo? ConnectedDevice { get; }
        
        Task StartDetectionAsync();
        Task StopDetectionAsync();
        Task<bool> ConnectToDeviceAsync(DeviceInfo device);
        Task DisconnectAsync();
        Task<List<AlcoholTestRecord>> ImportDataAsync();
        Task<bool> ClearDeviceDataAsync();
    }
}
