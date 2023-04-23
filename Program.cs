using Avalonia;
using Avalonia.ReactiveUI;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using ToucanAPI.Data;
using ToucanAPI;
using System.Threading.Tasks;

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

            MIToucanCreateInfo SpacedockCreateInfo = new MIToucanCreateInfo();
            SpacedockCreateInfo.InstallType = InstallType.SPACEDOCK;
            SpacedockCreateInfo.ToucanModificationId = 0;

            MIToucan SpacedockMIToucan = new MIToucan(SpacedockCreateInfo);
            SpacedockMIToucan = await ToucanUtilities.PopulateMIToucan(SpacedockMIToucan);

            return 0;
        }

        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .LogToTrace()
                .UseReactiveUI();
    }
}
