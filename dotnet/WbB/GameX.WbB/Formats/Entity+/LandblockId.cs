using GameX.WbB.Formats.Props;
using System;

namespace GameX.WbB.Formats.Entity
{
    public struct LandblockId
    {
        public uint Raw { get; }

        public LandblockId(uint raw) => Raw = raw;
        public LandblockId(byte x, byte y) => Raw = (uint)x << 24 | (uint)y << 16;
        public LandblockId East => new LandblockId(Convert.ToByte(LandblockX + 1), LandblockY);
        public LandblockId West => new LandblockId(Convert.ToByte(LandblockX - 1), LandblockY);
        public LandblockId North => new LandblockId(LandblockX, Convert.ToByte(LandblockY + 1));
        public LandblockId South => new LandblockId(LandblockX, Convert.ToByte(LandblockY - 1));
        public LandblockId NorthEast => new LandblockId(Convert.ToByte(LandblockX + 1), Convert.ToByte(LandblockY + 1));
        public LandblockId NorthWest => new LandblockId(Convert.ToByte(LandblockX - 1), Convert.ToByte(LandblockY + 1));
        public LandblockId SouthEast => new LandblockId(Convert.ToByte(LandblockX + 1), Convert.ToByte(LandblockY - 1));
        public LandblockId SouthWest => new LandblockId(Convert.ToByte(LandblockX - 1), Convert.ToByte(LandblockY - 1));
        public ushort Landblock => (ushort)((Raw >> 16) & 0xFFFF);
        public byte LandblockX => (byte)((Raw >> 24) & 0xFF);
        public byte LandblockY => (byte)((Raw >> 16) & 0xFF);

        /// <summary>
        /// This is only used to calculate LandcellX and LandcellY - it has no other function.
        /// </summary>
        public ushort Landcell => (byte)((Raw & 0x3F) - 1);
        public byte LandcellX => Convert.ToByte((Landcell >> 3) & 0x7);
        public byte LandcellY => Convert.ToByte(Landcell & 0x7);

#if false
        // not sure where this logic came from, i don't think MapScope.IndoorsSmall and MapScope.IndoorsLarge was a thing?
        //public MapScope MapScope => (MapScope)((Raw & 0x0F00) >> 8);

        // just nuking this now, keeping this code here for reference
        /*public MapScope MapScope
        {
            // TODO: port the updated version of Position and Landblock from Instancing branch, get rid of this MapScope thing..
            get
            {
                var cell = Raw & 0xFFFF;

                if (cell < 0x100)
                    return MapScope.Outdoors;
                else if (cell < 0x200)
                    return MapScope.IndoorsSmall;
                else
                    return MapScope.IndoorsLarge;
            }
        }*/
#endif

        public bool Indoors => (Raw & 0xFFFF) >= 0x100;

        public static bool operator ==(LandblockId c1, LandblockId c2) => c1.Landblock == c2.Landblock;
        public static bool operator !=(LandblockId c1, LandblockId c2) => c1.Landblock != c2.Landblock;

        public bool IsAdjacentTo(LandblockId block) => Math.Abs(LandblockX - block.LandblockX) <= 1 && Math.Abs(LandblockY - block.LandblockY) <= 1;

        public LandblockId? TransitionX(int blockOffset)
        {
            var newX = LandblockX + blockOffset;
            return newX < 0 || newX > 254
                ? null
                : (LandblockId?)new LandblockId((uint)newX << 24 | (uint)LandblockY << 16 | Raw & 0xFFFF);
        }

        public LandblockId? TransitionY(int blockOffset)
        {
            var newY = LandblockY + blockOffset;
            return newY < 0 || newY > 254
                ? null
                : (LandblockId?)new LandblockId((uint)LandblockX << 24 | (uint)newY << 16 | Raw & 0xFFFF);
        }

        public override bool Equals(object obj) => obj is LandblockId id && id == this;
        public override int GetHashCode() => base.GetHashCode();
        public override string ToString() => Raw.ToString("X8");
    }
}
