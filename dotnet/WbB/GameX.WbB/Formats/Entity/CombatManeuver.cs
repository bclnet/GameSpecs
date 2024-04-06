using GameX.WbB.Formats.Props;
using GameX.Meta;
using GameX.Formats;
using System.Collections.Generic;
using System.IO;

namespace GameX.WbB.Formats.Entity
{
    public class CombatManeuver : IHaveMetaInfo
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
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                new MetaInfo($"Stance: {Style}"),
                new MetaInfo($"Attack Height: {AttackHeight}"),
                new MetaInfo($"Attack Type: {AttackType}"),
                MinSkillLevel != 0 ? new MetaInfo($"Min Skill: {MinSkillLevel}") : null,
                new MetaInfo($"Motion: {Motion}"),
            };
            return nodes;
        }
    }
}
