using System.Diagnostics;
using System.Runtime.CompilerServices;
using static System.NumericsX.Plane;
using static System.NumericsX.Platform;

namespace System.NumericsX
{
    public class Winding
    {
        protected int numPoints;                // number of points
        protected Vector5[] p;                        // pointer to point data
        protected int allocedSize;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Winding()
        {
            numPoints = allocedSize = 0;
            p = null;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Winding(int n)                               // allocate for n points
        {
            numPoints = allocedSize = 0;
            p = null;
            EnsureAlloced(n);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Winding(Vector3[] verts, int n)          // winding from points
        {
            numPoints = allocedSize = 0;
            p = null;
            if (!EnsureAlloced(n)) { numPoints = 0; return; }
            for (var i = 0; i < n; i++) { p[i].ToVec3() = verts[i]; p[i].s = p[i].t = 0f; }
            numPoints = n;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Winding(in Vector3 normal, float dist)    // base winding for plane
        {
            numPoints = allocedSize = 0;
            p = null;
            BaseForPlane(normal, dist);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Winding(in Plane plane)                     // base winding for plane
        {
            numPoints = allocedSize = 0;
            p = null;
            BaseForPlane(plane);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Winding(in Winding winding)
        {
            if (!EnsureAlloced(winding.NumPoints)) { numPoints = 0; return; }
            for (var i = 0; i < winding.NumPoints; i++) p[i] = winding[i];
            numPoints = winding.NumPoints;
        }

        public ref Vector5 this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref p[index];
        }

        // add a point to the end of the winding point array
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Winding operator +(in Winding _, in Vector3 v)
        {
            _.AddPoint(v);
            return _;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Winding operator +(in Winding _, in Vector5 v)
        {
            _.AddPoint(v);
            return _;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddPoint(in Vector3 v)
        {
            if (!EnsureAlloced(numPoints + 1, true)) return;
            p[numPoints] = reinterpret.cast_vec5(v);
            numPoints++;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddPoint(in Vector5 v)
        {
            if (!EnsureAlloced(numPoints + 1, true)) return;
            p[numPoints] = v;
            numPoints++;
        }

        // number of points on winding
        public int NumPoints
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => numPoints;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                if (!EnsureAlloced(value, true)) return;
                numPoints = value;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual void Clear()
        {
            numPoints = 0;
            p = null;
        }

        // huge winding for plane, the points go counter clockwise when facing the front of the plane
        public void BaseForPlane(in Vector3 normal, float dist)
        {
            var org = normal * dist;

            normal.NormalVectors(out var vup, out var vright);
            vup *= MAX_WORLD_SIZE;
            vright *= MAX_WORLD_SIZE;

            EnsureAlloced(4);
            numPoints = 4;
            p[0].ToVec3() = org - vright + vup; p[0].s = p[0].t = 0f;
            p[1].ToVec3() = org + vright + vup; p[1].s = p[1].t = 0f;
            p[2].ToVec3() = org + vright - vup; p[2].s = p[2].t = 0f;
            p[3].ToVec3() = org - vright - vup; p[3].s = p[3].t = 0f;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void BaseForPlane(in Plane plane)
            => BaseForPlane(plane.Normal, plane.Dist);

        // splits the winding into a front and back winding, the winding itself stays unchanged, returns a SIDE_?
        public unsafe int Split(in Plane plane, float epsilon, out Winding front, out Winding back)
        {
            int i, j, maxpts; int* counts = stackalloc int[3]; float dot;
            Vector5* p1, p2; Vector5 mid = new();
            Winding f, b;

            var dists = stackalloc float[numPoints + 4];
            var sides = stackalloc byte[numPoints + 4];

            counts[0] = counts[1] = counts[2] = 0;

            // determine sides for each point
            for (i = 0; i < numPoints; i++)
            {
                dists[i] = dot = plane.Distance(p[i].ToVec3());
                sides[i] = (byte)(dot > epsilon ? SIDE_FRONT : dot < -epsilon ? SIDE_BACK : SIDE_ON);
                counts[sides[i]]++;
            }
            sides[i] = sides[0];
            dists[i] = dists[0];

            front = back = null;

            // if coplanar, put on the front side if the normals match
            if (counts[SIDE_FRONT] == 0 && counts[SIDE_BACK] == 0)
            {
                GetPlane(out var windingPlane);
                if (windingPlane.Normal * plane.Normal > 0f) { front = Copy(); return SIDE_FRONT; }
                else { back = Copy(); return SIDE_BACK; }
            }
            // if nothing at the front of the clipping plane
            if (counts[SIDE_FRONT] == 0) { back = Copy(); return SIDE_BACK; }
            // if nothing at the back of the clipping plane
            if (counts[SIDE_BACK] == 0) { front = Copy(); return SIDE_FRONT; }

            maxpts = numPoints + 4; // cant use counts[0]+2 because of fp grouping errors

            front = f = new Winding(maxpts);
            back = b = new Winding(maxpts);

            fixed (Vector5* p = this.p)
                for (i = 0; i < numPoints; i++)
                {
                    p1 = &p[i];

                    if (sides[i] == SIDE_ON) { f.p[f.numPoints] = *p1; f.numPoints++; b.p[b.numPoints] = *p1; b.numPoints++; continue; }
                    if (sides[i] == SIDE_FRONT) { f.p[f.numPoints] = *p1; f.numPoints++; }
                    if (sides[i] == SIDE_BACK) { b.p[b.numPoints] = *p1; b.numPoints++; }
                    if (sides[i + 1] == SIDE_ON || sides[i + 1] == sides[i]) continue;

                    // generate a split point
                    p2 = &p[(i + 1) % numPoints];

                    // always calculate the split going from the same side or minor epsilon issues can happen
                    if (sides[i] == SIDE_FRONT)
                    {
                        dot = dists[i] / (dists[i] - dists[i + 1]);
                        for (j = 0; j < 3; j++)
                            // avoid round off error when possible
                            mid[j] = plane.Normal[j] == 1f ? plane.Dist
                                : plane.Normal[j] == -1f ? -plane.Dist
                                : (*p1)[j] + dot * ((*p2)[j] - (*p1)[j]);
                        mid.s = p1->s + dot * (p2->s - p1->s);
                        mid.t = p1->t + dot * (p2->t - p1->t);
                    }
                    else
                    {
                        dot = dists[i + 1] / (dists[i + 1] - dists[i]);
                        for (j = 0; j < 3; j++)
                        {
                            // avoid round off error when possible
                            mid[j] = plane.Normal[j] == 1f ? plane.Dist
                            : plane.Normal[j] == -1f ? -plane.Dist
                            : (*p2)[j] + dot * ((*p1)[j] - (*p2)[j]); //: opt
                        }
                        mid.s = p2->s + dot * (p1->s - p2->s);
                        mid.t = p2->t + dot * (p1->t - p2->t);
                    }

                    f.p[f.numPoints] = mid; f.numPoints++;
                    b.p[b.numPoints] = mid; b.numPoints++;
                }

            if (f.numPoints > maxpts || b.numPoints > maxpts) FatalError("Winding::Split: points exceeded estimate.");

            return SIDE_CROSS;
        }

        // returns the winding fragment at the front of the clipping plane, if there is nothing at the front the winding itself is destroyed and null is returned
        public unsafe Winding Clip(in Plane plane, float epsilon = Plane.ON_EPSILON, bool keepOn = false)
        {
            int i, j, newNumPoints, maxpts; int* counts = stackalloc int[3];
            float dot;
            Vector5* p1, p2; Vector5 mid = new();

            var dists = stackalloc float[numPoints + 4];
            var sides = stackalloc byte[numPoints + 4];

            counts[SIDE_FRONT] = counts[SIDE_BACK] = counts[SIDE_ON] = 0;

            // determine sides for each point
            for (i = 0; i < numPoints; i++)
            {
                dists[i] = dot = plane.Distance(p[i].ToVec3());
                sides[i] = (byte)(dot > epsilon ? SIDE_FRONT : dot < -epsilon ? SIDE_BACK : SIDE_ON);
                counts[sides[i]]++;
            }
            sides[i] = sides[0];
            dists[i] = dists[0];

            // if the winding is on the plane and we should keep it
            if (keepOn && counts[SIDE_FRONT] == 0 && counts[SIDE_BACK] == 0) return this;
            // if nothing at the front of the clipping plane
            if (counts[SIDE_FRONT] == 0) return null;
            // if nothing at the back of the clipping plane
            if (counts[SIDE_BACK] == 0) return this;

            maxpts = numPoints + 4;     // cant use counts[0]+2 because of fp grouping errors

            var newPoints = stackalloc Vector5[maxpts + Vector5.ALLOC16]; newPoints = (Vector5*)_alloca16(newPoints);
            newNumPoints = 0;

            fixed (Vector5* p = this.p)
            {
                for (i = 0; i < numPoints; i++)
                {
                    p1 = &p[i];

                    if (newNumPoints + 1 > maxpts) return this;        // can't split -- fall back to original
                    if (sides[i] == SIDE_ON) { newPoints[newNumPoints] = *p1; newNumPoints++; continue; }
                    if (sides[i] == SIDE_FRONT) { newPoints[newNumPoints] = *p1; newNumPoints++; }
                    if (sides[i + 1] == SIDE_ON || sides[i + 1] == sides[i]) continue;
                    if (newNumPoints + 1 > maxpts) return this;        // can't split -- fall back to original

                    // generate a split point
                    p2 = &p[(i + 1) % numPoints];

                    dot = dists[i] / (dists[i] - dists[i + 1]);
                    for (j = 0; j < 3; j++)
                        // avoid round off error when possible
                        mid[j] = plane.Normal[j] == 1f ? plane.Dist
                            : plane.Normal[j] == -1f ? -plane.Dist
                            : (*p1)[j] + dot * ((*p2)[j] - (*p1)[j]);
                    mid.s = p1->s + dot * (p2->s - p1->s);
                    mid.t = p1->t + dot * (p2->t - p1->t);

                    newPoints[newNumPoints] = mid;
                    newNumPoints++;
                }

                if (!EnsureAlloced(newNumPoints, false)) return this;

                numPoints = newNumPoints;
                Unsafe.CopyBlock(p, newPoints, (uint)(newNumPoints * sizeof(Vector5)));
            }
            return this;
        }

        // cuts off the part at the back side of the plane, returns true if some part was at the front, if there is nothing at the front the number of points is set to zero
        public unsafe bool ClipInPlace(in Plane plane, float epsilon = Plane.ON_EPSILON, bool keepOn = false)
        {
            int i, j, maxpts, newNumPoints; int* counts = stackalloc int[3]; float dot;
            Vector5* p1, p2; Vector5 mid = new();

            var dists = stackalloc float[numPoints + 4];
            var sides = stackalloc byte[numPoints + 4];

            counts[SIDE_FRONT] = counts[SIDE_BACK] = counts[SIDE_ON] = 0;

            // determine sides for each point
            for (i = 0; i < numPoints; i++)
            {
                dists[i] = dot = plane.Distance(p[i].ToVec3());
                sides[i] = (byte)(dot > epsilon ? SIDE_FRONT : dot < -epsilon ? SIDE_BACK : SIDE_ON);
                counts[sides[i]]++;
            }
            sides[i] = sides[0];
            dists[i] = dists[0];

            // if the winding is on the plane and we should keep it
            if (keepOn && counts[SIDE_FRONT] == 0 && counts[SIDE_BACK] == 0) return true;
            // if nothing at the front of the clipping plane
            if (counts[SIDE_FRONT] == 0) { numPoints = 0; return false; }
            // if nothing at the back of the clipping plane
            if (counts[SIDE_BACK] == 0) return true;

            maxpts = numPoints + 4;     // cant use counts[0]+2 because of fp grouping errors

            var newPoints = stackalloc Vector5[maxpts + Vector5.ALLOC16]; newPoints = (Vector5*)_alloca16(newPoints);
            newNumPoints = 0;

            fixed (Vector5* p = this.p)
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
                    for (j = 0; j < 3; j++)
                        // avoid round off error when possible
                        mid[j] = plane.Normal[j] == 1f ? plane.Dist
                            : plane.Normal[j] == -1f ? -plane.Dist
                            : (*p1)[j] + dot * ((*p2)[j] - (*p1)[j]);
                    mid.s = p1->s + dot * (p2->s - p1->s);
                    mid.t = p1->t + dot * (p2->t - p1->t);

                    newPoints[newNumPoints] = mid;
                    newNumPoints++;
                }

                if (!EnsureAlloced(newNumPoints, false)) return true;

                numPoints = newNumPoints;
                Unsafe.CopyBlock(p, newPoints, (uint)(newNumPoints * sizeof(Vector5)));
            }
            return true;
        }

        // returns a copy of the winding
        public unsafe Winding Copy()
        {
            var w = new Winding(numPoints) { numPoints = numPoints };
            fixed (void* w_p = w.p, p = this.p) Unsafe.CopyBlock(w_p, p, (uint)(numPoints * sizeof(Vector5)));
            return w;
        }

        public Winding Reverse()
        {
            var w = new Winding(numPoints) { numPoints = numPoints };
            for (var i = 0; i < numPoints; i++) w.p[numPoints - i - 1] = p[i];
            return w;
        }

        public void ReverseSelf()
        {
            Vector5 v; int i;

            for (i = 0; i < (numPoints >> 1); i++)
            {
                v = p[i];
                p[i] = p[numPoints - i - 1];
                p[numPoints - i - 1] = v;
            }
        }

        public void RemoveEqualPoints(float epsilon = Plane.ON_EPSILON)
        {
            int i, j;

            for (i = 0; i < numPoints; i++)
            {
                if ((p[i].ToVec3() - p[(i + numPoints - 1) % numPoints].ToVec3()).LengthSqr >= MathX.Square(epsilon)) continue;
                numPoints--;
                for (j = i; j < numPoints; j++) p[j] = p[j + 1];
                i--;
            }
        }

        public void RemoveColinearPoints(in Vector3 normal, float epsilon = Plane.ON_EPSILON)
        {
            int i, j; float dist; Vector3 edgeNormal;

            if (numPoints <= 3) return;

            for (i = 0; i < numPoints; i++)
            {
                // create plane through edge orthogonal to winding plane
                edgeNormal = (p[i].ToVec3() - p[(i + numPoints - 1) % numPoints].ToVec3()).Cross(normal);
                edgeNormal.Normalize();
                dist = edgeNormal * p[i].ToVec3();

                if (MathX.Fabs(edgeNormal * p[(i + 1) % numPoints].ToVec3() - dist) > epsilon) continue;

                numPoints--;
                for (j = i; j < numPoints; j++) p[j] = p[j + 1];
                i--;
            }
        }

        public unsafe void RemovePoint(int point)
        {
            if (point < 0 || point >= numPoints) FatalError("Winding::removePoint: point out of range");
            if (point < numPoints - 1) fixed (Vector5* p = this.p) UnsafeX.MoveBlock(&p[point], &p[point + 1], (uint)((numPoints - point - 1) * sizeof(Vector5)));
            numPoints--;
        }

        public void InsertPoint(in Vector3 point, int spot)
        {
            int i;

            if (spot > numPoints) FatalError("Winding::insertPoint: spot > numPoints");
            if (spot < 0) FatalError("Winding::insertPoint: spot < 0");

            EnsureAlloced(numPoints + 1, true);
            for (i = numPoints; i > spot; i--) p[i] = p[i - 1];
            p[spot] = reinterpret.cast_vec5(point);
            numPoints++;
        }

        public bool InsertPointIfOnEdge(in Vector3 point, in Plane plane, float epsilon = Plane.ON_EPSILON)
        {
            int i; float dist, dot; Vector3 normal;

            // point may not be too far from the winding plane
            if (MathX.Fabs(plane.Distance(point)) > epsilon) return false;

            for (i = 0; i < numPoints; i++)
            {
                // create plane through edge orthogonal to winding plane
                normal = (p[(i + 1) % numPoints].ToVec3() - p[i].ToVec3()).Cross(plane.Normal);
                normal.Normalize();
                dist = normal * p[i].ToVec3();

                if (MathX.Fabs(normal * point - dist) > epsilon) continue;

                normal = plane.Normal.Cross(normal);
                dot = normal * point;

                dist = dot - normal * p[i].ToVec3();

                // if the winding already has the point
                if (dist < epsilon) { if (dist > -epsilon) return false; continue; }

                dist = dot - normal * p[(i + 1) % numPoints].ToVec3();

                // if the winding already has the point
                if (dist > -epsilon) { if (dist < epsilon) return false; continue; }

                InsertPoint(point, i + 1);
                return true;
            }
            return false;
        }

        // Adds the given winding to the convex hull.
        // Assumes the current winding already is a convex hull with three or more points.
        // add a winding to the convex hull
        public unsafe void AddToConvexHull(in Winding winding, in Vector3 normal, float epsilon = Plane.ON_EPSILON)
        {
            int i, j, k, maxPts, numNewHullPoints; float d; Vector3 dir; bool outside;

            if (winding == null) return;

            maxPts = numPoints + winding.numPoints;

            if (!EnsureAlloced(maxPts, true)) return;

            var newHullPoints = stackalloc Vector5[maxPts];
            var hullDirs = stackalloc Vector3[maxPts];
            var hullSide = stackalloc bool[maxPts];

            for (i = 0; i < winding.numPoints; i++)
            {
                var p1 = winding.p[i];

                // calculate hull edge vectors
                for (j = 0; j < numPoints; j++)
                {
                    dir = p[(j + 1) % numPoints].ToVec3() - p[j].ToVec3();
                    dir.Normalize();
                    hullDirs[j] = normal.Cross(dir);
                }

                // calculate side for each hull edge
                outside = false;
                for (j = 0; j < numPoints; j++)
                {
                    dir = p1.ToVec3() - p[j].ToVec3();
                    d = dir * hullDirs[j];
                    if (d >= epsilon) outside = true;

                    hullSide[j] = d >= -epsilon; //: opt
                }

                // if the point is effectively inside, do nothing
                if (!outside) continue;

                // find the back side to front side transition
                for (j = 0; j < numPoints; j++) if (!hullSide[j] && hullSide[(j + 1) % numPoints]) break;
                if (j >= numPoints) continue;

                // insert the point here
                newHullPoints[0] = p1;
                numNewHullPoints = 1;

                // copy over all points that aren't double fronts
                j = (j + 1) % numPoints;
                for (k = 0; k < numPoints; k++)
                {
                    if (hullSide[(j + k) % numPoints] && hullSide[(j + k + 1) % numPoints]) continue;
                    newHullPoints[numNewHullPoints] = p[(j + k + 1) % numPoints];
                    numNewHullPoints++;
                }

                numPoints = numNewHullPoints;
                fixed (Vector5* p = this.p) Unsafe.CopyBlock(p, newHullPoints, (uint)(numNewHullPoints * sizeof(Vector5)));
            }
        }

        // Add a point to the convex hull.
        // The current winding must be convex but may be degenerate and can have less than three points.
        // add a point to the convex hull
        public unsafe void AddToConvexHull(in Vector3 point, in Vector3 normal, float epsilon = Plane.ON_EPSILON)
        {
            int j, k, numHullPoints; float d; bool outside; Vector3 dir;

            switch (numPoints)
            {
                case 0: p[0] = reinterpret.cast_vec5(point); numPoints++; return;
                // don't add the same point second
                case 1: if (p[0].ToVec3().Compare(point, epsilon)) return; p[1].ToVec3() = point; numPoints++; return;
                case 2:
                    // don't add a point if it already exists
                    if (p[0].ToVec3().Compare(point, epsilon) || p[1].ToVec3().Compare(point, epsilon)) return;
                    // if only two points make sure we have the right ordering according to the normal
                    dir = point - p[0].ToVec3();
                    dir = dir.Cross(p[1].ToVec3() - p[0].ToVec3());
                    // points don't make a plane
                    if (dir[0] == 0f && dir[1] == 0f && dir[2] == 0f) return;
                    if (dir * normal > 0f) p[2].ToVec3() = point;
                    else { p[2] = p[1]; p[1].ToVec3() = point; }
                    numPoints++;
                    return;
            }

            var hullDirs = stackalloc Vector3[numPoints];
            var hullSide = stackalloc bool[numPoints];

            // calculate hull edge vectors
            for (j = 0; j < numPoints; j++) { dir = p[(j + 1) % numPoints].ToVec3() - p[j].ToVec3(); hullDirs[j] = normal.Cross(dir); }

            // calculate side for each hull edge
            outside = false;
            for (j = 0; j < numPoints; j++)
            {
                dir = point - p[j].ToVec3();
                d = dir * hullDirs[j];
                if (d >= epsilon) outside = true;
                hullSide[j] = d >= -epsilon; //: opt
            }

            // if the point is effectively inside, do nothing
            if (!outside) return;

            // find the back side to front side transition
            for (j = 0; j < numPoints; j++) if (!hullSide[j] && hullSide[(j + 1) % numPoints]) break;
            if (j >= numPoints) return;

            var hullPoints = stackalloc Vector5[numPoints + 1];

            // insert the point here
            hullPoints[0] = reinterpret.cast_vec5(point);
            numHullPoints = 1;

            // copy over all points that aren't double fronts
            j = (j + 1) % numPoints;
            for (k = 0; k < numPoints; k++)
            {
                if (hullSide[(j + k) % numPoints] && hullSide[(j + k + 1) % numPoints]) continue;
                hullPoints[numHullPoints] = p[(j + k + 1) % numPoints];
                numHullPoints++;
            }

            if (!EnsureAlloced(numHullPoints, false)) return;
            numPoints = numHullPoints;
            fixed (Vector5* p = this.p) Unsafe.CopyBlock(p, hullPoints, (uint)(numHullPoints * sizeof(Vector5)));
        }

        // tries to merge 'this' with the given winding, returns null if merge fails, both 'this' and 'w' stay intact 'keep' tells if the contacting points should stay even if they create colinear edges
        const float CONTINUOUS_EPSILON = 0.005f;
        public unsafe Winding TryMerge(in Winding w, in Vector3 normal, bool keep = false)
        {
            int i, j, k, l; float dot; bool keep1, keep2;
            Vector3 normal2, delta, p1 = default, p2 = default, p3, p4, back;
            Winding f1, f2, newf;

            f1 = new(this);
            f2 = new(w);
            j = 0;

            for (i = 0; i < f1.numPoints; i++)
            {
                p1 = f1.p[i].ToVec3();
                p2 = f1.p[(i + 1) % f1.numPoints].ToVec3();
                for (j = 0; j < f2.numPoints; j++)
                {
                    p3 = f2.p[j].ToVec3();
                    p4 = f2.p[(j + 1) % f2.numPoints].ToVec3();
                    for (k = 0; k < 3; k++)
                    {
                        if (MathX.Fabs(p1[k] - p4[k]) > 0.1f) break;
                        if (MathX.Fabs(p2[k] - p3[k]) > 0.1f) break;
                    }
                    if (k == 3) break;
                }
                if (j < f2.numPoints) break;
            }

            if (i == f1.numPoints) return null;            // no matching edges

            // check slope of connected lines. if the slopes are colinear, the point can be removed
            back = f1.p[(i + f1.numPoints - 1) % f1.numPoints].ToVec3();
            delta = p1 - back;
            normal2 = normal.Cross(delta);
            normal2.Normalize();

            back = f2.p[(j + 2) % f2.numPoints].ToVec3();
            delta = back - p1;
            dot = delta * normal2;
            if (dot > CONTINUOUS_EPSILON) return null;            // not a convex polygon

            keep1 = dot < -CONTINUOUS_EPSILON;

            back = f1.p[(i + 2) % f1.numPoints].ToVec3();
            delta = back - p2;
            normal2 = normal.Cross(delta);
            normal2.Normalize();

            back = f2.p[(j + f2.numPoints - 1) % f2.numPoints].ToVec3();
            delta = back - p2;
            dot = delta * normal2;
            if (dot > CONTINUOUS_EPSILON) return null;            // not a convex polygon

            keep2 = dot < -CONTINUOUS_EPSILON;

            // build the new polygon
            newf = new Winding(f1.numPoints + f2.numPoints);

            // copy first polygon
            for (k = (i + 1) % f1.numPoints; k != i; k = (k + 1) % f1.numPoints)
            {
                if (!keep && k == (i + 1) % f1.numPoints && !keep2) continue;

                newf.p[newf.numPoints] = f1.p[k];
                newf.numPoints++;
            }

            // copy second polygon
            for (l = (j + 1) % f2.numPoints; l != j; l = (l + 1) % f2.numPoints)
            {
                if (!keep && l == (j + 1) % f2.numPoints && !keep1) continue;
                newf.p[newf.numPoints] = f2.p[l];
                newf.numPoints++;
            }

            return newf;
        }

        // check whether the winding is valid or not
        public bool Check(bool print = true)
        {
            int i, j; float area, d, edgedist;
            Vector3 dir, edgenormal; Plane plane;

            if (numPoints < 3) { if (print) Printf($"Winding::Check: only {numPoints} points."); return false; }

            area = Area;
            if (area < 1f) { if (print) Printf($"Winding::Check: tiny area: {area}"); return false; }

            GetPlane(out plane);

            for (i = 0; i < numPoints; i++)
            {
                var p1 = p[i].ToVec3();

                // check if the winding is huge
                for (j = 0; j < 3; j++) if (p1[j] >= MAX_WORLD_COORD || p1[j] <= MIN_WORLD_COORD) { if (print) Printf($"Winding::Check: point {i} outside world {'X' + j}-axis: {p1[j]}"); return false; }

                j = i + 1 == numPoints ? 0 : i + 1;

                // check if the point is on the face plane
                d = p1 * plane.Normal + plane[3];
                if (d < -ON_EPSILON || d > ON_EPSILON) { if (print) Printf($"Winding::Check: point {i} off plane."); return false; }

                // check if the edge isn't degenerate
                var p2 = p[j].ToVec3();
                dir = p2 - p1;

                if (dir.Length < ON_EPSILON) { if (print) Printf($"Winding::Check: edge {i} is degenerate."); return false; }

                // check if the winding is convex
                edgenormal = plane.Normal.Cross(dir);
                edgenormal.Normalize();
                edgedist = p1 * edgenormal;
                edgedist += ON_EPSILON;

                // all other points must be on front side
                for (j = 0; j < numPoints; j++)
                {
                    if (j == i) continue;
                    d = p[j].ToVec3() * edgenormal;
                    if (d > edgedist) { if (print) Printf("Winding::Check: non-convex."); return false; }
                }
            }
            return true;
        }

        public float Area
        {
            get
            {
                var total = 0f;
                for (var i = 2; i < numPoints; i++)
                {
                    var d1 = p[i - 1].ToVec3() - p[0].ToVec3();
                    var d2 = p[i].ToVec3() - p[0].ToVec3();
                    var cross = d1.Cross(d2);
                    total += cross.Length;
                }
                return total * 0.5f;
            }
        }

        public Vector3 Center
        {
            get
            {
                Vector3 center = new();
                center.Zero();
                for (var i = 0; i < numPoints; i++) center += p[i].ToVec3();
                center *= (1f / numPoints);
                return center;
            }
        }

        public float GetRadius(Vector3 center)
        {
            var radius = 0f;
            for (var i = 0; i < numPoints; i++)
            {
                var dir = p[i].ToVec3() - center;
                var r = dir * dir;
                if (r > radius) radius = r;
            }
            return MathX.Sqrt(radius);
        }

        public void GetPlane(out Vector3 normal, out float dist)
        {
            normal = new();
            if (numPoints < 3) { normal.Zero(); dist = 0f; return; }

            var center = Center;
            var v1 = p[0].ToVec3() - center;
            var v2 = p[1].ToVec3() - center;
            normal = v2.Cross(v1);
            normal.Normalize();
            dist = p[0].ToVec3() * normal;
        }

        public void GetPlane(out Plane plane)
        {
            plane = new();
            if (numPoints < 3) { plane.Zero(); return; }

            var center = Center;
            var v1 = p[0].ToVec3() - center;
            var v2 = p[1].ToVec3() - center;
            plane.SetNormal(v2.Cross(v1));
            plane.Normalize();
            plane.FitThroughPoint(p[0].ToVec3());
        }

        public void GetBounds(out Bounds bounds)
        {
            bounds = new();
            if (numPoints == 0) { bounds.Clear(); return; }

            bounds[0] = bounds[1] = p[0].ToVec3();
            for (var i = 1; i < numPoints; i++)
            {
                if (p[i].x < bounds[0].x) bounds[0].x = p[i].x;
                else if (p[i].x > bounds[1].x) bounds[1].x = p[i].x;
                if (p[i].y < bounds[0].y) bounds[0].y = p[i].y;
                else if (p[i].y > bounds[1].y) bounds[1].y = p[i].y;
                if (p[i].z < bounds[0].z) bounds[0].z = p[i].z;
                else if (p[i].z > bounds[1].z) bounds[1].z = p[i].z;
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
                    var delta = p[(i + 1) % numPoints].ToVec3() - p[i].ToVec3();
                    var len = delta.Length;
                    if (len > EDGE_LENGTH) if (++edges == 3) return false;
                }
                return true;
            }
        }
        public bool IsHuge    // base winding for a plane is typically huge
        {
            get
            {
                for (var i = 0; i < numPoints; i++) for (var j = 0; j < 3; j++) if (p[i][j] <= MIN_WORLD_COORD || p[i][j] >= MAX_WORLD_COORD) return true;
                return false;
            }
        }
        public void Print()
        {
            for (var i = 0; i < numPoints; i++) Printf($"({p[i].x:5.1}, {p[i].y:5.1}, {p[i].z:5.1})\n");
        }

        public float PlaneDistance(in Plane plane)
        {
            var min = MathX.INFINITY;
            var max = -min;
            for (var i = 0; i < numPoints; i++)
            {
                var d = plane.Distance(p[i].ToVec3());
                if (d < min) { min = d; if (MathX.FLOATSIGNBITSET(min) & MathX.FLOATSIGNBITNOTSET(max)) return 0f; }
                if (d > max) { max = d; if (MathX.FLOATSIGNBITSET(min) & MathX.FLOATSIGNBITNOTSET(max)) return 0f; }
            }
            if (MathX.FLOATSIGNBITNOTSET(min)) return min;
            if (MathX.FLOATSIGNBITSET(max)) return max;
            return 0f;
        }

        public int PlaneSide(in Plane plane, float epsilon = Plane.ON_EPSILON)
        {
            var front = false;
            var back = false;
            for (var i = 0; i < numPoints; i++)
            {
                var d = plane.Distance(p[i].ToVec3());
                if (d < -epsilon) { if (front) return SIDE_CROSS; back = true; continue; }
                else if (d > epsilon) { if (back) return SIDE_CROSS; front = true; continue; }
            }
            if (back) return SIDE_BACK;
            if (front) return SIDE_FRONT;
            return SIDE_ON;
        }

        const float WCONVEX_EPSILON = 0.2f;
        public bool PlanesConcave(in Winding w2, in Vector3 normal1, in Vector3 normal2, float dist1, float dist2)
        {
            int i;

            // check if one of the points of winding 1 is at the back of the plane of winding 2
            for (i = 0; i < numPoints; i++) if (normal2 * p[i].ToVec3() - dist2 > WCONVEX_EPSILON) return true;
            // check if one of the points of winding 2 is at the back of the plane of winding 1
            for (i = 0; i < w2.numPoints; i++) if (normal1 * w2.p[i].ToVec3() - dist1 > WCONVEX_EPSILON) return true;
            return false;
        }

        public bool PointInside(in Vector3 normal, in Vector3 point, float epsilon)
        {
            for (var i = 0; i < numPoints; i++)
            {
                var dir = p[(i + 1) % numPoints].ToVec3() - p[i].ToVec3();
                var pointvec = point - p[i].ToVec3();
                var n = dir.Cross(normal);
                if (pointvec * n < -epsilon) return false;
            }
            return true;
        }

        // returns true if the line or ray intersects the winding
        public bool LineIntersection(in Plane windingPlane, in Vector3 start, in Vector3 end, bool backFaceCull = false)
        {
            var front = windingPlane.Distance(start);
            var back = windingPlane.Distance(end);

            // if both points at the same side of the plane
            if (front < 0f && back < 0f) return false;
            if (front > 0f && back > 0f) return false;
            // if back face culled
            if (backFaceCull && front < 0f) return false;

            // get point of intersection with winding plane
            Vector3 mid;
            if (MathX.Fabs(front - back) < 0.0001f) mid = end;
            else
            {
                var frac = front / (front - back);
                mid.x = start.x + (end.x - start.x) * frac;
                mid.y = start.y + (end.y - start.y) * frac;
                mid.z = start.z + (end.z - start.z) * frac;
            }

            return PointInside(windingPlane.Normal, mid, 0f);
        }

        // intersection point is start + dir * scale
        public bool RayIntersection(in Plane windingPlane, in Vector3 start, in Vector3 dir, out float scale, bool backFaceCull = false)
        {
            int i; bool side, lastside = false; Pluecker pl1 = new(), pl2 = new();

            scale = 0f;
            pl1.FromRay(start, dir);
            for (i = 0; i < numPoints; i++)
            {
                pl2.FromLine(p[i].ToVec3(), p[(i + 1) % numPoints].ToVec3());
                side = pl1.PermutedInnerProduct(pl2) > 0f;
                if (i != 0 && side != lastside) return false;
                lastside = side;
            }
            if (!backFaceCull || lastside) { windingPlane.RayIntersection(start, dir, out scale); return true; }
            return false;
        }

        public static float TriangleArea(in Vector3 a, in Vector3 b, in Vector3 c)
        {
            var v1 = b - a;
            var v2 = c - a;
            var cross = v1.Cross(v2);
            return 0.5f * cross.Length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected bool EnsureAlloced(int n, bool keep = false)
            => n <= allocedSize || ReAllocate(n, keep); //: opt

        protected unsafe virtual bool ReAllocate(int n, bool keep = false)
        {
            var oldP = p;
            n = (n + 3) & ~3;   // align up to multiple of four
            p = new Vector5[n];
            if (oldP != null && keep) fixed (Vector5* p = this.p, oldP_ = oldP) Unsafe.CopyBlock(p, oldP_, (uint)(numPoints * sizeof(Vector5)));
            allocedSize = n;
            return true;
        }
    }

    public class FixedWinding : Winding
    {
        internal const int MAX_POINTS_ON_WINDING = 64;

        protected Vector5[] data = new Vector5[MAX_POINTS_ON_WINDING]; // point data

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FixedWinding()
        {
            numPoints = 0;
            p = data;
            allocedSize = MAX_POINTS_ON_WINDING;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FixedWinding(int n)
        {
            numPoints = 0;
            p = data;
            allocedSize = MAX_POINTS_ON_WINDING;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FixedWinding(Vector3[] verts, int n)
        {
            numPoints = 0;
            p = data;
            allocedSize = MAX_POINTS_ON_WINDING;
            if (!EnsureAlloced(n)) { numPoints = 0; return; }
            for (var i = 0; i < n; i++) { p[i].ToVec3() = verts[i]; p[i].s = p[i].t = 0; }
            numPoints = n;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FixedWinding(in Vector3 normal, float dist)
        {
            numPoints = 0;
            p = data;
            allocedSize = MAX_POINTS_ON_WINDING;
            BaseForPlane(normal, dist);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FixedWinding(in Plane plane)
        {
            numPoints = 0;
            p = data;
            allocedSize = MAX_POINTS_ON_WINDING;
            BaseForPlane(plane);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FixedWinding(in Winding winding)
        {
            p = data;
            allocedSize = MAX_POINTS_ON_WINDING;
            if (!EnsureAlloced(winding.NumPoints)) { numPoints = 0; return; }
            for (var i = 0; i < winding.NumPoints; i++) p[i] = winding[i];
            numPoints = winding.NumPoints;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FixedWinding(in FixedWinding winding)
        {
            p = data;
            allocedSize = MAX_POINTS_ON_WINDING;
            if (!EnsureAlloced(winding.NumPoints)) { numPoints = 0; return; }
            for (var i = 0; i < winding.NumPoints; i++) p[i] = winding[i];
            numPoints = winding.NumPoints;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Clear()
            => numPoints = 0;

        // splits the winding in a back and front part, 'this' becomes the front part, returns a SIDE_?
        public unsafe int Split(in FixedWinding back, in Plane plane, float epsilon = Plane.ON_EPSILON)
        {
            int i, j; float dot;
            Vector5* p1, p2; Vector5 mid = new();
            FixedWinding winding = new();

            var counts = stackalloc int[3];
            var dists = stackalloc float[MAX_POINTS_ON_WINDING + 4];
            var sides = stackalloc byte[MAX_POINTS_ON_WINDING + 4];

            counts[SIDE_FRONT] = counts[SIDE_BACK] = counts[SIDE_ON] = 0;

            // determine sides for each point
            for (i = 0; i < numPoints; i++)
            {
                dists[i] = dot = plane.Distance(p[i].ToVec3());
                sides[i] = (byte)(dot > epsilon ? SIDE_FRONT : dot < -epsilon ? SIDE_BACK : SIDE_ON); //: opt
                counts[sides[i]]++;
            }

            if (counts[SIDE_BACK] == 0) return counts[SIDE_FRONT] == 0 ? SIDE_ON : SIDE_FRONT;
            if (counts[SIDE_FRONT] == 0) return SIDE_BACK;

            sides[i] = sides[0];
            dists[i] = dists[0];

            winding.numPoints = 0;
            back.numPoints = 0;

            fixed (Vector5* p = this.p)
            {
                for (i = 0; i < numPoints; i++)
                {
                    p1 = &p[i];

                    if (!winding.EnsureAlloced(winding.numPoints + 1, true)) return SIDE_FRONT;      // can't split -- fall back to original
                    if (!back.EnsureAlloced(back.numPoints + 1, true)) return SIDE_FRONT;      // can't split -- fall back to original

                    if (sides[i] == SIDE_ON)
                    {
                        winding.p[winding.numPoints] = *p1;
                        winding.numPoints++;
                        back.p[back.numPoints] = *p1;
                        back.numPoints++;
                        continue;
                    }

                    if (sides[i] == SIDE_FRONT) { winding.p[winding.numPoints] = *p1; winding.numPoints++; }
                    if (sides[i] == SIDE_BACK) { back.p[back.numPoints] = *p1; back.numPoints++; }

                    if (sides[i + 1] == SIDE_ON || sides[i + 1] == sides[i]) continue;
                    if (!winding.EnsureAlloced(winding.numPoints + 1, true)) return SIDE_FRONT;      // can't split -- fall back to original
                    if (!back.EnsureAlloced(back.numPoints + 1, true)) return SIDE_FRONT;      // can't split -- fall back to original

                    // generate a split point
                    j = i + 1;
                    p2 = &p[j >= numPoints ? 0 : j]; //: opt

                    dot = dists[i] / (dists[i] - dists[i + 1]);
                    for (j = 0; j < 3; j++)
                        // avoid round off error when possible
                        mid[j] = plane.Normal[j] == 1f ? plane.Dist
                            : plane.Normal[j] == -1f ? -plane.Dist
                            : (*p1)[j] + dot * ((*p2)[j] - (*p1)[j]);
                    mid.s = p1->s + dot * (p2->s - p1->s);
                    mid.t = p1->t + dot * (p2->t - p1->t);

                    winding.p[winding.numPoints] = mid;
                    winding.numPoints++;
                    back.p[back.numPoints] = mid;
                    back.numPoints++;
                }
                for (i = 0; i < winding.numPoints; i++) p[i] = winding.p[i];
                numPoints = winding.numPoints;
            }

            return SIDE_CROSS;
        }

        protected override bool ReAllocate(int n, bool keep = false)
        {
            Debug.Assert(n <= MAX_POINTS_ON_WINDING);

            if (n > MAX_POINTS_ON_WINDING) { Printf("WARNING: FixedWinding . MAX_POINTS_ON_WINDING overflowed\n"); return false; }
            return true;
        }
    }
}