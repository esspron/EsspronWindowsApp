using System.IO;
using System.Text;
using EsspronAlcoholTester.Models;

namespace EsspronAlcoholTester.Services
{
    public class DataService : IDataService
    {
        public async Task<bool> ExportToCsvAsync(List<AlcoholTestRecord> records, string filePath)
        {
            try
            {
                var sb = new StringBuilder();
                
                // Header
                sb.AppendLine("ID,Device ID,Device Name,Test Time,Alcohol Level,Unit,Result,Subject ID,Notes,Imported At");
                
                // Data rows
                foreach (var record in records)
                {
                    sb.AppendLine($"{record.Id},{record.DeviceId},{record.DeviceName},{record.TestTime:yyyy-MM-dd HH:mm:ss},{record.AlcoholLevel},{record.AlcoholUnit},{record.ResultStatus},{record.SubjectId},{record.Notes},{record.ImportedAt:yyyy-MM-dd HH:mm:ss}");
                }

                await File.WriteAllTextAsync(filePath, sb.ToString());
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"CSV export error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> ExportToExcelAsync(List<AlcoholTestRecord> records, string filePath)
        {
            try
            {
                // For MVP, export as CSV with .xlsx extension
                // TODO: Implement proper Excel export using a library like ClosedXML
                return await ExportToCsvAsync(records, filePath.Replace(".xlsx", ".csv"));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Excel export error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> ExportToPdfAsync(List<AlcoholTestRecord> records, string filePath)
        {
            try
            {
                // TODO: Implement PDF export using a library like iTextSharp or QuestPDF
                await Task.Delay(100);
                return false; // Not implemented for MVP
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"PDF export error: {ex.Message}");
                return false;
            }
        }

        public async Task<(int Total, int Passed, int Warning, int Failed)> GetStatisticsAsync(List<AlcoholTestRecord> records)
        {
            return await Task.Run(() =>
            {
                var total = records.Count;
                var passed = records.Count(r => r.ResultStatus == "Pass");
                var warning = records.Count(r => r.ResultStatus == "Warning");
                var failed = records.Count(r => r.ResultStatus == "Fail");
                
                return (total, passed, warning, failed);
            });
        }

        public List<AlcoholTestRecord> FilterByDateRange(List<AlcoholTestRecord> records, DateTime startDate, DateTime endDate)
        {
            return records.Where(r => r.TestTime >= startDate && r.TestTime <= endDate).ToList();
        }

        public List<AlcoholTestRecord> FilterByResult(List<AlcoholTestRecord> records, string result)
        {
            if (string.IsNullOrEmpty(result) || result == "All")
                return records;
                
            return records.Where(r => r.ResultStatus == result).ToList();
        }
    }
}
