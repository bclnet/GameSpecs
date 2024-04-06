using GameX.Meta;
using GameX.Formats;
using System.Collections.Generic;
using System.IO;

namespace GameX.WbB.Formats.Entity
{
    public class SkyObjectReplace : IHaveMetaInfo
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
            MaxBright = r.ReadSingle(); r.Align();
        }

        //: Entity.SkyObjectReplace
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                new MetaInfo($"Obj Idx: {ObjectIndex}"),
                GFXObjId != 0 ? new MetaInfo($"GfxObj ID: {GFXObjId:X8}", clickable: true) : null,
                Rotate != 0 ? new MetaInfo($"Rotate: {Rotate}") : null,
                Transparent != 0 ? new MetaInfo($"Transparent: {Transparent}") : null,
                Luminosity != 0 ? new MetaInfo($"Luminosity: {Luminosity}") : null,
                MaxBright != 0 ? new MetaInfo($"MaxBright: {MaxBright}") : null,
            };
            return nodes;
        }
    }
}
