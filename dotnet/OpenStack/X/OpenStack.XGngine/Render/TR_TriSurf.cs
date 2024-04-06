//#define USE_TRI_DATA_ALLOCATOR
#define USE_INVA // this shouldn't change anything, but previously renderbumped models seem to need it
#define DERIVE_UNSMOOTHED_BITANGENT // instead of using the texture T vector, cross the normal and S vector for an orthogonal axis
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static System.NumericsX.OpenStack.Gngine.Gngine;
using static System.NumericsX.OpenStack.OpenStack;
using static System.NumericsX.Platform;
using GlIndex = System.Int32;

namespace System.NumericsX.OpenStack.Gngine.Render
{
    unsafe partial class R
    {
        const int MAX_SIL_EDGES = 0x10000;
        const int SILEDGE_HASH_SIZE = 1024;

        static int numSilEdges;
        static SilEdge* silEdges;
        static HashIndex silEdgeHash = new(SILEDGE_HASH_SIZE, MAX_SIL_EDGES);
        static int numPlanes;

        static BlockAlloc<SrfTriangles> srfTrianglesAllocator = new(1 << 8);

#if USE_TRI_DATA_ALLOCATOR
        static DynamicBlockAlloc<DrawVert> triVertexAllocator = new(1 << 20, 1 << 10);
        static DynamicBlockAlloc<GlIndex> triIndexAllocator = new(1 << 18, 1 << 10);
        static DynamicBlockAlloc<ShadowCache> triShadowVertexAllocator = new(1 << 18, 1 << 10);
        static DynamicBlockAlloc<Plane> triPlaneAllocator = new(1 << 17, 1 << 10);
        static DynamicBlockAlloc<GlIndex> triSilIndexAllocator = new(1 << 17, 1 << 10);
        static DynamicBlockAlloc<SilEdge> triSilEdgeAllocator = new(1 << 17, 1 << 10);
        static DynamicBlockAlloc<DominantTri> triDominantTrisAllocator = new(1 << 16, 1 << 10);
        static DynamicBlockAlloc<int> triMirroredVertAllocator = new(1 << 16, 1 << 10);
        static DynamicBlockAlloc<int> triDupVertAllocator = new(1 << 16, 1 << 10);
#else
        static DynamicAlloc<DrawVert> triVertexAllocator = new(1 << 20, 1 << 10);
        static DynamicAlloc<GlIndex> triIndexAllocator = new(1 << 18, 1 << 10);
        static DynamicAlloc<ShadowCache> triShadowVertexAllocator = new(1 << 18, 1 << 10);
        static DynamicAlloc<Plane> triPlaneAllocator = new(1 << 17, 1 << 10);
        static DynamicAlloc<GlIndex> triSilIndexAllocator = new(1 << 17, 1 << 10);
        static DynamicAlloc<SilEdge> triSilEdgeAllocator = new(1 << 17, 1 << 10);
        static DynamicAlloc<DominantTri> triDominantTrisAllocator = new(1 << 16, 1 << 10);
        static DynamicAlloc<int> triMirroredVertAllocator = new(1 << 16, 1 << 10);
        static DynamicAlloc<int> triDupVertAllocator = new(1 << 16, 1 << 10);
#endif

        public static void R_InitTriSurfData()
        {
            silEdges = (SilEdge*)R_StaticAlloc(MAX_SIL_EDGES * sizeof(SilEdge));

            // initialize allocators for triangle surfaces
            triVertexAllocator.Init();
            triIndexAllocator.Init();
            triShadowVertexAllocator.Init();
            triPlaneAllocator.Init();
            triSilIndexAllocator.Init();
            triSilEdgeAllocator.Init();
            triDominantTrisAllocator.Init();
            triMirroredVertAllocator.Init();
            triDupVertAllocator.Init();

            // never swap out triangle surfaces
            triVertexAllocator.SetLockMemory(true);
            triIndexAllocator.SetLockMemory(true);
            triShadowVertexAllocator.SetLockMemory(true);
            triPlaneAllocator.SetLockMemory(true);
            triSilIndexAllocator.SetLockMemory(true);
            triSilEdgeAllocator.SetLockMemory(true);
            triDominantTrisAllocator.SetLockMemory(true);
            triMirroredVertAllocator.SetLockMemory(true);
            triDupVertAllocator.SetLockMemory(true);
        }

        public static void R_ShutdownTriSurfData()
        {
            R_StaticFree(silEdges);
            silEdgeHash.Free();

            srfTrianglesAllocator.Shutdown();
            triVertexAllocator.Shutdown();
            triIndexAllocator.Shutdown();
            triShadowVertexAllocator.Shutdown();
            triPlaneAllocator.Shutdown();
            triSilIndexAllocator.Shutdown();
            triSilEdgeAllocator.Shutdown();
            triDominantTrisAllocator.Shutdown();
            triMirroredVertAllocator.Shutdown();
            triDupVertAllocator.Shutdown();
        }

        public static void R_PurgeTriSurfData(FrameData frame)
        {
            // free deferred triangle surfaces
            R_FreeDeferredTriSurfs(frame);

            // free empty base blocks
            triVertexAllocator.FreeEmptyBaseBlocks();
            triIndexAllocator.FreeEmptyBaseBlocks();
            triShadowVertexAllocator.FreeEmptyBaseBlocks();
            triPlaneAllocator.FreeEmptyBaseBlocks();
            triSilIndexAllocator.FreeEmptyBaseBlocks();
            triSilEdgeAllocator.FreeEmptyBaseBlocks();
            triDominantTrisAllocator.FreeEmptyBaseBlocks();
            triMirroredVertAllocator.FreeEmptyBaseBlocks();
            triDupVertAllocator.FreeEmptyBaseBlocks();
        }

        public static void R_ShowTriSurfMemory_f(CmdArgs args)
        {
            common.Printf($"{(srfTrianglesAllocator.AllocCount * SrfTriangles.sizeOf) >> 10:6} kB in {srfTrianglesAllocator.AllocCount} triangle surfaces\n");
            common.Printf($"{triVertexAllocator.BaseBlockMemory >> 10:6} kB vertex memory ({triVertexAllocator.FreeBlockMemory >> 10} kB free in {triVertexAllocator.NumFreeBlocks} blocks, {triVertexAllocator.NumEmptyBaseBlocks} empty base blocks)\n");
            common.Printf($"{triIndexAllocator.BaseBlockMemory >> 10:6} kB index memory ({triIndexAllocator.FreeBlockMemory >> 10} kB free in {triIndexAllocator.NumFreeBlocks} blocks, {triIndexAllocator.NumEmptyBaseBlocks} empty base blocks)\n");
            common.Printf($"{triShadowVertexAllocator.BaseBlockMemory >> 10:6} kB shadow vert memory ({triShadowVertexAllocator.FreeBlockMemory >> 10} kB free in {triShadowVertexAllocator.NumFreeBlocks} blocks, {triShadowVertexAllocator.NumEmptyBaseBlocks} empty base blocks)\n");
            common.Printf($"{triPlaneAllocator.BaseBlockMemory >> 10:6} kB tri plane memory ({triPlaneAllocator.FreeBlockMemory >> 10} kB free in {triPlaneAllocator.NumFreeBlocks} blocks, {triPlaneAllocator.NumEmptyBaseBlocks} empty base blocks)\n");
            common.Printf($"{triSilIndexAllocator.BaseBlockMemory >> 10:6} kB sil index memory ({triSilIndexAllocator.FreeBlockMemory >> 10} kB free in {triSilIndexAllocator.NumFreeBlocks} blocks, {triSilIndexAllocator.NumEmptyBaseBlocks} empty base blocks)\n");
            common.Printf($"{triSilEdgeAllocator.BaseBlockMemory >> 10:6} kB sil edge memory ({triSilEdgeAllocator.FreeBlockMemory >> 10} kB free in {triSilEdgeAllocator.NumFreeBlocks} blocks, {triSilEdgeAllocator.NumEmptyBaseBlocks} empty base blocks)\n");
            common.Printf($"{triDominantTrisAllocator.BaseBlockMemory >> 10:6} kB dominant tri memory ({triDominantTrisAllocator.FreeBlockMemory >> 10} kB free in {triDominantTrisAllocator.NumFreeBlocks} blocks, {triDominantTrisAllocator.NumEmptyBaseBlocks} empty base blocks)\n");
            common.Printf($"{triMirroredVertAllocator.BaseBlockMemory >> 10:6} kB mirror vert memory ({triMirroredVertAllocator.FreeBlockMemory >> 10} kB free in {triMirroredVertAllocator.NumFreeBlocks} blocks, {triMirroredVertAllocator.NumEmptyBaseBlocks} empty base blocks)\n");
            common.Printf($"{triDupVertAllocator.BaseBlockMemory >> 10:6} kB dup vert memory ({triDupVertAllocator.FreeBlockMemory >> 10} kB free in {triDupVertAllocator.NumFreeBlocks} blocks, {triDupVertAllocator.NumEmptyBaseBlocks} empty base blocks)\n");
            common.Printf($"{((srfTrianglesAllocator.AllocCount * SrfTriangles.sizeOf + triVertexAllocator.BaseBlockMemory + triIndexAllocator.BaseBlockMemory + triShadowVertexAllocator.BaseBlockMemory + triPlaneAllocator.BaseBlockMemory + triSilIndexAllocator.BaseBlockMemory + triSilEdgeAllocator.BaseBlockMemory + triDominantTrisAllocator.BaseBlockMemory + triMirroredVertAllocator.BaseBlockMemory + triDupVertAllocator.BaseBlockMemory) >> 10):6} kB total triangle memory\n");
        }

        // For memory profiling
        public static int R_TriSurfMemory(SrfTriangles tri)
        {
            var total = 0;

            if (tri == null) return total;

            // used as a flag in interations
            if (tri == IInteraction.LIGHT_TRIS_DEFERRED) return total;

            if (tri.shadowVertexes != null) total += tri.numVerts * sizeof(ShadowCache);
            else if (tri.verts != null)
            {
                if (tri.ambientSurface == null || tri.verts != tri.ambientSurface.verts) total += tri.numVerts * sizeof(DrawVert);
            }
            if (tri.facePlanes != null) total += tri.numIndexes / 3 * sizeof(Plane);
            if (tri.indexes != null)
            {
                if (tri.ambientSurface == null || tri.indexes != tri.ambientSurface.indexes) total += tri.numIndexes * sizeof(GlIndex);
            }
            if (tri.silIndexes != null) total += tri.numIndexes * sizeof(GlIndex);
            if (tri.silEdges != null) total += tri.numSilEdges * sizeof(SilEdge);
            if (tri.dominantTris != null) total += tri.numVerts * sizeof(DominantTri);
            if (tri.mirroredVerts != null) total += tri.numMirroredVerts * sizeof(int);
            if (tri.dupVerts != null) total += tri.numDupVerts * sizeof(int);

            total += SrfTriangles.sizeOf;

            return total;
        }

        public static void R_FreeStaticTriSurfVertexCaches(SrfTriangles tri)
        {
            // this is a real model surface
            if (tri.ambientSurface == null) { vertexCache.Free(ref tri.ambientCache); tri.ambientCache = null; }
            // this is a light interaction surface that references a different ambient model surface
            else { vertexCache.Free(ref tri.lightingCache); tri.lightingCache = null; }
            if (tri.indexCache != null) { vertexCache.Free(ref tri.indexCache); tri.indexCache = null; }
            // if we don't have tri.shadowVertexes, these are a reference to a shadowCache on the original surface, which a vertex program
            // will take care of making unique for each light
            if (tri.shadowCache != null && (tri.shadowVertexes != null || tri.verts != null)) { vertexCache.Free(ref tri.shadowCache); tri.shadowCache = null; }
        }

        // This does the actual free
        public static void R_ReallyFreeStaticTriSurf(SrfTriangles tri)
        {
            if (tri == null) return;

            R_FreeStaticTriSurfVertexCaches(tri);

            // R_CreateLightTris points tri.verts at the verts of the ambient surface
            if (tri.verts != null && (tri.ambientSurface == null || tri.verts != tri.ambientSurface.verts)) triVertexAllocator.Free(tri.verts);

            if (!tri.deformedSurface)
            {
                if (tri.indexes != null)
                {
                    // if a surface is completely inside a light volume R_CreateLightTris points tri.indexes at the indexes of the ambient surface
                    if (tri.ambientSurface == null || tri.indexes != tri.ambientSurface.indexes) triIndexAllocator.Free(tri.indexes);
                }
                if (tri.silIndexes != null) triSilIndexAllocator.Free(tri.silIndexes);
                if (tri.silEdges != null) triSilEdgeAllocator.Free(tri.silEdges);
                if (tri.dominantTris != null) triDominantTrisAllocator.Free(tri.dominantTris);
                if (tri.mirroredVerts != null) triMirroredVertAllocator.Free(tri.mirroredVerts);
                if (tri.dupVerts != null) triDupVertAllocator.Free(tri.dupVerts);
            }

            if (tri.facePlanes != null) triPlaneAllocator.Free(tri.facePlanes);
            if (tri.shadowVertexes != null) triShadowVertexAllocator.Free(tri.shadowVertexes);

#if DEBUG
            tri.memset();
#endif

            srfTrianglesAllocator.Free(tri);
        }

        static void R_CheckStaticTriSurfMemory(SrfTriangles tri)
        {
            if (tri == null) return;

            // R_CreateLightTris points tri.verts at the verts of the ambient surface
            if (tri.verts != null && (tri.ambientSurface == null || tri.verts != tri.ambientSurface.verts))
            {
                var error = triVertexAllocator.CheckMemory(tri.verts);
                Debug.Assert(error == null);
            }

            // if a surface is completely inside a light volume R_CreateLightTris points tri.indexes at the indexes of the ambient surface
            if (!tri.deformedSurface && tri.indexes != null)
                if (tri.ambientSurface == null || tri.indexes != tri.ambientSurface.indexes) { var error = triIndexAllocator.CheckMemory(tri.indexes); Debug.Assert(error == null); }

            if (tri.shadowVertexes != null) { var error = triShadowVertexAllocator.CheckMemory(tri.shadowVertexes); Debug.Assert(error == null); }
        }

        public static void R_FreeDeferredTriSurfs(FrameData frame)
        {
            SrfTriangles tri, next;

            if (frame == null) return;

            for (tri = frame.firstDeferredFreeTriSurf; tri != null; tri = next) { next = tri.nextDeferredFree; R_ReallyFreeStaticTriSurf(tri); }

            frame.firstDeferredFreeTriSurf = null;
            frame.lastDeferredFreeTriSurf = null;
        }

        // This will defer the free until the current frame has run through the back end.
        public static void R_FreeStaticTriSurf(SrfTriangles tri)
        {
            FrameData frame;

            if (tri == null) return;

            if (tri.nextDeferredFree != null) common.Error("R_FreeStaticTriSurf: freed a freed triangle");
            frame = frameData;

            // command line utility, or rendering in editor preview mode ( force )
            if (frame == null) R_ReallyFreeStaticTriSurf(tri);
            else
            {
#if ID_DEBUG_MEMORY
                R_CheckStaticTriSurfMemory(tri);
#endif
                tri.nextDeferredFree = null;
                if (frame.lastDeferredFreeTriSurf != null) frame.lastDeferredFreeTriSurf.nextDeferredFree = tri;
                else frame.firstDeferredFreeTriSurf = tri;
                frame.lastDeferredFreeTriSurf = tri;
            }
        }

        public static SrfTriangles R_AllocStaticTriSurf()
        {
            var tris = srfTrianglesAllocator.Alloc();
            tris.memset();
            return tris;
        }

        // This only duplicates the indexes and verts, not any of the derived data.
        public static SrfTriangles R_CopyStaticTriSurf(SrfTriangles tri)
        {
            SrfTriangles newTri;

            newTri = R_AllocStaticTriSurf();
            R_AllocStaticTriSurfVerts(newTri, tri.numVerts);
            R_AllocStaticTriSurfIndexes(newTri, tri.numIndexes);
            newTri.numVerts = tri.numVerts;
            newTri.numIndexes = tri.numIndexes;
            fixed (DrawVert* vertsA = newTri.verts, vertsB = tri.verts) Unsafe.CopyBlock(vertsA, vertsB, (uint)(tri.numVerts * sizeof(DrawVert)));
            fixed (GlIndex* indexesA = newTri.indexes, indexesB = tri.indexes) Unsafe.CopyBlock(indexesA, indexesB, (uint)(tri.numIndexes * sizeof(GlIndex)));
            return newTri;
        }

        public static void R_AllocStaticTriSurfVerts(SrfTriangles tri, int numVerts)
        {
            Debug.Assert(tri.verts == null);
            tri.verts = triVertexAllocator.Alloc(numVerts);
        }

        public static void R_AllocStaticTriSurfIndexes(SrfTriangles tri, int numIndexes)
        {
            Debug.Assert(tri.indexes == null);
            tri.indexes = triIndexAllocator.Alloc(numIndexes);
        }

        public static void R_AllocStaticTriSurfShadowVerts(SrfTriangles tri, int numVerts)
        {
            Debug.Assert(tri.shadowVertexes == null);
            tri.shadowVertexes = triShadowVertexAllocator.Alloc(numVerts);
        }

        public static void R_AllocStaticTriSurfPlanes(SrfTriangles tri, int numIndexes)
        {
            if (tri.facePlanes != null) triPlaneAllocator.Free(tri.facePlanes);
            tri.facePlanes = triPlaneAllocator.Alloc(numIndexes / 3);
        }

        public static void R_ResizeStaticTriSurfVerts(SrfTriangles tri, int numVerts)
#if USE_TRI_DATA_ALLOCATOR
            => tri.verts = triVertexAllocator.Resize(tri.verts, numVerts);
#else
            => Debug.Assert(false);
#endif

        public static void R_ResizeStaticTriSurfIndexes(SrfTriangles tri, int numIndexes)
#if USE_TRI_DATA_ALLOCATOR
            => tri.indexes = triIndexAllocator.Resize(tri.indexes, numIndexes);
#else
            => Debug.Assert(false);
#endif

        public static void R_ResizeStaticTriSurfShadowVerts(SrfTriangles tri, int numVerts)
#if USE_TRI_DATA_ALLOCATOR
            => tri.shadowVertexes = triShadowVertexAllocator.Resize(tri.shadowVertexes, numVerts);
#else
            => Debug.Assert(false);
#endif

        public static void R_ReferenceStaticTriSurfVerts(SrfTriangles tri, SrfTriangles reference)
            => tri.verts = reference.verts;

        public static void R_ReferenceStaticTriSurfIndexes(SrfTriangles tri, SrfTriangles reference)
            => tri.indexes = reference.indexes;

        public static void R_FreeStaticTriSurfSilIndexes(SrfTriangles tri)
        {
            triSilIndexAllocator.Free(tri.silIndexes);
            tri.silIndexes = null;
        }

        // Check for syntactically incorrect indexes, like out of range values. Does not check for semantics, like degenerate triangles.
        // No vertexes is acceptable if no indexes. No indexes is acceptable. More vertexes than are referenced by indexes are acceptable.
        public static void R_RangeCheckIndexes(SrfTriangles tri)
        {
            int i;
            var tri_indexes = tri.indexes;

            if (tri.numIndexes < 0) common.Error("R_RangeCheckIndexes: numIndexes < 0");
            if (tri.numVerts < 0) common.Error("R_RangeCheckIndexes: numVerts < 0");

            // must specify an integral number of triangles
            if (tri.numIndexes % 3 != 0) common.Error("R_RangeCheckIndexes: numIndexes %% 3");

            for (i = 0; i < tri.numIndexes; i++) if (tri_indexes[i] < 0 || tri_indexes[i] >= tri.numVerts) common.Error("R_RangeCheckIndexes: index out of range");

            // this should not be possible unless there are unused verts
            //if (tri.numVerts > tri.numIndexes) common.Printf("R_RangeCheckIndexes: tri.numVerts > tri.numIndexes\n"); // FIXME: find the causes of these
        }

        public static void R_BoundTriSurf(SrfTriangles tri)
        {
            fixed (DrawVert* vertsD = tri.verts) Simd.MinMaxd(out tri.bounds[0], out tri.bounds[1], vertsD, tri.numVerts);
        }

        static int* R_CreateSilRemap(SrfTriangles tri)
        {
            int c_removed, c_unique;
            int i, j, hashKey;

            var remap = (int*)R_ClearedStaticAlloc(tri.numVerts * sizeof(int));

            if (!r_useSilRemap.Bool)
            {
                for (i = 0; i < tri.numVerts; i++) remap[i] = i;
                return remap;
            }

            HashIndex hash = new(1024, tri.numVerts);

            c_removed = 0;
            c_unique = 0;
            for (i = 0; i < tri.numVerts; i++)
            {
                ref DrawVert v1 = ref tri.verts[i];

                // see if there is an earlier vert that it can map to
                hashKey = hash.GenerateKey(v1.xyz);
                for (j = hash.First(hashKey); j >= 0; j = hash.Next(j))
                {
                    ref DrawVert v2 = ref tri.verts[j];
                    if (v2.xyz.x == v1.xyz.z && v2.xyz.y == v1.xyz.y && v2.xyz.z == v1.xyz.z) { c_removed++; remap[i] = j; break; }
                }
                if (j < 0) { c_unique++; remap[i] = i; hash.Add(hashKey, i); }
            }

            return remap;
        }

        // Uniquing vertexes only on xyz before creating sil edges reduces the edge count by about 20% on Q3 models
        public static void R_CreateSilIndexes(SrfTriangles tri)
        {
            int i; int* remap;

            if (tri.silIndexes != null) { triSilIndexAllocator.Free(tri.silIndexes); tri.silIndexes = null; }
            remap = R_CreateSilRemap(tri);

            // remap indexes to the first one
            tri.silIndexes = triSilIndexAllocator.Alloc(tri.numIndexes);
            var tri_silIndexes = tri.silIndexes; var tri_indexes = tri.indexes;
            for (i = 0; i < tri.numIndexes; i++) tri_silIndexes[i] = remap[tri_indexes[i]];
            R_StaticFree(remap);
        }

        static void R_CreateDupVerts(SrfTriangles tri)
        {
            int i;
            var tri_indexes = tri.indexes; var tri_silIndexes = tri.silIndexes;

            int* remap = stackalloc int[tri.numVerts * sizeof(int) + intX.ALLOC16]; remap = (int*)_alloca16(remap);

            // initialize vertex remap in case there are unused verts
            for (i = 0; i < tri.numVerts; i++) remap[i] = i;

            // set the remap based on how the silhouette indexes are remapped
            for (i = 0; i < tri.numIndexes; i++) remap[tri_indexes[i]] = tri_silIndexes[i];

            // create duplicate vertex index based on the vertex remap
            // GB Also protecting this as I think it has got too big
            // DG: windows only has a 1MB stack and it could happen that we try to allocate >1MB here (in lost mission mod, game/le_hell map), causing a stack overflow to prevent that, use heap allocation if it's >600KB
            var allocaSize = tri.numVerts * 2 * sizeof(int);
            var tempDupVertsB = allocaSize < 600000 ? stackalloc int[allocaSize + intX.ALLOC16] : new int[allocaSize + intX.ALLOC16];
            fixed (int* tempDupVertsI = tempDupVertsB)
            {
                var tempDupVerts = (int*)_alloca16(tempDupVertsI);

                tri.numDupVerts = 0;
                for (i = 0; i < tri.numVerts; i++) if (remap[i] != i) { tempDupVerts[tri.numDupVerts * 2 + 0] = i; tempDupVerts[tri.numDupVerts * 2 + 1] = remap[i]; tri.numDupVerts++; }

                tri.dupVerts = triDupVertAllocator.Alloc(tri.numDupVerts * 2);
                fixed (int* dupVertsI = tri.dupVerts) Unsafe.CopyBlock(dupVertsI, tempDupVerts, (uint)(tri.numDupVerts * 2 * sizeof(int)));
            }
        }

        // Writes the facePlanes values, overwriting existing ones if present
        public static void R_DeriveFacePlanes(SrfTriangles tri)
        {
            if (tri.facePlanes == null) R_AllocStaticTriSurfPlanes(tri, tri.numIndexes);

#if true
            fixed (Plane* planesP = tri.facePlanes)
            fixed (DrawVert* vertsD = tri.verts)
            fixed (GlIndex* indexesG = tri.indexes)
                Simd.DeriveTriPlanesi(planesP, vertsD, tri.numVerts, indexesG, tri.numIndexes);
#else
            var tri_facePlanes = tri.facePlanes.Value;
            var tri_indexes = tri.indexes.Value;
            var tri_verts = tri.verts.Value;
            fixed (Plane* planesP = tri_facePlanes)
            {
                Plane* plane = planesP;
                for (var i = 0; i < tri.numIndexes; i += 3, plane++)
                {
                    int i1, i2, i3;
                    Vector3 d1, d2, normal;

                    i1 = tri_indexes[i + 0];
                    i2 = tri_indexes[i + 1];
                    i3 = tri_indexes[i + 2];

                    ref Vector3 v1 = ref tri_verts[i1].xyz;
                    ref Vector3 v2 = ref tri_verts[i2].xyz;
                    ref Vector3 v3 = ref tri_verts[i3].xyz;

                    d1.x = v2.x - v1.x;
                    d1.y = v2.y - v1.y;
                    d1.z = v2.z - v1.z;

                    d2.x = v3.x - v1.x;
                    d2.y = v3.y - v1.y;
                    d2.z = v3.z - v1.z;

                    normal.x = d2.y * d1.z - d2.z * d1.y;
                    normal.y = d2.z * d1.x - d2.x * d1.z;
                    normal.z = d2.x * d1.y - d2.y * d1.x;

                    float sqrLength, invLength;

                    sqrLength = normal.x * normal.x + normal.y * normal.y + normal.z * normal.z;
                    invLength = MathX.RSqrt(sqrLength);

                    plane->a = normal.x * invLength;
                    plane->b = normal.y * invLength;
                    plane->c = normal.z * invLength;

                    plane->FitThroughPoint(v1);
                }
            }
#endif
            tri.facePlanesCalculated = true;
        }

        // Averages together the contributions of all faces that are used by a vertex, creating drawVert.normal
        public static void R_CreateVertexNormals(SrfTriangles tri)
        {
            int i, j;
            var tri_verts = tri.verts; var tri_indexes = tri.indexes; var tri_silIndexes = tri.silIndexes;

            for (i = 0; i < tri.numVerts; i++) tri_verts[i].normal.Zero();

            if (tri.facePlanes == null || !tri.facePlanesCalculated) R_DeriveFacePlanes(tri);
            if (tri.silIndexes == null) R_CreateSilIndexes(tri);
            fixed (Plane* planesP = tri.facePlanes)
            {
                Plane* plane = planesP;
                for (i = 0; i < tri.numIndexes; i += 3, plane++)
                    for (j = 0; j < 3; j++)
                    {
                        var index = tri_silIndexes[i + j];
                        tri_verts[index].normal += plane->Normal;
                    }
            }

            // normalize and replicate from silIndexes to all indexes
            for (i = 0; i < tri.numIndexes; i++)
            {
                tri_verts[tri_indexes[i]].normal = tri_verts[tri_silIndexes[i]].normal;
                tri_verts[tri_indexes[i]].normal.Normalize();
            }
        }

        static int c_duplicatedEdges, c_tripledEdges;
        static void R_DefineEdge(int v1, int v2, int planeNum)
        {
            int i, hashKey;

            // check for degenerate edge
            if (v1 == v2) return;
            hashKey = silEdgeHash.GenerateKey(v1, v2);
            // search for a matching other side
            for (i = silEdgeHash.First(hashKey); i >= 0 && i < MAX_SIL_EDGES; i = silEdgeHash.Next(i))
            {
                if (silEdges[i].v1 == v1 && silEdges[i].v2 == v2) { c_duplicatedEdges++; continue; } // allow it to still create a new edge
                if (silEdges[i].v2 == v1 && silEdges[i].v1 == v2)
                {
                    if (silEdges[i].p2 != numPlanes) { c_tripledEdges++; continue; } // allow it to still create a new edge
                    // this is a matching back side
                    silEdges[i].p2 = planeNum;
                    return;
                }
            }

            // define the new edge
            if (numSilEdges == MAX_SIL_EDGES) { common.DWarning("MAX_SIL_EDGES"); return; }

            silEdgeHash.Add(hashKey, numSilEdges);

            silEdges[numSilEdges].p1 = planeNum;
            silEdges[numSilEdges].p2 = numPlanes;
            silEdges[numSilEdges].v1 = v1;
            silEdges[numSilEdges].v2 = v2;

            numSilEdges++;
        }

        static int SilEdgeSort(SilEdge* a, SilEdge* b)
        {
            if (a->p1 < b->p1) return -1;
            else if (a->p1 > b->p1) return 1;
            else if (a->p2 < b->p2) return -1;
            else if (a->p2 > b->p2) return 1;
            else return 0;
        }

        // If the surface will not deform, coplanar edges (polygon interiors) can never create silhouette plains, and can be omited
        static int c_coplanarSilEdges, c_totalSilEdges;
        static void R_IdentifySilEdges(SrfTriangles tri, bool omitCoplanarEdges)
        {
            int i, numTris, shared, single;
            var tri_verts = tri.verts; var tri_silIndexes = tri.silIndexes;

            omitCoplanarEdges = false;  // optimization doesn't work for some reason

            numTris = tri.numIndexes / 3;

            numSilEdges = 0;
            silEdgeHash.Clear();
            numPlanes = numTris;

            c_duplicatedEdges = 0;
            c_tripledEdges = 0;

            for (i = 0; i < numTris; i++)
            {
                int i1, i2, i3;

                i1 = tri_silIndexes[i * 3 + 0];
                i2 = tri_silIndexes[i * 3 + 1];
                i3 = tri_silIndexes[i * 3 + 2];

                // create the edges
                R_DefineEdge(i1, i2, i);
                R_DefineEdge(i2, i3, i);
                R_DefineEdge(i3, i1, i);
            }

            if (c_duplicatedEdges != 0 || c_tripledEdges != 0) common.DWarning($"{c_duplicatedEdges} duplicated edge directions, {c_tripledEdges} tripled edges");

            // if we know that the vertexes aren't going to deform, we can remove interior triangulation edges on otherwise planar polygons. I earlier believed that I could also remove concave
            // edges, because they are never silhouettes in the conventional sense, but they are still needed to balance out all the true sil edges for the shadow algorithm to function
            var c_coplanarCulled = 0;

            if (omitCoplanarEdges)
            {
                for (i = 0; i < numSilEdges; i++)
                {
                    int i1, i2, i3, base_, j; float d;
                    Plane plane = default;

                    if (silEdges[i].p2 == numPlanes) continue; // the fake dangling edge

                    base_ = silEdges[i].p1 * 3;
                    i1 = tri_silIndexes[base_ + 0];
                    i2 = tri_silIndexes[base_ + 1];
                    i3 = tri_silIndexes[base_ + 2];

                    plane.FromPoints(tri_verts[i1].xyz, tri_verts[i2].xyz, tri_verts[i3].xyz);

                    // check to see if points of second triangle are not coplanar
                    base_ = silEdges[i].p2 * 3;
                    for (j = 0; j < 3; j++)
                    {
                        i1 = tri_silIndexes[base_ + j];
                        d = plane.Distance(tri_verts[i1].xyz);
                        if (d != 0) break; // even a small epsilon causes problems
                    }

                    if (j == 3)
                    {
                        // we can cull this sil edge
                        Unsafe.CopyBlock(&silEdges[i], &silEdges[i + 1], (uint)((numSilEdges - i - 1) * sizeof(SilEdge)));
                        c_coplanarCulled++;
                        numSilEdges--;
                        i--;
                    }
                }
                if (c_coplanarCulled != 0)
                {
                    c_coplanarSilEdges += c_coplanarCulled;
                    //common.Printf($"{c_coplanarCulled} of {c_coplanarCulled + numSilEdges} sil edges coplanar culled\n");
                }
            }
            c_totalSilEdges += numSilEdges;

            // sort the sil edges based on plane number
            UnsafeX.QuickSort(silEdges, numSilEdges, sizeof(SilEdge), (a, b) => SilEdgeSort((SilEdge*)a, (SilEdge*)b));

            // count up the distribution. a perfectly built model should only have shared edges, but most models will have some interpenetration and dangling edges
            shared = 0;
            single = 0;
            for (i = 0; i < numSilEdges; i++)
                if (silEdges[i].p2 == numPlanes) single++;
                else shared++;

            tri.perfectHull = single == 0;

            tri.numSilEdges = numSilEdges;
            tri.silEdges = triSilEdgeAllocator.Alloc(numSilEdges);
            fixed (SilEdge* silEdgesI = tri.silEdges) Unsafe.CopyBlock(silEdgesI, silEdges, (uint)(numSilEdges * sizeof(SilEdge)));
        }

        // Returns true if the texture polarity of the face is negative, false if it is positive or zero
        static bool R_FaceNegativePolarity(SrfTriangles tri, int firstIndex)
        {
            var tri_verts = tri.verts; var tri_indexes = tri.indexes;

            ref DrawVert a = ref tri_verts[tri_indexes[firstIndex + 0]];
            ref DrawVert b = ref tri_verts[tri_indexes[firstIndex + 1]];
            ref DrawVert c = ref tri_verts[tri_indexes[firstIndex + 2]];

            var d03 = b.st.x - a.st.x;
            var d04 = b.st.y - a.st.y;
            var d13 = c.st.x - a.st.x;
            var d14 = c.st.y - a.st.y;

            var area = d03 * d14 - d04 * d13;
            return area < 0f;
        }

        struct FaceTangents
        {
            public const int ALLOC16 = 1;
            public Vector3 tangents0;
            public Vector3 tangents1;
            public bool negativePolarity;
            public bool degenerate;
        }

        static void R_DeriveFaceTangents(SrfTriangles tri, FaceTangents* faceTangents)
        {
            var tri_verts = tri.verts; var tri_indexes = tri.indexes;

            // calculate tangent vectors for each face in isolation
            int c_positive = 0, c_negative = 0, c_textureDegenerateFaces = 0;

            float* d0 = stackalloc float[5], d1 = stackalloc float[5];
            for (var i = 0; i < tri.numIndexes; i += 3)
            {
                float area; Vector3 temp;

                ref FaceTangents ft = ref faceTangents[i / 3];

                ref DrawVert a = ref tri_verts[tri_indexes[i + 0]];
                ref DrawVert b = ref tri_verts[tri_indexes[i + 1]];
                ref DrawVert c = ref tri_verts[tri_indexes[i + 2]];

                d0[0] = b.xyz.x - a.xyz.x;
                d0[1] = b.xyz.y - a.xyz.y;
                d0[2] = b.xyz.z - a.xyz.z;
                d0[3] = b.st.x - a.st.x;
                d0[4] = b.st.y - a.st.y;

                d1[0] = c.xyz.x - a.xyz.x;
                d1[1] = c.xyz.y - a.xyz.y;
                d1[2] = c.xyz.z - a.xyz.z;
                d1[3] = c.st.x - a.st.x;
                d1[4] = c.st.y - a.st.y;

                area = d0[3] * d1[4] - d0[4] * d1[3];
                if ((float)Math.Abs(area) < 1e-20f)
                {
                    ft.negativePolarity = false;
                    ft.degenerate = true;
                    ft.tangents0.Zero();
                    ft.tangents1.Zero();
                    c_textureDegenerateFaces++;
                    continue;
                }
                if (area > 0f) { ft.negativePolarity = false; c_positive++; }
                else { ft.negativePolarity = true; c_negative++; }
                ft.degenerate = false;

#if USE_INVA
                var inva = area < 0f ? -1f : 1f;      // was = 1f / area;

                temp.x = (d0[0] * d1[4] - d0[4] * d1[0]) * inva;
                temp.y = (d0[1] * d1[4] - d0[4] * d1[1]) * inva;
                temp.z = (d0[2] * d1[4] - d0[4] * d1[2]) * inva;
                temp.Normalize();
                ft.tangents0 = temp;

                temp.x = (d0[3] * d1[0] - d0[0] * d1[3]) * inva;
                temp.y = (d0[3] * d1[1] - d0[1] * d1[3]) * inva;
                temp.z = (d0[3] * d1[2] - d0[2] * d1[3]) * inva;
                temp.Normalize();
                ft.tangents1 = temp;
#else
                temp.x = (d0[0] * d1[4] - d0[4] * d1[0]);
                temp.y = (d0[1] * d1[4] - d0[4] * d1[1]);
                temp.z = (d0[2] * d1[4] - d0[4] * d1[2]);
                temp.Normalize();
                ft.tangents0 = temp;

                temp.x = (d0[3] * d1[0] - d0[0] * d1[3]);
                temp.y = (d0[3] * d1[1] - d0[1] * d1[3]);
                temp.z = (d0[3] * d1[2] - d0[2] * d1[3]);
                temp.Normalize();
                ft.tangents1 = temp;
#endif
            }
        }

        // Modifies the surface to bust apart any verts that are shared by both positive and negative texture polarities, so tangent space smoothing at the vertex doesn't degenerate.
        // This will create some identical vertexes (which will eventually get different tangent vectors), so never optimize the resulting mesh, or it will get the mirrored edges back.
        // Reallocates tri.verts and changes tri.indexes in place Silindexes are unchanged by this.
        // sets mirroredVerts and mirroredVerts[]
        struct TangentVert
        {
            public const int ALLOC16 = 2;
            public bool polarityUsed0;
            public bool polarityUsed1;
            public int negativeRemap;
        }

        static void R_DuplicateMirroredVertexes(SrfTriangles tri)
        {
            var tri_verts = tri.verts; var tri_indexes = tri.indexes; var tri_mirroredVerts = tri.mirroredVerts;

            int i, j, totalVerts, numMirror;

            //GB Also protecting this as I think it has got too big
            //DG: windows only has a 1MB stack and it could happen that we try to allocate >1MB here (in lost mission mod, game/le_hell map), causing a stack overflow to prevent that, use heap allocation if it's >600KB
            var allocaSize = tri.numVerts * sizeof(TangentVert);
            var tvertsB = allocaSize < 600000 ? stackalloc TangentVert[allocaSize + TangentVert.ALLOC16] : new TangentVert[allocaSize + TangentVert.ALLOC16];
            fixed (TangentVert* tvertsT = tvertsB)
            {
                var tverts = (TangentVert*)_alloca16(tvertsT);
                Unsafe.InitBlock(tverts, 0, (uint)(tri.numVerts * sizeof(TangentVert)));

                // determine texture polarity of each surface

                // mark each vert with the polarities it uses
                for (i = 0; i < tri.numIndexes; i += 3)
                {
                    var polarity = R_FaceNegativePolarity(tri, i);
                    for (j = 0; j < 3; j++)
                        if (polarity) tverts[tri_indexes[i + j]].polarityUsed1 = true;
                        else tverts[tri_indexes[i + j]].polarityUsed0 = true;
                }

                // now create new verts as needed
                totalVerts = tri.numVerts;
                for (i = 0; i < tri.numVerts; i++)
                {
                    ref TangentVert vert = ref tverts[i];
                    if (vert.polarityUsed0 && vert.polarityUsed1) { vert.negativeRemap = totalVerts; totalVerts++; }
                }

                tri.numMirroredVerts = totalVerts - tri.numVerts;

                // now create the new list
                if (totalVerts == tri.numVerts) { tri.mirroredVerts = null; return; }

                tri.mirroredVerts = triMirroredVertAllocator.Alloc(tri.numMirroredVerts);

#if USE_TRI_DATA_ALLOCATOR
                tri.verts = triVertexAllocator.Resize(tri.verts, totalVerts);
#else
                var oldVerts = tri.verts;
                R_AllocStaticTriSurfVerts(tri, totalVerts);
                fixed (DrawVert* vertsA = tri.verts, vertsB = oldVerts) Unsafe.CopyBlock(vertsA, vertsB, (uint)(tri.numVerts * sizeof(DrawVert)));
                triVertexAllocator.Free(oldVerts);
#endif
                tri_verts = tri.verts;

                // create the duplicates
                numMirror = 0;
                for (i = 0; i < tri.numVerts; i++)
                {
                    j = tverts[i].negativeRemap;
                    if (j != 0) { tri_verts[j] = tri_verts[i]; tri_mirroredVerts[numMirror] = i; numMirror++; }
                }

                tri.numVerts = totalVerts;
                // change the indexes
                for (i = 0; i < tri.numIndexes; i++) if (tverts[tri_indexes[i]].negativeRemap != 0 && R_FaceNegativePolarity(tri, 3 * (i / 3))) tri_indexes[i] = tverts[tri_indexes[i]].negativeRemap;
                tri.numVerts = totalVerts;
            }
        }

        // Build texture space tangents for bump mapping
        // If a surface is deformed, this must be recalculated
        // This assumes that any mirrored vertexes have already been duplicated, so any shared vertexes will have the tangent spaces smoothed across.
        // Texture wrapping slightly complicates this, but as long as the normals are shared, and the tangent vectors are projected onto the normals, the separate vertexes should wind up with identical tangent spaces.
        // mirroring a normalmap WILL cause a slightly visible seam unless the normals are completely flat around the edge's full bilerp support.
        // Vertexes which are smooth shaded must have their tangent vectors in the same plane, which will allow a seamless rendering as long as the normal map is even on both sides of the seam.
        // A smooth shaded surface may have multiple tangent vectors at a vertex due to texture seams or mirroring, but it should only have a single normal vector.
        // Each triangle has a pair of tangent vectors in it's plane
        // Should we consider having vertexes point at shared tangent spaces to save space or speed transforms?
        // this version only handles bilateral symetry
        static void R_DeriveTangentsWithoutNormals(SrfTriangles tri)
        {
            int i, j;
            var tri_verts = tri.verts; var tri_indexes = tri.indexes;

            // DG: windows only has a 1MB stack and it could happen that we try to allocate >1MB here (in lost mission mod, game/le_hell map), causing a stack overflow to prevent that, use heap allocation if it's >600KB
            var allocaSize = sizeof(FaceTangents) * tri.numIndexes / 3;
            var faceTangentsB = allocaSize < 600000 ? stackalloc FaceTangents[allocaSize + FaceTangents.ALLOC16] : new FaceTangents[allocaSize + FaceTangents.ALLOC16];
            fixed (FaceTangents* faceTangentsF = faceTangentsB)
            {
                var faceTangents = (FaceTangents*)_alloca16(faceTangentsF);

                R_DeriveFaceTangents(tri, faceTangents);

                // clear the tangents
                for (i = 0; i < tri.numVerts; i++)
                {
                    tri_verts[i].tangents0.Zero();
                    tri_verts[i].tangents1.Zero();
                }

                // sum up the neighbors
                for (i = 0; i < tri.numIndexes; i += 3)
                {
                    ref FaceTangents ft = ref faceTangents[i / 3];

                    // for each vertex on this face
                    for (j = 0; j < 3; j++)
                    {
                        ref DrawVert vert = ref tri_verts[tri_indexes[i + j]];

                        vert.tangents0 += ft.tangents0;
                        vert.tangents1 += ft.tangents1;
                    }
                }

#if false
                // sum up both sides of the mirrored verts so the S vectors exactly mirror, and the T vectors are equal
                for (i = 0; i < tri.numMirroredVerts; i++)
                {
                    DrawVert v1, v2;

                    v1 = ref tri_verts[tri.numVerts - tri.numMirroredVerts + i];
                    v2 = ref tri_verts[tri_mirroredVerts[i]];

                    v1.tangents[0] -= v2.tangents[0];
                    v1.tangents[1] += v2.tangents[1];

                    v2.tangents[0] = Vector3.origin - v1.tangents[0];
                    v2.tangents[1] = v1.tangents[1];
                }
#endif

                // project the summed vectors onto the normal plane and normalize.  The tangent vectors will not necessarily be orthogonal to each other, but they will be orthogonal to the surface normal.
                for (i = 0; i < tri.numVerts; i++)
                {
                    ref DrawVert vert = ref tri_verts[i];
                    vert.tangents0 -= vert.tangents0 * vert.normal * vert.normal; vert.tangents0.Normalize();
                    vert.tangents1 -= vert.tangents1 * vert.normal * vert.normal; vert.tangents1.Normalize();
                }

                tri.tangentsCalculated = true;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void VectorNormalizeFast2(in Vector3 v, out Vector3 o)
        {
            var ilength = MathX.RSqrt(v[0] * v[0] + v[1] * v[1] + v[2] * v[2]);
            o.x = v[0] * ilength;
            o.y = v[1] * ilength;
            o.z = v[2] * ilength;
        }


        struct IndexSort
        {
            public int vertexNum;
            public int faceNum;
        }

        static int IndexSortCompar(IndexSort* a, IndexSort* b)
        {
            if (a->vertexNum < b->vertexNum) return -1;
            if (a->vertexNum > b->vertexNum) return 1;
            return 0;
        }

        // Find the largest triangle that uses each vertex
        static void R_BuildDominantTris(SrfTriangles tri)
        {
            int i, j;
            var tri_verts = tri.verts; var tri_indexes = tri.indexes;

            var ind = (IndexSort*)R_StaticAlloc(tri.numIndexes);

            for (i = 0; i < tri.numIndexes; i++) { ind[i].vertexNum = tri_indexes[i]; ind[i].faceNum = i / 3; }
            UnsafeX.QuickSort(ind, tri.numIndexes, sizeof(IndexSort), (a, b) => IndexSortCompar((IndexSort*)a, (IndexSort*)b));

            tri.dominantTris = triDominantTrisAllocator.Alloc(tri.numVerts);
            var dt = tri.dominantTris;
            fixed (DominantTri* dominantTrisD = dt) Unsafe.InitBlock(dominantTrisD, 0, (uint)(tri.numVerts * sizeof(DominantTri)));

            float* d0 = stackalloc float[5], d1 = stackalloc float[5];
            for (i = 0; i < tri.numIndexes; i += j)
            {
                var maxArea = 0f;
                var vertNum = ind[i].vertexNum;
                for (j = 0; i + j < tri.numIndexes && ind[i + j].vertexNum == vertNum; j++)
                {
                    Vector3 normal, tangent, bitangent;

                    int i1 = tri_indexes[ind[i + j].faceNum * 3 + 0];
                    int i2 = tri_indexes[ind[i + j].faceNum * 3 + 1];
                    int i3 = tri_indexes[ind[i + j].faceNum * 3 + 2];

                    ref DrawVert a = ref tri_verts[i1];
                    ref DrawVert b = ref tri_verts[i2];
                    ref DrawVert c = ref tri_verts[i3];

                    d0[0] = b.xyz.x - a.xyz.x;
                    d0[1] = b.xyz.y - a.xyz.y;
                    d0[2] = b.xyz.z - a.xyz.z;
                    d0[3] = b.st.x - a.st.x;
                    d0[4] = b.st.y - a.st.y;

                    d1[0] = c.xyz.x - a.xyz.x;
                    d1[1] = c.xyz.y - a.xyz.y;
                    d1[2] = c.xyz.z - a.xyz.z;
                    d1[3] = c.st.x - a.st.x;
                    d1[4] = c.st.y - a.st.y;

                    normal.x = d1[1] * d0[2] - d1[2] * d0[1];
                    normal.y = d1[2] * d0[0] - d1[0] * d0[2];
                    normal.z = d1[0] * d0[1] - d1[1] * d0[0];

                    var area = normal.Length;

                    // if this is smaller than what we already have, skip it
                    if (area < maxArea) continue;
                    maxArea = area;

                    if (i1 == vertNum) { dt[vertNum].v2 = i2; dt[vertNum].v3 = i3; }
                    else if (i2 == vertNum) { dt[vertNum].v2 = i3; dt[vertNum].v3 = i1; }
                    else { dt[vertNum].v2 = i1; dt[vertNum].v3 = i2; }
                    var len = area;
                    if (len < 0.001f) len = 0.001f;
                    dt[vertNum].normalizationScale[2] = 1f / len;     // normal

                    // texture area
                    area = d0[3] * d1[4] - d0[4] * d1[3];

                    tangent.x = d0[0] * d1[4] - d0[4] * d1[0];
                    tangent.y = d0[1] * d1[4] - d0[4] * d1[1];
                    tangent.z = d0[2] * d1[4] - d0[4] * d1[2];
                    len = tangent.Length;
                    if (len < 0.001f) len = 0.001f;
                    dt[vertNum].normalizationScale[0] = (area > 0 ? 1 : -1) / len;  // tangents[0]

                    bitangent.x = (d0[3] * d1[0] - d0[0] * d1[3]);
                    bitangent.y = (d0[3] * d1[1] - d0[1] * d1[3]);
                    bitangent.z = (d0[3] * d1[2] - d0[2] * d1[3]);
                    len = bitangent.Length;
                    if (len < 0.001f) len = 0.001f;
#if DERIVE_UNSMOOTHED_BITANGENT
                    dt[vertNum].normalizationScale[1] = (area > 0 ? 1 : -1);
#else
                    dt[vertNum].normalizationScale[1] = (area > 0 ? 1 : -1) / len;  // tangents[1]
#endif
                }
            }

            R_StaticFree(ind);
        }

        // Uses the single largest area triangle for each vertex, instead of smoothing over all
        static void R_DeriveUnsmoothedTangents(SrfTriangles tri)
        {
            if (tri.tangentsCalculated) return;

#if true
            fixed (DrawVert* verts = tri.verts)
            fixed (DominantTri* dominantTris = tri.dominantTris)
                Simd.DeriveUnsmoothedTangents(verts, dominantTris, tri.numVerts);
#else
            var tri_verts = tri.verts.Value; var tri_dominantTris = tri.dominantTris.Value;
            float* d0 = stackalloc float[5], d1 = stackalloc float[5];
            for (var i = 0; i < tri.numVerts; i++)
            {
                Vector3 temp;
                ref DominantTri dt = ref tri_dominantTris[i];

                ref DrawVert a = ref tri_verts[i];
                ref DrawVert b = ref tri_verts[dt.v2];
                ref DrawVert c = ref tri_verts[dt.v3];

                d0[0] = b.xyz.x - a.xyz.z;
                d0[1] = b.xyz.y - a.xyz.y;
                d0[2] = b.xyz.z - a.xyz.z;
                d0[3] = b.st.x - a.st.x;
                d0[4] = b.st.y - a.st.y;

                d1[0] = c.xyz.x - a.xyz.x;
                d1[1] = c.xyz.y - a.xyz.y;
                d1[2] = c.xyz.z - a.xyz.z;
                d1[3] = c.st.x - a.st.x;
                d1[4] = c.st.y - a.st.y;

                a.normal.x = dt.normalizationScale[2] * (d1[1] * d0[2] - d1[2] * d0[1]);
                a.normal.y = dt.normalizationScale[2] * (d1[2] * d0[0] - d1[0] * d0[2]);
                a.normal.z = dt.normalizationScale[2] * (d1[0] * d0[1] - d1[1] * d0[0]);

                a.tangents0.x = dt.normalizationScale[0] * (d0[0] * d1[4] - d0[4] * d1[0]);
                a.tangents0.y = dt.normalizationScale[0] * (d0[1] * d1[4] - d0[4] * d1[1]);
                a.tangents0.z = dt.normalizationScale[0] * (d0[2] * d1[4] - d0[4] * d1[2]);

#if DERIVE_UNSMOOTHED_BITANGENT
                // derive the bitangent for a completely orthogonal axis, instead of using the texture T vector
                a.tangents1.x = dt.normalizationScale[1] * (a.normal.z * a.tangents0.y - a.normal.y * a.tangents0.z);
                a.tangents1.y = dt.normalizationScale[1] * (a.normal.x * a.tangents0.z - a.normal.z * a.tangents0.x);
                a.tangents1.z = dt.normalizationScale[1] * (a.normal.y * a.tangents0.x - a.normal.x * a.tangents0.y);
#else
                // calculate the bitangent from the texture T vector
                a.tangents1.x = dt.normalizationScale[1] * (d0[3] * d1[0] - d0[0] * d1[3]);
                a.tangents1.y = dt.normalizationScale[1] * (d0[3] * d1[1] - d0[1] * d1[3]);
                a.tangents1.z = dt.normalizationScale[1] * (d0[3] * d1[2] - d0[2] * d1[3]);
#endif
            }

#endif
            tri.tangentsCalculated = true;
        }

        // This is called once for static surfaces, and every frame for deforming surfaces. Builds tangents, normals, and face planes.
        public static void R_DeriveTangents(SrfTriangles tri, bool allocFacePlanes = true)
        {
            int i;
            var tri_verts = tri.verts; var tri_indexes = tri.indexes;

            if (tri.dominantTris != null) { R_DeriveUnsmoothedTangents(tri); return; }
            if (tri.tangentsCalculated) return;

            tr.pc.c_tangentIndexes += tri.numIndexes;

            if (tri.facePlanes == null && allocFacePlanes) R_AllocStaticTriSurfPlanes(tri, tri.numIndexes);

#if true
            var planesB = tri.facePlanes ?? stackalloc Plane[(tri.numIndexes / 3) + Plane.ALLOC16];
            fixed (Plane* planesP = planesB)
            {
                var planes = (Plane*)_alloca16(planesP);

                fixed (DrawVert* vertsV = tri_verts)
                fixed (GlIndex* indexesG = tri_indexes)
                    Simd.DeriveTangentsi(planes, vertsV, tri.numVerts, indexesG, tri.numIndexes);
            }
#else
            var planesB = tri.facePlanes?.Value
            for (i = 0; i < tri.numVerts; i++)
            {
                tri_verts[i].normal.Zero();
                tri_verts[i].tangents0.Zero();
                tri_verts[i].tangents1.Zero();
            }

            float* d0 = stackalloc float[5], d1 = stackalloc float[5];
            for (i = 0; i < tri.numIndexes; i += 3)
            {
                // make face tangents
                ref DrawVert a = ref tri_verts[tri_indexes[i + 0]];
                ref DrawVert b = ref tri_verts[tri_indexes[i + 1]];
                ref DrawVert c = ref tri_verts[tri_indexes[i + 2]];

                d0[0] = b.xyz.x - a.xyz.x;
                d0[1] = b.xyz.y - a.xyz.y;
                d0[2] = b.xyz.z - a.xyz.z;
                d0[3] = b.st.x - a.st.x;
                d0[4] = b.st.y - a.st.y;

                d1[0] = c.xyz.x - a.xyz.x;
                d1[1] = c.xyz.y - a.xyz.y;
                d1[2] = c.xyz.z - a.xyz.z;
                d1[3] = c.st.x - a.st.x;
                d1[4] = c.st.y - a.st.y;

                // normal
                Vector3 temp;
                temp.x = d1[1] * d0[2] - d1[2] * d0[1];
                temp.y = d1[2] * d0[0] - d1[0] * d0[2];
                temp.z = d1[0] * d0[1] - d1[1] * d0[0];
                VectorNormalizeFast2(temp, out var normal);

#if USE_INVA
                var area = d0[3] * d1[4] - d0[4] * d1[3];
                var inva = area < 0f ? -1f : 1f;      // was = 1f / area;

                temp.x = (d0[0] * d1[4] - d0[4] * d1[0]) * inva;
                temp.y = (d0[1] * d1[4] - d0[4] * d1[1]) * inva;
                temp.z = (d0[2] * d1[4] - d0[4] * d1[2]) * inva;
                VectorNormalizeFast2(temp, out var tangents0);

                temp.x = (d0[3] * d1[0] - d0[0] * d1[3]) * inva;
                temp.y = (d0[3] * d1[1] - d0[1] * d1[3]) * inva;
                temp.z = (d0[3] * d1[2] - d0[2] * d1[3]) * inva;
                VectorNormalizeFast2(temp, out var tangents1);
#else
                temp.x = d0[0] * d1[4] - d0[4] * d1[0];
                temp.y = d0[1] * d1[4] - d0[4] * d1[1];
                temp.z = d0[2] * d1[4] - d0[4] * d1[2];
                VectorNormalizeFast2(temp, out var tangents0);

                temp.x = d0[3] * d1[0] - d0[0] * d1[3];
                temp.y = d0[3] * d1[1] - d0[1] * d1[3];
                temp.z = d0[3] * d1[2] - d0[2] * d1[3];
                VectorNormalizeFast2(temp, out var tangents1);
#endif

                // sum up the tangents and normals for each vertex on this face
                for (var j = 0; j < 3; j++)
                {
                    ref DrawVert vert = ref tri_verts[tri_indexes[i + j]];
                    vert.normal += normal;
                    vert.tangents0 += tangents0;
                    vert.tangents1 += tangents1;
                }

                if (planes != null)
                {
                    planes->Normal = normal;
                    planes->FitThroughPoint(a.xyz);
                    planes++;
                }
            }
#endif

#if false
            if (tri_silIndexes != null)
            {
                for (i = 0; i < tri.numVerts; i++) tri_verts[i].normal.Zero();
                for (i = 0; i < tri.numIndexes; i++) tri_verts[tri_silIndexes[i]].normal += planes[i / 3].Normal();
                for (i = 0; i < tri.numIndexes; i++) tri_verts[tri_indexes[i]].normal = tri.verts[tri_silIndexes[i]].normal;
            }
#else
            var tri_dupVerts = tri.dupVerts;

            // add the normal of a duplicated vertex to the normal of the first vertex with the same XYZ
            for (i = 0; i < tri.numDupVerts; i++) tri_verts[tri_dupVerts[i * 2 + 0]].normal += tri_verts[tri_dupVerts[i * 2 + 1]].normal;

            // copy vertex normals to duplicated vertices
            for (i = 0; i < tri.numDupVerts; i++) tri_verts[tri_dupVerts[i * 2 + 1]].normal = tri_verts[tri_dupVerts[i * 2 + 0]].normal;
#endif

#if false
            var tri_mirroredVerts = tri.mirroredVerts.Value;

            // sum up both sides of the mirrored verts so the S vectors exactly mirror, and the T vectors are equal
            for (i = 0; i < tri.numMirroredVerts; i++)
            {
                ref DrawVert v1 = ref tri_verts[tri.numVerts - tri.numMirroredVerts + i];
                ref DrawVert v2 = ref tri_verts[tri_mirroredVerts[i]];

                v1.tangents0 -= v2.tangents0;
                v1.tangents1 += v2.tangents1;

                v2.tangents0 = Vector3.origin - v1.tangents0;
                v2.tangents1 = v1.tangents1;
            }
#endif

            // project the summed vectors onto the normal plane and normalize.  The tangent vectors will not necessarily be orthogonal to each other, but they will be orthogonal to the surface normal.
#if true
            fixed (DrawVert* vertsD = tri_verts) Simd.NormalizeTangents(vertsD, tri.numVerts);
#else
            for (i = 0; i < tri.numVerts; i++)
            {
                ref DrawVert vert = ref tri.verts[i];

                VectorNormalizeFast2(vert.normal, vert.normal);

                // project the tangent vectors
                for (int j = 0; j < 2; j++)
                {
                    float d;

                    d = vert.tangents[j] * vert.normal;
                    vert.tangents[j] = vert.tangents[j] - d * vert.normal;
                    VectorNormalizeFast2(vert.tangents[j], vert.tangents[j]);
                }
            }
#endif
            tri.tangentsCalculated = true;
            tri.facePlanesCalculated = true;
        }

        // silIndexes must have already been calculated.
        // silIndexes are used instead of indexes, because duplicated triangles could have different texture coordinates.
        public static void R_RemoveDuplicatedTriangles(SrfTriangles tri)
        {
            int i, j, r, a, b, c, c_removed;
            var tri_indexes = tri.indexes; var tri_silIndexes = tri.silIndexes;

            c_removed = 0;

            // check for completely duplicated triangles any rotation of the triangle is still the same, but a mirroring is considered different
            for (i = 0; i < tri.numIndexes; i += 3)
            {
                for (r = 0; r < 3; r++)
                {
                    a = tri_silIndexes[i + r];
                    b = tri_silIndexes[i + (r + 1) % 3];
                    c = tri_silIndexes[i + (r + 2) % 3];
                    for (j = i + 3; j < tri.numIndexes; j += 3)
                    {
                        if (tri_silIndexes[j] == a && tri_silIndexes[j + 1] == b && tri_silIndexes[j + 2] == c)
                        {
                            c_removed++;
                            fixed (GlIndex* indexesG = tri_indexes) Unsafe.CopyBlock(indexesG + j, indexesG + j + 3, (uint)((tri.numIndexes - j - 3) * sizeof(GlIndex)));
                            fixed (GlIndex* silIndexesG = tri_silIndexes) Unsafe.CopyBlock(silIndexesG + j, silIndexesG + j + 3, (uint)((tri.numIndexes - j - 3) * sizeof(GlIndex)));
                            tri.numIndexes -= 3;
                            j -= 3;
                        }
                    }
                }
            }

            if (c_removed != 0) common.Printf($"removed {c_removed} duplicated triangles\n");
        }

        // silIndexes must have already been calculated
        public static void R_RemoveDegenerateTriangles(SrfTriangles tri)
        {
            int i, a, b, c, c_removed;
            var tri_indexes = tri.indexes; var tri_silIndexes = tri.silIndexes;

            // check for completely degenerate triangles
            c_removed = 0;
            for (i = 0; i < tri.numIndexes; i += 3)
            {
                a = tri_silIndexes[i];
                b = tri_silIndexes[i + 1];
                c = tri_silIndexes[i + 2];
                if (a == b || a == c || b == c)
                {
                    c_removed++;
                    fixed (GlIndex* indexesG = tri_indexes) Unsafe.CopyBlock(indexesG + i, indexesG + i + 3, (uint)((tri.numIndexes - i - 3) * sizeof(GlIndex)));
                    if (tri_silIndexes != null) fixed (GlIndex* silIndexesG = tri_silIndexes) Unsafe.CopyBlock(silIndexesG + i, silIndexesG + i + 3, (uint)((tri.numIndexes - i - 3) * sizeof(GlIndex)));
                    tri.numIndexes -= 3;
                    i -= 3;
                }
            }

            // this doesn't free the memory used by the unused verts
            if (c_removed != 0) common.Printf($"removed {c_removed} degenerate triangles\n");
        }

        static void R_TestDegenerateTextureSpace(SrfTriangles tri)
        {
            int i, c_degenerate;
            var tri_verts = tri.verts; var tri_indexes = tri.indexes;

            // check for triangles with a degenerate texture space
            c_degenerate = 0;
            for (i = 0; i < tri.numIndexes; i += 3)
            {
                ref DrawVert a = ref tri_verts[tri_indexes[i + 0]];
                ref DrawVert b = ref tri_verts[tri_indexes[i + 1]];
                ref DrawVert c = ref tri_verts[tri_indexes[i + 2]];

                if (a.st == b.st || b.st == c.st || c.st == a.st) c_degenerate++;
            }

            //if (c_degenerate != 0) common.Printf($"{c_degenerate} triangles with a degenerate texture space\n",  );
        }

        public static void R_RemoveUnusedVerts(SrfTriangles tri)
        {
            int i, index, used;
            var tri_verts = tri.verts; var tri_indexes = tri.indexes; var tri_silIndexes = tri.silIndexes;

            var mark = (int*)R_ClearedStaticAlloc(tri.numVerts * sizeof(int));

            for (i = 0; i < tri.numIndexes; i++)
            {
                index = tri_indexes[i];
                if (index < 0 || index >= tri.numVerts) common.Error("R_RemoveUnusedVerts: bad index");
                mark[index] = 1;

                if (tri.silIndexes != null)
                {
                    index = tri_silIndexes[i];
                    if (index < 0 || index >= tri.numVerts) common.Error("R_RemoveUnusedVerts: bad index");
                    mark[index] = 1;
                }
            }

            used = 0;
            for (i = 0; i < tri.numVerts; i++)
            {
                if (mark[i] == 0) continue;
                mark[i] = used + 1;
                used++;
            }

            if (used != tri.numVerts)
            {
                for (i = 0; i < tri.numIndexes; i++)
                {
                    tri_indexes[i] = mark[tri_indexes[i]] - 1;
                    if (tri_silIndexes != null) tri_silIndexes[i] = mark[tri_silIndexes[i]] - 1;
                }
                tri.numVerts = used;

                for (i = 0; i < tri.numVerts; i++)
                {
                    index = mark[i];
                    if (index == 0) continue;
                    tri_verts[index - 1] = tri_verts[i];
                }

                // this doesn't realloc the arrays to save the memory used by the unused verts
            }

            R_StaticFree(mark);
        }

        // Only deals with vertexes and indexes, not silhouettes, planes, etc. Does NOT perform a cleanup triangles, so there may be duplicated verts in the result.
        public static SrfTriangles R_MergeSurfaceList(SrfTriangles[] surfaces, int numSurfaces)
        {
            int i, j, totalVerts, totalIndexes; SrfTriangles newTri, tri;

            totalVerts = 0; totalIndexes = 0;
            for (i = 0; i < numSurfaces; i++) { totalVerts += surfaces[i].numVerts; totalIndexes += surfaces[i].numIndexes; }

            newTri = R_AllocStaticTriSurf();
            newTri.numVerts = totalVerts;
            newTri.numIndexes = totalIndexes;
            R_AllocStaticTriSurfVerts(newTri, newTri.numVerts);
            R_AllocStaticTriSurfIndexes(newTri, newTri.numIndexes);
            var newTri_indexes = newTri.indexes;

            totalVerts = 0; totalIndexes = 0;
            for (i = 0; i < numSurfaces; i++)
            {
                tri = surfaces[i];
                var tri_indexes = tri.indexes;
                fixed (DrawVert* vertsA = newTri.verts, vertsB = tri.verts) Unsafe.CopyBlock(vertsA + totalVerts, vertsB, (uint)(tri.numVerts * sizeof(DrawVert)));
                for (j = 0; j < tri.numIndexes; j++) newTri_indexes[totalIndexes + j] = totalVerts + tri_indexes[j];
                totalVerts += tri.numVerts;
                totalIndexes += tri.numIndexes;
            }

            return newTri;
        }

        // Only deals with vertexes and indexes, not silhouettes, planes, etc. Does NOT perform a cleanup triangles, so there may be duplicated verts in the result.
        public static SrfTriangles R_MergeTriangles(SrfTriangles tri1, SrfTriangles tri2)
            => R_MergeSurfaceList(new[] { tri1, tri2 }, 2);

        // Lit two sided surfaces need to have the triangles actually duplicated, they can't just turn on two sided lighting, because the normal and tangents are wrong on the other sides.
        // This should be called before R_CleanupTriangles
        public static void R_ReverseTriangles(SrfTriangles tri)
        {
            int i;
            var tri_verts = tri.verts; var tri_indexes = tri.indexes;

            // flip the normal on each vertex. If the surface is going to have generated normals, this won't matter, but if it has explicit normals, this will keep it on the correct side
            for (i = 0; i < tri.numVerts; i++) tri_verts[i].normal = Vector3.origin - tri_verts[i].normal;

            // flip the index order to make them back sided
            for (i = 0; i < tri.numIndexes; i += 3)
            {
                var temp = tri_indexes[i + 0];
                tri_indexes[i + 0] = tri_indexes[i + 1];
                tri_indexes[i + 1] = temp;
            }
        }

        // FIXME: allow createFlat and createSmooth normals, as well as explicit
        public static void R_CleanupTriangles(SrfTriangles tri, bool createNormals, bool identifySilEdges, bool useUnsmoothedTangents)
        {
            R_RangeCheckIndexes(tri);
            R_CreateSilIndexes(tri);
            //R_RemoveDuplicatedTriangles(tri); // this may remove valid overlapped transparent triangles
            R_RemoveDegenerateTriangles(tri);
            R_TestDegenerateTextureSpace(tri);
            //R_RemoveUnusedVerts(tri);
            if (identifySilEdges) R_IdentifySilEdges(tri, true);  // assume it is non-deformable, and omit coplanar edges
            // bust vertexes that share a mirrored edge into separate vertexes
            R_DuplicateMirroredVertexes(tri);
            // optimize the index order (not working?)
            //R_OrderIndexes(tri.numIndexes, tri.indexes);
            R_CreateDupVerts(tri);
            R_BoundTriSurf(tri);
            if (useUnsmoothedTangents) { R_BuildDominantTris(tri); R_DeriveUnsmoothedTangents(tri); }
            else if (!createNormals) { R_DeriveFacePlanes(tri); R_DeriveTangentsWithoutNormals(tri); }
            else R_DeriveTangents(tri);
        }

        #region DEFORMED SURFACES

        // deformable meshes precalculate as much as possible from a base frame, then generate complete srfTriangles_t from just a new set of vertexes
        public class DeformInfo
        {
            public static int sizeOf = Marshal.SizeOf<DeformInfo>();

            public int numSourceVerts;

            // numOutputVerts may be smaller if the input had duplicated or degenerate triangles it will often be larger if the input had mirrored texture seams that needed to be busted for proper tangent spaces
            public int numOutputVerts;
            public DrawVert[] verts;

            public int numMirroredVerts;
            public int[] mirroredVerts;

            public int numIndexes;
            public GlIndex[] indexes;

            public GlIndex[] silIndexes;

            public int numDupVerts;
            public int[] dupVerts;

            public int numSilEdges;
            public SilEdge[] silEdges;

            public DominantTri[] dominantTris;
        }

        public static DeformInfo R_BuildDeformInfo(int numVerts, DrawVert* verts, int numIndexes, int[] indexes, bool useUnsmoothedTangents)
        {
            DeformInfo deform; SrfTriangles tri = new();
            int i;

            tri.numVerts = numVerts;
            R_AllocStaticTriSurfVerts(tri, tri.numVerts);
            fixed (DrawVert* vertsD = tri.verts) Simd.Memcpy(vertsD, verts, tri.numVerts * sizeof(DrawVert));

            tri.numIndexes = numIndexes;
            R_AllocStaticTriSurfIndexes(tri, tri.numIndexes);
            var tri_indexes = tri.indexes;

            // don't memcpy, so we can change the index type from int to short without changing the interface
            for (i = 0; i < tri.numIndexes; i++) tri_indexes[i] = indexes[i];

            R_RangeCheckIndexes(tri);
            R_CreateSilIndexes(tri);

            // should we order the indexes here?
            //R_RemoveDuplicatedTriangles(tri);
            //R_RemoveDegenerateTriangles(tri);
            //R_RemoveUnusedVerts(tri);
            R_IdentifySilEdges(tri, false); // we cannot remove coplanar edges, because they can deform to silhouettes

            R_DuplicateMirroredVertexes(tri); // split mirror points into multiple points

            R_CreateDupVerts(tri);

            if (useUnsmoothedTangents) R_BuildDominantTris(tri);

            deform = R_ClearedStaticAllocT<DeformInfo>();

            deform.numSourceVerts = numVerts;
            deform.numOutputVerts = tri.numVerts;
            deform.verts = tri.verts;

            deform.numIndexes = numIndexes;
            deform.indexes = tri.indexes;

            deform.silIndexes = tri.silIndexes;

            deform.numSilEdges = tri.numSilEdges;
            deform.silEdges = tri.silEdges;

            deform.dominantTris = tri.dominantTris;

            deform.numMirroredVerts = tri.numMirroredVerts;
            deform.mirroredVerts = tri.mirroredVerts;

            deform.numDupVerts = tri.numDupVerts;
            deform.dupVerts = tri.dupVerts;

            if (tri.verts != null) triVertexAllocator.Free(tri.verts);
            if (tri.facePlanes != null) triPlaneAllocator.Free(tri.facePlanes);

            return deform;
        }

        public static void R_FreeDeformInfo(DeformInfo deformInfo)
        {
            if (deformInfo.indexes != null) triIndexAllocator.Free(deformInfo.indexes);
            if (deformInfo.silIndexes != null) triSilIndexAllocator.Free(deformInfo.silIndexes);
            if (deformInfo.silEdges != null) triSilEdgeAllocator.Free(deformInfo.silEdges);
            if (deformInfo.dominantTris != null) triDominantTrisAllocator.Free(deformInfo.dominantTris);
            if (deformInfo.mirroredVerts != null) triMirroredVertAllocator.Free(deformInfo.mirroredVerts);
            if (deformInfo.dupVerts != null) triDupVertAllocator.Free(deformInfo.dupVerts);
            R_StaticFreeT(ref deformInfo);
        }

        public static int R_DeformInfoMemoryUsed(DeformInfo deformInfo)
        {
            var total = 0;
            if (deformInfo.indexes != null) total += deformInfo.numIndexes * sizeof(GlIndex);
            if (deformInfo.silIndexes != null) total += deformInfo.numIndexes * sizeof(GlIndex);
            if (deformInfo.silEdges != null) total += deformInfo.numSilEdges * sizeof(SilEdge);
            if (deformInfo.dominantTris != null) total += deformInfo.numSourceVerts * sizeof(DominantTri);
            if (deformInfo.mirroredVerts != null) total += deformInfo.numMirroredVerts * sizeof(int);
            if (deformInfo.dupVerts != null) total += deformInfo.numDupVerts * sizeof(int);
            total += DeformInfo.sizeOf;
            return total;
        }

        #endregion
    }
}
