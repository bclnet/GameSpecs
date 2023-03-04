using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace GameSpec.App.Explorer
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
            //if (defaultOpen) MainMenu.OnFirstLoad(); // opens OpenFile dialog
        }

        //public event PropertyChangedEventHandler PropertyChanged;
        void NotifyPropertyChanged([CallerMemberName] string propertyName = "") { } // => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

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
    }
}