using GlIndex = System.Int32;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static System.NumericsX.OpenStack.Gngine.Gngine;
using static System.NumericsX.OpenStack.Gngine.Render.R;
using static System.NumericsX.OpenStack.OpenStack;
using static System.NumericsX.Platform;

namespace System.NumericsX.OpenStack.Gngine.Render
{
    unsafe partial class TR
    {
        static int c_turboUsedVerts, c_turboUnusedVerts;

        // are dangling edges that are outside the light frustum still making planes?
        public static SrfTriangles R_CreateVertexProgramTurboShadowVolume(IRenderEntity ent, SrfTriangles tri, IRenderLight light, ref SrfCullInfo cullInfo)
        {
            int i, j; SrfTriangles newTri; GlIndex[] indexes; byte* facing;
            var tri_indexes = tri.indexes; var tri_silEdges = tri.silEdges;

            R_CalcInteractionFacing(ent, tri, light, ref cullInfo);

            if (r_useShadowProjectedCull.Bool) R_CalcInteractionCullBits(ent, tri, light, ref cullInfo);

            var numFaces = tri.numIndexes / 3;
            var numShadowingFaces = 0;
            facing = cullInfo.facing;

            // if all the triangles are inside the light frustum
            if (cullInfo.cullBits == Interaction.LIGHT_CULL_ALL_FRONT || !r_useShadowProjectedCull.Bool)
            {
                // count the number of shadowing faces
                for (i = 0; i < numFaces; i++) numShadowingFaces += facing[i];
                numShadowingFaces = numFaces - numShadowingFaces;
            }
            else
            {
                // make all triangles that are outside the light frustum "facing", so they won't cast shadows
                indexes = tri_indexes;
                var modifyFacing = cullInfo.facing;
                var cullBits = cullInfo.cullBits;

                for (j = i = 0; i < tri.numIndexes; i += 3, j++)
                    if (modifyFacing[j] == 0)
                    {
                        var i1 = indexes[i + 0];
                        var i2 = indexes[i + 1];
                        var i3 = indexes[i + 2];

                        if ((cullBits[i1] & cullBits[i2] & cullBits[i3]) != 0) modifyFacing[j] = 1;
                        else numShadowingFaces++;
                    }
            }

            // no faces are inside the light frustum and still facing the right way
            if (numShadowingFaces == 0) return null;

            // shadowVerts will be NULL on these surfaces, so the shadowVerts will be taken from the ambient surface
            newTri = R_AllocStaticTriSurf();

            newTri.numVerts = tri.numVerts * 2;

            // alloc the max possible size
#if USE_TRI_DATA_ALLOCATOR
            R_AllocStaticTriSurfIndexes(newTri, (numShadowingFaces + tri.numSilEdges) * 6);
            GlIndex tempIndexes = newTri.indexes;
            GlIndex shadowIndexes = newTri.indexes;
#else
            GlIndex* tempIndexes = stackalloc GlIndex[tri.numSilEdges * 6 + intX.ALLOC16]; tempIndexes = (GlIndex*)_alloca16(tempIndexes);
            GlIndex* shadowIndexes = tempIndexes;
#endif

            // create new triangles along sil planes
            SilEdge sil;
            for (sil = tri_silEdges, i = tri.numSilEdges; i > 0; i--, sil++)
            {
                int f1 = facing[sil.p1], f2 = facing[sil.p2];

                if ((f1 ^ f2) == 0) continue;

                int v1 = sil.v1 << 1, v2 = sil.v2 << 1;

                // set the two triangle winding orders based on facing without using a poorly-predictable branch

                shadowIndexes[0] = v1;
                shadowIndexes[1] = v2 ^ f1;
                shadowIndexes[2] = v2 ^ f2;
                shadowIndexes[3] = v1 ^ f2;
                shadowIndexes[4] = v1 ^ f1;
                shadowIndexes[5] = v2 ^ 1;

                shadowIndexes += 6;
            }

            int numShadowIndexes = shadowIndexes - tempIndexes;

            // we aren't bothering to separate front and back caps on these
            newTri.numIndexes = newTri.numShadowIndexesNoFrontCaps = numShadowIndexes + numShadowingFaces * 6;
            newTri.numShadowIndexesNoCaps = numShadowIndexes;
            newTri.shadowCapPlaneBits = SHADOW_CAP_INFINITE;

#if USE_TRI_DATA_ALLOCATOR
            // decrease the size of the memory block to only store the used indexes
            R_ResizeStaticTriSurfIndexes(newTri, newTri.numIndexes);
#else
            // allocate memory for the indexes
            R_AllocStaticTriSurfIndexes(newTri, newTri.numIndexes);
            // copy the indexes we created for the sil planes
            Simd.Memcpy(newTri.indexes, tempIndexes, numShadowIndexes * sizeof(GlIndex));
#endif

            // these have no effect, because they extend to infinity
            newTri.bounds.Clear();

            // put some faces on the model and some on the distant projection
            indexes = tri_indexes;
            shadowIndexes = newTri.indexes + numShadowIndexes;

            for (i = 0, j = 0; i < tri.numIndexes; i += 3, j++)
            {
                if (facing[j] != 0) continue;

                var i0 = indexes[i + 0] << 1; shadowIndexes[2] = i0; shadowIndexes[3] = i0 ^ 1;
                var i1 = indexes[i + 1] << 1; shadowIndexes[1] = i1; shadowIndexes[4] = i1 ^ 1;
                var i2 = indexes[i + 2] << 1; shadowIndexes[0] = i2; shadowIndexes[5] = i2 ^ 1;
                shadowIndexes += 6;
            }

            return newTri;
        }
    }
}