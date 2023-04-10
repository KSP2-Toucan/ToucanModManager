﻿using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Reactive;
using System.Text.Json;
using System.Threading.Tasks;
using ToucanUI.Models;
using ToucanUI.Services;

namespace ToucanUI.ViewModels
{
    public class ModlistViewModel : ViewModelBase
    {
        SpacedockAPI api = new SpacedockAPI();

        public ObservableCollection<Mod> Mods { get; set; }

        public ReactiveCommand<Mod, Unit> DownloadMod { get; }
        public ReactiveCommand<Unit, Unit> ToggleViewCommand { get; }


        private Mod _selectedMod;
        public Mod SelectedMod
        {
            get => _selectedMod;
            set => this.RaiseAndSetIfChanged(ref _selectedMod, value);
            
        }

        private bool _isClassicViewVisible = true;
        public bool IsClassicViewVisible
        {
            get => _isClassicViewVisible;
            set => this.RaiseAndSetIfChanged(ref _isClassicViewVisible, value);
        }

        private bool _isGridViewVisible = false;
        public bool IsGridViewVisible
        {
            get => _isGridViewVisible;
            set => this.RaiseAndSetIfChanged(ref _isGridViewVisible, value);
        }

        //Parent ViewModel for communication
        public MainWindowViewModel MainViewModel { get; }

        // MODLIST VIEWMODEL CONSTRUCTOR
        public ModlistViewModel(MainWindowViewModel mainViewModel)
        {
            //Added this so MainViewModel acts as the communicator between sidepanel and modlist
            MainViewModel = mainViewModel;

            DownloadMod = ReactiveCommand.Create<Mod>(mod => DownloadModAsync(mod));
            ToggleViewCommand = ReactiveCommand.Create(SwitchView);
            LoadMods(true);
            
        }

        public void SwitchView()
        {
            IsClassicViewVisible = !IsClassicViewVisible;
            IsGridViewVisible = !IsGridViewVisible;
        }

        // Load the mod list from the API
        private async Task LoadMods(bool useDummyData = false)
        {
            var mods = await api.GetMods(useDummyData);
            Mods = new ObservableCollection<Mod>(mods);
            foreach (var mod in Mods)
            {
                Debug.WriteLine("Name: " + mod.Name);
                Debug.WriteLine("Id: " + mod.Id);
                Debug.WriteLine("Game: " + mod.Game);
            }
        }


        // Function to download a mod asynchronously
        public async Task DownloadModAsync(Mod mod)
        {
            mod.IsInstalled = false;
            mod.Progress = 0;

            while (mod.Progress <= 100)
            {
                // Dummy code to simulate downloading
                await Task.Delay(100);
                mod.Progress += 2;
                Debug.WriteLine($"Mod {mod.Name} at {mod.Progress}%");
            }

            mod.IsInstalled = true;
        }

        public bool IsSelected(Mod mod)
        {
            return mod == SelectedMod;
        }


    }
}