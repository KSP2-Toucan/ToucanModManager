using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ToucanServices.Services.API.Models.Spacedock;
using ToucanServices.Services.Data;

namespace ToucanServices.Services.API.Types.Spacedock
{
    public static class SpacedockModification
    {
        /// <summary>
        /// Creates a spacedock api ulr from the input id. 
        /// </summary>
        /// <param name="Id"></param>
        /// <returns>The url</returns>
        public static string GetModificationApiUrlById(ulong Id) => $"https://spacedock.info/api/mod/{Id}";

        /// <summary>
        /// This function uses an HttpClient to send a GET request to the 
        /// GetModificationUrlById(<paramref name="Id"/>) to retireve information about a 
        /// specific modification.
        /// </summary>
        /// <param name="Id"></param>
        /// <returns>The json response in a string</returns>
        public static async Task<string> GetModificationInformationJsonString(ulong Id)
        {
            string ModificationUlr = GetModificationApiUrlById(Id);

            HttpClient ToucanHttpClient = ServicesHandler.GetHttpClient();

            HttpResponseMessage ToucanHttpClientResponse = await ToucanHttpClient.GetAsync(ModificationUlr);
            ToucanHttpClientResponse.EnsureSuccessStatusCode();
            string ResponseContentJsonString = await ToucanHttpClientResponse.Content.ReadAsStringAsync();
            return ResponseContentJsonString;
        }

        /// <summary>
        /// This function converts the data recieved by 
        /// GetModificationInformationJsonString(<paramref name="Id"/>) and parses it to a 
        /// JObject before returning it.
        /// </summary>
        /// <param name="Id"></param>
        /// <returns>JObject containing data about a mod from Spacedock</returns>
        public static async Task<JObject> GetModificationInformationJson(ulong Id) => JObject.Parse(await GetModificationInformationJsonString(Id));

        /// <summary>
        /// JsonConvert.DeserializeObjects the json string retrieved from 
        /// GetModificationInformationJsonString(<paramref name="Id"/>)
        /// </summary>
        /// <param name="Id"></param>
        /// <returns>The converted struct</returns>
        public static async Task<SpacedockModificationModel> GetModificationModel(ulong Id)
            => JsonConvert.DeserializeObject<SpacedockModificationModel>(await GetModificationInformationJsonString(Id));
    }
}
