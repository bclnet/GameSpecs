using GameSpec.Explorer;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GameSpec.AC.Formats.Entity
{
    public class PhysicsScriptTableData : IGetExplorerInfo
    {
        public readonly ScriptAndModData[] Scripts;

        public PhysicsScriptTableData(BinaryReader r)
            => Scripts = r.ReadL32Array(x => new ScriptAndModData(r));

        //: Entity.PhysicsScriptTableData
        List<ExplorerInfoNode> IGetExplorerInfo.GetInfoNodes(ExplorerManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<ExplorerInfoNode> {
                new ExplorerInfoNode("ScriptMods", items: Scripts.Select(x=>new ExplorerInfoNode($"{x}", clickable: true))),
            };
            return nodes;
        }
    }
}
