namespace EsspronAlcoholTester.Models
{
    public class License
    {
        public string Id { get; set; } = string.Empty;
        public string LicenseKey { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public int AllowedDevices { get; set; }
        public int ActivatedDevices { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiryDate { get; set; }
        public bool IsActive { get; set; }
        public decimal MonthlyPrice { get; set; }
        public string PlanName { get; set; } = string.Empty;

        public bool IsValid => IsActive && ExpiryDate > DateTime.Now;
        public int RemainingDevices => AllowedDevices - ActivatedDevices;
        public int DaysRemaining => (ExpiryDate - DateTime.Now).Days;
    }
}
