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
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace GameSpec.App.Explorer.Views
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
            if (!string.IsNullOrEmpty(Config.DefaultFamily)) Family.SelectedIndex = FamilyManager.Families.Keys.ToList().IndexOf(Config.DefaultFamily);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public ICollection<Family> Families { get; } = FamilyManager.Families.Values;

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

        ICollection<FamilyGame> _familyGames;
        public ICollection<FamilyGame> FamilyGames
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

        void Family_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selected = (Family)Family.SelectedItem;
            FamilyGames = selected?.Games.Values;
            if (!string.IsNullOrEmpty(Config.DefaultGameId)) FamilyGame.SelectedIndex = FamilyManager.Families[Config.DefaultFamily].Games.Keys.ToList().IndexOf(Config.DefaultGameId);
            else FamilyGame.SelectedIndex = -1;
            OnReady();
        }

        void FamilyGame_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selected = (FamilyGame)FamilyGame.SelectedItem;
            PakUris = selected?.Paks;
        }

        void PakUriFile_Click(object sender, RoutedEventArgs e)
        {
            var openDialog = new OpenFileDialog { Filter = "PAK files|*.*" };
            if (openDialog.ShowDialog() == true)
            {
                var files = openDialog.FileNames;
                if (files.Length < 1) return;
                var file = files[0];
                var selected = (FamilyGame)FamilyGame.SelectedItem;
                PakUri = new UriBuilder(file) { Fragment = selected?.Id ?? "Unknown" }.Uri;
            }
        }

        void Pak2UriFile_Click(object sender, RoutedEventArgs e)
        {
            var openDialog = new OpenFileDialog { Filter = "PAK files|*.*" };
            if (openDialog.ShowDialog() == true)
            {
                var files = openDialog.FileNames;
                if (files.Length < 1) return;
                var file = files[0];
                var selected = (FamilyGame)FamilyGame.SelectedItem;
                Pak2Uri = new UriBuilder(file) { Fragment = selected?.Id ?? "Unknown" }.Uri;
            }
        }

        void Pak3UriFile_Click(object sender, RoutedEventArgs e)
        {
            var openDialog = new OpenFileDialog { Filter = "PAK files|*.*" };
            if (openDialog.ShowDialog() == true)
            {
                var files = openDialog.FileNames;
                if (files.Length < 1) return;
                var file = files[0];
                var selected = (FamilyGame)FamilyGame.SelectedItem;
                Pak3Uri = new UriBuilder(file) { Fragment = selected?.Id ?? "Unknown" }.Uri;
            }
        }

        void Cancel_Click(object sender, RoutedEventArgs e) => Close();

        void Open_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        void OnReady()
        {
            if (Config.ForceOpen && Config.DefaultGameId != null) Open_Click(null, null);
        }
    }
}
