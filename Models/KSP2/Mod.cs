using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Text.Json;
using ToucanUI.Models.KSP2;

public class Mod
{
    // Properties
    [JsonProperty("id")]
    public int Id { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("game")]
    public string Game { get; set; }

    [JsonProperty("game_id")]
    public int GameId { get; set; }

    [JsonProperty("short_description")]
    public string Description { get; set; }

    [JsonProperty("downloads")]
    public int Downloads { get; set; }

    [JsonProperty("followers")]
    public int Followers { get; set; }

    [JsonProperty("author")]
    public string Author { get; set; }

    [JsonProperty("default_version_id")]
    public int DefaultVersionId { get; set; }

    [JsonConverter(typeof(IgnoreErrorConverter))]
    [JsonProperty("shared_authors")]
    public List<string> SharedAuthors { get; set; }

    [JsonProperty("background")]
    public string Background { get; set; }

    [JsonProperty("license")]
    public string License { get; set; }

    [JsonProperty("website")]
    public string Website { get; set; }

    [JsonProperty("donations")]
    public string Donations { get; set; }

    [JsonProperty("source_code")]
    public string SourceCode { get; set; }

    [JsonProperty("url")]
    public string Url { get; set; }

    [JsonProperty("versions")]
    public List<Version> Versions { get; set; }

    // Constructor for deserialization
    [JsonConstructor]
    public Mod()
    {
    }

    // Serialize to JSON
    public string ToJson()
    {
        return JsonConvert.SerializeObject(this);
    }

    // Deserialize from JSON
    public static Mod FromJson(string jsonString)
    {
        var mod = JsonConvert.DeserializeObject<Mod>(jsonString);

        // Deserialize the Versions list
        var json = JObject.Parse(jsonString);
        if (json["versions"] != null)
        {
            mod.Versions = new List<Version>();
            foreach (var versionElement in json["versions"])
            {
                mod.Versions.Add(Version.FromJson(versionElement.ToString()));
            }
        }

        return mod;
    }


}
