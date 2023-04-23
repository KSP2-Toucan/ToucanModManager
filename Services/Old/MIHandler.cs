using Newtonsoft.Json.Linq;
using System;
using System.Net;
using System.Threading.Tasks;

using ToucanAPI.Data;
using ToucanClient.Github;
using ToucanClient.Spacedock;
using ToucanClient.Toucan;
using ToucanUI.Services;

namespace ToucanAPI
{
    namespace Data
    {
        /* Enums */

        public enum InstallType
        {
            TOUCAN,
            GITHUB,
            SPACEDOCK,
        };

        /* Text */

        public struct MIString
        {
            public bool isRequired { get; }
            public string Value { get; set; }

            public MIString(string _Value)
            {
                this.Value = _Value;
                this.isRequired = false;
            }

            public MIString(bool _isRequired)
            {
                this.Value = "";
                this.isRequired = _isRequired;
            }

            public MIString(string _Value, bool _isRequired)
            {
                this.Value = _Value;
                this.isRequired = _isRequired;
            }

            public override string ToString()
            {
                return Value;
            }
        }

        public struct MIStringA
        {
            public bool isRequired { get; }
            public string[] Values { get; set; }

            public MIStringA(string[] _Values)
            {
                this.Values = _Values;
                this.isRequired = false;
            }

            public MIStringA(bool _isRequired)
            {
                this.Values = new string[0];
                this.isRequired = _isRequired;
            }

            public MIStringA(string[] _Values, bool _isRequired)
            {
                this.Values = _Values;
                this.isRequired = _isRequired;
            }

            public override string ToString()
            {
                string ValuesToString = "";
                foreach (string Value in this.Values)
                {
                    ValuesToString = $"{ValuesToString} {Value}";
                }

                return ValuesToString;
            }
        }

        /* Numbers */

        public struct MIUInt16
        {
            public bool isRequired { get; }
            public UInt16 Value { get; set; }

            public MIUInt16(UInt16 _Value)
            {
                this.Value = _Value;
                this.isRequired = false;
            }

            public MIUInt16(bool _isRequired)
            {
                this.Value = 0;
                this.isRequired = _isRequired;
            }

            public MIUInt16(UInt16 _Value, bool _isRequired)
            {
                this.Value = _Value;
                this.isRequired = _isRequired;
            }

            public override string ToString()
            {
                return Value.ToString();
            }
        }

        public struct MIUInt32
        {
            public bool isRequired { get; }
            public UInt32 Value { get; set; }

            public MIUInt32(UInt32 _Value)
            {
                this.Value = _Value;
                this.isRequired = false;
            }

            public MIUInt32(bool _isRequired)
            {
                this.Value = 0;
                this.isRequired = _isRequired;
            }

            public MIUInt32(UInt32 _Value, bool _isRequired)
            {
                this.Value = _Value;
                this.isRequired = _isRequired;
            }

            public override string ToString()
            {
                return Value.ToString();
            }
        }

        public struct MIUInt64
        {
            public bool isRequired { get; }
            public UInt64 Value { get; set; }

            public MIUInt64(UInt64 _Value)
            {
                this.Value = _Value;
                this.isRequired = false;
            }

            public MIUInt64(bool _isRequired)
            {
                this.Value = 0;
                this.isRequired = _isRequired;
            }

            public MIUInt64(UInt64 _Value, bool _isRequired)
            {
                this.Value = _Value;
                this.isRequired = _isRequired;
            }

            public override string ToString()
            {
                return Value.ToString();
            }
        }

        public struct MIFloat
        {
            public bool isRequired { get; }
            public float Value { get; set; }

            public MIFloat(float _Value)
            {
                this.Value = _Value;
                this.isRequired = false;
            }

            public MIFloat(bool _isRequired)
            {
                this.Value = 0;
                this.isRequired = _isRequired;
            }

            public MIFloat(float _Value, bool _isRequired)
            {
                this.Value = _Value;
                this.isRequired = _isRequired;
            }

            public override string ToString()
            {
                return Value.ToString();
            }
        }

        public struct MIDouble
        {
            public bool isRequired { get; }
            public double Value { get; set; }

            public MIDouble(double _Value)
            {
                this.Value = _Value;
                this.isRequired = false;
            }

            public MIDouble(bool _isRequired)
            {
                this.Value = 0;
                this.isRequired = _isRequired;
            }

            public MIDouble(double _Value, bool _isRequired)
            {
                this.Value = _Value;
                this.isRequired = _isRequired;
            }

            public override string ToString()
            {
                return Value.ToString();
            }
        }

        /* Modification Information structs */

        /// <summary>
        /// This struct contains all the information for installing from github. If InstallType is set to Github you will 
        /// need to set the values of MIGithubInstall too. The owner, name and tag of the repository is required.
        /// </summary>
        public struct MIGithubInstall
        {
            public string RepositoryOwner { get; }
            public string RepositoryName { get; }
            public string RepositoryTag { get; }

            public MIGithubInstall(string _RepositoryOwner, string _RepositoryName, string _RepositoryTag)
            {
                this.RepositoryOwner = _RepositoryOwner;
                this.RepositoryName = _RepositoryName;
                this.RepositoryTag = _RepositoryTag;
            }

            public string GetGithubUrl()
            {
                return $"https://github.com/{RepositoryOwner}/{RepositoryName}/releases/tag/{RepositoryTag}";
            }

            public string GetGihubApiUrl()
            {
                return $"https://api.github.com/repos/{RepositoryOwner}/{RepositoryName}/releases/tags/{RepositoryTag}";
            }

            public override string ToString()
            {
                return GetGithubUrl();
            }
        }

        public struct MISpacedockInstall
        {

        }

        public struct MIToucanInstall
        {

        }

        /// <summary>
        /// Modification Information in a semantic version format. This might need a rework soon though.
        /// </summary>
        public struct MISVersion
        {
            public UInt16 Major; // X.n.n
            public UInt16 Minor; // n.X.n
            public UInt16 Patch; // n.n.X

            public string Label; // n.n.n-<Label>
            public string Build; // n.n.n-<Label>+<Build>

            public string VersionOverride;

            public MISVersion(UInt16 _Major, UInt16 _Minor, UInt16 _Patch)
            {
                this.Major = _Major;
                this.Minor = _Minor;
                this.Patch = _Patch;

                this.Label = "";
                this.Build = "";

                this.VersionOverride = "";
            }

            public MISVersion(UInt16 _Major, UInt16 _Minor, UInt16 _Patch, string _Label)
            {
                this.Major = _Major;
                this.Minor = _Minor;
                this.Patch = _Patch;

                this.Label = _Label;
                this.Build = "";

                this.VersionOverride = "";
            }

            public MISVersion(UInt16 _Major, UInt16 _Minor, UInt16 _Patch, string _Label, string _Build)
            {
                this.Major = _Major;
                this.Minor = _Minor;
                this.Patch = _Patch;

                this.Label = _Label;
                this.Build = _Build;

                this.VersionOverride = "";
            }

            public MISVersion(string _VersionOverride)
            {
                this.Major = 0;
                this.Minor = 0;
                this.Patch = 0;

                this.Label = "";
                this.Build = "";

                this.VersionOverride = _VersionOverride;
            }

            public override string ToString()
            {
                if (!string.IsNullOrEmpty(VersionOverride)) return VersionOverride;
                else if (string.IsNullOrEmpty(Label) && string.IsNullOrEmpty(Build)) return $"{this.Major}.{this.Minor}.{this.Patch}";                                  // We dont have label or build
                else if (!string.IsNullOrEmpty(Label) && string.IsNullOrEmpty(Build)) return $"{this.Major}.{this.Minor}.{this.Patch}-{this.Label}";                    // We dont have build
                else if (string.IsNullOrEmpty(Label) && !string.IsNullOrEmpty(Build)) return $"{this.Major}.{this.Minor}.{this.Patch}+{this.Build}";                    // We dont have label
                else if (!string.IsNullOrEmpty(Label) && !string.IsNullOrEmpty(Build)) return $"{this.Major}.{this.Minor}.{this.Patch}-{this.Label}+{this.Build}";      // We have build and label
                else return (Label + " " + Build);
            }
        }

        public struct MIVersion
        {
            public MISVersion ModificationSemanticVersion { get; set; }
            public string? ModificationDownload { get; set; }
        }

        public struct MIVersionA
        {
            public MIVersion[] ModificationVersions { get; set; }
            public bool isRequired { get; }

            public MIVersionA(bool _isRequired)
            {
                this.isRequired = _isRequired;
                this.ModificationVersions = new MIVersion[0];
            }
        }

        /// <summary>
        /// This struct contains all the information about the author. The LinkedIn was a joke but I 
        /// cannot go back now.
        /// </summary>
        public struct MIAuthor
        {
            public string AuthorName { get; set; }
            public string AuthorContact { get; set; }
            public string AuthorIconUrl { get; set; }
            public string AuthorLinkedIn { get; set; }

            public MIAuthor(string _AuthorName)
            {
                this.AuthorName = _AuthorName;
                this.AuthorContact = "";
                this.AuthorIconUrl = "";
                this.AuthorLinkedIn = "";
            }

            public MIAuthor(string _AuthorName, string _AuthorContact)
            {
                this.AuthorName = _AuthorName;
                this.AuthorContact = _AuthorContact;
                this.AuthorIconUrl = "";
                this.AuthorLinkedIn = "";
            }

            public MIAuthor(string _AuthorName, string _AuthorContact, string _AuthorIconUrl)
            {
                this.AuthorName = _AuthorName;
                this.AuthorContact = _AuthorContact;
                this.AuthorIconUrl = _AuthorIconUrl;
                this.AuthorLinkedIn = "";
            }

            public MIAuthor(string _AuthorName, string _AuthorContact, string _AuthorIconUrl, string _AuthorLinkedIn)
            {
                this.AuthorName = _AuthorName;
                this.AuthorContact = _AuthorContact;
                this.AuthorIconUrl = _AuthorIconUrl;
                this.AuthorLinkedIn = _AuthorLinkedIn;
            }

            public override string ToString()
            {
                return AuthorName;
            }
        }

        /// <summary>
        /// This is a Modification Information Author Array. Contains if its required or not and the array 
        /// of authors. Makes it easier for me as a developer to work with authors, and makes it a 
        /// possibility for there to easily be multiple authors.
        /// </summary>
        public struct MIAuthorA
        {
            public MIAuthor[] Authors { get; set; }
            public bool isRequired { get; }

            public MIAuthorA(MIAuthor[] _Authors)
            {
                this.Authors = _Authors;
                this.isRequired = false;
            }

            public MIAuthorA(bool _isRequired)
            {
                this.Authors = new MIAuthor[0];
                this.isRequired = _isRequired;
            }

            public MIAuthorA(MIAuthor[] _Authors, bool _isRequired)
            {
                this.Authors = _Authors;
                this.isRequired = _isRequired;
            }

            public override string ToString()
            {
                string AuthorNamesToString = "";
                foreach (MIAuthor Author in this.Authors)
                {
                    AuthorNamesToString = $"{AuthorNamesToString} {Author}";
                }

                return AuthorNamesToString;
            }
        }

        /// <summary>
        /// This is a create info struct, and yes, this style of creating structs is taken right from 
        /// the khronos group's vulkan. This struct contains a InstallType for wether the information comes 
        /// from spacedock, toucan or github, and this is needed because there is different information we 
        /// need to reference based which website it is installed from.
        /// </summary>
        public struct MIToucanCreateInfo
        {
            public InstallType InstallType { get; set; } // Probably a better way of doing this

            /* ToucanAPI */

            public UInt64 ToucanModificationId { get; set; }

            /* Github */

            public MIGithubInstall GithubInstallInformation { get; set; }

            /* Spacedock */

            public UInt64 SpacedockModificationId { get; set; }

        }

        /// <summary>
        /// This is the struct which contains will contain all the data about a mod. This struct is 
        /// what you want to reference wach time you need information about a mod. Just pair it 
        /// with the create info which should point the initializer to the correct mod and you 
        /// will, after a little bit of time, have a bit of information about the specified mod.
        /// </summary>
        public struct MIToucan
        {
            /* Metadata Specifications */

            public UInt32 MetadataVersion { get; set; }

            /* File Specifications */

            public InstallType InstallType { get; set; }
            public MIString InstallPath { get; set; }
            public MIString InstallerPath { get; set; }
            public MIString SourceCode { get; set; }

            /* Github Install Information */
            public MIGithubInstall? GithubInstallInformation { get; set; }

            /* Toucan Install Information */
            public MIUInt64 ToucanModificationId { get; set; }

            /* Spacedock Install Information */
            public MIUInt64 SpacedockModificationId { get; set; }

            /* Modification Specifications */

            public MIString ModificationName { get; set; }
            public MIVersionA ModificationVersions { get; set; }
            public MIString ModificationDescriptionShort { get; set; }
            public MIString ModificationDescriptionLong { get; set; }
            public MIStringA ModificationTags { get; set; }

            /* Publicity */

            public MIAuthorA ModificationAuthors { get; set; }
            public MIString ModificationWebsite { get; set; }
            public MIString ModificationDonation { get; set; }
            public MIString ModificationLicense { get; set; }

            /* Statistics */

            /* Github */
            public MIUInt32 GithubStars { get; set; }

            /* Spacedock */
            public MIUInt32 SpacedockDownloads { get; set; }
            public MIUInt32 SpacedockFollowers { get; set; }

            /* Toucan */
            public MIUInt32 ToucanDownloads { get; set; }
            public MIUInt32 ToucanUpvotes { get; set; }
            public MIUInt32 ToucanDownvotes { get; set; }

            /// <summary>
            /// Constructor for the MIToucan struct. This is an important function so I would recommend you reading 
            /// through it. It sets all the information from the MIToucanCreateInfo to the required places and 
            /// then defines which variables that is required and which variables that is not.
            /// </summary>
            /// <param name="ToucanCreateInfo"></param>
            public MIToucan(MIToucanCreateInfo ToucanCreateInfo)
            {
                /* Create Info */

                InstallType = ToucanCreateInfo.InstallType;
                ToucanModificationId = new MIUInt64(ToucanCreateInfo.InstallType == InstallType.TOUCAN ? ToucanCreateInfo.ToucanModificationId : 0, true); // Sets to required, but if InstallType isnt set to toucan it will just be ignored.
                GithubInstallInformation = ToucanCreateInfo.InstallType == InstallType.GITHUB ? ToucanCreateInfo.GithubInstallInformation : null; // I can't really set this to required or not, but it will throw errors if some values is empty.
                SpacedockModificationId = new MIUInt64(ToucanCreateInfo.InstallType == InstallType.SPACEDOCK ? ToucanCreateInfo.SpacedockModificationId : 0, true); // Same with this one as it was with the ToucanModId

                /* Other */

                MetadataVersion = new UInt32();             // Doesnt really have a set required or not, but errors will be thrown if this value is set or not
                InstallPath = new MIString(false);      // Although you may think that this should be true, it just defaults to the BepInEx plugin if its empty or equal to 'DEFAULT'
                InstallerPath = new MIString(false);      // The installer is a python script that can do other things. The mod author has to create this himself. Can do things as create a new folder in the root directory to store models, like saberfactory for beatsaber.
                SourceCode = new MIString(false);      // Link to where you can take a look at the source code.
                ModificationName = new MIString(true);       // Required, self explainatory
                ModificationVersions = new MIVersionA(true);     // Required, but it only needs one version.
                ModificationDescriptionShort = new MIString(true);       // Required, the user needs to know a little about the mod.
                ModificationDescriptionLong = new MIString(false);      // Not required, the short description should be enough
                ModificationTags = new MIStringA(false);     // You dont NEED tags, but its really useful and can help with a kond of SEO
                ModificationAuthors = new MIAuthorA(true);      // Required, need at least one author
                ModificationWebsite = new MIString(false);
                ModificationDonation = new MIString(false);
                ModificationLicense = new MIString(true);       // Required, I am not a lawyer, but probaly some legal issues if I dont

                GithubStars = new MIUInt32(false);      // Since the search feature isnt up yet I think its a bad time to make this required
                SpacedockDownloads = new MIUInt32(false);      // ---------- | | ----------
                SpacedockFollowers = new MIUInt32(false);      // ---------- | | ----------
                ToucanDownloads = new MIUInt32(false);      // ---------- | | ----------
                ToucanUpvotes = new MIUInt32(false);      // ---------- | | ----------
                ToucanDownvotes = new MIUInt32(false);      // ---------- | | ----------
            }

            public override string ToString()
            {
                return ModificationName.Value;
            }
        }
    }

    /// <summary>
    /// This class will be merged with MIHandler soon, since it doesnt just work with Toucan.
    /// </summary>
    public static class ToucanUtilities
    {
        public static async Task<MIToucan> PopulateMIToucan(MIToucan _MIToucan)
        {
            if (_MIToucan.InstallType == InstallType.TOUCAN) _MIToucan = await MIToucanAPI.PopulateMIToucan(_MIToucan);
            else if (_MIToucan.InstallType == InstallType.SPACEDOCK) _MIToucan = await MISpacedockAPI.PopulateMIToucan(_MIToucan);
            else if (_MIToucan.InstallType == InstallType.GITHUB) _MIToucan = await MIGithubAPI.PopulateMIToucan(_MIToucan);

            return _MIToucan;
        }
    }

    class MIHandler
    {
        //public static async Task<int> Main(string[] Args)
        //{
        //    MIToucanCreateInfo SpacedockCreateInfo = new MIToucanCreateInfo();
        //    SpacedockCreateInfo.InstallType = InstallType.SPACEDOCK;
        //    SpacedockCreateInfo.SpacedockModificationId = 21;

        //    MIToucan SpacedockMIToucan = new MIToucan(SpacedockCreateInfo);
        //    SpacedockMIToucan = await ToucanUtilities.PopulateMIToucan(SpacedockMIToucan);

        //    Console.WriteLine(JObject.FromObject(SpacedockMIToucan).ToString());

        //    return 0;
        //}
    }
}