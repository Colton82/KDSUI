using System;
using System.Collections.ObjectModel;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using KDSUI.Models;
using Newtonsoft.Json;

/// <summary>
/// Client for the WebSocket server, used by the POS system to receive orders
/// </summary>
public static class WebSocketClient
{
    private static readonly ClientWebSocket _webSocket = new ClientWebSocket();
    private static readonly Uri _serverUri = new Uri("wss://localhost:7121/wss/orders");

    public static ObservableCollection<OrderModel> Orders { get; set; } = new ObservableCollection<OrderModel>();

    /// <summary>
    /// Event that is raised when a new order is received, used to update station views
    /// </summary>
    public static event Action<OrderModel> OrderReceived;

    /// <summary>
    /// Connects to the WebSocket server
    /// </summary>
    /// <returns></returns>
    public static async Task ConnectAsync()
    {
        try
        {
            await _webSocket.ConnectAsync(_serverUri, CancellationToken.None);
            Console.WriteLine("Connected to WebSocket server.");
            await ListenForOrders();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"WebSocket connection error: {ex.Message}");
        }
    }

    /// <summary>
    /// Listens for incoming orders
    /// </summary>
    /// <returns></returns>
    private static async Task ListenForOrders()
    {
        var buffer = new byte[1024 * 4];

        while (_webSocket.State == WebSocketState.Open)
        {
            var result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            if (result.MessageType == WebSocketMessageType.Text)
            {
                string json = Encoding.UTF8.GetString(buffer, 0, result.Count);
                var order = JsonConvert.DeserializeObject<OrderModel>(json);

                // Add to global Orders collection
                Orders.Insert(0, order);

                OrderManager.AddOrder(order);

                // Notify all station windows
                OrderReceived?.Invoke(order);

                Console.WriteLine($"New order received: {order.Id} for station {order.Station}");
            }
        }
    }
}
