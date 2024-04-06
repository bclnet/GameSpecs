using GameX.Meta;
using GameX.Formats;
using System;
using System.Collections.Generic;
using System.IO;

namespace GameX.WbB.Formats.Entity
{
    public class LandSurf : IHaveMetaInfo
    {
        public readonly uint Type;
        //public readonly PalShift PalShift; // This is used if Type == 1 (which we haven't seen yet)
        public readonly TexMerge TexMerge;

        public LandSurf(BinaryReader r)
        {
            Type = r.ReadUInt32(); // This is always 0
            if (Type == 1) throw new FormatException("Type value unknown");
            TexMerge = new TexMerge(r);
        }

        //: Entity.LandSurf
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag) => (TexMerge as IHaveMetaInfo).GetInfoNodes(resource, file, tag);
    }
}
