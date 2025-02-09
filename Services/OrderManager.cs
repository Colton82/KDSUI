using KDSUI.Models;
using System;
using System.Collections.Generic;

public static class OrderManager
{
    /// <summary>
    /// Maps orders to stations
    /// </summary>
    private static readonly Dictionary<string, List<OrderModel>> _stationOrders = new();

    /// <summary>
    /// Event that is triggered when an order is updated
    /// </summary>
    public static event Action<OrderModel> OrderUpdated;

    /// <summary>
    /// Adds an order to the station orders dictionary
    /// </summary>
    /// <param name="order"></param>
    public static void AddOrder(OrderModel order)
    {
        if (!_stationOrders.ContainsKey(order.Station))
            _stationOrders[order.Station] = new List<OrderModel>();

        _stationOrders[order.Station].Add(order);

        OrderUpdated?.Invoke(order);
    }

    /// <summary>
    /// Gets all orders for a station
    /// </summary>
    /// <param name="station"></param>
    /// <returns></returns>
    public static List<OrderModel> GetOrdersForStation(string station)
    {
        return _stationOrders.ContainsKey(station) ? _stationOrders[station] : new List<OrderModel>();
    }

    /// <summary>
    /// Updates the "station" property of an order and moves it to that place in the array
    /// </summary>
    /// <param name="order"></param>
    /// <param name="newStation"></param>
    public static void UpdateOrderStation(OrderModel order, string newStation)
    {
        if (_stationOrders.ContainsKey(order.Station))
        {
            _stationOrders[order.Station].Remove(order);
        }

        order.Station = newStation;
        AddOrder(order);

        OrderUpdated?.Invoke(order);
    }

    /// <summary>
    /// Removes an order from the station orders dictionary
    /// </summary>
    /// <param name="order"></param>
    public static void RemoveOrder(OrderModel order)
    {
        if (_stationOrders.ContainsKey(order.Station))
        {
            _stationOrders[order.Station].Remove(order);
        }

        OrderUpdated?.Invoke(order);
    }
}
