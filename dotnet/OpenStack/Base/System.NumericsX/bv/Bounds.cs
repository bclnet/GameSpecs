using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace System.NumericsX
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Bounds
    {
        public static readonly Bounds zero = new(Vector3.origin, Vector3.origin);

        public Vector3 b0;
        public Vector3 b1;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Bounds(in Bounds a)
            => this = a;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Bounds(in Vector3 mins, in Vector3 maxs)
        {
            b0 = mins;
            b1 = maxs;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Bounds(in Vector3 point)
        {
            b0 = point;
            b1 = point;
        }

        public unsafe ref Vector3 this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { fixed (Vector3* _ = &b0) return ref _[index]; }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Bounds operator +(in Bounds _, in Vector3 t)                // returns translated bounds
            => new(_.b0 + t, _.b1 + t);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Bounds operator *(in Bounds _, in Matrix3x3 r)              // returns rotated bounds
        {
            Bounds bounds = new();
            bounds.FromTransformedBounds(_, Vector3.origin, r);
            return bounds;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Bounds operator +(in Bounds _, in Bounds a)
        {
            Bounds newBounds = new(_);
            newBounds.AddBounds(a);
            return newBounds;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Bounds operator -(in Bounds _, in Bounds a)
        {
            Debug.Assert(
                _.b1.x - _.b0.x > a.b1.x - a.b0.x &&
                _.b1.y - _.b0.y > a.b1.y - a.b0.y &&
                _.b1.z - _.b0.z > a.b1.z - a.b0.z);
            return new(
                new Vector3(_.b0.x + a.b1.x, _.b0.y + a.b1.y, _.b0.z + a.b1.z),
                new Vector3(_.b1.x + a.b0.x, _.b1.y + a.b0.y, _.b1.z + a.b0.z));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Compare(in Bounds a)                          // exact compare, no epsilon
            => b0.Compare(a.b0) && b1.Compare(a.b1);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Compare(in Bounds a, float epsilon)   // compare with epsilon
            => b0.Compare(a.b0, epsilon) && b1.Compare(a.b1, epsilon);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(in Bounds _, in Bounds a)                      // exact compare, no epsilon
            => _.Compare(a);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(in Bounds _, in Bounds a)                      // exact compare, no epsilon
            => !_.Compare(a);
        public override bool Equals(object obj)
            => obj is Bounds q && Compare(q);
        public override int GetHashCode()
            => b0.GetHashCode() ^ b1.GetHashCode();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()                                    // inside out bounds
        {
            b0.x = b0.y = b0.z = MathX.INFINITY;
            b1.x = b1.y = b1.z = -MathX.INFINITY;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Zero()                                 // single point at origin
            =>
            b0.x = b0.y = b0.z =
            b1.x = b1.y = b1.z = 0;

        public Vector3 Center                      // returns center of bounds
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => new((b1.x + b0.x) * 0.5f, (b1.y + b0.y) * 0.5f, (b1.z + b0.z) * 0.5f);
        }

        public float GetRadius()                       // returns the radius relative to the bounds origin
        {
            var total = 0f;
            for (var i = 0; i < 3; i++)
            {
                var b0_ = (float)MathX.Fabs(b0[i]);
                var b1_ = (float)MathX.Fabs(b1[i]);
                if (b0_ > b1_) total += b0_ * b0_;
                else total += b1_ * b1_;
            }
            return MathX.Sqrt(total);
        }
        public float GetRadius(in Vector3 center)     // returns the radius relative to the given center
        {
            var total = 0f;
            for (var i = 0; i < 3; i++)
            {
                var b0_ = (float)MathX.Fabs(center[i] - b0[i]);
                var b1_ = (float)MathX.Fabs(b1[i] - center[i]);
                if (b0_ > b1_) total += b0_ * b0_;
                else total += b1_ * b1_;
            }
            return MathX.Sqrt(total);
        }

        public float Volume                       // returns the volume of the bounds
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => b0.x >= b1.x || b0.y >= b1.y || b0.z >= b1.z ? 0f
                : (b1.x - b0.x) * (b1.y - b0.y) * (b1.z - b0.z);
        }

        public bool IsCleared
        {                        // returns true if bounds are inside out
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => b0.x > b1.x;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AddPoint(in Vector3 v)                    // add the point, returns true if the bounds expanded
        {
            var expanded = false;
            if (v.x < b0.x) { b0.x = v.x; expanded = true; }
            if (v.x > b1.x) { b1.x = v.x; expanded = true; }
            if (v.y < b0.y) { b0.y = v.y; expanded = true; }
            if (v.y > b1.y) { b1.y = v.y; expanded = true; }
            if (v.z < b0.z) { b0.z = v.z; expanded = true; }
            if (v.z > b1.z) { b1.z = v.z; expanded = true; }
            return expanded;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AddBounds(in Bounds a)                    // add the bounds, returns true if the bounds expanded
        {
            var expanded = false;
            if (a.b0.x < b0.x) { b0.x = a.b0.x; expanded = true; }
            if (a.b0.y < b0.y) { b0.y = a.b0.y; expanded = true; }
            if (a.b0.z < b0.z) { b0.z = a.b0.z; expanded = true; }
            if (a.b1.x > b1.x) { b1.x = a.b1.x; expanded = true; }
            if (a.b1.y > b1.y) { b1.y = a.b1.y; expanded = true; }
            if (a.b1.z > b1.z) { b1.z = a.b1.z; expanded = true; }
            return expanded;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Bounds Intersect(in Bounds a)          // return intersection of this bounds with the given bounds
        {
            Bounds n = new();
            n.b0.x = a.b0.x > b0.x ? a.b0.x : b0.x;
            n.b0.y = a.b0.y > b0.y ? a.b0.y : b0.y;
            n.b0.z = a.b0.z > b0.z ? a.b0.z : b0.z;
            n.b1.x = a.b1.x < b1.x ? a.b1.x : b1.x;
            n.b1.y = a.b1.y < b1.y ? a.b1.y : b1.y;
            n.b1.z = a.b1.z < b1.z ? a.b1.z : b1.z;
            return n;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Bounds IntersectSelf(in Bounds a)              // intersect this bounds with the given bounds
        {
            if (a.b0.x > b0.x) b0.x = a.b0.x;
            if (a.b0.y > b0.y) b0.y = a.b0.y;
            if (a.b0.z > b0.z) b0.z = a.b0.z;
            if (a.b1.x < b1.x) b1.x = a.b1.x;
            if (a.b1.y < b1.y) b1.y = a.b1.y;
            if (a.b1.z < b1.z) b1.z = a.b1.z;
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Bounds Expand(float d)                  // return bounds expanded in all directions with the given value
            => new(
            new Vector3(b0.x - d, b0.y - d, b0.z - d),
            new Vector3(b1.x + d, b1.y + d, b1.z + d));
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Bounds ExpandSelf(float d)                  // expand bounds in all directions with the given value
        {
            b0.x -= d; b0.y -= d; b0.z -= d;
            b1.x += d; b1.y += d; b1.z += d;
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Bounds Translate(in Vector3 translation)   // return translated bounds
        => new(b0 + translation, b1 + translation);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Bounds TranslateSelf(in Vector3 translation)       // translate this bounds
        {
            b0 += translation;
            b1 += translation;
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Bounds Rotate(in Matrix3x3 rotation)           // return rotated bounds
        {
            Bounds bounds = new();
            bounds.FromTransformedBounds(this, Vector3.origin, rotation);
            return bounds;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Bounds RotateSelf(in Matrix3x3 rotation)           // rotate this bounds
        {
            FromTransformedBounds(this, Vector3.origin, rotation);
            return this;
        }

        public float PlaneDistance(in Plane plane)
        {
            var center = (b0 + b1) * 0.5f;

            var pn = plane.Normal;
            var d1 = plane.Distance(center);
            var d2 =
                MathX.Fabs((b1.x - center.x) * pn.x) +
                MathX.Fabs((b1.y - center.y) * pn.y) +
                MathX.Fabs((b1.z - center.z) * pn.z); //: opt

            if (d1 - d2 > 0f) return d1 - d2;
            if (d1 + d2 < 0f) return d1 + d2;
            return 0f;
        }

        public PLANESIDE PlaneSide(in Plane plane, float epsilon = Plane.ON_EPSILON)
        {
            var center = (b0 + b1) * 0.5f;

            var pn = plane.Normal;
            var d1 = plane.Distance(center);
            var d2 =
                MathX.Fabs((b1.x - center.x) * pn[0]) +
                MathX.Fabs((b1.y - center.y) * pn[1]) +
                MathX.Fabs((b1.z - center.z) * pn[2]);

            if (d1 - d2 > epsilon) return PLANESIDE.FRONT;
            if (d1 + d2 < -epsilon) return PLANESIDE.BACK;
            return PLANESIDE.CROSS;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ContainsPoint(in Vector3 p)           // includes touching
            =>
            p.x >= b0.x && p.y >= b0.y && p.z >= b0.z &&
            p.x <= b1.x && p.y <= b1.y && p.z <= b1.z;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IntersectsBounds(in Bounds a) // includes touching
            =>
            a.b1.x >= b0.x && a.b1.y >= b0.y && a.b1.z >= b0.z &&
            a.b0.x <= b1.x && a.b0.y <= b1.y && a.b0.z <= b1.z;

        // Returns true if the line intersects the bounds between the start and end point.
        public bool LineIntersection(in Vector3 start, in Vector3 end)
        {
            var center = (b0 + b1) * 0.5f;
            var extents = b1 - center;
            var lineDir = 0.5f * (end - start);
            var lineCenter = start + lineDir;
            var dir = lineCenter - center;

            var ld_x = MathX.Fabs(lineDir.x); if (MathX.Fabs(dir.x) > extents.x + ld_x) return false;
            var ld_y = MathX.Fabs(lineDir.y); if (MathX.Fabs(dir.y) > extents.y + ld_y) return false;
            var ld_z = MathX.Fabs(lineDir.z); if (MathX.Fabs(dir.z) > extents.z + ld_z) return false;

            var cross = lineDir.Cross(dir);
            if (MathX.Fabs(cross.x) > extents.y * ld_z + extents.z * ld_y) return false;
            if (MathX.Fabs(cross.y) > extents.x * ld_z + extents.z * ld_x) return false;
            if (MathX.Fabs(cross.z) > extents.x * ld_y + extents.y * ld_x) return false;
            return true;
        }

        // Returns true if the ray intersects the bounds.
        // The ray can intersect the bounds in both directions from the start point.
        // If start is inside the bounds it is considered an intersection with scale = 0
        // intersection point is start + dir * scale
        public bool RayIntersection(in Vector3 start, in Vector3 dir, ref float scale)
        {
            var ax0 = -1;
            int inside = 0, side;
            for (var i = 0; i < 3; i++)
            {
                if (start[i] < b0[i]) side = 0;
                else if (start[i] > b1[i]) side = 1;
                else { inside++; continue; }
                if (dir[i] == 0f) continue;
                var f = start[i] - (side == 0 ? b0[i] : b1[i]);
                if (ax0 < 0 || MathX.Fabs(f) > MathX.Fabs(scale * dir[i])) { scale = -(f / dir[i]); ax0 = i; }
            }

            // return true if the start point is inside the bounds
            if (ax0 < 0) { scale = 0f; return inside == 3; }

            var ax1 = (ax0 + 1) % 3;
            var ax2 = (ax0 + 2) % 3;
            Vector3 hit = new();
            hit[ax1] = start[ax1] + scale * dir[ax1];
            hit[ax2] = start[ax2] + scale * dir[ax2];
            return
                hit[ax1] >= b0[ax1] && hit[ax1] <= b1[ax1] &&
                hit[ax2] >= b0[ax2] && hit[ax2] <= b1[ax2];
        }

        // most tight bounds for the given transformed bounds
        public void FromTransformedBounds(in Bounds bounds, in Vector3 origin, in Matrix3x3 axis)
        {
            var center = (bounds[0] + bounds[1]) * 0.5f;
            var extents = bounds[1] - center;

            var rotatedExtents = new Vector3
            {
                x = MathX.Fabs(extents.x * axis.mat0.x) +
                    MathX.Fabs(extents.y * axis.mat1.x) +
                    MathX.Fabs(extents.z * axis.mat2.x),
                y = MathX.Fabs(extents.x * axis.mat0.y) +
                    MathX.Fabs(extents.y * axis.mat1.y) +
                    MathX.Fabs(extents.z * axis.mat2.y),
                z = MathX.Fabs(extents.x * axis.mat0.z) +
                    MathX.Fabs(extents.y * axis.mat1.z) +
                    MathX.Fabs(extents.z * axis.mat2.z),
            }; //: unroll

            center = origin + center * axis;
            b0 = center - rotatedExtents;
            b1 = center + rotatedExtents;
        }

        // most tight bounds for a point set
        public unsafe void FromPoints(Vector3[] points, int numPoints)
        {
            fixed (Vector3* _ = points) Simd.MinMax3(out b0, out b1, _, numPoints);
        }

        // Most tight bounds for the translational movement of the given point.
        public void FromPointTranslation(Vector3 point, Vector3 translation)
        {
            if (translation.x < 0f) { b0.x = point.x + translation.x; b1.x = point.x; }
            else { b0.x = point.y; b1.x = point.x + translation.x; } //: unroll
            if (translation.y < 0f) { b0.y = point.y + translation.y; b1.y = point.y; }
            else { b0.y = point.y; b1.y = point.y + translation.y; } //: unroll
            if (translation.z < 0f) { b0.z = point.z + translation.z; b1.z = point.z; }
            else { b0.z = point.z; b1.z = point.z + translation.z; } //: unroll
        }

        // Most tight bounds for the translational movement of the given bounds.
        public void FromBoundsTranslation(in Bounds bounds, in Vector3 origin, in Matrix3x3 axis, in Vector3 translation)
        {
            if (axis.IsRotated()) FromTransformedBounds(bounds, origin, axis);
            else { b0 = bounds[0] + origin; b1 = bounds[1] + origin; }
            if (translation.x < 0f) b0.x += translation.x;
            else b1.x += translation.x; //: unroll
            if (translation.y < 0f) b0.y += translation.y;
            else b1.y += translation.y; //: unroll
            if (translation.z < 0f) b0.z += translation.z;
            else b1.z += translation.z; //: unroll
        }

        // only for rotations < 180 degrees
        Bounds BoundsForPointRotation(in Vector3 start, in Rotation rotation)
        {
            var end = start * rotation;
            var axis = rotation.Vec;
            var origin = rotation.Origin + axis * (axis * (start - rotation.Origin));
            var radiusSqr = (start - origin).LengthSqr;
            var v1 = (start - origin).Cross(axis);
            var v2 = (end - origin).Cross(axis);

            Bounds bounds = new();
            for (var i = 0; i < 3; i++)
                // if the derivative changes sign along this axis during the rotation from start to end
                if ((v1[i] > 0f && v2[i] < 0f) || (v1[i] < 0f && v2[i] > 0f))
                {
                    if ((0.5f * (start[i] + end[i]) - origin[i]) > 0f) { bounds[0][i] = Math.Min(start[i], end[i]); bounds[1][i] = origin[i] + MathX.Sqrt(radiusSqr * (1f - axis[i] * axis[i])); }
                    else { bounds[0][i] = origin[i] - MathX.Sqrt(radiusSqr * (1f - axis[i] * axis[i])); bounds[1][i] = Math.Max(start[i], end[i]); }
                }
                else if (start[i] > end[i]) { bounds[0][i] = end[i]; bounds[1][i] = start[i]; }
                else { bounds[0][i] = start[i]; bounds[1][i] = end[i]; }

            return bounds;
        }

        // Most tight bounds for the rotational movement of the given point.
        public void FromPointRotation(in Vector3 point, in Rotation rotation)
        {
            if (MathX.Fabs(rotation.Angle) < 180f) this = BoundsForPointRotation(point, rotation);
            else
            {
                var radius = (point - rotation.Origin).Length;
                // FIXME: these bounds are usually way larger
                b0.Set(-radius, -radius, -radius);
                b1.Set(radius, radius, radius);
            }
        }

        // Most tight bounds for the rotational movement of the given bounds.
        public void FromBoundsRotation(in Bounds bounds, in Vector3 origin, in Matrix3x3 axis, in Rotation rotation)
        {
            if (MathX.Fabs(rotation.Angle) < 180f)
            {
                this = BoundsForPointRotation(bounds[0] * axis + origin, rotation);
                for (var i = 1; i < 8; i++)
                {
                    var point = new Vector3
                    {
                        x = bounds[(i ^ (i >> 1)) & 1].z,
                        y = bounds[(i >> 1) & 1].y,
                        z = bounds[(i >> 2) & 1].z,
                    };
                    this += BoundsForPointRotation(point * axis + origin, rotation);
                }
            }
            else
            {
                var point = (bounds[1] - bounds[0]) * 0.5f;
                var radius = (bounds[1] - point).Length + (point - rotation.Origin).Length;
                // FIXME: these bounds are usually way larger
                b0.Set(-radius, -radius, -radius);
                b1.Set(radius, radius, radius);
            }
        }

        public void ToPoints(Vector3[] points)
        {
            for (var i = 0; i < 8; i++)
            {
                points[i].x = ((i ^ (i >> 1)) & 1) == 0 ? b0.x : b1.x;
                points[i].y = ((i >> 1) & 1) == 0 ? b0.y : b1.y;
                points[i].z = ((i >> 2) & 1) == 0 ? b0.z : b1.z;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Sphere ToSphere()
        {
            Sphere sphere = new();
            sphere.Origin = (b0 + b1) * 0.5f;
            sphere.Radius = (b1 - sphere.Origin).Length;
            return sphere;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AxisProjection(in Vector3 dir, out float min, out float max)
        {
            var center = (b0 + b1) * 0.5f;
            var extents = b1 - center;

            var d1 = dir * center;
            var d2 =
                MathX.Fabs(extents.x * dir.x) +
                MathX.Fabs(extents.y * dir.y) +
                MathX.Fabs(extents.z * dir.z);

            min = d1 - d2;
            max = d1 + d2;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AxisProjection(in Vector3 origin, in Matrix3x3 axis, in Vector3 dir, out float min, out float max)
        {
            var center = (b0 + b1) * 0.5f;
            var extents = b1 - center;
            center = origin + center * axis;

            var d1 = dir * center;
            var d2 =
                MathX.Fabs(extents.x * (dir * axis.mat0)) +
                MathX.Fabs(extents.y * (dir * axis.mat1)) +
                MathX.Fabs(extents.z * (dir * axis.mat2));

            min = d1 - d2;
            max = d1 + d2;
        }
    }
}