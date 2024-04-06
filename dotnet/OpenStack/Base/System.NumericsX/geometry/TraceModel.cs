using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static System.NumericsX.Platform;
using TraceModelVert = System.NumericsX.Vector3;

namespace System.NumericsX
{
    // trace model type
    public enum TRM
    {
        INVALID,        // invalid trm
        BOX,            // box
        OCTAHEDRON,     // octahedron
        DODECAHEDRON,   // dodecahedron
        CYLINDER,       // cylinder approximation
        CONE,           // cone approximation
        BONE,           // two tetrahedrons attached to each other
        POLYGON,        // arbitrary convex polygon
        POLYGONVOLUME,  // volume for arbitrary convex polygon
        CUSTOM          // loaded from map model or ASE/LWO
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct TraceModelEdge
    {
        public fixed int v[2];
        public Vector3 normal;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct TraceModelPoly
    {
        public Vector3 normal;
        public float dist;
        public Bounds bounds;
        public int numEdges;
        public fixed int edges[TraceModel.MAX_TRACEMODEL_POLYEDGES];
    }

    public class TraceModel
    {
        // these are bit cache limits
        public const int MAX_TRACEMODEL_VERTS = 32;
        public const int MAX_TRACEMODEL_EDGES = 32;
        public const int MAX_TRACEMODEL_POLYS = 16;
        public const int MAX_TRACEMODEL_POLYEDGES = 16;

        public TRM type;
        public int numVerts;
        public TraceModelVert[] verts = new TraceModelVert[MAX_TRACEMODEL_VERTS];
        public int numEdges;
        public TraceModelEdge[] edges = new TraceModelEdge[MAX_TRACEMODEL_EDGES + 1];
        public int numPolys;
        public TraceModelPoly[] polys = new TraceModelPoly[MAX_TRACEMODEL_POLYS];
        public Vector3 offset;          // offset to center of model
        public Bounds bounds;           // bounds of model
        public bool isConvex;       // true when model is convex

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TraceModel()
        {
            type = TRM.INVALID;
            numVerts = numEdges = numPolys = 0;
            bounds.Zero();
        }
        // axial bounding box
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TraceModel(in Bounds boxBounds)
        {
            InitBox();
            SetupBox(boxBounds);
        }
        // cylinder approximation
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TraceModel(in Bounds cylBounds, int numSides)
            => SetupCylinder(cylBounds, numSides);
        // bone
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TraceModel(float length, float width)
        {
            InitBone();
            SetupBone(length, width);
        }

        #region Box: axial box

        // Initialize size independent box.
        unsafe void InitBox()
        {
            int i;

            type = TRM.BOX;
            numVerts = 8;
            numEdges = 12;
            numPolys = 6;

            // set box edges
            for (i = 0; i < 4; i++)
            {
                edges[i + 1].v[0] = i;
                edges[i + 1].v[1] = (i + 1) & 3;
                edges[i + 5].v[0] = 4 + i;
                edges[i + 5].v[1] = 4 + ((i + 1) & 3);
                edges[i + 9].v[0] = i;
                edges[i + 9].v[1] = 4 + i;
            }

            // all edges of a polygon go counter clockwise
            polys[0].numEdges = 4;
            polys[0].edges[0] = -4;
            polys[0].edges[1] = -3;
            polys[0].edges[2] = -2;
            polys[0].edges[3] = -1;
            polys[0].normal.Set(0f, 0f, -1f);

            polys[1].numEdges = 4;
            polys[1].edges[0] = 5;
            polys[1].edges[1] = 6;
            polys[1].edges[2] = 7;
            polys[1].edges[3] = 8;
            polys[1].normal.Set(0f, 0f, 1f);

            polys[2].numEdges = 4;
            polys[2].edges[0] = 1;
            polys[2].edges[1] = 10;
            polys[2].edges[2] = -5;
            polys[2].edges[3] = -9;
            polys[2].normal.Set(0f, -1f, 0f);

            polys[3].numEdges = 4;
            polys[3].edges[0] = 2;
            polys[3].edges[1] = 11;
            polys[3].edges[2] = -6;
            polys[3].edges[3] = -10;
            polys[3].normal.Set(1f, 0f, 0f);

            polys[4].numEdges = 4;
            polys[4].edges[0] = 3;
            polys[4].edges[1] = 12;
            polys[4].edges[2] = -7;
            polys[4].edges[3] = -11;
            polys[4].normal.Set(0f, 1f, 0f);

            polys[5].numEdges = 4;
            polys[5].edges[0] = 4;
            polys[5].edges[1] = 9;
            polys[5].edges[2] = -8;
            polys[5].edges[3] = -12;
            polys[5].normal.Set(-1f, 0f, 0f);

            // convex model
            isConvex = true;

            GenerateEdgeNormals();
        }

        public void SetupBox(in Bounds boxBounds)
        {
            int i;

            if (type != TRM.BOX) InitBox();

            // offset to center
            offset = (boxBounds[0] + boxBounds[1]) * 0.5f;
            // set box vertices
            for (i = 0; i < 8; i++)
            {
                verts[i][0] = boxBounds[(i ^ (i >> 1)) & 1][0];
                verts[i][1] = boxBounds[(i >> 1) & 1][1];
                verts[i][2] = boxBounds[(i >> 2) & 1][2];
            }
            // set polygon plane distances
            polys[0].dist = -boxBounds[0][2];
            polys[1].dist = boxBounds[1][2];
            polys[2].dist = -boxBounds[0][1];
            polys[3].dist = boxBounds[1][0];
            polys[4].dist = boxBounds[1][1];
            polys[5].dist = -boxBounds[0][0];
            // set polygon bounds
            for (i = 0; i < 6; i++) polys[i].bounds = boxBounds;
            polys[0].bounds[1][2] = boxBounds[0][2];
            polys[1].bounds[0][2] = boxBounds[1][2];
            polys[2].bounds[1][1] = boxBounds[0][1];
            polys[3].bounds[0][0] = boxBounds[1][0];
            polys[4].bounds[0][1] = boxBounds[1][1];
            polys[5].bounds[1][0] = boxBounds[0][0];

            bounds = boxBounds;
        }

        // The origin is placed at the center of the cube.
        public void SetupBox(float size)
        {
            var halfSize = size * 0.5f;
            var boxBounds = new Bounds();
            boxBounds[0].Set(-halfSize, -halfSize, -halfSize);
            boxBounds[1].Set(halfSize, halfSize, halfSize);
            SetupBox(boxBounds);
        }

        #endregion

        #region Octahedron: octahedron

        // Initialize size independent octahedron.
        unsafe void InitOctahedron()
        {
            type = TRM.OCTAHEDRON;
            numVerts = 6;
            numEdges = 12;
            numPolys = 8;

            // set edges
            edges[1].v[0] = 4; edges[1].v[1] = 0;
            edges[2].v[0] = 0; edges[2].v[1] = 2;
            edges[3].v[0] = 2; edges[3].v[1] = 4;
            edges[4].v[0] = 2; edges[4].v[1] = 1;
            edges[5].v[0] = 1; edges[5].v[1] = 4;
            edges[6].v[0] = 1; edges[6].v[1] = 3;
            edges[7].v[0] = 3; edges[7].v[1] = 4;
            edges[8].v[0] = 3; edges[8].v[1] = 0;
            edges[9].v[0] = 5; edges[9].v[1] = 2;
            edges[10].v[0] = 0; edges[10].v[1] = 5;
            edges[11].v[0] = 5; edges[11].v[1] = 1;
            edges[12].v[0] = 5; edges[12].v[1] = 3;

            // all edges of a polygon go counter clockwise
            polys[0].numEdges = 3;
            polys[0].edges[0] = 1;
            polys[0].edges[1] = 2;
            polys[0].edges[2] = 3;

            polys[1].numEdges = 3;
            polys[1].edges[0] = -3;
            polys[1].edges[1] = 4;
            polys[1].edges[2] = 5;

            polys[2].numEdges = 3;
            polys[2].edges[0] = -5;
            polys[2].edges[1] = 6;
            polys[2].edges[2] = 7;

            polys[3].numEdges = 3;
            polys[3].edges[0] = -7;
            polys[3].edges[1] = 8;
            polys[3].edges[2] = -1;

            polys[4].numEdges = 3;
            polys[4].edges[0] = 9;
            polys[4].edges[1] = -2;
            polys[4].edges[2] = 10;

            polys[5].numEdges = 3;
            polys[5].edges[0] = 11;
            polys[5].edges[1] = -4;
            polys[5].edges[2] = -9;

            polys[6].numEdges = 3;
            polys[6].edges[0] = 12;
            polys[6].edges[1] = -6;
            polys[6].edges[2] = -11;

            polys[7].numEdges = 3;
            polys[7].edges[0] = -10;
            polys[7].edges[1] = -8;
            polys[7].edges[2] = -12;

            // convex model
            isConvex = true;
        }

        public unsafe void SetupOctahedron(in Bounds octBounds)
        {
            int i, e0, e1, v0, v1, v2; Vector3 v = new();

            if (type != TRM.OCTAHEDRON) InitOctahedron();

            offset = (octBounds[0] + octBounds[1]) * 0.5f;
            v[0] = octBounds[1][0] - offset[0];
            v[1] = octBounds[1][1] - offset[1];
            v[2] = octBounds[1][2] - offset[2];

            // set vertices
            verts[0].Set(offset.x + v[0], offset.y, offset.z);
            verts[1].Set(offset.x - v[0], offset.y, offset.z);
            verts[2].Set(offset.x, offset.y + v[1], offset.z);
            verts[3].Set(offset.x, offset.y - v[1], offset.z);
            verts[4].Set(offset.x, offset.y, offset.z + v[2]);
            verts[5].Set(offset.x, offset.y, offset.z - v[2]);

            // set polygons
            for (i = 0; i < numPolys; i++)
            {
                e0 = polys[i].edges[0];
                e1 = polys[i].edges[1];
                v0 = edges[Math.Abs(e0)].v[MathX.INTSIGNBITSET_(e0)];
                v1 = edges[Math.Abs(e0)].v[MathX.INTSIGNBITNOTSET_(e0)];
                v2 = edges[Math.Abs(e1)].v[MathX.INTSIGNBITNOTSET_(e1)];
                // polygon plane
                polys[i].normal = (verts[v1] - verts[v0]).Cross(verts[v2] - verts[v0]);
                polys[i].normal.Normalize();
                polys[i].dist = polys[i].normal * verts[v0];
                // polygon bounds
                polys[i].bounds[0] = polys[i].bounds[1] = verts[v0];
                polys[i].bounds.AddPoint(verts[v1]);
                polys[i].bounds.AddPoint(verts[v2]);
            }

            // trm bounds
            bounds = octBounds;

            GenerateEdgeNormals();
        }

        // The origin is placed at the center of the octahedron.
        public void SetupOctahedron(float size)
        {
            var halfSize = size * 0.5f;
            var octBounds = new Bounds();
            octBounds[0].Set(-halfSize, -halfSize, -halfSize);
            octBounds[1].Set(halfSize, halfSize, halfSize);
            SetupOctahedron(octBounds);
        }

        #endregion

        #region Dodecahedron: dodecahedron

        // Initialize size independent dodecahedron.
        unsafe void InitDodecahedron()
        {
            type = TRM.DODECAHEDRON;
            numVerts = 20;
            numEdges = 30;
            numPolys = 12;

            // set edges
            edges[1].v[0] = 0; edges[1].v[1] = 8;
            edges[2].v[0] = 8; edges[2].v[1] = 9;
            edges[3].v[0] = 9; edges[3].v[1] = 4;
            edges[4].v[0] = 4; edges[4].v[1] = 16;
            edges[5].v[0] = 16; edges[5].v[1] = 0;
            edges[6].v[0] = 16; edges[6].v[1] = 17;
            edges[7].v[0] = 17; edges[7].v[1] = 2;
            edges[8].v[0] = 2; edges[8].v[1] = 12;
            edges[9].v[0] = 12; edges[9].v[1] = 0;
            edges[10].v[0] = 2; edges[10].v[1] = 10;
            edges[11].v[0] = 10; edges[11].v[1] = 3;
            edges[12].v[0] = 3; edges[12].v[1] = 13;
            edges[13].v[0] = 13; edges[13].v[1] = 12;
            edges[14].v[0] = 9; edges[14].v[1] = 5;
            edges[15].v[0] = 5; edges[15].v[1] = 15;
            edges[16].v[0] = 15; edges[16].v[1] = 14;
            edges[17].v[0] = 14; edges[17].v[1] = 4;
            edges[18].v[0] = 3; edges[18].v[1] = 19;
            edges[19].v[0] = 19; edges[19].v[1] = 18;
            edges[20].v[0] = 18; edges[20].v[1] = 1;
            edges[21].v[0] = 1; edges[21].v[1] = 13;
            edges[22].v[0] = 7; edges[22].v[1] = 11;
            edges[23].v[0] = 11; edges[23].v[1] = 6;
            edges[24].v[0] = 6; edges[24].v[1] = 14;
            edges[25].v[0] = 15; edges[25].v[1] = 7;
            edges[26].v[0] = 1; edges[26].v[1] = 8;
            edges[27].v[0] = 18; edges[27].v[1] = 5;
            edges[28].v[0] = 6; edges[28].v[1] = 17;
            edges[29].v[0] = 11; edges[29].v[1] = 10;
            edges[30].v[0] = 19; edges[30].v[1] = 7;

            // all edges of a polygon go counter clockwise
            polys[0].numEdges = 5;
            polys[0].edges[0] = 1;
            polys[0].edges[1] = 2;
            polys[0].edges[2] = 3;
            polys[0].edges[3] = 4;
            polys[0].edges[4] = 5;

            polys[1].numEdges = 5;
            polys[1].edges[0] = -5;
            polys[1].edges[1] = 6;
            polys[1].edges[2] = 7;
            polys[1].edges[3] = 8;
            polys[1].edges[4] = 9;

            polys[2].numEdges = 5;
            polys[2].edges[0] = -8;
            polys[2].edges[1] = 10;
            polys[2].edges[2] = 11;
            polys[2].edges[3] = 12;
            polys[2].edges[4] = 13;

            polys[3].numEdges = 5;
            polys[3].edges[0] = 14;
            polys[3].edges[1] = 15;
            polys[3].edges[2] = 16;
            polys[3].edges[3] = 17;
            polys[3].edges[4] = -3;

            polys[4].numEdges = 5;
            polys[4].edges[0] = 18;
            polys[4].edges[1] = 19;
            polys[4].edges[2] = 20;
            polys[4].edges[3] = 21;
            polys[4].edges[4] = -12;

            polys[5].numEdges = 5;
            polys[5].edges[0] = 22;
            polys[5].edges[1] = 23;
            polys[5].edges[2] = 24;
            polys[5].edges[3] = -16;
            polys[5].edges[4] = 25;

            polys[6].numEdges = 5;
            polys[6].edges[0] = -9;
            polys[6].edges[1] = -13;
            polys[6].edges[2] = -21;
            polys[6].edges[3] = 26;
            polys[6].edges[4] = -1;

            polys[7].numEdges = 5;
            polys[7].edges[0] = -26;
            polys[7].edges[1] = -20;
            polys[7].edges[2] = 27;
            polys[7].edges[3] = -14;
            polys[7].edges[4] = -2;

            polys[8].numEdges = 5;
            polys[8].edges[0] = -4;
            polys[8].edges[1] = -17;
            polys[8].edges[2] = -24;
            polys[8].edges[3] = 28;
            polys[8].edges[4] = -6;

            polys[9].numEdges = 5;
            polys[9].edges[0] = -23;
            polys[9].edges[1] = 29;
            polys[9].edges[2] = -10;
            polys[9].edges[3] = -7;
            polys[9].edges[4] = -28;

            polys[10].numEdges = 5;
            polys[10].edges[0] = -25;
            polys[10].edges[1] = -15;
            polys[10].edges[2] = -27;
            polys[10].edges[3] = -19;
            polys[10].edges[4] = 30;

            polys[11].numEdges = 5;
            polys[11].edges[0] = -30;
            polys[11].edges[1] = -18;
            polys[11].edges[2] = -11;
            polys[11].edges[3] = -29;
            polys[11].edges[4] = -22;

            // convex model
            isConvex = true;
        }

        public unsafe void SetupDodecahedron(in Bounds dodBounds)
        {
            int i, e0, e1, e2, e3, v0, v1, v2, v3, v4;
            float s, d;
            Vector3 a = new(), b = new(), c = new();

            if (type != TRM.DODECAHEDRON)                InitDodecahedron();

            a[0] = a[1] = a[2] = 0.5773502691896257f; // 1f / ( 3f ) ^ 0.5f;
            b[0] = b[1] = b[2] = 0.3568220897730899f; // ( ( 3f - ( 5f ) ^ 0.5f ) / 6f ) ^ 0.5f;
            c[0] = c[1] = c[2] = 0.9341723589627156f; // ( ( 3f + ( 5f ) ^ 0.5f ) / 6f ) ^ 0.5f;
            d = 0.5f / c[0];
            s = (dodBounds[1][0] - dodBounds[0][0]) * d;
            a[0] *= s; b[0] *= s; c[0] *= s;
            s = (dodBounds[1][1] - dodBounds[0][1]) * d;
            a[1] *= s; b[1] *= s; c[1] *= s;
            s = (dodBounds[1][2] - dodBounds[0][2]) * d;
            a[2] *= s; b[2] *= s; c[2] *= s;

            offset = (dodBounds[0] + dodBounds[1]) * 0.5f;

            // set vertices
            verts[0].Set(offset.x + a[0], offset.y + a[1], offset.z + a[2]);
            verts[1].Set(offset.x + a[0], offset.y + a[1], offset.z - a[2]);
            verts[2].Set(offset.x + a[0], offset.y - a[1], offset.z + a[2]);
            verts[3].Set(offset.x + a[0], offset.y - a[1], offset.z - a[2]);
            verts[4].Set(offset.x - a[0], offset.y + a[1], offset.z + a[2]);
            verts[5].Set(offset.x - a[0], offset.y + a[1], offset.z - a[2]);
            verts[6].Set(offset.x - a[0], offset.y - a[1], offset.z + a[2]);
            verts[7].Set(offset.x - a[0], offset.y - a[1], offset.z - a[2]);
            verts[8].Set(offset.x + b[0], offset.y + c[1], offset.z);
            verts[9].Set(offset.x - b[0], offset.y + c[1], offset.z);
            verts[10].Set(offset.x + b[0], offset.y - c[1], offset.z);
            verts[11].Set(offset.x - b[0], offset.y - c[1], offset.z);
            verts[12].Set(offset.x + c[0], offset.y, offset.z + b[2]);
            verts[13].Set(offset.x + c[0], offset.y, offset.z - b[2]);
            verts[14].Set(offset.x - c[0], offset.y, offset.z + b[2]);
            verts[15].Set(offset.x - c[0], offset.y, offset.z - b[2]);
            verts[16].Set(offset.x, offset.y + b[1], offset.z + c[2]);
            verts[17].Set(offset.x, offset.y - b[1], offset.z + c[2]);
            verts[18].Set(offset.x, offset.y + b[1], offset.z - c[2]);
            verts[19].Set(offset.x, offset.y - b[1], offset.z - c[2]);

            // set polygons
            for (i = 0; i < numPolys; i++)
            {
                e0 = polys[i].edges[0];
                e1 = polys[i].edges[1];
                e2 = polys[i].edges[2];
                e3 = polys[i].edges[3];
                v0 = edges[Math.Abs(e0)].v[MathX.INTSIGNBITSET_(e0)];
                v1 = edges[Math.Abs(e0)].v[MathX.INTSIGNBITNOTSET_(e0)];
                v2 = edges[Math.Abs(e1)].v[MathX.INTSIGNBITNOTSET_(e1)];
                v3 = edges[Math.Abs(e2)].v[MathX.INTSIGNBITNOTSET_(e2)];
                v4 = edges[Math.Abs(e3)].v[MathX.INTSIGNBITNOTSET_(e3)];
                // polygon plane
                polys[i].normal = (verts[v1] - verts[v0]).Cross(verts[v2] - verts[v0]);
                polys[i].normal.Normalize();
                polys[i].dist = polys[i].normal * verts[v0];
                // polygon bounds
                polys[i].bounds[0] = polys[i].bounds[1] = verts[v0];
                polys[i].bounds.AddPoint(verts[v1]);
                polys[i].bounds.AddPoint(verts[v2]);
                polys[i].bounds.AddPoint(verts[v3]);
                polys[i].bounds.AddPoint(verts[v4]);
            }

            // trm bounds
            bounds = dodBounds;

            GenerateEdgeNormals();
        }

        // The origin is placed at the center of the octahedron.
        public void SetupDodecahedron(float size)
        {
            var halfSize = size * 0.5f;
            var dodBounds = new Bounds();
            dodBounds[0].Set(-halfSize, -halfSize, -halfSize);
            dodBounds[1].Set(halfSize, halfSize, halfSize);
            SetupDodecahedron(dodBounds);
        }

        #endregion

        #region Cylinder: cylinder approximation

        public unsafe void SetupCylinder(in Bounds cylBounds, int numSides)
        {
            int i, n, ii, n2; float angle; Vector3 halfSize;

            n = numSides;
            if (n < 3) n = 3;
            if (n * 2 > MAX_TRACEMODEL_VERTS) { Printf("WARNING: TraceModel::SetupCylinder: too many vertices\n"); n = MAX_TRACEMODEL_VERTS / 2; }
            if (n * 3 > MAX_TRACEMODEL_EDGES) { Printf("WARNING: TraceModel::SetupCylinder: too many sides\n"); n = MAX_TRACEMODEL_EDGES / 3; }
            if (n + 2 > MAX_TRACEMODEL_POLYS) { Printf("WARNING: TraceModel::SetupCylinder: too many polygons\n"); n = MAX_TRACEMODEL_POLYS - 2; }

            type = TRM.CYLINDER;
            numVerts = n * 2;
            numEdges = n * 3;
            numPolys = n + 2;
            offset = (cylBounds[0] + cylBounds[1]) * 0.5f;
            halfSize = cylBounds[1] - offset;
            for (i = 0; i < n; i++)
            {
                // verts
                angle = MathX.TWO_PI * i / n;
                verts[i].x = (float)Math.Cos(angle) * halfSize.x + offset.x;
                verts[i].y = (float)Math.Sin(angle) * halfSize.y + offset.y;
                verts[i].z = -halfSize.z + offset.z;
                verts[n + i].x = verts[i].x;
                verts[n + i].y = verts[i].y;
                verts[n + i].z = halfSize.z + offset.z;
                // edges
                ii = i + 1;
                n2 = n << 1;
                edges[ii].v[0] = i;
                edges[ii].v[1] = ii % n;
                edges[n + ii].v[0] = edges[ii].v[0] + n;
                edges[n + ii].v[1] = edges[ii].v[1] + n;
                edges[n2 + ii].v[0] = i;
                edges[n2 + ii].v[1] = n + i;
                // vertical polygon edges
                polys[i].numEdges = 4;
                polys[i].edges[0] = ii;
                polys[i].edges[1] = n2 + (ii % n) + 1;
                polys[i].edges[2] = -(n + ii);
                polys[i].edges[3] = -(n2 + ii);
                // bottom and top polygon edges
                polys[n].edges[i] = -(n - i);
                polys[n + 1].edges[i] = n + ii;
            }
            // bottom and top polygon numEdges
            polys[n].numEdges = n;
            polys[n + 1].numEdges = n;
            // polygons
            for (i = 0; i < n; i++)
            {
                // vertical polygon plane
                polys[i].normal = (verts[(i + 1) % n] - verts[i]).Cross(verts[n + i] - verts[i]);
                polys[i].normal.Normalize();
                polys[i].dist = polys[i].normal * verts[i];
                // vertical polygon bounds
                polys[i].bounds.Clear();
                polys[i].bounds.AddPoint(verts[i]);
                polys[i].bounds.AddPoint(verts[(i + 1) % n]);
                polys[i].bounds[0][2] = -halfSize.z + offset.z;
                polys[i].bounds[1][2] = halfSize.z + offset.z;
            }
            // bottom and top polygon plane
            polys[n].normal.Set(0f, 0f, -1f);
            polys[n].dist = -cylBounds[0][2];
            polys[n + 1].normal.Set(0f, 0f, 1f);
            polys[n + 1].dist = cylBounds[1][2];
            // trm bounds
            bounds = cylBounds;
            // bottom and top polygon bounds
            polys[n].bounds = bounds;
            polys[n].bounds[1][2] = bounds[0][2];
            polys[n + 1].bounds = bounds;
            polys[n + 1].bounds[0][2] = bounds[1][2];
            // convex model
            isConvex = true;

            GenerateEdgeNormals();
        }

        // The origin is placed at the center of the cylinder.
        public void SetupCylinder(float height, float width, int numSides)
        {
            var halfHeight = height * 0.5f;
            var halfWidth = width * 0.5f;
            var cylBounds = new Bounds();
            cylBounds[0].Set(-halfWidth, -halfWidth, -halfHeight);
            cylBounds[1].Set(halfWidth, halfWidth, halfHeight);
            SetupCylinder(cylBounds, numSides);
        }

        #endregion

        #region Cone: cone approximation

        public unsafe void SetupCone(in Bounds coneBounds, int numSides)
        {
            int i, n, ii; float angle; Vector3 halfSize;

            n = numSides;
            if (n < 2) n = 3;
            if (n + 1 > MAX_TRACEMODEL_VERTS) { Printf("WARNING: TraceModel::SetupCone: too many vertices\n"); n = MAX_TRACEMODEL_VERTS - 1; }
            if (n * 2 > MAX_TRACEMODEL_EDGES) { Printf("WARNING: TraceModel::SetupCone: too many edges\n"); n = MAX_TRACEMODEL_EDGES / 2; }
            if (n + 1 > MAX_TRACEMODEL_POLYS) { Printf("WARNING: TraceModel::SetupCone: too many polygons\n"); n = MAX_TRACEMODEL_POLYS - 1; }

            type = TRM.CONE;
            numVerts = n + 1;
            numEdges = n * 2;
            numPolys = n + 1;
            offset = (coneBounds[0] + coneBounds[1]) * 0.5f;
            halfSize = coneBounds[1] - offset;
            verts[n].Set(0f, 0f, halfSize.z + offset.z);
            for (i = 0; i < n; i++)
            {
                // verts
                angle = MathX.TWO_PI * i / n;
                verts[i].x = (float)Math.Cos(angle) * halfSize.x + offset.x;
                verts[i].y = (float)Math.Sin(angle) * halfSize.y + offset.y;
                verts[i].z = -halfSize.z + offset.z;
                // edges
                ii = i + 1;
                edges[ii].v[0] = i;
                edges[ii].v[1] = ii % n;
                edges[n + ii].v[0] = i;
                edges[n + ii].v[1] = n;
                // vertical polygon edges
                polys[i].numEdges = 3;
                polys[i].edges[0] = ii;
                polys[i].edges[1] = n + (ii % n) + 1;
                polys[i].edges[2] = -(n + ii);
                // bottom polygon edges
                polys[n].edges[i] = -(n - i);
            }
            // bottom polygon numEdges
            polys[n].numEdges = n;

            // polygons
            for (i = 0; i < n; i++)
            {
                // polygon plane
                polys[i].normal = (verts[(i + 1) % n] - verts[i]).Cross(verts[n] - verts[i]);
                polys[i].normal.Normalize();
                polys[i].dist = polys[i].normal * verts[i];
                // polygon bounds
                polys[i].bounds.Clear();
                polys[i].bounds.AddPoint(verts[i]);
                polys[i].bounds.AddPoint(verts[(i + 1) % n]);
                polys[i].bounds.AddPoint(verts[n]);
            }
            // bottom polygon plane
            polys[n].normal.Set(0f, 0f, -1f);
            polys[n].dist = -coneBounds[0][2];
            // trm bounds
            bounds = coneBounds;
            // bottom polygon bounds
            polys[n].bounds = bounds;
            polys[n].bounds[1][2] = bounds[0][2];
            // convex model
            isConvex = true;

            GenerateEdgeNormals();
        }

        // The origin is placed at the apex of the cone.
        public void SetupCone(float height, float width, int numSides)
        {
            var halfWidth = width * 0.5f;
            var coneBounds = new Bounds();
            coneBounds[0].Set(-halfWidth, -halfWidth, -height);
            coneBounds[1].Set(halfWidth, halfWidth, 0f);
            SetupCone(coneBounds, numSides);
        }

        #endregion

        #region Bone: two tetrahedrons attached to each other

        // Initialize size independent bone.
        unsafe void InitBone()
        {
            int i;

            type = TRM.BONE;
            numVerts = 5;
            numEdges = 9;
            numPolys = 6;

            // set bone edges
            for (i = 0; i < 3; i++)
            {
                edges[i + 1].v[0] = 0;
                edges[i + 1].v[1] = i + 1;
                edges[i + 4].v[0] = 1 + i;
                edges[i + 4].v[1] = 1 + ((i + 1) % 3);
                edges[i + 7].v[0] = i + 1;
                edges[i + 7].v[1] = 4;
            }

            // all edges of a polygon go counter clockwise
            polys[0].numEdges = 3;
            polys[0].edges[0] = 2;
            polys[0].edges[1] = -4;
            polys[0].edges[2] = -1;

            polys[1].numEdges = 3;
            polys[1].edges[0] = 3;
            polys[1].edges[1] = -5;
            polys[1].edges[2] = -2;

            polys[2].numEdges = 3;
            polys[2].edges[0] = 1;
            polys[2].edges[1] = -6;
            polys[2].edges[2] = -3;

            polys[3].numEdges = 3;
            polys[3].edges[0] = 4;
            polys[3].edges[1] = 8;
            polys[3].edges[2] = -7;

            polys[4].numEdges = 3;
            polys[4].edges[0] = 5;
            polys[4].edges[1] = 9;
            polys[4].edges[2] = -8;

            polys[5].numEdges = 3;
            polys[5].edges[0] = 6;
            polys[5].edges[1] = 7;
            polys[5].edges[2] = -9;

            // convex model
            isConvex = true;
        }

        // The origin is placed at the center of the bone.
        public unsafe void SetupBone(float length, float width)
        {
            int i, j, edgeNum; float halfLength = length * 0.5f;

            if (type != TRM.BONE) InitBone();

            // offset to center
            offset.Set(0f, 0f, 0f);
            // set vertices
            verts[0].Set(0f, 0f, -halfLength);
            verts[1].Set(0f, width * -0.5f, 0f);
            verts[2].Set(width * 0.5f, width * 0.25f, 0f);
            verts[3].Set(width * -0.5f, width * 0.25f, 0f);
            verts[4].Set(0f, 0f, halfLength);
            // set bounds
            bounds[0].Set(width * -0.5f, width * -0.5f, -halfLength);
            bounds[1].Set(width * 0.5f, width * 0.25f, halfLength);
            // poly plane normals
            polys[0].normal = (verts[2] - verts[0]).Cross(verts[1] - verts[0]);
            polys[0].normal.Normalize();
            polys[2].normal.Set(-polys[0].normal[0], polys[0].normal[1], polys[0].normal[2]);
            polys[3].normal.Set(polys[0].normal[0], polys[0].normal[1], -polys[0].normal[2]);
            polys[5].normal.Set(-polys[0].normal[0], polys[0].normal[1], -polys[0].normal[2]);
            polys[1].normal = (verts[3] - verts[0]).Cross(verts[2] - verts[0]);
            polys[1].normal.Normalize();
            polys[4].normal.Set(polys[1].normal[0], polys[1].normal[1], -polys[1].normal[2]);
            // poly plane distances
            for (i = 0; i < 6; i++)
            {
                polys[i].dist = polys[i].normal * verts[edges[Math.Abs(polys[i].edges[0])].v[0]];
                polys[i].bounds.Clear();
                for (j = 0; j < 3; j++)
                {
                    edgeNum = polys[i].edges[j];
                    polys[i].bounds.AddPoint(verts[edges[Math.Abs(edgeNum)].v[edgeNum < 0 ? 1 : 0]]);
                }
            }

            GenerateEdgeNormals();
        }

        #endregion

        #region Polygon: arbitrary convex polygon

        public unsafe void SetupPolygon(Vector3* v, int count)
        {
            int i, j; Vector3 mid;

            type = TRM.POLYGON;
            numVerts = count;
            // times three because we need to be able to turn the polygon into a volume
            if (numVerts * 3 > MAX_TRACEMODEL_EDGES) { Printf("WARNING: TraceModel::SetupPolygon: too many vertices\n"); numVerts = MAX_TRACEMODEL_EDGES / 3; }

            numEdges = numVerts;
            numPolys = 2;
            // set polygon planes
            polys[0].numEdges = numEdges;
            polys[0].normal = (v[1] - v[0]).Cross(v[2] - v[0]);
            polys[0].normal.Normalize();
            polys[0].dist = polys[0].normal * v[0];
            polys[1].numEdges = numEdges;
            polys[1].normal = -polys[0].normal;
            polys[1].dist = -polys[0].dist;
            // setup verts, edges and polygons
            polys[0].bounds.Clear();
            mid = Vector3.origin;
            for (i = 0, j = 1; i < numVerts; i++, j++)
            {
                if (j >= numVerts) j = 0;
                verts[i] = v[i];
                edges[i + 1].v[0] = i;
                edges[i + 1].v[1] = j;
                edges[i + 1].normal = polys[0].normal.Cross(v[i] - v[j]);
                edges[i + 1].normal.Normalize();
                polys[0].edges[i] = i + 1;
                polys[1].edges[i] = -(numVerts - i);
                polys[0].bounds.AddPoint(verts[i]);
                mid += v[i];
            }
            polys[1].bounds = polys[0].bounds;
            // offset to center
            offset = mid * (1f / numVerts);
            // total bounds
            bounds = polys[0].bounds;
            // considered non convex because the model has no volume
            isConvex = false;
        }

        public unsafe void SetupPolygon(in Winding w)
        {
            var verts = stackalloc Vector3[w.NumPoints + Vector3.ALLOC16]; verts = (Vector3*)_alloca16(verts);
            for (var i = 0; i < w.NumPoints; i++) verts[i] = w[i].ToVec3();
            SetupPolygon(verts, w.NumPoints);
        }

        unsafe void VolumeFromPolygon(out TraceModel trm, float thickness)
        {
            int i;

            trm = this;
            trm.type = TRM.POLYGONVOLUME;
            trm.numVerts = numVerts * 2;
            trm.numEdges = numEdges * 3;
            trm.numPolys = numEdges + 2;
            for (i = 0; i < numEdges; i++)
            {
                trm.verts[numVerts + i] = verts[i] - thickness * polys[0].normal;
                trm.edges[numEdges + i + 1].v[0] = numVerts + i;
                trm.edges[numEdges + i + 1].v[1] = numVerts + (i + 1) % numVerts;
                trm.edges[numEdges * 2 + i + 1].v[0] = i;
                trm.edges[numEdges * 2 + i + 1].v[1] = numVerts + i;
                trm.polys[1].edges[i] = -(numEdges + i + 1);
                trm.polys[2 + i].numEdges = 4;
                trm.polys[2 + i].edges[0] = -(i + 1);
                trm.polys[2 + i].edges[1] = numEdges * 2 + i + 1;
                trm.polys[2 + i].edges[2] = numEdges + i + 1;
                trm.polys[2 + i].edges[3] = -(numEdges * 2 + (i + 1) % numEdges + 1);
                trm.polys[2 + i].normal = (verts[(i + 1) % numVerts] - verts[i]).Cross(polys[0].normal);
                trm.polys[2 + i].normal.Normalize();
                trm.polys[2 + i].dist = trm.polys[2 + i].normal * verts[i];
            }
            trm.polys[1].dist = trm.polys[1].normal * trm.verts[numEdges];

            trm.GenerateEdgeNormals();
        }

        #endregion

        // generate edge normals
        const float SHARP_EDGE_DOT = -0.7f;
        public unsafe int GenerateEdgeNormals()
        {
            int i, j, edgeNum, numSharpEdges; float dot; Vector3 dir; TraceModelEdge* edge;

            for (i = 0; i <= numEdges; i++) edges[i].normal.Zero();

            numSharpEdges = 0;
            fixed (TraceModelEdge* edges = this.edges)
                for (i = 0; i < numPolys; i++)
                {
                    ref TraceModelPoly poly = ref polys[i];
                    for (j = 0; j < poly.numEdges; j++)
                    {
                        edgeNum = poly.edges[j];
                        edge = edges + Math.Abs(edgeNum);
                        if (edge->normal[0] == 0f && edge->normal[1] == 0f && edge->normal[2] == 0f) edge->normal = poly.normal;
                        else
                        {
                            dot = edge->normal * poly.normal;
                            // if the two planes make a very sharp edge
                            if (dot < SHARP_EDGE_DOT)
                            {
                                // max length normal pointing outside both polygons
                                dir = verts[edge->v[edgeNum > 0 ? 1 : 0]] - verts[edge->v[edgeNum < 0 ? 1 : 0]];
                                edge->normal = edge->normal.Cross(dir) + poly.normal.Cross(-dir);
                                edge->normal *= 0.5f / (0.5f + 0.5f * SHARP_EDGE_DOT) / edge->normal.Length;
                                numSharpEdges++;
                            }
                            else edge->normal = 0.5f / (0.5f + 0.5f * dot) * (edge->normal + poly.normal);
                        }
                    }
                }
            return numSharpEdges;
        }

        // translate the trm
        public void Translate(in Vector3 translation)
        {
            int i;

            for (i = 0; i < numVerts; i++) verts[i] += translation;
            for (i = 0; i < numPolys; i++)
            {
                polys[i].dist += polys[i].normal * translation;
                polys[i].bounds[0] += translation;
                polys[i].bounds[1] += translation;
            }
            offset += translation;
            bounds[0] += translation;
            bounds[1] += translation;
        }

        // rotate the trm
        public unsafe void Rotate(in Matrix3x3 rotation)
        {
            int i, j, edgeNum;

            for (i = 0; i < numVerts; i++) verts[i] *= rotation;

            bounds.Clear();
            for (i = 0; i < numPolys; i++)
            {
                polys[i].normal *= rotation;
                polys[i].bounds.Clear();
                edgeNum = 0;
                for (j = 0; j < polys[i].numEdges; j++)
                {
                    edgeNum = polys[i].edges[j];
                    polys[i].bounds.AddPoint(verts[edges[Math.Abs(edgeNum)].v[MathX.INTSIGNBITSET_(edgeNum)]]);
                }
                polys[i].dist = polys[i].normal * verts[edges[Math.Abs(edgeNum)].v[MathX.INTSIGNBITSET_(edgeNum)]];
                bounds += polys[i].bounds;
            }

            GenerateEdgeNormals();
        }

        // shrink the model m units on all sides
        public unsafe void Shrink(float m)
        {
            int i, j, edgeNum; TraceModelEdge* edge; Vector3 dir;

            fixed (TraceModelEdge* edges = this.edges)
            {
                if (type == TRM.POLYGON)
                {
                    for (i = 0; i < numEdges; i++)
                    {
                        edgeNum = polys[0].edges[i];
                        edge = &edges[Math.Abs(edgeNum)];
                        dir = verts[edge->v[MathX.INTSIGNBITSET_(edgeNum)]] - verts[edge->v[MathX.INTSIGNBITNOTSET_(edgeNum)]];
                        if (dir.Normalize() < 2f * m) continue;
                        dir *= m;
                        verts[edge->v[0]] -= dir;
                        verts[edge->v[1]] += dir;
                    }
                    return;
                }

                for (i = 0; i < numPolys; i++)
                {
                    polys[i].dist -= m;

                    for (j = 0; j < polys[i].numEdges; j++)
                    {
                        edgeNum = polys[i].edges[j];
                        edge = &edges[Math.Abs(edgeNum)];
                        verts[edge->v[MathX.INTSIGNBITSET_(edgeNum)]] -= polys[i].normal * m;
                    }
                }
            }
        }

        // compare
        public bool Compare(in TraceModel a)
        {
            int i;

            if (type != a.type || numVerts != a.numVerts || numEdges != a.numEdges || numPolys != a.numPolys) return false;
            if (bounds != a.bounds || offset != a.offset) return false;

            switch (type)
            {
                case TRM.INVALID:
                case TRM.BOX:
                case TRM.OCTAHEDRON:
                case TRM.DODECAHEDRON:
                case TRM.CYLINDER:
                case TRM.CONE: break;
                case TRM.BONE:
                case TRM.POLYGON:
                case TRM.POLYGONVOLUME:
                case TRM.CUSTOM: for (i = 0; i < a.numVerts; i++) if (verts[i] != a.verts[i]) return false; break;
            }
            return true;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(in TraceModel _, in TraceModel a)
            => _.Compare(a);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(in TraceModel _, in TraceModel a)
            => !_.Compare(a);
        public override bool Equals(object obj)
            => obj is TraceModel q && Compare(q);
        public override int GetHashCode()
            => base.GetHashCode();

        // get the area of one of the polygons
        public unsafe float GetPolygonArea(int polyNum)
        {
            int i; float total; Vector3 base_, v1, v2, cross;

            if (polyNum < 0 || polyNum >= numPolys) return 0f;
            ref TraceModelPoly poly = ref polys[polyNum];
            total = 0f;
            base_ = verts[edges[Math.Abs(poly.edges[0])].v[MathX.INTSIGNBITSET_(poly.edges[0])]];
            for (i = 0; i < poly.numEdges; i++)
            {
                v1 = verts[edges[Math.Abs(poly.edges[i])].v[MathX.INTSIGNBITSET_(poly.edges[i])]] - base_;
                v2 = verts[edges[Math.Abs(poly.edges[i])].v[MathX.INTSIGNBITNOTSET_(poly.edges[i])]] - base_;
                cross = v1.Cross(v2);
                total += cross.Length;
            }
            return total * 0.5f;
        }

        #region SilhouetteEdges

        unsafe int GetOrderedSilhouetteEdges(int* edgeIsSilEdge, int[] silEdges)
        {
            int i, j, edgeNum, numSilEdges, nextSilVert; int* unsortedSilEdges = stackalloc int[MAX_TRACEMODEL_EDGES];

            numSilEdges = 0;
            for (i = 1; i <= numEdges; i++) if (edgeIsSilEdge[i] != 0) unsortedSilEdges[numSilEdges++] = i;

            silEdges[0] = unsortedSilEdges[0];
            unsortedSilEdges[0] = -1;
            nextSilVert = edges[silEdges[0]].v[0];
            for (i = 1; i < numSilEdges; i++)
            {
                for (j = 1; j < numSilEdges; j++)
                {
                    edgeNum = unsortedSilEdges[j];
                    if (edgeNum >= 0)
                    {
                        if (edges[edgeNum].v[0] == nextSilVert) { nextSilVert = edges[edgeNum].v[1]; silEdges[i] = edgeNum; break; }
                        if (edges[edgeNum].v[1] == nextSilVert) { nextSilVert = edges[edgeNum].v[0]; silEdges[i] = -edgeNum; break; }
                    }
                }
                if (j >= numSilEdges) silEdges[i] = 1;    // shouldn't happen
                unsortedSilEdges[j] = -1;
            }
            return numSilEdges;
        }

        // get the silhouette edges
        public unsafe int GetProjectionSilhouetteEdges(in Vector3 projectionOrigin, int[] silEdges)
        {
            int i, j, edgeNum; int* edgeIsSilEdge = stackalloc int[MAX_TRACEMODEL_EDGES + 1]; Vector3 dir;

            Unsafe.InitBlock(edgeIsSilEdge, 0, (MAX_TRACEMODEL_EDGES + 1) * sizeof(int));

            for (i = 0; i < numPolys; i++)
            {
                ref TraceModelPoly poly = ref polys[i];
                edgeNum = poly.edges[0];
                dir = verts[edges[Math.Abs(edgeNum)].v[MathX.INTSIGNBITSET_(edgeNum)]] - projectionOrigin;
                if (dir * poly.normal < 0f) for (j = 0; j < poly.numEdges; j++) { edgeNum = poly.edges[j]; edgeIsSilEdge[Math.Abs(edgeNum)] ^= 1; }
            }

            return GetOrderedSilhouetteEdges(edgeIsSilEdge, silEdges);
        }

        public unsafe int GetParallelProjectionSilhouetteEdges(in Vector3 projectionDir, int[] silEdges)
        {
            int i, j, edgeNum; int* edgeIsSilEdge = stackalloc int[MAX_TRACEMODEL_EDGES + 1];

            Unsafe.InitBlock(edgeIsSilEdge, 0, (MAX_TRACEMODEL_EDGES + 1) * sizeof(int));

            for (i = 0; i < numPolys; i++)
            {
                ref TraceModelPoly poly = ref polys[i];
                if (projectionDir * poly.normal < 0f) for (j = 0; j < poly.numEdges; j++) { edgeNum = poly.edges[j]; edgeIsSilEdge[Math.Abs(edgeNum)] ^= 1; }
            }

            return GetOrderedSilhouetteEdges(edgeIsSilEdge, silEdges);
        }

        #endregion

        // calculate mass properties assuming an uniform density
        public void GetMassProperties(float density, float mass, out Vector3 centerOfMass, in Matrix3x3 inertiaTensor)
        {
            // if polygon trace model
            if (type == TRM.POLYGON) { VolumeFromPolygon(out var trm, 1f); trm.GetMassProperties(density, mass, out centerOfMass, inertiaTensor); return; }

            VolumeIntegrals(out var integrals);

            // if no volume
            if (integrals.T0 == 0f) { mass = 1f; centerOfMass = Vector3.origin; inertiaTensor.Identity(); return; }

            // mass of model
            mass = density * integrals.T0;
            // center of mass
            centerOfMass = integrals.T1 / integrals.T0;
            // compute inertia tensor
            inertiaTensor[0][0] = density * (integrals.T2[1] + integrals.T2[2]);
            inertiaTensor[1][1] = density * (integrals.T2[2] + integrals.T2[0]);
            inertiaTensor[2][2] = density * (integrals.T2[0] + integrals.T2[1]);
            inertiaTensor[0][1] = inertiaTensor[1][0] = -density * integrals.TP[0];
            inertiaTensor[1][2] = inertiaTensor[2][1] = -density * integrals.TP[1];
            inertiaTensor[2][0] = inertiaTensor[0][2] = -density * integrals.TP[2];
            // translate inertia tensor to center of mass
            inertiaTensor[0][0] -= mass * (centerOfMass[1] * centerOfMass[1] + centerOfMass[2] * centerOfMass[2]);
            inertiaTensor[1][1] -= mass * (centerOfMass[2] * centerOfMass[2] + centerOfMass[0] * centerOfMass[0]);
            inertiaTensor[2][2] -= mass * (centerOfMass[0] * centerOfMass[0] + centerOfMass[1] * centerOfMass[1]);
            inertiaTensor[0][1] = inertiaTensor[1][0] += mass * centerOfMass[0] * centerOfMass[1];
            inertiaTensor[1][2] = inertiaTensor[2][1] += mass * centerOfMass[1] * centerOfMass[2];
            inertiaTensor[2][0] = inertiaTensor[0][2] += mass * centerOfMass[2] * centerOfMass[0];
        }

        #region Polyhedral Mass Properties

        struct VolumeIntegrals_
        {
            public float T0;
            public Vector3 T1;
            public Vector3 T2;
            public Vector3 TP;
        }

        struct ProjectionIntegrals_
        {
            public float P1;
            public float Pa, Pb;
            public float Paa, Pab, Pbb;
            public float Paaa, Paab, Pabb, Pbbb;
        }

        struct PolygonIntegrals_
        {
            public float Fa, Fb, Fc;
            public float Faa, Fbb, Fcc;
            public float Faaa, Fbbb, Fccc;
            public float Faab, Fbbc, Fcca;
        }

        unsafe void ProjectionIntegrals(int polyNum, int a, int b, out ProjectionIntegrals_ integrals)
        {
            int i, edgeNum;
            Vector3 v1, v2;
            float a0, a1, da;
            float b0, b1, db;
            float a0_2, a0_3, a0_4, b0_2, b0_3, b0_4;
            float a1_2, a1_3, b1_2, b1_3;
            float C1, Ca, Caa, Caaa, Cb, Cbb, Cbbb;
            float Cab, Kab, Caab, Kaab, Cabb, Kabb;

            integrals = new(); // Unsafe.InitBlock(integrals, 0, sizeof(ProjectionIntegrals_));

            ref TraceModelPoly poly = ref polys[polyNum];
            for (i = 0; i < poly.numEdges; i++)
            {
                edgeNum = poly.edges[i];
                v1 = verts[edges[Math.Abs(edgeNum)].v[edgeNum < 0 ? 1 : 0]];
                v2 = verts[edges[Math.Abs(edgeNum)].v[edgeNum > 0 ? 1 : 0]];
                a0 = v1[a];
                b0 = v1[b];
                a1 = v2[a];
                b1 = v2[b];
                da = a1 - a0;
                db = b1 - b0;
                a0_2 = a0 * a0;
                a0_3 = a0_2 * a0;
                a0_4 = a0_3 * a0;
                b0_2 = b0 * b0;
                b0_3 = b0_2 * b0;
                b0_4 = b0_3 * b0;
                a1_2 = a1 * a1;
                a1_3 = a1_2 * a1;
                b1_2 = b1 * b1;
                b1_3 = b1_2 * b1;

                C1 = a1 + a0;
                Ca = a1 * C1 + a0_2;
                Caa = a1 * Ca + a0_3;
                Caaa = a1 * Caa + a0_4;
                Cb = b1 * (b1 + b0) + b0_2;
                Cbb = b1 * Cb + b0_3;
                Cbbb = b1 * Cbb + b0_4;
                Cab = 3 * a1_2 + 2 * a1 * a0 + a0_2;
                Kab = a1_2 + 2 * a1 * a0 + 3 * a0_2;
                Caab = a0 * Cab + 4 * a1_3;
                Kaab = a1 * Kab + 4 * a0_3;
                Cabb = 4 * b1_3 + 3 * b1_2 * b0 + 2 * b1 * b0_2 + b0_3;
                Kabb = b1_3 + 2 * b1_2 * b0 + 3 * b1 * b0_2 + 4 * b0_3;

                integrals.P1 += db * C1;
                integrals.Pa += db * Ca;
                integrals.Paa += db * Caa;
                integrals.Paaa += db * Caaa;
                integrals.Pb += da * Cb;
                integrals.Pbb += da * Cbb;
                integrals.Pbbb += da * Cbbb;
                integrals.Pab += db * (b1 * Cab + b0 * Kab);
                integrals.Paab += db * (b1 * Caab + b0 * Kaab);
                integrals.Pabb += da * (a1 * Cabb + a0 * Kabb);
            }

            integrals.P1 *= 1f / 2f;
            integrals.Pa *= 1f / 6f;
            integrals.Paa *= 1f / 12f;
            integrals.Paaa *= 1f / 20f;
            integrals.Pb *= 1f / -6f;
            integrals.Pbb *= 1f / -12f;
            integrals.Pbbb *= 1f / -20f;
            integrals.Pab *= 1f / 24f;
            integrals.Paab *= 1f / 60f;
            integrals.Pabb *= 1f / -60f;
        }

        unsafe void PolygonIntegrals(int polyNum, int a, int b, int c, out PolygonIntegrals_ integrals)
        {
            float w, k1, k2, k3, k4; Vector3 n; ProjectionIntegrals_ pi;

            ProjectionIntegrals(polyNum, a, b, out pi);

            n = polys[polyNum].normal;
            w = -polys[polyNum].dist;
            k1 = 1 / n[c];
            k2 = k1 * k1;
            k3 = k2 * k1;
            k4 = k3 * k1;

            integrals.Fa = k1 * pi.Pa;
            integrals.Fb = k1 * pi.Pb;
            integrals.Fc = -k2 * (n[a] * pi.Pa + n[b] * pi.Pb + w * pi.P1);

            integrals.Faa = k1 * pi.Paa;
            integrals.Fbb = k1 * pi.Pbb;
            integrals.Fcc = k3 * (MathX.Square(n[a]) * pi.Paa + 2 * n[a] * n[b] * pi.Pab + MathX.Square(n[b]) * pi.Pbb + w * (2 * (n[a] * pi.Pa + n[b] * pi.Pb) + w * pi.P1));

            integrals.Faaa = k1 * pi.Paaa;
            integrals.Fbbb = k1 * pi.Pbbb;
            integrals.Fccc = -k4 * (MathX.Cube(n[a]) * pi.Paaa + 3 * MathX.Square(n[a]) * n[b] * pi.Paab + 3 * n[a] * MathX.Square(n[b]) * pi.Pabb + MathX.Cube(n[b]) * pi.Pbbb
                    + 3 * w * (MathX.Square(n[a]) * pi.Paa + 2 * n[a] * n[b] * pi.Pab + MathX.Square(n[b]) * pi.Pbb) + w * w * (3 * (n[a] * pi.Pa + n[b] * pi.Pb) + w * pi.P1));

            integrals.Faab = k1 * pi.Paab;
            integrals.Fbbc = -k2 * (n[a] * pi.Pabb + n[b] * pi.Pbbb + w * pi.Pbb);
            integrals.Fcca = k3 * (MathX.Square(n[a]) * pi.Paaa + 2 * n[a] * n[b] * pi.Paab + MathX.Square(n[b]) * pi.Pabb + w * (2 * (n[a] * pi.Paa + n[b] * pi.Pab) + w * pi.Pa));
        }

        void VolumeIntegrals(out VolumeIntegrals_ integrals)
        {
            int i, a, b, c; float nx, ny, nz; PolygonIntegrals_ pi;

            integrals = new(); // Unsafe.InitBlock(integrals, 0, (uint)sizeof(VolumeIntegrals_));

            for (i = 0; i < numPolys; i++)
            {
                ref TraceModelPoly poly = ref polys[i];

                nx = MathX.Fabs(poly.normal[0]);
                ny = MathX.Fabs(poly.normal[1]);
                nz = MathX.Fabs(poly.normal[2]);
                c = nx > ny && nx > nz ? 0 : ny > nz ? 1 : 2; //: opt
                a = (c + 1) % 3;
                b = (a + 1) % 3;

                PolygonIntegrals(i, a, b, c, out pi);

                integrals.T0 += poly.normal[0] * (a == 0 ? pi.Fa : b == 0 ? pi.Fb : pi.Fc);

                integrals.T1[a] += poly.normal[a] * pi.Faa;
                integrals.T1[b] += poly.normal[b] * pi.Fbb;
                integrals.T1[c] += poly.normal[c] * pi.Fcc;
                integrals.T2[a] += poly.normal[a] * pi.Faaa;
                integrals.T2[b] += poly.normal[b] * pi.Fbbb;
                integrals.T2[c] += poly.normal[c] * pi.Fccc;
                integrals.TP[a] += poly.normal[a] * pi.Faab;
                integrals.TP[b] += poly.normal[b] * pi.Fbbc;
                integrals.TP[c] += poly.normal[c] * pi.Fcca;
            }

            integrals.T1 *= 0.5f;
            integrals.T2 *= 1f / 3f;
            integrals.TP *= 0.5f;
        }

        #endregion
    }
}