using ToucanServices.Data;
using ToucanServices.Data.Versioning;
using ToucanServices.SpacedockAPI;
using ToucanServices.SpacedockAPI.Models;

namespace ToucanServices
{
    public static class Program
    {
        public static async Task<int> Main(string[] Args)
        {
            //ServicesModificationInformation ServicesModificationInformation = await SpacedockAPIRetrieveModificationInformation.GetModificationInformationServicesModel(21);
            //ServicesBrowser ServicesBrowser = SpacedockAPIRetrieveBrowse.ConvertToServicesBrowse(await SpacedockAPIRetrieveBrowse.GetBrowseModel(BrowseTypes.TOP, 12, OrderBy.NAME, Order.DESC, 20));
            Console.WriteLine(SpacedockAPIRetrieveSearch.GetSeachApiUrl(SearchType.USER, "Ee"));

            return 0;
        }
    }
}