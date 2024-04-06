using System.Runtime.CompilerServices;
using static System.NumericsX.OpenStack.Gngine.Render.R;
using static System.NumericsX.OpenStack.OpenStack;
using GlIndex = System.Int32;

namespace System.NumericsX.OpenStack.Gngine.Render
{
    unsafe partial class TR
    {
#if false
        const int CACHE_SIZE = 24;
        const int STALL_SIZE = 8;
        static int R_MeshCost(int numIndexes, GlIndex indexes)
        {
            int inCache[CACHE_SIZE];
            int i, j, v, c_stalls, c_loads, fifo;

            for (i = 0; i < CACHE_SIZE; i++) inCache[i] = -1;
            c_loads = 0;
            c_stalls = 0;
            fifo = 0;

            for (i = 0; i < numIndexes; i++)
            {
                v = indexes[i];
                for (j = 0; j < CACHE_SIZE; j++) if (inCache[(fifo + j) % CACHE_SIZE] == v) break;
                if (j == CACHE_SIZE) { c_loads++; inCache[fifo % CACHE_SIZE] = v; fifo++; }
                else if (j < STALL_SIZE) c_stalls++;
            }
            return c_loads;
        }
#endif

        struct VertRef
        {
            public VertRef* next;
            public int tri;
        }

        // Reorganizes the indexes so they will take best advantage of the internal GPU vertex caches
        public static void R_OrderIndexes(int numIndexes, GlIndex* indexes)
        {
            int numTris, numOldIndexes, tri, i, numVerts, v1, v2, c_starts; //int c_cost;

            if (!r_orderIndexes.Bool) return;

            // save off the original indexes
            var oldIndexes = stackalloc GlIndex[numIndexes];
            Unsafe.CopyBlock(oldIndexes, indexes, (uint)(numIndexes * sizeof(GlIndex)));
            numOldIndexes = numIndexes;

            // make a table to mark the triangles when they are emited
            numTris = numIndexes / 3;
            var triangleUsed = stackalloc bool[numTris];
            Unsafe.InitBlock(triangleUsed, 0, (uint)(numTris * sizeof(bool)));

            // find the highest vertex number
            numVerts = 0;
            for (i = 0; i < numIndexes; i++) if (indexes[i] > numVerts) numVerts = indexes[i];
            numVerts++;

            // create a table of triangles used by each vertex
            var vrefs = stackalloc VertRef*[numVerts];
            Unsafe.InitBlock(vrefs, 0, (uint)(numVerts * sizeof(VertRef)));

            var vrefTable = stackalloc VertRef[numIndexes];
            for (i = 0; i < numIndexes; i++)
            {
                tri = i / 3;

                vrefTable[i].tri = tri;
                vrefTable[i].next = vrefs[oldIndexes[i]];
                vrefs[oldIndexes[i]] = &vrefTable[i];
            }

            // generate new indexes
            numIndexes = 0;
            c_starts = 0;
            VertRef* vref;
            while (numIndexes != numOldIndexes)
            {
                // find a triangle that hasn't been used
                for (tri = 0; tri < numTris; tri++) if (!triangleUsed[tri]) break;

                if (tri == numTris) common.Error("R_OrderIndexes: ran out of unused tris");

                c_starts++;
                do
                {
                    // emit this tri
                    var base_ = &oldIndexes[tri * 3];
                    indexes[numIndexes + 0] = base_[0];
                    indexes[numIndexes + 1] = base_[1];
                    indexes[numIndexes + 2] = base_[2];
                    numIndexes += 3;

                    triangleUsed[tri] = true;

                    // try to find a shared edge to another unused tri
                    for (i = 0; i < 3; i++)
                    {
                        v1 = base_[i];
                        v2 = base_[(i + 1) % 3];

                        for (vref = vrefs[v1]; vref != null; vref = vref->next)
                        {
                            tri = vref->tri;
                            if (triangleUsed[tri]) continue;

                            // if this triangle also uses v2, grab it
                            if (oldIndexes[tri * 3 + 0] == v2 ||
                                oldIndexes[tri * 3 + 1] == v2 ||
                                oldIndexes[tri * 3 + 2] == v2)
                                break;
                        }
                        if (vref != null) break;
                    }

                    // if we couldn't chain off of any verts, we need to find a new one
                    if (i == 3) break;
                } while (true);
            }
            //c_cost = R_MeshCost( numIndexes, indexes );
        }
    }
}
