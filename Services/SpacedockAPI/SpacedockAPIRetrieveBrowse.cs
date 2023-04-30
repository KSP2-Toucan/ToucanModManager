using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ToucanServices.Data;
using ToucanServices.SpacedockAPI.Models;

namespace ToucanServices.SpacedockAPI
{
    /// <summary>
    /// Browse setting by the name of browse type which determines which type to browse from.
    /// </summary>
    public enum BrowseTypes
    {
        DEFAULT,
        TOP,
        FEATURED,
        NEW
    }

    /// <summary>
    /// Browse setting by the name orderby. Which thing to order or sort the list by.
    /// </summary>
    public enum OrderBy
    {
        NAME,
        UPDATED,
        CREATED
    }

    /// <summary>
    /// Browse setting by the name order. Sorts by either ascending or descending.
    /// </summary>
    public enum Order
    {
        ASC,
        DESC
    }

    public static class SpacedockAPIRetrieveBrowse
    {
        /// <summary>
        /// Takes in a model aquired by running 'GetBrowseModel' and converts it into the universial model and returns it.
        /// </summary>
        /// <param name="Model"></param>
        /// <returns></returns>
        public static ServicesBrowser ConvertToServicesBrowse(SpacedockAPIBrowseModel Model)
        {
            /* Browse Specs */

            ServicesBrowserSpecs BrowserSpecs = new ServicesBrowserSpecs();
            BrowserSpecs.Amount = Model.Total;
            BrowserSpecs.PageAmount = Model.Pages;
            BrowserSpecs.PerPageAmount = Model.Count;
            BrowserSpecs.PageIndex = Model.Page;

            /* Creating the struct */

            ServicesBrowser Browser = new ServicesBrowser();
            Browser.ServicesBrowserSpecs = BrowserSpecs;
            Browser.ServicesBrowserModificationInformation = new List<ServicesBrowserModificationInformation>();
            for (int i = 0; i < Model.Results.Length; i++)
            {
                /* File Specs */

                ServicesBrowserModificationInformationFileSpecs ModInfoFileSpecs = new ServicesBrowserModificationInformationFileSpecs();
                ModInfoFileSpecs.InstallPath = "";

                /* Mod Specs */
                ServicesBrowserModificationInformationModSpecs ModInfoModSpecs = new ServicesBrowserModificationInformationModSpecs();
                ModInfoModSpecs.ModName = Model.Results[i].Name;
                ModInfoModSpecs.ModId = Model.Results[i].Id.ToString();
                ModInfoModSpecs.ModVersionElements = new List<ModVersionElement>();
                for (int j = 0; j < Model.Results[i].Versions.Count; j++) // Oh no, another for loop
                {
                    ModVersionElement NewModVersionElement = new ModVersionElement();

                    Data.Versioning.VersionCreateInfo ModVersionCreateInfo = new Data.Versioning.VersionCreateInfo(Data.Versioning.VersioningType.Arbitrary); // Arbitrary is the eaisest, worst and defautl for this application
                    ModVersionCreateInfo.ArbitraryVersion = Model.Results[i].Versions[j].ModVersion;
                    ModVersionCreateInfo.Date = Model.Results[i].Versions[j].Created;

                    Data.Versioning.VersionCreateInfo GameVersionCreateInfo = Data.Versioning.Convert.FromString(Model.Results[i].Versions[j].GameVersion); // Game version will always* be semantic

                    NewModVersionElement.ModVersion = new Data.Versioning.Version(ModVersionCreateInfo); 
                    NewModVersionElement.GameVersion = new Data.Versioning.Version(GameVersionCreateInfo);
                    NewModVersionElement.DownloadUrl = Model.Results[i].Versions[j].DownloadUrl;
                    NewModVersionElement.Changelog = Model.Results[i].Versions[j].Changelog;
                    NewModVersionElement.Downloads = Model.Results[i].Versions[j].Downloads;

                    ModInfoModSpecs.ModVersionElements.Add(NewModVersionElement);
                }

                /* Publicity */
                ServicesBrowserModificationInformationPublicity ModInfoPublicity = new ServicesBrowserModificationInformationPublicity();
                
                Author NewAuthor = new Author();
                NewAuthor.AuthorName = Model.Results[i].Author;

                ModInfoPublicity.ModAuthors = new List<Author> { NewAuthor };
                ModInfoPublicity.ModWebsite = Model.Results[i].Website;
                ModInfoPublicity.ModDonation = Model.Results[i].Donations;
                ModInfoPublicity.License = Model.Results[i].License;



                /* Creates the sub struct */

                ServicesBrowserModificationInformation NewServicesBrowserModificationInformation = new ServicesBrowserModificationInformation();
                NewServicesBrowserModificationInformation.FileSpecs = ModInfoFileSpecs;
                NewServicesBrowserModificationInformation.ModSpecs = ModInfoModSpecs;
                NewServicesBrowserModificationInformation.Publicity = ModInfoPublicity;

                Browser.ServicesBrowserModificationInformation.Add(NewServicesBrowserModificationInformation);
            }

            return Browser;
        }

        /// <summary>
        /// Due to technical diffuculties relating to the spacedock.info/api/browse/ endpoint
        /// you can only specify the Order, OrderBy and Count when Type is equal to 
        /// BrowseTypes.Default. I do not know why this is, I do not know how this is and I do 
        /// not know what this is. And due to this restraint I am unsure if the code works as 
        /// it should, so if you encounter wierd and icky bugs with the url being wrong take 4
        /// another look here.
        /// </summary>
        /// <param name="Type"></param>
        /// <param name="Page"></param>
        /// <param name="OrderBy"></param>
        /// <param name="Order"></param>
        /// <param name="Count"></param>
        /// <returns>A string from which represents the ULR for a specific browse api endpoint with settings</returns>
        public static string GetBrowseApiUrl(BrowseTypes Type, UInt32 Page, OrderBy OrderBy, Order Order, UInt32 Count)
        {

            string Url = $"https://spacedock.info/api/browse";

            if (Type != BrowseTypes.DEFAULT) // Default Value
            {
                if (Type == BrowseTypes.TOP) { Url = Url + "/top"; }
                else if (Type == BrowseTypes.FEATURED) { Url = Url + "/featured"; }
                else if (Type == BrowseTypes.NEW) { Url = Url + "/new"; }
            }

            Url = Page != 0 || Page != 1 ? $"{Url}?page={Page}" : $"{Url}?page=1";
            
            if (OrderBy != OrderBy.CREATED) // Default Value
            {
                if (OrderBy == OrderBy.NAME) { Url = $"{Url}&orderby=name"; }
                else if (OrderBy == OrderBy.UPDATED) { Url = $"{Url}&orderby=updated"; }
            }

            Url = Order == Order.ASC ? Url : $"{Url}&order=desc";

            Url = Count != 0 ? $"{Url}&count={Page}" : $"{Url}&count=30";

            return Url;
        }

        /// <summary>
        /// Retrieves the Json contents in a string from a url generated by the input 
        /// variables using the GetBrowseApiUrl() function.
        /// </summary>
        /// <param name="Type"></param>
        /// <param name="Page"></param>
        /// <param name="OrderBy"></param>
        /// <param name="Order"></param>
        /// <param name="Count"></param>
        /// <returns></returns>
        public static async Task<string> GetBrowseJsonString(BrowseTypes Type, UInt32 Page, OrderBy OrderBy, Order Order, UInt32 Count)
        {
            string BrowseUrl = GetBrowseApiUrl(Type, Page, OrderBy, Order, Count);

            HttpClient ToucanHttpClient = new HttpClient();
            ToucanHttpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:93.0; mm:Toucan-Mod-Manager;) Gecko/20100101 Firefox/93.0");

            HttpResponseMessage ToucanHttpClientResponse = await ToucanHttpClient.GetAsync(BrowseUrl);
            ToucanHttpClientResponse.EnsureSuccessStatusCode();
            string ResponseContentJsonString = await ToucanHttpClientResponse.Content.ReadAsStringAsync();
            return ResponseContentJsonString;
        }

        /// <summary>
        /// Retrieves the response from GetBrowseJsonString() and JObject.Parse()s it to 
        /// a JObject before returning it.
        /// </summary>
        /// <param name="Type"></param>
        /// <param name="Page"></param>
        /// <param name="OrderBy"></param>
        /// <param name="Order"></param>
        /// <param name="Count"></param>
        /// <returns></returns>
        public static async Task<JObject> GetBrowseJson(BrowseTypes Type, UInt32 Page, OrderBy OrderBy, Order Order, UInt32 Count)
            => JObject.Parse(await GetBrowseJsonString(Type, Page, OrderBy, Order, Count));

        /// <summary>
        /// Retrieves the contents of the string response from the GetBrowseJsonString() 
        /// function and JsonConver.DeserializeObject<MODEL>()s before returning it.
        /// </summary>
        /// <param name="Type"></param>
        /// <param name="Page"></param>
        /// <param name="OrderBy"></param>
        /// <param name="Order"></param>
        /// <param name="Count"></param>
        /// <returns></returns>
        public static async Task<SpacedockAPIBrowseModel> GetBrowseModel(BrowseTypes Type, UInt32 Page, OrderBy OrderBy, Order Order, UInt32 Count) 
        {
            if (Type == BrowseTypes.DEFAULT) return JsonConvert.DeserializeObject<SpacedockAPIBrowseModel>(await GetBrowseJsonString(Type, Page, OrderBy, Order, Count));

            SpacedockAPIBrowseModel NewSpacedockAPIBrowseModel = new SpacedockAPIBrowseModel
            {
                Page = Page,
                Results = JsonConvert.DeserializeObject<Result[]>(await GetBrowseJsonString(Type, Page, OrderBy, Order, Count)) 
                            ?? throw new Exception("Results from attempt at recieving the json string using a type other than default is for some reason null.")
            };

            return NewSpacedockAPIBrowseModel;
        }
    }
}
