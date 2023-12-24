﻿using GameSpec.Formats;
using GameSpec.Metadata;

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

        public static readonly BindableProperty OpenPathProperty = BindableProperty.Create(nameof(OpenPath), typeof(string), typeof(FileExplorer));
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
                fileExplorer.SelectedItem = string.IsNullOrEmpty(fileExplorer.OpenPath) ? null : fileExplorer.FindByPath(fileExplorer.OpenPath, Resource);
                fileExplorer.OnReady();
            });
        public PakFile PakFile
        {
            get => (PakFile)GetValue(PakFileProperty);
            set => SetValue(PakFileProperty, value);
        }

        public MetadataItem FindByPath(string path, MetadataManager manager)
        {
            var paths = path.Split(new[] { '\\', '/', ':' }, 2);
            var node = PakNodes.FirstOrDefault(x => x.Name == paths[0]);
            if (node != null && node.Source is FileSource z) z.Pak?.Open(node.Items, manager);
            return paths.Length == 1 ? node : node?.FindByPath(paths[1], manager);
        }

        List<MetadataItem.Filter> _nodeFilters;
        public List<MetadataItem.Filter> NodeFilters
        {
            get => _nodeFilters;
            set { _nodeFilters = value; OnPropertyChanged(); }
        }

        //void OnNodeFilterKeyUp(object sender, EventArgs e)
        //{
        //    if (string.IsNullOrEmpty(NodeFilter.SelectedItem as string)) Nodes = PakNodes;
        //    else Nodes = PakNodes.Select(x => x.Search(y => y.Name.Contains(NodeFilter.SelectedItem as string))).Where(x => x != null).ToList();
        //    //var view = (CollectionView)CollectionViewSource.GetDefaultView(Node.ItemsSource);
        //    //view.Filter = o =>
        //    //{
        //    //    if (string.IsNullOrEmpty(NodeFilter.Text)) return true;
        //    //    else return (o as MetadataItem).Name.Contains(NodeFilter.Text);
        //    //};
        //    //view.Refresh();
        //}

        void OnNodeFilterSelected(object s, EventArgs e)
        {
            var filter = (MetadataItem.Filter)NodeFilter.SelectedItem;
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
                try
                {
                    OnFileInfo(value?.PakFile?.GetMetadataInfosAsync(Resource, value).Result);
                }
                catch (Exception ex)
                {
                    OnFileInfo(new[] {
                        new MetadataInfo($"EXCEPTION: {ex.Message}"),
                        new MetadataInfo(ex.StackTrace),
                    });
                }
            }
        }

        public void OnFileInfo(IEnumerable<MetadataInfo> infos)
        {
            FileContent.Instance.OnFileInfo(PakFile, infos?.Where(x => x.Name == null).ToList());
            Infos = infos?.Where(x => x.Name != null).ToList();
        }

        void OnReady()
        {
            if (!string.IsNullOrEmpty(Config.ForcePath)) SelectedItem = FindByPath(Config.ForcePath, Resource);
        }
    }
}