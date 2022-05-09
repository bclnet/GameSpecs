using GameSpec.Metadata;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;

namespace GameSpec.AC.Formats.Entity
{
    public class SkyObject : IGetMetadataInfo
    {
        public readonly float BeginTime;
        public readonly float EndTime;
        public readonly float BeginAngle;
        public readonly float EndAngle;
        public readonly float TexVelocityX;
        public readonly float TexVelocityY;
        public readonly float TexVelocityZ = 0;
        public readonly uint DefaultGFXObjectId;
        public readonly uint DefaultPESObjectId;
        public readonly uint Properties;

        public SkyObject(BinaryReader r)
        {
            BeginTime = r.ReadSingle();
            EndTime = r.ReadSingle();
            BeginAngle = r.ReadSingle();
            EndAngle = r.ReadSingle();
            TexVelocityX = r.ReadSingle();
            TexVelocityY = r.ReadSingle();
            DefaultGFXObjectId = r.ReadUInt32();
            DefaultPESObjectId = r.ReadUInt32();
            Properties = r.ReadUInt32(); r.AlignBoundary();
        }

        //: Entity.SkyObject
        List<MetadataInfo> IGetMetadataInfo.GetInfoNodes(MetadataManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<MetadataInfo> {
                BeginTime != 0 ? new MetadataInfo($"BeginTime: {BeginTime}") : null,
                EndTime != 0 ? new MetadataInfo($"EndTime: {EndTime}") : null,
                BeginAngle != 0 ? new MetadataInfo($"BeginAngle: {BeginAngle}") : null,
                EndAngle != 0 ? new MetadataInfo($"EndAngle: {EndAngle}") : null,
                TexVelocityX != 0 ? new MetadataInfo($"TexVelocityX: {TexVelocityX}") : null,
                TexVelocityY != 0 ? new MetadataInfo($"TexVelocityY: {TexVelocityY}") : null,
                DefaultGFXObjectId != 0 ? new MetadataInfo($"DefaultGFXObjectId: {DefaultGFXObjectId:X8}", clickable: true) : null,
                DefaultPESObjectId != 0 ? new MetadataInfo($"DefaultPESObjectId: {DefaultPESObjectId:X8}", clickable: true) : null,
                Properties != 0 ? new MetadataInfo($"Properties: {Properties:X}") : null,
            };
            return nodes;
        }
    }
}
