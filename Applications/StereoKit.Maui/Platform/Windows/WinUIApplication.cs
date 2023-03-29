using Microsoft.Maui;
using Microsoft.Maui.Handlers;

namespace StereoKit.Maui
{
#if WINDOWS
    [Windows.Foundation.Metadata.ContractVersion(typeof(Microsoft.UI.Xaml.WinUIContract), 65536)]
    [Windows.Foundation.Metadata.MarshalingBehavior(Windows.Foundation.Metadata.MarshalingType.Agile)]
    [Windows.Foundation.Metadata.Threading(Windows.Foundation.Metadata.ThreadingModel.Both)]
#endif
    public class WinUIApplication
    {
#if WINDOWS
        public Microsoft.UI.Xaml.DebugSettings DebugSettings { get; } = Microsoft.UI.Xaml.DebugSettings.FromAbi(0);

        public event Microsoft.UI.Xaml.UnhandledExceptionEventHandler UnhandledException;
#endif

        public void CreatePlatformWindow(IApplication application, OpenWindowRequest? openWindowRequest)
        {
        }

        public void Exit()
        {
        }
    }
}
