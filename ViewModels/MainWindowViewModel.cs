using Avalonia.Controls;
using Avalonia.Threading;
using MessageBox.Avalonia.DTO;
using MessageBox.Avalonia.Models;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ToucanUI.Models.KSP2;
using ToucanUI.Services;
using ToucanUI.Themes;
using static ToucanUI.Services.InstallManager;

namespace ToucanUI.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        // =====================
        // VIEW MODELS
        // =====================
        public HeaderViewModel HeaderVM { get; }
        public ControlPanelViewModel ControlPanelVM { get; }
        public ModlistViewModel ModlistVM { get; }
        public SidePanelViewModel SidePanelVM { get; }
        public FooterViewModel FooterVM { get; }



        // =====================
        // SERVICES
        // =====================
        public InstallManager installer;
        public ConfigurationManager configManager = new ConfigurationManager();
        private KSP2Service ksp2Service = new KSP2Service();


        // =====================
        // VARIABLES
        // =====================

        // Tracks the current theme
        private Theme _currentTheme;
        public Theme CurrentTheme
        {
            get => _currentTheme;
            set => this.RaiseAndSetIfChanged(ref _currentTheme, value);
        }

        // Keeps track of currently selected mod
        private ModViewModel _selectedMod;
        public ModViewModel SelectedMod
        {
            get => _selectedMod;
            set
            {
                // Deselect the previously selected mod in the side panel
                if (_selectedMod != null && _selectedMod != value)
                {
                    _selectedMod.IsSelectedSidePanel = false;
                }

                this.RaiseAndSetIfChanged(ref _selectedMod, value);

                // Check if the SelectedMod is null
                if (SelectedMod != null)
                {
                    SidePanelVM.SidePanelVisible = true; // Show the side panel
                    SelectedMod.IsSelectedSidePanel = true;

                    // If download sizes for versions haven't been fetched, retrieve them
                    if (!SelectedMod.VersionViewModels.All(vm => vm.IsDownloadSizeFetched))
                    {
                        Task.Run(() => SelectedMod.InitializeDownloadSizesAsync());
                    }
                }
                else
                {
                    SidePanelVM.SidePanelVisible = false; // Hide the side panel
                }
            }
        }



        // Tracks if BepInEx mod has been installed or not
        private InstallManager.BepInExStatusEnum _bepInExState;
        public InstallManager.BepInExStatusEnum BepInExState
        {
            get => _bepInExState;
            set => this.RaiseAndSetIfChanged(ref _bepInExState, value);
        }

        // Track selected mods for bulk actions (i.e Install/Update/Delete selected)
        public ObservableCollection<Mod> SelectedBulkMods { get; } = new ObservableCollection<Mod>();

        // Keeps track of whether a user is allowed to download mods or not
        private bool _validGameFound;
        public bool ValidGameFound
        {
            get => _validGameFound;
            set => this.RaiseAndSetIfChanged(ref _validGameFound, value);
        }


        // 
        // Commands
        // =====================



        // =====================
        // CONSTRUCTOR
        // =====================
        public MainWindowViewModel()
        {
            Trace.WriteLine($"\n[INIT] Initialising Toucan: {DateTime.Now}");
            // Get the theme from the config
            string themeFullName = configManager.GetTheme();

            // If the theme is not null, apply it
            if (themeFullName != null)
            {
                Type themeType = Type.GetType(themeFullName);
                if (themeType != null && typeof(Theme).IsAssignableFrom(themeType))
                {
                    Theme theme = (Theme)Activator.CreateInstance(themeType);
                    CurrentTheme = theme;
                }
            }
            else
            {
                // Set default theme if no theme is found in the config
                CurrentTheme = HotRodTheme.Instance;
            }

            FooterVM = new FooterViewModel(this);
            HeaderVM = new HeaderViewModel(this);
            ControlPanelVM = new ControlPanelViewModel(this);

            InitializeGameInstallPath();
            InitializeInstaller();
            CheckValidGameFound();

            ModlistVM = new ModlistViewModel(this);
            SidePanelVM = new SidePanelViewModel(this);
        }



        // =====================
        // METHODS
        // =====================
        public void InitializeInstaller()
        {
            string gamePath = configManager.GetGamePath();
            if (!string.IsNullOrEmpty(gamePath))
            {
                installer = new InstallManager(gamePath);
            }
        }

 
        // Run at startup to check if KSP2 and BepInEx installed
        private async void CheckValidGameFound()
        {

            // If a valid game path is found
            if (CheckGameInstallPath())
            {
                string gamePath = configManager.GetGamePath();
                installer = new InstallManager(gamePath);

                // Check if BepInEx is installed or not
                BepInExState = await Dispatcher.UIThread.InvokeAsync(async () => await installer.CheckIfBepInEx());

                switch (BepInExState)
                {
                    // If it's already installed, set ValidGameFound
                    case BepInExStatusEnum.Installed:
                        Trace.WriteLine("[INFO] BepInEx is installed");
                        ValidGameFound = true;
                        break;

                    // Else install it
                    case BepInExStatusEnum.NotInstalled:
                        Trace.WriteLine("[WARNING] BepInEx is not installed");
                        bool bepInExInstalled = await installer.BepInExStatusBox(ModlistVM);
                        if (bepInExInstalled)
                        {
                            BepInExState = BepInExStatusEnum.Installed;
                            InitializeInstaller();
                            ValidGameFound = true;
                        }
                        else
                        {
                            ValidGameFound = false;
                        }
                        break;

                    case BepInExStatusEnum.Error:
                        Trace.WriteLine("[ERROR] Error checking BepInEx");
                        ValidGameFound = false;
                        break;

                    default:
                        Trace.WriteLine("[ERROR] Unexpected BepInEx status");
                        ValidGameFound = false;
                        break;
                }
            }

            else
            {
               await ShowNoGameFoundMessageBox();
            }

            // Fetch the modlist
            ModlistVM.FetchMods(SpacedockAPI.Category.All);

        }

        private void InitializeGameInstallPath()
        {
            string gameExePath = configManager.GetGamePath();
            if (!string.IsNullOrEmpty(gameExePath) && File.Exists(gameExePath))
            {
                // Get the directory of the game executable
                string gameRootDirectory = Path.GetDirectoryName(gameExePath);

                // Use the KSP2Service to detect the game version and path
                (string detectedGameExePath, string gameVersion) = ksp2Service.DetectGameVersion(gameRootDirectory);

                if (!string.IsNullOrEmpty(detectedGameExePath))
                {
                    string configVersion = configManager.GetGameVersion();

                    // Compare the detected game version with the version stored in the config
                    if (configVersion != gameVersion)
                    {
                        // Update the config with the new version and path
                        configManager.SetGameVersion(gameVersion);
                        configManager.SetGamePath(detectedGameExePath);

                        Trace.WriteLine($"[INFO] Updated game version to {gameVersion}: {detectedGameExePath}");
                    }
                    else
                    {
                        Trace.WriteLine($"[INFO] Initialized {gameVersion}: {detectedGameExePath}");
                    }
                }
            }
        }



        // Check the game install path from config
        private bool CheckGameInstallPath()
        {
            string gamePath = configManager.GetGamePath();
            if (!string.IsNullOrEmpty(gamePath))
            {

                if (File.Exists(gamePath))
                {
                    Trace.WriteLine($"[INFO] Config GamePath found: {gamePath}");
                    return true;
                }
            }

            return false;
        }



        // Run on startup to show if a game install path was not found
        private async Task ShowNoGameFoundMessageBox()
        {
            // Set a time delay
            await Task.Delay(1000);
            var messageBoxCustomWindow = MessageBox.Avalonia.MessageBoxManager.GetMessageBoxCustomWindow(
                new MessageBoxCustomParams
                {
                    ContentHeader = $"A game install path was not found!",
                    ContentMessage = "Do you want to scan for the game install path automatically or search manually?\r\n" +
                                     "(You will be unable to download/install mods if this is not set)",
                    Icon = MessageBox.Avalonia.Enums.Icon.Info,
                    SizeToContent = SizeToContent.WidthAndHeight,
                    Topmost = true,
                    ButtonDefinitions = new[]
                    {
                        new ButtonDefinition { Name = "Scan Automatically", IsDefault = true },
                        new ButtonDefinition { Name = "Search Manually", IsCancel = true }
                    },
                    WindowStartupLocation = WindowStartupLocation.CenterScreen
                });

            var result = await messageBoxCustomWindow.Show();

            if (result == "Scan Automatically")
            {
                await HeaderVM.ScanKSP2InstallLocations();
            }
            else if (result == "Search Manually")
            {
                await HeaderVM.SetGameInstallPath();
            }

            CheckValidGameFound();
        }


    }
}