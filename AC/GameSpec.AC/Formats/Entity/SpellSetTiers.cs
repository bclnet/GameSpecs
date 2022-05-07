using GameSpec.Explorer;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GameSpec.AC.Formats.Entity
{
    public class SpellSetTiers : IGetExplorerInfo
    {
        /// <summary>
        /// A list of spell ids that are active in the spell set tier
        /// </summary>
        public readonly uint[] Spells;

        public SpellSetTiers(BinaryReader r)
            => Spells = r.ReadL32Array<uint>(sizeof(uint));

        //: Entity.SpellSetTier
        List<ExplorerInfoNode> IGetExplorerInfo.GetInfoNodes(ExplorerManager resource, FileMetadata file, object tag)
        {
            var spells = DatabaseManager.Portal.SpellTable.Spells;
            var nodes = Spells.Select(x => new ExplorerInfoNode($"{x} - {spells[x].Name}")).ToList();
            return nodes;
        }
    }
}
