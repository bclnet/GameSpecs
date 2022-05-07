using GameSpec.Explorer;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GameSpec.AC.Formats.Entity
{
    public class StringTableData : IGetExplorerInfo
    {
        public readonly uint Id;
        public readonly string[] VarNames;
        public readonly string[] Vars;
        public readonly string[] Strings;
        public readonly uint[] Comments;
        public readonly byte Unknown;

        public StringTableData(BinaryReader r)
        {
            Id = r.ReadUInt32();
            VarNames = r.ReadL16Array(x => x.ReadUnicodeString());
            Vars = r.ReadL16Array(x => x.ReadUnicodeString());
            Strings = r.ReadL32Array(x => x.ReadUnicodeString());
            Comments = r.ReadL32Array<uint>(sizeof(uint));
            Unknown = r.ReadByte();
        }

        //: Entity.StringTableData
        List<ExplorerInfoNode> IGetExplorerInfo.GetInfoNodes(ExplorerManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<ExplorerInfoNode> {
                new ExplorerInfoNode($"{Id:X8}"),
                VarNames.Length > 0 ? new ExplorerInfoNode("Variable Names", items: VarNames.Select(x => new ExplorerInfoNode($"{x}"))) : null,
                Vars.Length > 0 ? new ExplorerInfoNode("Variables", items: Vars.Select(x => new ExplorerInfoNode($"{x}"))) : null,
                Strings.Length > 0 ? new ExplorerInfoNode("Strings", items: Strings.Select(x => new ExplorerInfoNode($"{x}"))) : null,
                Comments.Length > 0 ? new ExplorerInfoNode("Comments", items: Comments.Select(x => new ExplorerInfoNode($"{x:X8}"))) : null,
            };
            return nodes;
        }
    }
}
