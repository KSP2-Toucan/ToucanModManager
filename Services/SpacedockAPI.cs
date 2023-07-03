using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

// This API is being used temporarily, until we get our own database and API up and running which will greatly simplify things

namespace ToucanUI.Services
{
    public class SpacedockAPI
    {
        // =====================
        // VARIABLES
        // =====================
        private const string BROWSE_URL = "https://spacedock.info/api/browse";
        private const string MOD_URL = "https://spacedock.info/api/mod";
        /*private const string GAME_ID = "&game_id=22407";*/
        private const string GAME_ID = "&game_id=3102";
        private readonly HttpClient _client;

        public enum Category
        {
            All,
            Top,
            New,
            Featured
        }

        // =====================
        // CONSTRUCTOR
        // =====================
        public SpacedockAPI()
        {
            _client = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
        }


        // =====================
        // METHODS
        // =====================

        // Returns a single Mod from spacedock API
        public async Task<Mod> GetMod(int id)
        {
            Mod mod = null;

            try
            {
                // Request mod data from the API using the provided id
                var url = $"{MOD_URL}/{id}?{GAME_ID}";
                Debug.WriteLine($"Requesting mod data from {url}");
                var response = await _client.GetAsync(url);
                Debug.WriteLine($"Response status code: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();

                    // Parse the JSON data
                    //var jsonDocument = JsonDocument.Parse(jsonString);
                    //var data = jsonDocument.RootElement;

                    // Create a Mod object from the JSON data
                    mod = Mod.FromJson(jsonString);
                }
                else
                {
                    Debug.WriteLine($"Error retrieving mod data: {response.ReasonPhrase}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error retrieving mod data: {ex.Message}");
            }

            return mod;
        }


        // Returns a list of Mods from spacedock API
        public async Task<List<Mod>> GetMods(Category category = Category.All)
        {
            string categoryString;

            switch (category)
            {
                case Category.All:
                    categoryString = "";
                    break;

                case Category.Top:
                    categoryString = "/top";
                    break;

                case Category.New:
                    categoryString = "/new";
                    break;

                case Category.Featured:
                    categoryString = "/featured";
                    break;

                default:
                    categoryString = "";
                    break;
            }

            var mods = new List<Mod>();
            Debug.WriteLine($"Fetching mod data for category {category}");

            try
            {
                // Request mod data from the first URL
                var url = $"{BROWSE_URL}{categoryString}?{GAME_ID}";
                Debug.WriteLine($"Requesting mod data from {url}");
                var response = await _client.GetAsync(url);
                Debug.WriteLine($"Response status code: {response.StatusCode}");
                var jsonString = await response.Content.ReadAsStringAsync();

                // Parse the JSON data
                var jsonDocument = JsonDocument.Parse(jsonString);
                var data = jsonDocument.RootElement;

                if (category == Category.All)
                {
                    // Get the totalPages
                    int totalPages = data.GetProperty("pages").GetInt32();

                    // Now iterate through all the pages
                    for (int currentPage = 1; currentPage <= totalPages; currentPage++)
                    {

                        // Request mod data from the URL
                        url = $"{BROWSE_URL}{categoryString}?{GAME_ID}&page={currentPage}";
                        Debug.WriteLine($"Requesting mod data from {url} on page {currentPage}");
                        response = await _client.GetAsync(url);
                        Debug.WriteLine($"Response status code: {response.StatusCode}");
                        jsonString = await response.Content.ReadAsStringAsync();

                        // Parse the mod data
                        var modData = ParseModData(jsonString);
                        mods.AddRange(modData);

                        //Debug.WriteLine($"Finished retrieving {modData.Count} mods from page {currentPage}");
                    }
                }

                else
                {
                    var modData = ParseModData(jsonString);
                    mods.AddRange(modData);
                }



                Debug.WriteLine($"{mods.Count} mods retrieved!");

                mods = mods.OrderBy(mod => mod.Name).ToList();
            }

            catch (Exception ex)
            {
                Debug.WriteLine($"Error retrieving mod data: {ex.Message}");
            }

            return mods;

        }

        // Method to ping Spacedock.info to see if it's online
        public async Task<bool> PingSpacedock()
        {
            try
            {
                var response = await _client.GetAsync("https://spacedock.info/");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error pinging Spacedock: {ex.Message}");
                return false;
            }
        }

        public List<Mod> ParseModData(string jsonString)
        {
            var mods = new List<Mod>();

            try
            {
                var json = JObject.Parse(jsonString);
                JToken dataArray;

                if (json["result"] != null) // If the "results" property exists
                {
                    dataArray = json["result"];
                }
                else if (json.Type == JTokenType.Array) // If the JSON data is directly an array
                {
                    dataArray = JArray.Parse(jsonString);
                }
                else // If the JSON data is neither an array nor has a "results" property
                {
                    Debug.WriteLine($"Unexpected JSON data format: {jsonString}");
                    return mods;
                }

                foreach (var item in dataArray)
                {
                    try
                    {
                        var mod = Mod.FromJson(item.ToString());
                        mods.Add(mod);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error: {ex}");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error parsing mod data: {ex.Message}");
            }

            return mods;
        }



    }
}
