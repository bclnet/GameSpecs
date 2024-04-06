using GameX.Cig.Apps.Subsumption.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace GameX.Cig.Apps.Subsumption
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : ResourceDictionary
    {
        void Application_Startup(object sender, StartupEventArgs e)
        {
            var page = new MainPage((SubsumptionApp)sender);
            page.Show();
        }
    }
}
