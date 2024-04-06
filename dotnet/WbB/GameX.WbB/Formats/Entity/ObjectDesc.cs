using GameX.Meta;
using GameX.Formats;
using System.Collections.Generic;
using System.IO;

namespace GameX.WbB.Formats.Entity
{
    public class ObjectDesc : IHaveMetaInfo
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
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                new MetaInfo($"Object ID: {ObjId:X8}", clickable: true),
                new MetaInfo($"BaseLoc: {BaseLoc}"),
                new MetaInfo($"Frequency: {Freq}"),
                new MetaInfo($"DisplaceX: {DisplaceX} DisplaceY: {DisplaceY}"),
                new MetaInfo($"MinScale: {MinScale} MaxScale: {MaxScale}"),
                new MetaInfo($"MaxRotation: {MaxRotation}"),
                new MetaInfo($"MinSlope: {MinSlope} MaxSlope: {MaxSlope}"),
                Align != 0 ? new MetaInfo($"Align: {Align}") : null,
                Orient != 0 ? new MetaInfo($"Orient: {Orient}") : null,
                WeenieObj != 0 ? new MetaInfo($"WeenieObj: {WeenieObj}") : null,
            };
            return nodes;
        }
    }
}
