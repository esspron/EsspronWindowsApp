using System.Management;
using EsspronAlcoholTester.Models;

namespace EsspronAlcoholTester.Services
{
    public class UsbDeviceService : IUsbDeviceService, IDisposable
    {
        public event EventHandler<DeviceConnectionState>? ConnectionStateChanged;
        public event EventHandler<DeviceInfo>? DeviceDetected;
        public event EventHandler<List<AlcoholTestRecord>>? DataImported;
        public event EventHandler<int>? ImportProgressChanged;

        public DeviceConnectionState CurrentState { get; private set; } = DeviceConnectionState.Disconnected;
        public DeviceInfo? ConnectedDevice { get; private set; }

        private ManagementEventWatcher? _insertWatcher;
        private ManagementEventWatcher? _removeWatcher;
        private bool _isDetecting;
        private CancellationTokenSource? _detectionCts;

        // TODO: Update these values after reverse engineering the device
        private const int TARGET_VENDOR_ID = 0x0000;  // Replace with actual VID
        private const int TARGET_PRODUCT_ID = 0x0000; // Replace with actual PID
        private const string DEVICE_NAME_PATTERN = "Alcohol"; // Pattern to match device name

        public async Task StartDetectionAsync()
        {
            if (_isDetecting) return;
            
            _isDetecting = true;
            _detectionCts = new CancellationTokenSource();
            
            UpdateState(DeviceConnectionState.Detecting);

            try
            {
                // Set up USB device insertion watcher
                var insertQuery = new WqlEventQuery("SELECT * FROM __InstanceCreationEvent WITHIN 2 WHERE TargetInstance ISA 'Win32_USBHub'");
                _insertWatcher = new ManagementEventWatcher(insertQuery);
                _insertWatcher.EventArrived += OnDeviceInserted;
                _insertWatcher.Start();

                // Set up USB device removal watcher
                var removeQuery = new WqlEventQuery("SELECT * FROM __InstanceDeletionEvent WITHIN 2 WHERE TargetInstance ISA 'Win32_USBHub'");
                _removeWatcher = new ManagementEventWatcher(removeQuery);
                _removeWatcher.EventArrived += OnDeviceRemoved;
                _removeWatcher.Start();

                // Initial scan for already connected devices
                await ScanForDevicesAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Detection error: {ex.Message}");
                UpdateState(DeviceConnectionState.Error);
            }
        }

        public async Task StopDetectionAsync()
        {
            _isDetecting = false;
            _detectionCts?.Cancel();

            _insertWatcher?.Stop();
            _insertWatcher?.Dispose();
            _insertWatcher = null;

            _removeWatcher?.Stop();
            _removeWatcher?.Dispose();
            _removeWatcher = null;

            UpdateState(DeviceConnectionState.Disconnected);
            await Task.CompletedTask;
        }

        private async Task ScanForDevicesAsync()
        {
            try
            {
                var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_USBHub");
                
                foreach (ManagementObject device in searcher.Get())
                {
                    var deviceId = device["DeviceID"]?.ToString() ?? "";
                    var description = device["Description"]?.ToString() ?? "";
                    var pnpDeviceId = device["PNPDeviceID"]?.ToString() ?? "";

                    System.Diagnostics.Debug.WriteLine($"Found USB: {description} - {deviceId}");

                    // TODO: Update detection logic after reverse engineering
                    // For MVP, simulate device detection
                    if (ShouldDetectDevice(deviceId, description))
                    {
                        var deviceInfo = CreateDeviceInfo(deviceId, description, pnpDeviceId);
                        DeviceDetected?.Invoke(this, deviceInfo);
                        UpdateState(DeviceConnectionState.Found);
                        return;
                    }
                }

                // For MVP demo: Simulate finding a device after 3 seconds
                await SimulateDeviceDetectionAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Scan error: {ex.Message}");
            }
        }

        private async Task SimulateDeviceDetectionAsync()
        {
            // This is for demo purposes - remove after implementing actual USB detection
            await Task.Delay(3000);
            
            if (_isDetecting && CurrentState == DeviceConnectionState.Detecting)
            {
                var simulatedDevice = new DeviceInfo
                {
                    DeviceId = "ESSPRON-AT-001",
                    DeviceName = "Esspron Alcohol Tester Pro",
                    SerialNumber = "SN-2024-001234",
                    FirmwareVersion = "1.0.5",
                    TotalRecords = 247,
                    IsConnected = false,
                    IsActivated = false
                };

                DeviceDetected?.Invoke(this, simulatedDevice);
                UpdateState(DeviceConnectionState.Found);
            }
        }

        private bool ShouldDetectDevice(string deviceId, string description)
        {
            // TODO: Implement actual device matching after reverse engineering
            // Check for VID/PID or device name pattern
            return description.Contains(DEVICE_NAME_PATTERN, StringComparison.OrdinalIgnoreCase);
        }

        private DeviceInfo CreateDeviceInfo(string deviceId, string description, string pnpDeviceId)
        {
            return new DeviceInfo
            {
                DeviceId = deviceId,
                DeviceName = description,
                SerialNumber = ExtractSerialNumber(pnpDeviceId),
                FirmwareVersion = "Unknown",
                IsConnected = true,
                IsActivated = false
            };
        }

        private string ExtractSerialNumber(string pnpDeviceId)
        {
            // Extract serial number from PNP device ID
            var parts = pnpDeviceId.Split('\\');
            return parts.Length > 0 ? parts[^1] : "Unknown";
        }

        private void OnDeviceInserted(object sender, EventArrivedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("USB device inserted");
            Task.Run(async () => await ScanForDevicesAsync());
        }

        private void OnDeviceRemoved(object sender, EventArrivedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("USB device removed");
            if (ConnectedDevice != null)
            {
                ConnectedDevice = null;
                UpdateState(DeviceConnectionState.Disconnected);
            }
        }

        public async Task<bool> ConnectToDeviceAsync(DeviceInfo device)
        {
            try
            {
                UpdateState(DeviceConnectionState.Connecting);
                
                // TODO: Implement actual USB connection after reverse engineering
                await Task.Delay(1500); // Simulate connection time
                
                ConnectedDevice = device;
                ConnectedDevice.IsConnected = true;
                
                UpdateState(DeviceConnectionState.Connected);
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Connect error: {ex.Message}");
                UpdateState(DeviceConnectionState.Error);
                return false;
            }
        }

        public async Task DisconnectAsync()
        {
            if (ConnectedDevice != null)
            {
                ConnectedDevice.IsConnected = false;
                ConnectedDevice = null;
            }
            
            UpdateState(DeviceConnectionState.Disconnected);
            await Task.CompletedTask;
        }

        public async Task<List<AlcoholTestRecord>> ImportDataAsync()
        {
            var records = new List<AlcoholTestRecord>();
            
            if (ConnectedDevice == null)
            {
                return records;
            }

            try
            {
                UpdateState(DeviceConnectionState.ReadingData);
                
                // TODO: Implement actual data reading from USB device
                // For MVP, generate sample data
                var random = new Random();
                var recordCount = ConnectedDevice.TotalRecords > 0 ? ConnectedDevice.TotalRecords : random.Next(50, 300);

                UpdateState(DeviceConnectionState.Importing);

                for (int i = 0; i < recordCount; i++)
                {
                    // Simulate reading progress
                    int progress = (int)((i + 1) / (double)recordCount * 100);
                    ImportProgressChanged?.Invoke(this, progress);

                    if (i % 10 == 0)
                    {
                        await Task.Delay(50); // Simulate read time
                    }

                    var testTime = DateTime.Now.AddHours(-random.Next(1, 72)).AddMinutes(-random.Next(0, 60));
                    var alcoholLevel = GenerateRealisticAlcoholLevel(random);

                    records.Add(new AlcoholTestRecord
                    {
                        Id = i + 1,
                        DeviceId = ConnectedDevice.DeviceId,
                        DeviceName = ConnectedDevice.DeviceName,
                        TestTime = testTime,
                        AlcoholLevel = alcoholLevel,
                        AlcoholUnit = "mg/100ml",
                        SubjectId = $"SUBJ-{random.Next(1000, 9999)}",
                        ImportedAt = DateTime.Now
                    });
                }

                UpdateState(DeviceConnectionState.ImportComplete);
                DataImported?.Invoke(this, records);
                
                return records;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Import error: {ex.Message}");
                UpdateState(DeviceConnectionState.Error);
                return records;
            }
        }

        private double GenerateRealisticAlcoholLevel(Random random)
        {
            // Generate realistic BAC values
            // Most tests should be 0 or very low (passed)
            // Some should be warning level
            // Few should be high (failed)
            
            var roll = random.Next(100);
            
            if (roll < 70) // 70% pass (0-19 mg/100ml)
            {
                return Math.Round(random.NextDouble() * 19, 2);
            }
            else if (roll < 90) // 20% warning (20-49 mg/100ml)
            {
                return Math.Round(20 + random.NextDouble() * 29, 2);
            }
            else // 10% fail (50+ mg/100ml)
            {
                return Math.Round(50 + random.NextDouble() * 100, 2);
            }
        }

        public async Task<bool> ClearDeviceDataAsync()
        {
            try
            {
                // TODO: Implement actual device data clearing
                await Task.Delay(1000);
                
                if (ConnectedDevice != null)
                {
                    ConnectedDevice.TotalRecords = 0;
                }
                
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Clear data error: {ex.Message}");
                return false;
            }
        }

        private void UpdateState(DeviceConnectionState newState)
        {
            CurrentState = newState;
            ConnectionStateChanged?.Invoke(this, newState);
        }

        public void Dispose()
        {
            _insertWatcher?.Dispose();
            _removeWatcher?.Dispose();
            _detectionCts?.Dispose();
        }
    }
}
