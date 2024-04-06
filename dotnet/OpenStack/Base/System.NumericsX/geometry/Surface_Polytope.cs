using static System.NumericsX.Plane;
using static System.NumericsX.Platform;

namespace System.NumericsX
{
    public class Surface_Polytope : Surface
    {
        const float POLYTOPE_VERTEX_EPSILON = 0.1f;

        public unsafe void FromPlanes(Plane[] planes, int numPlanes)
        {
            int i, j, k; FixedWinding w = new(); DrawVert newVert = new();

            var windingVerts = stackalloc int[FixedWinding.MAX_POINTS_ON_WINDING];

            for (i = 0; i < numPlanes; i++)
            {
                w.BaseForPlane(planes[i]);

                for (j = 0; j < numPlanes; j++)
                {
                    if (j == i) continue;
                    if (!w.ClipInPlace(-planes[j], Plane.ON_EPSILON, true)) break;
                }
                if (w.NumPoints == 0) continue;

                for (j = 0; j < w.NumPoints; j++)
                {
                    for (k = 0; k < verts.Count; k++) if (verts[k].xyz.Compare(w[j].ToVec3(), POLYTOPE_VERTEX_EPSILON)) break;
                    if (k >= verts.Count) { newVert.xyz = w[j].ToVec3(); k = verts.Add_(newVert); }
                    windingVerts[j] = k;
                }

                for (j = 2; j < w.NumPoints; j++)
                {
                    indexes.Add(windingVerts[0]);
                    indexes.Add(windingVerts[j - 1]);
                    indexes.Add(windingVerts[j]);
                }
            }

            GenerateEdgeIndexes();
        }

        public void SetupTetrahedron(in Bounds bounds)
        {
            Vector3 center, scale;
            float c1, c2, c3;

            c1 = 0.4714045207f;
            c2 = 0.8164965809f;
            c3 = -0.3333333333f;

            center = bounds.Center;
            scale = bounds[1] - center;

            var verts_ = verts.SetNum(4);
            verts_[0].xyz = center + new Vector3(0.0f, 0.0f, scale.z);
            verts_[1].xyz = center + new Vector3(2.0f * c1 * scale.x, 0.0f, c3 * scale.z);
            verts_[2].xyz = center + new Vector3(-c1 * scale.x, c2 * scale.y, c3 * scale.z);
            verts_[3].xyz = center + new Vector3(-c1 * scale.x, -c2 * scale.y, c3 * scale.z);

            indexes.SetNum(4 * 3);
            indexes[0 * 3 + 0] = 0;
            indexes[0 * 3 + 1] = 1;
            indexes[0 * 3 + 2] = 2;
            indexes[1 * 3 + 0] = 0;
            indexes[1 * 3 + 1] = 2;
            indexes[1 * 3 + 2] = 3;
            indexes[2 * 3 + 0] = 0;
            indexes[2 * 3 + 1] = 3;
            indexes[2 * 3 + 2] = 1;
            indexes[3 * 3 + 0] = 1;
            indexes[3 * 3 + 1] = 3;
            indexes[3 * 3 + 2] = 2;

            GenerateEdgeIndexes();
        }

        public void SetupHexahedron(in Bounds bounds)
        {
            Vector3 center, scale;

            center = bounds.Center;
            scale = bounds[1] - center;

            var verts_ = verts.SetNum(8);
            verts_[0].xyz = center + new Vector3(-scale.x, -scale.y, -scale.z);
            verts_[1].xyz = center + new Vector3(scale.x, -scale.y, -scale.z);
            verts_[2].xyz = center + new Vector3(scale.x, scale.y, -scale.z);
            verts_[3].xyz = center + new Vector3(-scale.x, scale.y, -scale.z);
            verts_[4].xyz = center + new Vector3(-scale.x, -scale.y, scale.z);
            verts_[5].xyz = center + new Vector3(scale.x, -scale.y, scale.z);
            verts_[6].xyz = center + new Vector3(scale.x, scale.y, scale.z);
            verts_[7].xyz = center + new Vector3(-scale.x, scale.y, scale.z);

            indexes.SetNum(12 * 3);
            indexes[0 * 3 + 0] = 0;
            indexes[0 * 3 + 1] = 3;
            indexes[0 * 3 + 2] = 2;
            indexes[1 * 3 + 0] = 0;
            indexes[1 * 3 + 1] = 2;
            indexes[1 * 3 + 2] = 1;
            indexes[2 * 3 + 0] = 0;
            indexes[2 * 3 + 1] = 1;
            indexes[2 * 3 + 2] = 5;
            indexes[3 * 3 + 0] = 0;
            indexes[3 * 3 + 1] = 5;
            indexes[3 * 3 + 2] = 4;
            indexes[4 * 3 + 0] = 0;
            indexes[4 * 3 + 1] = 4;
            indexes[4 * 3 + 2] = 7;
            indexes[5 * 3 + 0] = 0;
            indexes[5 * 3 + 1] = 7;
            indexes[5 * 3 + 2] = 3;
            indexes[6 * 3 + 0] = 6;
            indexes[6 * 3 + 1] = 5;
            indexes[6 * 3 + 2] = 1;
            indexes[7 * 3 + 0] = 6;
            indexes[7 * 3 + 1] = 1;
            indexes[7 * 3 + 2] = 2;
            indexes[8 * 3 + 0] = 6;
            indexes[8 * 3 + 1] = 2;
            indexes[8 * 3 + 2] = 3;
            indexes[9 * 3 + 0] = 6;
            indexes[9 * 3 + 1] = 3;
            indexes[9 * 3 + 2] = 7;
            indexes[10 * 3 + 0] = 6;
            indexes[10 * 3 + 1] = 7;
            indexes[10 * 3 + 2] = 4;
            indexes[11 * 3 + 0] = 6;
            indexes[11 * 3 + 1] = 4;
            indexes[11 * 3 + 2] = 5;

            GenerateEdgeIndexes();
        }

        public void SetupOctahedron(in Bounds bounds)
        {
            Vector3 center, scale;

            center = bounds.Center;
            scale = bounds[1] - center;

            var verts_ = verts.SetNum(6);
            verts_[0].xyz = center + new Vector3(scale.x, 0.0f, 0.0f);
            verts_[1].xyz = center + new Vector3(-scale.x, 0.0f, 0.0f);
            verts_[2].xyz = center + new Vector3(0.0f, scale.y, 0.0f);
            verts_[3].xyz = center + new Vector3(0.0f, -scale.y, 0.0f);
            verts_[4].xyz = center + new Vector3(0.0f, 0.0f, scale.z);
            verts_[5].xyz = center + new Vector3(0.0f, 0.0f, -scale.z);

            indexes.SetNum(8 * 3);
            indexes[0 * 3 + 0] = 4;
            indexes[0 * 3 + 1] = 0;
            indexes[0 * 3 + 2] = 2;
            indexes[1 * 3 + 0] = 4;
            indexes[1 * 3 + 1] = 2;
            indexes[1 * 3 + 2] = 1;
            indexes[2 * 3 + 0] = 4;
            indexes[2 * 3 + 1] = 1;
            indexes[2 * 3 + 2] = 3;
            indexes[3 * 3 + 0] = 4;
            indexes[3 * 3 + 1] = 3;
            indexes[3 * 3 + 2] = 0;
            indexes[4 * 3 + 0] = 5;
            indexes[4 * 3 + 1] = 2;
            indexes[4 * 3 + 2] = 0;
            indexes[5 * 3 + 0] = 5;
            indexes[5 * 3 + 1] = 1;
            indexes[5 * 3 + 2] = 2;
            indexes[6 * 3 + 0] = 5;
            indexes[6 * 3 + 1] = 3;
            indexes[6 * 3 + 2] = 1;
            indexes[7 * 3 + 0] = 5;
            indexes[7 * 3 + 1] = 0;
            indexes[7 * 3 + 2] = 3;

            GenerateEdgeIndexes();
        }

        public void SetupDodecahedron(in Bounds bounds)
            => throw new NotSupportedException();

        public void SetupIcosahedron(in Bounds bounds)
            => throw new NotSupportedException();

        public void SetupCylinder(in Bounds bounds, int numSides)
            => throw new NotSupportedException();

        public void SetupCone(in Bounds bounds, int numSides)
            => throw new NotSupportedException();

        public unsafe int SplitPolytope(in Plane plane, float epsilon, out Surface_Polytope front, out Surface_Polytope back)
        {
            int side, i, j, s, v0, v1, v2, edgeNum;
            var surface = new Surface[2];
            var polytopeSurfaces = new Surface_Polytope[2]; Surface_Polytope surf;
            int[][] onPlaneEdges = new int[2][];

            onPlaneEdges[0] = new int[indexes.Count / 3];
            onPlaneEdges[1] = new int[indexes.Count / 3];

            side = Split(plane, epsilon, out surface[0], out surface[1], onPlaneEdges[0], onPlaneEdges[1]);

            front = polytopeSurfaces[0] = new Surface_Polytope();
            back = polytopeSurfaces[1] = new Surface_Polytope();

            for (s = 0; s < 2; s++)
                if (surface[s] != null)
                {
                    polytopeSurfaces[s] = new Surface_Polytope();
                    polytopeSurfaces[s].SwapTriangles(surface[s]);
                    surface[s] = null;
                }

            front = polytopeSurfaces[0];
            back = polytopeSurfaces[1];

            if (side != SIDE_CROSS) return side;

            // add triangles to close off the front and back polytope
            for (s = 0; s < 2; s++)
            {
                surf = polytopeSurfaces[s];

                edgeNum = surf.edgeIndexes[onPlaneEdges[s][0]];
                v0 = surf.edges[Math.Abs(edgeNum)].verts_(MathX.INTSIGNBITSET_(edgeNum));
                v1 = surf.edges[Math.Abs(edgeNum)].verts_(MathX.INTSIGNBITNOTSET_(edgeNum));

                for (i = 1; onPlaneEdges[s][i] >= 0; i++)
                    for (j = i + 1; onPlaneEdges[s][j] >= 0; j++)
                    {
                        edgeNum = surf.edgeIndexes[onPlaneEdges[s][j]];
                        if (v1 == surf.edges[Math.Abs(edgeNum)].verts_(MathX.INTSIGNBITSET_(edgeNum)))
                        {
                            v1 = surf.edges[Math.Abs(edgeNum)].verts_(MathX.INTSIGNBITNOTSET_(edgeNum));
                            Swap(ref onPlaneEdges[s][i], ref onPlaneEdges[s][j]);
                            break;
                        }
                    }

                for (i = 2; onPlaneEdges[s][i] >= 0; i++)
                {
                    edgeNum = surf.edgeIndexes[onPlaneEdges[s][i]];
                    v1 = surf.edges[Math.Abs(edgeNum)].verts_(MathX.INTSIGNBITNOTSET_(edgeNum));
                    v2 = surf.edges[Math.Abs(edgeNum)].verts_(MathX.INTSIGNBITSET_(edgeNum));
                    surf.indexes.Add(v0);
                    surf.indexes.Add(v1);
                    surf.indexes.Add(v2);
                }

                surf.GenerateEdgeIndexes();
            }

            return side;
        }
    }
}