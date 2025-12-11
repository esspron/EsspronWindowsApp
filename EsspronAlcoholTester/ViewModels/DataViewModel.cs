using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EsspronAlcoholTester.Services;
using EsspronAlcoholTester.Models;
using System.Collections.ObjectModel;

namespace EsspronAlcoholTester.ViewModels
{
    public partial class DataViewModel : ObservableObject
    {
        private readonly IDataService _dataService;
        private readonly ISupabaseService _supabaseService;
        private List<AlcoholTestRecord> _allRecords = new();

        [ObservableProperty]
        private ObservableCollection<AlcoholTestRecord> _records = new();

        [ObservableProperty]
        private AlcoholTestRecord? _selectedRecord;

        [ObservableProperty]
        private DateTime _startDate = DateTime.Now.AddDays(-30);

        [ObservableProperty]
        private DateTime _endDate = DateTime.Now;

        [ObservableProperty]
        private string _selectedFilter = "All";

        [ObservableProperty]
        private string _searchText = string.Empty;

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private int _totalCount;

        [ObservableProperty]
        private int _passCount;

        [ObservableProperty]
        private int _warningCount;

        [ObservableProperty]
        private int _failCount;

        public List<string> FilterOptions { get; } = new() { "All", "Pass", "Warning", "Fail" };

        public DataViewModel()
        {
            _dataService = App.ServiceProvider.GetService(typeof(IDataService)) as IDataService
                ?? throw new InvalidOperationException("DataService not found");
            _supabaseService = App.ServiceProvider.GetService(typeof(ISupabaseService)) as ISupabaseService
                ?? throw new InvalidOperationException("SupabaseService not found");
        }

        public void LoadRecords(IEnumerable<AlcoholTestRecord> records)
        {
            _allRecords = records.ToList();
            ApplyFilters();
        }

        partial void OnStartDateChanged(DateTime value) => ApplyFilters();
        partial void OnEndDateChanged(DateTime value) => ApplyFilters();
        partial void OnSelectedFilterChanged(string value) => ApplyFilters();
        partial void OnSearchTextChanged(string value) => ApplyFilters();

        private void ApplyFilters()
        {
            var filtered = _allRecords.AsEnumerable();

            // Date range filter
            filtered = filtered.Where(r => r.TestTime >= StartDate && r.TestTime <= EndDate.AddDays(1));

            // Result filter
            if (SelectedFilter != "All")
            {
                filtered = filtered.Where(r => r.ResultStatus == SelectedFilter);
            }

            // Search filter
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var search = SearchText.ToLower();
                filtered = filtered.Where(r =>
                    r.SubjectId.ToLower().Contains(search) ||
                    r.DeviceName.ToLower().Contains(search) ||
                    r.Notes.ToLower().Contains(search));
            }

            var list = filtered.OrderByDescending(r => r.TestTime).ToList();
            Records = new ObservableCollection<AlcoholTestRecord>(list);

            UpdateCounts();
        }

        private void UpdateCounts()
        {
            TotalCount = Records.Count;
            PassCount = Records.Count(r => r.ResultStatus == "Pass");
            WarningCount = Records.Count(r => r.ResultStatus == "Warning");
            FailCount = Records.Count(r => r.ResultStatus == "Fail");
        }

        [RelayCommand]
        private async Task ExportToCsvAsync()
        {
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                DefaultExt = ".csv",
                Filter = "CSV Files (*.csv)|*.csv",
                FileName = $"AlcoholTests_{DateTime.Now:yyyyMMdd_HHmmss}"
            };

            if (dialog.ShowDialog() == true)
            {
                await _dataService.ExportToCsvAsync(Records.ToList(), dialog.FileName);
            }
        }

        [RelayCommand]
        private void ClearFilters()
        {
            StartDate = DateTime.Now.AddDays(-30);
            EndDate = DateTime.Now;
            SelectedFilter = "All";
            SearchText = string.Empty;
        }

        [RelayCommand]
        private async Task RefreshDataAsync()
        {
            IsLoading = true;
            try
            {
                var user = await _supabaseService.GetCurrentUserAsync();
                if (user != null)
                {
                    _allRecords = await _supabaseService.GetTestRecordsAsync(user.Id);
                    ApplyFilters();
                }
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
