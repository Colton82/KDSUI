using System.Collections.ObjectModel;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using KDSUI.Services;
using Newtonsoft.Json;

public static class LayoutManager
{
    private static readonly HttpClient _httpClient = new HttpClient();

    public static ObservableCollection<string> Stations { get; private set; } = new ObservableCollection<string>();

    static LayoutManager()
    {
        GetStationsAsync();
    }

    /// <summary>
    /// Loads stations from the API asynchronously
    /// </summary>
    /// <returns></returns>
    public static async Task GetStationsAsync()
    {
        var id = SessionManager._userId;

        try
        {
            var response = await _httpClient.GetStringAsync($"https://localhost:7121/api/Layout?id={id}");

            var stationList = JsonConvert.DeserializeObject<List<string>>(response);

            Stations = new ObservableCollection<string>(stationList);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading stations: {ex.Message}");
        }
    }

    /// <summary>
    /// Saves the current Stations collection to the API asynchronously
    /// </summary>
    /// <returns></returns>
    public static async Task SaveStationsAsync()
    {
        var id = SessionManager._userId;

        try
        {
            var jsonContent = JsonConvert.SerializeObject(Stations);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"https://localhost:7121/api/Layout/save/{id}", content);


            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error saving stations: {ex.Message}");
        }
    }
}
