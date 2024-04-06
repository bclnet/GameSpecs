using System.Numerics;

namespace GameX.Formats.Unknown
{
    public interface IUnknownProxy
    {
        public struct Proxy
        {
            public Vector3[] Vertexs;
            public int[] Indexs;
        }

        Proxy[] PhysicalProxys { get; }
    }
}
