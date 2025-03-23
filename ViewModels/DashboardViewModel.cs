using GalaSoft.MvvmLight.Command;
using KDSUI.Models;
using KDSUI.Pages;
using KDSUI.Services;
using KDSUI.Windows;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace KDSUI.ViewModels
{
    /// <summary>
    /// ViewModel for the main Dashboard screen.
    /// Handles station selection, order updates, and access to manager tools.
    /// </summary>
    public class DashboardViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<DynamicOrderModel> _orders;

        // All current orders displayed in the dashboard
        public ObservableCollection<DynamicOrderModel> Orders
        {
            get => _orders;
            set
            {
                _orders = value;
                OnPropertyChanged(nameof(Orders));
            }
        }

        // List of all available stations (bound to Views column)
        public ObservableCollection<string> Stations => LayoutManager.Stations;

        private string _selectedStation;

        // Currently selected station
        public string SelectedStation
        {
            get => _selectedStation;
            set
            {
                _selectedStation = value;
                OnPropertyChanged();
            }
        }

        private DynamicOrderModel _selectedOrder;

        // Currently selected order (bound to Orders panel)
        public DynamicOrderModel SelectedOrder
        {
            get => _selectedOrder;
            set
            {
                _selectedOrder = value;
                OnPropertyChanged();
            }
        }

        // Command bindings for UI buttons
        public ICommand EditLayoutCommand { get; }
        public ICommand EditOrdersCommand { get; }
        public ICommand LogoutCommand { get; }
        public ICommand SelectStationCommand { get; }
        public ICommand AnalyticsCommand { get; }

        // Constructor sets up initial state, subscriptions, and command bindings
        public DashboardViewModel()
        {
            EditOrdersCommand = new RelayCommand(EditOrders);
            EditLayoutCommand = new RelayCommand(EditLayout);
            AnalyticsCommand = new RelayCommand(Analytics);
            LogoutCommand = new RelayCommand(Logout);

            // This command seems duplicated — last one wins
            SelectStationCommand = new RelayCommand(OpenStationWindow);
            SelectStationCommand = new RelayCommand<string>(OpenStationWindow);

            Orders = OrderManager.Orders;
            OrderManager.OrdersUpdated += RefreshOrders;

            _ = LoadStationsAsync();

            // Preload pages to avoid rebuilding them repeatedly
            App.EditOrdersPage = new EditOrders();
            App.EditLayoutPage = new EditLayout();
            App.Analytics = new Analytics();
        }

        // Refresh UI when orders are updated
        private void RefreshOrders()
        {
            OnPropertyChanged(nameof(Orders));
        }

        // Notifies the UI of property changes
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // Loads the latest station layout from the server
        private async Task LoadStationsAsync()
        {
            await LayoutManager.GetStationsAsync();
            OnPropertyChanged(nameof(Stations));
        }

        // Opens the layout editor window with a password prompt
        private void EditLayout()
        {
            EditLayout editLayout = new();
            PasswordWindow passwordBox = new(App.EditLayoutPage)
            {
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };
            passwordBox.Show();
        }

        // Logs the user out, closes all secondary windows, and returns to the login screen
        private void Logout()
        {
            foreach (Window window in Application.Current.Windows)
            {
                if (window != Application.Current.MainWindow)
                {
                    window.Close();
                }
            }

            SessionManager.LogoutAsync();
            Application.Current.MainWindow.Content = new Login();
        }

        // Opens the selected station window (parameterless version)
        private void OpenStationWindow()
        {
            if (!string.IsNullOrEmpty(SelectedStation))
            {
                StationWindow stationWindow = new(SelectedStation);
                stationWindow.Show();
            }
        }

        // Opens a specific station window using the station name as a parameter
        private void OpenStationWindow(string station)
        {
            if (!string.IsNullOrEmpty(station))
            {
                StationWindow stationWindow = new(station);
                stationWindow.Show();
            }
        }

        // Opens the order editor window with a password prompt
        private void EditOrders()
        {
            if (App.EditOrdersPage == null)
            {
                App.EditOrdersPage = new EditOrders();
            }

            PasswordWindow passwordBox = new(App.EditOrdersPage)
            {
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };
            passwordBox.Show();
        }

        // Opens the analytics window with a password prompt
        private void Analytics()
        {
            PasswordWindow passwordBox = new(App.Analytics)
            {
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };
            passwordBox.Show();
        }
    }
}
