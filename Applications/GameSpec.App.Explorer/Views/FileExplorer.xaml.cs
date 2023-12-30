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
        public static MetadataManager Resource = new ResourceManager();
        public static FileExplorer Instance;

        public FileExplorer()
        {
            InitializeComponent();
            Instance = this;
            DataContext = this;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public static readonly DependencyProperty PakFileProperty = DependencyProperty.Register(nameof(PakFile), typeof(PakFile), typeof(FileExplorer),
            new PropertyMetadata((d, e) =>
            {
                if (d is not FileExplorer fileExplorer || e.NewValue is not PakFile pakFile) return;
                fileExplorer.Filters = pakFile.GetMetadataFiltersAsync(Resource).Result;
                fileExplorer.Nodes = new ObservableCollection<MetadataItem>(fileExplorer.PakNodes = pakFile.GetMetadataItemsAsync(Resource).Result.ToList());
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
            if (node != null && node.Source is FileSource z) z.Pak?.Open(node.Items, manager);
            return paths.Length == 1 ? node : node?.FindByPath(paths[1], manager);
        }

        List<MetadataItem.Filter> _filters;
        public List<MetadataItem.Filter> Filters
        {
            get => _filters;
            set { _filters = value; OnPropertyChanged(); }
        }

        void OnFilterKeyUp(object sender, KeyEventArgs e)
        {
            if (string.IsNullOrEmpty(Filter.Text)) Nodes = new ObservableCollection<MetadataItem>(PakNodes);
            else Nodes = new ObservableCollection<MetadataItem>(PakNodes.Select(x => x.Search(y => y.Name.Contains(Filter.Text))).Where(x => x != null));
            //var view = (CollectionView)CollectionViewSource.GetDefaultView(Node.ItemsSource);
            //view.Filter = o =>
            //{
            //    if (string.IsNullOrEmpty(Filter.Text)) return true;
            //    else return (o as MetadataItem).Name.Contains(Filter.Text);
            //};
            //view.Refresh();
        }

        void OnFilterSelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count <= 0) return;
            var filter = e.AddedItems[0] as MetadataItem.Filter;
            if (string.IsNullOrEmpty(Filter.Text)) Nodes = new ObservableCollection<MetadataItem>(PakNodes);
            else Nodes = new ObservableCollection<MetadataItem>(PakNodes.Select(x => x.Search(y => y.Name.Contains(filter.Description))).Where(x => x != null));
        }

        List<MetadataItem> PakNodes;

        ObservableCollection<MetadataItem> _nodes;
        public ObservableCollection<MetadataItem> Nodes
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
                        OnFilterKeyUp(null, null);
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

        void OnNodeSelected(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is TreeViewItem item && item.Items.Count > 0) (item.Items[0] as TreeViewItem).IsSelected = true;
            else if (e.NewValue is MetadataItem itemNode && itemNode.PakFile != null && SelectedItem != itemNode) SelectedItem = itemNode;
            e.Handled = true;
        }

        void OnReady()
        {
            //SelectedItem = string.IsNullOrEmpty(OpenPath) ? null : FindByPath(OpenPath, Resource);
            if (!string.IsNullOrEmpty(Config.ForcePath) && !Config.ForcePath.StartsWith("app:")) SelectedItem = FindByPath(Config.ForcePath, Resource);
        }
    }
}
