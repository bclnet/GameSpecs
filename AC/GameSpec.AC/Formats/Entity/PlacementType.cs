using GameSpec.Explorer;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;

namespace GameSpec.AC.Formats.Entity
{
    public class PlacementType : IGetExplorerInfo
    {
        public readonly AnimationFrame AnimFrame;

        public PlacementType(BinaryReader r, uint numParts)
           =>  AnimFrame = new AnimationFrame(r, numParts);

        //: Entity.PlacementType
        List<ExplorerInfoNode> IGetExplorerInfo.GetInfoNodes(ExplorerManager resource, FileMetadata file, object tag) => (AnimFrame as IGetExplorerInfo).GetInfoNodes(resource, file, tag);
    }
}
