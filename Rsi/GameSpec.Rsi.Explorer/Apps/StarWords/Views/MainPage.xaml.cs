using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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

namespace GameSpec.Rsi.Apps.StarWords.Views
{
    /// <summary>
    /// ContentTab
    /// </summary>
    public class ContentTab
    {
        public string Name { get; set; }
        public object Document { get; set; }
    }

    /// <summary>
    /// Interaction logic for MainPage.xaml
    /// </summary>
    public partial class MainPage : Window, INotifyPropertyChanged
    {
        public static MainPage Instance;
        public StarWordsApp App;

        public MainPage(StarWordsApp app)
        {
            InitializeComponent();
            Instance = this;
            DataContext = this;
            App = app;
            Navigator.Nodes = new ObservableCollection<Node>(app.Db.Nodes);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        IList<ContentTab> _contentTabs;
        public IList<ContentTab> ContentTabs
        {
            get => _contentTabs;
            set { _contentTabs = value; OnPropertyChanged(); }
        }
    }
}
