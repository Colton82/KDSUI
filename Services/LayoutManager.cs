using System.Collections.ObjectModel;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using KDSUI.Services;
using Newtonsoft.Json;

public static class LayoutManager
{
    // Static HTTP client used for communicating with the backend
    private static readonly HttpClient _httpClient = new HttpClient();

    // Observable list of station names bound to the UI
    public static ObservableCollection<string> Stations { get; set; } = new ObservableCollection<string>();

    // Static constructor automatically fetches station data when the class is first used
    static LayoutManager() => GetStationsAsync();

    // Retrieves the station layout for the current user from the API
    public static async Task GetStationsAsync()
    {
        var username = SessionManager._username;

        try
        {
            // Add the user's authentication token to the request
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", SessionManager._jwtToken);

            // Make a GET request to retrieve the user's saved layout
            var response = await _httpClient.GetStringAsync($"https://localhost:7121/api/Layout?username={username}");

            // Deserialize the station list from JSON
            var stationList = JsonConvert.DeserializeObject<List<string>>(response);

            // If no stations exist, provide a fallback default
            if (stationList == null || stationList.Count <= 0)
            {
                stationList = new List<string>()
                {
                    "Station 1"
                };
            }

            // If station names are numeric, convert them to "Station {number}"
            for (int i = 0; i < stationList.Count; i++)
            {
                if (stationList[i].All(char.IsDigit))
                {
                    stationList[i] = $"Station {stationList[i]}";
                }
            }

            // Clear the existing observable list and repopulate it
            Stations.Clear();

            if (stationList != null)
            {
                stationList.ForEach(station => Stations.Add(station));
            }
        }
        catch (Exception ex)
        {
            // Log any errors to the debug console
            System.Diagnostics.Debug.WriteLine($"Error loading stations: {ex.Message}");
        }
    }

    // Sends the current list of stations to the backend to be saved
    public static async Task SaveStationsAsync()
    {
        var username = SessionManager._username;

        try
        {
            // Add authentication token to the request
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", SessionManager._jwtToken);

            // Serialize the station list to JSON
            var jsonContent = new StringContent(JsonConvert.SerializeObject(Stations), Encoding.UTF8, "application/json");

            // Send a POST request to save the layout for the current user
            var response = await _httpClient.PostAsync($"https://localhost:7121/api/Layout/save/{username}", jsonContent);

            // Ensure the response indicates success; otherwise, throw an exception
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            // Log any errors to the debug console
            System.Diagnostics.Debug.WriteLine($"Error saving stations: {ex.Message}");
        }
    }
}
