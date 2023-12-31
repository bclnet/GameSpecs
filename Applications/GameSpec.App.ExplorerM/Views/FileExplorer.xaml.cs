using GameSpec.Formats;
using GameSpec.Metadata;

namespace GameSpec.App.Explorer.Views
{
    public partial class FileExplorer : ContentView
    {
        public static MetadataManager Resource = new ResourceManager();
        public static FileExplorer Instance;

        public FileExplorer()
        {
            InitializeComponent();
            Instance = this;
            BindingContext = this;
        }

        public static readonly BindableProperty PakFileProperty = BindableProperty.Create(nameof(PakFile), typeof(PakFile), typeof(FileExplorer),
            propertyChanged: (d, e, n) =>
            {
                if (d is not FileExplorer fileExplorer || n is not PakFile pakFile) return;
                fileExplorer.Filters = pakFile.GetMetadataFiltersAsync(Resource).Result;
                fileExplorer.Nodes = fileExplorer.PakNodes = pakFile.GetMetadataItemsAsync(Resource).Result;
                fileExplorer.OnReady();
            });
        public PakFile PakFile
        {
            get => (PakFile)GetValue(PakFileProperty);
            set => SetValue(PakFileProperty, value);
        }

        List<MetadataItem.Filter> _filters;
        public List<MetadataItem.Filter> Filters
        {
            get => _filters;
            set { _filters = value; OnPropertyChanged(); }
        }

        //void OnFilterKeyUp(object sender, EventArgs e)
        //{
        //    if (string.IsNullOrEmpty(Filter.SelectedItem as string)) Nodes = PakNodes;
        //    else Nodes = PakNodes.Select(x => x.Search(y => y.Name.Contains(Filter.SelectedItem as string))).Where(x => x != null).ToList();
        //    //var view = (CollectionView)CollectionViewSource.GetDefaultView(Node.ItemsSource);
        //    //view.Filter = o =>
        //    //{
        //    //    if (string.IsNullOrEmpty(Filter.Text)) return true;
        //    //    else return (o as MetadataItem).Name.Contains(Filter.Text);
        //    //};
        //    //view.Refresh();
        //}

        void OnFilterSelected(object s, EventArgs e)
        {
            var filter = (MetadataItem.Filter)Filter.SelectedItem;
            if (filter == null) Nodes = PakNodes;
            else Nodes = PakNodes.Select(x => x.Search(y => y.Name.Contains(filter.Description))).Where(x => x != null).ToList();
        }

        List<MetadataItem> PakNodes;

        List<MetadataItem> _nodes;
        public List<MetadataItem> Nodes
        {
            get => _nodes;
            set { _nodes = value; OnPropertyChanged(); }
        }

        List<MetadataInfo> _infos;
        public List<MetadataInfo> Infos
        {
            get => _infos;
            set { _infos = value; OnPropertyChanged(); }
        }

        void OnNodeSelected(object s, EventArgs args)
        {
            var parameter = ((TappedEventArgs)args).Parameter;
            if (parameter is MetadataItem item && item.PakFile != null) SelectedItem = item;
            //e.Handled = true;
        }

        MetadataItem _selectedItem;
        public MetadataItem SelectedItem
        {
            get => _selectedItem;
            set
            {
                if (_selectedItem == value) return;
                _selectedItem = value;
                if (value == null) { OnInfo(); return; }
                try
                {
                    var pak = (value.Source as FileSource)?.Pak;
                    if (pak != null)
                    {
                        if (pak.Status == PakFile.PakStatus.Opened) return;
                        pak.Open(value.Items, Resource);
                        //value.Items.AddRange(pak.GetMetadataItemsAsync(Resource).Result);
                        //OnFilterKeyUp(null, null);
                    }
                    OnInfo(value.PakFile?.GetMetadataInfosAsync(Resource, value).Result);
                }
                catch (Exception ex)
                {
                    OnInfo(new[] {
                        new MetadataInfo($"EXCEPTION: {ex.Message}"),
                        new MetadataInfo(ex.StackTrace),
                    });
                }
            }
        }

        public void OnInfo(IEnumerable<MetadataInfo> infos = null)
        {
            FileContent.Instance.OnInfo(PakFile, infos?.Where(x => x.Name == null).ToList());
            Infos = infos?.Where(x => x.Name != null).ToList();
        }

        void OnReady()
        {
            if (!string.IsNullOrEmpty(Config.ForcePath)) SelectedItem = MetadataItem.FindByPath(PakNodes, Config.ForcePath, Resource);
        }
    }
}