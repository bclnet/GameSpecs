#define TEST_TRACE
using System.Diagnostics;
using static System.NumericsX.OpenStack.OpenStack;
using static System.NumericsX.OpenStack.Gngine.Render.R;

namespace System.NumericsX.OpenStack.Gngine.Render
{
    partial class TR
    {
        public struct LocalTrace
        {
            public float fraction;
            // only valid if fraction < 1.0
            public Vector3 point;
            public Vector3 normal;
            public int[] indexes;
        }

        // If we resort the vertexes so all silverts come first, we can save some work here.
        public unsafe static LocalTrace R_LocalTrace(Vector3 start, Vector3 end, float radius, SrfTriangles tri)
        {
            int i, j;
            Plane* planes = stackalloc Plane[4];
            LocalTrace hit = new();
            int c_testEdges, c_testPlanes, c_intersect;
            Vector3 startDir;
            byte totalOr;
            float radiusSqr;
            var tri_verts = tri.verts.Value; var tri_indexes = tri.indexes.Value; var tri_facePlanes = tri.facePlanes.Value;

#if TEST_TRACE
            Stopwatch trace_timer = new();
            trace_timer.Start();
#endif

            hit.fraction = 1f;

            // create two planes orthogonal to each other that intersect along the trace
            startDir = end - start;
            startDir.Normalize();
            startDir.NormalVectors(out planes[0].Normal, out planes[1].Normal);
            planes[0].d = -start * planes[0].Normal;
            planes[1].d = -start * planes[1].Normal;

            // create front and end planes so the trace is on the positive sides of both
            planes[2] = startDir; planes[2].d = -start * planes[2].Normal;
            planes[3] = -startDir; planes[3].d = -end * planes[3].Normal;

            // catagorize each point against the four planes
            var cullBits = stackalloc byte[tri.numVerts];
            fixed(DrawVert* vertsD = tri_verts) Simd.TracePointCull(cullBits, out totalOr, radius, planes, vertsD, tri.numVerts);

            // if we don't have points on both sides of both the ray planes, no intersection
            if (((totalOr ^ (totalOr >> 4)) & 3) != 0) { /*common.Printf("nothing crossed the trace planes\n");*/ return hit; }

            // if we don't have any points between front and end, no intersection
            if (((totalOr ^ (totalOr >> 1)) & 4) != 0) { /*common.Printf("trace didn't reach any triangles\n");*/ return hit; }

            // scan for triangles that cross both planes
            c_testPlanes = c_testEdges = c_intersect = 0;

            radiusSqr = MathX.Square(radius);
            startDir = end - start;

            if (tri.facePlanes == null || !tri.facePlanesCalculated) R_DeriveFacePlanes(tri);

            for (i = 0, j = 0; i < tri.numIndexes; i += 3, j++)
            {
                float d1, d2, f, d, edgeLengthSqr; byte triOr;
                Vector3 cross, edge; Vector3[] dir = new Vector3[3];

                // get sidedness info for the triangle
                triOr = cullBits[tri_indexes[i + 0]];
                triOr |= cullBits[tri_indexes[i + 1]];
                triOr |= cullBits[tri_indexes[i + 2]];

                // if we don't have points on both sides of both the ray planes, no intersection
                if (((triOr ^ (triOr >> 4)) & 3) != 0) continue;

                // if we don't have any points between front and end, no intersection
                if (((triOr ^ (triOr >> 1)) & 4) != 0) continue;

                c_testPlanes++;

                ref Plane plane = ref tri_facePlanes[j];
                d1 = plane.Distance(start);
                d2 = plane.Distance(end);

                if (d1 <= d2) continue;       // comning at it from behind or parallel
                if (d1 < 0f) continue;       // starts past it
                if (d2 > 0f) continue;       // finishes in front of it
                f = d1 / (d1 - d2);
                if (f < 0f) continue;       // shouldn't happen
                if (f >= hit.fraction) continue;       // have already hit something closer

                c_testEdges++;

                // find the exact point of impact with the plane
                var point = start + f * startDir;

                // see if the point is within the three edges if radius > 0 the triangle is expanded with a circle in the triangle plane

                dir[0] = tri_verts[tri_indexes[i + 0]].xyz - point;
                dir[1] = tri_verts[tri_indexes[i + 1]].xyz - point;

                cross = dir[0].Cross(dir[1]);
                d = plane.Normal * cross;
                if (d > 0f)
                {
                    if (radiusSqr <= 0f) continue;
                    edge = tri_verts[tri_indexes[i + 0]].xyz - tri_verts[tri_indexes[i + 1]].xyz;
                    edgeLengthSqr = edge.LengthSqr;
                    if (cross.LengthSqr > edgeLengthSqr * radiusSqr) continue;
                    d = edge * dir[0];
                    if (d < 0f)
                    {
                        edge = tri_verts[tri_indexes[i + 0]].xyz - tri_verts[tri_indexes[i + 2]].xyz;
                        d = edge * dir[0];
                        if (d < 0f && dir[0].LengthSqr > radiusSqr) continue;
                    }
                    else if (d > edgeLengthSqr)
                    {
                        edge = tri_verts[tri_indexes[i + 1]].xyz - tri_verts[tri_indexes[i + 2]].xyz;
                        d = edge * dir[1];
                        if (d < 0f && dir[1].LengthSqr > radiusSqr) continue;
                    }
                }

                dir[2] = tri_verts[tri_indexes[i + 2]].xyz - point;

                cross = dir[1].Cross(dir[2]);
                d = plane.Normal * cross;
                if (d > 0f)
                {
                    if (radiusSqr <= 0f) continue;
                    edge = tri_verts[tri_indexes[i + 1]].xyz - tri_verts[tri_indexes[i + 2]].xyz;
                    edgeLengthSqr = edge.LengthSqr;
                    if (cross.LengthSqr > edgeLengthSqr * radiusSqr) continue;
                    d = edge * dir[1];
                    if (d < 0f)
                    {
                        edge = tri_verts[tri_indexes[i + 1]].xyz - tri_verts[tri_indexes[i + 0]].xyz;
                        d = edge * dir[1];
                        if (d < 0f && dir[1].LengthSqr > radiusSqr) continue;
                    }
                    else if (d > edgeLengthSqr)
                    {
                        edge = tri_verts[tri_indexes[i + 2]].xyz - tri_verts[tri_indexes[i + 0]].xyz;
                        d = edge * dir[2];
                        if (d < 0f && dir[2].LengthSqr > radiusSqr) continue;
                    }
                }

                cross = dir[2].Cross(dir[0]);
                d = plane.Normal * cross;
                if (d > 0f)
                {
                    if (radiusSqr <= 0f) continue;
                    edge = tri_verts[tri_indexes[i + 2]].xyz - tri_verts[tri_indexes[i + 0]].xyz;
                    edgeLengthSqr = edge.LengthSqr;
                    if (cross.LengthSqr > edgeLengthSqr * radiusSqr) continue;
                    d = edge * dir[2];
                    if (d < 0f)
                    {
                        edge = tri_verts[tri_indexes[i + 2]].xyz - tri_verts[tri_indexes[i + 1]].xyz;
                        d = edge * dir[2];
                        if (d < 0f && dir[2].LengthSqr > radiusSqr) continue;
                    }
                    else if (d > edgeLengthSqr)
                    {
                        edge = tri_verts[tri_indexes[i + 0]].xyz - tri_verts[tri_indexes[i + 1]].xyz;
                        d = edge * dir[0];
                        if (d < 0f && dir[0].LengthSqr > radiusSqr) continue;
                    }
                }

                // we hit it
                c_intersect++;

                hit.fraction = f;
                hit.normal = plane.Normal;
                hit.point = point;
                hit.indexes[0] = tri_indexes[i];
                hit.indexes[1] = tri_indexes[i + 1];
                hit.indexes[2] = tri_indexes[i + 2];
            }

#if TEST_TRACE
            trace_timer.Stop();
            common.Printf($"testVerts:{tri.numVerts} c_testPlanes:{c_testPlanes} c_testEdges:{c_testEdges} c_intersect:{c_intersect} msec:{trace_timer.ElapsedMilliseconds}\n");
#endif

            return hit;
        }
    }
}

