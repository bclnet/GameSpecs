using Microsoft.Maui;
using Microsoft.Maui.Hosting;
using StereoKit;
using System.Threading.Tasks;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace GameSpec.App.Explorer.WinUI
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : MauiWinUIApplication
    {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
        }

        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

        static bool running = false;
        internal static void Run()
        {
            if (running) return;
            running = true;

            Task.Run(() =>
            {
                var app = Explorer.App.Instance;
                app.PlatformStartup().Wait();

                // Now pass execution over to StereoKit
                SK.Run(app.Step, () => Log.Info("Done"));
            });
        }
    }
}