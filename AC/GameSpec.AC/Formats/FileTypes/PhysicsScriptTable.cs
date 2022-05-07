using GameSpec.AC.Formats.Entity;
using GameSpec.Explorer;
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
    public class PhysicsScriptTable : FileType, IGetExplorerInfo
    {
        public readonly Dictionary<uint, PhysicsScriptTableData> ScriptTable;

        public PhysicsScriptTable(BinaryReader r)
        {
            Id = r.ReadUInt32();
            ScriptTable = r.ReadL32Many<uint, PhysicsScriptTableData>(sizeof(uint), x => new PhysicsScriptTableData(x));
        }

        //: FileTypes.PhysicsScriptTable
        List<ExplorerInfoNode> IGetExplorerInfo.GetInfoNodes(ExplorerManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<ExplorerInfoNode> {
                new ExplorerInfoNode($"{nameof(PhysicsScriptTable)}: {Id:X8}", items: new List<ExplorerInfoNode> {
                    new ExplorerInfoNode("ScriptTable", items: ScriptTable.Select(x => new ExplorerInfoNode($"{x.Key}", items: (x.Value as IGetExplorerInfo).GetInfoNodes()))),
                })
            };
            return nodes;
        }
    }
}
