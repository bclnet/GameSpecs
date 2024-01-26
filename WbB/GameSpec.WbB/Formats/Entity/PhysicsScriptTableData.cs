using GameSpec.Meta;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GameSpec.WbB.Formats.Entity
{
    public class PhysicsScriptTableData : IHaveMetaInfo
    {
        public readonly ScriptAndModData[] Scripts;

        public PhysicsScriptTableData(BinaryReader r)
            => Scripts = r.ReadL32FArray(x => new ScriptAndModData(r));

        //: Entity.PhysicsScriptTableData
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                new MetaInfo("ScriptMods", items: Scripts.Select(x=>new MetaInfo($"{x}", clickable: true))),
            };
            return nodes;
        }
    }
}
