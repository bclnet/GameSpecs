using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace System.NumericsX
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Box
    {
        public static readonly Box zero = new(Vector3.origin, Vector3.origin, Matrix3x3.identity);

        /*
                        4---{4}---5
             +         /|        /|
             Z      {7} {8}   {5} |
             -     /    |    /    {9}
                  7--{6}----6     |
                  |     |   |     |
                {11}    0---|-{0}-1
                  |    /    |    /       -
                  | {3}  {10} {1}       Y
                  |/        |/         +
                  3---{2}---2

                    - X +
        */

        // plane bits:
        // 0 = min x
        // 1 = max x
        // 2 = min y
        // 3 = max y
        // 4 = min z
        // 5 = max z
        static readonly int[][] BoxPlaneBitsSilVerts = new int[][]{
            new[]{ 0, 0, 0, 0, 0, 0, 0 }, // 000000 = 0
	        new[]{ 4, 7, 4, 0, 3, 0, 0 }, // 000001 = 1
	        new[]{ 4, 5, 6, 2, 1, 0, 0 }, // 000010 = 2
	        new[]{ 0, 0, 0, 0, 0, 0, 0 }, // 000011 = 3
	        new[]{ 4, 4, 5, 1, 0, 0, 0 }, // 000100 = 4
	        new[]{ 6, 3, 7, 4, 5, 1, 0 }, // 000101 = 5
	        new[]{ 6, 4, 5, 6, 2, 1, 0 }, // 000110 = 6
	        new[]{ 0, 0, 0, 0, 0, 0, 0 }, // 000111 = 7
	        new[]{ 4, 6, 7, 3, 2, 0, 0 }, // 001000 = 8
	        new[]{ 6, 6, 7, 4, 0, 3, 2 }, // 001001 = 9
	        new[]{ 6, 5, 6, 7, 3, 2, 1 }, // 001010 = 10
	        new[]{ 0, 0, 0, 0, 0, 0, 0 }, // 001011 = 11
	        new[]{ 0, 0, 0, 0, 0, 0, 0 }, // 001100 = 12
	        new[]{ 0, 0, 0, 0, 0, 0, 0 }, // 001101 = 13
	        new[]{ 0, 0, 0, 0, 0, 0, 0 }, // 001110 = 14
	        new[]{ 0, 0, 0, 0, 0, 0, 0 }, // 001111 = 15
	        new[]{ 4, 0, 1, 2, 3, 0, 0 }, // 010000 = 16
	        new[]{ 6, 0, 1, 2, 3, 7, 4 }, // 010001 = 17
	        new[]{ 6, 3, 2, 6, 5, 1, 0 }, // 010010 = 18
	        new[]{ 0, 0, 0, 0, 0, 0, 0 }, // 010011 = 19
	        new[]{ 6, 1, 2, 3, 0, 4, 5 }, // 010100 = 20
	        new[]{ 6, 1, 2, 3, 7, 4, 5 }, // 010101 = 21
	        new[]{ 6, 2, 3, 0, 4, 5, 6 }, // 010110 = 22
	        new[]{ 0, 0, 0, 0, 0, 0, 0 }, // 010111 = 23
	        new[]{ 6, 0, 1, 2, 6, 7, 3 }, // 011000 = 24
	        new[]{ 6, 0, 1, 2, 6, 7, 4 }, // 011001 = 25
	        new[]{ 6, 0, 1, 5, 6, 7, 3 }, // 011010 = 26
	        new[]{ 0, 0, 0, 0, 0, 0, 0 }, // 011011 = 27
	        new[]{ 0, 0, 0, 0, 0, 0, 0 }, // 011100 = 28
	        new[]{ 0, 0, 0, 0, 0, 0, 0 }, // 011101 = 29
	        new[]{ 0, 0, 0, 0, 0, 0, 0 }, // 011110 = 30
	        new[]{ 0, 0, 0, 0, 0, 0, 0 }, // 011111 = 31
	        new[]{ 4, 7, 6, 5, 4, 0, 0 }, // 100000 = 32
	        new[]{ 6, 7, 6, 5, 4, 0, 3 }, // 100001 = 33
	        new[]{ 6, 5, 4, 7, 6, 2, 1 }, // 100010 = 34
	        new[]{ 0, 0, 0, 0, 0, 0, 0 }, // 100011 = 35
	        new[]{ 6, 4, 7, 6, 5, 1, 0 }, // 100100 = 36
	        new[]{ 6, 3, 7, 6, 5, 1, 0 }, // 100101 = 37
	        new[]{ 6, 4, 7, 6, 2, 1, 0 }, // 100110 = 38
	        new[]{ 0, 0, 0, 0, 0, 0, 0 }, // 100111 = 39
	        new[]{ 6, 6, 5, 4, 7, 3, 2 }, // 101000 = 40
	        new[]{ 6, 6, 5, 4, 0, 3, 2 }, // 101001 = 41
	        new[]{ 6, 5, 4, 7, 3, 2, 1 }, // 101010 = 42
	        new[]{ 0, 0, 0, 0, 0, 0, 0 }, // 101011 = 43
	        new[]{ 0, 0, 0, 0, 0, 0, 0 }, // 101100 = 44
	        new[]{ 0, 0, 0, 0, 0, 0, 0 }, // 101101 = 45
	        new[]{ 0, 0, 0, 0, 0, 0, 0 }, // 101110 = 46
	        new[]{ 0, 0, 0, 0, 0, 0, 0 }, // 101111 = 47
	        new[]{ 0, 0, 0, 0, 0, 0, 0 }, // 110000 = 48
	        new[]{ 0, 0, 0, 0, 0, 0, 0 }, // 110001 = 49
	        new[]{ 0, 0, 0, 0, 0, 0, 0 }, // 110010 = 50
	        new[]{ 0, 0, 0, 0, 0, 0, 0 }, // 110011 = 51
	        new[]{ 0, 0, 0, 0, 0, 0, 0 }, // 110100 = 52
	        new[]{ 0, 0, 0, 0, 0, 0, 0 }, // 110101 = 53
	        new[]{ 0, 0, 0, 0, 0, 0, 0 }, // 110110 = 54
	        new[]{ 0, 0, 0, 0, 0, 0, 0 }, // 110111 = 55
	        new[]{ 0, 0, 0, 0, 0, 0, 0 }, // 111000 = 56
	        new[]{ 0, 0, 0, 0, 0, 0, 0 }, // 111001 = 57
	        new[]{ 0, 0, 0, 0, 0, 0, 0 }, // 111010 = 58
	        new[]{ 0, 0, 0, 0, 0, 0, 0 }, // 111011 = 59
	        new[]{ 0, 0, 0, 0, 0, 0, 0 }, // 111100 = 60
	        new[]{ 0, 0, 0, 0, 0, 0, 0 }, // 111101 = 61
	        new[]{ 0, 0, 0, 0, 0, 0, 0 }, // 111110 = 62
	        new[]{ 0, 0, 0, 0, 0, 0, 0 }, // 111111 = 63
        };

        Vector3 center;
        Vector3 extents;
        Matrix3x3 axis;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Box(in Box a)
        {
            center = a.center;
            extents = a.extents;
            axis = new(a.axis);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Box(in Vector3 center, in Vector3 extents, in Matrix3x3 axis)
        {
            this.center = center;
            this.extents = extents;
            this.axis = axis;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Box(in Vector3 point)
        {
            this.center = point;
            this.extents = Vector3.origin;
            this.axis = Matrix3x3.identity;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Box(Bounds bounds)
        {
            this.center = (bounds[0] + bounds[1]) * 0.5f;
            this.extents = bounds[1] - this.center;
            this.axis = Matrix3x3.identity;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Box(in Bounds bounds, in Vector3 origin, in Matrix3x3 axis)
        {
            this.center = (bounds[0] + bounds[1]) * 0.5f;
            this.extents = bounds[1] - this.center;
            this.center = origin + this.center * axis;
            this.axis = axis;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Box operator +(in Box _, in Vector3 t)               // returns translated box
            => new(_.center + t, _.extents, _.axis);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Box operator *(in Box _, in Matrix3x3 r)             // returns rotated box
            => new(_.center * r, _.extents, _.axis * r);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Box operator +(in Box _, in Box a)
        {
            Box newBox = new(_);
            newBox.AddBox(a);
            return newBox;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Box operator -(in Box _, in Box a)
            => new(_.center, _.extents - a.extents, _.axis);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Compare(in Box a)                     // exact compare, no epsilon
            => center.Compare(a.center) && extents.Compare(a.extents) && axis.Compare(a.axis);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Compare(in Box a, float epsilon)  // compare with epsilon
            => center.Compare(a.center, epsilon) && extents.Compare(a.extents, epsilon) && axis.Compare(a.axis, epsilon);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(in Box _, in Box a)                     // exact compare, no epsilon
            => _.Compare(a);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(in Box _, in Box a)                     // exact compare, no epsilon
            => !_.Compare(a);
        public override bool Equals(object obj)
            => obj is Box q && Compare(q);
        public override int GetHashCode()
            => center.GetHashCode() ^ extents.GetHashCode() ^ axis.GetHashCode();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()                                    // inside out box
        {
            center.Zero();
            extents.x = extents.y = extents.z = -MathX.INFINITY;
            axis.Identity();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Zero()                                 // single point at origin
        {
            center.Zero();
            extents.Zero();
            axis.Identity();
        }

        public Vector3 Center                     // returns center of the box
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => center;
        }

        public Vector3 Extents                        // returns extents of the box
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => extents;
        }

        public Matrix3x3 Axis                         // returns the axis of the box
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => axis;
        }

        public float Volume                       // returns the volume of the box
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (extents * 2f).LengthSqr;
        }

        public bool IsCleared                        // returns true if box are inside out
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => extents.x < 0f;
        }

        public bool AddPoint(in Vector3 v)                    // add the point, returns true if the box expanded
        {
            if (extents.x < 0f) { extents.Zero(); center = v; axis.Identity(); return true; }

            Bounds bounds1 = new();
            bounds1[0].x = bounds1[1].x = center * axis[0];
            bounds1[0].y = bounds1[1].y = center * axis[1];
            bounds1[0].z = bounds1[1].z = center * axis[2];
            bounds1[0] -= extents;
            bounds1[1] += extents;
            // point is contained in the box
            if (!bounds1.AddPoint(new Vector3(v * axis[0], v * axis[1], v * axis[2]))) return false;

            Matrix3x3 axis2 = new();
            axis2[0] = v - center;
            axis2[0].Normalize();
            axis2[1] = axis[MathX.Min3Index(axis2[0] * axis[0], axis2[0] * axis[1], axis2[0] * axis[2])];
            axis2[1] = axis2[1] - axis2[1] * axis2[0] * axis2[0];
            axis2[1].Normalize();
            axis2[2].Cross(axis2[0], axis2[1]);

            AxisProjection(axis2, out var bounds2);
            bounds2.AddPoint(new Vector3(v * axis2[0], v * axis2[1], v * axis2[2]));

            // create new box based on the smallest bounds
            if (bounds1.Volume < bounds2.Volume) { center = (bounds1[0] + bounds1[1]) * 0.5f; extents = bounds1[1] - center; center *= axis; }
            else { center = (bounds2[0] + bounds2[1]) * 0.5f; extents = bounds2[1] - center; center *= axis2; axis = axis2; }
            return true;
        }

        public bool AddBox(in Box a)                      // add the box, returns true if the box expanded
        {
            if (a.extents.x < 0f) return false;

            if (extents.x < 0f) { center = a.center; extents = a.extents; axis = a.axis; return true; }

            // test axis of this box
            var ax = new Matrix3x3[3]; var bounds = new Bounds[4];
            ax[0] = axis;
            bounds[0][0].x = bounds[0][1].x = center * ax[0][0];
            bounds[0][0].y = bounds[0][1].y = center * ax[0][1];
            bounds[0][0].z = bounds[0][1].z = center * ax[0][2];
            bounds[0][0] -= extents;
            bounds[0][1] += extents;
            a.AxisProjection(ax[0], out var b);
            // the other box is contained in this box
            if (!bounds[0].AddBounds(b)) return false;

            // test axis of other box
            ax[1] = a.axis;
            bounds[1][0].x = bounds[1][1].x = a.center * ax[1][0];
            bounds[1][0].y = bounds[1][1].y = a.center * ax[1][1];
            bounds[1][0].z = bounds[1][1].z = a.center * ax[1][2];
            bounds[1][0] -= a.extents;
            bounds[1][1] += a.extents;
            AxisProjection(ax[1], out b);
            // this box is contained in the other box
            if (!bounds[1].AddBounds(b)) { center = a.center; extents = a.extents; axis = a.axis; return true; }

            // test axes aligned with the vector between the box centers and one of the box axis
            var dir = a.center - center;
            dir.Normalize();
            for (var i = 2; i < 4; i++)
            {
                ax[i][0] = dir;
                ax[i][1] = ax[i - 2][MathX.Min3Index(dir * ax[i - 2][0], dir * ax[i - 2][1], dir * ax[i - 2][2])];
                ax[i][1] = ax[i][1] - (ax[i][1] * dir) * dir;
                ax[i][1].Normalize();
                ax[i][2].Cross(dir, ax[i][1]);

                AxisProjection(ax[i], out bounds[i]);
                a.AxisProjection(ax[i], out b);
                bounds[i].AddBounds(b);
            }

            // get the bounds with the smallest volume
            var bestv = MathX.INFINITY;
            var besti = 0;
            for (var i = 0; i < 4; i++)
            {
                var v = bounds[i].Volume;
                if (v < bestv) { bestv = v; besti = i; }
            }

            // create a box from the smallest bounds axis pair
            center = (bounds[besti][0] + bounds[besti][1]) * 0.5f;
            extents = bounds[besti][1] - center;
            center *= ax[besti];
            axis = ax[besti];

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Box Expand(float d)                 // return box expanded in all directions with the given value
            => new(center, extents + new Vector3(d, d, d), axis);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Box ExpandSelf(float d)                 // expand box in all directions with the given value
        {
            extents.x += d;
            extents.y += d;
            extents.z += d;
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Box Translate(in Vector3 translation)  // return translated box
            => new(center + translation, extents, axis);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Box TranslateSelf(in Vector3 translation)      // translate this box
        {
            center += translation;
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Box Rotate(in Matrix3x3 rotation)          // return rotated box
            => new(center * rotation, extents, axis * rotation);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Box RotateSelf(in Matrix3x3 rotation)          // rotate this box
        {
            center *= rotation;
            axis *= rotation;
            return this;
        }

        public float PlaneDistance(in Plane plane)
        {
            var pn = plane.Normal;
            var d1 = plane.Distance(center);
            var d2 =
                MathX.Fabs(extents.x * pn.x) +
                MathX.Fabs(extents.y * pn.y) +
                MathX.Fabs(extents.z * pn.z);

            if (d1 - d2 > 0f) return d1 - d2;
            if (d1 + d2 < 0f) return d1 + d2;
            return 0f;
        }

        public PLANESIDE PlaneSide(in Plane plane, float epsilon = Plane.ON_EPSILON)
        {
            var pn = plane.Normal;
            var d1 = plane.Distance(center);
            var d2 =
                MathX.Fabs(extents.x * pn.x) +
                MathX.Fabs(extents.y * pn.y) +
                MathX.Fabs(extents.z * pn.z);

            if (d1 - d2 > epsilon) return PLANESIDE.FRONT;
            if (d1 + d2 < -epsilon) return PLANESIDE.BACK;
            return PLANESIDE.CROSS;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ContainsPoint(in Vector3 p)           // includes touching
        {
            var lp = p - center;
            return MathX.Fabs(lp * axis[0]) <= extents.x &&
                   MathX.Fabs(lp * axis[1]) <= extents.y &&
                   MathX.Fabs(lp * axis[2]) <= extents.z;
        }

        public bool IntersectsBox(in Box a)           // includes touching
        {
            var c = new float[3, 3];      // matrix c = axis.Transpose() * a.axis
            var ac = new float[3, 3];     // absolute values of c
            var axisdir = new float[3];   // axis[i] * dir
            float d, e0, e1;    // distance between centers and projected extents

            var dir = a.center - center; // vector between centers

            // axis C0 + t * A0
            c[0, 0] = axis[0] * a.axis[0];
            c[0, 1] = axis[0] * a.axis[1];
            c[0, 2] = axis[0] * a.axis[2];
            axisdir[0] = axis[0] * dir;
            ac[0, 0] = MathX.Fabs(c[0, 0]);
            ac[0, 1] = MathX.Fabs(c[0, 1]);
            ac[0, 2] = MathX.Fabs(c[0, 2]);

            d = MathX.Fabs(axisdir[0]);
            e0 = extents.x;
            e1 = a.extents.x * ac[0, 0] + a.extents.y * ac[0, 1] + a.extents.z * ac[0, 2];
            if (d > e0 + e1) return false;

            // axis C0 + t * A1
            c[1, 0] = axis[1] * a.axis[0];
            c[1, 1] = axis[1] * a.axis[1];
            c[1, 2] = axis[1] * a.axis[2];
            axisdir[1] = axis[1] * dir;
            ac[1, 0] = MathX.Fabs(c[1, 0]);
            ac[1, 1] = MathX.Fabs(c[1, 1]);
            ac[1, 2] = MathX.Fabs(c[1, 2]);

            d = MathX.Fabs(axisdir[1]);
            e0 = extents.y;
            e1 = a.extents.x * ac[1, 0] + a.extents.y * ac[1, 1] + a.extents.z * ac[1, 2];
            if (d > e0 + e1) return false;

            // axis C0 + t * A2
            c[2, 0] = axis[2] * a.axis[0];
            c[2, 1] = axis[2] * a.axis[1];
            c[2, 2] = axis[2] * a.axis[2];
            axisdir[2] = axis[2] * dir;
            ac[2, 0] = MathX.Fabs(c[2, 0]);
            ac[2, 1] = MathX.Fabs(c[2, 1]);
            ac[2, 2] = MathX.Fabs(c[2, 2]);

            d = MathX.Fabs(axisdir[2]);
            e0 = extents.z;
            e1 = a.extents.x * ac[2, 0] + a.extents.y * ac[2, 1] + a.extents.z * ac[2, 2];
            if (d > e0 + e1) return false;

            // axis C0 + t * B0
            d = MathX.Fabs(a.axis[0] * dir);
            e0 = extents.x * ac[0, 0] + extents.y * ac[1, 0] + extents.z * ac[2, 0];
            e1 = a.extents.x;
            if (d > e0 + e1) return false;

            // axis C0 + t * B1
            d = MathX.Fabs(a.axis[1] * dir);
            e0 = extents.x * ac[0, 1] + extents.y * ac[1, 1] + extents.z * ac[2, 1];
            e1 = a.extents.y;
            if (d > e0 + e1) return false;

            // axis C0 + t * B2
            d = MathX.Fabs(a.axis[2] * dir);
            e0 = extents.x * ac[0, 2] + extents.y * ac[1, 2] + extents.z * ac[2, 2];
            e1 = a.extents.z;
            if (d > e0 + e1) return false;

            // axis C0 + t * A0xB0
            d = MathX.Fabs(axisdir[2] * c[1, 0] - axisdir[1] * c[2, 0]);
            e0 = extents.y * ac[2, 0] + extents.z * ac[1, 0];
            e1 = a.extents.y * ac[0, 2] + a.extents.z * ac[0, 1];
            if (d > e0 + e1) return false;

            // axis C0 + t * A0xB1
            d = MathX.Fabs(axisdir[2] * c[1, 1] - axisdir[1] * c[2, 1]);
            e0 = extents.y * ac[2, 1] + extents.z * ac[1, 1];
            e1 = a.extents.x * ac[0, 2] + a.extents.z * ac[0, 0];
            if (d > e0 + e1) return false;

            // axis C0 + t * A0xB2
            d = MathX.Fabs(axisdir[2] * c[1, 2] - axisdir[1] * c[2, 2]);
            e0 = extents.y * ac[2, 2] + extents.z * ac[1, 2];
            e1 = a.extents.x * ac[0, 1] + a.extents.y * ac[0, 0];
            if (d > e0 + e1) return false;

            // axis C0 + t * A1xB0
            d = MathX.Fabs(axisdir[0] * c[2, 0] - axisdir[2] * c[0, 0]);
            e0 = extents.x * ac[2, 0] + extents.z * ac[0, 0];
            e1 = a.extents.y * ac[1, 2] + a.extents.z * ac[1, 1];
            if (d > e0 + e1) return false;

            // axis C0 + t * A1xB1
            d = MathX.Fabs(axisdir[0] * c[2, 1] - axisdir[2] * c[0, 1]);
            e0 = extents.x * ac[2, 1] + extents.z * ac[0, 1];
            e1 = a.extents.x * ac[1, 2] + a.extents.z * ac[1, 0];
            if (d > e0 + e1) return false;

            // axis C0 + t * A1xB2
            d = MathX.Fabs(axisdir[0] * c[2, 2] - axisdir[2] * c[0, 2]);
            e0 = extents.x * ac[2, 2] + extents.z * ac[0, 2];
            e1 = a.extents.x * ac[1, 1] + a.extents.y * ac[1, 0];
            if (d > e0 + e1) return false;

            // axis C0 + t * A2xB0
            d = MathX.Fabs(axisdir[1] * c[0, 0] - axisdir[0] * c[1, 0]);
            e0 = extents.x * ac[1, 0] + extents.y * ac[0, 0];
            e1 = a.extents.y * ac[2, 2] + a.extents.z * ac[2, 1];
            if (d > e0 + e1) return false;

            // axis C0 + t * A2xB1
            d = MathX.Fabs(axisdir[1] * c[0, 1] - axisdir[0] * c[1, 1]);
            e0 = extents.x * ac[1, 1] + extents.y * ac[0, 1];
            e1 = a.extents.x * ac[2, 2] + a.extents.z * ac[2, 0];
            if (d > e0 + e1) return false;

            // axis C0 + t * A2xB2
            d = MathX.Fabs(axisdir[1] * c[0, 2] - axisdir[0] * c[1, 2]);
            e0 = extents.x * ac[1, 2] + extents.y * ac[0, 2];
            e1 = a.extents.x * ac[2, 1] + a.extents.y * ac[2, 0];
            if (d > e0 + e1) return false;
            return true;
        }

        // Returns true if the line intersects the box between the start and end point.
        public bool LineIntersection(in Vector3 start, in Vector3 end)
        {
            var lineDir = 0.5f * (end - start);
            var lineCenter = start + lineDir;
            var dir = lineCenter - center;

            var ld_x = MathX.Fabs(lineDir * axis[0]); if (MathX.Fabs(dir * axis[0]) > extents.x + ld_x) return false;
            var ld_y = MathX.Fabs(lineDir * axis[1]); if (MathX.Fabs(dir * axis[1]) > extents.y + ld_y) return false;
            var ld_z = MathX.Fabs(lineDir * axis[2]); if (MathX.Fabs(dir * axis[2]) > extents.z + ld_z) return false;

            var cross = lineDir.Cross(dir);
            if (MathX.Fabs(cross * axis[0]) > extents.y * ld_z + extents.z * ld_y) return false;
            if (MathX.Fabs(cross * axis[1]) > extents.x * ld_z + extents.z * ld_x) return false;
            if (MathX.Fabs(cross * axis[2]) > extents.x * ld_y + extents.y * ld_x) return false;
            return true;
        }

        static bool BoxPlaneClip(float denom, float numer, ref float scale0, ref float scale1)
        {
            if (denom > 0f)
            {
                if (numer > denom * scale1) return false;
                if (numer > denom * scale0) scale0 = numer / denom;
                return true;
            }
            else if (denom < 0f)
            {
                if (numer > denom * scale0) return false;
                if (numer > denom * scale1) scale1 = numer / denom;
                return true;
            }
            else return numer <= 0f;
        }

        // Returns true if the ray intersects the box.
        // The ray can intersect the box in both directions from the start point.
        // If start is inside the box then scale1< 0 and scale2> 0.
        // intersection points are (start + dir * scale1) and (start + dir * scale2)
        public bool RayIntersection(in Vector3 start, in Vector3 dir, out float scale1, out float scale2)
        {
            var localStart = (start - center) * axis.Transpose();
            var localDir = dir * axis.Transpose();

            scale1 = -MathX.INFINITY;
            scale2 = MathX.INFINITY;
            return
                BoxPlaneClip(localDir.x, -localStart.x - extents.x, ref scale1, ref scale2) &&
                BoxPlaneClip(-localDir.x, localStart.x - extents.x, ref scale1, ref scale2) &&
                BoxPlaneClip(localDir.y, -localStart.y - extents.y, ref scale1, ref scale2) &&
                BoxPlaneClip(-localDir.y, localStart.y - extents.y, ref scale1, ref scale2) &&
                BoxPlaneClip(localDir.z, -localStart.z - extents.z, ref scale1, ref scale2) &&
                BoxPlaneClip(-localDir.z, localStart.z - extents.z, ref scale1, ref scale2);
        }

        // Tight box for a collection of points.
        public void FromPoints(Vector3[] points, int numPoints)
        {
            int i; float invNumPoints, sumXX, sumXY, sumXZ, sumYY, sumYZ, sumZZ;
            Vector3 dir; Bounds bounds = new(); MatrixX eigenVectors = new(); VectorX eigenValues = new();

            // compute mean of points
            center = points[0];
            for (i = 1; i < numPoints; i++) center += points[i];
            invNumPoints = 1f / numPoints;
            center *= invNumPoints;

            // compute covariances of points
            sumXX = 0f; sumXY = 0f; sumXZ = 0f;
            sumYY = 0f; sumYZ = 0f; sumZZ = 0f;
            for (i = 0; i < numPoints; i++)
            {
                dir = points[i] - center;
                sumXX += dir.x * dir.x;
                sumXY += dir.x * dir.y;
                sumXZ += dir.x * dir.z;
                sumYY += dir.y * dir.y;
                sumYZ += dir.y * dir.z;
                sumZZ += dir.z * dir.z;
            }
            sumXX *= invNumPoints;
            sumXY *= invNumPoints;
            sumXZ *= invNumPoints;
            sumYY *= invNumPoints;
            sumYZ *= invNumPoints;
            sumZZ *= invNumPoints;

            // compute eigenvectors for covariance matrix
            eigenValues.SetData(3, VectorX.VECX_ALLOCA(3));
            eigenVectors.SetData(3, 3, MatrixX.MATX_ALLOCA(3 * 3));

            eigenVectors[0][0] = sumXX;
            eigenVectors[0][1] = sumXY;
            eigenVectors[0][2] = sumXZ;
            eigenVectors[1][0] = sumXY;
            eigenVectors[1][1] = sumYY;
            eigenVectors[1][2] = sumYZ;
            eigenVectors[2][0] = sumXZ;
            eigenVectors[2][1] = sumYZ;
            eigenVectors[2][2] = sumZZ;
            eigenVectors.Eigen_SolveSymmetric(ref eigenValues);
            eigenVectors.Eigen_SortIncreasing(eigenValues);

            axis[0].x = eigenVectors[0][0];
            axis[0].y = eigenVectors[0][1];
            axis[0].z = eigenVectors[0][2];
            axis[1].x = eigenVectors[1][0];
            axis[1].y = eigenVectors[1][1];
            axis[1].z = eigenVectors[1][2];
            axis[2].x = eigenVectors[2][0];
            axis[2].y = eigenVectors[2][1];
            axis[2].z = eigenVectors[2][2];

            extents.x = eigenValues[0];
            extents.y = eigenValues[0];
            extents.z = eigenValues[0];

            // refine by calculating the bounds of the points projected onto the axis and adjusting the center and extents
            bounds.Clear();
            for (i = 0; i < numPoints; i++) bounds.AddPoint(new Vector3(points[i] * axis[0], points[i] * axis[1], points[i] * axis[2]));
            center = (bounds[0] + bounds[1]) * 0.5f;
            extents = bounds[1] - center;
            center *= axis;
        }

        // Most tight box for the translational movement of the given point.
        public void FromPointTranslation(in Vector3 point, in Vector3 translation)
            => throw new NotImplementedException();

        // Most tight box for the translational movement of the given box.
        public void FromBoxTranslation(in Box box, in Vector3 translation)
            => throw new NotImplementedException();

        // Most tight bounds for the rotational movement of the given point.
        public void FromPointRotation(in Vector3 point, in Rotation rotation)
            => throw new NotImplementedException();

        // Most tight box for the rotational movement of the given box.
        public void FromBoxRotation(in Box box, in Rotation rotation)
            => throw new NotImplementedException();

        public void ToPoints(out Vector3[] points)
        {
            Matrix3x3 ax = new();
            ax[0] = extents.x * axis[0];
            ax[1] = extents.y * axis[1];
            ax[2] = extents.z * axis[2];
            var temp0 = center - ax[0];
            var temp1 = center + ax[0];
            var temp2 = ax[1] - ax[2];
            var temp3 = ax[1] + ax[2];
            points = new[] {
                temp0 - temp3,
                temp1 - temp3,
                temp1 + temp2,
                temp0 + temp2,
                temp0 - temp2,
                temp1 - temp2,
                temp1 + temp3,
                temp0 + temp3,
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Sphere ToSphere()
            => new(center, extents.Length);

        // calculates the projection of this box onto the given axis
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AxisProjection(in Vector3 dir, out float min, out float max)
        {
            var d1 = dir * center;
            var d2 =
                MathX.Fabs(extents.x * (dir * axis[0])) +
                MathX.Fabs(extents.y * (dir * axis[1])) +
                MathX.Fabs(extents.z * (dir * axis[2]));
            min = d1 - d2;
            max = d1 + d2;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AxisProjection(in Matrix3x3 ax, out Bounds bounds)
        {
            bounds = new Bounds();
            for (var i = 0; i < 3; i++)
            {
                var d1 = ax[i] * center;
                var d2 =
                    MathX.Fabs(extents.x * (ax[i] * axis[0])) +
                    MathX.Fabs(extents.y * (ax[i] * axis[1])) +
                    MathX.Fabs(extents.z * (ax[i] * axis[2]));
                bounds[0][i] = d1 - d2;
                bounds[1][i] = d1 + d2;
            }
        }

        // calculates the silhouette of the box
        public int GetProjectionSilhouetteVerts(in Vector3 projectionOrigin, Vector3[] silVerts)
        {
            ToPoints(out var points);

            float f; int planeBits;
            var dir1 = points[0] - projectionOrigin;
            var dir2 = points[6] - projectionOrigin;
            f = dir1 * axis[0]; planeBits = MathX.FLOATSIGNBITNOTSET_(f);
            f = dir2 * axis[0]; planeBits |= MathX.FLOATSIGNBITSET_(f) << 1;
            f = dir1 * axis[1]; planeBits |= MathX.FLOATSIGNBITNOTSET_(f) << 2;
            f = dir2 * axis[1]; planeBits |= MathX.FLOATSIGNBITSET_(f) << 3;
            f = dir1 * axis[2]; planeBits |= MathX.FLOATSIGNBITNOTSET_(f) << 4;
            f = dir2 * axis[2]; planeBits |= MathX.FLOATSIGNBITSET_(f) << 5;

            var index = BoxPlaneBitsSilVerts[planeBits];
            for (var i = 0; i < index[0]; i++) silVerts[i] = points[index[i + 1]];
            return index[0];
        }

        public int GetParallelProjectionSilhouetteVerts(in Vector3 projectionDir, Vector3[] silVerts)
        {
            ToPoints(out var points);

            float f; int planeBits = 0;
            f = projectionDir * axis[0]; if (MathX.FLOATNOTZERO(f)) planeBits = 1 << MathX.FLOATSIGNBITSET_(f);
            f = projectionDir * axis[1]; if (MathX.FLOATNOTZERO(f)) planeBits |= 4 << MathX.FLOATSIGNBITSET_(f);
            f = projectionDir * axis[2]; if (MathX.FLOATNOTZERO(f)) planeBits |= 16 << MathX.FLOATSIGNBITSET_(f);

            var index = BoxPlaneBitsSilVerts[planeBits];
            for (var i = 0; i < index[0]; i++) silVerts[i] = points[index[i + 1]];
            return index[0];
        }
    }
}