//#define FRUSTUM_DEBUG
using System.Diagnostics;
using System.Runtime.CompilerServices;
using static System.NumericsX.Platform;

namespace System.NumericsX
{
    public class Frustum
    {
        // bit 0 = min x
        // bit 1 = max x
        // bit 2 = min y
        // bit 3 = max y
        // bit 4 = min z
        // bit 5 = max z
        static readonly int[] BoxVertPlanes = new[]{
            (1<<0) | (1<<2) | (1<<4),
            (1<<1) | (1<<2) | (1<<4),
            (1<<1) | (1<<3) | (1<<4),
            (1<<0) | (1<<3) | (1<<4),
            (1<<0) | (1<<2) | (1<<5),
            (1<<1) | (1<<2) | (1<<5),
            (1<<1) | (1<<3) | (1<<5),
            (1<<0) | (1<<3) | (1<<5)};

        Vector3 origin;      // frustum origin
        Matrix3x3 axis;        // frustum orientation
        float dNear;        // distance of near plane, dNear >= 0f
        float dFar;     // distance of far plane, dFar > dNear
        float dLeft;        // half the width at the far plane
        float dUp;      // half the height at the far plane
        float invFar;       // 1f / dFar

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Frustum(in Frustum a)
        {
            origin = a.origin;
            axis = new(a.axis);
            dNear = a.dNear;
            dFar = a.dFar;
            dLeft = a.dLeft;
            dUp = a.dUp;
            invFar = a.invFar;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Frustum()
            => dNear = dFar = 0f;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetOrigin(in Vector3 origin)
            => this.origin = origin;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetAxis(in Matrix3x3 axis)
            => this.axis = axis;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetSize(float dNear, float dFar, float dLeft, float dUp)
        {
            Debug.Assert(dNear >= 0f && dFar > dNear && dLeft > 0f && dUp > 0f);
            this.dNear = dNear;
            this.dFar = dFar;
            this.dLeft = dLeft;
            this.dUp = dUp;
            this.invFar = 1f / dFar;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetPyramid(float dNear, float dFar)
        {
            Debug.Assert(dNear >= 0f && dFar > dNear);
            this.dNear = dNear;
            this.dFar = dFar;
            this.dLeft = dFar;
            this.dUp = dFar;
            this.invFar = 1f / dFar;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void MoveNearDistance(float dNear)
        {
            Debug.Assert(dNear >= 0f);
            this.dNear = dNear;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void MoveFarDistance(float dFar)
        {
            Debug.Assert(dFar > this.dNear);
            var scale = dFar / this.dFar;
            this.dFar = dFar;
            this.dLeft *= scale;
            this.dUp *= scale;
            this.invFar = 1f / dFar;
        }

        public Vector3 Origin                     // returns frustum origin
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => origin;
        }

        public Matrix3x3 Axis                         // returns frustum orientation
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => axis;
        }

        public Vector3 Center                     // returns center of frustum
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => origin + axis[0] * ((dFar - dNear) * 0.5f);
        }

        public bool IsValid                          // returns true if the frustum is valid
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => dFar > dNear;
        }

        public float NearDistance                 // returns distance to near plane
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => dNear;
        }

        public float FarDistance                  // returns distance to far plane
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => dFar;
        }

        public float Left                         // returns left vector length
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => dLeft;
        }

        public float Up                           // returns up vector length
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => dUp;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Frustum Expand(float d)                   // returns frustum expanded in all directions with the given value
        {
            Frustum f = new(this);
            f.origin -= d * f.axis[0];
            f.dFar += 2f * d;
            f.dLeft = f.dFar * dLeft * invFar;
            f.dUp = f.dFar * dUp * invFar;
            f.invFar = 1f / dFar;
            return f;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Frustum ExpandSelf(float d)                   // expands frustum in all directions with the given value
        {
            origin -= d * axis[0];
            dFar += 2f * d;
            dLeft = dFar * dLeft * invFar;
            dUp = dFar * dUp * invFar;
            invFar = 1f / dFar;
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Frustum Translate(in Vector3 translation)    // returns translated frustum
        {
            Frustum f = new(this);
            f.origin += translation;
            return f;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Frustum TranslateSelf(in Vector3 translation)        // translates frustum
        {
            origin += translation;
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Frustum Rotate(in Matrix3x3 rotation)            // returns rotated frustum
        {
            Frustum f = new(this);
            f.axis *= rotation;
            return f;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Frustum RotateSelf(in Matrix3x3 rotation)            // rotates frustum
        {
            axis *= rotation;
            return this;
        }

        void BoxToPoints(in Vector3 center, in Vector3 extents, in Matrix3x3 axis, out Vector3[] points)
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

        public float PlaneDistance(in Plane plane)
        {
            AxisProjection(plane.Normal, out var min, out var max);
            if (min + plane[3] > 0f) return min + plane[3];
            if (max + plane[3] < 0f) return max + plane[3];
            return 0f;
        }

        public PLANESIDE PlaneSide(in Plane plane, float epsilon = Plane.ON_EPSILON)
        {
            AxisProjection(plane.Normal, out var min, out var max);
            if (min + plane[3] > epsilon) return PLANESIDE.FRONT;
            if (max + plane[3] < epsilon) return PLANESIDE.BACK;
            return PLANESIDE.CROSS;
        }

        // fast culling but might not cull everything outside the frustum
        public bool CullPoint(in Vector3 point)
        {
            // transform point to frustum space
            var p = (point - origin) * axis.Transpose();
            // test whether or not the point is within the frustum
            if (p.x < dNear || p.x > dFar) return true;
            var scale = p.x * invFar;
            if (MathX.Fabs(p.y) > dLeft * scale) return true;
            if (MathX.Fabs(p.z) > dUp * scale) return true;
            return false;
        }

        // Tests if any of the planes of the frustum can be used as a separating plane.
        // 24 muls best case
        // 37 muls worst case
        public bool CullBounds(in Bounds bounds)
        {
            var center = (bounds[0] + bounds[1]) * 0.5f;
            var extents = bounds[1] - center;

            // transform the bounds into the space of this frustum
            var localOrigin = (center - origin) * axis.Transpose();
            var localAxis = axis.Transpose();

            return CullLocalBox(localOrigin, extents, localAxis);
        }

        // Tests if any of the planes of the frustum can be used as a separating plane.
        // 39 muls best case
        // 61 muls worst case
        public bool CullBox(in Box box)
        {
            // transform the box into the space of this frustum
            var localOrigin = (box.Center - origin) * axis.Transpose();
            var localAxis = box.Axis * axis.Transpose();

            return CullLocalBox(localOrigin, box.Extents, localAxis);
        }

        // Tests if any of the planes of the frustum can be used as a separating plane.
        // 9 muls best case
        // 21 muls worst case
        public bool CullSphere(in Sphere sphere)
        {
            var center = (sphere.Origin - origin) * axis.Transpose();
            var r = sphere.Radius;

            // test near plane
            if (dNear - center.x > r) return true;

            // test far plane
            if (center.x - dFar > r) return true;

            var rs = r * r;
            var sFar = dFar * dFar;

            // test left/right planes
            var d = dFar * MathX.Fabs(center.y) - dLeft * center.x;
            if ((d * d) > rs * (sFar + dLeft * dLeft)) return true;

            // test up/down planes
            d = dFar * MathX.Fabs(center.z) - dUp * center.x;
            if ((d * d) > rs * (sFar + dUp * dUp)) return true;

            return false;
        }

        // Tests if any of the planes of this frustum can be used as a separating plane.
        // 58 muls best case
        // 88 muls worst case
        public bool CullFrustum(in Frustum frustum)
        {
            // transform the given frustum into the space of this frustum
            Frustum localFrustum = new(frustum);
            localFrustum.origin = (frustum.origin - origin) * axis.Transpose();
            localFrustum.axis = frustum.axis * axis.Transpose();

            localFrustum.ToIndexPointsAndCornerVecs(out var indexPoints, out var cornerVecs);

            return CullLocalFrustum(localFrustum, indexPoints, cornerVecs);
        }

        public unsafe bool CullWinding(in Winding winding)
        {
            var localPoints = stackalloc Vector3[winding.NumPoints + Vector3.ALLOC16]; localPoints = (Vector3*)_alloca16(localPoints);
            var pointCull = stackalloc int[winding.NumPoints + intX.ALLOC16]; pointCull = (int*)_alloca16(pointCull);

            var transpose = axis.Transpose();
            for (var i = 0; i < winding.NumPoints; i++) localPoints[i] = (winding[i].ToVec3() - origin) * transpose;

            return CullLocalWinding(localPoints, winding.NumPoints, pointCull);
        }

        // exact intersection tests
        public bool ContainsPoint(in Vector3 point)
            => !CullPoint(point);

        public bool IntersectsBounds(in Bounds bounds)
        {
            var center = (bounds[0] + bounds[1]) * 0.5f;
            var extents = bounds[1] - center;

            var localOrigin = (center - origin) * axis.Transpose();
            var localAxis = axis.Transpose();

            if (CullLocalBox(localOrigin, extents, localAxis)) return false;

            ToIndexPointsAndCornerVecs(out var indexPoints, out var cornerVecs);

            if (BoundsCullLocalFrustum(bounds, this, indexPoints, cornerVecs)) return false;

            Swap(ref indexPoints[2], ref indexPoints[3]);
            Swap(ref indexPoints[6], ref indexPoints[7]);

            if (LocalFrustumIntersectsBounds(indexPoints, bounds)) return true;

            BoxToPoints(localOrigin, extents, localAxis, out indexPoints);

            if (LocalFrustumIntersectsFrustum(indexPoints, true)) return true;

            return false;
        }

        public bool IntersectsBox(in Box box)
        {
            var localOrigin = (box.Center - origin) * axis.Transpose();
            var localAxis = box.Axis * axis.Transpose();

            if (CullLocalBox(localOrigin, box.Extents, localAxis)) return false;

            Frustum localFrustum = new(this);
            localFrustum.origin = (origin - box.Center) * box.Axis.Transpose();
            localFrustum.axis = axis * box.Axis.Transpose();
            localFrustum.ToIndexPointsAndCornerVecs(out var indexPoints, out var cornerVecs);

            if (BoundsCullLocalFrustum(new Bounds(-box.Extents, box.Extents), localFrustum, indexPoints, cornerVecs)) return false;

            Swap(ref indexPoints[2], ref indexPoints[3]);
            Swap(ref indexPoints[6], ref indexPoints[7]);

            if (LocalFrustumIntersectsBounds(indexPoints, new Bounds(-box.Extents, box.Extents))) return true;

            BoxToPoints(localOrigin, box.Extents, localAxis, out indexPoints);

            if (LocalFrustumIntersectsFrustum(indexPoints, true)) return true;

            return false;
        }

        static int VORONOI_INDEX(int x, int y, int z) => x + y * 3 + z * 9;
        const int VORONOI_INDEX_000 = 0 + 0 * 3 + 0 * 9;
        const int VORONOI_INDEX_100 = 1 + 0 * 3 + 0 * 9;
        const int VORONOI_INDEX_200 = 2 + 0 * 3 + 0 * 9;
        const int VORONOI_INDEX_010 = 0 + 1 * 3 + 0 * 9;
        const int VORONOI_INDEX_020 = 0 + 2 * 3 + 0 * 9;
        const int VORONOI_INDEX_001 = 0 + 0 * 3 + 1 * 9;
        const int VORONOI_INDEX_002 = 0 + 0 * 3 + 2 * 9;
        const int VORONOI_INDEX_111 = 1 + 1 * 3 + 1 * 9;
        const int VORONOI_INDEX_211 = 2 + 1 * 3 + 1 * 9;
        const int VORONOI_INDEX_121 = 1 + 2 * 3 + 1 * 9;
        const int VORONOI_INDEX_221 = 2 + 2 * 3 + 1 * 9;
        const int VORONOI_INDEX_112 = 1 + 1 * 3 + 2 * 9;
        const int VORONOI_INDEX_212 = 2 + 1 * 3 + 2 * 9;
        const int VORONOI_INDEX_122 = 1 + 2 * 3 + 2 * 9;
        const int VORONOI_INDEX_222 = 2 + 2 * 3 + 2 * 9;
        const int VORONOI_INDEX_110 = 1 + 1 * 3 + 0 * 9;
        const int VORONOI_INDEX_210 = 2 + 1 * 3 + 0 * 9;
        const int VORONOI_INDEX_120 = 1 + 2 * 3 + 0 * 9;
        const int VORONOI_INDEX_220 = 2 + 2 * 3 + 0 * 9;
        const int VORONOI_INDEX_101 = 1 + 0 * 3 + 1 * 9;
        const int VORONOI_INDEX_201 = 2 + 0 * 3 + 1 * 9;
        const int VORONOI_INDEX_011 = 0 + 1 * 3 + 1 * 9;
        const int VORONOI_INDEX_021 = 0 + 2 * 3 + 1 * 9;
        const int VORONOI_INDEX_102 = 1 + 0 * 3 + 2 * 9;
        const int VORONOI_INDEX_202 = 2 + 0 * 3 + 2 * 9;
        const int VORONOI_INDEX_012 = 0 + 1 * 3 + 2 * 9;
        const int VORONOI_INDEX_022 = 0 + 2 * 3 + 2 * 9;

        public bool IntersectsSphere(in Sphere sphere)
        {
            if (CullSphere(sphere)) return false;

            int x = 0, y = 0, z = 0;
            Vector3 dir = new();
            dir.Zero();

            var p = (sphere.Origin - origin) * axis.Transpose();

            float scale;
            if (p.x <= dNear)
            {
                scale = dNear * invFar;
                dir.y = MathX.Fabs(p.y) - dLeft * scale;
                dir.z = MathX.Fabs(p.z) - dUp * scale;
            }
            else if (p.x >= dFar)
            {
                dir.y = MathX.Fabs(p.y) - dLeft;
                dir.z = MathX.Fabs(p.z) - dUp;
            }
            else
            {
                scale = p.x * invFar;
                dir.y = MathX.Fabs(p.y) - dLeft * scale;
                dir.z = MathX.Fabs(p.z) - dUp * scale;
            }
            if (dir.y > 0f) y = 1 + MathX.FLOATSIGNBITNOTSET_(p.y);
            if (dir.z > 0f) z = 1 + MathX.FLOATSIGNBITNOTSET_(p.z);
            if (p.x < dNear)
            {
                scale = dLeft * dNear * invFar;
                if (p.x < dNear + (scale - p.y) * scale * invFar)
                {
                    scale = dUp * dNear * invFar;
                    if (p.x < dNear + (scale - p.z) * scale * invFar) x = 1;
                }
            }
            else
            {
                if (p.x > dFar) x = 2;
                else if (p.x > dFar + (dLeft - p.y) * dLeft * invFar) x = 2;
                else if (p.x > dFar + (dUp - p.z) * dUp * invFar) x = 2;
            }

            var r = sphere.Radius;
            var index = VORONOI_INDEX(x, y, z);

            float d;
            switch (index)
            {
                case VORONOI_INDEX_000: return true;
                case VORONOI_INDEX_100: return dNear - p.x < r;
                case VORONOI_INDEX_200: return p.x - dFar < r;
                case VORONOI_INDEX_010: d = dFar * p.y - dLeft * p.x; return d * d < r * r * (dFar * dFar + dLeft * dLeft);
                case VORONOI_INDEX_020: d = -dFar * p.z - dLeft * p.x; return d * d < r * r * (dFar * dFar + dLeft * dLeft);
                case VORONOI_INDEX_001: d = dFar * p.z - dUp * p.x; return d * d < r * r * (dFar * dFar + dUp * dUp);
                case VORONOI_INDEX_002: d = -dFar * p.z - dUp * p.x; return (d * d < r * r * (dFar * dFar + dUp * dUp));
                default:
                    ToIndexPoints(out var points);
                    return index switch
                    {
                        VORONOI_INDEX_111 => sphere.ContainsPoint(points[0]),
                        VORONOI_INDEX_211 => sphere.ContainsPoint(points[4]),
                        VORONOI_INDEX_121 => sphere.ContainsPoint(points[1]),
                        VORONOI_INDEX_221 => sphere.ContainsPoint(points[5]),
                        VORONOI_INDEX_112 => sphere.ContainsPoint(points[2]),
                        VORONOI_INDEX_212 => sphere.ContainsPoint(points[6]),
                        VORONOI_INDEX_122 => sphere.ContainsPoint(points[3]),
                        VORONOI_INDEX_222 => sphere.ContainsPoint(points[7]),
                        VORONOI_INDEX_110 => sphere.LineIntersection(points[0], points[2]),
                        VORONOI_INDEX_210 => sphere.LineIntersection(points[4], points[6]),
                        VORONOI_INDEX_120 => sphere.LineIntersection(points[1], points[3]),
                        VORONOI_INDEX_220 => sphere.LineIntersection(points[5], points[7]),
                        VORONOI_INDEX_101 => sphere.LineIntersection(points[0], points[1]),
                        VORONOI_INDEX_201 => sphere.LineIntersection(points[4], points[5]),
                        VORONOI_INDEX_011 => sphere.LineIntersection(points[0], points[4]),
                        VORONOI_INDEX_021 => sphere.LineIntersection(points[1], points[5]),
                        VORONOI_INDEX_102 => sphere.LineIntersection(points[2], points[3]),
                        VORONOI_INDEX_202 => sphere.LineIntersection(points[6], points[7]),
                        VORONOI_INDEX_012 => sphere.LineIntersection(points[2], points[6]),
                        VORONOI_INDEX_022 => sphere.LineIntersection(points[3], points[7]),
                        _ => false,
                    };
            }
        }

        public bool IntersectsFrustum(in Frustum frustum)
        {
            Frustum localFrustum2 = new(frustum);
            localFrustum2.origin = (frustum.origin - origin) * axis.Transpose();
            localFrustum2.axis = frustum.axis * axis.Transpose();
            localFrustum2.ToIndexPointsAndCornerVecs(out var indexPoints2, out var cornerVecs2);

            if (CullLocalFrustum(localFrustum2, indexPoints2, cornerVecs2)) return false;

            Frustum localFrustum1 = new(this);
            localFrustum1.origin = (origin - frustum.origin) * frustum.axis.Transpose();
            localFrustum1.axis = axis * frustum.axis.Transpose();
            localFrustum1.ToIndexPointsAndCornerVecs(out var indexPoints1, out var cornerVecs1);

            if (frustum.CullLocalFrustum(localFrustum1, indexPoints1, cornerVecs1)) return false;

            Swap(ref indexPoints2[2], ref indexPoints2[3]);
            Swap(ref indexPoints2[6], ref indexPoints2[7]);

            if (LocalFrustumIntersectsFrustum(indexPoints2, localFrustum2.dNear > 0f)) return true;

            Swap(ref indexPoints1[2], ref indexPoints1[3]);
            Swap(ref indexPoints1[6], ref indexPoints1[7]);

            if (frustum.LocalFrustumIntersectsFrustum(indexPoints1, localFrustum1.dNear > 0f)) return true;

            return false;
        }

        public unsafe bool IntersectsWinding(in Winding winding)
        {
            int i, j;
            var localPoints = stackalloc Vector3[winding.NumPoints + Vector3.ALLOC16]; localPoints = (Vector3*)_alloca16(localPoints);
            var pointCull = stackalloc int[winding.NumPoints + intX.ALLOC16]; pointCull = (int*)_alloca16(pointCull);

            var transpose = axis.Transpose();
            for (i = 0; i < winding.NumPoints; i++) localPoints[i] = (winding[i].ToVec3() - origin) * transpose;

            // if the winding is culled
            if (CullLocalWinding(localPoints, winding.NumPoints, pointCull)) return false;

            winding.GetPlane(out var plane);

            ToIndexPointsAndCornerVecs(out var indexPoints, out var cornerVecs);
            AxisProjection(indexPoints, cornerVecs, plane.Normal, out var min, out var max);

            // if the frustum does not cross the winding plane
            if (min + plane[3] > 0f || max + plane[3] < 0f) return false;

            // test if any of the winding edges goes through the frustum
            for (i = 0; i < winding.NumPoints; i++)
            {
                j = (i + 1) % winding.NumPoints;
                if ((pointCull[i] & pointCull[j]) == 0) if (LocalLineIntersection(localPoints[i], localPoints[j])) return true;
            }

            Swap(ref indexPoints[2], ref indexPoints[3]);
            Swap(ref indexPoints[6], ref indexPoints[7]);

            // test if any edges of the frustum intersect the winding
            for (i = 0; i < 4; i++) if (winding.LineIntersection(plane, indexPoints[i], indexPoints[4 + i])) return true;
            if (dNear > 0f) for (i = 0; i < 4; i++) if (winding.LineIntersection(plane, indexPoints[i], indexPoints[(i + 1) & 3])) return true;
            for (i = 0; i < 4; i++) if (winding.LineIntersection(plane, indexPoints[4 + i], indexPoints[4 + ((i + 1) & 3)])) return true;

            return false;
        }

        // Returns true if the line intersects the box between the start and end point.
        public bool LineIntersection(in Vector3 start, in Vector3 end)
            => LocalLineIntersection((start - origin) * axis.Transpose(), (end - origin) * axis.Transpose());

        // Returns true if the ray intersects the bounds.
        // The ray can intersect the bounds in both directions from the start point.
        // If start is inside the frustum then scale1< 0 and scale2> 0.
        public bool RayIntersection(in Vector3 start, in Vector3 dir, out float scale1, out float scale2)
        {
            if (LocalRayIntersection((start - origin) * axis.Transpose(), dir * axis.Transpose(), out scale1, out scale2)) return true;
            if (scale1 <= scale2) return true;
            return false;
        }

        // Creates a frustum which contains the projection of the bounds.
        // returns true if the projection origin is far enough away from the bounding volume to create a valid frustum
        public bool FromProjection(in Bounds bounds, in Vector3 projectionOrigin, float dFar)
            => FromProjection(new Box(bounds, Vector3.origin, Matrix3x3.identity), projectionOrigin, dFar);
        // Creates a frustum which contains the projection of the box.
        public bool FromProjection(in Box box, in Vector3 projectionOrigin, float dFar)
        {
            int i; float value;

            Debug.Assert(dFar > 0f);

            this.dNear = this.dFar = this.invFar = 0f;

            var dir = box.Center - projectionOrigin;
            if (dir.Normalize() == 0f) return false;

            var bestAxis = 0;
            var bestValue = MathX.Fabs(box.Axis[0] * dir);
            for (i = 1; i < 3; i++)
            {
                value = MathX.Fabs(box.Axis[i] * dir);
                if (value * box.Extents[bestAxis] * box.Extents[bestAxis] < bestValue * box.Extents[i] * box.Extents[i]) { bestValue = value; bestAxis = i; }
            }

#if true
            int j, minX, minY, maxY, minZ, maxZ; Vector3[] points = null;
            minX = minY = maxY = minZ = maxZ = 0;

            for (j = 0; j < 2; j++)
            {
                axis[0] = dir;
                axis[1] = box.Axis[bestAxis] - (box.Axis[bestAxis] * axis[0]) * axis[0];
                axis[1].Normalize();
                axis[2].Cross(axis[0], axis[1]);

                BoxToPoints((box.Center - projectionOrigin) * axis.Transpose(), box.Extents, box.Axis * axis.Transpose(), out points);

                if (points[0].x <= 1f) return false;

                minX = minY = maxY = minZ = maxZ = 0;
                for (i = 1; i < 8; i++)
                {
                    if (points[i].x <= 1f) return false;
                    if (points[i].x < points[minX].x) minX = i;
                    if (points[minY].x * points[i].y < points[i].x * points[minY].y) minY = i;
                    else if (points[maxY].x * points[i].y > points[i].x * points[maxY].y) maxY = i;
                    if (points[minZ].x * points[i].z < points[i].x * points[minZ].z) minZ = i;
                    else if (points[maxZ].x * points[i].z > points[i].x * points[maxZ].z) maxZ = i;
                }

                if (j == 0)
                {
                    dir += MathX.Tan16(0.5f * (MathX.ATan16(points[minY].y, points[minY].x) + MathX.ATan16(points[maxY].y, points[maxY].x))) * axis[1];
                    dir += MathX.Tan16(0.5f * (MathX.ATan16(points[minZ].z, points[minZ].x) + MathX.ATan16(points[maxZ].z, points[maxZ].x))) * axis[2];
                    dir.Normalize();
                }
            }

            this.origin = projectionOrigin;
            this.dNear = points[minX].x;
            this.dFar = dFar;
            this.dLeft = Math.Max(MathX.Fabs(points[minY].y / points[minY].x), MathX.Fabs(points[maxY].y / points[maxY].x)) * dFar;
            this.dUp = Math.Max(MathX.Fabs(points[minZ].z / points[minZ].x), MathX.Fabs(points[maxZ].z / points[maxZ].x)) * dFar;
            this.invFar = 1f / dFar;
#elif true
            int j; float f, x; Bounds b = new(); Vector3[] points = null;

            for (j = 0; j < 2; j++)
            {
                axis[0] = dir;
                axis[1] = box.Axis[bestAxis] - (box.Axis[bestAxis] * axis[0]) * axis[0];
                axis[1].Normalize();
                axis[2].Cross(axis[0], axis[1]);

                BoxToPoints((box.Center - projectionOrigin) * axis.Transpose(), box.Extents, box.Axis * axis.Transpose(), out points);

                b.Clear();
                for (i = 0; i < 8; i++)
                {
                    x = points[i].x;
                    if (x <= 1f) return false;
                    f = 1f / x;
                    points[i].y *= f;
                    points[i].z *= f;
                    b.AddPoint(points[i]);
                }

                if (j == 0)
                {
                    dir += MathX.Tan16(0.5f * (MathX.ATan16(b[1][1]) + MathX.ATan16(b[0][1]))) * axis[1];
                    dir += MathX.Tan16(0.5f * (MathX.ATan16(b[1][2]) + MathX.ATan16(b[0][2]))) * axis[2];
                    dir.Normalize();
                }
            }

            this.origin = projectionOrigin;
            this.dNear = b[0][0];
            this.dFar = dFar;
            this.dLeft = Math.Max(MathX.Fabs(b[0][1]), MathX.Fabs(b[1][1])) * dFar;
            this.dUp = Math.Max(MathX.Fabs(b[0][2]), MathX.Fabs(b[1][2])) * dFar;
            this.invFar = 1f / dFar;
#else
            float[] dist = new float[3]; Vector3 org;

            axis[0] = dir;
            axis[1] = box.Axis[bestAxis] - (box.Axis[bestAxis] * axis[0]) * axis[0];
            axis[1].Normalize();
            axis[2].Cross(axis[0], axis[1]);

            for (i = 0; i < 3; i++)
                dist[i] =
                    MathX.Fabs(box.Extents[0] * (axis[i] * box.Axis[0])) +
                    MathX.Fabs(box.Extents[1] * (axis[i] * box.Axis[1])) +
                    MathX.Fabs(box.Extents[2] * (axis[i] * box.Axis[2]));

            dist[0] = axis[0] * (box.Center - projectionOrigin) - dist[0];
            if (dist[0] <= 1f) return false;
            float invDist = 1f / dist[0];

            this.origin = projectionOrigin;
            this.dNear = dist[0];
            this.dFar = dFar;
            this.dLeft = dist[1] * invDist * dFar;
            this.dUp = dist[2] * invDist * dFar;
            this.invFar = 1f / dFar;
#endif
            return true;
        }
        // Creates a frustum which contains the projection of the sphere.
        public bool FromProjection(in Sphere sphere, in Vector3 projectionOrigin, float dFar)
        {
            Debug.Assert(dFar > 0f);

            var dir = sphere.Origin - projectionOrigin;
            var d = dir.Normalize();
            var r = sphere.Radius;

            if (d <= r + 1f) { this.dNear = this.dFar = this.invFar = 0f; return false; }

            origin = projectionOrigin;
            axis = dir.ToMat3();

            var s = MathX.Sqrt(d * d - r * r);
            var x = r / d * s;
            var y = MathX.Sqrt(s * s - x * x);

            this.dNear = d - r;
            this.dFar = dFar;
            this.dLeft = x / y * dFar;
            this.dUp = dLeft;
            this.invFar = 1f / dFar;

            return true;
        }

        // Returns false if no part of the bounds extends beyond the near plane.
        // moves the far plane so it extends just beyond the bounding volume
        public bool ConstrainToBounds(in Bounds bounds)
        {
            bounds.AxisProjection(axis[0], out var min, out var max);
            var newdFar = max - axis[0] * origin;
            if (newdFar <= dNear) { MoveFarDistance(dNear + 1f); return false; }
            MoveFarDistance(newdFar);
            return true;
        }

        // Returns false if no part of the box extends beyond the near plane.
        public bool ConstrainToBox(in Box box)
        {
            box.AxisProjection(axis[0], out var min, out var max);
            var newdFar = max - axis[0] * origin;
            if (newdFar <= dNear) { MoveFarDistance(dNear + 1f); return false; }
            MoveFarDistance(newdFar);
            return true;
        }

        // Returns false if no part of the sphere extends beyond the near plane.
        public bool ConstrainToSphere(in Sphere sphere)
        {
            sphere.AxisProjection(axis[0], out var min, out var max);
            var newdFar = max - axis[0] * origin;
            if (newdFar <= dNear) { MoveFarDistance(dNear + 1f); return false; }
            MoveFarDistance(newdFar);
            return true;
        }

        // Returns false if no part of the frustum extends beyond the near plane.
        public bool ConstrainToFrustum(in Frustum frustum)
        {
            frustum.AxisProjection(axis[0], out var min, out var max);
            var newdFar = max - axis[0] * origin;
            if (newdFar <= dNear) { MoveFarDistance(dNear + 1f); return false; }
            MoveFarDistance(newdFar);
            return true;
        }

        // planes point outwards
        public void ToPlanes(out Plane[] planes)           // planes point outwards
        {
            planes = new Plane[6];
            planes[0].Normal = -axis[0];
            planes[0].Dist = -dNear;
            planes[1].Normal = axis[0];
            planes[1].Dist = dFar;

            var scaled = new Vector3[2];
            scaled[0] = axis[1] * dLeft;
            scaled[1] = axis[2] * dUp;
            var points = new Vector3[4];
            points[0] = scaled[0] + scaled[1];
            points[1] = -scaled[0] + scaled[1];
            points[2] = -scaled[0] - scaled[1];
            points[3] = scaled[0] - scaled[1];

            for (var i = 0; i < 4; i++)
            {
                planes[i + 2].Normal = points[i].Cross(points[(i + 1) & 3] - points[i]);
                planes[i + 2].Normalize();
                planes[i + 2].FitThroughPoint(points[i]);
            }
        }

        public void ToPoints(out Vector3[] points)             // 8 corners of the frustum
        {
            Matrix3x3 scaled = new();
            scaled[0] = origin + axis[0] * dNear;
            scaled[1] = axis[1] * (dLeft * dNear * invFar);
            scaled[2] = axis[2] * (dUp * dNear * invFar);

            points = new Vector3[8];
            points[0] = scaled[0] + scaled[1];
            points[1] = scaled[0] - scaled[1];
            points[2] = points[1] - scaled[2];
            points[3] = points[0] - scaled[2];
            points[0] += scaled[2];
            points[1] += scaled[2];

            scaled[0] = origin + axis[0] * dFar;
            scaled[1] = axis[1] * dLeft;
            scaled[2] = axis[2] * dUp;

            points[4] = scaled[0] + scaled[1];
            points[5] = scaled[0] - scaled[1];
            points[6] = points[5] - scaled[2];
            points[7] = points[4] - scaled[2];
            points[4] += scaled[2];
            points[5] += scaled[2];
        }

        void ToClippedPoints(float[] fractions, out Vector3[] points)
        {
            Matrix3x3 scaled = new();
            scaled[0] = origin + axis[0] * dNear;
            scaled[1] = axis[1] * (dLeft * dNear * invFar);
            scaled[2] = axis[2] * (dUp * dNear * invFar);

            points = new Vector3[8];
            points[0] = scaled[0] + scaled[1];
            points[1] = scaled[0] - scaled[1];
            points[2] = points[1] - scaled[2];
            points[3] = points[0] - scaled[2];
            points[0] += scaled[2];
            points[1] += scaled[2];

            scaled[0] = axis[0] * dFar;
            scaled[1] = axis[1] * dLeft;
            scaled[2] = axis[2] * dUp;

            points[4] = scaled[0] + scaled[1];
            points[5] = scaled[0] - scaled[1];
            points[6] = points[5] - scaled[2];
            points[7] = points[4] - scaled[2];
            points[4] += scaled[2];
            points[5] += scaled[2];

            points[4] = origin + fractions[0] * points[4];
            points[5] = origin + fractions[1] * points[5];
            points[6] = origin + fractions[2] * points[6];
            points[7] = origin + fractions[3] * points[7];
        }

        void ToIndexPoints(out Vector3[] indexPoints)
        {
            Matrix3x3 scaled = new();
            scaled[0] = origin + axis[0] * dNear;
            scaled[1] = axis[1] * (dLeft * dNear * invFar);
            scaled[2] = axis[2] * (dUp * dNear * invFar);

            indexPoints = new Vector3[8];
            indexPoints[0] = scaled[0] - scaled[1];
            indexPoints[2] = scaled[0] + scaled[1];
            indexPoints[1] = indexPoints[0] + scaled[2];
            indexPoints[3] = indexPoints[2] + scaled[2];
            indexPoints[0] -= scaled[2];
            indexPoints[2] -= scaled[2];

            scaled[0] = origin + axis[0] * dFar;
            scaled[1] = axis[1] * dLeft;
            scaled[2] = axis[2] * dUp;

            indexPoints[4] = scaled[0] - scaled[1];
            indexPoints[6] = scaled[0] + scaled[1];
            indexPoints[5] = indexPoints[4] + scaled[2];
            indexPoints[7] = indexPoints[6] + scaled[2];
            indexPoints[4] -= scaled[2];
            indexPoints[6] -= scaled[2];
        }

        // 22 muls
        void ToIndexPointsAndCornerVecs(out Vector3[] indexPoints, out Vector3[] cornerVecs)
        {
            Matrix3x3 scaled = new();
            scaled[0] = origin + axis[0] * dNear;
            scaled[1] = axis[1] * (dLeft * dNear * invFar);
            scaled[2] = axis[2] * (dUp * dNear * invFar);

            indexPoints = new Vector3[8];
            indexPoints[0] = scaled[0] - scaled[1];
            indexPoints[2] = scaled[0] + scaled[1];
            indexPoints[1] = indexPoints[0] + scaled[2];
            indexPoints[3] = indexPoints[2] + scaled[2];
            indexPoints[0] -= scaled[2];
            indexPoints[2] -= scaled[2];

            scaled[0] = axis[0] * dFar;
            scaled[1] = axis[1] * dLeft;
            scaled[2] = axis[2] * dUp;

            cornerVecs = new Vector3[4];
            cornerVecs[0] = scaled[0] - scaled[1];
            cornerVecs[2] = scaled[0] + scaled[1];
            cornerVecs[1] = cornerVecs[0] + scaled[2];
            cornerVecs[3] = cornerVecs[2] + scaled[2];
            cornerVecs[0] -= scaled[2];
            cornerVecs[2] -= scaled[2];

            indexPoints[4] = cornerVecs[0] + origin;
            indexPoints[5] = cornerVecs[1] + origin;
            indexPoints[6] = cornerVecs[2] + origin;
            indexPoints[7] = cornerVecs[3] + origin;
        }

        // 18 muls
        void AxisProjection(Vector3[] indexPoints, Vector3[] cornerVecs, in Vector3 dir, out float min, out float max)
        {
            float dx, dy, dz;
            int index;

            dy = dir.x * axis[1].x + dir.y * axis[1].y + dir.z * axis[1].z;
            dz = dir.x * axis[2].x + dir.y * axis[2].y + dir.z * axis[2].z;
            index = (MathX.FLOATSIGNBITSET_(dy) << 1) | MathX.FLOATSIGNBITSET_(dz);
            dx = dir.x * cornerVecs[index].x + dir.y * cornerVecs[index].y + dir.z * cornerVecs[index].z;
            index |= (MathX.FLOATSIGNBITSET_(dx) << 2);
            min = indexPoints[index] * dir;
            index = ~index & 3;
            dx = -dir.x * cornerVecs[index].x - dir.y * cornerVecs[index].y - dir.z * cornerVecs[index].z;
            index |= (MathX.FLOATSIGNBITSET_(dx) << 2);
            max = indexPoints[index] * dir;
        }

        // calculates the projection of this frustum onto the given axis
        // 40 muls
        public void AxisProjection(in Vector3 dir, out float min, out float max)
        {
            ToIndexPointsAndCornerVecs(out var indexPoints, out var cornerVecs);
            AxisProjection(indexPoints, cornerVecs, dir, out min, out max);
        }

        // 76 muls
        public void AxisProjection(in Matrix3x3 ax, in Bounds bounds)
        {
            ToIndexPointsAndCornerVecs(out var indexPoints, out var cornerVecs);
            AxisProjection(indexPoints, cornerVecs, ax[0], out bounds[0].x, out bounds[1].x);
            AxisProjection(indexPoints, cornerVecs, ax[1], out bounds[0].y, out bounds[1].y);
            AxisProjection(indexPoints, cornerVecs, ax[2], out bounds[0].z, out bounds[1].z);
        }

        void AddLocalLineToProjectionBoundsSetCull(in Vector3 start, in Vector3 end, out int startCull, out int endCull, in Bounds bounds)
        {
            Vector3 p; float d1, d2, fstart, fend, lstart, lend, f; int cull1, cull2;

#if FRUSTUM_DEBUG
            if (r_showInteractionScissors_0() > 1) session.rw.DebugLine(colorGreen, origin + start * axis, origin + end * axis);
#endif
            var leftScale = dLeft * invFar;
            var upScale = dUp * invFar;
            var dir = end - start;

            fstart = dFar * start.y;
            fend = dFar * end.y;
            lstart = dLeft * start.x;
            lend = dLeft * end.x;

            // test left plane
            d1 = -fstart + lstart;
            d2 = -fend + lend;
            cull1 = MathX.FLOATSIGNBITSET_(d1);
            cull2 = MathX.FLOATSIGNBITSET_(d2);
            if (MathX.FLOATNOTZERO(d1) && MathX.FLOATSIGNBITSET(d1) ^ MathX.FLOATSIGNBITSET(d2)) //: opt
            {
                f = d1 / (d1 - d2);
                p.x = start.x + f * dir.x;
                if (p.x > 0f)
                {
                    p.z = start.z + f * dir.z;
                    if (MathX.Fabs(p.z) <= p.x * upScale) { p.y = 1f; p.z = p.z * dFar / (p.x * dUp); bounds.AddPoint(p); }
                }
            }

            // test right plane
            d1 = fstart + lstart;
            d2 = fend + lend;
            cull1 |= MathX.FLOATSIGNBITSET_(d1) << 1;
            cull2 |= MathX.FLOATSIGNBITSET_(d2) << 1;
            if (MathX.FLOATNOTZERO(d1) && MathX.FLOATSIGNBITSET(d1) ^ MathX.FLOATSIGNBITSET(d2)) //: opt
            {
                f = d1 / (d1 - d2);
                p.x = start.x + f * dir.x;
                if (p.x > 0f)
                {
                    p.z = start.z + f * dir.z;
                    if (MathX.Fabs(p.z) <= p.x * upScale) { p.y = -1f; p.z = p.z * dFar / (p.x * dUp); bounds.AddPoint(p); }
                }
            }

            fstart = dFar * start.z;
            fend = dFar * end.z;
            lstart = dUp * start.x;
            lend = dUp * end.x;

            // test up plane
            d1 = -fstart + lstart;
            d2 = -fend + lend;
            cull1 |= MathX.FLOATSIGNBITSET_(d1) << 2;
            cull2 |= MathX.FLOATSIGNBITSET_(d2) << 2;
            if (MathX.FLOATNOTZERO(d1) && MathX.FLOATSIGNBITSET(d1) ^ MathX.FLOATSIGNBITSET(d2)) //: opt
            {
                f = d1 / (d1 - d2);
                p.x = start.x + f * dir.x;
                if (p.x > 0f)
                {
                    p.y = start.y + f * dir.y;
                    if (MathX.Fabs(p.y) <= p.x * leftScale) { p.y = p.y * dFar / (p.x * dLeft); p.z = 1f; bounds.AddPoint(p); }
                }
            }

            // test down plane
            d1 = fstart + lstart;
            d2 = fend + lend;
            cull1 |= MathX.FLOATSIGNBITSET_(d1) << 3;
            cull2 |= MathX.FLOATSIGNBITSET_(d2) << 3;
            if (MathX.FLOATNOTZERO(d1) && MathX.FLOATSIGNBITSET(d1) ^ MathX.FLOATSIGNBITSET(d2)) //: opt
            {
                f = d1 / (d1 - d2);
                p.x = start.x + f * dir.x;
                if (p.x > 0f)
                {
                    p.y = start.y + f * dir.y;
                    if (MathX.Fabs(p.y) <= p.x * leftScale) { p.y = p.y * dFar / (p.x * dLeft); p.z = -1f; bounds.AddPoint(p); }
                }
            }

            // add start point to projection bounds
            if (cull1 == 0 && start.x > 0f) { p.x = start.x; p.y = start.y * dFar / (start.x * dLeft); p.z = start.z * dFar / (start.x * dUp); bounds.AddPoint(p); }

            // add end point to projection bounds
            if (cull2 == 0 && end.x > 0f) { p.x = end.x; p.y = end.y * dFar / (end.x * dLeft); p.z = end.z * dFar / (end.x * dUp); bounds.AddPoint(p); }

            if (start.x < bounds[0].x) bounds[0].x = start.x < 0f ? 0f : start.x;
            if (end.x < bounds[0].x) bounds[0].x = end.x < 0f ? 0f : end.x;

            startCull = cull1;
            endCull = cull2;
        }

        void AddLocalLineToProjectionBoundsUseCull(in Vector3 start, in Vector3 end, int startCull, int endCull, in Bounds bounds)
        {
            Vector3 p; float d1, d2, fstart, fend, lstart, lend, f;

            var clip = startCull ^ endCull;
            if (clip == 0) return;

#if  FRUSTUM_DEBUG
            if (r_showInteractionScissors_1() > 1) session.rw.DebugLine(colorGreen, origin + start * axis, origin + end * axis);
#endif

            var leftScale = dLeft * invFar;
            var upScale = dUp * invFar;
            var dir = end - start;

            if ((clip & (1 | 2)) != 0)
            {
                fstart = dFar * start.y;
                fend = dFar * end.y;
                lstart = dLeft * start.x;
                lend = dLeft * end.x;

                if ((clip & 1) != 0)
                {
                    // test left plane
                    d1 = -fstart + lstart;
                    d2 = -fend + lend;
                    if (MathX.FLOATNOTZERO(d1) && MathX.FLOATSIGNBITSET(d1) ^ MathX.FLOATSIGNBITSET(d2)) //: opt
                    {
                        f = d1 / (d1 - d2);
                        p.x = start.x + f * dir.x;
                        if (p.x > 0f)
                        {
                            p.z = start.z + f * dir.z;
                            if (MathX.Fabs(p.z) <= p.x * upScale) { p.y = 1f; p.z = p.z * dFar / (p.x * dUp); bounds.AddPoint(p); }
                        }
                    }
                }

                if ((clip & 2) != 0)
                {
                    // test right plane
                    d1 = fstart + lstart;
                    d2 = fend + lend;
                    if (MathX.FLOATNOTZERO(d1) && MathX.FLOATSIGNBITSET(d1) ^ MathX.FLOATSIGNBITSET(d2)) //: opt
                    {
                        f = d1 / (d1 - d2);
                        p.x = start.x + f * dir.x;
                        if (p.x > 0f)
                        {
                            p.z = start.z + f * dir.z;
                            if (MathX.Fabs(p.z) <= p.x * upScale) { p.y = -1f; p.z = p.z * dFar / (p.x * dUp); bounds.AddPoint(p); }
                        }
                    }
                }
            }

            if ((clip & (4 | 8)) != 0)
            {
                fstart = dFar * start.z;
                fend = dFar * end.z;
                lstart = dUp * start.x;
                lend = dUp * end.x;

                if ((clip & 4) != 0)
                {
                    // test up plane
                    d1 = -fstart + lstart;
                    d2 = -fend + lend;
                    if (MathX.FLOATNOTZERO(d1) && MathX.FLOATSIGNBITSET(d1) ^ MathX.FLOATSIGNBITSET(d2)) //: opt
                    {
                        f = d1 / (d1 - d2);
                        p.x = start.x + f * dir.x;
                        if (p.x > 0f)
                        {
                            p.y = start.y + f * dir.y;
                            if (MathX.Fabs(p.y) <= p.x * leftScale) { p.y = p.y * dFar / (p.x * dLeft); p.z = 1f; bounds.AddPoint(p); }
                        }
                    }
                }

                if ((clip & 8) != 0)
                {
                    // test down plane
                    d1 = fstart + lstart;
                    d2 = fend + lend;
                    if (MathX.FLOATNOTZERO(d1) && MathX.FLOATSIGNBITSET(d1) ^ MathX.FLOATSIGNBITSET(d2)) //: opt
                    {
                        f = d1 / (d1 - d2);
                        p.x = start.x + f * dir.x;
                        if (p.x > 0f)
                        {
                            p.y = start.y + f * dir.y;
                            if (MathX.Fabs(p.y) <= p.x * leftScale) { p.y = p.y * dFar / (p.x * dLeft); p.z = -1f; bounds.AddPoint(p); }
                        }
                    }
                }
            }
        }

        // Clips the frustum far extents to the box.
        void ClipFrustumToBox(in Box box, float[] clipFractions, int[] clipPlanes)
        {
            var transpose = box.Axis;
            transpose.TransposeSelf();
            var localOrigin = (origin - box.Center) * transpose;
            var localAxis = axis * transpose;

            Matrix3x3 scaled = new();
            scaled[0] = localAxis[0] * dFar;
            scaled[1] = localAxis[1] * dLeft;
            scaled[2] = localAxis[2] * dUp;
            var cornerVecs = new Vector3[4];
            cornerVecs[0] = scaled[0] + scaled[1];
            cornerVecs[1] = scaled[0] - scaled[1];
            cornerVecs[2] = cornerVecs[1] - scaled[2];
            cornerVecs[3] = cornerVecs[0] - scaled[2];
            cornerVecs[0] += scaled[2];
            cornerVecs[1] += scaled[2];

            Bounds bounds = new();
            bounds[0] = -box.Extents;
            bounds[1] = box.Extents;

            var minf = (dNear + 1f) * invFar;

            int index; float f;
            for (var i = 0; i < 4; i++)
            {
                index = MathX.FLOATSIGNBITNOTSET_(cornerVecs[i].x);
                f = (bounds[index].x - localOrigin.x) / cornerVecs[i].x;
                clipFractions[i] = f;
                clipPlanes[i] = 1 << index;

                index = MathX.FLOATSIGNBITNOTSET_(cornerVecs[i].y);
                f = (bounds[index].y - localOrigin.y) / cornerVecs[i].y;
                if (f < clipFractions[i]) { clipFractions[i] = f; clipPlanes[i] = 4 << index; }

                index = MathX.FLOATSIGNBITNOTSET_(cornerVecs[i].z);
                f = (bounds[index].z - localOrigin.z) / cornerVecs[i].z;
                if (f < clipFractions[i]) { clipFractions[i] = f; clipPlanes[i] = 16 << index; }

                // make sure the frustum is not clipped between the frustum origin and the near plane
                if (clipFractions[i] < minf) clipFractions[i] = minf;
            }
        }

        // Returns true if part of the line is inside the frustum.
        // Does not clip to the near and far plane.
        bool ClipLine(Vector3[] localPoints, Vector3[] points, int startIndex, int endIndex, out Vector3 start, out Vector3 end, out int startClip, out int endClip)
        {
            float f, x;

            var leftScale = dLeft * invFar;
            var upScale = dUp * invFar;

            var localStart = localPoints[startIndex];
            var localEnd = localPoints[endIndex];
            var localDir = localEnd - localStart;

            startClip = endClip = -1;
            var scale1 = MathX.INFINITY;
            var scale2 = -MathX.INFINITY;

            var fstart = dFar * localStart.y;
            var fend = dFar * localEnd.y;
            var lstart = dLeft * localStart.x;
            var lend = dLeft * localEnd.x;

            // test left plane
            var d1 = -fstart + lstart;
            var d2 = -fend + lend;
            var startCull = MathX.FLOATSIGNBITSET_(d1);
            var endCull = MathX.FLOATSIGNBITSET_(d2);
            if (MathX.FLOATNOTZERO(d1) &&
                MathX.FLOATSIGNBITSET(d1) ^ MathX.FLOATSIGNBITSET(d2)) //: opt
            {
                f = d1 / (d1 - d2);
                x = localStart.x + f * localDir.x;
                if (x >= 0f && MathX.Fabs(localStart.z + f * localDir.z) <= x * upScale) //: opt
                {
                    if (f < scale1) { scale1 = f; startClip = 0; }
                    if (f > scale2) { scale2 = f; endClip = 0; }
                }
            }

            // test right plane
            d1 = fstart + lstart;
            d2 = fend + lend;
            startCull |= MathX.FLOATSIGNBITSET_(d1) << 1;
            endCull |= MathX.FLOATSIGNBITSET_(d2) << 1;
            if (MathX.FLOATNOTZERO(d1) && MathX.FLOATSIGNBITSET(d1) ^ MathX.FLOATSIGNBITSET(d2)) //: opt
            {
                f = d1 / (d1 - d2);
                x = localStart.x + f * localDir.x;
                if (x >= 0f && MathX.Fabs(localStart.z + f * localDir.z) <= x * upScale) //: opt
                {
                    if (f < scale1) { scale1 = f; startClip = 1; }
                    if (f > scale2) { scale2 = f; endClip = 1; }
                }
            }

            fstart = dFar * localStart.z;
            fend = dFar * localEnd.z;
            lstart = dUp * localStart.x;
            lend = dUp * localEnd.x;

            // test up plane
            d1 = -fstart + lstart;
            d2 = -fend + lend;
            startCull |= MathX.FLOATSIGNBITSET_(d1) << 2;
            endCull |= MathX.FLOATSIGNBITSET_(d2) << 2;
            if (MathX.FLOATNOTZERO(d1) && MathX.FLOATSIGNBITSET(d1) ^ MathX.FLOATSIGNBITSET(d2)) //: opt
            {
                f = d1 / (d1 - d2);
                x = localStart.x + f * localDir.x;
                if (x >= 0f && MathX.Fabs(localStart.y + f * localDir.y) <= x * leftScale) //: opt
                {
                    if (f < scale1) { scale1 = f; startClip = 2; }
                    if (f > scale2) { scale2 = f; endClip = 2; }
                }
            }

            // test down plane
            d1 = fstart + lstart;
            d2 = fend + lend;
            startCull |= MathX.FLOATSIGNBITSET_(d1) << 3;
            endCull |= MathX.FLOATSIGNBITSET_(d2) << 3;
            if (MathX.FLOATNOTZERO(d1) && MathX.FLOATSIGNBITSET(d1) ^ MathX.FLOATSIGNBITSET(d2)) //: opt
            {
                f = d1 / (d1 - d2);
                x = localStart.x + f * localDir.x;
                if (x >= 0f && MathX.Fabs(localStart.y + f * localDir.y) <= x * leftScale) //: opt
                {
                    if (f < scale1) { scale1 = f; startClip = 3; }
                    if (f > scale2) { scale2 = f; endClip = 3; }
                }
            }

            // if completely inside
            if ((startCull | endCull) == 0) { start = points[startIndex]; end = points[endIndex]; return true; }
            else if (scale1 <= scale2)
            {
                if (startCull == 0) { start = points[startIndex]; startClip = -1; }
                else start = points[startIndex] + scale1 * (points[endIndex] - points[startIndex]);
                if (endCull == 0) { end = points[endIndex]; endClip = -1; }
                else end = points[startIndex] + scale2 * (points[endIndex] - points[startIndex]);
                return true;
            }
            start = default;
            end = default;
            return false;
        }

        static readonly int[][] CapPointIndex = new int[][] {
            new[]{ 0, 3 },
            new[]{ 1, 2 },
            new[]{ 0, 1 },
            new[]{ 2, 3 },
        };

        bool AddLocalCapsToProjectionBounds(Span<Vector3> endPoints, Span<int> endPointCull, in Vector3 point, int pointCull, int pointClip, in Bounds projectionBounds)
        {
            if (pointClip < 0) return false;
            var p = CapPointIndex[pointClip];
            AddLocalLineToProjectionBoundsUseCull(endPoints[p[0]], point, endPointCull[p[0]], pointCull, projectionBounds);
            AddLocalLineToProjectionBoundsUseCull(endPoints[p[1]], point, endPointCull[p[1]], pointCull, projectionBounds);
            return true;
        }

        //  Returns true if the ray starts inside the bounds.
        // If there was an intersection scale1 <= scale2
        bool BoundsRayIntersection(in Bounds bounds, in Vector3 start, in Vector3 dir, out float scale1, out float scale2)
        {
            Vector3 p = new(); float d1, d2, f;

            scale1 = MathX.INFINITY;
            scale2 = -MathX.INFINITY;

            var end = start + dir;

            var startInside = 1;
            for (var i = 0; i < 2; i++)
            {
                d1 = start.x - bounds[i].x;
                startInside &= MathX.FLOATSIGNBITSET_(d1) ^ i;
                d2 = end.x - bounds[i].x;
                if (d1 != d2)
                {
                    f = d1 / (d1 - d2);
                    p.y = start.y + f * dir.y;
                    if (bounds[0].y <= p.y && p.y <= bounds[1].y)
                    {
                        p.z = start.z + f * dir.z;
                        if (bounds[0].z <= p.z && p.z <= bounds[1].z)
                        {
                            if (f < scale1) scale1 = f;
                            if (f > scale2) scale2 = f;
                        }
                    }
                }

                d1 = start.y - bounds[i].y;
                startInside &= MathX.FLOATSIGNBITSET_(d1) ^ i;
                d2 = end.y - bounds[i].y;
                if (d1 != d2)
                {
                    f = d1 / (d1 - d2);
                    p.x = start.x + f * dir.x;
                    if (bounds[0].x <= p.x && p.x <= bounds[1].x)
                    {
                        p.z = start.z + f * dir.z;
                        if (bounds[0].z <= p.z && p.z <= bounds[1].z)
                        {
                            if (f < scale1) scale1 = f;
                            if (f > scale2) scale2 = f;
                        }
                    }
                }

                d1 = start.z - bounds[i].z;
                startInside &= MathX.FLOATSIGNBITSET_(d1) ^ i;
                d2 = end.z - bounds[i].z;
                if (d1 != d2)
                {
                    f = d1 / (d1 - d2);
                    p.x = start.x + f * dir.x;
                    if (bounds[0].x <= p.x && p.x <= bounds[1].x)
                    {
                        p.y = start.y + f * dir.y;
                        if (bounds[0].y <= p.y && p.y <= bounds[1].y)
                        {
                            if (f < scale1) scale1 = f;
                            if (f > scale2) scale2 = f;
                        }
                    }
                }
            }

            return startInside != 0;
        }

        // calculates the bounds for the projection in this frustum
        public bool ProjectionBounds(in Bounds bounds, in Bounds projectionBounds)
            => ProjectionBounds(new Box(bounds, Vector3.origin, Matrix3x3.identity), projectionBounds);

        public bool ProjectionBounds(in Box box, in Bounds projectionBounds)
        {
            var bounds = new Bounds(-box.Extents, box.Extents);

            // if the frustum origin is inside the bounds
            if (bounds.ContainsPoint((origin - box.Center) * box.Axis.Transpose()))
            {
                // bounds that cover the whole frustum
                var base_ = origin * axis[0];
                box.AxisProjection(axis[0], out var boxMin, out var boxMax);

                projectionBounds[0].x = boxMin - base_;
                projectionBounds[1].x = boxMax - base_;
                projectionBounds[0].y = projectionBounds[0].z = -1f;
                projectionBounds[1].y = projectionBounds[1].z = 1f;

                return true;
            }

            projectionBounds.Clear();

            // transform the bounds into the space of this frustum
            var localOrigin = (box.Center - origin) * axis.Transpose();
            var localAxis = box.Axis * axis.Transpose();
            BoxToPoints(localOrigin, box.Extents, localAxis, out var points);

            // test outer four edges of the bounds
            int culled = -1, outside = 0; int[] pointCull = new int[8];
            int i, p1, p2;
            for (i = 0; i < 4; i++)
            {
                p1 = i;
                p2 = 4 + i;
                AddLocalLineToProjectionBoundsSetCull(points[p1], points[p2], out pointCull[p1], out pointCull[p2], projectionBounds);
                culled &= pointCull[p1] & pointCull[p2];
                outside |= pointCull[p1] | pointCull[p2];
            }

            // if the bounds are completely outside this frustum
            if (culled != 0) return false;

            // if the bounds are completely inside this frustum
            if (outside == 0) return true;

            // test the remaining edges of the bounds
            for (i = 0; i < 4; i++)
            {
                p1 = i;
                p2 = (i + 1) & 3;
                AddLocalLineToProjectionBoundsUseCull(points[p1], points[p2], pointCull[p1], pointCull[p2], projectionBounds);
            }

            for (i = 0; i < 4; i++)
            {
                p1 = 4 + i;
                p2 = 4 + ((i + 1) & 3);
                AddLocalLineToProjectionBoundsUseCull(points[p1], points[p2], pointCull[p1], pointCull[p2], projectionBounds);
            }

            // if the bounds extend beyond two or more boundaries of this frustum
            float scale1, scale2;
            if (outside != 1 && outside != 2 && outside != 4 && outside != 8)
            {
                localOrigin = (origin - box.Center) * box.Axis.Transpose();
                var localScaled = axis * box.Axis.Transpose();
                localScaled[0] *= dFar;
                localScaled[1] *= dLeft;
                localScaled[2] *= dUp;

                // test the outer edges of this frustum for intersection with the bounds
                if ((outside & 2) != 0 && (outside & 8) != 0)
                {
                    BoundsRayIntersection(bounds, localOrigin, localScaled[0] - localScaled[1] - localScaled[2], out scale1, out scale2);
                    if (scale1 <= scale2 && scale1 >= 0f)
                    {
                        projectionBounds.AddPoint(new Vector3(scale1 * dFar, -1f, -1f));
                        projectionBounds.AddPoint(new Vector3(scale2 * dFar, -1f, -1f));
                    }
                }
                if ((outside & 2) != 0 && (outside & 4) != 0)
                {
                    BoundsRayIntersection(bounds, localOrigin, localScaled[0] - localScaled[1] + localScaled[2], out scale1, out scale2);
                    if (scale1 <= scale2 && scale1 >= 0f)
                    {
                        projectionBounds.AddPoint(new Vector3(scale1 * dFar, -1f, 1f));
                        projectionBounds.AddPoint(new Vector3(scale2 * dFar, -1f, 1f));
                    }
                }
                if ((outside & 1) != 0 && (outside & 8) != 0)
                {
                    BoundsRayIntersection(bounds, localOrigin, localScaled[0] + localScaled[1] - localScaled[2], out scale1, out scale2);
                    if (scale1 <= scale2 && scale1 >= 0f)
                    {
                        projectionBounds.AddPoint(new Vector3(scale1 * dFar, 1f, -1f));
                        projectionBounds.AddPoint(new Vector3(scale2 * dFar, 1f, -1f));
                    }
                }
                if ((outside & 1) != 0 && (outside & 2) != 0)
                {
                    BoundsRayIntersection(bounds, localOrigin, localScaled[0] + localScaled[1] + localScaled[2], out scale1, out scale2);
                    if (scale1 <= scale2 && scale1 >= 0f)
                    {
                        projectionBounds.AddPoint(new Vector3(scale1 * dFar, 1f, 1f));
                        projectionBounds.AddPoint(new Vector3(scale2 * dFar, 1f, 1f));
                    }
                }
            }

            return true;
        }

        public bool ProjectionBounds(in Sphere sphere, in Bounds projectionBounds)
        {
            projectionBounds.Clear();

            var center = (sphere.Origin - origin) * axis.Transpose();
            var r = sphere.Radius;
            var rs = r * r;
            var sFar = dFar * dFar;

            // test left/right planes
            var d = dFar * MathX.Fabs(center.y) - dLeft * center.x;
            if ((d * d) > rs * (sFar + dLeft * dLeft)) return false;

            // test up/down planes
            d = dFar * MathX.Fabs(center.z) - dUp * center.x;
            if ((d * d) > rs * (sFar + dUp * dUp)) return false;

            // bounds that cover the whole frustum
            projectionBounds[0].x = 0f;
            projectionBounds[1].x = dFar;
            projectionBounds[0].y = projectionBounds[0].z = -1f;
            projectionBounds[1].y = projectionBounds[1].z = 1f;
            return true;
        }

        public bool ProjectionBounds(in Frustum frustum, in Bounds projectionBounds)
        {
            // if the frustum origin is inside the other frustum
            if (frustum.ContainsPoint(origin))
            {
                // bounds that cover the whole frustum
                var base_ = origin * axis[0];
                frustum.AxisProjection(axis[0], out var frustumMin, out var frustumMax);

                projectionBounds[0].x = frustumMin - base_;
                projectionBounds[1].x = frustumMax - base_;
                projectionBounds[0].y = projectionBounds[0].z = -1f;
                projectionBounds[1].y = projectionBounds[1].z = 1f;
                return true;
            }

            projectionBounds.Clear();

            // transform the given frustum into the space of this frustum
            Frustum localFrustum = new(frustum);
            localFrustum.origin = (frustum.origin - origin) * axis.Transpose();
            localFrustum.axis = frustum.axis * axis.Transpose();
            localFrustum.ToPoints(out var points);

            // test outer four edges of the other frustum
            int culled = -1, outside = 0; int[] pointCull = new int[8];
            int i, p1, p2;

            for (i = 0; i < 4; i++)
            {
                p1 = i;
                p2 = 4 + i;
                AddLocalLineToProjectionBoundsSetCull(points[p1], points[p2], out pointCull[p1], out pointCull[p2], projectionBounds);
                culled &= pointCull[p1] & pointCull[p2];
                outside |= pointCull[p1] | pointCull[p2];
            }

            // if the other frustum is completely outside this frustum
            if (culled != 0) return false;

            // if the other frustum is completely inside this frustum
            if (outside == 0) return true;

            // test the remaining edges of the other frustum
            if (localFrustum.dNear > 0f)
                for (i = 0; i < 4; i++)
                {
                    p1 = i;
                    p2 = (i + 1) & 3;
                    AddLocalLineToProjectionBoundsUseCull(points[p1], points[p2], pointCull[p1], pointCull[p2], projectionBounds);
                }

            for (i = 0; i < 4; i++)
            {
                p1 = 4 + i;
                p2 = 4 + ((i + 1) & 3);
                AddLocalLineToProjectionBoundsUseCull(points[p1], points[p2], pointCull[p1], pointCull[p2], projectionBounds);
            }

            // if the other frustum extends beyond two or more boundaries of this frustum
            float scale1, scale2;
            if (outside != 1 && outside != 2 && outside != 4 && outside != 8)
            {
                var localOrigin = (origin - frustum.origin) * frustum.axis.Transpose();
                var localScaled = axis * frustum.axis.Transpose();
                localScaled[0] *= dFar;
                localScaled[1] *= dLeft;
                localScaled[2] *= dUp;

                // test the outer edges of this frustum for intersection with the other frustum
                if ((outside & 2) != 0 && (outside & 8) != 0)
                {
                    frustum.LocalRayIntersection(localOrigin, localScaled[0] - localScaled[1] - localScaled[2], out scale1, out scale2);
                    if (scale1 <= scale2 && scale1 >= 0f)
                    {
                        projectionBounds.AddPoint(new Vector3(scale1 * dFar, -1f, -1f));
                        projectionBounds.AddPoint(new Vector3(scale2 * dFar, -1f, -1f));
                    }
                }
                if ((outside & 2) != 0 && (outside & 4) != 0)
                {
                    frustum.LocalRayIntersection(localOrigin, localScaled[0] - localScaled[1] + localScaled[2], out scale1, out scale2);
                    if (scale1 <= scale2 && scale1 >= 0f)
                    {
                        projectionBounds.AddPoint(new Vector3(scale1 * dFar, -1f, 1f));
                        projectionBounds.AddPoint(new Vector3(scale2 * dFar, -1f, 1f));
                    }
                }
                if ((outside & 1) != 0 && (outside & 8) != 0)
                {
                    frustum.LocalRayIntersection(localOrigin, localScaled[0] + localScaled[1] - localScaled[2], out scale1, out scale2);
                    if (scale1 <= scale2 && scale1 >= 0f)
                    {
                        projectionBounds.AddPoint(new Vector3(scale1 * dFar, 1f, -1f));
                        projectionBounds.AddPoint(new Vector3(scale2 * dFar, 1f, -1f));
                    }
                }
                if ((outside & 1) != 0 && (outside & 2) != 0)
                {
                    frustum.LocalRayIntersection(localOrigin, localScaled[0] + localScaled[1] + localScaled[2], out scale1, out scale2);
                    if (scale1 <= scale2 && scale1 >= 0f)
                    {
                        projectionBounds.AddPoint(new Vector3(scale1 * dFar, 1f, 1f));
                        projectionBounds.AddPoint(new Vector3(scale2 * dFar, 1f, 1f));
                    }
                }
            }

            return true;
        }

        public unsafe bool ProjectionBounds(in Winding winding, in Bounds projectionBounds)
        {
            int i, p1, p2;

            projectionBounds.Clear();

            // transform the winding points into the space of this frustum
            var localPoints = stackalloc Vector3[winding.NumPoints + Vector3.ALLOC16]; localPoints = (Vector3*)_alloca16(localPoints);
            var transpose = axis.Transpose();
            for (i = 0; i < winding.NumPoints; i++) localPoints[i] = (winding[i].ToVec3() - origin) * transpose;

            // test the winding edges
            int culled = -1, outside = 0;
            var pointCull = stackalloc int[winding.NumPoints + intX.ALLOC16]; pointCull = (int*)_alloca16(pointCull);
            for (i = 0; i < winding.NumPoints; i += 2)
            {
                p1 = i;
                p2 = (i + 1) % winding.NumPoints;
                AddLocalLineToProjectionBoundsSetCull(localPoints[p1], localPoints[p2], out pointCull[p1], out pointCull[p2], projectionBounds);
                culled &= pointCull[p1] & pointCull[p2];
                outside |= pointCull[p1] | pointCull[p2];
            }

            // if completely culled
            if (culled != 0) return false;

            // if completely inside
            if (outside == 0) return true;

            // test remaining winding edges
            for (i = 1; i < winding.NumPoints; i += 2)
            {
                p1 = i;
                p2 = (i + 1) % winding.NumPoints;
                AddLocalLineToProjectionBoundsUseCull(localPoints[p1], localPoints[p2], pointCull[p1], pointCull[p2], projectionBounds);
            }

            // if the winding extends beyond two or more boundaries of this frustum
            float scale;
            if (outside != 1 && outside != 2 && outside != 4 && outside != 8)
            {
                winding.GetPlane(out var plane);
                Matrix3x3 scaled = new();
                scaled[0] = axis[0] * dFar;
                scaled[1] = axis[1] * dLeft;
                scaled[2] = axis[2] * dUp;

                // test the outer edges of this frustum for intersection with the winding
                if ((outside & 2) != 0 && (outside & 8) != 0 && winding.RayIntersection(plane, origin, scaled[0] - scaled[1] - scaled[2], out scale)) projectionBounds.AddPoint(new Vector3(scale * dFar, -1f, -1f));
                if ((outside & 2) != 0 && (outside & 4) != 0 && winding.RayIntersection(plane, origin, scaled[0] - scaled[1] + scaled[2], out scale)) projectionBounds.AddPoint(new Vector3(scale * dFar, -1f, 1f));
                if ((outside & 1) != 0 && (outside & 8) != 0 && winding.RayIntersection(plane, origin, scaled[0] + scaled[1] - scaled[2], out scale)) projectionBounds.AddPoint(new Vector3(scale * dFar, 1f, -1f));
                if ((outside & 1) != 0 && (outside & 2) != 0 && winding.RayIntersection(plane, origin, scaled[0] + scaled[1] + scaled[2], out scale)) projectionBounds.AddPoint(new Vector3(scale * dFar, 1f, 1f));
            }

            return true;
        }

        // calculates the bounds for the projection in this frustum of the given frustum clipped to the given box
        public bool ClippedProjectionBounds(in Frustum frustum, in Box clipBox, in Bounds projectionBounds)
        {
            int i, p1, p2; int[] clipPointCull = new int[8], clipPlanes = new int[4]; int usedClipPlanes, nearCull, farCull, outside;
            int[] pointCull = new int[2], boxPointCull = new int[8]; int startClip, endClip;
            float[] clipFractions = new float[4]; float s1, s2, t1, t2, leftScale, upScale;
            Frustum localFrustum;
            Vector3[] clipPoints, localPoints1 = new Vector3[8], localPoints2 = new Vector3[8]; Vector3 localOrigin1, localOrigin2, start, end;
            Matrix3x3 localAxis1, localAxis2, transpose;
            Bounds clipBounds = new();

            // if the frustum origin is inside the other frustum
            if (frustum.ContainsPoint(origin))
            {
                // bounds that cover the whole frustum
                var base_ = origin * axis[0];
                clipBox.AxisProjection(axis[0], out var clipBoxMin, out var clipBoxMax);
                frustum.AxisProjection(axis[0], out var frustumMin, out var frustumMax);

                projectionBounds[0].x = Math.Max(clipBoxMin, frustumMin) - base_;
                projectionBounds[1].x = Math.Min(clipBoxMax, frustumMax) - base_;
                projectionBounds[0].y = projectionBounds[0].z = -1f;
                projectionBounds[1].y = projectionBounds[1].z = 1f;
                return true;
            }

            projectionBounds.Clear();

            // clip the outer edges of the given frustum to the clip bounds

            frustum.ClipFrustumToBox(clipBox, clipFractions, clipPlanes);
            usedClipPlanes = clipPlanes[0] | clipPlanes[1] | clipPlanes[2] | clipPlanes[3];

            // transform the clipped frustum to the space of this frustum
            transpose = new(axis);
            transpose.TransposeSelf();
            localFrustum = new(frustum);
            localFrustum.origin = (frustum.origin - origin) * transpose;
            localFrustum.axis = frustum.axis * transpose;
            localFrustum.ToClippedPoints(clipFractions, out clipPoints);

            // test outer four edges of the clipped frustum
            for (i = 0; i < 4; i++)
            {
                p1 = i;
                p2 = 4 + i;
                AddLocalLineToProjectionBoundsSetCull(clipPoints[p1], clipPoints[p2], out clipPointCull[p1], out clipPointCull[p2], projectionBounds);
            }

            // get cull bits for the clipped frustum
            outside = clipPointCull[0] | clipPointCull[1] | clipPointCull[2] | clipPointCull[3] |
                      clipPointCull[4] | clipPointCull[5] | clipPointCull[6] | clipPointCull[7];
            nearCull = clipPointCull[0] & clipPointCull[1] & clipPointCull[2] & clipPointCull[3];
            farCull = clipPointCull[4] & clipPointCull[5] & clipPointCull[6] & clipPointCull[7];

            // if the clipped frustum is not completely inside this frustum
            if (outside != 0)
            {
                // test the remaining edges of the clipped frustum
                if (nearCull == 0 && localFrustum.dNear > 0f)
                    for (i = 0; i < 4; i++)
                    {
                        p1 = i;
                        p2 = (i + 1) & 3;
                        AddLocalLineToProjectionBoundsUseCull(clipPoints[p1], clipPoints[p2], clipPointCull[p1], clipPointCull[p2], projectionBounds);
                    }

                if (farCull == 0)
                    for (i = 0; i < 4; i++)
                    {
                        p1 = 4 + i;
                        p2 = 4 + ((i + 1) & 3);
                        AddLocalLineToProjectionBoundsUseCull(clipPoints[p1], clipPoints[p2], clipPointCull[p1], clipPointCull[p2], projectionBounds);
                    }
            }

            // if the clipped frustum far end points are inside this frustum
            if (!(farCull != 0 && (nearCull & farCull) == 0) &&
                // if the clipped frustum is not clipped to a single plane of the clip bounds
                (clipPlanes[0] != clipPlanes[1] || clipPlanes[1] != clipPlanes[2] || clipPlanes[2] != clipPlanes[3]))
            {
                // transform the clip box into the space of the other frustum
                transpose = new(frustum.axis);
                transpose.TransposeSelf();
                localOrigin1 = (clipBox.Center - frustum.origin) * transpose;
                localAxis1 = clipBox.Axis * transpose;
                BoxToPoints(localOrigin1, clipBox.Extents, localAxis1, out localPoints1);

                // cull the box corners with the other frustum
                leftScale = frustum.dLeft * frustum.invFar;
                upScale = frustum.dUp * frustum.invFar;
                for (i = 0; i < 8; i++)
                {
                    var p = localPoints1[i];
                    if ((BoxVertPlanes[i] & usedClipPlanes) == 0 || p.x <= 0f) boxPointCull[i] = 1 | 2 | 4 | 8;
                    else
                    {
                        boxPointCull[i] = 0;
                        if (MathX.Fabs(p.y) > p.x * leftScale) boxPointCull[i] |= 1 << MathX.FLOATSIGNBITSET_(p.y);
                        if (MathX.Fabs(p.z) > p.x * upScale) boxPointCull[i] |= 4 << MathX.FLOATSIGNBITSET_(p.z);
                    }
                }

                // transform the clip box into the space of this frustum
                transpose = axis;
                transpose.TransposeSelf();
                localOrigin2 = (clipBox.Center - origin) * transpose;
                localAxis2 = clipBox.Axis * transpose;
                BoxToPoints(localOrigin2, clipBox.Extents, localAxis2, out localPoints2);

                // clip the edges of the clip bounds to the other frustum and add the clipped edges to the projection bounds
                for (i = 0; i < 4; i++)
                {
                    p1 = i;
                    p2 = 4 + i;
                    if ((boxPointCull[p1] & boxPointCull[p2]) == 0 && frustum.ClipLine(localPoints1, localPoints2, p1, p2, out start, out end, out startClip, out endClip)) //: opt
                    {
                        AddLocalLineToProjectionBoundsSetCull(start, end, out pointCull[0], out pointCull[1], projectionBounds);
                        AddLocalCapsToProjectionBounds(clipPoints.AsSpan(4), clipPointCull.AsSpan(4), start, pointCull[0], startClip, projectionBounds);
                        AddLocalCapsToProjectionBounds(clipPoints.AsSpan(4), clipPointCull.AsSpan(4), end, pointCull[1], endClip, projectionBounds);
                        outside |= pointCull[0] | pointCull[1];
                    }
                }

                for (i = 0; i < 4; i++)
                {
                    p1 = i;
                    p2 = (i + 1) & 3;
                    if ((boxPointCull[p1] & boxPointCull[p2]) == 0 && frustum.ClipLine(localPoints1, localPoints2, p1, p2, out start, out end, out startClip, out endClip)) //: opt
                    {
                        AddLocalLineToProjectionBoundsSetCull(start, end, out pointCull[0], out pointCull[1], projectionBounds);
                        AddLocalCapsToProjectionBounds(clipPoints.AsSpan(4), clipPointCull.AsSpan(4), start, pointCull[0], startClip, projectionBounds);
                        AddLocalCapsToProjectionBounds(clipPoints.AsSpan(4), clipPointCull.AsSpan(4), end, pointCull[1], endClip, projectionBounds);
                        outside |= pointCull[0] | pointCull[1];
                    }
                }

                for (i = 0; i < 4; i++)
                {
                    p1 = 4 + i;
                    p2 = 4 + ((i + 1) & 3);
                    if ((boxPointCull[p1] & boxPointCull[p2]) == 0 && frustum.ClipLine(localPoints1, localPoints2, p1, p2, out start, out end, out startClip, out endClip)) //: opt
                    {
                        AddLocalLineToProjectionBoundsSetCull(start, end, out pointCull[0], out pointCull[1], projectionBounds);
                        AddLocalCapsToProjectionBounds(clipPoints.AsSpan(4), clipPointCull.AsSpan(4), start, pointCull[0], startClip, projectionBounds);
                        AddLocalCapsToProjectionBounds(clipPoints.AsSpan(4), clipPointCull.AsSpan(4), end, pointCull[1], endClip, projectionBounds);
                        outside |= pointCull[0] | pointCull[1];
                    }
                }
            }

            // if the clipped frustum extends beyond two or more boundaries of this frustum
            if (outside != 1 && outside != 2 && outside != 4 && outside != 8)
            {
                // transform this frustum into the space of the other frustum
                transpose = new(frustum.axis);
                transpose.TransposeSelf();
                localOrigin1 = (origin - frustum.origin) * transpose;
                localAxis1 = axis * transpose;
                localAxis1[0] *= dFar;
                localAxis1[1] *= dLeft;
                localAxis1[2] *= dUp;

                // transform this frustum into the space of the clip bounds
                transpose = new(clipBox.Axis);
                transpose.TransposeSelf();
                localOrigin2 = (origin - clipBox.Center) * transpose;
                localAxis2 = axis * transpose;
                localAxis2[0] *= dFar;
                localAxis2[1] *= dLeft;
                localAxis2[2] *= dUp;

                clipBounds[0] = -clipBox.Extents;
                clipBounds[1] = clipBox.Extents;

                // test the outer edges of this frustum for intersection with both the other frustum and the clip bounds
                if ((outside & 2) != 0 && (outside & 8) != 0)
                {
                    frustum.LocalRayIntersection(localOrigin1, localAxis1[0] - localAxis1[1] - localAxis1[2], out s1, out s2);
                    if (s1 <= s2 && s1 >= 0f)
                    {
                        BoundsRayIntersection(clipBounds, localOrigin2, localAxis2[0] - localAxis2[1] - localAxis2[2], out t1, out t2);
                        if (t1 <= t2 && t2 > s1 && t1 < s2)
                        {
                            projectionBounds.AddPoint(new Vector3(s1 * dFar, -1f, -1f));
                            projectionBounds.AddPoint(new Vector3(s2 * dFar, -1f, -1f));
                        }
                    }
                }
                if ((outside & 2) != 0 && (outside & 4) != 0)
                {
                    frustum.LocalRayIntersection(localOrigin1, localAxis1[0] - localAxis1[1] + localAxis1[2], out s1, out s2);
                    if (s1 <= s2 && s1 >= 0f)
                    {
                        BoundsRayIntersection(clipBounds, localOrigin2, localAxis2[0] - localAxis2[1] + localAxis2[2], out t1, out t2);
                        if (t1 <= t2 && t2 > s1 && t1 < s2)
                        {
                            projectionBounds.AddPoint(new Vector3(s1 * dFar, -1f, 1f));
                            projectionBounds.AddPoint(new Vector3(s2 * dFar, -1f, 1f));
                        }
                    }
                }
                if ((outside & 1) != 0 && (outside & 8) != 0)
                {
                    frustum.LocalRayIntersection(localOrigin1, localAxis1[0] + localAxis1[1] - localAxis1[2], out s1, out s2);
                    if (s1 <= s2 && s1 >= 0f)
                    {
                        BoundsRayIntersection(clipBounds, localOrigin2, localAxis2[0] + localAxis2[1] - localAxis2[2], out t1, out t2);
                        if (t1 <= t2 && t2 > s1 && t1 < s2)
                        {
                            projectionBounds.AddPoint(new Vector3(s1 * dFar, 1f, -1f));
                            projectionBounds.AddPoint(new Vector3(s2 * dFar, 1f, -1f));
                        }
                    }
                }
                if ((outside & 1) != 0 && (outside & 2) != 0)
                {
                    frustum.LocalRayIntersection(localOrigin1, localAxis1[0] + localAxis1[1] + localAxis1[2], out s1, out s2);
                    if (s1 <= s2 && s1 >= 0f)
                    {
                        BoundsRayIntersection(clipBounds, localOrigin2, localAxis2[0] + localAxis2[1] + localAxis2[2], out t1, out t2);
                        if (t1 <= t2 && t2 > s1 && t1 < s2)
                        {
                            projectionBounds.AddPoint(new Vector3(s1 * dFar, 1f, 1f));
                            projectionBounds.AddPoint(new Vector3(s2 * dFar, 1f, 1f));
                        }
                    }
                }
            }

            return true;
        }

        // Tests if any of the planes of the frustum can be used as a separating plane.
        // 3 muls best case
        // 25 muls worst case
        bool CullLocalBox(in Vector3 localOrigin, in Vector3 extents, in Matrix3x3 localAxis)
        {
            // near plane
            var d1 = dNear - localOrigin.x;
            var d2 =
                MathX.Fabs(extents.x * localAxis[0][0]) +
                MathX.Fabs(extents.y * localAxis[1][0]) +
                MathX.Fabs(extents.z * localAxis[2][0]);
            if (d1 - d2 > 0f) return true;

            // far plane
            d1 = localOrigin.x - dFar;
            if (d1 - d2 > 0f) return true;

            Vector3 testOrigin = localOrigin;
            Matrix3x3 testAxis = new(localAxis);

            if (testOrigin.y < 0f)
            {
                testOrigin.y = -testOrigin.y;
                testAxis[0][1] = -testAxis[0][1];
                testAxis[1][1] = -testAxis[1][1];
                testAxis[2][1] = -testAxis[2][1];
            }

            // test left/right planes
            d1 = dFar * testOrigin.y - dLeft * testOrigin.x;
            d2 =
                MathX.Fabs(extents.x * (dFar * testAxis[0][1] - dLeft * testAxis[0][0])) +
                MathX.Fabs(extents.y * (dFar * testAxis[1][1] - dLeft * testAxis[1][0])) +
                MathX.Fabs(extents.z * (dFar * testAxis[2][1] - dLeft * testAxis[2][0]));
            if (d1 - d2 > 0f) return true;

            if (testOrigin.z < 0f)
            {
                testOrigin.z = -testOrigin.z;
                testAxis[0][2] = -testAxis[0][2];
                testAxis[1][2] = -testAxis[1][2];
                testAxis[2][2] = -testAxis[2][2];
            }

            // test up/down planes
            d1 = dFar * testOrigin.z - dUp * testOrigin.x;
            d2 =
                MathX.Fabs(extents.x * (dFar * testAxis[0][2] - dUp * testAxis[0][0])) +
                MathX.Fabs(extents.y * (dFar * testAxis[1][2] - dUp * testAxis[1][0])) +
                MathX.Fabs(extents.z * (dFar * testAxis[2][2] - dUp * testAxis[2][0]));
            if (d1 - d2 > 0f) return true;

            return false;
        }

        // Tests if any of the planes of this frustum can be used as a separating plane.
        // 0 muls best case
        // 30 muls worst case
        bool CullLocalFrustum(in Frustum localFrustum, Vector3[] indexPoints, Vector3[] cornerVecs)
        {
            // test near plane
            var dy = -localFrustum.axis[1].x;
            var dz = -localFrustum.axis[2].x;
            var index = (MathX.FLOATSIGNBITSET_(dy) << 1) | MathX.FLOATSIGNBITSET_(dz);
            var dx = -cornerVecs[index].x;
            index |= (MathX.FLOATSIGNBITSET_(dx) << 2);

            if (indexPoints[index].x < dNear) return true;

            // test far plane
            dy = localFrustum.axis[1].x;
            dz = localFrustum.axis[2].x;
            index = (MathX.FLOATSIGNBITSET_(dy) << 1) | MathX.FLOATSIGNBITSET_(dz);
            dx = cornerVecs[index].x;
            index |= (MathX.FLOATSIGNBITSET_(dx) << 2);

            if (indexPoints[index].x > dFar) return true;

            var leftScale = dLeft * invFar;

            // test left plane
            dy = dFar * localFrustum.axis[1].y - dLeft * localFrustum.axis[1].x;
            dz = dFar * localFrustum.axis[2].y - dLeft * localFrustum.axis[2].x;
            index = (MathX.FLOATSIGNBITSET_(dy) << 1) | MathX.FLOATSIGNBITSET_(dz);
            dx = dFar * cornerVecs[index].y - dLeft * cornerVecs[index].x;
            index |= (MathX.FLOATSIGNBITSET_(dx) << 2);

            if (indexPoints[index].y > indexPoints[index].x * leftScale) return true;

            // test right plane
            dy = -dFar * localFrustum.axis[1].y - dLeft * localFrustum.axis[1].x;
            dz = -dFar * localFrustum.axis[2].y - dLeft * localFrustum.axis[2].x;
            index = (MathX.FLOATSIGNBITSET_(dy) << 1) | MathX.FLOATSIGNBITSET_(dz);
            dx = -dFar * cornerVecs[index].y - dLeft * cornerVecs[index].x;
            index |= (MathX.FLOATSIGNBITSET_(dx) << 2);

            if (indexPoints[index].y < -indexPoints[index].x * leftScale) return true;

            var upScale = dUp * invFar;

            // test up plane
            dy = dFar * localFrustum.axis[1].z - dUp * localFrustum.axis[1].x;
            dz = dFar * localFrustum.axis[2].z - dUp * localFrustum.axis[2].x;
            index = (MathX.FLOATSIGNBITSET_(dy) << 1) | MathX.FLOATSIGNBITSET_(dz);
            dx = dFar * cornerVecs[index].z - dUp * cornerVecs[index].x;
            index |= (MathX.FLOATSIGNBITSET_(dx) << 2);

            if (indexPoints[index].z > indexPoints[index].x * upScale) return true;

            // test down plane
            dy = -dFar * localFrustum.axis[1].z - dUp * localFrustum.axis[1].x;
            dz = -dFar * localFrustum.axis[2].z - dUp * localFrustum.axis[2].x;
            index = (MathX.FLOATSIGNBITSET_(dy) << 1) | MathX.FLOATSIGNBITSET_(dz);
            dx = -dFar * cornerVecs[index].z - dUp * cornerVecs[index].x;
            index |= (MathX.FLOATSIGNBITSET_(dx) << 2);

            if (indexPoints[index].z < -indexPoints[index].x * upScale) return true;

            return false;
        }

        unsafe bool CullLocalWinding(Vector3* points, int numPoints, int* pointCull)
        {
            var leftScale = dLeft * invFar;
            var upScale = dUp * invFar;

            var culled = -1;
            for (var i = 0; i < numPoints; i++)
            {
                var p = points[i];
                var pCull = 0;
                if (p.x < dNear) pCull = 1;
                else if (p.x > dFar) pCull = 2;
                if (MathX.Fabs(p.y) > p.x * leftScale) pCull |= 4 << MathX.FLOATSIGNBITSET_(p.y);
                if (MathX.Fabs(p.z) > p.x * upScale) pCull |= 16 << MathX.FLOATSIGNBITSET_(p.z);
                culled &= pCull;
                pointCull[i] = pCull;
            }

            return culled != 0;
        }

        // Tests if any of the bounding box planes can be used as a separating plane.
        bool BoundsCullLocalFrustum(in Bounds bounds, in Frustum localFrustum, Vector3[] indexPoints, Vector3[] cornerVecs)
        {
            var dy = -localFrustum.axis[1].x;
            var dz = -localFrustum.axis[2].x;
            var index = (MathX.FLOATSIGNBITSET_(dy) << 1) | MathX.FLOATSIGNBITSET_(dz);
            var dx = -cornerVecs[index].x;
            index |= (MathX.FLOATSIGNBITSET_(dx) << 2);

            if (indexPoints[index].x < bounds[0].x) return true;

            dy = localFrustum.axis[1].x;
            dz = localFrustum.axis[2].x;
            index = (MathX.FLOATSIGNBITSET_(dy) << 1) | MathX.FLOATSIGNBITSET_(dz);
            dx = cornerVecs[index].x;
            index |= (MathX.FLOATSIGNBITSET_(dx) << 2);

            if (indexPoints[index].x > bounds[1].x) return true;

            dy = -localFrustum.axis[1].y;
            dz = -localFrustum.axis[2].y;
            index = (MathX.FLOATSIGNBITSET_(dy) << 1) | MathX.FLOATSIGNBITSET_(dz);
            dx = -cornerVecs[index].y;
            index |= (MathX.FLOATSIGNBITSET_(dx) << 2);

            if (indexPoints[index].y < bounds[0].y) return true;

            dy = localFrustum.axis[1].y;
            dz = localFrustum.axis[2].y;
            index = (MathX.FLOATSIGNBITSET_(dy) << 1) | MathX.FLOATSIGNBITSET_(dz);
            dx = cornerVecs[index].y;
            index |= (MathX.FLOATSIGNBITSET_(dx) << 2);

            if (indexPoints[index].y > bounds[1].y) return true;

            dy = -localFrustum.axis[1].z;
            dz = -localFrustum.axis[2].z;
            index = (MathX.FLOATSIGNBITSET_(dy) << 1) | MathX.FLOATSIGNBITSET_(dz);
            dx = -cornerVecs[index].z;
            index |= (MathX.FLOATSIGNBITSET_(dx) << 2);

            if (indexPoints[index].z < bounds[0].z) return true;

            dy = localFrustum.axis[1].z;
            dz = localFrustum.axis[2].z;
            index = (MathX.FLOATSIGNBITSET_(dy) << 1) | MathX.FLOATSIGNBITSET_(dz);
            dx = cornerVecs[index].z;
            index |= (MathX.FLOATSIGNBITSET_(dx) << 2);

            if (indexPoints[index].z > bounds[1].z) return true;

            return false;
        }

        // 7 divs
        // 30 muls
        bool LocalLineIntersection(in Vector3 start, in Vector3 end)
        {
            float d1, d2, fstart, fend, lstart, lend, f, x; int startInside = 1;

            var leftScale = dLeft * invFar;
            var upScale = dUp * invFar;
            var dir = end - start;

            // test near plane
            if (dNear > 0f)
            {
                d1 = dNear - start.x;
                startInside &= MathX.FLOATSIGNBITSET_(d1);
                if (MathX.FLOATNOTZERO(d1))
                {
                    d2 = dNear - end.x;
                    if (MathX.FLOATSIGNBITSET(d1) ^ MathX.FLOATSIGNBITSET(d2))
                    {
                        f = d1 / (d1 - d2);
                        if (MathX.Fabs(start.y + f * dir.y) <= dNear * leftScale && MathX.Fabs(start.z + f * dir.z) <= dNear * upScale) return true;
                    }
                }
            }

            // test far plane
            d1 = start.x - dFar;
            startInside &= MathX.FLOATSIGNBITSET_(d1);
            if (MathX.FLOATNOTZERO(d1))
            {
                d2 = end.x - dFar;
                if (MathX.FLOATSIGNBITSET(d1) ^ MathX.FLOATSIGNBITSET(d2))
                {
                    f = d1 / (d1 - d2);
                    if (MathX.Fabs(start.y + f * dir.y) <= dFar * leftScale && MathX.Fabs(start.z + f * dir.z) <= dFar * upScale) return true;
                }
            }

            fstart = dFar * start.y;
            fend = dFar * end.y;
            lstart = dLeft * start.x;
            lend = dLeft * end.x;

            // test left plane
            d1 = fstart - lstart;
            startInside &= MathX.FLOATSIGNBITSET_(d1);
            if (MathX.FLOATNOTZERO(d1))
            {
                d2 = fend - lend;
                if (MathX.FLOATSIGNBITSET(d1) ^ MathX.FLOATSIGNBITSET(d2))
                {
                    f = d1 / (d1 - d2);
                    x = start.x + f * dir.x;
                    if (x >= dNear && x <= dFar && MathX.Fabs(start.z + f * dir.z) <= x * upScale) return true;
                }
            }

            // test right plane
            d1 = -fstart - lstart;
            startInside &= MathX.FLOATSIGNBITSET_(d1);
            if (MathX.FLOATNOTZERO(d1))
            {
                d2 = -fend - lend;
                if (MathX.FLOATSIGNBITSET(d1) ^ MathX.FLOATSIGNBITSET(d2))
                {
                    f = d1 / (d1 - d2);
                    x = start.x + f * dir.x;
                    if (x >= dNear && x <= dFar && MathX.Fabs(start.z + f * dir.z) <= x * upScale) return true;
                }
            }

            fstart = dFar * start.z;
            fend = dFar * end.z;
            lstart = dUp * start.x;
            lend = dUp * end.x;

            // test up plane
            d1 = fstart - lstart;
            startInside &= MathX.FLOATSIGNBITSET_(d1);
            if (MathX.FLOATNOTZERO(d1))
            {
                d2 = fend - lend;
                if (MathX.FLOATSIGNBITSET(d1) ^ MathX.FLOATSIGNBITSET(d2))
                {
                    f = d1 / (d1 - d2);
                    x = start.x + f * dir.x;
                    if (x >= dNear && x <= dFar && MathX.Fabs(start.y + f * dir.y) <= x * leftScale) return true;
                }
            }

            // test down plane
            d1 = -fstart - lstart;
            startInside &= MathX.FLOATSIGNBITSET_(d1);
            if (MathX.FLOATNOTZERO(d1))
            {
                d2 = -fend - lend;
                if (MathX.FLOATSIGNBITSET(d1) ^ MathX.FLOATSIGNBITSET(d2))
                {
                    f = d1 / (d1 - d2);
                    x = start.x + f * dir.x;
                    if (x >= dNear && x <= dFar && MathX.Fabs(start.y + f * dir.y) <= x * leftScale) return true;
                }
            }

            return startInside != 0;
        }

        // Returns true if the ray starts inside the frustum. If there was an intersection scale1 <= scale2
        bool LocalRayIntersection(in Vector3 start, in Vector3 dir, out float scale1, out float scale2)
        {
            float d1, d2, fstart, fend, lstart, lend, f, x; int startInside = 1;

            var leftScale = dLeft * invFar;
            var upScale = dUp * invFar;
            var end = start + dir;

            scale1 = MathX.INFINITY;
            scale2 = -MathX.INFINITY;

            // test near plane
            if (dNear > 0f)
            {
                d1 = dNear - start.x;
                startInside &= MathX.FLOATSIGNBITSET_(d1);
                d2 = dNear - end.x;
                if (d1 != d2)
                {
                    f = d1 / (d1 - d2);
                    if (MathX.Fabs(start.y + f * dir.y) <= dNear * leftScale && MathX.Fabs(start.z + f * dir.z) <= dNear * upScale)
                    {
                        if (f < scale1) scale1 = f;
                        if (f > scale2) scale2 = f;
                    }
                }
            }

            // test far plane
            d1 = start.x - dFar;
            startInside &= MathX.FLOATSIGNBITSET_(d1);
            d2 = end.x - dFar;
            if (d1 != d2)
            {
                f = d1 / (d1 - d2);
                if (MathX.Fabs(start.y + f * dir.y) <= dFar * leftScale && MathX.Fabs(start.z + f * dir.z) <= dFar * upScale)
                {
                    if (f < scale1) scale1 = f;
                    if (f > scale2) scale2 = f;
                }
            }

            fstart = dFar * start.y;
            fend = dFar * end.y;
            lstart = dLeft * start.x;
            lend = dLeft * end.x;

            // test left plane
            d1 = fstart - lstart;
            startInside &= MathX.FLOATSIGNBITSET_(d1);
            d2 = fend - lend;
            if (d1 != d2)
            {
                f = d1 / (d1 - d2);
                x = start.x + f * dir.x;
                if (x >= dNear && x <= dFar && MathX.Fabs(start.z + f * dir.z) <= x * upScale)
                {
                    if (f < scale1) scale1 = f;
                    if (f > scale2) scale2 = f;
                }
            }

            // test right plane
            d1 = -fstart - lstart;
            startInside &= MathX.FLOATSIGNBITSET_(d1);
            d2 = -fend - lend;
            if (d1 != d2)
            {
                f = d1 / (d1 - d2);
                x = start.x + f * dir.x;
                if (x >= dNear && x <= dFar && MathX.Fabs(start.z + f * dir.z) <= x * upScale)
                {
                    if (f < scale1) scale1 = f;
                    if (f > scale2) scale2 = f;
                }
            }

            fstart = dFar * start.z;
            fend = dFar * end.z;
            lstart = dUp * start.x;
            lend = dUp * end.x;

            // test up plane
            d1 = fstart - lstart;
            startInside &= MathX.FLOATSIGNBITSET_(d1);
            d2 = fend - lend;
            if (d1 != d2)
            {
                f = d1 / (d1 - d2);
                x = start.x + f * dir.x;
                if (x >= dNear && x <= dFar && MathX.Fabs(start.y + f * dir.y) <= x * leftScale)
                {
                    if (f < scale1) scale1 = f;
                    if (f > scale2) scale2 = f;
                }
            }

            // test down plane
            d1 = -fstart - lstart;
            startInside &= MathX.FLOATSIGNBITSET_(d1);
            d2 = -fend - lend;
            if (d1 != d2)
            {
                f = d1 / (d1 - d2);
                x = start.x + f * dir.x;
                if (x >= dNear && x <= dFar && MathX.Fabs(start.y + f * dir.y) <= x * leftScale)
                {
                    if (f < scale1) scale1 = f;
                    if (f > scale2) scale2 = f;
                }
            }

            return startInside != 0;
        }

        bool LocalFrustumIntersectsFrustum(Vector3[] points, bool testFirstSide)
        {
            int i;
            // test if any edges of the other frustum intersect this frustum
            for (i = 0; i < 4; i++) if (LocalLineIntersection(points[i], points[4 + i])) return true;
            if (testFirstSide) for (i = 0; i < 4; i++) if (LocalLineIntersection(points[i], points[(i + 1) & 3])) return true;
            for (i = 0; i < 4; i++) if (LocalLineIntersection(points[4 + i], points[4 + ((i + 1) & 3)])) return true;
            return false;
        }

        bool LocalFrustumIntersectsBounds(Vector3[] points, Bounds bounds)
        {
            int i;
            // test if any edges of the other frustum intersect this frustum
            for (i = 0; i < 4; i++) if (bounds.LineIntersection(points[i], points[4 + i])) return true;
            if (dNear > 0f) for (i = 0; i < 4; i++) if (bounds.LineIntersection(points[i], points[(i + 1) & 3])) return true;
            for (i = 0; i < 4; i++) if (bounds.LineIntersection(points[4 + i], points[4 + ((i + 1) & 3)])) return true;
            return false;
        }
    }
}