using GameSpec.AC.Formats.FileTypes;
using GameSpec.AC.Formats.Props;
using GameSpec.Explorer;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;

namespace GameSpec.AC.Formats.Entity
{
    public class SpellComponentBase : IGetExplorerInfo
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
            Name = r.ReadObfuscatedString(); r.AlignBoundary();
            Category = r.ReadUInt32();
            Icon = r.ReadUInt32();
            Type = r.ReadUInt32();
            Gesture = r.ReadUInt32();
            Time = r.ReadSingle();
            Text = r.ReadObfuscatedString(); r.AlignBoundary();
            CDM = r.ReadSingle();
        }

        //: Entity.SpellComponentBase
        List<ExplorerInfoNode> IGetExplorerInfo.GetInfoNodes(ExplorerManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<ExplorerInfoNode> {
                new ExplorerInfoNode($"Name: {Name}"),
                new ExplorerInfoNode($"Category: {Category}"),
                new ExplorerInfoNode($"Icon: {Icon:X8}", clickable: true),
                new ExplorerInfoNode($"Type: {(SpellComponentTable.Type)Type}"),
                Gesture != 0x80000000 ? new ExplorerInfoNode($"Gesture: {(MotionCommand)Gesture}") : null,
                new ExplorerInfoNode($"Time: {Time}"),
                !string.IsNullOrEmpty(Text) ? new ExplorerInfoNode($"Text: {Text}") : null,
                new ExplorerInfoNode($"CDM: {CDM}"),
            };
            return nodes;
        }
    }
}
