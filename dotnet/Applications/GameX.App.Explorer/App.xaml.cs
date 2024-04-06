using CommandLine;
using GameX.App.Explorer.Views;
using GameX.Platforms;
using OpenStack.Graphics.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

// https://www.wpf-tutorial.com/data-binding/debugging/
namespace GameX.App.Explorer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        static App() => Platform.Startups.Add(OpenGLPlatform.Startup);

        static string[] args = new string[0];
        //static string[] args = new string[] { "open", "-e", "AC", "-u", "game:/client_portal.dat#AC", "-p", "01000001.obj" };
        //static string[] args = new string[] { "open", "-e", "AC", "-u", "game:/client_portal.dat#AC", "-p", "02000001.set" };
        //static string[] args = new string[] { "open", "-e", "AC", "-u", "game:/client_portal.dat#AC", "-p", "03000001.obj" };
        //static string[] args = new string[] { "open", "-e", "AC", "-u", "game:/client_portal.dat#AC", "-p", "0400008E.pal" };
        //static string[] args = new string[] { "open", "-e", "Red", "-u", "game:/basegame_2_mainmenu.archive#CP77" };
        //static string[] args = new string[] { "open", "-e", "Red", "-u", "game:/basegame_1_engine.archive#CP77" };
        //static string[] args = new string[] { "open", "-e", "Red", "-u", "game:/lang_en_text.archive#CP77" };
        //static string[] args = new string[] { "open", "-e", "Valve", "-u", "game:/dota/pak01_dir.vpk#Dota2", "-p", "materials/models/npc_minions/siege1_color_psd_12a9c12b.vtex_c" };
        //static string[] args = new string[] { "open", "-e", "Valve", "-u", "game:/dota/pak01_dir.vpk#Dota2", "-p", "materials/models/npc_minions/siege1.vmat_c" };
        //static string[] args = new string[] { "open", "-e", "Valve", "-u", "game:/dota/pak01_dir.vpk#Dota2", "-p", "materials/startup_background_color_png_65ffcfa7.vtex_c" };
        //static string[] args = new string[] { "open", "-e", "Valve", "-u", "game:/dota/pak01_dir.vpk#Dota2", "-p", "materials/startup_background.vmat_c" };
        //static string[] args = new string[] { "open", "-e", "Valve", "-u", "game:/dota/pak01_dir.vpk#Dota2", "-p", "models/npc_minions/draft_siege_good_reference.vmesh_c" };
        //static string[] args = new string[] { "open", "-e", "Valve", "-u", "game:/dota/pak01_dir.vpk#Dota2", "-p", "models/npc_minions/draft_siege_good.vmdl_c" };
        //static string[] args = new string[] { "open", "-e", "Valve", "-u", "game:/dota/pak01_dir.vpk#Dota2", "-p", "particles/hw_fx/candy_carrying_overhead.vpcf_c" };

        void Application_Startup(object sender, StartupEventArgs e)
        {
            //GLViewerControl.ShowConsole = true;
            //var args = e.Args;
            Parser.Default.ParseArguments<DefaultOptions, TestOptions, OpenOptions>(args)
            .MapResult(
                (DefaultOptions opts) => RunDefault(opts),
                (TestOptions opts) => RunTest(opts),
                (OpenOptions opts) => RunOpen(opts),
                errs => RunError(errs));
        }

        #region Options

        [Verb("default", true, HelpText = "Default action.")]
        class DefaultOptions { }

        [Verb("test", HelpText = "Test fixture.")]
        class TestOptions { }

        [Verb("open", HelpText = "Extract files contents to folder.")]
        class OpenOptions
        {
            [Option('f', "family", HelpText = "Family", Required = true)]
            public string Family { get; set; }

            [Option('u', "uri", HelpText = "Pak file to be opened", Required = true)]
            public Uri Uri { get; set; }

            [Option('p', "path", HelpText = "optional file to be opened")]
            public string Path { get; set; }
        }

        #endregion

        static int RunDefault(DefaultOptions opts) { var page = new MainPage(); page.OnReady(); page.Show(); return 0; }

        static int RunTest(TestOptions opts) { var page = new MainPage(); page.OnReady(); page.Show(); return 0; }

        static int RunOpen(OpenOptions opts) { var page = new MainPage().Open(FamilyManager.GetFamily(opts.Family), new[] { opts.Uri }, opts.Path); page.Show(); return 0; }

        static int RunError(IEnumerable<Error> errs) { MessageBox.Show("Errors: \n\n" + errs.First()); Current.Shutdown(1); return 1; }
    }
}
