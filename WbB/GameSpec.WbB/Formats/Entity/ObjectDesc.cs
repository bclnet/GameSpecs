using GameSpec.Metadata;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;

namespace GameSpec.WbB.Formats.Entity
{
    public class ObjectDesc : IGetMetadataInfo
    {
        public readonly uint ObjId;
        public readonly Frame BaseLoc;
        public readonly float Freq;
        public readonly float DisplaceX; public readonly float DisplaceY;
        public readonly float MinScale; public readonly float MaxScale;
        public readonly float MaxRotation;
        public readonly float MinSlope; public readonly float MaxSlope;
        public readonly uint Align; public readonly uint Orient;
        public readonly uint WeenieObj;

        public ObjectDesc(BinaryReader r)
        {
            ObjId = r.ReadUInt32();
            BaseLoc = new Frame(r);
            Freq = r.ReadSingle();
            DisplaceX = r.ReadSingle(); DisplaceY = r.ReadSingle();
            MinScale = r.ReadSingle(); MaxScale = r.ReadSingle();
            MaxRotation = r.ReadSingle();
            MinSlope = r.ReadSingle(); MaxSlope = r.ReadSingle();
            Align = r.ReadUInt32(); Orient = r.ReadUInt32();
            WeenieObj = r.ReadUInt32();
        }

        //: Entity.ObjectDesc
        List<MetadataInfo> IGetMetadataInfo.GetInfoNodes(MetadataManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetadataInfo> {
                new MetadataInfo($"Object ID: {ObjId:X8}", clickable: true),
                new MetadataInfo($"BaseLoc: {BaseLoc}"),
                new MetadataInfo($"Frequency: {Freq}"),
                new MetadataInfo($"DisplaceX: {DisplaceX} DisplaceY: {DisplaceY}"),
                new MetadataInfo($"MinScale: {MinScale} MaxScale: {MaxScale}"),
                new MetadataInfo($"MaxRotation: {MaxRotation}"),
                new MetadataInfo($"MinSlope: {MinSlope} MaxSlope: {MaxSlope}"),
                Align != 0 ? new MetadataInfo($"Align: {Align}") : null,
                Orient != 0 ? new MetadataInfo($"Orient: {Orient}") : null,
                WeenieObj != 0 ? new MetadataInfo($"WeenieObj: {WeenieObj}") : null,
            };
            return nodes;
        }
    }
}
