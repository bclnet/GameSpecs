using GameSpec.Explorer;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;

namespace GameSpec.AC.Formats.Entity
{
    public class SkyObjectReplace : IGetExplorerInfo
    {
        public readonly uint ObjectIndex;
        public readonly uint GFXObjId;
        public readonly float Rotate;
        public readonly float Transparent;
        public readonly float Luminosity;
        public readonly float MaxBright;

        public SkyObjectReplace(BinaryReader r)
        {
            ObjectIndex = r.ReadUInt32();
            GFXObjId = r.ReadUInt32();
            Rotate = r.ReadSingle();
            Transparent = r.ReadSingle();
            Luminosity = r.ReadSingle();
            MaxBright = r.ReadSingle(); r.AlignBoundary();
        }

        //: Entity.SkyObjectReplace
        List<ExplorerInfoNode> IGetExplorerInfo.GetInfoNodes(ExplorerManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<ExplorerInfoNode> {
                new ExplorerInfoNode($"Obj Idx: {ObjectIndex}"),
                GFXObjId != 0 ? new ExplorerInfoNode($"GfxObj ID: {GFXObjId:X8}", clickable: true) : null,
                Rotate != 0 ? new ExplorerInfoNode($"Rotate: {Rotate}") : null,
                Transparent != 0 ? new ExplorerInfoNode($"Transparent: {Transparent}") : null,
                Luminosity != 0 ? new ExplorerInfoNode($"Luminosity: {Luminosity}") : null,
                MaxBright != 0 ? new ExplorerInfoNode($"MaxBright: {MaxBright}") : null,
            };
            return nodes;
        }
    }
}
