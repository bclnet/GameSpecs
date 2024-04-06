using GameX.Meta;
using GameX.Formats;
using System.Collections.Generic;
using System.IO;

namespace GameX.WbB.Formats.Entity
{
    public class AttackCone : IHaveMetaInfo
    {
        public readonly uint PartIndex;
        // these Left and Right are technically Vec2D types
        public readonly float LeftX; public readonly float LeftY;
        public readonly float RightX; public readonly float RightY;
        public readonly float Radius; public readonly float Height;

        public AttackCone(BinaryReader r)
        {
            PartIndex = r.ReadUInt32();
            LeftX = r.ReadSingle(); LeftY = r.ReadSingle();
            RightX = r.ReadSingle(); RightY = r.ReadSingle();
            Radius = r.ReadSingle(); Height = r.ReadSingle();
        }

        //: Entity.AttackCone
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                new MetaInfo($"PartIndex: {PartIndex}"),
                new MetaInfo($"LeftX: {LeftX}"),
                new MetaInfo($"LeftY: {LeftY}"),
                new MetaInfo($"RightX: {RightX}"),
                new MetaInfo($"RightY: {RightY}"),
                new MetaInfo($"Radius: {Radius}"),
                new MetaInfo($"Height: {Height}"),
            };
            return nodes;
        }
    }
}
