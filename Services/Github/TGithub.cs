using Microsoft.VisualBasic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Net;
using System.Net.Http;

using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ToucanAPI.Data;
using System.IO;

namespace ToucanAPI.Github
{
    public static class TGithub
    {
        public static async Task<MIToucan> PopulateMIToucan(MIToucan _MIToucan)
        {
            string ToucanModManagerGetUrl = _MIToucan.MIToucanCreateInfo.GithubInformation.GetGihubApiUrl();
            HttpClient ToucanModManagerHttpClient = new HttpClient();
            ToucanModManagerHttpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:93.0; mm:Toucan-Mod-Manager;) Gecko/20100101 Firefox/93.0");

            dynamic ToucanGithubApiResponseData = await FetchToucanModData(ToucanModManagerHttpClient, ToucanModManagerGetUrl);
            JObject ToucanMetadata = await TryGetToucanMetadata(ToucanGithubApiResponseData, ToucanModManagerHttpClient);

            _MIToucan.ModificationName                  = GetJsonMIStringFromPath(_MIToucan.ModificationName, ToucanMetadata, "tmm-metadata.mod-specifications.mod_name");
            _MIToucan.ModificationVersion               = GetJsonMIStringFromPath(_MIToucan.ModificationVersion, ToucanMetadata, "tmm-metadata.mod-specifications.mod_version");
            _MIToucan.ModificationDescriptionShort      = GetJsonMIStringFromPath(_MIToucan.ModificationDescriptionShort, ToucanMetadata, "tmm-metadata.mod-specifications.mod_description.short");
            _MIToucan.ModificationDescriptionLong       = GetJsonMIStringFromPath(_MIToucan.ModificationDescriptionLong, ToucanMetadata, "tmm-metadata.mod-specifications.mod_description.long");
            _MIToucan.ModificationAuthors               = GetJsonMIAuthorAFromPath(_MIToucan.ModificationAuthors, ToucanMetadata, "tmm-metadata.mod-specifications.mod_authors");
            _MIToucan.ModificationLicense               = GetJsonMIStringFromPath(_MIToucan.ModificationLicense, ToucanMetadata, "tmm-metadata.mod-specifications.license");
            _MIToucan.ModificationInstallationPath      = GetJsonMIStringFromPath(_MIToucan.ModificationInstallationPath, ToucanMetadata, "tmm-metadata.mod-specifications.install_path"); 

            return _MIToucan;
        }

        private async static Task<dynamic> TryGetToucanMetadata(dynamic ToucanModManagerGetResponseContentJSON, HttpClient ToucanModManagerHttpClient)
        {
            foreach (dynamic ResponseContentJsonObject in ToucanModManagerGetResponseContentJSON.assets)
            {
                if (ResponseContentJsonObject.name == "TMM-METADATA.json")
                {
                    string MetadataDownloadUrl = ResponseContentJsonObject.browser_download_url;
                    dynamic MetadataGetAsync = await ToucanModManagerHttpClient.GetAsync(MetadataDownloadUrl);
                    MetadataGetAsync.EnsureSuccessStatusCode();

                    MemoryStream MetadataMemoryStream = await MetadataGetAsync.Content.ReadAsStreamAsync();
                    MetadataMemoryStream.Position = 0;
                    using StreamReader MetadataMemoryStreamReader = new StreamReader(MetadataMemoryStream);

                    string MetadataJsonString = await MetadataMemoryStreamReader.ReadToEndAsync();

                    dynamic MetadataJson = JsonConvert.DeserializeObject(MetadataJsonString)
                        ?? throw new Exception("Failed to convert MetadataSteam to JSON. This may be because TMM-METADATA.json is empty.");

                    return MetadataJson;
                }
            }

            throw new Exception("'TMM-METADATA.json' was not found in github repo or release. If this is your mod remember to attach the TMM-METADATA json as a binary when creaing a release and if have the json in root if its a repo.");
        }

        private async static Task<dynamic> FetchToucanModData(HttpClient ToucanModManagerHttpClient, string ToucanModManagerGetUrl) 
        {
            HttpResponseMessage ToucanModManagerGetResponse = await ToucanModManagerHttpClient.GetAsync(ToucanModManagerGetUrl);
            ToucanModManagerGetResponse.EnsureSuccessStatusCode();
            string ToucanModManagerGetResponseContent = await ToucanModManagerGetResponse.Content.ReadAsStringAsync();
            dynamic ToucanModManagerGetResponseContentJSON = JsonConvert.DeserializeObject<JObject>(ToucanModManagerGetResponseContent) 
                ?? throw new Exception("JObject TouanModManagerGetResponseContentJSON is null. This may be because it didnt find ");

            return ToucanModManagerGetResponseContentJSON;
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
