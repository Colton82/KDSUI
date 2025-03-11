using KDSUI.Models;
using KDSUI.Pages;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Security.Policy;

public static class OrderManager
{
    /// <summary>
    /// Maps orders to stations
    /// </summary>
    public static readonly Dictionary<string, List<DynamicOrderModel>> _stationOrders = new();

    /// <summary>
    /// Collection of all orders
    /// </summary>
    public static ObservableCollection<DynamicOrderModel> Orders { get; set; } = new ObservableCollection<DynamicOrderModel>();

    /// <summary>
    /// Event that is triggered when an order is updated
    /// </summary>
    public static event Action<DynamicOrderModel> OrderUpdated;

    /// <summary>
    /// Adds an order to the station orders dictionary
    /// </summary>
    /// <param name="order"></param>
    public static void AddOrder(DynamicOrderModel order)
    {
        if(order.Station == null)
            order.Station = LayoutManager.Stations[0];
        if (!_stationOrders.ContainsKey(order.Station))
            _stationOrders[order.Station] = new List<DynamicOrderModel>();

        if (LayoutManager.Stations.Count > 0)
        {
            string firstStation = LayoutManager.Stations[0];

            // Check if the timestamp already starts with a station prefix
            if (!order.Timestamp.StartsWith(firstStation + " | "))
            {
                // Add the station prefix
                order.Timestamp = $"{firstStation} | {order.Timestamp}";
            }
        }


        _stationOrders[order.Station].Add(order);
        Orders.Insert(0,order);;
    }

    /// <summary>
    /// Gets all orders for a station
    /// </summary>
    /// <param name="station"></param>
    /// <returns></returns>
    public static List<DynamicOrderModel> GetOrdersForStation(string station)
    {
        return _stationOrders.ContainsKey(station) ? _stationOrders[station] : new List<DynamicOrderModel>();
    }

    /// <summary>
    /// Updates the "station" property of an order and moves it to that place in the array
    /// </summary>
    /// <param name="order"></param>
    /// <param name="newStation"></param>
    public static void UpdateOrderStation(DynamicOrderModel order, string newStation)
    {
        if (_stationOrders.ContainsKey(order.Station))
        {
            _stationOrders[order.Station].Remove(order);
        }
        order.Station = newStation;
        if (order.Station == null)
            order.Station = LayoutManager.Stations[0];
        if (!_stationOrders.ContainsKey(order.Station))
            _stationOrders[order.Station] = new List<DynamicOrderModel>();

        order.Timestamp += $" | {order.Station} | {DateTime.Now:yyyy-MM-ddTHH:mm:ss.fff}";

        _stationOrders[order.Station].Add(order);
        OrderUpdated?.Invoke(order);
    }

    /// <summary>
    /// Removes an order from the orders collection and archives it for analytics
    /// </summary>
    /// <param name="order"></param>
    public static void ArchiveOrder(DynamicOrderModel order)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Removes an order from the station orders dictionary
    /// </summary>
    /// <param name="order"></param>
    public static void RemoveOrder(DynamicOrderModel order)
    {
        if (_stationOrders.ContainsKey(order.Station))
        {
            _stationOrders[order.Station].Remove(order);
        }
    }
}
