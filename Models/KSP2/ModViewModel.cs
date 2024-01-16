using ReactiveUI;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ToucanUI.Models.KSP2
{
    public class ModViewModel : ViewModelBase
    {
        // =====================
        // VARIABLES
        // =====================

        public Mod ModObject { get; set; }

        public ObservableCollection<VersionViewModel> VersionViewModels { get; } = new ObservableCollection<VersionViewModel>();


        // Keep track of the Selected Version
        private VersionViewModel _selectedVersionViewModel;
        public VersionViewModel SelectedVersionViewModel
        {
            get => _selectedVersionViewModel;
            set => this.RaiseAndSetIfChanged(ref _selectedVersionViewModel, value);
        }

        // Cancellation token for mod download
        public CancellationTokenSource CancellationTokenSource { get; set; }

        public enum ModStateEnum
        {
            NotInstalled,
            Installed,
            Downloading,
            Canceled,
            Uninstalling,
            Skipped
        }

        private ModStateEnum _modState;
        public ModStateEnum ModState
        {
            get => _modState;
            set => this.RaiseAndSetIfChanged(ref _modState, value);
        }

        // Flag if an update is available
        private bool _isUpdateAvailable;
        public bool IsUpdateAvailable
        {
            get => _isUpdateAvailable;
            set => this.RaiseAndSetIfChanged(ref _isUpdateAvailable, value);
        }

        // If mod is currently selected in the side panel
        private bool _isSelectedSidePanel;
        public bool IsSelectedSidePanel
        {
            get => _isSelectedSidePanel;
            set => this.RaiseAndSetIfChanged(ref _isSelectedSidePanel, value);
        }

        // If mod is selected for bulk actions
        private bool _isSelectedBulk;
        public bool IsSelectedBulk
        {
            get => _isSelectedBulk;
            set
            {
                this.RaiseAndSetIfChanged(ref _isSelectedBulk, value);
            }
        }

        // If mod version is modifiable (i.e not installed or installed but not up to date)
        private bool _isModifiable;
        public bool IsModifiable
        {
            get => _isModifiable;
            set => this.RaiseAndSetIfChanged(ref _isModifiable, value);
        }

        // Mod download/install progress
        private int _progress;
        public int Progress
        {
            get => _progress;
            set => this.RaiseAndSetIfChanged(ref _progress, value);
        }

        // =====================
        // CONSTRUCTORS
        // =====================

        // Fetched mod constructor
        public ModViewModel(Mod mod)
        {
  
            ModObject = mod;

            // Initialize VersionViewModels collection
            foreach (var version in ModObject.Versions)
            {
                var versionViewModel = new VersionViewModel(version, this);
                VersionViewModels.Add(versionViewModel);

                // Set IsSelected property of the installed version to true
                if (version.IsInstalled)
                {
                    versionViewModel.IsSelected = true;
                }
            }

            // Find the latest version in the VersionViewModels collection
            var latestVersionViewModel = VersionViewModels.OrderByDescending(v => v.VersionObject.Created).FirstOrDefault();

            // Set SelectedVersionViewModel to the latest version found in the collection
            if (latestVersionViewModel != null && !VersionViewModels.Any(vvm => vvm.IsSelected))
            {
                SelectedVersionViewModel = latestVersionViewModel;
                SelectedVersionViewModel.GetDownloadSizeAsync().ConfigureAwait(false);
                SelectedVersionViewModel.IsSelected = true;
            }

            IsSelectedSidePanel = false;
            IsSelectedBulk = false;
            IsModifiable = true;
            Progress = 0;

            ModState = ModStateEnum.NotInstalled;

        }



        // ===============
        // METHODS
        // ===============

        // Method to get the latest version
        public Version GetLatestVersion()
        {
            return ModObject.Versions.OrderByDescending(v => v.Created).FirstOrDefault();
        }

        // Method to select only one version
        public void OnVersionSelected(VersionViewModel selectedVersionViewModel)
        {
            // Iterate through all VersionViewModel instances
            foreach (var versionViewModel in VersionViewModels)
            {
                // If the current instance is the selected one, set IsSelected to true, otherwise set it to false
                versionViewModel.IsSelected = versionViewModel == selectedVersionViewModel;
            }

            // Update the SelectedVersionViewModel property
            SelectedVersionViewModel = selectedVersionViewModel;

            // Check if the selected version is the latest version
            Version latestVersion = GetLatestVersion();
            if (latestVersion != null && SelectedVersionViewModel.VersionObject != latestVersion && ModState == ModStateEnum.Installed)
            {
                IsUpdateAvailable = true;
            }
            else
            {
                IsUpdateAvailable = false;
            }
        }


        // Method to get the download size of all versions of a mod
        public async Task InitializeDownloadSizesAsync()
        {
            // Call GetDownloadSizeAsync for each VersionViewModel instance that hasn't fetched the download size yet
            var downloadTasks = VersionViewModels.Where(vm => !vm.IsDownloadSizeFetched)
                                                 .Select(versionViewModel => versionViewModel.GetDownloadSizeAsync());

            // Wait for all download tasks to complete
            await Task.WhenAll(downloadTasks);
        }




    }
}
