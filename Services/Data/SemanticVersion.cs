using Newtonsoft.Json;

namespace ToucanServices.Data
{
    /// <summary>
    /// This struct exists to make versioning better and easier for the development process and the users. Without
    /// this we would have to endure a lot of suffering due to issues finding the latest version of a mod, or 
    /// comparing versions. This struct makes it easier.
    /// </summary>
    public struct SemanticVersion : IComparable<SemanticVersion>
    {
        /// <summary>
        /// An array of version numbers that make up the semantic version number, including major, minor, and patch numbers.
        /// The number of version parts can vary depending on the software, but typically there are three numbers.
        /// Some software may use four or more numbers, but this format may not follow the semantic versioning convention.
        /// 
        /// The version numbers are evaluated from left to right to determine the highest version. If two versions have the
        /// same version numbers, the one with no version identifier or build metadata is considered the highest version.
        /// </summary>
        [JsonProperty("version_parts")] public UInt32[] VersionParts;

        /// <summary>
        /// A string that represents the pre-release version identifier, which is added after the version numbers in a 
        /// semantic version number. For example, the string "alpha" or "beta" could be used as a version identifier to 
        /// indicate that the software is in a pre-release or testing phase. The version identifier is included in a full 
        /// semantic version number along with the version numbers and build metadata, with a hyphen (-) separating the 
        /// version identifier from the version numbers and a plus sign (+) separating the version numbers from the 
        /// build metadata.
        /// </summary>
        [JsonProperty("version_identifier")] public string VersionIdentifier;

        /// <summary>
        /// A string that represents build metadata for the software package. Build metadata provides additional information
        /// about the build process or environment, such as the date of the build or the commit hash in the source control system.
        /// The build metadata is included in a full semantic version number along with the version numbers and version
        /// identifier, with a plus sign (+) separating the version numbers from the build metadata.
        /// </summary>
        [JsonProperty("build_metadata")] public string BuildMetadata;

        /// <summary>
        /// Default constructor for the SemanticVersion struct. VersionParts gets initialized with three parts, each valued 
        /// at 0. VersionIentifier will be empty together with the BuildMetadata.
        /// </summary>
        public SemanticVersion()
        {
            this.VersionParts = new UInt32[3];
            for (int i = 0; i < this.VersionParts.Length; i++)
                this.VersionParts[i] = 0;

            this.VersionIdentifier = "";

            this.BuildMetadata = "";
        }

        public SemanticVersion(UInt32 Major)
        {
            this.VersionParts = new UInt32[1];
            this.VersionParts[0] = Major;

            this.VersionIdentifier = "";

            this.BuildMetadata = "";
        }

        public SemanticVersion(UInt32 Major, UInt32 Minor)
        {
            this.VersionParts = new UInt32[2];
            this.VersionParts[0] = Major;
            this.VersionParts[1] = Minor;

            this.VersionIdentifier = "";

            this.BuildMetadata = "";
        }

        public SemanticVersion(UInt32 Major, UInt32 Minor, UInt32 Patch)
        {
            this.VersionParts = new UInt32[3];
            this.VersionParts[0] = Major;
            this.VersionParts[1] = Minor;
            this.VersionParts[2] = Patch;

            this.VersionIdentifier = "";

            this.BuildMetadata = "";
        }

        public SemanticVersion(UInt32 Major, UInt32 Minor, UInt32 Patch, string VersionIdentifier)
        {
            this.VersionParts = new UInt32[3];
            this.VersionParts[0] = Major;
            this.VersionParts[1] = Minor;
            this.VersionParts[2] = Patch;

            this.VersionIdentifier = VersionIdentifier;

            this.BuildMetadata = "";
        }

        public SemanticVersion(UInt32 Major, UInt32 Minor, UInt32 Patch, string VersionIdentifier, string BuildMetadata)
        {
            this.VersionParts = new UInt32[3];
            this.VersionParts[0] = Major;
            this.VersionParts[1] = Minor;
            this.VersionParts[2] = Patch;

            this.VersionIdentifier = VersionIdentifier;

            this.BuildMetadata = BuildMetadata;
        }

        public SemanticVersion(UInt32[] VersionParts)
        {
            this.VersionParts = VersionParts;

            this.VersionIdentifier = "";

            this.BuildMetadata = "";
        }

        public SemanticVersion(UInt32[] VersionParts, string VersionIdentifier)
        {
            this.VersionParts = VersionParts;

            this.VersionIdentifier = VersionIdentifier;

            this.BuildMetadata = "";
        }

        public SemanticVersion(UInt32[] VersionParts, string VersionIdentifier, string BuildMetadata)
        {
            this.VersionParts = VersionParts;

            this.VersionIdentifier = VersionIdentifier;

            this.BuildMetadata = BuildMetadata;
        }

        /// <summary>
        /// Constructor for the SemanticVersion struct which takes in a semantic version which is a string and automatically 
        /// converts it to the required values.
        /// </summary>
        /// <param name="StringedVersion"></param>
        public SemanticVersion(string StringedVersion)
        {
            // Version Parts
            string[] VersionPartsString = StringedVersion.Split(".");
            this.VersionParts = new UInt32[VersionPartsString.Length];

            for (int i = 0; i < VersionPartsString.Length; i++)
            {
                if (i == (VersionPartsString.Length - 1))
                {
                    VersionPartsString[i] = VersionPartsString[i].Split("-")[0];  // Removes the pre release version identifier from the last version part
                    VersionPartsString[i] = VersionPartsString[i].Split("+")[0];  // Removes the build metadata from the last version part.
                }

                this.VersionParts[i] = UInt32.Parse(VersionPartsString[i]);
            }

            // Version Identifier
            List<string> VersionIdentifierString = StringedVersion.Split("-").ToList();
            VersionIdentifierString.RemoveAt(0);
            VersionIdentifier = string.Join("-", VersionIdentifierString).Split("+")[0];

            // Build Metadata
            List<string> BuildMetadataString = StringedVersion.Split("+").ToList();
            BuildMetadataString.RemoveAt(0);
            BuildMetadata = string.Join("+", BuildMetadataString);
        }


        /// <summary>
        /// This ToString override makes showing the version much easier. 'With just a click of a button'
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string VersionString = string.Join(".", VersionParts);
            if (!string.IsNullOrEmpty(VersionIdentifier)) VersionString += $"-{VersionIdentifier}";

            if (!string.IsNullOrEmpty(BuildMetadata)) VersionString += $"+{BuildMetadata}";

            return VersionString;
        }

        public int CompareTo(SemanticVersion other)
        {
            for (int i = 0; i < this.VersionParts.Length; i++)
            {
                int CompareValue = (int)(this.VersionParts[i] - other.VersionParts[i]); // Create a comparison value if this.VP[i] is 1 and other.VP[i] is 2 will the comparison result in -1
                if (CompareValue == 0) { continue; } // The version parts are the same so we move on to the next version number
                else { return CompareValue; } // Since the compare value already tells us which one is larger (negative if other is larger and positive if this is larger, and by larger i mean higher verison)
            }
            // All of the version numbers are alike so we check the length of the version identifier. The one with the shortest identifier wins.
            return this.VersionIdentifier.Length - other.VersionIdentifier.Length;
        }
    }

    public static class SemanticVersionUtilities
    {
        public static SemanticVersion[] SortSemanticVersionList(SemanticVersion[] UnsortedSemanticVersions)
        {
            Array.Sort(UnsortedSemanticVersions);
            return UnsortedSemanticVersions;
        }
    }
}
