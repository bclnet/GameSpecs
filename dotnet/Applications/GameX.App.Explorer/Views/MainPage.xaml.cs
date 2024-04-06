using GameX.Meta;
using GameX.Unknown;
using MimeKit;
using Org.BouncyCastle.Cms;
using StereoKit;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GameX.App.Explorer.Views
{
    /// <summary>
    /// ExplorerMainTab
    /// </summary>
    public class ExplorerMainTab
    {
        public string Name { get; set; }
        public PakFile PakFile { get; set; }
        public IList<FamilyApp> AppList { get; set; }
        public string Text { get; set; }
    }

    /// <summary>
    /// Interaction logic for MainPage.xaml
    /// </summary>
    public partial class MainPage : Window, INotifyPropertyChanged
    {
        public static MetaManager Manager = new ResourceManager();
        public static MainPage Instance;

        public MainPage()
        {
            InitializeComponent();
            Instance = this;
            DataContext = this;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public MainPage Open(Family family, IEnumerable<Uri> pakUris, string path = null)
        {
            foreach (var pakFile in PakFiles) pakFile?.Dispose();
            PakFiles.Clear();
            if (family == null) return this;
            FamilyApps = family.Apps;
            foreach (var pakUri in pakUris)
            {
                Log.WriteLine($"Opening {pakUri}");
                var pak = family.OpenPakFile(pakUri);
                if (pak != null) PakFiles.Add(pak);
            }
            Log.WriteLine("Done");
            OnOpenedAsync(family, path).Wait();
            return this;
        }

        IList<ExplorerMainTab> _mainTabs;
        public IList<ExplorerMainTab> MainTabs
        {
            get => _mainTabs;
            set { _mainTabs = value; OnPropertyChanged(); }
        }

        public readonly IList<PakFile> PakFiles = new List<PakFile>();
        public IDictionary<string, FamilyApp> FamilyApps;

        public Task OnOpenedAsync(Family family, string path = null)
        {
            MainTabControl.SelectedIndex = 0; // family.Apps != null ? 1 : 0;
            var tabs = PakFiles.Select(pakFile => new ExplorerMainTab
            {
                Name = pakFile.Name,
                PakFile = pakFile,
            }).ToList();
            var firstPakFile = tabs.FirstOrDefault()?.PakFile ?? PakFile.Empty;
            if (FamilyApps.Count > 0)
                tabs.Add(new ExplorerMainTab
                {
                    Name = "Apps",
                    PakFile = firstPakFile,
                    AppList = FamilyApps.Values.ToList(),
                    Text = "Choose an application.",
                });
            if (!string.IsNullOrEmpty(family.Description))
                tabs.Add(new ExplorerMainTab
                {
                    Name = "Information",
                    Text = family.Description,
                });
            MainTabs = tabs;
            return Task.CompletedTask;
        }

        void App_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var app = (FamilyApp)button.DataContext;
            app.OpenAsync(app.ExplorerType, Manager).Wait();
        }

        internal void OnReady()
        {
            if (!string.IsNullOrEmpty(Config.ForcePath) && Config.ForcePath.StartsWith("app:") && FamilyApps != null && FamilyApps.TryGetValue(Config.ForcePath[4..], out var app))
                App_Click(new Button { DataContext = app }, null);
            OpenPage_Click(null, null);
        }

        #region Menu

        void OpenPage_Click(object sender, RoutedEventArgs e)
        {
            var openPage = new OpenPage();
            if (openPage.ShowDialog() == true) Instance.Open((Family)openPage.Family.SelectedItem, openPage.PakUris);
        }

        void OptionsPage_Click(object sender, RoutedEventArgs e)
        {
            var optionsPage = new OptionsPage();
            optionsPage.ShowDialog();
        }

        void WorldMap_Click(object sender, RoutedEventArgs e)
        {
            //if (DatManager.CellDat == null || DatManager.PortalDat == null) return;
            //EngineView.ViewMode = ViewMode.Map;
        }

        void AboutPage_Click(object sender, RoutedEventArgs e)
        {
            var aboutPage = new AboutPage();
            aboutPage.ShowDialog();
        }

        void Guide_Click(object sender, RoutedEventArgs e)
        {
            //Process.Start(@"docs\index.html");
        }

        #endregion
    }
}
