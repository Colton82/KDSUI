using KDSUI;
using KDSUI.Data;
using KDSUI.Models;
using KDSUI.Pages;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

public static class OrderManager
{
    // Internal dictionary to group orders by station
    public static readonly Dictionary<string, List<DynamicOrderModel>> _stationOrders = new();

    // Observable collection of all active orders in the system
    public static ObservableCollection<DynamicOrderModel> Orders { get; set; } = new ObservableCollection<DynamicOrderModel>();

    // Event to notify subscribers whenever the order list is updated
    public static event Action OrdersUpdated;

    // DAO for database communication
    public static OrderDAO orderDAO = new OrderDAO();

    // Adds a new order to the appropriate station group and the shared order list
    public static void AddOrder(DynamicOrderModel order)
    {
        // If station is missing or invalid, assign it to the first station in layout
        if (!LayoutManager.Stations.Contains(order.Station))
            order.Station = LayoutManager.Stations[0];

        // Create the station entry in the dictionary if it doesn't exist yet
        if (!_stationOrders.ContainsKey(order.Station))
            _stationOrders[order.Station] = new List<DynamicOrderModel>();

        // Add the station name to the order’s timestamp if it's missing
        if (LayoutManager.Stations.Count > 0)
        {
            string firstStation = LayoutManager.Stations[0];
            if (!order.Timestamp.Contains(firstStation))
            {
                order.Timestamp = $"{firstStation} | {order.Timestamp}";
            }
        }

        // Ensure the Items list is initialized to avoid null references
        order.Items ??= new ObservableCollection<OrderItem>();

        // Add the order to the station-specific and global order lists
        _stationOrders[order.Station].Add(order);
        Orders.Insert(0, order);
    }

    // Retrieves all orders currently assigned to a specific station
    public static List<DynamicOrderModel> GetOrdersForStation(string station)
    {
        return _stationOrders.ContainsKey(station) ? _stationOrders[station] : new List<DynamicOrderModel>();
    }

    // Moves an order to a new station and updates its timestamp
    public static void UpdateOrderStation(DynamicOrderModel order, string newStation)
    {
        // Remove the order from its current station list
        if (_stationOrders.ContainsKey(order.Station))
        {
            _stationOrders[order.Station].Remove(order);
        }

        // Assign the new station, defaulting to the first station if none provided
        order.Station = newStation ?? LayoutManager.Stations[0];

        // Append station transition info to the timestamp
        order.Timestamp += $" | {order.Station} | {DateTime.Now:yyyy-MM-ddTHH:mm:ss.fff}";

        // If the order is not marked "Complete", add it to the new station and update it in the DB
        if (order.Station != "Complete")
        {
            if (!_stationOrders.ContainsKey(order.Station))
                _stationOrders.Add(order.Station, new List<DynamicOrderModel>());

            _stationOrders[order.Station].Add(order);
            orderDAO.UpdateOrderAsync(order);
        }
        else
        {
            // Completed orders are removed from the UI and archived in the database
            Orders.Remove(order);
            orderDAO.ArchiveOrder(order);
        }

        // Notify subscribers that orders have changed
        OrdersUpdated?.Invoke();
    }

    // Completely removes an order from both the station group and global list
    public static void RemoveOrder(DynamicOrderModel order)
    {
        if (_stationOrders.ContainsKey(order.Station))
        {
            _stationOrders[order.Station].Remove(order);
        }

        Orders.Remove(order);
    }

    // Updates an existing order’s data (e.g. name, station, item list)
    public static void UpdateOrder(DynamicOrderModel updatedOrder)
    {
        var existingOrder = Orders.FirstOrDefault(o => o.Id == updatedOrder.Id);

        if (existingOrder != null)
        {
            // Update basic properties
            existingOrder.CustomerName = updatedOrder.CustomerName;
            existingOrder.Station = updatedOrder.Station;

            // Replace item list
            existingOrder.Items.Clear();
            foreach (var item in updatedOrder.Items)
            {
                existingOrder.Items.Add(item);
            }

            // Sync with database
            orderDAO.UpdateOrderAsync(updatedOrder);
        }

        // Debug log and notify subscribers
        System.Diagnostics.Debug.WriteLine($"Updating order: {updatedOrder.Id}");
        OrdersUpdated?.Invoke();
    }

    // Triggers the OrdersUpdated event manually
    public static void TriggerOrdersUpdated()
    {
        OrdersUpdated?.Invoke();
    }
}
