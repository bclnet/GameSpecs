using GameSpec.Metadata;
using OpenStack.Graphics;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace GameSpec.App.Explorer.Views
{
    public partial class FileContent : ContentView, INotifyPropertyChanged
    {
        public static FileContent Instance;

        public FileContent()
        {
            InitializeComponent();
            Instance = this;
            BindingContext = this;
        }

        void ContentTab_Changed(object sender, CheckedChangedEventArgs e) => ContentTabContent.BindingContext = ((RadioButton)sender).BindingContext;

        public new event PropertyChangedEventHandler PropertyChanged;
        void NotifyPropertyChanged([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        IOpenGraphic _graphic;
        public IOpenGraphic Graphic
        {
            get => _graphic;
            set { _graphic = value; NotifyPropertyChanged(); }
        }

        public static readonly BindableProperty ContentTabsProperty = BindableProperty.Create(nameof(ContentTabs), typeof(IList<MetadataContent>), typeof(FileContent),
            propertyChanged: (d, e, n) =>
            {
                var contentTab = ((FileContent)d).ContentTab;
                var firstTab = (RadioButton)contentTab.Children.FirstOrDefault();
                if (firstTab != null) firstTab.IsChecked = true;
            });
        public IList<MetadataContent> ContentTabs
        {
            get => (IList<MetadataContent>)GetValue(ContentTabsProperty);
            set => SetValue(ContentTabsProperty, value);
        }

        public void OnFileInfo(PakFile pakFile, List<MetadataInfo> infos)
        {
            if (ContentTabs != null) foreach (var dispose in ContentTabs.Where(x => x.Dispose != null).Select(x => x.Dispose)) dispose.Dispose();
            Graphic = pakFile.Graphic;
            ContentTabs = infos?.Select(x => x.Tag as MetadataContent).Where(x => x != null).ToList();
            //ContentTab.CurrentItem = ContentTabs != null ? ContentTabs.FirstOrDefault() : null;
        }
    }
}