using GameX.Meta;
using GameX.Formats;
using System.Collections.Generic;
using System.IO;

namespace GameX.WbB.Formats.Entity
{
    public class SkyObject : IHaveMetaInfo
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
            Properties = r.ReadUInt32(); r.Align();
        }

        //: Entity.SkyObject
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                BeginTime != 0 ? new MetaInfo($"BeginTime: {BeginTime}") : null,
                EndTime != 0 ? new MetaInfo($"EndTime: {EndTime}") : null,
                BeginAngle != 0 ? new MetaInfo($"BeginAngle: {BeginAngle}") : null,
                EndAngle != 0 ? new MetaInfo($"EndAngle: {EndAngle}") : null,
                TexVelocityX != 0 ? new MetaInfo($"TexVelocityX: {TexVelocityX}") : null,
                TexVelocityY != 0 ? new MetaInfo($"TexVelocityY: {TexVelocityY}") : null,
                DefaultGFXObjectId != 0 ? new MetaInfo($"DefaultGFXObjectId: {DefaultGFXObjectId:X8}", clickable: true) : null,
                DefaultPESObjectId != 0 ? new MetaInfo($"DefaultPESObjectId: {DefaultPESObjectId:X8}", clickable: true) : null,
                Properties != 0 ? new MetaInfo($"Properties: {Properties:X}") : null,
            };
            return nodes;
        }
    }
}
