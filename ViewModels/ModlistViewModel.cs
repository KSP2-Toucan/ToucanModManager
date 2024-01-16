using Avalonia;
using Avalonia.Threading;
using DynamicData;
using DynamicData.Binding;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using ToucanUI.Models.KSP2;
using ToucanUI.Services;
using static ToucanUI.Services.InstallManager;

namespace ToucanUI.ViewModels
{
    public class ModlistViewModel : ViewModelBase
    {

        // =====================
        // VIEW MODELS
        // =====================
        public MainWindowViewModel MainViewModel { get; }



        // =====================
        // SERVICES
        // =====================
        SpacedockAPI api = new SpacedockAPI();

        public InstallManager Installer => MainViewModel.installer;



        // =====================
        // VARIABLES
        // =====================

        public readonly int BepinexId = 3277;
        // The current category (All, Top, New, Featured) (currently hardcoded to All due to API issues)
        private SpacedockAPI.Category _category;

        public SpacedockAPI.Category Category
        {
            get => _category;
            set
            {
                _category = value;
                this.RaiseAndSetIfChanged(ref _category, value);

                // Clear the side panel when the category changes
                MainViewModel.SelectedMod = null;
                MainViewModel.SidePanelVM.SidePanelVisible = false;

                // Fetch mods with the new category
                FetchMods(Category);
            }
        }

        // List for filtering mods
        private ReadOnlyObservableCollection<ModViewModel> _mods;
        public ReadOnlyObservableCollection<ModViewModel> Mods { get => _mods; set => this.RaiseAndSetIfChanged(ref _mods, value); }

        // The main mod list
        public ObservableCollection<ModViewModel> ModList { get; set; }

        // List of mods selected via checkbox for bulk actions
        public SourceList<ModViewModel> SelectedBulkMods { get; set; }
        public ReadOnlyObservableCollection<ModViewModel> SelectedBulkModsReadOnly { get; private set; }


        public enum FetchStateEnum
        {
            Fetching,
            Failed,
            Offline,
            Success
        }

        private FetchStateEnum _fetchState;
        public FetchStateEnum FetchState
        {
            get => _fetchState;
            set => this.RaiseAndSetIfChanged(ref _fetchState, value);
        }

        public enum ViewStateEnum
        {
            Grid,
            Classic
        }

        private ViewStateEnum _viewState;
        public ViewStateEnum ViewState
        {
            get => _viewState;
            set => this.RaiseAndSetIfChanged(ref _viewState, value);
        }

        // Sets the fetching message
        private string _fetchingMessage;
        public string FetchingMessage
        {
            get => _fetchingMessage;
            set => this.RaiseAndSetIfChanged(ref _fetchingMessage, value);
        }

        // Sets the fetching progress bar value (current page)
        private int _fetchCurrentPage;
        public int FetchCurrentPage
        {
            get => _fetchCurrentPage;
            set => this.RaiseAndSetIfChanged(ref _fetchCurrentPage, value);
        }

        // Sets the fetching progress bar max value (total pages)
        private int _fetchTotalPages;
        public int FetchTotalPages
        {
            get => _fetchTotalPages;
            set => this.RaiseAndSetIfChanged(ref _fetchTotalPages, value);
        }


        // Static fetch phrases list
        private static readonly List<string> _fetchPhrases = new List<string>()
        {
            "Intercepting Mod Telemetry...",
            "Fetching mod data from Spacedock...",
            "Calculating Δv...",
            "Initializing Warp Drive...",
            "Executing Gravity Turn...",
            "Scanning the Modiverse...",
            "Deploying orbital repairs...",
            "Colonizing Duna...",
            "Avoiding catastrophic meltdown...",
            "Slaying the Kraken, please wait...",
            "Preparing for launch...",
            "Simulating aerodynamic forces...",
            "Increasing thrust...",
            "Deploying backup parachutes...",
            "Breaking the sound barrier...",
            "Calculating Hohmann transfer...",
        };

        // Tracks if the user is searching
        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                this.RaiseAndSetIfChanged(ref _searchText, value);
                //Clear the side panel if the user types - fixes visual bug
                MainViewModel.SelectedMod = null;
                MainViewModel.SidePanelVM.SidePanelVisible = false;

            }
        }

        // Progress message for the bulk action panel
        private string _bulkActionMessage;
        public string BulkActionMessage
        {
            get => _bulkActionMessage;
            set => this.RaiseAndSetIfChanged(ref _bulkActionMessage, value);
        }

        // Tracks if a bulk action is in progress
        private bool _isBulkActionInProgress;
        public bool IsBulkActionInProgress
        {
            get => _isBulkActionInProgress;
            set => this.RaiseAndSetIfChanged(ref _isBulkActionInProgress, value);
        }


        private int _bulkActionProgressCount;
        public int BulkActionProgressCount
        {
            get => _bulkActionProgressCount;
            set => this.RaiseAndSetIfChanged(ref _bulkActionProgressCount, value);
        }

        private ModViewModel _currentBulkActionMod;
        public ModViewModel CurrentBulkActionMod
        {
            get => _currentBulkActionMod;
            set => this.RaiseAndSetIfChanged(ref _currentBulkActionMod, value);
        }




        // =====================
        // COMMANDS
        // =====================
        public ReactiveCommand<ModViewModel, Unit> DownloadModCommand { get; }
        public ReactiveCommand<ModViewModel, Unit> CancelDownloadModCommand { get; }
        public ReactiveCommand<ModViewModel, Unit> UninstallModCommand { get; }
        public ReactiveCommand<ModViewModel, Unit> UpdateModCommand { get; }
        public ReactiveCommand<Unit, Unit> UpdateAllCommand { get; }
        public ReactiveCommand<Unit, Unit> ToggleViewCommand { get; }
        public ReactiveCommand<Unit, Unit> SelectAllModsCommand { get; }
        public ReactiveCommand<Unit, Unit> UnselectAllModsCommand { get; }
        public ReactiveCommand<Unit, Unit> InstallSelectedModsCommand { get; }
        public ReactiveCommand<Unit, Unit> UpdateSelectedModsCommand { get; }
        public ReactiveCommand<Unit, Unit> UninstallSelectedModsCommand { get; }

        public ReactiveCommand<Unit, Unit> SkipBulkActionCommand { get; }
        public ReactiveCommand<Unit, Unit> CancelBulkActionCommand { get; }

        public ReactiveCommand<Unit, Unit> LoadOfflineModsCommand { get; }




        // =====================
        // CONSTRUCTOR
        // =====================
        public ModlistViewModel(MainWindowViewModel mainViewModel)
        {

            MainViewModel = mainViewModel;
            ViewState = ViewStateEnum.Grid;

            DownloadModCommand = ReactiveCommand.Create<ModViewModel>(mod => DownloadModAsync(mod), mainViewModel.WhenAnyValue(x => x.ValidGameFound));
            CancelDownloadModCommand = ReactiveCommand.Create<ModViewModel>(CancelSingleDownload);
            UninstallModCommand = ReactiveCommand.Create<ModViewModel>(mod => UninstallModAndSetState(mod));
            UpdateModCommand = ReactiveCommand.Create<ModViewModel>(mod => UpdateMod(mod));
            UpdateAllCommand = ReactiveCommand.Create(UpdateAllInstalledMods);
            ToggleViewCommand = ReactiveCommand.Create(SwitchView);
            SelectAllModsCommand = ReactiveCommand.Create(SelectAllMods);
            UnselectAllModsCommand = ReactiveCommand.Create(UnselectAllMods);

            InstallSelectedModsCommand = ReactiveCommand.Create(BulkInstallSelectedMods);
            UpdateSelectedModsCommand = ReactiveCommand.Create(BulkUpdateSelectedMods);
            UninstallSelectedModsCommand = ReactiveCommand.Create(BulkUninstallSelectedMods);
            SkipBulkActionCommand = ReactiveCommand.Create(SkipBulkAction);
            CancelBulkActionCommand = ReactiveCommand.Create(() => CancelBulkAction());

            LoadOfflineModsCommand = ReactiveCommand.CreateFromTask(LoadOfflineMods);


            ModList = new ObservableCollection<ModViewModel>();

            SelectedBulkMods = new SourceList<ModViewModel>();

            // Create a derived ReadOnlyObservableCollection
            SelectedBulkMods.Connect()
                .Bind(out var readOnlyCollection)
                .Subscribe();

            SelectedBulkModsReadOnly = readOnlyCollection;


            // Fetch list of mods from spacedock api
            //FetchMods(SpacedockAPI.Category.All);

            var observableSearchFilter = this.WhenAnyValue(viewModel => viewModel.SearchText).Select(SearchNameAndAuthor);
            var InstalledFilter = this.WhenAnyValue(x => x.MainViewModel.ControlPanelVM.FilterInstalled).Select(SetInstalledFilter);
            var NotInstalledFilter = this.WhenAnyValue(x => x.MainViewModel.ControlPanelVM.FilterNotInstalled).Select(SetNotInstalledFilter);
            var VersionFilter = this.WhenAnyValue(x => x.MainViewModel.ControlPanelVM.FilterVersion).Select(SetVersionFilter);
            var UpdateAvailableFilter = this.WhenAnyValue(x => x.MainViewModel.ControlPanelVM.FilterUpdateAvailable).Select(SetUpdateAvailableFilter);

            var modListChangeSet = ModList.ToObservableChangeSet();

            var _sourceList = modListChangeSet
                .Sort(SortExpressionComparer<ModViewModel>.Ascending(x => x.ModObject.Name))
                .Filter(observableSearchFilter)
                .Filter(InstalledFilter)
                .Filter(NotInstalledFilter)
                .Filter(VersionFilter)
                .Filter(UpdateAvailableFilter)
                .AsObservableList();

            _sourceList.Connect()
                .Bind(out _mods)
                .DisposeMany()
                .Subscribe();

        }



        // =====================
        // METHODS (Filters)
        // =====================

        // Search bar filter
        private Func<ModViewModel, bool> SearchNameAndAuthor(string searchText)
        {
            if (string.IsNullOrEmpty(searchText)) //If searchbar is empty, show all the mods
            {
                return Mod => true;
            }
            else
            {
                return Mod => Mod.ModObject.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                              Mod.ModObject.Author.Contains(searchText, StringComparison.OrdinalIgnoreCase);
            }
        }

        // Set the IsInstalled filter on or off
        private Func<ModViewModel, bool> SetInstalledFilter(bool isInstalledFilterOn)
        {
            if (isInstalledFilterOn)
            {
                return Mod => Mod.ModState == ModViewModel.ModStateEnum.Installed;
            }
            else
            {
                return Mod => true;
            }
        }

        private Func<ModViewModel, bool> SetNotInstalledFilter(bool isNotInstalledFilterOn)
        {
            if (isNotInstalledFilterOn)
            {
                return Mod => Mod.ModState == ModViewModel.ModStateEnum.NotInstalled;
            }
            else
            {
                return Mod => true;
            }
        }


        // Set the compatible version filter on/off
        private Func<ModViewModel, bool> SetVersionFilter(bool isOn)
        {
            var configManager = new ConfigurationManager();
            if (isOn)
            {
                return Mod => Mod.GetLatestVersion().GameVersion.Equals(configManager.GetGameVersion());
            }
            else
            {
                return Mod => true;
            }
        }

        // Set the IsUpdateAvailable filter on or off
        private Func<ModViewModel, bool> SetUpdateAvailableFilter(bool isOn)
        {
            if (isOn)
            {
                return Mod => Mod.IsUpdateAvailable;
            }
            else
            {
                return Mod => true;
            }
        }


        // Switch between Classic modlist view or DataGrid view
        public void SwitchView()
        {
            if(ViewState == ViewStateEnum.Classic)
            {
                ViewState = ViewStateEnum.Grid;
            }

            else
            {
                ViewState = ViewStateEnum.Classic;
            }

        }


        // =====================
        // METHODS (Download/Install)
        // =====================

        // Cancel a download/install
        public void CancelSingleDownload(ModViewModel mod)
        {
            mod.CancellationTokenSource.Cancel();
            mod.ModState = ModViewModel.ModStateEnum.NotInstalled;

        }

        // Load the mod list from the API
        public async Task FetchMods(SpacedockAPI.Category category)
        {
            
            // Clear the mod list first
            ModList.Clear();
            SelectedBulkMods.Clear();

            // Set defaults for progress bar
            FetchTotalPages = 1;
            FetchCurrentPage = 0;

            // Set the fetching message
            FetchingMessage = _fetchPhrases[new Random().Next(0, _fetchPhrases.Count)];
            FetchState = FetchStateEnum.Fetching;

            // Get the modlist from the API
            var mods = await api.GetMods((currentPage, totalPages) =>
            {
                FetchCurrentPage = currentPage;
                FetchTotalPages = totalPages;
            }, category).ConfigureAwait(false);

            // Update ModList on the UI thread
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                ModList.Clear();
                foreach (var mod in mods)
                {
                    var modViewModel = new ModViewModel(mod);

                    // Skip the single bepinex mod (we have the bepinex/spacewarp version)
                    if (mod.Id == 3255)
                    {
                        Trace.WriteLine($"[INFO] Skipping {mod.Name} [{mod.Id}]");
                        continue;
                    }

                    // Else if its the correct BepInEx/SpaceWarp id
                    else if (mod.Id == BepinexId)
                    {
                        // If the mod is installed, set the state to installed
                        if (MainViewModel.BepInExState == InstallManager.BepInExStatusEnum.Installed)
                        {
                            modViewModel.ModState = ModViewModel.ModStateEnum.Installed;
                            modViewModel.Progress = 100;
                        }
                    }

                    ModList.Add(modViewModel);

                    // Subscribe to IsSelectedBulk changes
                    modViewModel
                        .WhenAnyValue(x => x.IsSelectedBulk)
                        .Subscribe(isSelected =>
                        {
                            if (isSelected)
                            {
                                SelectedBulkMods.Add(modViewModel);
                            }
                            else
                            {
                                SelectedBulkMods.Remove(modViewModel);
                            }

                        });
                }
                // Load the installed JSON list here
                ObservableCollection<Mod> installedMods = Installer?.ReadInstalledMods() ?? new ObservableCollection<Mod>();

                ObservableCollection<ModViewModel> installedModViewModels = new ObservableCollection<ModViewModel>(installedMods.Select(mod => new ModViewModel(mod)));

                MarkInstalledMods(ModList, installedModViewModels);



            });

            // If nothing was returned from the API, set the fetching message to an error
            if (ModList.Count < 1)
            {

                // Check if Spacedock.info is online
                bool isSpacedockOnline = await api.PingSpacedock().ConfigureAwait(false);

                if (!isSpacedockOnline)
                {
                    FetchingMessage = "Unable to reach spacedock.info";
                }
                else
                {
                    FetchingMessage = "Something went wrong! Unable to fetch data.";
                }

                FetchState = FetchStateEnum.Failed;
                return;
            }

            // If the API returned data, set the fetching message to null
            else
            {
                FetchingMessage = "";
                FetchState = FetchStateEnum.Success;
            }


        }


        public void MarkInstalledMods(ObservableCollection<ModViewModel> onlineMods, ObservableCollection<ModViewModel> installedModViewModels)
        {
            // Iterate through the onlineMods and installedModViewModels
            foreach (var onlineMod in onlineMods)
            {
                var offlineMod = installedModViewModels.FirstOrDefault(om => om.ModObject.Id == onlineMod.ModObject.Id);
                if (offlineMod != null)
                {
                    // If a match is found, set the IsInstalled property to true
                    onlineMod.ModState = ModViewModel.ModStateEnum.Installed;
                    onlineMod.Progress = 100;
                    onlineMod.IsModifiable = false;

                    // Find the installed version in the offline mod
                    var installedVersion = offlineMod.ModObject.Versions.FirstOrDefault(v => v.IsInstalled);

                    // Set the SelectedVersionViewModel from the installed version
                    if (installedVersion != null)
                    {
                        var selectedVersionViewModel = onlineMod.VersionViewModels
                            .FirstOrDefault(vvm => vvm.VersionObject.VersionID == installedVersion.VersionID);

                        if (selectedVersionViewModel != null)
                        {
                            onlineMod.SelectedVersionViewModel = selectedVersionViewModel;

                            // Update the IsSelected field based on the IsInstalled field of the VersionObject
                            selectedVersionViewModel.IsSelected = installedVersion.IsInstalled;
                        }
                    }

                    // Compare the Created field of the LatestVersion with the installed SelectedVersion
                    if (onlineMod.GetLatestVersion().Created > onlineMod.SelectedVersionViewModel.VersionObject.Created)
                    {
                        // Notify the user about the version mismatch
                        Trace.WriteLine($"[INFO] Mod {onlineMod.ModObject.Name} is not up to date. Installed version: {onlineMod.SelectedVersionViewModel.VersionObject.FriendlyVersion}, Latest version: {onlineMod.GetLatestVersion().FriendlyVersion}");
                        onlineMod.IsUpdateAvailable = true;
                    }
                    else
                    {
                        onlineMod.IsUpdateAvailable = false;
                    }
                }
            }
        }


        // Function to download a mod asynchronously
        public Task DownloadModAsync(ModViewModel mod, Action onSuccess = null)
        {

            if (Installer == null)
            {
                Trace.WriteLine("[WARNING] Installer is null!");
                return Task.CompletedTask;
            }

            Trace.WriteLine($"[INFO] Installing {mod.ModObject.Name}!");

            mod.ModState = ModViewModel.ModStateEnum.Downloading;
            mod.Progress = 0;
            mod.IsModifiable = false;

            mod.CancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = mod.CancellationTokenSource.Token;

            var tcs = new TaskCompletionSource<bool>();

            cancellationToken.Register(() =>
            {

                if (mod.ModState != ModViewModel.ModStateEnum.Installed)
                {
                    Trace.WriteLine($"[INFO] Cancelling {mod.ModObject.Name}!");
                    mod.Progress = 0;
                }
                mod.IsModifiable = true;
                tcs.TrySetCanceled();
            });

            try
            {
                Installer.DownloadMod(mod, tcs, cancellationToken);
                tcs.Task.ContinueWith(t =>
                {
                    if (t.IsCompletedSuccessfully)
                    {
                        Trace.WriteLine($"[INFO] Installed {mod.ModObject.Name}!");
                        onSuccess?.Invoke();
                    }
                });
            }

            catch (Exception ex)
            {
                Trace.WriteLine($"[ERROR] {ex}");
            }

            return tcs.Task;
        }

        public async Task UpdateMod(ModViewModel mod)
        {
            if (!mod.IsUpdateAvailable)
            {
                return;
            }

            Trace.WriteLine($"[INFO] Updating {mod.ModObject.Name}!");

            if(mod.ModObject.Id == BepinexId)
            {
                // Update BepInEx function
                await Installer.UpdateBepInEx(this);
            }

            // Delete the current mod
            UninstallModAndSetState(mod);

            // Set the SelectedVersionViewModel to latest version
            Models.KSP2.Version latestVersion = mod.GetLatestVersion();
            if (latestVersion != null)
            {
                mod.SelectedVersionViewModel = mod.VersionViewModels.FirstOrDefault(v => v.VersionObject == latestVersion);
            }

            // Call the DownloadModAsync function to install the latest version
            await DownloadModAsync(mod, () =>
            {
                // Update the SelectedVersionViewModel to the latest version after the download is successful
                mod.SelectedVersionViewModel = mod.VersionViewModels.FirstOrDefault(v => v.VersionObject == latestVersion);
                mod.SelectedVersionViewModel.IsSelected = true;
                mod.IsUpdateAvailable = false;
                mod.RaisePropertyChanged(nameof(mod.SelectedVersionViewModel));

            });

        }

        public void UpdateAllInstalledMods()
        {
            // Create a new list to store the mods to be updated
            List<ModViewModel> modsToUpdate = new List<ModViewModel>();

            // Iterate through the ModList
            for (int i = 0; i < ModList.Count; i++)
            {
                ModViewModel mod = ModList[i];

                // Check if the mod is installed and an update is available
                if (mod.ModState == ModViewModel.ModStateEnum.Installed && mod.IsUpdateAvailable)
                {
                    // Add the mod to the modsToUpdate list
                    modsToUpdate.Add(mod);
                }
            }

            // Update SelectedBulkMods with the modsToUpdate list
            SelectedBulkMods.Clear();
            foreach (ModViewModel mod in modsToUpdate)
            {
                SelectedBulkMods.Add(mod);
            }

            // Call the BulkUpdateSelectedMods function to update the selected mods
            BulkUpdateSelectedMods();
        }

        private async Task LoadOfflineMods()
        {
            FetchState = FetchStateEnum.Offline;
            FetchingMessage = "";

            // Load the offline JSON list here
            ObservableCollection<Mod> offlineMods = Installer.ReadInstalledMods();

            ObservableCollection<ModViewModel> installedModViewModels = new ObservableCollection<ModViewModel>(offlineMods.Select(mod => new ModViewModel(mod)));

            // Update the ModList on the UI thread
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                ModList.Clear();
                foreach (var modViewModel in installedModViewModels)
                {
                    modViewModel.ModState = ModViewModel.ModStateEnum.Installed;
                    modViewModel.Progress = 100;
                    modViewModel.IsModifiable = false;


                    // Subscribe to IsSelectedBulk changes
                    modViewModel
                        .WhenAnyValue(x => x.IsSelectedBulk)
                        .Subscribe(isSelected =>
                        {
                            if (isSelected)
                            {
                                SelectedBulkMods.Add(modViewModel);
                            }
                            else
                            {
                                SelectedBulkMods.Remove(modViewModel);
                            }

                        });

                    ModList.Add(modViewModel);
                }


            });

            FetchState = FetchStateEnum.Success;
        }


        // =====================
        // METHODS (Commands)
        // =====================

        public async void BulkInstallSelectedMods()
        {
            IsBulkActionInProgress = true;

            // Convert SelectedBulkMods.Items to a List<ModViewModel> to access elements by index
            List<ModViewModel> modList = SelectedBulkMods.Items.ToList();

            // Iterate through the modList
            for (int i = 0; i < modList.Count; i++)
            {
                ModViewModel mod = modList[i];
                CurrentBulkActionMod = mod;

                // Update the progress message
                BulkActionMessage = $"Installing mod {i + 1} of {modList.Count}";

                // Check if the mod is already installed
                if (mod.ModState == ModViewModel.ModStateEnum.Installed)
                {
                    // If it is, skip this mod and continue to the next one
                    continue;
                }

                // If the mod is not installed, call the DownloadModAsync function
                try
                {
                    await DownloadModAsync(mod);
                    BulkActionProgressCount++;
                }
                catch (OperationCanceledException)
                {
                    // If the user clicked "Cancel", break the loop
                    if (mod.ModState == ModViewModel.ModStateEnum.Canceled)
                    {
                        break;
                    }

                    // If the user clicked "Skip", continue to the next mod
                    if (mod.ModState == ModViewModel.ModStateEnum.Skipped)
                    {
                        mod.ModState = ModViewModel.ModStateEnum.NotInstalled;
                        continue;
                    }


                }
            }

            // Reset the progress message and switch back to the first bulk action panel
            BulkActionMessage = "";
            IsBulkActionInProgress = false;
            BulkActionProgressCount = 0;
        }

        public async void BulkUpdateSelectedMods()
        {
            IsBulkActionInProgress = true;

            // Convert SelectedBulkMods.Items to a List<ModViewModel> to access elements by index
            List<ModViewModel> modList = SelectedBulkMods.Items.ToList();

            // Iterate through the modList
            for (int i = 0; i < modList.Count; i++)
            {
                ModViewModel mod = modList[i];
                CurrentBulkActionMod = mod;

                // Update the progress message
                BulkActionMessage = $"Updating mod {i + 1} of {modList.Count}";

                // Check if the mod is installed
                if (mod.ModState != ModViewModel.ModStateEnum.Installed)
                {
                    // If it is not installed, skip this mod and continue to the next one
                    continue;
                }

                // If the mod is installed, call the UpdateMod function
                try
                {
                    await UpdateMod(mod);
                    BulkActionProgressCount++;
                }
                catch (Exception)
                {
                    // Handle any exceptions that occur during the update process
                }
            }

            // Reset the progress message and switch back to the first bulk action panel
            BulkActionMessage = "";
            IsBulkActionInProgress = false;
            BulkActionProgressCount = 0;
            SelectedBulkMods.Clear();
        }


        public async void BulkUninstallSelectedMods()
        {
            IsBulkActionInProgress = true;

            // Convert SelectedBulkMods.Items to a List<ModViewModel> to access elements by index
            List<ModViewModel> modList = SelectedBulkMods.Items.ToList();

            // Iterate through the modList
            for (int i = 0; i < modList.Count; i++)
            {
                ModViewModel mod = modList[i];
                CurrentBulkActionMod = mod;

                // Update the progress message
                BulkActionMessage = $"Uninstalling mod {i + 1} of {modList.Count}";

                // Check if the mod is installed
                if (mod.ModState != ModViewModel.ModStateEnum.Installed)
                {
                    // If it is not installed, skip this mod and continue to the next one
                    continue;
                }

                // If the mod is installed, call the DeleteMod function
                try
                {
                    UninstallModAndSetState(mod);
                    BulkActionProgressCount++;
                }
                catch (Exception)
                {
                    // Handle any exceptions that occur during the uninstallation process
                }
            }

            // Reset the progress message and switch back to the first bulk action panel
            BulkActionMessage = "";
            IsBulkActionInProgress = false;
            BulkActionProgressCount = 0;
        }




        public void SkipBulkAction()
        {
            if (CurrentBulkActionMod != null)
            {
                CurrentBulkActionMod.ModState = ModViewModel.ModStateEnum.Skipped;
                CurrentBulkActionMod.CancellationTokenSource.Cancel();


            }
        }


        public void CancelBulkAction()
        {
            foreach (var mod in SelectedBulkMods.Items)
            {
                // Check if CancellationTokenSource is not null before calling Cancel()
                if (mod.CancellationTokenSource != null)
                {
                    CurrentBulkActionMod.ModState = ModViewModel.ModStateEnum.Canceled;
                    mod.CancellationTokenSource.Cancel();
                }
            }
            SelectedBulkMods.Clear();
            UnselectAllMods();
        }


        private void SelectAllMods()
        {
            foreach (var mod in Mods)
            {
                mod.IsSelectedBulk = true;
            }
        }

        private void UnselectAllMods()
        {
            foreach (var mod in Mods)
            {
                mod.IsSelectedBulk = false;
            }
        }

        private void UninstallModAndSetState(ModViewModel mod)
        {
            bool isDeleted = false;

            if (mod.ModObject.Id == BepinexId)
            {
                // Delete BepInEx function
                isDeleted = Installer.DeleteBepInEx();

                if (isDeleted)
                {
                    // Set BepInExState to NotInstalled
                    MainViewModel.BepInExState = BepInExStatusEnum.NotInstalled;
                }
            }
            else
            {
                // Delete mod function
                isDeleted = Installer.DeleteMod(mod);
            }

            if (isDeleted)
            {
                mod.ModState = ModViewModel.ModStateEnum.NotInstalled;
                mod.Progress = 0;
                mod.IsModifiable = true;
                Trace.WriteLine($"[INFO] {mod.ModObject.Name} deleted successfully!");
            }
        }

    }
}