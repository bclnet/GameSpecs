using GameSpec.Metadata;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;

namespace GameSpec.AC.Formats.Entity
{
    public class AttackCone : IGetMetadataInfo
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
        List<MetadataInfo> IGetMetadataInfo.GetInfoNodes(MetadataManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<MetadataInfo> {
                new MetadataInfo($"PartIndex: {PartIndex}"),
                new MetadataInfo($"LeftX: {LeftX}"),
                new MetadataInfo($"LeftY: {LeftY}"),
                new MetadataInfo($"RightX: {RightX}"),
                new MetadataInfo($"RightY: {RightY}"),
                new MetadataInfo($"Radius: {Radius}"),
                new MetadataInfo($"Height: {Height}"),
            };
            return nodes;
        }
    }
}
