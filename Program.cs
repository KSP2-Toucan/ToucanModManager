using Avalonia;
using Avalonia.ReactiveUI;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Globalization;
using ToucanServices.Services.API.Models;
using ToucanServices.Services.Data;

namespace ToucanUI
{
    internal class Program
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool AllocConsole();

        [STAThread]
        public static async Task<int> Main(string[] args)
        {
            // Add this line to unhide the console window
            AllocConsole();

            BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args);

            /* Browse */

            ToucanServices.Services.Data.Debugging.ServiceResult<ServicesBrowseModel> ServicesBrowseModelServiceResult
                = await ToucanServices.Services.API.ServicesBrowse.Browse(BrowseCategories.TOP, 12, SortBy.NAME, SortingDirection.ASC, 20, AvailableApis.Spacedock); // Retrieval 

            if (!ServicesBrowseModelServiceResult.IsSuccess) Console.WriteLine(ServicesBrowseModelServiceResult.Message);  // Checks 

            /* Modification */

            ToucanServices.Services.Data.Debugging.ServiceResult<ServicesModificationModel> ServicesModificationModelServiceResult // Retrieval 
                = await ToucanServices.Services.API.ServicesModification.Modification("3384", AvailableApis.Spacedock);

            if (!ServicesModificationModelServiceResult.IsSuccess) Console.WriteLine(ServicesModificationModelServiceResult.Message); // Checks 

            return 0;
        }

        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .LogToTrace()
                .UseReactiveUI();
    }
}
