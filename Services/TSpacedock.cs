using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToucanAPI.Data;

namespace ToucanAPI.Spacedock
{
    public static class TSpacedock // Boring
    {
        public async static Task<MIToucan> PopulateMIToucan(MIToucan _MIToucan)
        {
            HttpClient ToucanModManagerHttpClient = new HttpClient();
            ToucanModManagerHttpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:93.0; mm:Toucan-Mod-Manager;) Gecko/20100101 Firefox/93.0");

            HttpResponseMessage SpacedockGetAsync = await ToucanModManagerHttpClient.GetAsync($"https://spacedock.info/api/mod/{_MIToucan.MIToucanCreateInfo.SpacedockModificationId}");
            SpacedockGetAsync.EnsureSuccessStatusCode();
            String SpacedockContent = await SpacedockGetAsync.Content.ReadAsStringAsync();

            dynamic SpacedockContentJson = JsonConvert.DeserializeObject(SpacedockContent)
                ?? throw new Exception("Failed to convert SpacedockContent to JSON. I dont know why.");

            _MIToucan.ModificationName = GetJsonMIStringFromPath(_MIToucan.ModificationName, SpacedockContentJson, "name");

            return _MIToucan;
        }

        private static MIString GetJsonMIStringFromPath(MIString MIInput, JObject JsonObject, string JsonPath)
        {
            JToken? JsonToken = JsonObject.SelectToken(JsonPath);
            MIInput.Value = (string?)JsonToken ?? (MIInput.isRequired ? throw new Exception("JsonToken is empty and isRequired is enabled so an error has been thrown.") : "");

            return MIInput;
        }
        private static MIAuthorA GetJsonMIAuthorAFromPath(MIAuthorA MIInput, JObject JsonObject, string JsonPath)
        {
            JArray? JsonToken = (JArray?)JsonObject.SelectToken(JsonPath);
            MIInput.Authors = new MIAuthor[(JsonToken != null ? JsonToken.Count : 0)];

            // Crimes has been committed
            for (int i = 0; i < (JsonToken != null ? JsonToken.Count : 0); i++)
                MIInput.Authors[i] = new MIAuthor((string?)JsonToken[i]["name"] ?? "author-name-" + i + "-not-found", (string?)JsonToken[i]["contact_link"] ?? "contact-link-" + i + "-not-found");

            return MIInput;
        }
    }
}
