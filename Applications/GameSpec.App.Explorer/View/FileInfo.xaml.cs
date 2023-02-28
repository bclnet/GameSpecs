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
using GameSpec.Metadata;

namespace GameSpec.App.Explorer.View
{
    /// <summary>
    /// Interaction logic for FileInfo.xaml
    /// </summary>
    public partial class FileInfo : UserControl, INotifyPropertyChanged
    {
        public static FileInfo Instance;

        public FileInfo()
        {
            InitializeComponent();
            Instance = this;
            DataContext = this;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        void NotifyPropertyChanged([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        List<MetadataInfo> _infos;
        public List<MetadataInfo> Infos
        {
            get => _infos;
            set { _infos = value; NotifyPropertyChanged(); }
        }
    }
}
