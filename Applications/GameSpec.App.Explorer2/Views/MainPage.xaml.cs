using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace GameSpec.App.Explorer.Views
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

    public partial class MainPage : ContentPage, INotifyPropertyChanged
    {
        public static MainPage Instance;

        public MainPage()
        {
            InitializeComponent();
            Instance = this;
            BindingContext = this;
        }

        internal void OnFirstLoad() => OpenPage_Click(null, null);

        public new event PropertyChangedEventHandler PropertyChanged;
        void NotifyPropertyChanged([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

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

        IList<ExplorerMainTab> _mainTabs;
        public IList<ExplorerMainTab> MainTabs
        {
            get => _mainTabs;
            set { _mainTabs = value; NotifyPropertyChanged(); }
        }

        public readonly IList<PakFile> PakFiles = new List<PakFile>();

        // https://dev.to/davidortinau/making-a-tabbar-or-segmentedcontrol-in-net-maui-54ha
        public Task OnOpenedAsync(string path = null)
        {
            //MainTabControl.SelectedIndex = 0;
            var tabs = PakFiles.Where(x => x != null).Select(pakFile => new ExplorerMainTab
            {
                Name = pakFile.Name,
                PakFile = pakFile,
                Text = "Example",
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
            Navigation.PushModalAsync(openPage).Wait();
        }

        void OptionsPage_Click(object sender, EventArgs e)
        {
            var optionsPage = new OptionsPage();
            Navigation.PushModalAsync(optionsPage).Wait();
        }

        void WorldMap_Click(object sender, EventArgs e)
        {
            //if (DatManager.CellDat == null || DatManager.PortalDat == null) return;
            //EngineView.ViewMode = ViewMode.Map;
        }

        void AboutPage_Click(object sender, EventArgs e)
        {
            var aboutPage = new AboutPage();
            Navigation.PushModalAsync(aboutPage).Wait();
        }

        void Guide_Click(object sender, EventArgs e)
        {
            //Process.Start(@"docs\index.html");
        }

        #endregion
    }
}