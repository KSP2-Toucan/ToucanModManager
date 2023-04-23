using Newtonsoft.Json;

/*
 *  We do not currently have support for
 *      
 *      shared_authors      | I dont see why we would need it, implementation is planned in the future though
 *      bg_offset_y         | Seems irrelevant for what we need and would ever need 
 *      description_html    | We dont have any HTML.
 * 
 *  Other than that there is still some things we have in the model 
 *  but will never actually use. Things such as background is never 
 *  needed as we dont have a place to put it. The background wont 
 *  be loaded or downloaded either, it just represents a string to 
 *  the url where you can download it.
 */

namespace ToucanServices.SpacedockAPI.Models
{

    public struct SpacedockAPIModificationVersionsModel
    {
        [JsonProperty("friendly_version")] public string FriendlyVersion { get; set; }
        [JsonProperty("name")] public string GameVersion { get; set; }
        [JsonProperty("id")] public ulong Id { get; set; }
        [JsonProperty("created")] public string Created { get; set; }
        [JsonProperty("download_path")] public string DownloadPath { get; set; }
        [JsonProperty("changelog")] public string Changelog { get; set; }
        [JsonProperty("downloads")] public ulong Downloads { get; set; }
    }

    public struct SpacedockAPIModificationModel
    {
        [JsonProperty("name")] public string Name { get; set; }
        [JsonProperty("id")] public ulong Id { get; set; }
        [JsonProperty("game")] public string Game { get; set; }
        [JsonProperty("game_id")] public ulong GameId { get; set; }
        [JsonProperty("short_description")] public string ShortDescription { get; set; }
        [JsonProperty("downloads")] public ulong Downloads { get; set; }
        [JsonProperty("followers")] public uint Followers { get; set; }
        [JsonProperty("author")] public string Author { get; set; }
        [JsonProperty("default_version_id")] public uint DefaultVersionID { get; set; }
        [JsonProperty("background")] public string Background { get; set; }
        [JsonProperty("license")] public string License { get; set; }
        [JsonProperty("website")] public string Website { get; set; }
        [JsonProperty("donations")] public string Donations { get; set; }
        [JsonProperty("url")] public string Url { get; set; }
        [JsonProperty("versions")] public SpacedockAPIModificationVersionsModel[] Versions { get; set; }
        [JsonProperty("description")] public string Description { get; set; }
    }
}
