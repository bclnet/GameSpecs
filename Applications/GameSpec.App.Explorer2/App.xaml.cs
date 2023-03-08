using CommandLine;
using static Microsoft.Maui.ApplicationModel.Permissions;

namespace GameSpec.App.Explorer
{
    public partial class App : Application
    {
        public static App Instance;

        static App() => FamilyPlatform.Startups.Add(OpenGLPlatform.Startup);

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

        public App()
        {
            InitializeComponent();
            Instance = this;
            MainPage = new AppShell();
        }

        public static void Startup()
        {
            var status = CheckAndRequestPermission<StorageWrite>().Result;
            Console.WriteLine(status);
            if (status == PermissionStatus.Granted) status = CheckAndRequestPermission<StorageRead>().Result;
            Console.WriteLine(status);
            if (status != PermissionStatus.Granted)
            {
                Instance.MainPage.DisplayAlert("Prompt", $"NO ACCESS", "Cancel").Wait();
                return;
            }
            Parser.Default.ParseArguments<DefaultOptions, TestOptions, OpenOptions>(args)
            .MapResult(
                (DefaultOptions opts) => Instance.RunDefault(opts),
                (TestOptions opts) => Instance.RunTest(opts),
                (OpenOptions opts) => Instance.RunOpen(opts),
                errs => Instance.RunError(errs));
        }

        async static Task<PermissionStatus> CheckAndRequestPermission<TPermission>() where TPermission : BasePermission, new()
        {
            var status = await CheckStatusAsync<TPermission>();
            if (status == PermissionStatus.Granted) return status;
            else if (status == PermissionStatus.Denied && DeviceInfo.Platform == DevicePlatform.iOS)
            {
                await Instance.MainPage.DisplayAlert("Prompt", $"turn on in settings", "Cancel");
                return status;
            }
            else if (ShouldShowRationale<TPermission>())
            {
                await Instance.MainPage.DisplayAlert("Prompt", "Why the permission is needed", "Cancel");
            }
            status = await RequestAsync<TPermission>();
            return status;
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

        int RunDefault(DefaultOptions opts)
        {
            var page = (AppShell)MainPage;
            page.OnFirstLoad().Wait();
            return 0;
        }

        int RunTest(TestOptions opts)
        {
            var page = (AppShell)MainPage;
            page.OnFirstLoad().Wait();
            return 0;
        }

        int RunOpen(OpenOptions opts)
        {
            var page = (AppShell)MainPage;
            var family = FamilyManager.GetFamily(opts.Family);
            //var wnd = new MainWindow(false);
            //MainPage.Open(family, new[] { opts.Uri }, opts.Path);
            //MainPage.Show();
            return 0;
        }

        int RunError(IEnumerable<Error> errs)
        {
            MainPage.DisplayAlert("Alert", $"Errors: \n\n {errs.First()}", "Cancel").Wait();
            //Current.Shutdown(1);
            return 1;
        }
    }
}