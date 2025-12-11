using EsspronAlcoholTester.Models;

namespace EsspronAlcoholTester.Services
{
    public interface IDataService
    {
        Task<bool> ExportToCsvAsync(List<AlcoholTestRecord> records, string filePath);
        Task<bool> ExportToExcelAsync(List<AlcoholTestRecord> records, string filePath);
        Task<bool> ExportToPdfAsync(List<AlcoholTestRecord> records, string filePath);
        Task<(int Total, int Passed, int Warning, int Failed)> GetStatisticsAsync(List<AlcoholTestRecord> records);
        List<AlcoholTestRecord> FilterByDateRange(List<AlcoholTestRecord> records, DateTime startDate, DateTime endDate);
        List<AlcoholTestRecord> FilterByResult(List<AlcoholTestRecord> records, string result);
    }
}
