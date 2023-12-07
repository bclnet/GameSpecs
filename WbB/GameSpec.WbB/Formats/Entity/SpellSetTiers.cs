using GameSpec.Metadata;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GameSpec.WbB.Formats.Entity
{
    public class SpellSetTiers : IGetMetadataInfo
    {
        /// <summary>
        /// A list of spell ids that are active in the spell set tier
        /// </summary>
        public readonly uint[] Spells;

        public SpellSetTiers(BinaryReader r)
            => Spells = r.ReadL32Array<uint>(sizeof(uint));

        //: Entity.SpellSetTier
        List<MetadataInfo> IGetMetadataInfo.GetInfoNodes(MetadataManager resource, FileSource file, object tag)
        {
            var spells = DatabaseManager.Portal.SpellTable.Spells;
            var nodes = Spells.Select(x => new MetadataInfo($"{x} - {spells[x].Name}")).ToList();
            return nodes;
        }
    }
}
