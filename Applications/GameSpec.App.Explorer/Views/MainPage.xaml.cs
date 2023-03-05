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

    /// <summary>
    /// Interaction logic for MainPage.xaml
    /// </summary>
    public partial class MainPage : Window, INotifyPropertyChanged
    {
        public static MainPage Instance;

        public MainPage()
        {
            InitializeComponent();
            Instance = this;
            DataContext = this;
        }

        public void OnFirstLoad() => OpenPage_Click(null, null);

        public event PropertyChangedEventHandler PropertyChanged;
        void NotifyPropertyChanged([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public MainPage Open(Family family, IEnumerable<Uri> pakUris, string path = null)
        {
            foreach (var pakFile in PakFiles) pakFile?.Dispose();
            PakFiles.Clear();
            if (family == null) return this;
            foreach (var pakUri in pakUris)
            {
                Status.WriteLine($"Opening {pakUri}");
                PakFiles.Add(family.OpenPakFile(pakUri));
            }
            Status.WriteLine("Done");
            OnOpenedAsync(path).Wait();
            return this;
        }

        IList<ExplorerMainTab> _mainTabs;
        public IList<ExplorerMainTab> MainTabs
        {
            get => _mainTabs;
            set { _mainTabs = value; NotifyPropertyChanged(); }
        }

        public readonly IList<PakFile> PakFiles = new List<PakFile>();

        public Task OnOpenedAsync(string path = null)
        {
            MainTabControl.SelectedIndex = 0;
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
