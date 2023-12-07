using GameSpec.Metadata;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GameSpec.WbB.Formats.Entity
{
    public class PhysicsScriptTableData : IGetMetadataInfo
    {
        public readonly ScriptAndModData[] Scripts;

        public PhysicsScriptTableData(BinaryReader r)
            => Scripts = r.ReadL32Array(x => new ScriptAndModData(r));

        //: Entity.PhysicsScriptTableData
        List<MetadataInfo> IGetMetadataInfo.GetInfoNodes(MetadataManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetadataInfo> {
                new MetadataInfo("ScriptMods", items: Scripts.Select(x=>new MetadataInfo($"{x}", clickable: true))),
            };
            return nodes;
        }
    }
}
