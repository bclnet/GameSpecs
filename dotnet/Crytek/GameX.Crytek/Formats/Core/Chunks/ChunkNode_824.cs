using System;
using System.IO;
using System.Numerics;

namespace GameX.Crytek.Formats.Core.Chunks
{
    public class ChunkNode_824 : ChunkNode
    {
        public override void Read(BinaryReader r)
        {
            base.Read(r);

            Name = r.ReadFYString(64);
            if (string.IsNullOrEmpty(Name)) Name = "unknown";
            ObjectNodeID = r.ReadInt32(); // Object reference ID
            ParentNodeID = r.ReadInt32();
            __NumChildren = r.ReadInt32();
            MatID = r.ReadInt32(); // Material ID?
            SkipBytes(r, 4);

            // Read the 4x4 transform matrix.
            var transform = new Matrix4x4
            {
                M11 = r.ReadSingle(),
                M12 = r.ReadSingle(),
                M13 = r.ReadSingle(),
                M14 = r.ReadSingle(),
                M21 = r.ReadSingle(),
                M22 = r.ReadSingle(),
                M23 = r.ReadSingle(),
                M24 = r.ReadSingle(),
                M31 = r.ReadSingle(),
                M32 = r.ReadSingle(),
                M33 = r.ReadSingle(),
                M34 = r.ReadSingle(),
                M41 = r.ReadSingle() * VERTEX_SCALE,
                M42 = r.ReadSingle() * VERTEX_SCALE,
                M43 = r.ReadSingle() * VERTEX_SCALE,
                M44 = r.ReadSingle(),
            };
            // original transform matrix is 3x4 stored as 4x4.
            transform.M14 = transform.M24 = transform.M34 = 0f;
            transform.M44 = 1f;
            Transform = transform;

            Pos = r.ReadVector3() * VERTEX_SCALE;
            Rot = r.ReadQuaternion();
            Scale = r.ReadVector3();

            // read the controller pos/rot/scale
            PosCtrlID = r.ReadInt32();
            RotCtrlID = r.ReadInt32();
            SclCtrlID = r.ReadInt32();

            Properties = r.ReadL32String();
        }
    }
}