using Newtonsoft.Json;

namespace ToucanServices.Data
{
    public struct ServicesBrowserSpecs
    {
        [JsonProperty("amount")]
        public UInt32 Amount { get; set; }

        [JsonProperty("page_amount")]
        public UInt32 PageAmount { get; set; }

        [JsonProperty("per_page_amount")]
        public UInt32 PrePageAmount { get; set; }

        [JsonProperty("page_index")]
        public UInt32 PageIndex { get; set; }
    }

    public struct ServicesBrowserModificationInformationFileSpecs
    {
        [JsonProperty("install_path")]
        public string InstallPath { get; set; }
    }

    public struct ServicesBrowserModificationInformationModSpecs
    {
        [JsonProperty("mod_name")]
        public string ModName { get; set; }

        [JsonProperty("mod_id")]
        public string ModId { get; set; }

        [JsonProperty("mod_version_elements")]
        public List<ModVersionElement> ModVersionElements { get; set; }

        [JsonProperty("mod_description")]
        public ModDescription ModDescription { get; set; }

        [JsonProperty("tags")]
        public string[] Tags { get; set; }
    }

    public struct ServicesBrowserModificationInformationPublicity
    {
        [JsonProperty("mod_authors")]
        public List<Author> ModAuthors { get; set; }

        [JsonProperty("mod_website")]
        public string ModWebsite { get; set; }

        [JsonProperty("mod_donation")]
        public string ModDonation { get; set; }

        [JsonProperty("license")]
        public string License { get; set; }
    }

    public struct ServicesBrowserModificationInformationStatistics
    {
        [JsonProperty("verified")]
        public bool Verified { get; set; }

        [JsonProperty("downloads")]
        public ulong Downloads { get; set; }

        [JsonProperty("upvotes")]
        public uint Upvotes { get; set; }

        [JsonProperty("downvotes")]
        public uint Downvotes { get; set; }
    }

    public struct ServicesBrowserModificationInformation
    {
        [JsonProperty("file_specs")]
        public ServicesBrowserModificationInformationFileSpecs FileSpecs { get; set; }

        [JsonProperty("mod_specs")]
        public ServicesBrowserModificationInformationModSpecs ModSpecs { get; set; }

        [JsonProperty("publicity")]
        public ServicesBrowserModificationInformationPublicity Publicity { get; set; }

        [JsonProperty("statistics")]
        public ServicesBrowserModificationInformationStatistics Statistics { get; set; }
    }

    public struct ServicesBrowser
    {
        [JsonProperty("browser_specs")]
        public ServicesBrowserSpecs ServicesBrowserSpecs { get; set; }

        [JsonProperty("browser_modification_informations")]
        public List<ServicesBrowserModificationInformation> ServicesBrowserModificationInformation { get; set; }
    }
}
