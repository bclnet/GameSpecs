using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using OpenStack.Graphics;
using GameX.Meta;

// https://stackoverflow.com/questions/2783378/wpf-byte-array-to-hex-view-similar-to-notepad-hex-editor-plugin
namespace GameX.App.Explorer.Views
{
    /// <summary>
    /// Interaction logic for FileContent.xaml
    /// </summary>
    public partial class FileContent : UserControl, INotifyPropertyChanged
    {
        public static FileContent Instance;

        public FileContent()
        {
            InitializeComponent();
            Instance = this;
            DataContext = this;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        IOpenGraphic _graphic;
        public IOpenGraphic Graphic
        {
            get => _graphic;
            set { _graphic = value; OnPropertyChanged(); }
        }

        IList<MetaContent> _contentTabs;
        public IList<MetaContent> ContentTabs
        {
            get => _contentTabs;
            set { _contentTabs = value; OnPropertyChanged(); }
        }

        public void OnInfo(PakFile pakFile, List<MetaInfo> infos)
        {
            if (ContentTabs != null) foreach (var dispose in ContentTabs.Where(x => x.Dispose != null).Select(x => x.Dispose)) dispose.Dispose();
            Graphic = pakFile.Graphic;
            ContentTabs = infos?.Select(x => x.Tag as MetaContent).Where(x => x != null).ToList();
            ContentTab.SelectedIndex = ContentTabs != null ? 0 : -1;
        }
    }
}
