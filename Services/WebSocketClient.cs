using System;
using System.Collections.ObjectModel;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using KDSUI.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

/// <summary>
/// Client for the WebSocket server, used by the POS system to receive orders
/// </summary>
public static class WebSocketClient
{
    private static readonly ClientWebSocket _webSocket = new ClientWebSocket();
    private static readonly Uri _serverUri = new Uri("wss://localhost:7121/wss/orders");

    /// <summary>
    /// Event that is raised when a new order is received, used to update station views
    /// </summary>
    public static event Action<DynamicOrderModel> OrderReceived;

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

                if (string.IsNullOrWhiteSpace(json))
                {
                    Console.WriteLine("Received empty JSON. Skipping...");
                    continue;
                }

                try
                {
                    JObject jObject = JObject.Parse(json);
                    Console.WriteLine($"Parsed JSON: {jObject}");

                    // Ensure 'items' exists, otherwise set an empty dictionary
                    if (!jObject.ContainsKey("Items"))
                    {
                        jObject["Items"] = new JObject(); // Default to empty dictionary
                    }

                    // If station is missing, assign to the first available station
                    if (!jObject.ContainsKey("station") || string.IsNullOrWhiteSpace(jObject["station"]?.ToString()))
                    {
                        if (LayoutManager.Stations.Count > 0)
                        {
                            jObject["station"] = LayoutManager.Stations[0]; // Assign first station
                        }
                        else
                        {
                            jObject["station"] = "Unassigned"; // Default if no stations exist
                        }
                    }

                    // Convert JSON to DynamicOrderModel
                    DynamicOrderModel order = jObject.ToObject<DynamicOrderModel>();

                    // Ensure 'items' is stored as a Dictionary<string, object>
                    if (jObject["Items"] is JObject itemsObject)
                    {
                        var deserializedItems = itemsObject.ToObject<Dictionary<string, object>>();

                        // Fix any nested JSON strings that are stored inside `Items`
                        order.Items = ParseNestedItems(deserializedItems);
                    }

                    // Insert into orders and notify system
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

    public static Dictionary<string, object> ParseNestedItems(Dictionary<string, object> items)
    {
        var parsedItems = new Dictionary<string, object>();

        foreach (var kvp in items)
        {
            if (kvp.Value is string jsonString && IsJson(jsonString))
            {
                try
                {
                    // Deserialize nested JSON strings into Dictionary<string, object>
                    var nestedDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonString);
                    parsedItems[kvp.Key] = ParseNestedItems(nestedDict); // Recursively parse deeper
                }
                catch
                {
                    parsedItems[kvp.Key] = kvp.Value; // Keep as-is if deserialization fails
                }
            }
            else if (kvp.Value is JObject jObject)
            {
                // Directly convert JObject to Dictionary
                parsedItems[kvp.Key] = ParseNestedItems(jObject.ToObject<Dictionary<string, object>>());
            }
            else
            {
                parsedItems[kvp.Key] = kvp.Value;
            }
        }

        return parsedItems;
    }

    /// <summary>
    /// Checks if a string is valid JSON.
    /// </summary>
    private static bool IsJson(string input)
    {
        input = input.Trim();
        return (input.StartsWith("{") && input.EndsWith("}")) || (input.StartsWith("[") && input.EndsWith("]"));
    }

}