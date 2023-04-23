using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using ToucanUI.Models;
using System.Diagnostics;
using System.Text.Json;
using Avalonia.Platform;
using Avalonia;
using System.IO;

// THIS IS JUST TEMPORARY, WE CAN FIX IT UP LATER
// THE ACTUAL API IS NOT FULLY WORKING BUT DUMMY DATA DOES, CANT BE BOTHERED TO FIX IF WE ARE REWRITING THIS ANYWAY

namespace ToucanUI.Services
{
    public class SpacedockAPI
    {
        private const string BASE_URL = "https://spacedock.info/api/browse?&game_id=22407";
        private readonly HttpClient _client;

        public SpacedockAPI()
        {
            _client = new HttpClient();
        }

        public async Task<List<Mod>> GetMods(bool useDummyData = false)
        {
            var mods = new List<Mod>();


            if (useDummyData)
            {
                Debug.WriteLine("Using dummy data");
                // Load data from file
                var assembly = typeof(SpacedockAPI).Assembly;
                var resource = assembly.GetManifestResourceStream("ToucanUI.Assets.output.json");
                var jsonString = new StreamReader(resource).ReadToEnd();

                var jsonDocument = JsonDocument.Parse(jsonString);
                var data = jsonDocument.RootElement;

                mods = ParseModData(data);
            }

            else
            {
                Debug.WriteLine("Getting mod data");
                // Get total number of pages
                var url = $"{BASE_URL}";
                Debug.WriteLine($"Requesting total number of pages from {url}");
                var response = await _client.GetAsync(url);
                Debug.WriteLine($"Response status code: {response.StatusCode}");
                var json = await response.Content.ReadAsStringAsync();
                var data = JsonSerializer.Deserialize<JsonElement>(json);
                int pages = data.GetProperty("pages").GetInt32();
                Debug.WriteLine($"Total number of pages: {pages}");

                // Iterate through all pages and retrieve mods
                for (int page = 1; page <= pages; page++)
                {
                    var pageUrl = $"{url}&page={page}";
                    Debug.WriteLine($"Requesting mods from {pageUrl}");
                    var pageResponse = await _client.GetAsync(pageUrl);
                    Debug.WriteLine($"Response status code: {pageResponse.StatusCode}");
                    var pageJson = await pageResponse.Content.ReadAsStringAsync();
                    var pageData = JsonSerializer.Deserialize<JsonElement>(pageJson);

                    mods = ParseModData(pageData);

                    
                }

                Debug.WriteLine($"Finished retrieving {mods.Count} mods");
                
            }

            return mods;

        }

        public List<Mod> ParseModData(JsonElement data)
        {
            var mods = new List<Mod>();

            foreach (var item in data.GetProperty("result").EnumerateArray())
            {
                try
                {
                    var mod = new Mod(item);
                    mods.Add(mod);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error: {ex}");
                }

                Debug.WriteLine($"Retrieved {mods.Count} mods so far");
            }

            return mods;
        }

    }
}
