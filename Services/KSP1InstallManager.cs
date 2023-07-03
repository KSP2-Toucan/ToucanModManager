using System;
using System.Collections.Generic;
using Avalonia.Controls;
using MessageBox.Avalonia.DTO;
using MessageBox.Avalonia.Enums;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
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

    public class KSP1InstallManager
    {
        string KSPRoot;
        string GameDataFolder;
        string ToucanFolder;


        public KSP1InstallManager()
        {
            try
            {
                KSPRoot = @"D:\SteamLibrary\steamapps\common\Kerbal Space Program\"; //CHANGE THIS LATER
                if (!string.IsNullOrEmpty(KSPRoot))
                {
                    ToucanFolder = Path.Combine(KSPRoot, "Toucan");
                    GameDataFolder = Path.Combine(KSPRoot, "GameData");
                  
                }
                else
                {
           
                    ToucanFolder = null;
                    GameDataFolder = null;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        // Reads in all mods that are stored in the offline json file
        public ObservableCollection<Mod> ReadInstalledMods()
        {
            string path = ToucanFolder + @"\InstalledMods\InstalledMods.json";

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
                    Debug.WriteLine($"JSON deserialization error: {ex.Message}");
                }
                catch (InvalidOperationException ex)
                {
                    Debug.WriteLine($"Invalid operation error: {ex.Message}");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Unexpected error: {ex.Message}");
                }
            }

            return new ObservableCollection<Mod>();
        }


        // Writes mods to the offline json file
        private void WriteInstalledMods(ObservableCollection<Mod> installedMods)
        {
            string path = ToucanFolder + @"\InstalledMods\InstalledMods.json";
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
            };
            string json = JsonSerializer.Serialize(installedMods, options);
            File.WriteAllText(path, json);
        }

        public async void DownloadMod(ModViewModel mod, TaskCompletionSource<bool> tcs, CancellationToken cancellationToken)
        {
            string path = ToucanFolder + @"\InstalledMods";
            string fileName = System.IO.Path.Combine(path, mod.ModObject.Name + ".zip");
            try
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                    Debug.WriteLine("Creating Directory");
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("Process Failed");
            }

            //if (!File.Exists(fileName))
            //{
            try
            {
                using (var client = new HttpClient())
                {
                    // Download the file asynchronously
                    Debug.WriteLine(mod.GetLatestVersion().DownloadPath);
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
                        MoveToGameData(fileName, mod);

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
                Debug.WriteLine("Download Failed");
                Debug.WriteLine(e);
                tcs.TrySetException(e);
            }
        }

        //Similar to 'MoveToPluigins' for KSP2, except there's no BepInEx
        private void MoveToGameData(string fileName, ModViewModel mod)
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

            // Extract the contents, and remove the BepInEx folders if there are any (there shouldn't be, but just incase)
            if (isBepInExRoot)
            {
                ZipFile.ExtractToDirectory(fileName, GameDataFolder, true);
                var folders = Directory.GetDirectories(GameDataFolder + @"\BepInEx\plugins");
                var folderName = (new DirectoryInfo(folders[0]).Name);
                Directory.Move(GameDataFolder + @"\BepInEx\plugins\" + folderName, GameDataFolder + "\\" + folderName);
                Directory.Delete(GameDataFolder + @"\BepInEx", true);

            }
            else
            {
                ZipFile.ExtractToDirectory(fileName, GameDataFolder, true);
            }
        }

        public bool DeleteMod(ModViewModel mod)
        {
            bool isDeleted = false;

            // Check if mod name is same as folder mod is stored in
            try
            {
                string modFolderPath = Path.Combine(GameDataFolder, mod.ModObject.Name);
                if (!modFolderPath.Equals(GameDataFolder, StringComparison.OrdinalIgnoreCase))
                {
                    Debug.WriteLine($"Deleting: {modFolderPath}");
                    Directory.Delete(modFolderPath, true);
                    // Remove the uninstalled mod from the list and update the JSON file
                    isDeleted = true;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("Failed to delete mod directory");
            }

            // Use manifest to delete mod
            try
            {
                string[] lines = File.ReadAllLines(Path.Combine(ToucanFolder + ".\\Manifests", $"{mod.ModObject.Name}-Manifest.txt"));
                foreach (string file in lines)
                {
                    try
                    {
                        string filePath = Path.Combine(KSPRoot, file);
                        string parentFolder = Directory.GetParent(filePath).Name;

                        if (!parentFolder.Equals("BepInEx", StringComparison.OrdinalIgnoreCase) &&
                            !parentFolder.Equals("plugins", StringComparison.OrdinalIgnoreCase) &&
                            !parentFolder.Equals("patchers", StringComparison.OrdinalIgnoreCase) &&
                            !parentFolder.Equals("config", StringComparison.OrdinalIgnoreCase))
                        {


                            if (File.Exists(filePath))
                            {
                                Debug.WriteLine($"Deleting file: {filePath}");
                                File.Delete(filePath);
                            }
                            else if (Directory.Exists(filePath))
                            {
                                Debug.WriteLine($"Deleting directory: {filePath}");
                                Directory.Delete(filePath, true);
                            }

                            isDeleted = true; // If this point is reached, consider the deletion successful even if some files fail to delete
                        }
                    }
                    catch
                    {
                        Debug.WriteLine($"Error: {file}");
                    }
                }
            }
            catch
            {
                Debug.WriteLine("Failed to delete using manifest");
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
                Debug.WriteLine($"{e.Message}");
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
                            //Debug.WriteLine(entry.FullName);
                            // outputFile.WriteLine(entry.FullName.Replace('/', '\\')); //replace the / with \ for valid windows path. NOTE - this breaks linux systems
                            outputFile.WriteLine(entry.FullName.Replace('/', Path.DirectorySeparatorChar)); // This should work cross-platform

                        }
                    }

                }

            }
            catch (Exception e)
            {

                Debug.WriteLine("Error creating manifest");

            }

        }
    }
}
