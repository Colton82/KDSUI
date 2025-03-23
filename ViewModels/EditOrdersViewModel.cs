using KDSUI.Data;
using KDSUI.Models;
using KDSUI;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows;
using GalaSoft.MvvmLight.Command;

namespace KDSUI.ViewModels
{
    /// <summary>
    /// ViewModel for the Edit Orders page.
    /// Handles order selection, editing, updating, and navigation.
    /// </summary>
    public class EditOrdersViewModel : INotifyPropertyChanged
    {
        // Constructor sets up initial bindings and pre-selects the first order
        public EditOrdersViewModel()
        {
            // Use the shared order list managed by OrderManager
            Orders = OrderManager.Orders;

            // Bind commands to actions
            UpdateOrderCommand = new RelayCommand(UpdateOrder);
            BackCommand = new RelayCommand(GoBack);

            // Refresh the UI when the orders are updated externally
            OrderManager.OrdersUpdated += RefreshOrders;

            // Pre-select the first order if available
            if (Orders.Any())
            {
                SelectedOrder = Orders.First();
            }
        }

        // Command for saving the current edits to the selected order
        public ICommand UpdateOrderCommand { get; }

        // Command to navigate back to the dashboard
        public ICommand BackCommand { get; }

        // List of all orders currently loaded
        public ObservableCollection<DynamicOrderModel> Orders { get; }

        private DynamicOrderModel _selectedOrder;

        // The order currently being edited
        public DynamicOrderModel SelectedOrder
        {
            get => _selectedOrder;
            set
            {
                _selectedOrder = value;
                OnPropertyChanged();

                // When a new order is selected, copy its items into an editable collection
                EditableItems = _selectedOrder != null
                    ? new ObservableCollection<OrderItem>(_selectedOrder.Items)
                    : new ObservableCollection<OrderItem>();
            }
        }

        private ObservableCollection<OrderItem> _editableItems = new();

        // A temporary, editable copy of the selected order's items
        public ObservableCollection<OrderItem> EditableItems
        {
            get => _editableItems;
            set
            {
                _editableItems = value;
                OnPropertyChanged();
            }
        }

        // Provides access to the list of available stations for dropdowns
        public ObservableCollection<string> Stations => LayoutManager.Stations;

        // Saves the changes made to the selected order and updates both in-memory and database versions
        private async void UpdateOrder()
        {
            if (SelectedOrder != null)
            {
                var updatedOrder = new DynamicOrderModel
                {
                    Id = SelectedOrder.Id,
                    CustomerName = SelectedOrder.CustomerName,
                    Station = SelectedOrder.Station,
                    Timestamp = SelectedOrder.Timestamp,
                    Items = new ObservableCollection<OrderItem>(EditableItems)
                };

                // Update in memory
                OrderManager.UpdateOrder(updatedOrder);

                // Update in database
                var orderDAO = new OrderDAO();
                await orderDAO.UpdateOrderAsync(updatedOrder);

                MessageBox.Show("Order updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        // Forces the UI to reload and display any external changes to the order list
        private void RefreshOrders()
        {
            System.Diagnostics.Debug.WriteLine("Forcing UI to recognize order updates...");

            var tempOrders = new ObservableCollection<DynamicOrderModel>(OrderManager.Orders);
            Orders.Clear();
            foreach (var order in tempOrders)
            {
                Orders.Add(order);
            }

            OnPropertyChanged(nameof(Orders));
        }

        // Navigates back to the dashboard
        private void GoBack()
        {
            Application.Current.MainWindow.Content = App.DashboardPage;
        }

        // Notifies the UI when a property changes
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
