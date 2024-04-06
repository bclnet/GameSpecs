using System.Collections.Generic;
using System.Numerics;

namespace GameX.Formats.Unknown
{
    public interface IUnknownSkin
    {
        public struct BoneMap
        {
            public int[] BoneIndex;
            public int[] Weight; // Byte / 256?
        }

        public struct IntVertex
        {
            public Vector3 Obsolete0;
            public Vector3 Position;
            public Vector3 Obsolete2;
            public ushort[] BoneIDs; // 4 bone IDs
            public float[] Weights; // Should be 4 of these
            public object Color;
        }

        bool HasSkinningInfo { get; }
        ICollection<IUnknownBone> CompiledBones { get; }
        IntVertex[] IntVertexs { get; }
        BoneMap[] BoneMaps { get; }
        ushort[] Ext2IntMaps { get; }
    }
}
