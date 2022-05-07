using GameSpec.Explorer;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;

namespace GameSpec.AC.Formats.Entity
{
    public class SkyObject : IGetExplorerInfo
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
        List<ExplorerInfoNode> IGetExplorerInfo.GetInfoNodes(ExplorerManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<ExplorerInfoNode> {
                BeginTime != 0 ? new ExplorerInfoNode($"BeginTime: {BeginTime}") : null,
                EndTime != 0 ? new ExplorerInfoNode($"EndTime: {EndTime}") : null,
                BeginAngle != 0 ? new ExplorerInfoNode($"BeginAngle: {BeginAngle}") : null,
                EndAngle != 0 ? new ExplorerInfoNode($"EndAngle: {EndAngle}") : null,
                TexVelocityX != 0 ? new ExplorerInfoNode($"TexVelocityX: {TexVelocityX}") : null,
                TexVelocityY != 0 ? new ExplorerInfoNode($"TexVelocityY: {TexVelocityY}") : null,
                DefaultGFXObjectId != 0 ? new ExplorerInfoNode($"DefaultGFXObjectId: {DefaultGFXObjectId:X8}", clickable: true) : null,
                DefaultPESObjectId != 0 ? new ExplorerInfoNode($"DefaultPESObjectId: {DefaultPESObjectId:X8}", clickable: true) : null,
                Properties != 0 ? new ExplorerInfoNode($"Properties: {Properties:X}") : null,
            };
            return nodes;
        }
    }
}
