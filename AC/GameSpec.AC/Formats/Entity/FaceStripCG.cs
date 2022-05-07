using GameSpec.Explorer;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;

namespace GameSpec.AC.Formats.Entity
{
    public class FaceStripCG : IGetExplorerInfo
    {
        public readonly uint IconImage;
        public readonly ObjDesc ObjDesc;

        public FaceStripCG(BinaryReader r)
        {
            IconImage = r.ReadUInt32();
            ObjDesc = new ObjDesc(r);
        }

        //: Entity.FaceStripCG
        List<ExplorerInfoNode> IGetExplorerInfo.GetInfoNodes(ExplorerManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<ExplorerInfoNode> {
                IconImage != 0 ? new ExplorerInfoNode($"Icon: {IconImage:X8}", clickable: true) : null,
                new ExplorerInfoNode("ObjDesc", items: (ObjDesc as IGetExplorerInfo).GetInfoNodes()),
            };
            return nodes;
        }
    }
}
