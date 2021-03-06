using GameSpec.AC.Formats.Entity;
using GameSpec.Metadata;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GameSpec.AC.Formats.FileTypes
{
    /// <summary>
    /// These are client_portal.dat files starting with 0x34. 
    /// </summary>
    [PakFileType(PakFileType.PhysicsScriptTable)]
    public class PhysicsScriptTable : FileType, IGetMetadataInfo
    {
        public readonly Dictionary<uint, PhysicsScriptTableData> ScriptTable;

        public PhysicsScriptTable(BinaryReader r)
        {
            Id = r.ReadUInt32();
            ScriptTable = r.ReadL32Many<uint, PhysicsScriptTableData>(sizeof(uint), x => new PhysicsScriptTableData(x));
        }

        //: FileTypes.PhysicsScriptTable
        List<MetadataInfo> IGetMetadataInfo.GetInfoNodes(MetadataManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<MetadataInfo> {
                new MetadataInfo($"{nameof(PhysicsScriptTable)}: {Id:X8}", items: new List<MetadataInfo> {
                    new MetadataInfo("ScriptTable", items: ScriptTable.Select(x => new MetadataInfo($"{x.Key}", items: (x.Value as IGetMetadataInfo).GetInfoNodes()))),
                })
            };
            return nodes;
        }
    }
}
