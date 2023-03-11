using GameSpec.Metadata;
using System.Windows.Input;

namespace GameSpec.App.Explorer.Views
{
    public partial class FileExplorer : ContentView
    {
        public static MetadataManager Resource = new ResourceManagerProvider();
        public static FileExplorer Instance;

        public FileExplorer()
        {
            InitializeComponent();
            Instance = this;
            BindingContext = this;
        }

        public static readonly BindableProperty OpenPathProperty = BindableProperty.Create(nameof(OpenPath), typeof(string), typeof(FileExplorer), null);
        public string OpenPath
        {
            get => (string)GetValue(OpenPathProperty);
            set => SetValue(OpenPathProperty, value);
        }

        public static readonly BindableProperty PakFileProperty = BindableProperty.Create(nameof(PakFile), typeof(PakFile), typeof(FileExplorer),
            propertyChanged: (d, e, n) =>
            {
                if (d is not FileExplorer fileExplorer || n is not PakFile pakFile) return;
                fileExplorer.NodeFilters = pakFile.GetMetadataItemFiltersAsync(Resource).Result;
                fileExplorer.Nodes = fileExplorer.PakNodes = pakFile.GetMetadataItemsAsync(Resource).Result;
                fileExplorer.SelectedItem = string.IsNullOrEmpty(fileExplorer.OpenPath) ? null : fileExplorer.FindByPath(fileExplorer.OpenPath);
                fileExplorer.OnReady();
            });
        public PakFile PakFile
        {
            get => (PakFile)GetValue(PakFileProperty);
            set => SetValue(PakFileProperty, value);
        }

        public MetadataItem FindByPath(string path)
        {
            var paths = path.Split(new[] { '\\', '/', ':' }, 2);
            var node = PakNodes.FirstOrDefault(x => x.Name == paths[0]);
            return paths.Length == 1 ? node : node?.FindByPath(paths[1]);
        }

        public static readonly BindableProperty NodeFiltersProperty = BindableProperty.Create(nameof(NodeFilters), typeof(List<MetadataItem.Filter>), typeof(FileExplorer), null);
        public List<MetadataItem.Filter> NodeFilters
        {
            get => (List<MetadataItem.Filter>)GetValue(NodeFiltersProperty);
            set => SetValue(NodeFiltersProperty, value);
        }

        //void NodeFilter_KeyUp(object sender, EventArgs e)
        //{
        //    if (string.IsNullOrEmpty(NodeFilter.SelectedItem as string)) Nodes = PakNodes;
        //    else Nodes = PakNodes.Select(x => x.Search(y => y.Name.Contains(NodeFilter.SelectedItem as string))).ToList();
        //    //var view = (CollectionView)CollectionViewSource.GetDefaultView(Node.ItemsSource);
        //    //view.Filter = o =>
        //    //{
        //    //    if (string.IsNullOrEmpty(NodeFilter.Text)) return true;
        //    //    else return (o as MetadataItem).Name.Contains(NodeFilter.Text);
        //    //};
        //    //view.Refresh();
        //}

        //void NodeFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    if (e.AddedItems.Count <= 0) return;
        //    var filter = e.AddedItems[0] as MetadataItem.Filter;
        //    if (string.IsNullOrEmpty(NodeFilter.Text)) Nodes = PakNodes;
        //    else Nodes = PakNodes.Select(x => x.Search(y => y.Name.Contains(filter.Description))).ToList();
        //}

        List<MetadataItem> PakNodes;

        public static readonly BindableProperty NodesProperty = BindableProperty.Create(nameof(Nodes), typeof(List<MetadataItem>), typeof(FileExplorer), null);
        public List<MetadataItem> Nodes
        {
            get => (List<MetadataItem>)GetValue(NodesProperty);
            set => SetValue(NodesProperty, value);
        }

        void OnNodeTapped(object s, EventArgs args)
        {
            var parameter = ((TappedEventArgs)args).Parameter;
            if (parameter is MetadataItem item && item.PakFile != null) SelectedItem = item;
        }

        MetadataItem _selectedItem;
        public MetadataItem SelectedItem
        {
            get => _selectedItem;
            set
            {
                if (_selectedItem == value) return;
                _selectedItem = value;
                OnFileInfo(value?.PakFile?.GetMetadataInfosAsync(Resource, value).Result);
            }
        }

        public void OnFileInfo(List<MetadataInfo> infos)
        {
            FileContent.Instance.OnFileInfo(PakFile, infos?.Where(x => x.Name == null).ToList());
            FileInfo.Infos = infos?.Where(x => x.Name != null).ToList();
        }

        void OnReady()
        {
            if (!string.IsNullOrEmpty(Config.ForcePath)) SelectedItem = FindByPath(Config.ForcePath);
        }
    }
}