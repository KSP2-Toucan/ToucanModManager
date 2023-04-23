using Newtonsoft.Json;

namespace ToucanServices.SpacedockAPI.Models
{
    public struct Result
    {
        [JsonProperty("name")] public string Name { get; set; }
        [JsonProperty("id")] public UInt32 Id { get; set; }
        [JsonProperty("game")] public string Game { get; set; }

        [JsonProperty("game_id")] public UInt32 GameId { get; set; }
        [JsonProperty("short_description")] public string ShrotDescription { get; set; }
        [JsonProperty("author")] public string Author { get; set; }
        [JsonProperty("license")] public string License { get; set; }
        [JsonProperty("website")] public string Website { get; set; }
        [JsonProperty("donations")] public string Donations { get; set; }
        [JsonProperty("source_code")] public string SourceCode{ get; set; }
        [JsonProperty("url")] public string Url { get; set; }

    }

    public struct SpacedockAPIBrowserModel
    {
        [JsonProperty("total")] public UInt32 Total { get; set; }
        [JsonProperty("Count")] public UInt32 Count { get; set; }
        [JsonProperty("Pages")] public UInt32 Pages { get; set; }
        [JsonProperty("Page")] public UInt32 Page { get; set; }
    }
}
