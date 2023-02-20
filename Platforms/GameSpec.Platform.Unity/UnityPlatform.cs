using OpenStack;
using System;
using System.Threading.Tasks;
using Unity.Collections.LowLevel.Unsafe;

namespace GameSpec
{
    public static class UnityPlatform
    {
        public static unsafe bool Startup()
        {
            var task = Task.Run(() => UnityEngine.Application.platform.ToString());
            try
            {
                FamilyPlatform.Platform = FamilyPlatform.Type.Unity;
                FamilyPlatform.PlatformTag = task.Result;
                FamilyPlatform.GraphicFactory = source => new UnityGraphic(source);
                //Debug.Log(Platform);
                UnsafeX.Memcpy = (dest, src, count) => { UnsafeUtility.MemCpy((void*)dest, (void*)src, count); return IntPtr.Zero; };
                Debug.AssertFunc = x => UnityEngine.Debug.Assert(x);
                Debug.LogFunc = a => UnityEngine.Debug.Log(a);
                Debug.LogFormatFunc = (a, b) => UnityEngine.Debug.LogFormat(a, b);
                return true;
            }
            catch { return false; }
        }
    }
}