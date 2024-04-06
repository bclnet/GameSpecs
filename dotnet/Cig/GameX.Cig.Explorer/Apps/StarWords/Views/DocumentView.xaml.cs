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

namespace GameX.Cig.Apps.StarWords.Views
{
    /// <summary>
    /// Interaction logic for DocumentView.xaml
    /// </summary>
    public partial class DocumentView : UserControl, INotifyPropertyChanged
    {
        public DocumentView()
        {
            InitializeComponent();
            DataContext = this;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public static readonly DependencyProperty DocumentProperty = DependencyProperty.Register(nameof(Document), typeof(object), typeof(DocumentView),
            new PropertyMetadata((d, e) =>
            {
                //if (d is not FileExplorer fileExplorer || e.NewValue is not PakFile pakFile) return;
                //fileExplorer.NodeFilters = pakFile.GetMetaItemFiltersAsync(Resource).Result;
                //fileExplorer.Nodes = new ObservableCollection<MetaItem>(fileExplorer.PakNodes = pakFile.GetMetaItemsAsync(Resource).Result.ToList());
                //fileExplorer.SelectedItem = string.IsNullOrEmpty(fileExplorer.OpenPath) ? null : fileExplorer.FindByPath(fileExplorer.OpenPath);
                //fileExplorer.OnReady();
            }));
        public object Document
        {
            get => (object)GetValue(DocumentProperty);
            set => SetValue(DocumentProperty, value);
        }
    }
}
