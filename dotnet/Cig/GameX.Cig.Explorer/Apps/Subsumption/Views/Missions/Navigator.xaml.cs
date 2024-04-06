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
using GameX.Meta;
using GameX.Formats;
using System.Collections.Specialized;
using System.Collections.ObjectModel;

namespace GameX.Cig.Apps.Subsumption.Views.Missions
{
    /// <summary>
    /// NavigatorItem
    /// </summary>
    public class NavigatorItem
    {
        public string Name { get; set; }
        public List<NavigatorItem> Items { get; set; }
    }

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

        ObservableCollection<NavigatorItem> _nodes;
        public ObservableCollection<NavigatorItem> Nodes
        {
            get => _nodes;
            set { _nodes = value; OnPropertyChanged(); }
        }

        NavigatorItem _selectedItem;
        public NavigatorItem SelectedItem
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
                //        value.Items.AddRange(pak.GetMetaItemsAsync(Resource).Result);
                //        OnNodeFilterKeyUp(null, null);
                //    }
                //    OnFileInfo(value?.PakFile?.GetMetaInfosAsync(Resource, value).Result);
                //}
                //catch (Exception ex)
                //{
                //    OnFileInfo(new[] {
                //        new MetaInfo($"EXCEPTION: {ex.Message}"),
                //        new MetaInfo(ex.StackTrace),
                //    });
                //}
            }
        }

        void OnNodeSelected(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is TreeViewItem item && item.Items.Count > 0) (item.Items[0] as TreeViewItem).IsSelected = true;
            else if (e.NewValue is NavigatorItem itemNode && SelectedItem != itemNode) SelectedItem = itemNode;
            e.Handled = true;
        }
    }
}
