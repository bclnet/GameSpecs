using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Media;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Threading.Tasks;
using System.Windows.Data;
using GameSpec.Metadata;
using GameSpec.Formats;
using System.Collections.Specialized;
using System.Collections.ObjectModel;

namespace GameSpec.App.Explorer.Views
{
    /// <summary>
    /// Interaction logic for FileType.xaml
    /// </summary>
    public partial class FileExplorer : UserControl, INotifyPropertyChanged
    {
        public static MetadataManager Resource = new ResourceManagerProvider();
        public static FileExplorer Instance;

        public FileExplorer()
        {
            InitializeComponent();
            Instance = this;
            DataContext = this;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public static readonly DependencyProperty OpenPathProperty = DependencyProperty.Register(nameof(OpenPath), typeof(string), typeof(FileExplorer));
        public string OpenPath
        {
            get => (string)GetValue(OpenPathProperty);
            set => SetValue(OpenPathProperty, value);
        }

        public static readonly DependencyProperty PakFileProperty = DependencyProperty.Register(nameof(PakFile), typeof(PakFile), typeof(FileExplorer),
            new PropertyMetadata((d, e) =>
            {
                if (d is not FileExplorer fileExplorer || e.NewValue is not PakFile pakFile) return;
                fileExplorer.NodeFilters = pakFile.GetMetadataItemFiltersAsync(Resource).Result;
                fileExplorer.Nodes = new ObservableCollection<MetadataItem>(fileExplorer.PakNodes = pakFile.GetMetadataItemsAsync(Resource).Result.ToList());
                fileExplorer.SelectedItem = string.IsNullOrEmpty(fileExplorer.OpenPath) ? null : fileExplorer.FindByPath(fileExplorer.OpenPath, Resource);
                fileExplorer.OnReady();
            }));
        public PakFile PakFile
        {
            get => (PakFile)GetValue(PakFileProperty);
            set => SetValue(PakFileProperty, value);
        }

        public MetadataItem FindByPath(string path, MetadataManager manager)
        {
            var paths = path.Split(new[] { '\\', '/', ':' }, 2);
            var node = PakNodes.FirstOrDefault(x => x.Name == paths[0]);
            return paths.Length == 1 ? node : node?.FindByPath(paths[1], manager);
        }

        List<MetadataItem.Filter> _nodeFilters;
        public List<MetadataItem.Filter> NodeFilters
        {
            get => _nodeFilters;
            set { _nodeFilters = value; OnPropertyChanged(); }
        }

        void OnNodeFilterKeyUp(object sender, KeyEventArgs e)
        {
            if (string.IsNullOrEmpty(NodeFilter.Text)) Nodes = new ObservableCollection<MetadataItem>(PakNodes);
            else Nodes = new ObservableCollection<MetadataItem>(PakNodes.Select(x => x.Search(y => y.Name.Contains(NodeFilter.Text))).Where(x => x != null));
            //var view = (CollectionView)CollectionViewSource.GetDefaultView(Node.ItemsSource);
            //view.Filter = o =>
            //{
            //    if (string.IsNullOrEmpty(NodeFilter.Text)) return true;
            //    else return (o as MetadataItem).Name.Contains(NodeFilter.Text);
            //};
            //view.Refresh();
        }

        void OnNodeFilterSelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count <= 0) return;
            var filter = e.AddedItems[0] as MetadataItem.Filter;
            if (string.IsNullOrEmpty(NodeFilter.Text)) Nodes = new ObservableCollection<MetadataItem>(PakNodes);
            else Nodes = new ObservableCollection<MetadataItem>(PakNodes.Select(x => x.Search(y => y.Name.Contains(filter.Description))).Where(x => x != null));
        }

        List<MetadataItem> PakNodes;

        ObservableCollection<MetadataItem> _nodes;
        public ObservableCollection<MetadataItem> Nodes
        {
            get => _nodes;
            set { _nodes = value; OnPropertyChanged(); }
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
                    var pak = (value?.Source as FileMetadata)?.Pak;
                    if (pak != null)
                    {
                        if (pak.Status == PakFile.PakStatus.Opened) return;
                        pak.Open(value.Items, Resource);
                        //value.Items.AddRange(pak.GetMetadataItemsAsync(Resource).Result);
                        OnNodeFilterKeyUp(null, null);
                    }
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
            FileInfo.Infos = infos?.Where(x => x.Name != null).ToList();
        }

        void OnNodeSelected(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is TreeViewItem item && item.Items.Count > 0) (item.Items[0] as TreeViewItem).IsSelected = true;
            else if (e.NewValue is MetadataItem itemNode && itemNode.PakFile != null && SelectedItem != itemNode) SelectedItem = itemNode;
            e.Handled = true;
        }

        void OnReady()
        {
            if (!string.IsNullOrEmpty(Config.ForcePath) && !Config.ForcePath.StartsWith("app:")) SelectedItem = FindByPath(Config.ForcePath, Resource);
        }
    }
}
