using GameX.Meta;
using GameX.Formats;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GameX.WbB.Formats.Entity
{
    public class SpellSetTiers : IHaveMetaInfo
    {
        /// <summary>
        /// A list of spell ids that are active in the spell set tier
        /// </summary>
        public readonly uint[] Spells;

        public SpellSetTiers(BinaryReader r)
            => Spells = r.ReadL32TArray<uint>(sizeof(uint));

        //: Entity.SpellSetTier
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var spells = DatabaseManager.Portal.SpellTable.Spells;
            var nodes = Spells.Select(x => new MetaInfo($"{x} - {spells[x].Name}")).ToList();
            return nodes;
        }
    }
}
