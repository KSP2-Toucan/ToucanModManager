using ToucanServices.Data;
using ToucanServices.SpacedockAPI;

namespace ToucanServices
{
    public static class Program
    {
        public static async Task<int> Main(string[] Args)
        {
            //ServicesModificationInformation servicesModificationInformation = await SpacedockAPIRetrieveModificationInformation.GetModificationInformationServicesModel(21);

            Console.WriteLine(SpacedockAPIRetrieveBrowser.GetBrowserApiUrl(BrowseTypes.TOP, 12, OrderBy.NAME, Order.ASC, 20));

            return 0;
        }
    }
}