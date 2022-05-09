using GameSpec.Metadata;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;

namespace GameSpec.AC.Formats.Entity
{
    public class SkyObjectReplace : IGetMetadataInfo
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
        List<MetadataInfo> IGetMetadataInfo.GetInfoNodes(MetadataManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<MetadataInfo> {
                new MetadataInfo($"Obj Idx: {ObjectIndex}"),
                GFXObjId != 0 ? new MetadataInfo($"GfxObj ID: {GFXObjId:X8}", clickable: true) : null,
                Rotate != 0 ? new MetadataInfo($"Rotate: {Rotate}") : null,
                Transparent != 0 ? new MetadataInfo($"Transparent: {Transparent}") : null,
                Luminosity != 0 ? new MetadataInfo($"Luminosity: {Luminosity}") : null,
                MaxBright != 0 ? new MetadataInfo($"MaxBright: {MaxBright}") : null,
            };
            return nodes;
        }
    }
}
