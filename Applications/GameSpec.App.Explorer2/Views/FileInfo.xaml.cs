using GameSpec.Metadata;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TreeView.Maui.Core;

namespace GameSpec.App.Explorer.Views
{
    public partial class FileInfo : ContentView, INotifyPropertyChanged
    {
        public static FileInfo Instance;

        public FileInfo()
        {
            InitializeComponent();
            Instance = this;
            BindingContext = this;
        }

        public new event PropertyChangedEventHandler PropertyChanged;
        void NotifyPropertyChanged([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        List<MetadataInfo> _infos;
        public List<MetadataInfo> Infos
        {
            get => _infos;
            set { _infos = value; NotifyPropertyChanged(); }
        }
    }
}