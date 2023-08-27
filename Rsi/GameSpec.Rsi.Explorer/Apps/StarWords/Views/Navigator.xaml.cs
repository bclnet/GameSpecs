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

namespace GameSpec.Rsi.Apps.StarWords.Views
{
    /// <summary>
    /// Interaction logic for Navigator.xaml
    /// </summary>
    public partial class Navigator : UserControl, INotifyPropertyChanged
    {
        public Navigator()
        {
            InitializeComponent();
            DataContext = this;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        ObservableCollection<Node> _nodes;
        public ObservableCollection<Node> Nodes
        {
            get => _nodes;
            set { _nodes = value; OnPropertyChanged(); }
        }

        Node _selectedItem;
        public Node SelectedItem
        {
            get => _selectedItem;
            set
            {
                if (_selectedItem == value) return;
                _selectedItem = value;
                //try
                //{
                //    var pak = (value?.Source as FileMetadata)?.Pak;
                //    if (pak != null && pak.Status == PakFile.PakStatus.Closed)
                //    {
                //        pak.Open();
                //        value.Items.AddRange(pak.GetMetadataItemsAsync(Resource).Result);
                //        OnNodeFilterKeyUp(null, null);
                //    }
                //    OnFileInfo(value?.PakFile?.GetMetadataInfosAsync(Resource, value).Result);
                //}
                //catch (Exception ex)
                //{
                //    OnFileInfo(new[] {
                //        new MetadataInfo($"EXCEPTION: {ex.Message}"),
                //        new MetadataInfo(ex.StackTrace),
                //    });
                //}
            }
        }

        void OnNodeSelected(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is TreeViewItem item && item.Items.Count > 0) (item.Items[0] as TreeViewItem).IsSelected = true;
            else if (e.NewValue is Node itemNode && SelectedItem != itemNode) SelectedItem = itemNode;
            e.Handled = true;
        }
    }
}
