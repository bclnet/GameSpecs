using GameSpec.Explorer;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GameSpec.AC.Formats.Entity
{
    public class SpellSet : IGetExplorerInfo
    {
        // uint key is the total combined item level of all the equipped pieces in the set client calls this m_PieceCount
        public readonly SortedDictionary<uint, SpellSetTiers> SpellSetTiers;
        public readonly uint HighestTier;
        public readonly SortedDictionary<uint, SpellSetTiers> SpellSetTiersNoGaps;

        public SpellSet(BinaryReader r)
        {
            SpellSetTiers = r.ReadL16SortedMany<uint, SpellSetTiers>(sizeof(uint), x => new SpellSetTiers(x), offset: 2);
            HighestTier = SpellSetTiers.Keys.LastOrDefault();
            SpellSetTiersNoGaps = new SortedDictionary<uint, SpellSetTiers>();
            var lastSpellSetTier = default(SpellSetTiers);
            for (var i = 0U; i <= HighestTier; i++)
            {
                if (SpellSetTiers.TryGetValue(i, out var spellSetTiers)) lastSpellSetTier = spellSetTiers;
                if (lastSpellSetTier != null) SpellSetTiersNoGaps.Add(i, lastSpellSetTier);
            }
        }

        //: Entity.SpellSet
        List<ExplorerInfoNode> IGetExplorerInfo.GetInfoNodes(ExplorerManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<ExplorerInfoNode> {
                new ExplorerInfoNode("SpellSetTiers", items: SpellSetTiers.Select(x => new ExplorerInfoNode($"{x.Key}", items: (x.Value as IGetExplorerInfo).GetInfoNodes()))),
                new ExplorerInfoNode($"HighestTier: {HighestTier}"),
            };
            return nodes;
        }
    }
}
