using GameX.WbB.Formats.Props;
using GameX.Meta;
using GameX.Formats;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;

namespace GameX.WbB.Formats.Entity
{
    public class MotionData : IHaveMetaInfo
    {
        public readonly byte Bitfield;
        public readonly MotionDataFlags Flags;
        public readonly AnimData[] Anims;
        public readonly Vector3 Velocity;
        public readonly Vector3 Omega;

        public MotionData(BinaryReader r)
        {
            var numAnims = r.ReadByte();
            Bitfield = r.ReadByte();
            Flags = (MotionDataFlags)r.ReadByte(); r.Align();
            Anims = r.ReadFArray(x => new AnimData(x), numAnims);
            if ((Flags & MotionDataFlags.HasVelocity) != 0) Velocity = r.ReadVector3();
            if ((Flags & MotionDataFlags.HasOmega) != 0) Omega = r.ReadVector3();
        }

        //: Entity.MotionData
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                Bitfield != 0 ? new MetaInfo($"Bitfield: {Bitfield:X8}") : null,
                Anims.Length == 0 ? null : Anims.Length == 1
                    ? new MetaInfo("Animation", items: (Anims[0] as IHaveMetaInfo).GetInfoNodes())
                    : new MetaInfo("Animations", items: Anims.Select((x, i) => new MetaInfo($"{i}", items: (x as IHaveMetaInfo).GetInfoNodes()))),
                Flags.HasFlag(MotionDataFlags.HasVelocity) ? new MetaInfo($"Velocity: {Velocity}") : null,
                Flags.HasFlag(MotionDataFlags.HasOmega) ? new MetaInfo($"Omega: {Omega}") : null,
            };
            return nodes;
        }
    }
}
