using GameSpec.WbB.Formats.FileTypes;
using GameSpec.WbB.Formats.Props;
using GameSpec.Metadata;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;

namespace GameSpec.WbB.Formats.Entity
{
    public class SpellComponentBase : IGetMetadataInfo
    {
        public readonly string Name;
        public readonly uint Category;
        public readonly uint Icon;
        public readonly uint Type;
        public readonly uint Gesture;
        public readonly float Time;
        public readonly string Text;
        public readonly float CDM; // Unsure what this is

        public SpellComponentBase(BinaryReader r)
        {
            Name = r.ReadL16StringObfuscated(); r.Align();
            Category = r.ReadUInt32();
            Icon = r.ReadUInt32();
            Type = r.ReadUInt32();
            Gesture = r.ReadUInt32();
            Time = r.ReadSingle();
            Text = r.ReadL16StringObfuscated(); r.Align();
            CDM = r.ReadSingle();
        }

        //: Entity.SpellComponentBase
        List<MetadataInfo> IGetMetadataInfo.GetInfoNodes(MetadataManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<MetadataInfo> {
                new MetadataInfo($"Name: {Name}"),
                new MetadataInfo($"Category: {Category}"),
                new MetadataInfo($"Icon: {Icon:X8}", clickable: true),
                new MetadataInfo($"Type: {(SpellComponentTable.Type)Type}"),
                Gesture != 0x80000000 ? new MetadataInfo($"Gesture: {(MotionCommand)Gesture}") : null,
                new MetadataInfo($"Time: {Time}"),
                !string.IsNullOrEmpty(Text) ? new MetadataInfo($"Text: {Text}") : null,
                new MetadataInfo($"CDM: {CDM}"),
            };
            return nodes;
        }
    }
}
