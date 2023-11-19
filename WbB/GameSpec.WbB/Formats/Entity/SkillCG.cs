using GameSpec.WbB.Formats.Props;
using GameSpec.Metadata;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;

namespace GameSpec.WbB.Formats.Entity
{
    public class SkillCG : IGetMetadataInfo
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
        List<MetadataInfo> IGetMetadataInfo.GetInfoNodes(MetadataManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<MetadataInfo> {
                new MetadataInfo($"Skill: {SkillNum}"),
                new MetadataInfo($"Normal Cost: {NormalCost}"),
                new MetadataInfo($"Primary Cost: {PrimaryCost}"),
            };
            return nodes;
        }
    }
}
