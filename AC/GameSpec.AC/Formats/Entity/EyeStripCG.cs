using GameSpec.Explorer;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;

namespace GameSpec.AC.Formats.Entity
{
    public class EyeStripCG : IGetExplorerInfo
    {
        public readonly uint IconImage;
        public readonly uint IconImageBald;
        public readonly ObjDesc ObjDesc;
        public readonly ObjDesc ObjDescBald;

        public EyeStripCG(BinaryReader r)
        {
            IconImage = r.ReadUInt32();
            IconImageBald = r.ReadUInt32();
            ObjDesc = new ObjDesc(r);
            ObjDescBald = new ObjDesc(r);
        }

        //: Entity.EyeStripCG
        List<ExplorerInfoNode> IGetExplorerInfo.GetInfoNodes(ExplorerManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<ExplorerInfoNode> {
                IconImage != 0 ? new ExplorerInfoNode($"Icon: {IconImage:X8}", clickable: true) : null,
                IconImageBald != 0 ? new ExplorerInfoNode($"Bald Icon: {IconImageBald:X8}", clickable: true) : null,
                new ExplorerInfoNode("ObjDesc", items: (ObjDesc as IGetExplorerInfo).GetInfoNodes()),
                new ExplorerInfoNode("ObjDescBald", items: (ObjDescBald as IGetExplorerInfo).GetInfoNodes()),
            };
            return nodes;
        }
    }
}
