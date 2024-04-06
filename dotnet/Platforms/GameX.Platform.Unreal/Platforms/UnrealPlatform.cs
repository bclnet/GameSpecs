using OpenStack;
using FDebug = UnrealEngine.Framework.Debug;
using FLogLevel = UnrealEngine.Framework.LogLevel;

namespace GameX.Platforms
{
    public static class UnrealPlatform
    {
        public static unsafe bool Startup()
        {
            FDebug.Log(FLogLevel.Display, "Startup");
            try
            {
                Platform.PlatformType = Platform.Type.Unreal;
                Platform.GraphicFactory = source => new UnrealGraphic(source);
                Debug.AssertFunc = x => System.Diagnostics.Debug.Assert(x);
                Debug.LogFunc = a => FDebug.Log(FLogLevel.Display, a);
                Debug.LogFormatFunc = (a, b) => FDebug.Log(FLogLevel.Display, string.Format(a, b));
                FDebug.Log(FLogLevel.Display, "Startup:GOOD");
                return true;
            }
            catch { return false; }
        }
    }
}