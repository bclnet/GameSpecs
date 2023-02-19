using GameSpec.Metadata;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GameSpec.AC.Formats.Entity
{
    public class StringTableData : IGetMetadataInfo
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
            VarNames = r.ReadL16Array(x => x.ReadCU32String());
            Vars = r.ReadL16Array(x => x.ReadCU32String());
            Strings = r.ReadL32Array(x => x.ReadCU32String());
            Comments = r.ReadL32Array<uint>(sizeof(uint));
            Unknown = r.ReadByte();
        }

        //: Entity.StringTableData
        List<MetadataInfo> IGetMetadataInfo.GetInfoNodes(MetadataManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<MetadataInfo> {
                new MetadataInfo($"{Id:X8}"),
                VarNames.Length > 0 ? new MetadataInfo("Variable Names", items: VarNames.Select(x => new MetadataInfo($"{x}"))) : null,
                Vars.Length > 0 ? new MetadataInfo("Variables", items: Vars.Select(x => new MetadataInfo($"{x}"))) : null,
                Strings.Length > 0 ? new MetadataInfo("Strings", items: Strings.Select(x => new MetadataInfo($"{x}"))) : null,
                Comments.Length > 0 ? new MetadataInfo("Comments", items: Comments.Select(x => new MetadataInfo($"{x:X8}"))) : null,
            };
            return nodes;
        }
    }
}
