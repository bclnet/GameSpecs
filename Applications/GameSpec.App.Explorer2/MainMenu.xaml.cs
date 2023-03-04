namespace GameSpec.App.Explorer
{
    public partial class MainMenu : ContentPage
    {
        public MainMenu() => InitializeComponent();

        public void OnFirstLoad() { } // => OpenFile_Click(null, null);

        //void OpenFile_Click(object sender, RoutedEventArgs e)
        //{
        //    var openDialog = new OpenDialog();
        //    if (openDialog.ShowDialog() == true) MainWindow.Instance.Open((Family)openDialog.Family.SelectedItem, openDialog.PakUris);
        //}

        //void Options_Click(object sender, RoutedEventArgs e)
        //{
        //    var options = new Options();
        //    options.ShowDialog();
        //}

        //void WorldMap_Click(object sender, RoutedEventArgs e)
        //{
        //    //if (DatManager.CellDat == null || DatManager.PortalDat == null) return;
        //    //EngineView.ViewMode = ViewMode.Map;
        //}

        //void About_Click(object sender, RoutedEventArgs e)
        //{
        //    var about = new About();
        //    about.ShowDialog();
        //}

        //void Guide_Click(object sender, RoutedEventArgs e)
        //{
        //    //Process.Start(@"docs\index.html");
        //}
    }
}