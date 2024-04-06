using GameX.Metadata;
using Microsoft.Maui.Controls;
using System.Collections.Generic;
using System.ComponentModel;

namespace GameX.App.Explorer.Views
{
    public partial class FileInfo : ContentView
    {
        public static FileInfo Instance;

        public FileInfo()
        {
            InitializeComponent();
            Instance = this;
            BindingContext = this;
        }

        List<MetadataInfo> _infos;
        public List<MetadataInfo> Infos
        {
            get => _infos;
            set { _infos = value; OnPropertyChanged(); }
        }
    }
}
