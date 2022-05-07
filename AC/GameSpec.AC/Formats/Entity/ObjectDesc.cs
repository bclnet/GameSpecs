using GameSpec.Explorer;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;

namespace GameSpec.AC.Formats.Entity
{
    public class ObjectDesc : IGetExplorerInfo
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
        List<ExplorerInfoNode> IGetExplorerInfo.GetInfoNodes(ExplorerManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<ExplorerInfoNode> {
                new ExplorerInfoNode($"Object ID: {ObjId:X8}", clickable: true),
                new ExplorerInfoNode($"BaseLoc: {BaseLoc}"),
                new ExplorerInfoNode($"Frequency: {Freq}"),
                new ExplorerInfoNode($"DisplaceX: {DisplaceX} DisplaceY: {DisplaceY}"),
                new ExplorerInfoNode($"MinScale: {MinScale} MaxScale: {MaxScale}"),
                new ExplorerInfoNode($"MaxRotation: {MaxRotation}"),
                new ExplorerInfoNode($"MinSlope: {MinSlope} MaxSlope: {MaxSlope}"),
                Align != 0 ? new ExplorerInfoNode($"Align: {Align}") : null,
                Orient != 0 ? new ExplorerInfoNode($"Orient: {Orient}") : null,
                WeenieObj != 0 ? new ExplorerInfoNode($"WeenieObj: {WeenieObj}") : null,
            };
            return nodes;
        }
    }
}
