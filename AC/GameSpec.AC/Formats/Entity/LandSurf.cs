using GameSpec.Explorer;
using GameSpec.Formats;
using System;
using System.Collections.Generic;
using System.IO;

namespace GameSpec.AC.Formats.Entity
{
    public class LandSurf : IGetExplorerInfo
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
        List<ExplorerInfoNode> IGetExplorerInfo.GetInfoNodes(ExplorerManager resource, FileMetadata file, object tag) => (TexMerge as IGetExplorerInfo).GetInfoNodes(resource, file, tag);
    }
}
