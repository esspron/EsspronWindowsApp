namespace EsspronAlcoholTester.Models
{
    public class DeviceInfo
    {
        public string DeviceId { get; set; } = string.Empty;
        public string DeviceName { get; set; } = string.Empty;
        public string SerialNumber { get; set; } = string.Empty;
        public string FirmwareVersion { get; set; } = string.Empty;
        public int TotalRecords { get; set; }
        public DateTime? LastSyncTime { get; set; }
        public bool IsConnected { get; set; }
        public bool IsActivated { get; set; }
        public string ActivationKey { get; set; } = string.Empty;
        public DateTime? ActivatedAt { get; set; }
        public DateTime? LicenseExpiryDate { get; set; }

        public string ConnectionStatus => IsConnected ? "Connected" : "Disconnected";
        public string ActivationStatus => IsActivated ? "Activated" : "Not Activated";
        
        public bool IsLicenseValid => LicenseExpiryDate.HasValue && LicenseExpiryDate.Value > DateTime.Now;
    }
}
