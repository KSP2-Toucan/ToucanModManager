using System;
using System.Net;
using System.Threading.Tasks;

using ToucanAPI.Data;

namespace ToucanAPI
{
    namespace Data
    {
        /* General */

        public enum API
        {
            TOUCANAPI,
            GITHUB,
            SPACEDOCK,
        };

        
        public enum MIBrowserFilter // I swear mom these are not from reddit
        {
            TOP,
            HOT,
            NEW,
            BEST
        };

        public enum MIBrowserTime
        {
            ALLTIME,
            THISYEAR,
            THISMONTH,
            THISWEEK,
            TODAY
        };

        public struct SVersion
        {
            public UInt16 Major; // X.n.n
            public UInt16 Minor; // n.X.n
            public UInt16 Patch; // n.n.X

            public string Before;// <string>n.n.n
            public string After; // n.n.n<string>

            public SVersion(UInt16 _Major, UInt16 _Minor, UInt16 _Patch)
            {
                this.Major = _Major;
                this.Minor = _Minor;
                this.Patch = _Patch;

                this.Before = "";
                this.After = "";
            }

            public SVersion(UInt16 _Major, UInt16 _Minor, UInt16 _Patch, string _After)
            {
                this.Major = _Major;
                this.Minor = _Minor;
                this.Patch = _Patch;

                this.Before = "";
                this.After = _After;
            }

            public SVersion(string _Before, UInt16 _Major, UInt16 _Minor, UInt16 _Patch)
            {
                this.Major = _Major;
                this.Minor = _Minor;
                this.Patch = _Patch;

                this.Before = _Before;
                this.After = "";
            }

            public SVersion(string _Before, UInt16 _Major, UInt16 _Minor, UInt16 _Patch, string _After)
            {
                this.Major = _Major;
                this.Minor = _Minor;
                this.Patch = _Patch;

                this.Before = _Before;
                this.After = _After;
            }

            public override string ToString()
            {
                return $"{this.Major}.{this.Minor}.{this.Patch}";
            }
        }

        public struct GithubInformation
        {
            public string RepositoryOwner { get; set; }
            public string RepositoryName { get; set; }
            public string RepositoryTag { get; set; }

            public GithubInformation(string _RepositoryOwner, string _RepositoryName)
            {
                this.RepositoryOwner = _RepositoryOwner;
                this.RepositoryName = _RepositoryName;
                this.RepositoryTag = "";
            }

            public GithubInformation(string _RepositoryOwner, string _RepositoryName, string _RepositoryTag)
            {
                this.RepositoryOwner = _RepositoryOwner;
                this.RepositoryName = _RepositoryName;
                this.RepositoryTag = _RepositoryTag;
            }

            public string GetGithubUrl()
            {
                if (RepositoryTag.Length != 0) // It is an release
                    return $"https://github.com/{RepositoryOwner}/{RepositoryName}/releases/tag/{RepositoryTag}";
                else // Is is not
                    return $"https://github.com/{RepositoryOwner}/{RepositoryName}";
            }

            public string GetGihubApiUrl()
            {
                if (RepositoryTag.Length != 0) // It is an release
                    return $"https://api.github.com/repos/{RepositoryOwner}/{RepositoryName}/releases/tags/{RepositoryTag}";
                else // Is is not
                    return $"https://api.github.com/repos/{RepositoryOwner}/{RepositoryName}";
            }

            public override string ToString()
            {
                return GetGithubUrl();
            }
        }

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

        /* Other Modification Information structs */

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

        public struct MIToucanCreateInfo
        {
            public API API { get; set; } // Probably a better way of doing this

            /* ToucanAPI */

            public UInt64 ToucanModificationId { get; set; }

            /* Github */

            public GithubInformation GithubInformation { get; set; }

            /* Spacedock */

            public UInt64 SpacedockModificationId { get; set; }

        }

        public struct MIToucan
        {
            public MIToucanCreateInfo MIToucanCreateInfo { get; }

            public MIString ModificationName { get; set; }
            public MIString ModificationVersion { get; set; } // TODO : Replace with SVersion in the future
            public MIString ModificationDescriptionShort { get; set; }
            public MIString ModificationDescriptionLong { get; set; }
            public MIUInt64 ModificationSize { get; set; }
            public MIString ModificationLicense { get; set; } // TODO : Replace with MILicense in the future
            public MIAuthorA ModificationAuthors { get; set; }
            public MIString ModificatioGameName { get; set; }
            public MIString ModificationInstallationPath { get; set; }

            public MIToucan(MIToucanCreateInfo CreateInfo)
            {
                this.MIToucanCreateInfo = CreateInfo;

                this.ModificationName                       = new MIString(true);       // Required
                this.ModificationVersion                    = new MIString(true);       // Required
                this.ModificationDescriptionShort           = new MIString(false);
                this.ModificationDescriptionLong            = new MIString(false);
                this.ModificationSize                       = new MIUInt64(0, true);    // Required
                this.ModificationAuthors                    = new MIAuthorA(true);      // Required
                this.ModificationLicense                    = new MIString(true);       // Required
                this.ModificatioGameName                    = new MIString(true);       // Required
                this.ModificationInstallationPath           = new MIString(false);
            }

            public override string ToString()
            {
                return this.ModificationName.Value;
            }
        }

        public struct MIToucanBrowser
        {
            
        }

        public struct MIToucanInstallProgress
        {

        }
    }

    public static class ToucanUtilities
    {
        public static async Task<MIToucan> PopulateMIToucan(MIToucan _MIToucan)
        {
            if          (_MIToucan.MIToucanCreateInfo.API == API.TOUCANAPI)    _MIToucan = await ToucanAPI.TToucanAPI.PopulateMIToucan  (_MIToucan);
            else if     (_MIToucan.MIToucanCreateInfo.API == API.SPACEDOCK)    _MIToucan = await Spacedock.TSpacedock.PopulateMIToucan  (_MIToucan);
            else if     (_MIToucan.MIToucanCreateInfo.API == API.GITHUB)       _MIToucan = await Github.TGithub.PopulateMIToucan        (_MIToucan);

            return _MIToucan;
        }
    }

    class MIHandler
    {
        static async Task<int> Main(string[] args)
        {
            //Data.MIToucanCreateInfo CreateInfo = new Data.MIToucanCreateInfo();
            //CreateInfo.API = Data.API.GITHUB;
            //CreateInfo.GithubInformation = new GithubInformation("KSP2-Toucan", "Toucan-Mod", "0.0.2");

            //Data.MIToucan MIToucan = new Data.MIToucan(CreateInfo);
            //MIToucan = await ToucanUtilities.PopulateMIToucan(MIToucan);

            Data.MIToucanCreateInfo CreateInfo = new Data.MIToucanCreateInfo();
            CreateInfo.API = Data.API.SPACEDOCK;
            CreateInfo.SpacedockModificationId = 21;

            Data.MIToucan MIToucan = new Data.MIToucan(CreateInfo);
            MIToucan = await ToucanUtilities.PopulateMIToucan(MIToucan);

            return 0;
        }
    }
}