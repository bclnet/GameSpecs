using OpenStack;

namespace GameX.Platforms
{
    public static class VulkenPlatform
    {
        public static unsafe bool Startup()
        {
            try
            {
                Platform.PlatformType = Platform.Type.Vulken;
                Platform.GraphicFactory = source => new VulkenGraphic(source);
                Debug.AssertFunc = x => System.Diagnostics.Debug.Assert(x);
                Debug.LogFunc = a => System.Diagnostics.Debug.Print(a);
                Debug.LogFormatFunc = (a, b) => System.Diagnostics.Debug.Print(a, b);
                return true;
            }
            catch { return false; }
        }
    }
}