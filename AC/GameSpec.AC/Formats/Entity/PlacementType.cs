using GameSpec.Metadata;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;

namespace GameSpec.AC.Formats.Entity
{
    public class PlacementType : IGetMetadataInfo
    {
        public readonly AnimationFrame AnimFrame;

        public PlacementType(BinaryReader r, uint numParts)
           =>  AnimFrame = new AnimationFrame(r, numParts);

        //: Entity.PlacementType
        List<MetadataInfo> IGetMetadataInfo.GetInfoNodes(MetadataManager resource, FileMetadata file, object tag) => (AnimFrame as IGetMetadataInfo).GetInfoNodes(resource, file, tag);
    }
}
