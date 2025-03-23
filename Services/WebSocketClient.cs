using System;
using System.Collections.ObjectModel;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using KDSUI.Models;
using KDSUI.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

/// <summary>
/// Manages the WebSocket connection used to receive real-time orders from the POS system.
/// </summary>
public static class WebSocketClient
{
    // Persistent WebSocket client instance used for the connection
    private static ClientWebSocket _webSocket = new ClientWebSocket();

    // Event that is triggered whenever a new order is received via WebSocket
    public static event Action<DynamicOrderModel> OrderReceived;

    // Constructs the WebSocket URI
    private static Uri GetWebSocketUri()
    {
        return new Uri("wss://localhost:7121/wss/orders");
    }

    public static async Task ConnectAsync()
    {
        int maxRetries = 5;
        int delay = 2000;
        Uri serverUri = GetWebSocketUri();
        if (_webSocket == null)
            _webSocket = new ClientWebSocket();

        // Attach JWT token as Authorization header
        _webSocket.Options.SetRequestHeader("Authorization", $"Bearer {SessionManager._jwtToken}");
        System.Diagnostics.Debug.WriteLine($"Attaching JWT: {SessionManager._jwtToken}");

        Console.WriteLine($"Attempting to connect to WebSocket: {serverUri}");

        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                await _webSocket.ConnectAsync(serverUri, CancellationToken.None);
                Console.WriteLine("Connected to WebSocket server.");
                await ListenForOrders();
                break;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"WebSocket connection error (Attempt {attempt}/{maxRetries}): {ex.Message}");

                if (attempt == maxRetries)
                    throw;

                await Task.Delay(delay);
            }
        }
    }


    // Continuously listens for incoming order messages from the server
    private static async Task ListenForOrders()
    {
        var buffer = new byte[1024 * 4];

        while (_webSocket.State == WebSocketState.Open)
        {
            var result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

            // Handle only text-based messages
            if (result.MessageType == WebSocketMessageType.Text)
            {
                string json = Encoding.UTF8.GetString(buffer, 0, result.Count);

                if (string.IsNullOrWhiteSpace(json))
                {
                    Console.WriteLine("Received empty JSON. Skipping...");
                    continue;
                }

                try
                {
                    // Parse the incoming JSON into a JObject
                    JObject jObject = JObject.Parse(json);
                    Console.WriteLine($"Parsed JSON: {jObject}");

                    // Ensure "Items" exists, even if it's an empty array
                    if (!jObject.ContainsKey("Items"))
                    {
                        jObject["Items"] = new JArray();
                    }

                    // Default station assignment if missing
                    if (!jObject.ContainsKey("station") || string.IsNullOrWhiteSpace(jObject["station"]?.ToString()))
                    {
                        jObject["station"] = LayoutManager.Stations.FirstOrDefault() ?? "Unassigned";
                    }

                    // Convert the JObject into a DynamicOrderModel instance
                    DynamicOrderModel order = jObject.ToObject<DynamicOrderModel>();

                    // Deserialize the order items properly into ObservableCollection
                    if (jObject["Items"] is JArray itemsArray)
                    {
                        var deserializedItems = itemsArray.ToObject<List<OrderItem>>() ?? new List<OrderItem>();
                        order.Items = new ObservableCollection<OrderItem>(deserializedItems);
                    }

                    // Add the order to the system and trigger any live updates
                    OrderManager.AddOrder(order);
                    OrderReceived?.Invoke(order);
                    Console.WriteLine($"New order received: {order.Id} for station {order.Station}");
                }
                catch (JsonException ex)
                {
                    Console.WriteLine($"Error parsing JSON: {ex.Message}");
                }
                catch (ArgumentNullException ex)
                {
                    Console.WriteLine($"Null value error: {ex.Message}");
                }
            }
        }
    }

    public static async Task DisconnectAsync()
    {
        try
        {
            if (_webSocket != null && _webSocket.State == WebSocketState.Open)
            {
                Console.WriteLine("Closing WebSocket due to logout...");
                await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "User logout", CancellationToken.None);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error while closing WebSocket: {ex.Message}");
        }
        finally
        {
            _webSocket?.Dispose();
            _webSocket = null;
        }
    }
}
