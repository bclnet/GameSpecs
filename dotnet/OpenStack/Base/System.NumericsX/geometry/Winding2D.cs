using System.Diagnostics;
using System.Runtime.CompilerServices;
using static System.NumericsX.Plane;
using static System.NumericsX.Platform;

namespace System.NumericsX
{
    public class Winding2D
    {
        const int MAX_POINTS_ON_WINDING_2D = 16;

        int numPoints;
        Vector2[] p = new Vector2[MAX_POINTS_ON_WINDING_2D];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Winding2D(in Winding2D winding)
        {
            for (var i = 0; i < winding.numPoints; i++) p[i] = winding.p[i];
            numPoints = winding.numPoints;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Winding2D()
           => numPoints = 0;

        public ref Vector2 this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref p[index];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
            => numPoints = 0;

        public void AddPoint(in Vector2 point)
           => p[numPoints++] = point;

        public int NumPoints
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => numPoints;
        }

        static bool GetAxialBevel(in Vector3 plane1, in Vector3 plane2, in Vector2 point, out Vector3 bevel)
        {
            bevel = new();
            if (MathX.FLOATSIGNBITSET(plane1.x) ^ MathX.FLOATSIGNBITSET(plane2.x) && MathX.Fabs(plane1.x) > 0.1f && MathX.Fabs(plane2.x) > 0.1f) { bevel.x = 0f; bevel.y = MathX.FLOATSIGNBITSET(plane1.y) ? -1f : 1f; bevel.z = -(point.x * bevel.x + point.y * bevel.y); return true; }
            if (MathX.FLOATSIGNBITSET(plane1.y) ^ MathX.FLOATSIGNBITSET(plane2.y) && MathX.Fabs(plane1.y) > 0.1f && MathX.Fabs(plane2.y) > 0.1f) { bevel.y = 0f; bevel.x = MathX.FLOATSIGNBITSET(plane1.x) ? -1f : 1f; bevel.z = -(point.x * bevel.x + point.y * bevel.y); return true; }
            return false;
        }

        public unsafe void Expand(float d)
        {
            int i;
            var edgeNormals = stackalloc Vector2[MAX_POINTS_ON_WINDING_2D];

            for (i = 0; i < numPoints; i++)
            {
                var start = p[i];
                var end = p[(i + 1) % numPoints];
                edgeNormals[i].x = start.y - end.y;
                edgeNormals[i].y = end.x - start.x;
                edgeNormals[i].Normalize();
                edgeNormals[i] *= d;
            }

            for (i = 0; i < numPoints; i++) p[i] += edgeNormals[i] + edgeNormals[(i + numPoints - 1) % numPoints];
        }

        public unsafe void ExpandForAxialBox(Vector2[] bounds)
        {
            int i, j, numPlanes; Vector2 v; Vector3 plane, bevel;

            var planes = stackalloc Vector3[MAX_POINTS_ON_WINDING_2D];

            // get planes for the edges and add bevels
            for (numPlanes = i = 0; i < numPoints; i++)
            {
                j = (i + 1) % numPoints;
                if ((p[j] - p[i]).LengthSqr < 0.01f) continue;
                plane = Plane2DFromPoints(p[i], p[j], true);
                if (numPlanes > 0 && GetAxialBevel(planes[numPlanes - 1], plane, p[i], out bevel)) planes[numPlanes++] = bevel;
                Debug.Assert(numPlanes < MAX_POINTS_ON_WINDING_2D);
                planes[numPlanes++] = plane;
            }
            if (GetAxialBevel(planes[numPlanes - 1], planes[0], p[0], out bevel)) planes[numPlanes++] = bevel;

            // expand the planes
            for (i = 0; i < numPlanes; i++)
            {
                v.x = bounds[MathX.FLOATSIGNBITSET_(planes[i].x)].x;
                v.y = bounds[MathX.FLOATSIGNBITSET_(planes[i].y)].y;
                planes[i].z += v.x * planes[i].x + v.y * planes[i].y;
            }

            // get intersection points of the planes
            for (numPoints = i = 0; i < numPlanes; i++) if (Plane2DIntersection(planes[(i + numPlanes - 1) % numPlanes], planes[i], ref p[numPoints])) numPoints++;
        }

        // splits the winding into a front and back winding, the winding itself stays unchanged, returns a SIDE_?
        public unsafe int Split(in Vector3 plane, float epsilon, out Winding2D front, out Winding2D back)
        {
            int i, j; float dot; Vector2 mid = new(); Vector2* p1, p2; Winding2D f, b;

            var dists = stackalloc float[MAX_POINTS_ON_WINDING_2D];
            var sides = stackalloc byte[MAX_POINTS_ON_WINDING_2D];
            var counts = stackalloc int[3];

            counts[0] = counts[1] = counts[2] = 0;

            // determine sides for each point
            for (i = 0; i < numPoints; i++)
            {
                dists[i] = dot = plane.x * p[i].x + plane.y * p[i].y + plane.z;
                sides[i] = (byte)(dot > epsilon ? SIDE_FRONT : dot < -epsilon ? SIDE_BACK : SIDE_ON);
                counts[sides[i]]++;
            }
            sides[i] = sides[0];
            dists[i] = dists[0];

            front = back = null;

            // if nothing at the front of the clipping plane
            if (counts[SIDE_FRONT] == 0) { back = Copy(); return SIDE_BACK; }
            // if nothing at the back of the clipping plane
            if (counts[SIDE_BACK] == 0) { front = Copy(); return SIDE_FRONT; }

            front = f = new Winding2D();
            back = b = new Winding2D();
            fixed (Vector2* p = this.p)
                for (i = 0; i < numPoints; i++)
                {
                    p1 = &p[i];

                    if (sides[i] == SIDE_ON) { f.p[f.numPoints] = *p1; f.numPoints++; b.p[b.numPoints] = *p1; b.numPoints++; continue; }
                    if (sides[i] == SIDE_FRONT) { f.p[f.numPoints] = *p1; f.numPoints++; }
                    if (sides[i] == SIDE_BACK) { b.p[b.numPoints] = *p1; b.numPoints++; }
                    if (sides[i + 1] == SIDE_ON || sides[i + 1] == sides[i]) continue;

                    // generate a split point
                    p2 = &p[(i + 1) % numPoints];

                    // always calculate the split going from the same side
                    // or minor epsilon issues can happen
                    if (sides[i] == SIDE_FRONT)
                    {
                        dot = dists[i] / (dists[i] - dists[i + 1]);
                        // avoid round off error when possible
                        for (j = 0; j < 2; j++)
                            mid[j] = plane[j] == 1f ? plane.z
                                : plane[j] == -1f ? -plane.z
                                : (*p1)[j] + dot * ((*p2)[j] - (*p1)[j]);
                    }
                    else
                    {
                        dot = dists[i + 1] / (dists[i + 1] - dists[i]);
                        // avoid round off error when possible
                        for (j = 0; j < 2; j++)
                            mid[j] = plane[j] == 1f ? plane.z
                                : plane[j] == -1f ? -plane.z
                                : (*p2)[j] + dot * ((*p1)[j] - (*p2)[j]);
                    }

                    f.p[f.numPoints] = mid;
                    f.numPoints++;
                    b.p[b.numPoints] = mid;
                    b.numPoints++;
                }

            return SIDE_CROSS;
        }

        // cuts off the part at the back side of the plane, returns true if some part was at the front, if there is nothing at the front the number of points is set to zero
        public unsafe bool ClipInPlace(in Vector3 plane, float epsilon = Plane.ON_EPSILON, bool keepOn = false)
        {
            int i, j, maxpts, newNumPoints;
            int* sides = stackalloc int[MAX_POINTS_ON_WINDING_2D + 1], counts = stackalloc int[3];
            float dot; float* dists = stackalloc float[MAX_POINTS_ON_WINDING_2D + 1];
            Vector2 mid = new(); Vector2* p1, p2, newPoints = stackalloc Vector2[MAX_POINTS_ON_WINDING_2D + 4];

            counts[SIDE_FRONT] = counts[SIDE_BACK] = counts[SIDE_ON] = 0;

            for (i = 0; i < numPoints; i++)
            {
                dists[i] = dot = plane.x * p[i].x + plane.y * p[i].y + plane.z;
                sides[i] = dot > epsilon ? SIDE_FRONT : dot < -epsilon ? SIDE_BACK : SIDE_ON;
                counts[sides[i]]++;
            }
            sides[i] = sides[0];
            dists[i] = dists[0];

            // if the winding is on the plane and we should keep it
            if (keepOn && counts[SIDE_FRONT] == 0 && counts[SIDE_BACK] == 0) return true;
            if (counts[SIDE_FRONT] == 0) { numPoints = 0; return false; }
            if (counts[SIDE_BACK] == 0) return true;

            maxpts = numPoints + 4;     // cant use counts[0]+2 because of fp grouping errors
            newNumPoints = 0;

            fixed (Vector2* p = this.p)
            {
                for (i = 0; i < numPoints; i++)
                {
                    p1 = &p[i];

                    if (newNumPoints + 1 > maxpts) return true;        // can't split -- fall back to original

                    if (sides[i] == SIDE_ON) { newPoints[newNumPoints] = *p1; newNumPoints++; continue; }
                    if (sides[i] == SIDE_FRONT) { newPoints[newNumPoints] = *p1; newNumPoints++; }
                    if (sides[i + 1] == SIDE_ON || sides[i + 1] == sides[i]) continue;

                    if (newNumPoints + 1 > maxpts) return true;        // can't split -- fall back to original

                    // generate a split point
                    p2 = &p[(i + 1) % numPoints];

                    dot = dists[i] / (dists[i] - dists[i + 1]);
                    for (j = 0; j < 2; j++)
                        // avoid round off error when possible
                        mid[j] = plane[j] == 1f ? plane.z
                            : plane[j] == -1f ? -plane.z
                            : (*p1)[j] + dot * ((*p2)[j] - (*p1)[j]);

                    newPoints[newNumPoints] = mid;
                    newNumPoints++;
                }

                if (newNumPoints >= MAX_POINTS_ON_WINDING_2D)
                    return true;

                numPoints = newNumPoints;

                Unsafe.CopyBlock(p, newPoints, (uint)(newNumPoints * sizeof(Vector2)));
            }

            return true;
        }

        public unsafe Winding2D Copy()
        {
            var w = new Winding2D { numPoints = numPoints };
            fixed (void* w_p = w.p, p = this.p) Unsafe.CopyBlock(w_p, p, (uint)(numPoints * sizeof(Vector2)));
            return w;
        }

        public Winding2D Reverse()
        {
            var w = new Winding2D { numPoints = numPoints };
            for (var i = 0; i < numPoints; i++) w.p[numPoints - i - 1] = p[i];
            return w;
        }

        public float Area
        {
            get
            {
                var total = 0f;
                for (var i = 2; i < numPoints; i++) { var d1 = p[i - 1] - p[0]; var d2 = p[i] - p[0]; total += d1.x * d2.y - d1.y * d2.x; }
                return total * 0.5f;
            }
        }

        public Vector2 Center
        {
            get
            {
                Vector2 center = new();
                center.Zero();
                for (var i = 0; i < numPoints; i++) center += p[i];
                center *= (1f / numPoints);
                return center;
            }
        }

        public float GetRadius(in Vector2 center)
        {
            var radius = 0f;
            for (var i = 0; i < numPoints; i++)
            {
                var dir = p[i] - center;
                var r = dir * dir;
                if (r > radius) radius = r;
            }
            return MathX.Sqrt(radius);
        }

        public void GetBounds(Vector2[] bounds)
        {
            if (numPoints == 0) { bounds[0].x = bounds[0].y = MathX.INFINITY; bounds[1].x = bounds[1].y = -MathX.INFINITY; return; }

            bounds[0] = bounds[1] = p[0];
            for (var i = 1; i < numPoints; i++)
            {
                if (p[i].x < bounds[0].x) bounds[0].x = p[i].x;
                else if (p[i].x > bounds[1].x) bounds[1].x = p[i].x;
                if (p[i].y < bounds[0].y) bounds[0].y = p[i].y;
                else if (p[i].y > bounds[1].y) bounds[1].y = p[i].y;
            }
        }

        const float EDGE_LENGTH = 0.2f;
        public bool IsTiny
        {
            get
            {
                var edges = 0;
                for (var i = 0; i < numPoints; i++)
                {
                    var delta = p[(i + 1) % numPoints] - p[i];
                    var len = delta.Length;
                    if (len > EDGE_LENGTH && ++edges == 3) return false;
                }
                return true;
            }
        }

        public bool IsHuge   // base winding for a plane is typically huge
        {
            get
            {
                for (var i = 0; i < numPoints; i++) for (var j = 0; j < 2; j++) if (p[i][j] <= MIN_WORLD_COORD || p[i][j] >= MAX_WORLD_COORD) return true;
                return false;
            }
        }

        public void Print()
        {
            for (var i = 0; i < numPoints; i++) Printf($"({p[i].x:5.1}, {p[i].y:5.1})\n");
        }

        public float PlaneDistance(in Vector3 plane)
        {

            var min = MathX.INFINITY;
            var max = -min;
            for (var i = 0; i < numPoints; i++)
            {
                var d = plane.x * p[i].x + plane.y * p[i].y + plane.z;
                if (d < min) { min = d; if (MathX.FLOATSIGNBITSET(min) & MathX.FLOATSIGNBITNOTSET(max)) return 0f; }
                if (d > max) { max = d; if (MathX.FLOATSIGNBITSET(min) & MathX.FLOATSIGNBITNOTSET(max)) return 0f; }
            }
            if (MathX.FLOATSIGNBITNOTSET(min)) return min;
            if (MathX.FLOATSIGNBITSET(max)) return max;
            return 0f;
        }

        public int PlaneSide(in Vector3 plane, float epsilon = Plane.ON_EPSILON)
        {
            var front = false;
            var back = false;
            for (var i = 0; i < numPoints; i++)
            {
                var d = plane.x * p[i].x + plane.y * p[i].y + plane.z;
                if (d < -epsilon) { if (front) return SIDE_CROSS; back = true; continue; }
                else if (d > epsilon) { if (back) return SIDE_CROSS; front = true; continue; }
            }
            if (back) return SIDE_BACK;
            if (front) return SIDE_FRONT;
            return SIDE_ON;
        }

        public bool PointInside(in Vector2 point, float epsilon)
        {
            for (var i = 0; i < numPoints; i++)
            {
                var plane = Plane2DFromPoints(p[i], p[(i + 1) % numPoints]);
                var d = plane.x * point.x + plane.y * point.y + plane.z;
                if (d > epsilon) return false;
            }
            return true;
        }

        public unsafe bool LineIntersection(in Vector2 start, in Vector2 end)
        {
            int i, numEdges; int* sides = stackalloc int[MAX_POINTS_ON_WINDING_2D + 1], counts = stackalloc int[3];
            float d1, d2, epsilon = 0.1f;
            Vector3 plane; Vector3* edges = stackalloc Vector3[2];

            counts[SIDE_FRONT] = counts[SIDE_BACK] = counts[SIDE_ON] = 0;

            plane = Plane2DFromPoints(start, end);
            for (i = 0; i < numPoints; i++)
            {
                d1 = plane.x * p[i].x + plane.y * p[i].y + plane.z;
                sides[i] = d1 > epsilon ? SIDE_FRONT : d1 < -epsilon ? SIDE_BACK : SIDE_ON;
                counts[sides[i]]++;
            }
            sides[i] = sides[0];

            if (counts[SIDE_FRONT] == 0) return false;
            if (counts[SIDE_BACK] == 0) return false;

            numEdges = 0;
            for (i = 0; i < numPoints; i++)
                if (sides[i] != sides[i + 1] && sides[i + 1] != SIDE_ON)
                {
                    edges[numEdges++] = Plane2DFromPoints(p[i], p[(i + 1) % numPoints]);
                    if (numEdges >= 2) break;
                }
            if (numEdges < 2) return false;

            d1 = edges[0].x * start.x + edges[0].y * start.y + edges[0].z;
            d2 = edges[0].x * end.x + edges[0].y * end.y + edges[0].z;
            if (MathX.FLOATSIGNBITNOTSET(d1) & MathX.FLOATSIGNBITNOTSET(d2)) return false;
            d1 = edges[1].x * start.x + edges[1].y * start.y + edges[1].z;
            d2 = edges[1].x * end.x + edges[1].y * end.y + edges[1].z;
            if (MathX.FLOATSIGNBITNOTSET(d1) & MathX.FLOATSIGNBITNOTSET(d2)) return false;
            return true;
        }

        public unsafe bool RayIntersection(in Vector2 start, in Vector2 dir, out float scale1, out float scale2, int[] edgeNums = null)
        {
            int i, numEdges; int* localEdgeNums = stackalloc int[2], sides = stackalloc int[MAX_POINTS_ON_WINDING_2D + 1], counts = stackalloc int[3];
            float d1, d2, epsilon = 0.1f;
            Vector3 plane; Vector3* edges = stackalloc Vector3[2];

            scale1 = scale2 = 0f;
            counts[SIDE_FRONT] = counts[SIDE_BACK] = counts[SIDE_ON] = 0;

            plane = Plane2DFromVecs(start, dir);
            for (i = 0; i < numPoints; i++)
            {
                d1 = plane.x * p[i].x + plane.y * p[i].y + plane.z;
                sides[i] = d1 > epsilon ? SIDE_FRONT : d1 < -epsilon ? SIDE_BACK : SIDE_ON;
                counts[sides[i]]++;
            }
            sides[i] = sides[0];

            if (counts[SIDE_FRONT] == 0) return false;
            if (counts[SIDE_BACK] == 0) return false;

            numEdges = 0;
            for (i = 0; i < numPoints; i++)
                if (sides[i] != sides[i + 1] && sides[i + 1] != SIDE_ON)
                {
                    localEdgeNums[numEdges] = i;
                    edges[numEdges++] = Plane2DFromPoints(p[i], p[(i + 1) % numPoints]);
                    if (numEdges >= 2) break;
                }
            if (numEdges < 2) return false;

            d1 = edges[0].x * start.x + edges[0].y * start.y + edges[0].z;
            d2 = -(edges[0].x * dir.x + edges[0].y * dir.y);
            if (d2 == 0f) return false;
            scale1 = d1 / d2;
            d1 = edges[1].x * start.x + edges[1].y * start.y + edges[1].z;
            d2 = -(edges[1].x * dir.x + edges[1].y * dir.y);
            if (d2 == 0f) return false;
            scale2 = d1 / d2;

            if (MathX.Fabs(scale1) > MathX.Fabs(scale2)) { Swap(ref scale1, ref scale2); Swap(ref localEdgeNums[0], ref localEdgeNums[1]); }

            if (edgeNums != null) { edgeNums[0] = localEdgeNums[0]; edgeNums[1] = localEdgeNums[1]; }
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Plane2DFromPoints(in Vector2 start, in Vector2 end, bool normalize = false)
        {
            Vector3 plane = new();
            plane.x = start.y - end.y;
            plane.y = end.x - start.x;
            if (normalize) plane.ToVec2().Normalize();
            plane.z = -(start.x * plane.x + start.y * plane.y);
            return plane;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Plane2DFromVecs(in Vector2 start, in Vector2 dir, bool normalize = false)
        {
            Vector3 plane = new();
            plane.x = -dir.y;
            plane.y = dir.x;
            if (normalize) plane.ToVec2().Normalize();
            plane.z = -(start.x * plane.x + start.y * plane.y);
            return plane;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Plane2DIntersection(in Vector3 plane1, in Vector3 plane2, ref Vector2 point)
        {
            float n00, n01, n11, det, invDet, f0, f1;

            n00 = plane1.x * plane1.x + plane1.y * plane1.y;
            n01 = plane1.x * plane2.x + plane1.y * plane2.y;
            n11 = plane2.x * plane2.x + plane2.y * plane2.y;
            det = n00 * n11 - n01 * n01;

            if (MathX.Fabs(det) < 1e-6f) return false;

            invDet = 1f / det;
            f0 = (n01 * plane2.z - n11 * plane1.z) * invDet;
            f1 = (n01 * plane1.z - n00 * plane2.z) * invDet;
            point.x = f0 * plane1.x + f1 * plane2.x;
            point.y = f0 * plane1.y + f1 * plane2.y;
            return true;
        }
    }
}