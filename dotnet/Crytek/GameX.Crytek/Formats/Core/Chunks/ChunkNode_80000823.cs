using System;
using System.IO;
using System.Numerics;

namespace GameX.Crytek.Formats.Core.Chunks
{
    public class ChunkNode_80000823 : ChunkNode
    {
        public override void Read(BinaryReader r)
        {
            base.Read(r);

            Name = r.ReadFYString(64);
            if (string.IsNullOrEmpty(Name)) Name = "unknown";
            ObjectNodeID = MathX.SwapEndian(r.ReadInt32()); // Object reference ID
            ParentNodeID = MathX.SwapEndian(r.ReadInt32());
            __NumChildren = MathX.SwapEndian(r.ReadInt32());
            MatID = MathX.SwapEndian(r.ReadInt32());  // Material ID?
            SkipBytes(r, 4);

            // Read the 4x4 transform matrix.
            var transform = new Matrix4x4
            {
                M11 = MathX.SwapEndian(r.ReadSingle()),
                M12 = MathX.SwapEndian(r.ReadSingle()),
                M13 = MathX.SwapEndian(r.ReadSingle()),
                M14 = MathX.SwapEndian(r.ReadSingle()),
                M21 = MathX.SwapEndian(r.ReadSingle()),
                M22 = MathX.SwapEndian(r.ReadSingle()),
                M23 = MathX.SwapEndian(r.ReadSingle()),
                M24 = MathX.SwapEndian(r.ReadSingle()),
                M31 = MathX.SwapEndian(r.ReadSingle()),
                M32 = MathX.SwapEndian(r.ReadSingle()),
                M33 = MathX.SwapEndian(r.ReadSingle()),
                M34 = MathX.SwapEndian(r.ReadSingle()),
                M41 = MathX.SwapEndian(r.ReadSingle()) * VERTEX_SCALE,
                M42 = MathX.SwapEndian(r.ReadSingle()) * VERTEX_SCALE,
                M43 = MathX.SwapEndian(r.ReadSingle()) * VERTEX_SCALE,
                M44 = MathX.SwapEndian(r.ReadSingle()),
            };
            // original transform matrix is 3x4 stored as 4x4
            transform.M14 = transform.M24 = transform.M34 = 0f;
            transform.M44 = 1f;
            Transform = transform;

            // Read the position Pos Vector3
            Pos = new Vector3
            {
                X = MathX.SwapEndian(r.ReadSingle()) * VERTEX_SCALE,
                Y = MathX.SwapEndian(r.ReadSingle()) * VERTEX_SCALE,
                Z = MathX.SwapEndian(r.ReadSingle()) * VERTEX_SCALE,
            };

            // Read the rotation Rot Quad
            Rot = new Quaternion
            {
                X = MathX.SwapEndian(r.ReadSingle()),
                Y = MathX.SwapEndian(r.ReadSingle()),
                Z = MathX.SwapEndian(r.ReadSingle()),
                W = MathX.SwapEndian(r.ReadSingle()),
            };

            // Read the Scale Vector 3
            Scale = new Vector3
            {
                X = MathX.SwapEndian(r.ReadSingle()),
                Y = MathX.SwapEndian(r.ReadSingle()),
                Z = MathX.SwapEndian(r.ReadSingle()),
            };

            // read the controller pos/rot/scale
            PosCtrlID = MathX.SwapEndian(r.ReadInt32());
            RotCtrlID = MathX.SwapEndian(r.ReadInt32());
            SclCtrlID = MathX.SwapEndian(r.ReadInt32());

            Properties = r.ReadL32String();
        }
    }
}