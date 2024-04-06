using System.Collections.Generic;
using System;
using System.ComponentModel;
using System.Linq;
using Microsoft.Maui.Controls;
using GameX.App.ExplorerVR.Controls;

namespace GameX.App.Explorer.Views
{
    public partial class OpenPage : ContentPage
    {
        public OpenPage()
        {
            InitializeComponent();
            BindingContext = this;
            //if (!string.IsNullOrEmpty(Config.DefaultFamily)) Family.SelectedIndex = FamilyManager.Families.Keys.ToList().IndexOf(Config.DefaultFamily);
        }

        public IList<Family> Families { get; } = FamilyManager.Families.Values.ToList();

        internal Family FamilySelectedItem => (Family)Family.SelectedItem;

        public IList<Uri> PakUris
        {
            get => new[] { _pakUri, _pak2Uri, _pak3Uri }.Where(x => x != null).ToList();
            set
            {
                var idx = 0;
                Uri pakUri = null, pak2Uri = null, pak3Uri = null;
                if (value != null)
                    foreach (var uri in value)
                    {
                        if (uri == null) continue;
                        switch (++idx)
                        {
                            case 1: pakUri = uri; break;
                            case 2: pak2Uri = uri; break;
                            case 3: pak3Uri = uri; break;
                            default: break;
                        }
                    }
                PakUri = pakUri;
                Pak2Uri = pak2Uri;
                Pak3Uri = pak3Uri;
            }
        }

        IList<FamilyGame> _familyGames;
        public IList<FamilyGame> FamilyGames
        {
            get => _familyGames;
            set { _familyGames = value; OnPropertyChanged(); }
        }

        Uri _pakUri;
        public Uri PakUri
        {
            get => _pakUri;
            set { _pakUri = value; OnPropertyChanged(); }
        }

        Uri _pak2Uri;
        public Uri Pak2Uri
        {
            get => _pak2Uri;
            set { _pak2Uri = value; OnPropertyChanged(); }
        }

        Uri _pak3Uri;
        public Uri Pak3Uri
        {
            get => _pak3Uri;
            set { _pak3Uri = value; OnPropertyChanged(); }
        }

        void Family_SelectionChanged(object sender, EventArgs e)
        {
            var selected = (Family)Family.SelectedItem;
            FamilyGames = selected?.Games.Values.ToList();
            if (!string.IsNullOrEmpty(Config.DefaultGameId)) FamilyGame.SelectedIndex = FamilyManager.Families[Config.DefaultFamily].Games.Keys.ToList().IndexOf(Config.DefaultGameId);
            else FamilyGame.SelectedIndex = -1;
            OnReady();
        }

        void FamilyGame_SelectionChanged(object sender, EventArgs e)
        {
            var selected = (FamilyGame)FamilyGame.SelectedItem;
            PakUris = selected?.Paks;
        }

        async void PakUriFile_Click(object sender, EventArgs e)
        {
            var results = await FilePicker.Default.PickAsync(new PickOptions { PickerTitle = "PAK files" });
            if (results != null)
            {
                var file = results.FileName;
                var selected = (FamilyGame)FamilyGame.SelectedItem;
                PakUri = new UriBuilder(file) { Fragment = selected?.Id ?? "Unknown" }.Uri;
            }
        }

        async void Pak2UriFile_Click(object sender, EventArgs e)
        {
            var results = await FilePicker.Default.PickAsync(new PickOptions { PickerTitle = "PAK files" });
            if (results != null)
            {
                var file = results.FileName;
                var selected = (FamilyGame)FamilyGame.SelectedItem;
                Pak2Uri = new UriBuilder(file) { Fragment = selected?.Id ?? "Unknown" }.Uri;
            }
        }

        async void Pak3UriFile_Click(object sender, EventArgs e)
        {
            var results = await FilePicker.Default.PickAsync(new PickOptions { PickerTitle = "PAK files" });
            if (results != null)
            {
                var file = results.FileName;
                var selected = (FamilyGame)FamilyGame.SelectedItem;
                Pak3Uri = new UriBuilder(file) { Fragment = selected?.Id ?? "Unknown" }.Uri;
            }
        }

        async void Cancel_Click(object sender, EventArgs e) { } // => await Navigation.PushAsync(new MainPage());

        async void Open_Click(object sender, EventArgs e)
        {
            var mainPage = new MainPage();
            mainPage.Open(FamilySelectedItem, PakUris);
            //await Navigation.PushAsync(mainPage);
        }

        void OnReady()
        {
            if (Config.ForceOpen && Config.DefaultGameId != null) Open_Click(null, null);
        }
    }
}
