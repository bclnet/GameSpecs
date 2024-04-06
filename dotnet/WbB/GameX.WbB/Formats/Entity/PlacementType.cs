using GameX.Meta;
using GameX.Formats;
using System.Collections.Generic;
using System.IO;

namespace GameX.WbB.Formats.Entity
{
    public class PlacementType : IHaveMetaInfo
    {
        public readonly AnimationFrame AnimFrame;

        public PlacementType(BinaryReader r, uint numParts)
           =>  AnimFrame = new AnimationFrame(r, numParts);

        //: Entity.PlacementType
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag) => (AnimFrame as IHaveMetaInfo).GetInfoNodes(resource, file, tag);
    }
}
