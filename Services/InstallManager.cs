﻿using Avalonia.Controls;
using MessageBox.Avalonia.DTO;
using MessageBox.Avalonia.Enums;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using ToucanUI.Models.KSP2;
using ToucanUI.ViewModels;

namespace ToucanUI.Services
{
    public class InstallManager
    {
        ConfigurationManager config = new ConfigurationManager();
        SpacedockAPI api = new SpacedockAPI();

        private readonly int BepinexId = 3277;
        private readonly int UitkId = 3363;
        string PluginsLocation;
        string BepinExLocation;
        string KSProot;
        string ToucanFolder;

        public enum BepInExStatusEnum
        {
            Installed,
            NotInstalled,
            Error
        }

        public InstallManager(string gamePath)
        {
            try
            {
                if (!string.IsNullOrEmpty(gamePath))
                {
                    KSProot = Path.GetDirectoryName(gamePath);
                    ToucanFolder = Path.Combine(KSProot, "Toucan");
                    CreateToucanFolder(); // Call the method to create the Toucan folder
                    PluginsLocation = Path.Combine(KSProot, "BepInEx", "plugins");
                    BepinExLocation = Path.Combine(KSProot, "BepInEx");

                }
                else
                {
                    KSProot = null;
                    ToucanFolder = null;
                    PluginsLocation = null;
                    BepinExLocation = null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }



        private void CreateToucanFolder()
        {
            if (!string.IsNullOrEmpty(ToucanFolder) && !Directory.Exists(ToucanFolder))
            {
                Directory.CreateDirectory(ToucanFolder);
            }
        }


        // Reads in all mods that are stored in the offline json file
        public ObservableCollection<Mod> ReadInstalledMods()
        {
            if (string.IsNullOrEmpty(ToucanFolder))
            {
                return new ObservableCollection<Mod>();
            }
            string path = Path.Combine(ToucanFolder, "InstalledMods", "InstalledMods.json");

            if (File.Exists(path))
            {
                try
                {
                    string jsonString = File.ReadAllText(path);
                    var installedMods = JsonSerializer.Deserialize<List<Mod>>(jsonString, new JsonSerializerOptions { ReferenceHandler = ReferenceHandler.Preserve });
                    return new ObservableCollection<Mod>(installedMods);
                }
                catch (JsonException ex)
                {
                    Console.WriteLine($"[ERROR] JSON deserialization error: {ex.Message}");
                }
                catch (InvalidOperationException ex)
                {
                    Console.WriteLine($"[ERROR] Invalid operation error: {ex.Message}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] Unexpected error: {ex.Message}");
                }
            }

            return new ObservableCollection<Mod>();
        }


        // Writes mods to the offline json file
        private void WriteInstalledMods(ObservableCollection<Mod> installedMods)
        {
            try
            {
                string path = Path.Combine(ToucanFolder, "InstalledMods", "InstalledMods.json");


                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                };
                string json = JsonSerializer.Serialize(installedMods, options);
                File.WriteAllText(path, json);
            }

            catch (Exception e) 
            {
                Console.WriteLine(e.Message);
            }
            
        }


        public async void DownloadMod(ModViewModel mod, TaskCompletionSource<bool> tcs, CancellationToken cancellationToken)
        {
            string path = Path.Combine(ToucanFolder, "InstalledMods");
            string fileName = System.IO.Path.Combine(path, mod.ModObject.Name + ".zip");
            try
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                    Console.WriteLine($"[INFO] Creating InstalledMods directory in {path}");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"[ERROR] {e.Message}");
            }

            //if (!File.Exists(fileName))
            //{
            try
            {
                using (var client = new HttpClient())
                {
                    // Download the file asynchronously
                    using (HttpResponseMessage response = await client.GetAsync(new Uri(mod.SelectedVersionViewModel.VersionObject.DownloadPath), HttpCompletionOption.ResponseHeadersRead, cancellationToken))
                    using (Stream streamToReadFrom = await response.Content.ReadAsStreamAsync())
                    using (Stream streamToWriteTo = File.Open(fileName, FileMode.Create))
                    {
                        int bufferSize = 81920;
                        byte[] buffer = new byte[bufferSize];
                        int bytesRead;
                        long totalBytesRead = 0;
                        long contentLength = response.Content.Headers.ContentLength.GetValueOrDefault();

                        while ((bytesRead = await streamToReadFrom.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
                        {
                            cancellationToken.ThrowIfCancellationRequested();

                            await streamToWriteTo.WriteAsync(buffer, 0, bytesRead, cancellationToken);
                            totalBytesRead += bytesRead;
                            mod.Progress = (int)(((double)totalBytesRead / contentLength) * 100);
                        }
                    }

                    if (!cancellationToken.IsCancellationRequested)
                    {
                        // Set the Mod State to Installed and set the version to installed
                        mod.ModState = ModViewModel.ModStateEnum.Installed;
                        mod.SelectedVersionViewModel.VersionObject.IsInstalled = true;

                        WriteToManifest(fileName, mod);
                        MoveToPlugins(fileName, mod);

                        // Add the installed mod to the list and update JSON file
                        var installedMods = ReadInstalledMods();
                        installedMods.Add(mod.ModObject);
                        WriteInstalledMods(installedMods);

                        tcs.TrySetResult(true);
                    }
                    else
                    {
                        mod.ModState = ModViewModel.ModStateEnum.NotInstalled;

                        tcs.TrySetCanceled();
                    }
                }
            }
            catch (OperationCanceledException)
            {
                tcs.TrySetCanceled();
            }
            catch (Exception e)
            {
                mod.ModState = ModViewModel.ModStateEnum.NotInstalled;
                Console.WriteLine($"[ERROR] Download Failed! {e.Message}");
                tcs.TrySetException(e);
            }
        }

        private void MoveToPlugins(string fileName, ModViewModel mod)
        {
            try
            {
                // Check if the root folder in the zip is "BepInEx"
                bool isBepInExRoot = false;
                using (ZipArchive archive = ZipFile.OpenRead(fileName))
                {
                    ZipArchiveEntry firstEntry = archive.Entries[0];
                    // Split on /, //, \, and \\
                    string rootFolderName = Regex.Split(firstEntry.FullName, @"[/]{1,2}|[\\]{1,2}")[0];
                    if (rootFolderName.Equals("BepInEx", StringComparison.InvariantCultureIgnoreCase))
                    {
                        isBepInExRoot = true;
                    }
                }

                // Extract the contents to the appropriate folder based on the isBepInExRoot flag
                if (isBepInExRoot)
                {
                    ZipFile.ExtractToDirectory(fileName, KSProot, true);
                }
                else
                {
                    ZipFile.ExtractToDirectory(fileName, PluginsLocation, true);
                }
            }

            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

        }


        public bool DeleteMod(ModViewModel mod)
        {
            bool isDeleted = false;

            // Check if mod name is same as folder mod is stored in
            try
            {
                string modFolderPath = Path.Combine(PluginsLocation, mod.ModObject.Name);
                if (!modFolderPath.Equals(BepinExLocation, StringComparison.OrdinalIgnoreCase) && !modFolderPath.Equals(PluginsLocation, StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine($"[INFO] Deleting: {modFolderPath}");
                    Directory.Delete(modFolderPath, true);
                    // Remove the uninstalled mod from the list and update the JSON file
                    isDeleted = true;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"[ERROR] Failed to delete mod directory! {e.Message}");
            }

            // Use manifest to delete mod
            try
            {
                string[] lines = File.ReadAllLines(Path.Combine(ToucanFolder + ".\\Manifests", $"{mod.ModObject.Name}-Manifest.txt"));
                foreach (string file in lines)
                {
                    try
                    {
                        string filePath = Path.Combine(KSProot, file);
                        string parentFolder = Directory.GetParent(filePath).Name;

                        if (!parentFolder.Equals("BepInEx", StringComparison.OrdinalIgnoreCase) &&
                            !parentFolder.Equals("plugins", StringComparison.OrdinalIgnoreCase) &&
                            !parentFolder.Equals("patchers", StringComparison.OrdinalIgnoreCase) &&
                            !parentFolder.Equals("config", StringComparison.OrdinalIgnoreCase))
                        {


                            if (File.Exists(filePath))
                            {
                                Console.WriteLine($"[INFO] Deleting file: {filePath}");
                                File.Delete(filePath);
                            }
                            if (Directory.Exists(filePath))
                            {
                                Console.WriteLine($"[INFO] Deleting directory: {filePath}");
                                Directory.Delete(filePath, true);
                            }

                            isDeleted = true; // If this point is reached, consider the deletion successful even if some files fail to delete
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[ERROR] {file}:{ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to delete using manifest! {ex.Message}");
            }

            try
            {
                var installedMods = ReadInstalledMods();
                var installedMod = installedMods.FirstOrDefault(m => m.Id == mod.ModObject.Id);
                if (installedMod != null)
                {
                    installedMods.Remove(installedMod);
                    WriteInstalledMods(installedMods);
                    isDeleted = true;
                }
            }

            catch (Exception e)
            {
                Console.WriteLine($"[ERROR] {e.Message}");
            }

            return isDeleted;
        }



        private void WriteToManifest(string fileName, ModViewModel mod) //Manifests are used to keep track of files downloaded for each mod
        {
            try
            {
                if (!Directory.Exists(ToucanFolder + @".\Manifests"))
                {
                    Directory.CreateDirectory(ToucanFolder + @".\Manifests");
                }

                //Gets the contents of the zip. May be useful if a manifest is used to delete mods
                using (ZipArchive archive = ZipFile.OpenRead(fileName))
                {
                    using (StreamWriter outputFile = new StreamWriter(System.IO.Path.Combine(ToucanFolder + @".\Manifests", mod.ModObject.Name + "-Manifest.txt")))
                    {
                        foreach (ZipArchiveEntry entry in archive.Entries)

                        {
                            outputFile.WriteLine(entry.FullName.Replace('/', Path.DirectorySeparatorChar)); // This should work cross-platform

                        }
                    }

                }

            }
            catch (Exception ex)
            {

                Console.WriteLine($"[ERROR] Could not create manifest! {ex.Message}");

            }

        }

        public async Task<BepInExStatusEnum> CheckIfBepInEx()
        {
            try
            {
                if (Directory.Exists(BepinExLocation))
                {

                    if (!Directory.Exists(PluginsLocation))
                    {
                        Directory.CreateDirectory(PluginsLocation);
                    }

                    return BepInExStatusEnum.Installed;
                }
                else
                {

                    return BepInExStatusEnum.NotInstalled;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Could not check for BepInEx: {ex.Message}");
                return BepInExStatusEnum.Error;
            }
        }


        public async Task<bool> BepInExStatusBox(ModlistViewModel modlistViewModel)
        {
            var messageBoxStandardWindow = MessageBox.Avalonia.MessageBoxManager.GetMessageBoxStandardWindow(
                new MessageBoxStandardParams
                {
                    ContentHeader = "BepInEx Not Found!",
                    ContentMessage = "BepInEx is required for most mods to work (Requires UITK) \nWould you like to install both?",
                    Icon = MessageBox.Avalonia.Enums.Icon.Warning,
                    ButtonDefinitions = MessageBox.Avalonia.Enums.ButtonEnum.YesNo,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen


                });

            var result = await messageBoxStandardWindow.Show();

            if (result == ButtonResult.Yes)
            {
                await GetBepInEx(modlistViewModel);
                return true;
            }

            return false;
        }

        private async Task GetBepInEx(ModlistViewModel modlistViewModel)
        {
            try
            {
                // Get the modlist from the ModlistViewModel
                ObservableCollection<ModViewModel> modList = modlistViewModel.ModList;

                // Set the FetchState and FetchMessage
                modlistViewModel.FetchState = ModlistViewModel.FetchStateEnum.Fetching;
                modlistViewModel.FetchingMessage = "Installing BepInEx...";

                //Download bepinex files, unzip them, then remove the zip
                using (var client = new HttpClient())
                {
                    string bepinexZipPath = System.IO.Path.Combine(KSProot, "BepInEx.zip");
                    Console.WriteLine($"[INFO] Downloading & Installing BepInEx!");

                    // Create an instance of ModViewModel with the BepInExMod
                    var bepinexViewModel = new ModViewModel(await api.GetMod(BepinexId));

                    // Get the downloadPath for the LatestVersion
                    Uri downloadUri = new Uri(bepinexViewModel.GetLatestVersion().DownloadPath);
                    using (HttpResponseMessage response = await client.GetAsync(downloadUri, HttpCompletionOption.ResponseHeadersRead))
                    using (Stream streamToReadFrom = await response.Content.ReadAsStreamAsync())
                    using (Stream streamToWriteTo = File.Open(bepinexZipPath, FileMode.Create))
                    {
                        await streamToReadFrom.CopyToAsync(streamToWriteTo);
                    }

                    // Extract to directory
                    Console.WriteLine($"[INFO] Extracting files from {bepinexZipPath} to {KSProot}");
                    await Task.Run(() => ZipFile.ExtractToDirectory(bepinexZipPath, KSProot, overwriteFiles: true));

                    // Also need to Install UITK which is a dependency for BepInEx
                    modlistViewModel.FetchingMessage = "Installing UITK...";
                    ModViewModel UiTKMod = new ModViewModel(await api.GetMod(UitkId));

                    // Check if UiTK is already in the modList
                    var existingUiTK = modList.FirstOrDefault(m => m.ModObject.Id == UiTKMod.ModObject.Id);
                    if (existingUiTK != null)
                    {
                        modList.Remove(existingUiTK);
                    }

                    // Download and install UiTK
                    TaskCompletionSource<bool> UiTKtcs = new TaskCompletionSource<bool>();
                    DownloadMod(UiTKMod, UiTKtcs, CancellationToken.None);
                    modList.Add(UiTKMod);

                    // Delete zip
                    File.Delete(bepinexZipPath);

                    await UiTKtcs.Task; // Wait for the UiTK mod to be downloaded and installed
                    modlistViewModel.FetchState = ModlistViewModel.FetchStateEnum.Success;
                    modlistViewModel.FetchingMessage = "";
                    Console.WriteLine($"[INFO] Completed with no issues!");

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] {ex.Message}");
                modlistViewModel.FetchState = ModlistViewModel.FetchStateEnum.Failed;
                modlistViewModel.FetchingMessage = $"Error: {ex.Message}";
            }

        }

    }
}
