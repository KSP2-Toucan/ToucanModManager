﻿using ReactiveUI;
using ToucanUI.Models;

namespace ToucanUI.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public ModlistViewModel ModlistVM { get; }
        public SidePanelViewModel SidePanelVM { get; }

        private Mod _selectedMod;
        public Mod SelectedMod
        {
            get => _selectedMod;
            set => this.RaiseAndSetIfChanged(ref _selectedMod, value);
        }

        // In constructor create and refernce both child View Models
        public MainWindowViewModel()
        {
            ModlistVM = new ModlistViewModel(this);
            SidePanelVM = new SidePanelViewModel(this);
        }

    }
}