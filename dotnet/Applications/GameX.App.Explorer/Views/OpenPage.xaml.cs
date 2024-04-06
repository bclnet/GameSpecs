using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace GameX.App.Explorer.Views
{
    /// <summary>
    /// Interaction logic for OpenPage.xaml
    /// </summary>
    public partial class OpenPage : Window, INotifyPropertyChanged
    {
        public OpenPage()
        {
            InitializeComponent();
            DataContext = this;
            Loaded += OnReady;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public ICollection<Family> Families { get; } = FamilyManager.Families.Values;

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

        ICollection<FamilyGame> _games;
        public ICollection<FamilyGame> Games
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

        void Family_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selected = (Family)Family.SelectedItem;
            Games = selected?.Games.Values.Where(x => !x.Ignore).ToList();
            Game.SelectedIndex = -1;
        }

        void Game_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selected = (FamilyGame)Game.SelectedItem;
            Editions = selected?.Editions.Values.ToList();
            Edition.SelectedIndex = -1;
            PakUris = selected?.ToPaks(null);
        }

        void Edition_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedGame = (FamilyGame)Game.SelectedItem;
            var selected = (FamilyGame.Edition)Edition.SelectedItem;
            PakUris = selectedGame?.ToPaks(selected?.Id);
        }

        void Pak1Uri_Click(object sender, RoutedEventArgs e)
        {
            var openDialog = new OpenFileDialog { Filter = "PAK files|*.*" };
            if (openDialog.ShowDialog() == true)
            {
                var files = openDialog.FileNames;
                if (files.Length < 1) return;
                var file = files[0];
                var selected = (FamilyGame)Game.SelectedItem;
                Pak1Uri = new UriBuilder(file) { Fragment = selected?.Id ?? "Unknown" }.Uri;
            }
        }

        void Pak2Uri_Click(object sender, RoutedEventArgs e)
        {
            var openDialog = new OpenFileDialog { Filter = "PAK files|*.*" };
            if (openDialog.ShowDialog() == true)
            {
                var files = openDialog.FileNames;
                if (files.Length < 1) return;
                var file = files[0];
                var selected = (FamilyGame)Game.SelectedItem;
                Pak2Uri = new UriBuilder(file) { Fragment = selected?.Id ?? "Unknown" }.Uri;
            }
        }

        void Pak3Uri_Click(object sender, RoutedEventArgs e)
        {
            var openDialog = new OpenFileDialog { Filter = "PAK files|*.*" };
            if (openDialog.ShowDialog() == true)
            {
                var files = openDialog.FileNames;
                if (files.Length < 1) return;
                var file = files[0];
                var selected = (FamilyGame)Game.SelectedItem;
                Pak3Uri = new UriBuilder(file) { Fragment = selected?.Id ?? "Unknown" }.Uri;
            }
        }

        void Cancel_Click(object sender, RoutedEventArgs e) => Close();

        void Open_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        void OnReady(object sender, EventArgs e)
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
