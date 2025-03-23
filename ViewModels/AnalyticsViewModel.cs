using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Command;
using System.Windows.Input;
using System;
using System.Windows;
using KDSUI.Data;

namespace KDSUI.ViewModels
{
    /// <summary>
    /// ViewModel for the Analytics page. Handles logic for loading performance data,
    /// responding to timeframe changes, and binding analytics results to the UI.
    /// </summary>
    public class AnalyticsViewModel : INotifyPropertyChanged
    {
        private readonly OrderDAO _orderDAO;
        private string _selectedTimeframe;
        private double _averageTicketTime;
        private ObservableCollection<BusiestDay> _busiestDays;
        private ObservableCollection<PeakHour> _peakHours;
        private ObservableCollection<StationPerformance> _stationPerformance;

        // Available timeframe options for filtering analytics
        private List<string> _timeframeOptions = new() { "Today", "Past Week", "Past Month" };

        // Commands used by the UI
        public ICommand BackCommand { get; }
        public ICommand TimeframeChangedCommand { get; }

        // Constructor sets up defaults and loads analytics for the initial timeframe
        public AnalyticsViewModel()
        {
            _orderDAO = new OrderDAO();
            _stationPerformance = new ObservableCollection<StationPerformance>();

            TimeframeChangedCommand = new RelayCommand(async () => await LoadAnalytics());
            BackCommand = new RelayCommand(Back);

            SelectedTimeframe = "Today";

            // Fire and forget - avoid blocking UI
            LoadAnalytics().ConfigureAwait(false);
        }

        // Navigates back to the dashboard page
        private void Back()
        {
            Application.Current.MainWindow.Content = App.DashboardPage;
        }

        // Currently selected timeframe (e.g. "Today", "Past Week")
        public string SelectedTimeframe
        {
            get => _selectedTimeframe;
            set
            {
                _selectedTimeframe = value;
                OnPropertyChanged();
                LoadAnalytics();
            }
        }

        // Exposes the available options for timeframe selection
        public List<string> TimeframeOptions => _timeframeOptions;

        // Average ticket time displayed on the analytics page
        public double AverageTicketTime
        {
            get => _averageTicketTime;
            set
            {
                _averageTicketTime = value;
                OnPropertyChanged();
            }
        }

        // List of the busiest days, based on order volume
        public ObservableCollection<BusiestDay> BusiestDays
        {
            get => _busiestDays;
            set
            {
                _busiestDays = value;
                OnPropertyChanged();
            }
        }

        // List of the peak order hours throughout the day
        public ObservableCollection<PeakHour> PeakHours
        {
            get => _peakHours;
            set
            {
                _peakHours = value;
                OnPropertyChanged();
            }
        }

        // List of performance metrics for each station
        public ObservableCollection<StationPerformance> StationPerformance
        {
            get => _stationPerformance;
            set
            {
                _stationPerformance = value;
                OnPropertyChanged();
            }
        }

        // Loads analytics data from the backend for the selected timeframe
        public async Task LoadAnalytics()
        {
            try
            {
                var analytics = await _orderDAO.GetAnalyticsAsync(SelectedTimeframe);

                if (analytics != null)
                {
                    AverageTicketTime = analytics.AverageTicketTime;
                    StationPerformance = new ObservableCollection<StationPerformance>(analytics.StationPerformance);
                    BusiestDays = new ObservableCollection<BusiestDay>(analytics.BusiestDays);
                    PeakHours = new ObservableCollection<PeakHour>(analytics.PeakHours);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading analytics: {ex.Message}");
            }
        }

        // Notifies the UI of property changes
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
