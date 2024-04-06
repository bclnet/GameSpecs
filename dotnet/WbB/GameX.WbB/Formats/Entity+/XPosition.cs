using GameX.WbB.Formats.Props;
using System;
using System.IO;
using System.Numerics;

namespace GameX.WbB.Formats.Entity
{
    public class XPosition
    {
        public XPosition()
        {
            //Pos = Vector3.Zero;
            Rotation = Quaternion.Identity;
        }
        public XPosition(XPosition pos)
        {
            LandblockId = new LandblockId(pos.LandblockId.Raw);
            Pos = pos.Pos;
            Rotation = pos.Rotation;
        }
        public XPosition(uint blockCellID, float newPositionX, float newPositionY, float newPositionZ, float newRotationX, float newRotationY, float newRotationZ, float newRotationW)
        {
            LandblockId = new LandblockId(blockCellID);
            Pos = new Vector3(newPositionX, newPositionY, newPositionZ);
            Rotation = new Quaternion(newRotationX, newRotationY, newRotationZ, newRotationW);
            if ((blockCellID & 0xFFFF) == 0) SetPosition(Pos);
        }
        public XPosition(uint blockCellID, Vector3 position, Quaternion rotation)
        {
            LandblockId = new LandblockId(blockCellID);
            Pos = position;
            Rotation = rotation;
            if ((blockCellID & 0xFFFF) == 0) SetPosition(Pos);
        }
        public XPosition(BinaryReader r)
        {
            LandblockId = new LandblockId(r.ReadUInt32());
            PositionX = r.ReadSingle(); PositionY = r.ReadSingle(); PositionZ = r.ReadSingle();
            // packet stream isn't the same order as the quaternion constructor
            RotationW = r.ReadSingle(); RotationX = r.ReadSingle(); RotationY = r.ReadSingle(); RotationZ = r.ReadSingle();
        }
        public XPosition(float northSouth, float eastWest)
        {
            northSouth = (northSouth - 0.5f) * 10.0f;
            eastWest = (eastWest - 0.5f) * 10.0f;

            uint baseX = (uint)(eastWest + 0x400), baseY = (uint)(northSouth + 0x400);
            if (baseX >= 0x7F8 || baseY >= 0x7F8) throw new Exception("Bad coordinates");  // TODO: Instead of throwing exception should we set to a default location?

            float xOffset = ((baseX & 7) * 24.0f) + 12, yOffset = ((baseY & 7) * 24.0f) + 12; // zOffset = GetZFromCellXY(LandblockId.Raw, xOffset, yOffset);
            const float zOffset = 0.0f;

            LandblockId = new LandblockId(GetCellFromBase(baseX, baseY));
            PositionX = xOffset;
            PositionY = yOffset;
            PositionZ = zOffset;
            Rotation = Quaternion.Identity;
        }
        /// <summary>
        /// Given a Vector2 set of coordinates, create a new position object for use in converting from VLOC to LOC
        /// </summary>
        /// <param name="coordinates">A set coordinates provided in a Vector2 object with East-West being the X value and North-South being the Y value</param>
        public XPosition(Vector2 coordinates)
        {
            // convert from (-102, 102) to (0, 204)
            coordinates += Vector2.One * 102;

            // 204 = map clicks across dereth
            // 2040 = number of cells across dereth
            // 24 = meters per cell
            //var globalPos = coordinates / 204 * 2040 * 24;
            var globalPos = coordinates * 240; // simplified
            globalPos -= Vector2.One * 12.0f; // ?????

            // inlining, this logic is in PositionExtensions.FromGlobal()
            int blockX = (int)globalPos.X / BlockLength, blockY = (int)globalPos.Y / BlockLength;
            float originX = globalPos.X % BlockLength, originY = globalPos.Y % BlockLength;

            int cellX = (int)originX / CellLength, cellY = (int)originY / CellLength;
            var cell = cellX * CellSide + cellY + 1;
            var objCellID = (uint)(blockX << 24 | blockY << 16 | cell);
            LandblockId = new LandblockId(objCellID);
            Pos = new Vector3(originX, originY, 0); // must use PositionExtensions.AdjustMapCoords() to get Z
            Rotation = Quaternion.Identity;
        }

        LandblockId landblockId;
        public LandblockId LandblockId
        {
            get => landblockId.Raw != 0 ? landblockId : new LandblockId(Cell);
            set => landblockId = value;
        }

        public uint Landblock { get => landblockId.Raw >> 16; }

        // FIXME: this is returning landblock + cell
        public uint Cell { get => landblockId.Raw; }

        public uint CellX { get => landblockId.Raw >> 8 & 0xFF; }
        public uint CellY { get => landblockId.Raw & 0xFF; }

        public uint LandblockX { get => landblockId.Raw >> 24 & 0xFF; }
        public uint LandblockY { get => landblockId.Raw >> 16 & 0xFF; }
        public uint GlobalCellX { get => LandblockX * 8 + CellX; }
        public uint GlobalCellY { get => LandblockY * 8 + CellY; }

        public Vector3 Pos
        {
            get => new Vector3(PositionX, PositionY, PositionZ);
            set => SetPosition(value);
        }

        public (bool b, bool c) SetPosition(Vector3 pos)
        {
            PositionX = pos.X;
            PositionY = pos.Y;
            PositionZ = pos.Z;
            return (b: SetLandblock(), c: SetLandCell());
        }

        public Quaternion Rotation
        {
            get => new Quaternion(RotationX, RotationY, RotationZ, RotationW);
            set
            {
                RotationW = value.W;
                RotationX = value.X;
                RotationY = value.Y;
                RotationZ = value.Z;
            }
        }

        public void Rotate(Vector3 dir) => Rotation = Quaternion.CreateFromYawPitchRoll(0, 0, (float)Math.Atan2(-dir.X, dir.Y));

        // TODO: delete this, use proper Vector3 and Quaternion
        public float PositionX { get; set; }
        public float PositionY { get; set; }
        public float PositionZ { get; set; }
        public float RotationW { get; set; }
        public float RotationX { get; set; }
        public float RotationY { get; set; }
        public float RotationZ { get; set; }

        public bool Indoors => landblockId.Indoors;

        /// <summary>
        /// Returns the normalized 2D heading direction
        /// </summary>
        public Vector3 CurrentDir => Vector3.Normalize(Vector3.Transform(Vector3.UnitY, Rotation));

        /// <summary>
        /// Returns this vector as a unit vector
        /// with a length of 1
        /// </summary>
        public Vector3 Normalize(Vector3 v) => v * 1.0f / v.Length();

        public XPosition InFrontOf(double distanceInFront, bool rotate180 = false)
        {
            float qw = RotationW, qz = RotationZ; // north, south
            double x = 2 * qw * qz, y = 1 - 2 * qz * qz;

            var heading = Math.Atan2(x, y);
            var dx = -1 * Convert.ToSingle(Math.Sin(heading) * distanceInFront);
            var dy = Convert.ToSingle(Math.Cos(heading) * distanceInFront);

            // move the Z slightly up and let gravity pull it down.  just makes things easier.
            var bumpHeight = 0.05f;
            if (rotate180)
            {
                var rotate = new Quaternion(0, 0, qz, qw) * Quaternion.CreateFromYawPitchRoll(0, 0, (float)Math.PI);
                return new XPosition(LandblockId.Raw, PositionX + dx, PositionY + dy, PositionZ + bumpHeight, 0f, 0f, rotate.Z, rotate.W);
            }
            else return new XPosition(LandblockId.Raw, PositionX + dx, PositionY + dy, PositionZ + bumpHeight, 0f, 0f, qz, qw);
        }

        /// <summary>
        /// Handles the Position crossing over landblock boundaries
        /// </summary>
        public bool SetLandblock()
        {
            if (Indoors) return false;
            var changedBlock = false;
            if (PositionX < 0)
            {
                var blockOffset = (int)PositionX / BlockLength - 1;
                var landblock = LandblockId.TransitionX(blockOffset);
                if (landblock != null) { LandblockId = landblock.Value; PositionX -= BlockLength * blockOffset; changedBlock = true; }
                else PositionX = 0;
            }
            if (PositionX >= BlockLength)
            {
                var blockOffset = (int)PositionX / BlockLength;
                var landblock = LandblockId.TransitionX(blockOffset);
                if (landblock != null) { LandblockId = landblock.Value; PositionX -= BlockLength * blockOffset; changedBlock = true; }
                else PositionX = BlockLength;
            }
            if (PositionY < 0)
            {
                var blockOffset = (int)PositionY / BlockLength - 1;
                var landblock = LandblockId.TransitionY(blockOffset);
                if (landblock != null) { LandblockId = landblock.Value; PositionY -= BlockLength * blockOffset; changedBlock = true; }
                else PositionY = 0;
            }
            if (PositionY >= BlockLength)
            {
                var blockOffset = (int)PositionY / BlockLength;
                var landblock = LandblockId.TransitionY(blockOffset);
                if (landblock != null) { LandblockId = landblock.Value; PositionY -= BlockLength * blockOffset; changedBlock = true; }
                else PositionY = BlockLength;
            }
            return changedBlock;
        }

        /// <summary>
        /// Determines the outdoor landcell for current position
        /// </summary>
        public bool SetLandCell()
        {
            if (Indoors) return false;
            var cellX = (uint)PositionX / CellLength;
            var cellY = (uint)PositionY / CellLength;
            var cellID = cellX * CellSide + cellY + 1;
            var curCellID = LandblockId.Raw & 0xFFFF;
            if (cellID == curCellID) return false;
            LandblockId = new LandblockId((uint)((LandblockId.Raw & 0xFFFF0000) | cellID));
            return true;
        }

        public void Serialize(BinaryWriter w, PositionFlags positionFlags, int animationFrame, bool writeLandblock = true)
        {
            w.Write((uint)positionFlags);
            if (writeLandblock) w.Write(LandblockId.Raw);
            w.Write(PositionX); w.Write(PositionY); w.Write(PositionZ);
            if ((positionFlags & PositionFlags.OrientationHasNoW) == 0) w.Write(RotationW);
            if ((positionFlags & PositionFlags.OrientationHasNoX) == 0) w.Write(RotationX);
            if ((positionFlags & PositionFlags.OrientationHasNoY) == 0) w.Write(RotationY);
            if ((positionFlags & PositionFlags.OrientationHasNoZ) == 0) w.Write(RotationZ);
            if ((positionFlags & PositionFlags.HasPlacementID) != 0) w.Write(animationFrame); // TODO: this is current animationframe_id when we are animating (?) - when we are not, how are we setting on the ground Position_id.
            if ((positionFlags & PositionFlags.HasVelocity) != 0) { /*velocity would go here*/ w.Write(0f); w.Write(0f); w.Write(0f); }
        }

        public void Serialize(BinaryWriter w, bool writeQuaternion = true, bool writeLandblock = true)
        {
            if (writeLandblock) w.Write(LandblockId.Raw);
            w.Write(PositionX); w.Write(PositionY); w.Write(PositionZ);
            if (writeQuaternion) { w.Write(RotationW); w.Write(RotationX); w.Write(RotationY); w.Write(RotationZ); }
        }

        uint GetCellFromBase(uint baseX, uint baseY)
        {
            byte blockX = (byte)(baseX >> 3), blockY = (byte)(baseY >> 3), cellX = (byte)(baseX & 7), cellY = (byte)(baseY & 7);
            uint block = (uint)((blockX << 8) | blockY), cell = (uint)((cellX << 3) | cellY);
            return (block << 16) | (cell + 1);
        }

        /// <summary>
        /// Returns the 3D squared distance between 2 objects
        /// </summary>
        public float SquaredDistanceTo(XPosition p)
        {
            if (p.LandblockId == LandblockId)
            {
                var dx = PositionX - p.PositionX;
                var dy = PositionY - p.PositionY;
                var dz = PositionZ - p.PositionZ;
                return dx * dx + dy * dy + dz * dz;
            }
            //if (p.LandblockId.MapScope == MapScope.Outdoors && this.LandblockId.MapScope == MapScope.Outdoors)
            else
            {
                // verify this is working correctly if one of these is indoors
                var dx = (LandblockId.LandblockX - p.LandblockId.LandblockX) * 192 + PositionX - p.PositionX;
                var dy = (LandblockId.LandblockY - p.LandblockId.LandblockY) * 192 + PositionY - p.PositionY;
                var dz = PositionZ - p.PositionZ;
                return dx * dx + dy * dy + dz * dz;
            }
        }

        /// <summary>
        /// Returns the 2D distance between 2 objects
        /// </summary>
        public float Distance2D(XPosition p)
        {
            // originally this returned the offset instead of distance...
            if (p.LandblockId == LandblockId)
            {
                var dx = PositionX - p.PositionX;
                var dy = PositionY - p.PositionY;
                return (float)Math.Sqrt(dx * dx + dy * dy);
            }
            //if (p.LandblockId.MapScope == MapScope.Outdoors && this.LandblockId.MapScope == MapScope.Outdoors)
            else
            {
                // verify this is working correctly if one of these is indoors
                var dx = (LandblockId.LandblockX - p.LandblockId.LandblockX) * 192 + PositionX - p.PositionX;
                var dy = (LandblockId.LandblockY - p.LandblockId.LandblockY) * 192 + PositionY - p.PositionY;
                return (float)Math.Sqrt(dx * dx + dy * dy);
            }
        }

        /// <summary>
        /// Returns the squared 2D distance between 2 objects
        /// </summary>
        public float Distance2DSquared(XPosition p)
        {
            // originally this returned the offset instead of distance...
            if (p.LandblockId == LandblockId)
            {
                var dx = PositionX - p.PositionX;
                var dy = PositionY - p.PositionY;
                return dx * dx + dy * dy;
            }
            //if (p.LandblockId.MapScope == MapScope.Outdoors && this.LandblockId.MapScope == MapScope.Outdoors)
            else
            {
                // verify this is working correctly if one of these is indoors
                var dx = (this.LandblockId.LandblockX - p.LandblockId.LandblockX) * 192 + this.PositionX - p.PositionX;
                var dy = (this.LandblockId.LandblockY - p.LandblockId.LandblockY) * 192 + this.PositionY - p.PositionY;
                return dx * dx + dy * dy;
            }
        }

        /// <summary>
        /// Returns the 3D distance between 2 objects
        /// </summary>
        public float DistanceTo(XPosition p)
        {
            // originally this returned the offset instead of distance...
            if (p.LandblockId == LandblockId)
            {
                var dx = PositionX - p.PositionX;
                var dy = PositionY - p.PositionY;
                var dz = PositionZ - p.PositionZ;
                return (float)Math.Sqrt(dx * dx + dy * dy + dz * dz);
            }
            //if (p.LandblockId.MapScope == MapScope.Outdoors && this.LandblockId.MapScope == MapScope.Outdoors)
            else
            {
                // verify this is working correctly if one of these is indoors
                var dx = (LandblockId.LandblockX - p.LandblockId.LandblockX) * 192 + PositionX - p.PositionX;
                var dy = (LandblockId.LandblockY - p.LandblockId.LandblockY) * 192 + PositionY - p.PositionY;
                var dz = PositionZ - p.PositionZ;
                return (float)Math.Sqrt(dx * dx + dy * dy + dz * dz);
            }
        }

        /// <summary>
        /// Returns the offset from current position to input position
        /// </summary>
        public Vector3 GetOffset(XPosition p)
        {
            var dx = (p.LandblockId.LandblockX - LandblockId.LandblockX) * 192 + p.PositionX - PositionX;
            var dy = (p.LandblockId.LandblockY - LandblockId.LandblockY) * 192 + p.PositionY - PositionY;
            var dz = p.PositionZ - PositionZ;
            return new Vector3(dx, dy, dz);
        }

        public override string ToString() => $"{LandblockId.Raw:X8} [{PositionX} {PositionY} {PositionZ}]";
        public string ToLOCString() => $"0x{LandblockId.Raw:X8} [{PositionX:F6} {PositionY:F6} {PositionZ:F6}] {RotationW:F6} {RotationX:F6} {RotationY:F6} {RotationZ:F6}";

        public const int BlockLength = 192;
        public const int CellSide = 8;
        public const int CellLength = 24;

        public bool Equals(XPosition p) => Cell == p.Cell && Pos.Equals(p.Pos) && Rotation.Equals(p.Rotation);
    }
}
