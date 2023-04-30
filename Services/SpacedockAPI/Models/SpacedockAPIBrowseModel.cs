using Newtonsoft.Json;
using ToucanServices.Data;

namespace ToucanServices.SpacedockAPI.Models
{
    public struct ResultVersion
    {
        [JsonProperty("friendly_version")]
        public string ModVersion { get; set; }

        [JsonProperty("game_version")]
        public string GameVersion { get; set; }

        [JsonProperty("created")]
        public DateTimeOffset Created;

        [JsonProperty("download_path")]
        public string DownloadUrl { get; set; }

        [JsonProperty("changelog")]
        public string Changelog { get; set; }

        [JsonProperty("downloads")]
        public UInt32 Downloads { get; set; }
    }

    public struct Result
    {
        [JsonProperty("name")] public string Name { get; set; }
        [JsonProperty("id")] public UInt32 Id { get; set; }
        [JsonProperty("game")] public string Game { get; set; }
        [JsonProperty("game_id")] public UInt32 GameId { get; set; }
        [JsonProperty("short_description")] public string ShortDescription { get; set; }
        [JsonProperty("downloads")] public UInt32 Downloads { get; set; }
        [JsonProperty("followers")] public UInt32 Followers { get; set; }
        [JsonProperty("author")] public string Author { get; set; }
        [JsonProperty("license")] public string License { get; set; }
        [JsonProperty("website")] public string Website { get; set; }
        [JsonProperty("donations")] public string Donations { get; set; }
        [JsonProperty("source_code")] public string SourceCode{ get; set; }
        [JsonProperty("url")] public string Url { get; set; }
        [JsonProperty("versions")] public List<ResultVersion> Versions { get; set; }
    }

    public struct SpacedockAPIBrowseModel
    {
        [JsonProperty("total")] public UInt32 Total { get; set; }
        [JsonProperty("count")] public UInt32 Count { get; set; }
        [JsonProperty("pages")] public UInt32 Pages { get; set; }
        [JsonProperty("page")] public UInt32 Page { get; set; }
        [JsonProperty("result")] public Result[] Results { get; set; }
    }
}
