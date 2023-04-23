using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using ToucanServices.SpacedockAPI.Models;

namespace ToucanServices.SpacedockAPI
{
    public static class SpacedockAPIRetrieveModificationInformation
    {
        /// <summary>
        /// Converts the SpacedockAPIModificationModel struct into a Data.
        /// ServicesModifciationInformation and then returns it, a little heads up is that 
        /// upvotes and downvotes will always be 0 and the MetadataSpecs.MetadataType will 
        /// be "Spacedock", so if you ever want to check in runtime which struct you are 
        /// working with check there.
        /// </summary>
        /// <returns></returns>
        public static Data.ServicesModificationInformation ConvertToServicesModificationInformation(SpacedockAPIModificationModel Model)
        {
            /* Metadata Specs*/

            Data.MetadataSpecs MetadataSpecs = new Data.MetadataSpecs();
            MetadataSpecs.MetadataVersion = -1;
            MetadataSpecs.MetadataType = "Spacedock";

            /* File Specs */

            Data.FileSpecs FileSpecs = new Data.FileSpecs();
            FileSpecs.InstallPath = "";

            /* Mod Specs */

            Data.ModSpecs ModSpecs = new Data.ModSpecs();
            ModSpecs.ModName = Model.Name;
            ModSpecs.ModVersionElements = new List<Data.ModVersionElement>();
            for (int i = 0; i < Model.Versions.Length; i++)
            {
                Data.SemanticVersion NewModVersion = new Data.SemanticVersion(Model.Versions[i].FriendlyVersion);
                Data.SemanticVersion NewGameVersion = new Data.SemanticVersion(Model.Versions[i].GameVersion);

                Data.ModVersionElement NewModVersionElement = new Data.ModVersionElement();
                NewModVersionElement.ModVersion = NewModVersion;
                NewModVersionElement.GameVersion = NewGameVersion;

                ModSpecs.ModVersionElements.Add(NewModVersionElement);
            }
            
            /* Publicity */
            
            // Since we only support one author for now form the spacedock API we do it this way
            Data.Author SpacedockAuthor = new Data.Author();
            SpacedockAuthor.AuthorName = Model.Author;
            SpacedockAuthor.AuthorWebsite = $"https://spacedock.info/profile/{SpacedockAuthor.AuthorName}"; 

            Data.Publicity Publicity = new Data.Publicity();
            Publicity.ModAuthors = new List<Data.Author> { SpacedockAuthor };
            Publicity.ModWebsite = Model.Website;
            Publicity.ModDonation = Model.Donations;
            Publicity.License = Model.License;

            /* Statistics */

            Data.Statistics Statistics = new Data.Statistics();
            Statistics.Verified = false;
            Statistics.Downloads = Model.Downloads;
            Statistics.Upvotes = 0;
            Statistics.Downvotes = 0;

            /* Creating the struct */

            Data.ServicesModificationInformation ConvertedServicesModificationInformation = new Data.ServicesModificationInformation();
            ConvertedServicesModificationInformation.MetadataSpecs = MetadataSpecs;
            ConvertedServicesModificationInformation.FileSpecs = FileSpecs;
            ConvertedServicesModificationInformation.ModSpecs = ModSpecs;
            ConvertedServicesModificationInformation.Publicity = Publicity;
            ConvertedServicesModificationInformation.Statistics = Statistics;

            return ConvertedServicesModificationInformation;
        }

        /// <summary>
        /// Creates a spacedock api ulr from the input id. 
        /// </summary>
        /// <param name="Id"></param>
        /// <returns>The url</returns>
        public static string GetModificationApiUrlById(UInt64 Id) => $"https://spacedock.info/api/mod/{Id}";

        /// <summary>
        /// This function uses an HttpClient to send a GET request to the 
        /// GetModificationUrlById(<paramref name="Id"/>) to retireve information about a 
        /// specific modification.
        /// </summary>
        /// <param name="Id"></param>
        /// <returns>The json response in a string</returns>
        public static async Task<string> GetModificationInformationJsonString(UInt64 Id)
        {
            string ModificationUlr = GetModificationApiUrlById(Id);

            HttpClient ToucanHttpClient = new HttpClient();
            ToucanHttpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:93.0; mm:Toucan-Mod-Manager;) Gecko/20100101 Firefox/93.0");

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
        public static async Task<JObject> GetModificationInformationJson(UInt64 Id) => JObject.Parse(await GetModificationInformationJsonString(Id));

        /// <summary>
        /// JsonConvert.DeserializeObjects the json string retrieved from 
        /// GetModificationInformationJsonString(<paramref name="Id"/>)
        /// </summary>
        /// <param name="Id"></param>
        /// <returns>The converted struct</returns>
        public static async Task<SpacedockAPIModificationModel> GetModificationInformationSpacedockModel(UInt64 Id) 
            => JsonConvert.DeserializeObject<SpacedockAPIModificationModel>(await GetModificationInformationJsonString(Id));

        /// <summary>
        /// calls GetModificationInformationSpacedockModel(<paramref name="Id"/>) and then 
        /// returns the ConvertToServicesModifciationInformation().
        /// </summary>
        /// <param name="Id"></param>
        /// <returns>A Data.ServicesModificationInformation containing the information about the mod retrieved by the <paramref name="Id"/></returns>
        public static async Task<Data.ServicesModificationInformation> GetModificationInformationServicesModel(UInt64 Id)
            => ConvertToServicesModificationInformation(await GetModificationInformationSpacedockModel(Id));
    }
}
