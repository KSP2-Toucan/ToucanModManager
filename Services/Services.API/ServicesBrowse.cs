using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ToucanServices.Services.API.Models;
using ToucanServices.Services.API.Models.Spacedock;
using ToucanServices.Services.API.Types.Spacedock;
using ToucanServices.Services.Data;
using ToucanServices.Services.Data.Debugging;
using ToucanServices.Services.Data.Versioning;

namespace ToucanServices.Services.API
{
    public static class ServicesBrowse
    {
        public static async Task<ServiceResult<ServicesBrowseModel>> Browse
        (
            BrowseCategories BrowseCategory,
            UInt32 BrowsePage,
            SortBy SortBy,
            SortingDirection SortingDirection,
            UInt16 BrowseResultAmount, 
            AvailableApis SelectedApi
        )
        {
            ServiceResult<bool> IsBrowseInputsValid = ValidateBrowseInputs(BrowseCategory, BrowsePage, SortBy, SortingDirection, SelectedApi, BrowseResultAmount);
            if (!IsBrowseInputsValid.Result) return ServiceDebugger.Error<ServicesBrowseModel>("Browse inputs are not valid. " + IsBrowseInputsValid.Message);

            if (SelectedApi == AvailableApis.Spacedock)
                return await GetFromSpacedock(BrowseCategory, BrowsePage, SortBy, SortingDirection, BrowseResultAmount, SelectedApi);
            else return await GetFromSpacedock(BrowseCategory, BrowsePage, SortBy, SortingDirection, BrowseResultAmount, SelectedApi); // Line is not needed when Toucan rolls around
        }

        public static ServiceResult<bool> ValidateBrowseInputs
        (
            BrowseCategories BrowseCategory,
            UInt32 Page, 
            SortBy SortyBy, 
            SortingDirection 
            SortingDirection, 
            AvailableApis SelectedApi, 
            UInt16 BrowseResultAmount
        )
        {
            UInt16 MinimumBrowseResultAmount = 2;
            UInt16 MaximumBrowseResultAmount = 120;
            UInt32 MinimumBrowsePage = 0;

            if (Page < MinimumBrowsePage) return ServiceDebugger.Error<bool>($"Page has to be more than zero! (Page: {Page})");
            if (BrowseResultAmount < MinimumBrowseResultAmount) return ServiceDebugger.Error<bool>($"BrowseResultAmount has to be more than its minimum requirement! (Req: {MinimumBrowseResultAmount} Amount: {BrowseResultAmount}).");
            if (BrowseResultAmount > MaximumBrowseResultAmount) return ServiceDebugger.Error<bool>($"BrowseResultAmount has to be less than its maximum requirement! (Req: {MaximumBrowseResultAmount} Amount: {BrowseResultAmount}).");
            if (SelectedApi == AvailableApis.Toucan) return ServiceDebugger.Error<bool>($"Toucan as an API has not been implemented into our desktop application yet. ");

            return ServiceDebugger.Success<bool>(true);
        }

        public static async Task<ServiceResult<ServicesBrowseModel>> GetFromSpacedock
        (
            BrowseCategories BrowseCategory,
            UInt32 BrowsePage,
            SortBy SortBy,
            SortingDirection SortingDirection,
            UInt16 BrowseResultAmount,
            AvailableApis SelectedApi
        )
        {
            SpacedockBrowseModel SpacedockBrowseModel = await SpacedockBrowse.GetBrowseModel
                (
                    BrowseCategory,
                    BrowsePage,
                    SortBy,
                    SortingDirection,
                    BrowseResultAmount
                );

            ServicesBrowseSpecs ServicesBrowseSpecs = new ServicesBrowseSpecs
            {
                SelectedApi = SelectedApi,
                BrowseCategory = BrowseCategory,
                BrowsePage = BrowsePage,
                SortingDirection = SortingDirection,
                SortBy = SortBy,
                BrowseResultAmount = BrowseResultAmount,

                BrowseResultTotalAmount = SpacedockBrowseModel.Total,
                BrowsePagesTotalAmount = SpacedockBrowseModel.Pages,
            };

            ServicesBrowseModel ServicesBrowseModel = new ServicesBrowseModel();
            ServicesBrowseModel.ServicesBrowserSpecs = ServicesBrowseSpecs;
            ServicesBrowseModel.ServicesBrowserResults = new List<ServicesBrowseResults>();
            for (int i = 0; i < SpacedockBrowseModel.Results.Length; i++)
            {
                /* File Specs */

                ServicesBrowserResultFileSpecs ModInfoFileSpecs = new ServicesBrowserResultFileSpecs();
                ModInfoFileSpecs.InstallPath = "";

                /* Mod Specs */
                ServicesBrowserModSpecs ModInfoModSpecs = new ServicesBrowserModSpecs();
                ModInfoModSpecs.ModName = SpacedockBrowseModel.Results[i].Name;
                ModInfoModSpecs.ModId = SpacedockBrowseModel.Results[i].Id.ToString();
                ModInfoModSpecs.ModVersionElements = new List<ModVersionElement>();
                for (int j = 0; j < SpacedockBrowseModel.Results[i].Versions.Count; j++) // Oh no, another for loop
                {
                    ModVersionElement NewModVersionElement = new ModVersionElement();

                    VersionCreateInfo ModVersionCreateInfo = new Data.Versioning.VersionCreateInfo(VersioningType.Arbitrary); // Arbitrary is the eaisest, worst and defautl for this application
                    ModVersionCreateInfo.ArbitraryVersion = SpacedockBrowseModel.Results[i].Versions[j].ModVersion;
                    ModVersionCreateInfo.Date = SpacedockBrowseModel.Results[i].Versions[j].Created;

                    VersionCreateInfo GameVersionCreateInfo = Data.Versioning.Convert.FromString(SpacedockBrowseModel.Results[i].Versions[j].GameVersion); // Game version will always* be semantic

                    NewModVersionElement.ModVersion = new Data.Versioning.Version(ModVersionCreateInfo);
                    NewModVersionElement.GameVersion = new Data.Versioning.Version(GameVersionCreateInfo);
                    NewModVersionElement.DownloadUrl = SpacedockBrowseModel.Results[i].Versions[j].DownloadUrl;
                    NewModVersionElement.Changelog = SpacedockBrowseModel.Results[i].Versions[j].Changelog;
                    NewModVersionElement.Downloads = SpacedockBrowseModel.Results[i].Versions[j].Downloads;

                    ModInfoModSpecs.ModVersionElements.Add(NewModVersionElement);
                }

                /* Publicity */
                ServicesBrowserPublicity ModInfoPublicity = new ServicesBrowserPublicity();

                Author NewAuthor = new Author();
                NewAuthor.AuthorName = SpacedockBrowseModel.Results[i].Author;

                ModInfoPublicity.ModAuthors = new List<Author> { NewAuthor };
                ModInfoPublicity.ModWebsite = SpacedockBrowseModel.Results[i].Website;
                ModInfoPublicity.ModDonation = SpacedockBrowseModel.Results[i].Donations;
                ModInfoPublicity.License = SpacedockBrowseModel.Results[i].License;



                /* Creates the sub struct */

                ServicesBrowseResults NewServicesBrowserModificationInformation = new ServicesBrowseResults();
                NewServicesBrowserModificationInformation.FileSpecs = ModInfoFileSpecs;
                NewServicesBrowserModificationInformation.ModSpecs = ModInfoModSpecs;
                NewServicesBrowserModificationInformation.Publicity = ModInfoPublicity;

                ServicesBrowseModel.ServicesBrowserResults.Add(NewServicesBrowserModificationInformation);
            }

            return ServiceDebugger.Success(ServicesBrowseModel);
        }
        
    }


}