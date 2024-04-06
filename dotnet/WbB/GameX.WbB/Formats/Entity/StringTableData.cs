using GameX.Meta;
using GameX.Formats;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GameX.WbB.Formats.Entity
{
    public class StringTableData : IHaveMetaInfo
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
            VarNames = r.ReadL16FArray(x => x.ReadCU32String());
            Vars = r.ReadL16FArray(x => x.ReadCU32String());
            Strings = r.ReadL32FArray(x => x.ReadCU32String());
            Comments = r.ReadL32TArray<uint>(sizeof(uint));
            Unknown = r.ReadByte();
        }

        //: Entity.StringTableData
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                new MetaInfo($"{Id:X8}"),
                VarNames.Length > 0 ? new MetaInfo("Variable Names", items: VarNames.Select(x => new MetaInfo($"{x}"))) : null,
                Vars.Length > 0 ? new MetaInfo("Variables", items: Vars.Select(x => new MetaInfo($"{x}"))) : null,
                Strings.Length > 0 ? new MetaInfo("Strings", items: Strings.Select(x => new MetaInfo($"{x}"))) : null,
                Comments.Length > 0 ? new MetaInfo("Comments", items: Comments.Select(x => new MetaInfo($"{x:X8}"))) : null,
            };
            return nodes;
        }
    }
}
