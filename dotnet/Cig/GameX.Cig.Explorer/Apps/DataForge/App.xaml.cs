using GameX.Cig.Apps.DataForge.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace GameX.Cig.Apps.DataForge
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : ResourceDictionary
    {
        void Application_Startup(object sender, StartupEventArgs e)
        {
            var page = new MainPage((DataForgeApp)sender);
            page.Show();
        }
    }
}
