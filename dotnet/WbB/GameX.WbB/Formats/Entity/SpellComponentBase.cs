using GameX.WbB.Formats.FileTypes;
using GameX.WbB.Formats.Props;
using GameX.Meta;
using GameX.Formats;
using System.Collections.Generic;
using System.IO;

namespace GameX.WbB.Formats.Entity
{
    public class SpellComponentBase : IHaveMetaInfo
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
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                new MetaInfo($"Name: {Name}"),
                new MetaInfo($"Category: {Category}"),
                new MetaInfo($"Icon: {Icon:X8}", clickable: true),
                new MetaInfo($"Type: {(SpellComponentTable.Type)Type}"),
                Gesture != 0x80000000 ? new MetaInfo($"Gesture: {(MotionCommand)Gesture}") : null,
                new MetaInfo($"Time: {Time}"),
                !string.IsNullOrEmpty(Text) ? new MetaInfo($"Text: {Text}") : null,
                new MetaInfo($"CDM: {CDM}"),
            };
            return nodes;
        }
    }
}
