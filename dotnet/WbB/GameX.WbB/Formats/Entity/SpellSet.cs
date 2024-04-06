using GameX.Meta;
using GameX.Formats;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GameX.WbB.Formats.Entity
{
    public class SpellSet : IHaveMetaInfo
    {
        // uint key is the total combined item level of all the equipped pieces in the set client calls this m_PieceCount
        public readonly IDictionary<uint, SpellSetTiers> SpellSetTiers;
        public readonly uint HighestTier;
        public readonly IDictionary<uint, SpellSetTiers> SpellSetTiersNoGaps;

        public SpellSet(BinaryReader r)
        {
            SpellSetTiers = r.Skip(2).ReadL16TMany<uint, SpellSetTiers>(sizeof(uint), x => new SpellSetTiers(x), sorted: true);
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
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                new MetaInfo("SpellSetTiers", items: SpellSetTiers.Select(x => new MetaInfo($"{x.Key}", items: (x.Value as IHaveMetaInfo).GetInfoNodes()))),
                new MetaInfo($"HighestTier: {HighestTier}"),
            };
            return nodes;
        }
    }
}
