using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using ToucanAPI.Data;

namespace ToucanClient.Spacedock
{
    public static class MISpacedockAPI
    {
        public async static Task<MIToucan> PopulateMIToucan(MIToucan _MIToucan)
        {
            HttpClient ToucanModManagerHttpClient = new HttpClient();
            ToucanModManagerHttpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:93.0; mm:Toucan-Mod-Manager;) Gecko/20100101 Firefox/93.0");

            HttpResponseMessage SpacedockGetAsync = await ToucanModManagerHttpClient.GetAsync($"https://spacedock.info/api/mod/{_MIToucan.SpacedockModificationId.Value}");
            SpacedockGetAsync.EnsureSuccessStatusCode();
            String SpacedockContent = await SpacedockGetAsync.Content.ReadAsStringAsync();

            dynamic SpacedockContentJson = JsonConvert.DeserializeObject(SpacedockContent)
                ?? throw new Exception("Failed to convert SpacedockContent to JSON. I dont know why.");

            // Populating
            _MIToucan.MetadataVersion = 0;
            _MIToucan.InstallPath = new MIString("", _MIToucan.InstallPath.isRequired);
            _MIToucan.InstallerPath = new MIString("", _MIToucan.InstallPath.isRequired);
            _MIToucan.SourceCode = GetJsonMIStringFromPath(_MIToucan.SourceCode, SpacedockContentJson, "source_code");
            _MIToucan.ModificationName = GetJsonMIStringFromPath(_MIToucan.ModificationName, SpacedockContentJson, "name");
            _MIToucan.ModificationVersions = GetJsonMIVersionAFromPath(_MIToucan.ModificationVersions, SpacedockContentJson, "versions");
            _MIToucan.ModificationDescriptionShort = GetJsonMIStringFromPath(_MIToucan.ModificationDescriptionShort, SpacedockContentJson, "short_description");
            _MIToucan.ModificationDescriptionLong = GetJsonMIStringFromPath(_MIToucan.ModificationDescriptionLong, SpacedockContentJson, "description");
            _MIToucan.ModificationAuthors = GetJsonMIStringAFromPathSPACEDOCK(_MIToucan.ModificationAuthors, SpacedockContentJson, "author");
            _MIToucan.ModificationWebsite = GetJsonMIStringFromPath(_MIToucan.ModificationWebsite, SpacedockContentJson, "website");
            _MIToucan.ModificationDonation = GetJsonMIStringFromPath(_MIToucan.ModificationDonation, SpacedockContentJson, "donations");
            _MIToucan.ModificationLicense = GetJsonMIStringFromPath(_MIToucan.ModificationLicense, SpacedockContentJson, "license");

            Console.WriteLine(JObject.FromObject(_MIToucan).ToString());
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
        private static MIAuthorA GetJsonMIStringAFromPathSPACEDOCK(MIAuthorA MIInput, JObject JsonObject, string JsonPath)
        {
            JToken? JsonToken = JsonObject.SelectToken(JsonPath);
            string AuthorName = (string?)JsonToken ?? (MIInput.isRequired ? throw new Exception("JsonToken is empty and isRequired is enabled so an error has been thrown.") : "");

            MIInput.Authors = new MIAuthor[1];
            MIInput.Authors[0] = new MIAuthor(AuthorName);

            return MIInput;
        }
        private static MIVersionA GetJsonMIVersionAFromPath(MIVersionA MIInput, JObject JsonObject, string JsonPath)
        {
            JArray? JsonToken = (JArray?)JsonObject.SelectToken(JsonPath);
            MIInput.ModificationVersions = new MIVersion[(JsonToken != null ? JsonToken.Count : 0)];

            // Crimes has been committed
            for (int i = 0; i < (JsonToken != null ? JsonToken.Count : 0); i++)
            {
                MISVersion NewMISVersion = new MISVersion((string?)JsonToken[i]["friendly_version"] ?? throw new Exception("Could not find firendly_version."));
                MIVersion NewMIVersion = new MIVersion();
                NewMIVersion.ModificationDownload = $"https://spacedock.info{(string?)JsonToken[i]["download_path"]}";
                NewMIVersion.ModificationSemanticVersion = NewMISVersion;

                MIInput.ModificationVersions[i] = NewMIVersion;
            }


            return MIInput;
        }
    }
}