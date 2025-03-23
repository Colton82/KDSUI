using KDSUI.Data;
using KDSUI.Services;
using KDSUI.ViewModels;
using System.Windows.Controls;

namespace KDSUI.Pages
{
    // Code-behind for the Dashboard page UI
    public partial class Dashboard : Page
    {
        public Dashboard()
        {
            InitializeComponent();

            // Outputs the current number of orders to the debug console
            System.Diagnostics.Debug.WriteLine(OrderManager.Orders.Count());

            // Establishes a WebSocket connection for receiving real-time updates
            WebSocketClient.ConnectAsync();

            // Fetches existing orders from the backend API for the current user
            OrderDAO orderDAO = new OrderDAO();
            orderDAO.GetOrders(SessionManager._username);

            // Sets the data context to the DashboardViewModel for data binding
            DataContext = new DashboardViewModel();
        }
    }
}
