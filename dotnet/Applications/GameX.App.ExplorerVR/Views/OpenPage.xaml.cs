using GameX.App.Explorer.Controls;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GameX.App.Explorer.Views
{
    public partial class OpenPage : ContentPage
    {
        public OpenPage()
        {
            InitializeComponent();
            BindingContext = this;
        }

        public IList<Family> Families { get; } = FamilyManager.Families.Values.ToList();

        internal Family FamilySelectedItem => (Family)Family.SelectedItem;

        public IList<Uri> PakUris
        {
            get => new[] { _pak1Uri, _pak2Uri, _pak3Uri }.Where(x => x != null).ToList();
            set
            {
                var idx = 0;
                Uri pak1Uri = null, pak2Uri = null, pak3Uri = null;
                if (value != null)
                    foreach (var uri in value)
                    {
                        if (uri == null) continue;
                        switch (++idx)
                        {
                            case 1: pak1Uri = uri; break;
                            case 2: pak2Uri = uri; break;
                            case 3: pak3Uri = uri; break;
                            default: break;
                        }
                    }
                Pak1Uri = pak1Uri;
                Pak2Uri = pak2Uri;
                Pak3Uri = pak3Uri;
            }
        }

        IList<FamilyGame> _games;
        public IList<FamilyGame> Games
        {
            get => _games;
            set { _games = value; OnPropertyChanged(); }
        }

        ICollection<FamilyGame.Edition> _editions;
        public ICollection<FamilyGame.Edition> Editions
        {
            get => _editions;
            set { _editions = value; OnPropertyChanged(); }
        }

        Uri _pak1Uri;
        public Uri Pak1Uri
        {
            get => _pak1Uri;
            set { _pak1Uri = value; OnPropertyChanged(); }
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
            Games = selected?.Games.Values.Where(x => !x.Ignore).ToList();
            Game.SelectedIndex = -1;
        }

        void Game_SelectionChanged(object sender, EventArgs e)
        {
            var selected = (FamilyGame)Game.SelectedItem;
            Editions = selected?.Editions.Values.ToList();
            Edition.SelectedIndex = -1;
            PakUris = selected?.ToPaks(null);
        }

        void Edition_SelectionChanged(object sender, EventArgs e)
        {
            var selectedGame = (FamilyGame)Game.SelectedItem;
            var selected = (FamilyGame.Edition)Edition.SelectedItem;
            PakUris = selectedGame?.ToPaks(selected?.Id);
        }

        async void Pak1Uri_Click(object sender, EventArgs e)
        {
            var results = await FilePicker2.Default.PickAsync(new PickOptions { PickerTitle = "PAK files" });
            if (results != null)
            {
                var file = results.FileName;
                var selected = (FamilyGame)Game.SelectedItem;
                Pak1Uri = new UriBuilder(file) { Fragment = selected?.Id ?? "Unknown" }.Uri;
            }
        }

        async void Pak2Uri_Click(object sender, EventArgs e)
        {
            var results = await FilePicker2.Default.PickAsync(new PickOptions { PickerTitle = "PAK files" });
            if (results != null)
            {
                var file = results.FileName;
                var selected = (FamilyGame)Game.SelectedItem;
                Pak2Uri = new UriBuilder(file) { Fragment = selected?.Id ?? "Unknown" }.Uri;
            }
        }

        async void Pak3Uri_Click(object sender, EventArgs e)
        {
            var results = await FilePicker2.Default.PickAsync(new PickOptions { PickerTitle = "PAK files" });
            if (results != null)
            {
                var file = results.FileName;
                var selected = (FamilyGame)Game.SelectedItem;
                Pak3Uri = new UriBuilder(file) { Fragment = selected?.Id ?? "Unknown" }.Uri;
            }
        }

        async void Cancel_Click(object sender, EventArgs e) => await Navigation.PushAsync(new MainPage());

        async void Open_Click(object sender, EventArgs e)
        {
            var mainPage = new MainPage();
            mainPage.Open(FamilySelectedItem, PakUris);
            //App.Instance.MainPage = mainPage;
            await Navigation.PushAsync(mainPage);
        }

        internal void OnReady()
        {
            if (string.IsNullOrEmpty(Config.DefaultFamily)) return;
            Family.SelectedIndex = FamilyManager.Families.Keys.ToList().IndexOf(Config.DefaultFamily);
            if (string.IsNullOrEmpty(Config.DefaultGame)) return;
            Game.SelectedIndex = ((List<FamilyGame>)Games).FindIndex(x => x.Id == Config.DefaultGame);
            if (!string.IsNullOrEmpty(Config.DefaultEdition))
                Edition.SelectedIndex = ((List<FamilyGame.Edition>)Editions).FindIndex(x => x.Id == Config.DefaultEdition);
            if (Config.ForceOpen) Open_Click(null, null);
        }
    }
}