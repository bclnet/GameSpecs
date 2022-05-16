using GameSpec.AC.Formats.Props;
using GameSpec.Metadata;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;

namespace GameSpec.AC.Formats.Entity
{
    public class MotionData : IGetMetadataInfo
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
            Anims = r.ReadTArray(x => new AnimData(x), numAnims);
            if ((Flags & MotionDataFlags.HasVelocity) != 0) Velocity = r.ReadVector3();
            if ((Flags & MotionDataFlags.HasOmega) != 0) Omega = r.ReadVector3();
        }

        //: Entity.MotionData
        List<MetadataInfo> IGetMetadataInfo.GetInfoNodes(MetadataManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<MetadataInfo> {
                Bitfield != 0 ? new MetadataInfo($"Bitfield: {Bitfield:X8}") : null,
                Anims.Length == 0 ? null : Anims.Length == 1
                    ? new MetadataInfo("Animation", items: (Anims[0] as IGetMetadataInfo).GetInfoNodes())
                    : new MetadataInfo("Animations", items: Anims.Select((x, i) => new MetadataInfo($"{i}", items: (x as IGetMetadataInfo).GetInfoNodes()))),
                Flags.HasFlag(MotionDataFlags.HasVelocity) ? new MetadataInfo($"Velocity: {Velocity}") : null,
                Flags.HasFlag(MotionDataFlags.HasOmega) ? new MetadataInfo($"Omega: {Omega}") : null,
            };
            return nodes;
        }
    }
}
