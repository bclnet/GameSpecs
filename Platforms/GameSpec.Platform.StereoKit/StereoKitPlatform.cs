using OpenStack;

namespace GameSpec
{
    public static class StereoKitPlatform
    {
        public static unsafe bool Startup()
        {
            try
            {
                FamilyPlatform.Platform = FamilyPlatform.Type.StereoKit;
                FamilyPlatform.GraphicFactory = source => new StereoKitGraphic(source);
                Debug.AssertFunc = x => System.Diagnostics.Debug.Assert(x);
                Debug.LogFunc = a => System.Diagnostics.Debug.Print(a);
                Debug.LogFormatFunc = (a, b) => System.Diagnostics.Debug.Print(a, b);
                return true;
            }
            catch { return false; }
        }
    }
}