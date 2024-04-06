using OpenStack;
using System;
using System.Threading.Tasks;
using Unity.Collections.LowLevel.Unsafe;

namespace GameX.Platforms
{
    public static class UnityPlatform
    {
        public static unsafe bool Startup()
        {
            var task = Task.Run(() => UnityEngine.Application.platform.ToString());
            try
            {
                Platform.PlatformType = Platform.Type.Unity;
                Platform.PlatformTag = task.Result;
                Platform.GraphicFactory = source => new UnityGraphic(source);
                //Debug.Log(Platform);
                UnsafeX.Memcpy = (dest, src, count) => UnsafeUtility.MemCpy(dest, src, count);
                Debug.AssertFunc = x => UnityEngine.Debug.Assert(x);
                Debug.LogFunc = a => UnityEngine.Debug.Log(a);
                Debug.LogFormatFunc = (a, b) => UnityEngine.Debug.LogFormat(a, b);
                return true;
            }
            catch { return false; }
        }
    }
}