using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace ToucanUI.Services
{
    public class KSP2Service
    {
        // =====================
        // METHODS
        // =====================

        // Method to detect current game version and path for KSP2
        public (string path, string version) DetectGameVersion(string path = "")
        {
            List<string> searchDirectories = GenerateSearchDirectories(path);

            string ksp2Path = FindKSP2(searchDirectories);

            if (string.IsNullOrEmpty(ksp2Path))
            {
                Trace.WriteLine($"[ERROR] Could not find KSP2 executable!");
                return ("", "");
            }

            string version;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                version = GetFileVersionMacOS(ksp2Path);
            }

            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                version = GetFileVersionLinux(ksp2Path);
            }

            else
            {
                FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(ksp2Path);
                version = versionInfo.ProductVersion;
            }


            return (ksp2Path, version);
        }

        // Generate Search directories depending on OS
        private List<string> GenerateSearchDirectories(string path = "")
        {
            List<string> searchDirectories = new List<string>();

            if (string.IsNullOrEmpty(path))
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    // Windows search directories
                    // Get all available drive letters
                    DriveInfo[] allDrives = DriveInfo.GetDrives();

                    foreach (DriveInfo drive in allDrives)
                    {
                        // Steam
                        searchDirectories.Add(Path.Combine(drive.Name, "Program Files (x86)", "Steam", "steamapps", "common", "Kerbal Space Program 2"));
                        searchDirectories.Add(Path.Combine(drive.Name, "Program Files", "Steam", "steamapps", "common", "Kerbal Space Program 2"));
                        searchDirectories.Add(Path.Combine(drive.Name, "SteamLibrary", "steamapps", "common", "Kerbal Space Program 2"));

                        // Epic Games
                        searchDirectories.Add(Path.Combine(drive.Name, "Program Files", "Epic Games", "Kerbal Space Program 2"));
                        searchDirectories.Add(Path.Combine(drive.Name, "Epic Games", "Kerbal Space Program 2"));

                        // Generic
                        searchDirectories.Add(Path.Combine(drive.Name, "Games", "Kerbal Space Program 2"));
                    }
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    // macOS search directories
                    searchDirectories.Add(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Library", "Application Support", "Steam", "steamapps", "common", "Kerbal Space Program 2"));
                    searchDirectories.Add(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Library", "Application Support", "Epic", "Kerbal Space Program 2"));
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    // Linux search directories
                    searchDirectories.Add(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), ".local", "share", "Steam", "steamapps", "common", "Kerbal Space Program 2"));
                    searchDirectories.Add(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), ".local", "share", "Epic", "Kerbal Space Program 2"));
                }
            }
            else
            {
                searchDirectories = new List<string> { path };
            }

            return searchDirectories;
        }


        // Method to find KSP2 executable
        private string FindKSP2(List<string> searchDirectories)
        {
            string fileName;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                fileName = "KSP2_x64";
            }

            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                fileName = "KSP2";
            }

            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                fileName = "KSP2_x64";
            }
            else
            {
                throw new NotSupportedException("Unsupported platform");
            }

            foreach (string directory in searchDirectories)
            {
                try
                {
                    string[] files = Directory.GetFiles(directory);

                    foreach (string file in files)
                    {
                        if (Path.GetFileNameWithoutExtension(file) == fileName)
                        {
                            Trace.WriteLine($"[INFO] Found KSP2!");
                            return file;
                        }
                    }
                }

                catch (Exception ex)
                {
                    Trace.WriteLine($"[ERROR] {ex.Message}");
                }

            }

            return "";
        }

        // Get the version number of the KSP2 executable for Linux
        private string GetFileVersionLinux(string filePath)
        {
            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo()
                {
                    FileName = "exiftool",
                    Arguments = $"-ProductVersion {filePath}",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                };

                using (Process process = Process.Start(startInfo))
                {
                    using (StreamReader reader = process.StandardOutput)
                    {
                        string result = reader.ReadToEnd();
                        return result.Split(':')[0].Trim();
                    }
                }
            }

            catch (Exception ex)
            {
                Trace.WriteLine($"[ERROR]: {ex}");
                return "";
            }

        }

        // Get the version number for MacOS
        private string GetFileVersionMacOS(string filePath)
        {
            try
            {
                Process process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "mdls",
                        Arguments = $"-name kMDItemVersion \"{filePath}\"",
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                string version = output.Split('=').LastOrDefault()?.Trim();
                return version.Trim('"');
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"[ERROR]: {ex}");
                return "";
            }
        }

    }
}
