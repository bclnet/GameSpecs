using GameX.WbB.Formats.Entity;
using GameX.Meta;
using GameX.Formats;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GameX.WbB.Formats.FileTypes
{
    /// <summary>
    /// These are client_portal.dat files starting with 0x34. 
    /// </summary>
    [PakFileType(PakFileType.PhysicsScriptTable)]
    public class PhysicsScriptTable : FileType, IHaveMetaInfo
    {
        public readonly IDictionary<uint, PhysicsScriptTableData> ScriptTable;

        public PhysicsScriptTable(BinaryReader r)
        {
            Id = r.ReadUInt32();
            ScriptTable = r.ReadL32TMany<uint, PhysicsScriptTableData>(sizeof(uint), x => new PhysicsScriptTableData(x));
        }

        //: FileTypes.PhysicsScriptTable
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                new MetaInfo($"{nameof(PhysicsScriptTable)}: {Id:X8}", items: new List<MetaInfo> {
                    new MetaInfo("ScriptTable", items: ScriptTable.Select(x => new MetaInfo($"{x.Key}", items: (x.Value as IHaveMetaInfo).GetInfoNodes()))),
                })
            };
            return nodes;
        }
    }
}
