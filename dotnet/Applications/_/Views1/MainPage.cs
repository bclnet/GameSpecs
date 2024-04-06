using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GameX.App.Explorer.Views
{
    /// <summary>
    /// ExplorerMainTab
    /// </summary>
    public class ExplorerMainTab
    {
        public string Name { get; set; }
        public PakFile PakFile { get; set; }
        public string Text { get; set; }
        public string OpenPath { get; set; }
    }

    public partial class MainPage : ContentPage
    {
        public static MainPage Instance;

        public MainPage()
        {
            InitializeComponent();
            Instance = this;
            BindingContext = this;
        }

        // https://dev.to/davidortinau/making-a-tabbar-or-segmentedcontrol-in-net-maui-54ha
        //void MainTab_Changed(object sender, CheckedChangedEventArgs e) => MainTabContent.BindingContext = ((RadioButton)sender).BindingContext;

        internal void OnFirstLoad() => OpenPage_Click(null, null);

        public void Open(Family family, IEnumerable<Uri> pakUris, string path = null)
        {
            foreach (var pakFile in PakFiles) pakFile?.Dispose();
            PakFiles.Clear();
            if (family == null) return;
            foreach (var pakUri in pakUris)
            {
                Status.WriteLine($"Opening {pakUri}");
                PakFiles.Add(family.OpenPakFile(pakUri));
            }
            Status.WriteLine("Done");
            OnOpenedAsync(path).Wait();
        }

        public static readonly BindableProperty MainTabsProperty = BindableProperty.Create(nameof(MainTabs), typeof(IList<ExplorerMainTab>), typeof(MainPage),
            propertyChanged: (d, e, n) =>
            {
                var mainTab = ((MainPage)d).MainTab;
                //var firstTab = mainTab.Children.FirstOrDefault() as RadioButton;
                //if (firstTab != null) firstTab.IsChecked = true;
            });
        public IList<ExplorerMainTab> MainTabs
        {
            get => (IList<ExplorerMainTab>)GetValue(MainTabsProperty);
            set => SetValue(MainTabsProperty, value);
        }

        public readonly IList<PakFile> PakFiles = new List<PakFile>();

        public Task OnOpenedAsync(string path = null)
        {
            var tabs = PakFiles.Where(x => x != null).Select(pakFile => new ExplorerMainTab
            {
                Name = pakFile.Name,
                PakFile = pakFile,
                OpenPath = path,
            }).ToList();
            tabs.Add(new ExplorerMainTab
            {
                Name = "Information",
                Text = @"Leverage agile frameworks to provide a robust synopsis for high level overviews. Iterative approaches to corporate strategy foster collaborative thinking to further the overall value proposition. Organically grow the holistic world view of disruptive innovation via workplace diversity and empowerment.",
            });
            MainTabs = tabs;
            return Task.CompletedTask;
        }

        #region Menu

        void OpenPage_Click(object sender, EventArgs e)
        {
            var openPage = new OpenPage();
            //Navigation.PushModalAsync(openPage).Wait();
        }

        void OptionsPage_Click(object sender, EventArgs e)
        {
            var optionsPage = new OptionsPage();
            //Navigation.PushModalAsync(optionsPage).Wait();
        }

        void WorldMap_Click(object sender, EventArgs e)
        {
            //if (DatManager.CellDat == null || DatManager.PortalDat == null) return;
            //EngineView.ViewMode = ViewMode.Map;
        }

        void AboutPage_Click(object sender, EventArgs e)
        {
            var aboutPage = new AboutPage();
            //Navigation.PushModalAsync(aboutPage).Wait();
        }

        void Guide_Click(object sender, EventArgs e)
        {
            //Process.Start(@"docs\index.html");
        }

        #endregion
    }
}
