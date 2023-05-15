using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Newtonsoft.Json;
using ToucanServices.Services.Data;

namespace ToucanServices.Services.API.Models
{
    public struct ServicesBrowseSpecs
    {
        [JsonProperty("selected_api")]
        public AvailableApis SelectedApi { get; set; }

        [JsonProperty("browse_category")]
        public BrowseCategories BrowseCategory { get; set; }

        [JsonProperty("browse_page")]
        public uint BrowsePage { get; set; } // The current page

        [JsonProperty("sorting_direction")]
        public SortingDirection SortingDirection { get; set; }

        [JsonProperty("sort_by")]
        public SortBy SortBy { get; set; }

        [JsonProperty("browse_result_amount")]
        public ushort BrowseResultAmount { get; set; } // How many results per page


        [JsonProperty("browse_result_total_amount")]
        public UInt32 BrowseResultTotalAmount { get; set; } // How many results total
        
        [JsonProperty("browse_pages_total_amount")]
        public UInt32 BrowsePagesTotalAmount { get; set; } // How many pages total
    }

    public struct ServicesBrowserModSpecs
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

    public struct ServicesBrowserPublicity
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

    public struct ServicesBrowserStatistics
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

    public struct ServicesBrowserResultFileSpecs
    {
        [JsonProperty("install_path")]
        public string InstallPath { get; set; }
    }

    public struct ServicesBrowseResults
    {
        [JsonProperty("file_specs")]
        public ServicesBrowserResultFileSpecs FileSpecs { get; set; }

        [JsonProperty("mod_specs")]
        public ServicesBrowserModSpecs ModSpecs { get; set; }

        [JsonProperty("publicity")]
        public ServicesBrowserPublicity Publicity { get; set; }

        [JsonProperty("statistics")]
        public ServicesBrowserStatistics Statistics { get; set; }
    }

    public struct ServicesBrowseModel
    {
        [JsonProperty("browser_specs")]
        public ServicesBrowseSpecs ServicesBrowserSpecs { get; set; }

        [JsonProperty("browser_results")]
        public List<ServicesBrowseResults> ServicesBrowserResults { get; set; }
    }
}
