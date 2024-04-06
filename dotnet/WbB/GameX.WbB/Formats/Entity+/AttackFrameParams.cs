using GameX.WbB.Formats.Props;
using System;

namespace GameX.WbB.Formats.Entity
{
    public class AttackFrameParams : IEquatable<AttackFrameParams>
    {
        public uint MotionTableId;
        public MotionStance Stance;
        public MotionCommand Motion;

        public AttackFrameParams(uint motionTableId, MotionStance stance, MotionCommand motion)
        {
            MotionTableId = motionTableId;
            Stance = stance;
            Motion = motion;
        }

        public bool Equals(AttackFrameParams attackFrameParams)
            => MotionTableId == attackFrameParams.MotionTableId &&
                Stance == attackFrameParams.Stance &&
                Motion == attackFrameParams.Motion;

        public override int GetHashCode()
        {
            var hash = 0;
            hash = (hash * 397) ^ MotionTableId.GetHashCode();
            hash = (hash * 397) ^ Stance.GetHashCode();
            hash = (hash * 397) ^ Motion.GetHashCode();
            return hash;
        }
    }
}
