using GameX.Metadata;
using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace GameX.App.Explorer.Views
{
    public partial class FileExplorer : ContentView, INotifyPropertyChanged
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

        void OnNodeSelected(object s, EventArgs args)
        {
            //var parameter = ((TappedEventArgs)args).Parameter;
            //if (parameter is MetadataItem item && item.PakFile != null) SelectedItem = item;
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
