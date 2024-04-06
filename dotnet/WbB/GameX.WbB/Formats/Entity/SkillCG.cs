using GameX.WbB.Formats.Props;
using GameX.Meta;
using GameX.Formats;
using System.Collections.Generic;
using System.IO;

namespace GameX.WbB.Formats.Entity
{
    public class SkillCG : IHaveMetaInfo
    {
        public readonly Skill SkillNum;
        public readonly int NormalCost;
        public readonly int PrimaryCost;

        public SkillCG(BinaryReader r)
        {
            SkillNum = (Skill)r.ReadUInt32();
            NormalCost = r.ReadInt32();
            PrimaryCost = r.ReadInt32();
        }

        //: Entity.SkillCG
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                new MetaInfo($"Skill: {SkillNum}"),
                new MetaInfo($"Normal Cost: {NormalCost}"),
                new MetaInfo($"Primary Cost: {PrimaryCost}"),
            };
            return nodes;
        }
    }
}
