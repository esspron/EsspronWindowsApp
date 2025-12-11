namespace EsspronAlcoholTester.Models
{
    public class AlcoholTestRecord
    {
        public int Id { get; set; }
        public string DeviceId { get; set; } = string.Empty;
        public string DeviceName { get; set; } = string.Empty;
        public DateTime TestTime { get; set; }
        public double AlcoholLevel { get; set; }
        public string AlcoholUnit { get; set; } = "mg/100ml"; // BAC unit
        public string Result { get; set; } = string.Empty; // Pass, Warning, Fail
        public string SubjectId { get; set; } = string.Empty; // Optional: ID of person tested
        public string Notes { get; set; } = string.Empty;
        public DateTime ImportedAt { get; set; }
        public string UserId { get; set; } = string.Empty;

        public string FormattedAlcoholLevel => $"{AlcoholLevel:F2} {AlcoholUnit}";
        
        public string ResultStatus
        {
            get
            {
                if (AlcoholLevel < 20) return "Pass";
                if (AlcoholLevel < 50) return "Warning";
                return "Fail";
            }
        }
    }
}
