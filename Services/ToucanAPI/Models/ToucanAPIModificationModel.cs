using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ToucanServices.ToucanAPI.Models
{
    /* Structs generate by ChatGPT but has some tweaking by Eekk2 */
    public struct MetadataSpecs
    {
        [JsonProperty("metadata_version")]
        public int MetadataVersion { get; set; }

        [JsonProperty("metadata_type")]
        public string MetadataType { get; set; }
    }

    public struct FileSpecs
    {
        [JsonProperty("install_path")]
        public string InstallPath { get; set; }
    }

    public struct ModVersion
    {
        [JsonProperty("version_parts")]
        public uint[] VersionParts;

        [JsonProperty("version_identifier")]
        public string VersionIdentifier { get; set; }

        [JsonProperty("build_metadata")]
        public string BuildMetadata { get; set; }
    }

    public struct ModVersionElement
    {
        [JsonProperty("mod_versions")]
        public ModVersion ModVersion { get; set; }
    }

    public struct ModDescription
    {
        [JsonProperty("short_desc")]
        public string ShortDesc { get; set; }

        [JsonProperty("long_desc")]
        public string LongDesc { get; set; }
    }

    public struct ModSpecs
    {
        [JsonProperty("mod_name")]
        public string ModName { get; set; }

        [JsonProperty("mod_version_elements")]
        public List<ModVersionElement> ModVersionElements { get; set; }

        [JsonProperty("mod_description")]
        public ModDescription ModDescription { get; set; }

        [JsonProperty("tags")]
        public string[] Tags { get; set; }
    }

    public struct Author
    {
        [JsonProperty("author_name")]
        public string AuthorName { get; set; }

        [JsonProperty("author_website")]
        public string AuthorWebsite { get; set; }
    }

    public struct Publicity
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

    public struct Statistics
    {
        [JsonProperty("verified")]
        public bool Verified { get; set; }

        [JsonProperty("downloads")]
        public uint Downloads { get; set; }

        [JsonProperty("upvotes")]
        public uint Upvotes { get; set; }

        [JsonProperty("downvotes")]
        public uint Downvotes { get; set; }
    }

    struct ToucanAPIModificationModel
    {
        [JsonProperty("metadata_specs")]
        public MetadataSpecs MetadataSpecs { get; set; }

        [JsonProperty("file_specs")]
        public FileSpecs FileSpecs { get; set; }

        [JsonProperty("mod_specs")]
        public ModSpecs ModSpecs { get; set; }

        [JsonProperty("publicity")]
        public Publicity Publicity { get; set; }

        [JsonProperty("statistics")]
        public Statistics Statistics { get; set; }
    }

}

