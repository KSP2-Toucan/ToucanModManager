using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToucanServices.Services.API.Models;
using ToucanServices.Services.API.Models.Spacedock;
using ToucanServices.Services.API.Types.Spacedock;
using ToucanServices.Services.Data;
using ToucanServices.Services.Data.Debugging;

namespace ToucanServices.Services.API
{
    public static class ServicesModification
    {
        public static async Task<ServiceResult<ServicesModificationModel>> Modification(string StringId, AvailableApis SelecetdApi)
        {
            ServiceResult<bool> IsModificationInputsValid = ValidateModificationInputs(StringId, SelecetdApi);
            if (!IsModificationInputsValid.IsSuccess) return ServiceDebugger.Error<ServicesModificationModel>("Modificationinputs are not valid. " + IsModificationInputsValid.Message);

            if (SelecetdApi == AvailableApis.Spacedock)
                return await GetFromSpacedock(ulong.Parse(StringId));
            else return await GetFromSpacedock(ulong.Parse(StringId)); // Line is not needed when Toucan rolls around
        }

        public static ServiceResult<bool> ValidateModificationInputs(string StringId, AvailableApis SelectedApi)
        {
            if (SelectedApi == AvailableApis.Toucan) 
                return ServiceDebugger.Error<bool>("Toucan as an API has not been implemented into our desktop application yet.");
            
            if (SelectedApi == AvailableApis.Spacedock)
            {
                ulong Id = 0;
                if (!ulong.TryParse(StringId, out Id)) 
                    return ServiceDebugger.Error<bool>("There is something making it impossible for the spacedock id to be converted to a ulong.");
            }

            return ServiceDebugger.Success<bool>(true);

        }   

        public static async Task<ServiceResult<ServicesModificationModel>> GetFromSpacedock(ulong Id)
        {
            SpacedockModificationModel SpacedockModificationModel = await SpacedockModification.GetModificationModel(Id);

            /* Metadata Specs*/

            MetadataSpecs MetadataSpecs = new MetadataSpecs();
            MetadataSpecs.MetadataVersion = -1;
            MetadataSpecs.MetadataType = "Spacedock";

            /* File Specs */

            FileSpecs FileSpecs = new FileSpecs();
            FileSpecs.InstallPath = "";

            /* Mod Specs */

            ModSpecs ModSpecs = new ModSpecs();
            ModSpecs.ModName = SpacedockModificationModel.Name;
            ModSpecs.ModId = SpacedockModificationModel.Id.ToString();
            ModSpecs.ModVersionElements = new List<ModVersionElement>();
            for (int i = 0; i < SpacedockModificationModel.Versions.Length; i++)
            {
                Data.Versioning.VersionCreateInfo NewModVersionCreateInfo = new Data.Versioning.VersionCreateInfo(Data.Versioning.VersioningType.Arbitrary);
                NewModVersionCreateInfo.ArbitraryVersion = SpacedockModificationModel.Versions[i].FriendlyVersion;
                NewModVersionCreateInfo.Date = DateTimeOffset.Parse(SpacedockModificationModel.Versions[i].Created);

                Data.Versioning.VersionCreateInfo NewGameVersionCreateInfo = Data.Versioning.Convert.FromString(SpacedockModificationModel.Versions[i].GameVersion);

                Data.Versioning.Version NewModVersion = new Data.Versioning.Version(NewModVersionCreateInfo);
                Data.Versioning.Version NewGameVersion = new Data.Versioning.Version(NewGameVersionCreateInfo);

                ModVersionElement NewModVersionElement = new ModVersionElement();
                NewModVersionElement.ModVersion = NewModVersion;
                NewModVersionElement.GameVersion = NewGameVersion;

                ModSpecs.ModVersionElements.Add(NewModVersionElement);
            }
            ModDescription NewModDescription = new ModDescription();
            NewModDescription.ShortDesc = SpacedockModificationModel.ShortDescription;
            NewModDescription.LongDesc = SpacedockModificationModel.ShortDescription;
            ModSpecs.ModDescription = NewModDescription;

            ModSpecs.Tags = new string[0];

            /* Publicity */

            // Since we only support one author for now form the spacedock API we do it this way
            Author SpacedockAuthor = new Author();
            SpacedockAuthor.AuthorName = SpacedockModificationModel.Author;
            SpacedockAuthor.AuthorWebsite = $"https://spacedock.info/profile/{SpacedockAuthor.AuthorName}";

            Publicity Publicity = new Publicity();
            Publicity.ModAuthors = new List<Author> { SpacedockAuthor };
            Publicity.ModWebsite = SpacedockModificationModel.Website;
            Publicity.ModDonation = SpacedockModificationModel.Donations;
            Publicity.License = SpacedockModificationModel.License;

            /* Statistics */

            Statistics Statistics = new Statistics();
            Statistics.Verified = false;
            Statistics.Downloads = SpacedockModificationModel.Downloads;
            Statistics.Upvotes = 0;
            Statistics.Downvotes = 0;

            /* Creating the struct */

            ServicesModificationModel ServicesModificationModel = new ServicesModificationModel();
            ServicesModificationModel.MetadataSpecs = MetadataSpecs;
            ServicesModificationModel.FileSpecs = FileSpecs;
            ServicesModificationModel.ModSpecs = ModSpecs;
            ServicesModificationModel.Publicity = Publicity;
            ServicesModificationModel.Statistics = Statistics;

            return ServiceDebugger.Success<ServicesModificationModel>(ServicesModificationModel);
        }
    }
}
