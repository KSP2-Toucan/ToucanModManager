using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using MessageBox.Avalonia.DTO;
using MessageBox.Avalonia.Enums;
using MessageBox.Avalonia.Models;
using ReactiveUI;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Reactive;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading.Tasks;
using ToucanUI.Services;
using ToucanUI.Themes;

namespace ToucanUI.ViewModels
{
    public class HeaderViewModel : ViewModelBase
    {
        // =====================
        // VIEW MODELS
        // =====================
        public MainWindowViewModel MainViewModel { get; }



        // =====================
        // VARIABLES
        // =====================
        private static readonly HttpClient httpClient = new HttpClient();


        // =====================
        // SERVICES
        // =====================
        private readonly KSP2Service _ksp2Service;
        private readonly ConfigurationManager _configManager;

        // =====================
        // COMMANDS
        // =====================

        // File
        public ReactiveCommand<Unit, Unit> LaunchCommand { get; }
        public ReactiveCommand<Unit, Unit> RefreshModlistCommand { get; }
        public ReactiveCommand<Unit, Unit> ExitCommand { get; }


        // Edit
        public ReactiveCommand<Unit, Unit> ScanKSP2InstallLocationsCommand { get; }
        public ReactiveCommand<Unit, Unit> SetGameInstallPathCommand { get; }
        public ReactiveCommand<Unit, Unit> OpenGamePathDirectoryCommand { get; }
        public ReactiveCommand<Unit, Unit> ClearConfigFileCommand { get; }
        public ReactiveCommand<Unit, Unit> ViewConfigFileCommand { get; }

        // View
        public ReactiveCommand<Theme, Unit> ChangeThemeCommand { get; }

        // Help
        public ReactiveCommand<Unit, Unit> ToucanUpdateCheckCommand { get; }

        public ReactiveCommand<Unit, Unit> ToucanDisableModsCommand { get; }


        // =====================
        // CONSTRUCTOR
        // =====================
        public HeaderViewModel(MainWindowViewModel mainViewModel)
        {
            MainViewModel = mainViewModel;

            // Services
            _ksp2Service = new KSP2Service();
            _configManager = new ConfigurationManager();

            UpdateTimePlayed(_configManager.GetTimePlayed());
            IsToucanUpdateAvailable();

            // Commands

            ToucanUpdateCheckCommand = ReactiveCommand.CreateFromTask(IsToucanUpdateAvailable);
            ToucanDisableModsCommand = ReactiveCommand.Create(DisableMods);
            ScanKSP2InstallLocationsCommand = ReactiveCommand.CreateFromTask(ScanKSP2InstallLocations);
            SetGameInstallPathCommand = ReactiveCommand.CreateFromTask(SetGameInstallPath);
            OpenGamePathDirectoryCommand = ReactiveCommand.Create(OpenGamePathFolderAsync);
            ClearConfigFileCommand = ReactiveCommand.Create(ClearConfigFile);
            ViewConfigFileCommand = ReactiveCommand.Create(ViewConfigFile);

            ChangeThemeCommand = ReactiveCommand.Create<Theme>(ChangeTheme);

            LaunchCommand = ReactiveCommand.Create(LaunchApplication);
            RefreshModlistCommand = ReactiveCommand.Create(RefreshModlist);
            ExitCommand = ReactiveCommand.Create(ExitApplication);
        }



        // =====================
        // METHODS
        // =====================

        // Change theme based on user selection
        private void ChangeTheme(Theme theme)
        {
            MainViewModel.CurrentTheme = theme;
            _configManager.SetTheme(theme.GetType().FullName);

        }

        // Get zip file for correct OS build
        private string GetOsSpecificZipSuffix()
        {
            string osZipSuffix;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                osZipSuffix = "_win_x64";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                osZipSuffix = "_linux_x64";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                osZipSuffix = "_osx_x64";
            }
            else
            {
                throw new NotSupportedException("Unsupported OS");
            }
            return osZipSuffix;
        }


        private bool _modsEnabled = false;

        public bool ModsEnabled
        {
            get => _modsEnabled;
            set => this.RaiseAndSetIfChanged(ref _modsEnabled, value);
        }

        public void DisableMods()
        {
            Debug.WriteLine("Mods Status:" + ModsEnabled);
            ModsEnabled = !ModsEnabled;
            MainViewModel.installer.DisableBepInEx(ModsEnabled);
        }

        // Open game path directory
        public void OpenGamePathFolderAsync()
        {
            try
            {
                string gamePath = _configManager.GetGamePath();
                if (!string.IsNullOrEmpty(gamePath))
                {
                    // Get the parent directory of the game path
                    var directoryInfo = Directory.GetParent(gamePath);
                    string parentDirectory = directoryInfo.FullName;

                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    {
                        Process.Start("explorer.exe", parentDirectory);
                    }
                    else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                    {
                        Process.Start("open", parentDirectory);
                    }
                    else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    {
                        Process.Start("xdg-open", parentDirectory);
                    }
                    else
                    {
                        // Unsupported OS
                        Trace.WriteLine("[ERROR] Unsupported OS when attempting to open GamePath directory!");
                    }
                }
            }

            catch (Exception ex)
            {
                Trace.WriteLine($"[ERROR] {ex.Message}");
            }
        }



        // Check if an update is available on Github for Toucan Mod Manager
        public async Task IsToucanUpdateAvailable()
        {
            //var latestReleaseUrl = $"{MainViewModel.FooterVM.ToucanWebsite}/releases/latest";
            var latestReleaseUrl = $"https://api.github.com/repos/KSP2-Toucan/ToucanModManager/releases/latest";
            try
            {
                httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0");
                var response = await httpClient.GetAsync(latestReleaseUrl);
                response.EnsureSuccessStatusCode();
                var responseContent = await response.Content.ReadAsStringAsync();
                var releaseJson = JsonDocument.Parse(responseContent);

                if (releaseJson.RootElement.TryGetProperty("tag_name", out JsonElement tagNameElement))
                    {
                        var latestReleaseTagName = tagNameElement.GetString();

                        // If the latest release tag name is not the same as the current version of Toucan, then an update is available
                        if (!string.Equals(latestReleaseTagName, MainViewModel.FooterVM.ToucanVersion, StringComparison.OrdinalIgnoreCase))
                        {
                            Trace.WriteLine($"[INFO] Update {latestReleaseTagName} available! (Currently on {MainViewModel.FooterVM.ToucanVersion}");
                            var messageBoxCustomWindow = MessageBox.Avalonia.MessageBoxManager
                                .GetMessageBoxCustomWindow(new MessageBoxCustomParams
                                {
                                    Icon = MessageBox.Avalonia.Enums.Icon.Info,
                                    ContentHeader = "An update for Toucan Mod Manager is available!",
                                    ContentMessage = $"Current version: {MainViewModel.FooterVM.ToucanVersion}\nNewest version: {latestReleaseTagName}",
                                    ButtonDefinitions = new[] {
                                            new ButtonDefinition { Name = "Auto Update", IsDefault = true},
                                            new ButtonDefinition { Name = "Manual Update"},
                                            new ButtonDefinition { Name = "Cancel", IsCancel = true}
                                    },
                                    WindowStartupLocation = WindowStartupLocation.CenterScreen
                                });

                            var result = await messageBoxCustomWindow.Show();
                            if (result == "Manual Update")
                            {
                                Process.Start(new ProcessStartInfo
                                {
                                    FileName = "https://github.com/KSP2-Toucan/ToucanModManager/releases/latest",
                                    UseShellExecute = true
                                });
                            }

                            else if (result == "Auto Update")
                            {

                                string browserDownloadUrl = null;
                                string osZipSuffix = GetOsSpecificZipSuffix();

                                if (releaseJson.RootElement.TryGetProperty("assets", out JsonElement assetsElement) && assetsElement.ValueKind == JsonValueKind.Array)
                                {
                                    foreach (JsonElement assetElement in assetsElement.EnumerateArray())
                                    {
                                        if (assetElement.TryGetProperty("name", out JsonElement nameElement) && nameElement.GetString().EndsWith(osZipSuffix + ".zip"))
                                        {
                                            if (assetElement.TryGetProperty("browser_download_url", out JsonElement downloadUrlElement))
                                            {
                                                browserDownloadUrl = downloadUrlElement.GetString();
                                                Trace.WriteLine($"[INFO] Downloading update from: {browserDownloadUrl}");
                                                break;
                                            }
                                        }
                                    }
                                }

                                if (browserDownloadUrl != null)
                                {
                                    // Pass the browserDownloadUrl to the UpdateToucan function
                                    UpdateToucan(browserDownloadUrl);
                                }
                                else
                                {
                                    Trace.WriteLine("[WARNING] Zip asset not found or browser_download_url not found");
                                }
                            }
                        }

                    // Else the version is up to date (Disabled this as it was annoying getting a popup everytime the program runs)
                    //else
                    //{

                    //    var messageBoxStandardWindow = MessageBox.Avalonia.MessageBoxManager
                    //      .GetMessageBoxStandardWindow(new MessageBoxStandardParams
                    //      {
                    //          Icon = MessageBox.Avalonia.Enums.Icon.Success,
                    //          ContentHeader = "Toucan Mod Manager is up to date!",
                    //          ContentMessage = $"{MainViewModel.FooterVM.ToucanVersion} is the latest version.",

                    //      });

                    //    await messageBoxStandardWindow.Show();
                    //}

                }

                else
                    {
                        Trace.WriteLine("[ERROR] tag_name property not found in the JSON response");
                    }
            }

            catch (HttpRequestException ex)
            {
                // Error connecting to GitHub or retrieving the release information
                Trace.WriteLine($"[ERROR] Something went wrong retrieving Toucan Update: {ex.Message}");
            }

        }

        private void UpdateToucan(string downloadUrl)
        {
            try
            {
                // Save the download URL for the updater script
                string updaterDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Updater");
                string updaterConfigPath = Path.Combine(updaterDir, "updater_config.txt");
                File.WriteAllText(updaterConfigPath, downloadUrl);

                // Start the updater script based on the current OS
                string updaterScript = null;
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    updaterScript = "updater_windows.bat";
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    updaterScript = "updater_linux.sh";
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    updaterScript = "updater_macos.sh";
                }

                if (updaterScript != null)
                {
                    string scriptPath = Path.Combine(updaterDir, updaterScript);

                    if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                    {
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = "bash",
                            Arguments = $"-c \"{scriptPath}\"",
                            WorkingDirectory = updaterDir,
                            UseShellExecute = false,
                            RedirectStandardOutput = true,
                            RedirectStandardError = true
                        });

                    }

                    else
                    {
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = scriptPath,
                            WorkingDirectory = updaterDir,
                            UseShellExecute = true,
                        });
                    }

                }
                else
                {
                    Trace.WriteLine("[ERROR] Unsupported OS for self updater!");
                }

                // Close the Toucan application
                if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime)
                {
                    desktopLifetime.Shutdown();
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"[ERROR]: {ex}");
            }
        }



        // Method to update the time played values
        private void UpdateTimePlayed(int timePlayed)
        {

            // Calculate hours, minutes, and seconds from the stored seconds
            int totalSeconds = timePlayed;
            int hours = totalSeconds / 3600;
            int minutes = (totalSeconds % 3600) / 60;
            int seconds = totalSeconds % 60;

            MainViewModel.FooterVM.Hours = hours.ToString();
            MainViewModel.FooterVM.Minutes = minutes.ToString();
            MainViewModel.FooterVM.Seconds = seconds.ToString();

        }



        // File

        // This is convoluted because of bypassing the PD launcher, but it works
        private async void LaunchApplication()
        {
            try
            {
                string gamePath = _configManager.GetGamePath();
                Trace.WriteLine($"[INFO] Launching Application: {gamePath}");
                DateTime launchTime = DateTime.Now;

                // Set up the process start info
                ProcessStartInfo startInfo = new ProcessStartInfo(gamePath)
                {
                    Arguments = "-popupwindow", // Add the "-popupwindow" argument
                    UseShellExecute = false
                };

                // Launch the application 
                Process process = new Process { StartInfo = startInfo };
                process.Start();

                // Disable most of the UI
                MainViewModel.ValidGameFound = false;

                await Task.Run(async () =>
                {
                    process.WaitForExit();
                });

                //await Task.Delay(3000);

                // Run the process on a separate thread asynchronously
                //await Task.Run(async () =>
                //{
                //    Process gameProcess = null;
                //    var startTime = DateTime.Now;

                //    // Keep trying to get the process until it's found or until 30 seconds have passed
                //    while (gameProcess == null && (DateTime.Now - startTime).TotalSeconds < 30)
                //    {
                //        // Try to get the process by the first name 
                //        Process[] gameProcesses = Process.GetProcessesByName("KSP2_x64");
                //        if (gameProcesses.Length > 0)
                //        {
                //            gameProcess = gameProcesses[0];
                //        }
                //        else
                //        {
                //            // Try to get the process by the second name
                //            gameProcesses = Process.GetProcessesByName("Kerbal Space Program 2");
                //            if (gameProcesses.Length > 0)
                //            {
                //                gameProcess = gameProcesses[0];
                //            }
                //        }

                //        // If the game process wasn't found, wait a bit before trying again
                //        if (gameProcess == null)
                //        {
                //            await Task.Delay(1000);
                //        }
                //    }

                //    // If the game process was found, wait for it to exit
                //    if (gameProcess != null)
                //    {
                //        gameProcess.WaitForExit();
                //    }
                //});




                // Calculate time played
                TimeSpan timePlayed = DateTime.Now - launchTime;
                int timePlayedSeconds = (int)timePlayed.TotalSeconds;

                // Store the "Time played" in the config
                _configManager.SetTimePlayed(timePlayedSeconds);

                // Update the UI
                UpdateTimePlayed(_configManager.GetTimePlayed());

                MainViewModel.ValidGameFound = true;
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"[ERROR] {ex}");
            }
        }



        private async void RefreshModlist()
        {
            MainViewModel.SelectedMod = null;
            MainViewModel.SidePanelVM.SidePanelVisible = false;
            MainViewModel.ControlPanelVM.FilterInstalled = false;
            MainViewModel.ControlPanelVM.FilterNotInstalled = false;
            MainViewModel.ControlPanelVM.FilterVersion = false;
            MainViewModel.ControlPanelVM.FilterUpdateAvailable = false;

            await MainViewModel.ModlistVM.FetchMods(MainViewModel.ModlistVM.Category);
        }

        private async void ExitApplication()
        {
            var messageBoxStandardWindow = MessageBox.Avalonia.MessageBoxManager.GetMessageBoxStandardWindow(
                new MessageBoxStandardParams
                {
                    ContentHeader = "Exit Application",
                    ContentMessage = "Are you sure you want to quit?",
                    Icon = MessageBox.Avalonia.Enums.Icon.Warning,
                    ButtonDefinitions = MessageBox.Avalonia.Enums.ButtonEnum.YesNo,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen
                });

            var result = await messageBoxStandardWindow.Show();

            if (result == ButtonResult.Yes)
            {
                if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime)
                {
                    desktopLifetime.Shutdown();
                }
            }
        }

        // Edit
        public async Task ScanKSP2InstallLocations()
        {
            (string path, string version) = _ksp2Service.DetectGameVersion();
            Trace.WriteLine($"[INFO] Game Version is {version} at {path}");

            if (!string.IsNullOrEmpty(version))
            {
                await ShowSuccessMessageBox(version, path);
            }
            else
            {
                await ShowErrorMessageBox();
            }

        }

        public async Task SetGameInstallPath()
        {
            string folderPath = await SetKSP2BrowseFolderAsync();
            if (!string.IsNullOrEmpty(folderPath))
            {
                // Search for the exe in that folder path
                (string path, string version) = _ksp2Service.DetectGameVersion(folderPath);
                Trace.WriteLine($"[INFO] Game Version is {version} at {path}");

                if (!string.IsNullOrEmpty(version))
                {
                    await ShowSuccessMessageBox(version, path);
                }
                else
                {
                    await ShowErrorMessageBox();
                }
            }
        }

        public async Task<string> SetKSP2BrowseFolderAsync()
        {
            var openFolderDialog = new OpenFolderDialog
            {
                Title = "Select the 'Kerbal Space Program 2' folder",
            };

            string resultPath = string.Empty;

            if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime)
            {
                resultPath = await openFolderDialog.ShowAsync(desktopLifetime.MainWindow);
            }

            return resultPath;
        }

        private async void ClearConfigFile()
        {
            // A dialog to ask if the user is sure they want to delete
            Trace.WriteLine("[INFO] Clearing config file...");

            await ShowConfirmClearConfigMessageBox();

        }

        private async void ViewConfigFile()
        {
            await ShowViewConfigFileMessageBox();
        }

        // View


        // Help


        // =====================
        // DIALOG BOXES
        // =====================

        // Dialog to confirm the user wants to clear the config file
        private async Task ShowConfirmClearConfigMessageBox()
        {
            var messageBoxStandardWindow = MessageBox.Avalonia.MessageBoxManager.GetMessageBoxStandardWindow(
                new MessageBoxStandardParams
                {
                    ContentHeader = "Clear/Reset Configuration File",
                    ContentMessage = "Are you sure you want to reset your config file?",
                    Icon = MessageBox.Avalonia.Enums.Icon.Warning,
                    ButtonDefinitions = MessageBox.Avalonia.Enums.ButtonEnum.YesNo,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen
                });

            var result = await messageBoxStandardWindow.Show();

            if (result == ButtonResult.Yes)
            {
                _configManager.ClearConfig();
                UpdateTimePlayed(0);

                // Set the ValidGameFound to false
                MainViewModel.ValidGameFound = false;
            }
        }

        // Dialog to show the user the game version and path
        private async Task ShowViewConfigFileMessageBox()
        {
            var configManager = new ConfigurationManager();
            string contentMessage;
            string gamePath = configManager.GetGamePath();
            string gameVersion = configManager.GetGameVersion();
            int timePlayed = configManager.GetTimePlayed();

            if (gamePath == "" || gameVersion == "")
            {
                contentMessage = "No configuration file found.";
            }

            else
            {
                contentMessage = $"**Game Path:** {gamePath}\r\n\r\n" + $"**Game Version:** {gameVersion}\r\n\r\n" + $"**Time Played:** {timePlayed}";
            }

            var messageBoxMarkdownWindow = MessageBox.Avalonia.MessageBoxManager.GetMessageBoxCustomWindow(
                new MessageBoxCustomParams
                {
                    ContentHeader = "Configuration File",
                    ContentMessage = contentMessage,
                    Icon = MessageBox.Avalonia.Enums.Icon.Info,
                    ButtonDefinitions = new[]
                    {
                new ButtonDefinition { Name = "Close", IsDefault = true }
                    },
                    Markdown = true,
                    ShowInCenter = true,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen
                });

            await messageBoxMarkdownWindow.Show();
        }

        // Dialog to show found game version and path
        private async Task ShowSuccessMessageBox(string version, string path)
        {
            var messageBoxMarkdownWindow = MessageBox.Avalonia.MessageBoxManager.GetMessageBoxCustomWindow(
                new MessageBoxCustomParams
                {
                    ContentHeader = "A valid game version was detected:",
                    ContentMessage = $"✅ **Version:** {version}\r\n\r\n" +
                                     $"✅ **Path:** {path}",
                    Icon = MessageBox.Avalonia.Enums.Icon.Success,
                    ButtonDefinitions = new[]
                    {
                        new ButtonDefinition { Name = "Confirm", IsDefault = true },
                        new ButtonDefinition { Name = "Cancel", IsCancel = true }
                    },
                    Markdown = true,
                    ShowInCenter = true,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen
                });

            var result = await messageBoxMarkdownWindow.Show();

            if (result == "Confirm")
            {
                // Set the Game Install Path in config
                Trace.WriteLine("[INFO] Saving config file!");
                _configManager.SaveConfig(path, version);

            }
        }

        // Dialog to show error message
        private async Task ShowErrorMessageBox()
        {
            var messageBoxStandardWindow = MessageBox.Avalonia.MessageBoxManager.GetMessageBoxStandardWindow(
                new MessageBoxStandardParams
                {
                    ContentHeader = "A valid game version was not detected!",
                    ContentMessage = "Please ensure you have selected the\r\nroot 'Kerbal Space Program 2' folder.\n\n",
                    Icon = MessageBox.Avalonia.Enums.Icon.Error,
                    ButtonDefinitions = MessageBox.Avalonia.Enums.ButtonEnum.Ok,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen
                });

            var result = await messageBoxStandardWindow.Show();
        }
    }
}
