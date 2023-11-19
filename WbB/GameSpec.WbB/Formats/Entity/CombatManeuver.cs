using GameSpec.WbB.Formats.Props;
using GameSpec.Metadata;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;

namespace GameSpec.WbB.Formats.Entity
{
    public class CombatManeuver : IGetMetadataInfo
    {
        public readonly MotionStance Style;
        public readonly AttackHeight AttackHeight;
        public readonly AttackType AttackType;
        public readonly uint MinSkillLevel;
        public readonly MotionCommand Motion;

        public CombatManeuver(BinaryReader r)
        {
            Style = (MotionStance)r.ReadUInt32();
            AttackHeight = (AttackHeight)r.ReadUInt32();
            AttackType = (AttackType)r.ReadUInt32();
            MinSkillLevel = r.ReadUInt32();
            Motion = (MotionCommand)r.ReadUInt32();
        }

        //: Entity.CombatManeuver
        List<MetadataInfo> IGetMetadataInfo.GetInfoNodes(MetadataManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<MetadataInfo> {
                new MetadataInfo($"Stance: {Style}"),
                new MetadataInfo($"Attack Height: {AttackHeight}"),
                new MetadataInfo($"Attack Type: {AttackType}"),
                MinSkillLevel != 0 ? new MetadataInfo($"Min Skill: {MinSkillLevel}") : null,
                new MetadataInfo($"Motion: {Motion}"),
            };
            return nodes;
        }
    }
}
