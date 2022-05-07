using GameSpec.Explorer;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;

namespace GameSpec.AC.Formats.Entity
{
    public class HairStyleCG : IGetExplorerInfo
    {
        public readonly uint IconImage;
        public readonly bool Bald;
        public readonly uint AlternateSetup;
        public readonly ObjDesc ObjDesc;

        public HairStyleCG(BinaryReader r)
        {
            IconImage = r.ReadUInt32();
            Bald = r.ReadByte() == 1;
            AlternateSetup = r.ReadUInt32();
            ObjDesc = new ObjDesc(r);
        }

        //: Entity.HairStyleCG
        List<ExplorerInfoNode> IGetExplorerInfo.GetInfoNodes(ExplorerManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<ExplorerInfoNode> {
                IconImage != 0 ? new ExplorerInfoNode($"Icon: {IconImage:X8}", clickable: true) : null,
                Bald ? new ExplorerInfoNode($"Bald: True") : null,
                AlternateSetup != 0 ? new ExplorerInfoNode($"Alternate Setup: {AlternateSetup:X8}", clickable: true) : null,
                new ExplorerInfoNode("ObjDesc", items: (ObjDesc as IGetExplorerInfo).GetInfoNodes()),
            };
            return nodes;
        }
    }
}
