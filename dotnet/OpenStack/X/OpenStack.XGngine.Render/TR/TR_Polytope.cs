using static System.NumericsX.OpenStack.Gngine.Render.R;
using static System.NumericsX.OpenStack.OpenStack;

namespace System.NumericsX.OpenStack.Gngine.Render
{
    unsafe partial class TR
    {
        const int MAX_POLYTOPE_PLANES = 6;

        // Generate vertexes and indexes for a polytope, and optionally returns the polygon windings. The positive sides of the planes will be visible.
        public static SrfTriangles R_PolytopeSurface(int numPlanes, Plane[] planes, Winding[] windings)
        {
            int i, j;
            SrfTriangles tri;
            var planeWindings = new FixedWinding[MAX_POLYTOPE_PLANES];
            int numVerts, numIndexes;

            if (numPlanes > MAX_POLYTOPE_PLANES) common.Error($"R_PolytopeSurface: more than {MAX_POLYTOPE_PLANES} planes");

            numVerts = 0;
            numIndexes = 0;
            for (i = 0; i < numPlanes; i++)
            {
                ref Plane plane = ref planes[i];
                ref FixedWinding w = ref planeWindings[i];

                w.BaseForPlane(plane);
                for (j = 0; j < numPlanes; j++)
                {
                    ref Plane plane2 = ref planes[j];
                    if (j == i) continue;
                    if (!w.ClipInPlace(-plane2, Plane.ON_EPSILON)) break;
                }
                if (w.NumPoints <= 2) continue;
                numVerts += w.NumPoints;
                numIndexes += (w.NumPoints - 2) * 3;
            }

            // allocate the surface
            tri = R_AllocStaticTriSurf();
            R_AllocStaticTriSurfVerts(tri, numVerts);
            R_AllocStaticTriSurfIndexes(tri, numIndexes);
            var tri_verts = tri.verts.Value; var tri_indexes = tri.indexes.Value;

            // copy the data from the windings
            for (i = 0; i < numPlanes; i++)
            {
                ref FixedWinding w = ref planeWindings[i];
                if (w.NumPoints == 0) continue;
                for (j = 0; j < w.NumPoints; j++)
                {
                    tri_verts[tri.numVerts + j].Clear();
                    tri_verts[tri.numVerts + j].xyz = w[j].ToVec3();
                }
                for (j = 1; j < w.NumPoints - 1; j++)
                {
                    tri_indexes[tri.numIndexes + 0] = tri.numVerts;
                    tri_indexes[tri.numIndexes + 1] = tri.numVerts + j;
                    tri_indexes[tri.numIndexes + 2] = tri.numVerts + j + 1;
                    tri.numIndexes += 3;
                }
                tri.numVerts += w.NumPoints;

                // optionally save the winding
                if (windings != null) windings[i] = w; //windings[i] = new Winding(w.NumPoints);
            }

            R_BoundTriSurf(tri);

            return tri;
        }
    }
}
