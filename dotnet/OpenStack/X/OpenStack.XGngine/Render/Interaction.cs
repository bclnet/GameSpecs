using static System.NumericsX.OpenStack.Gngine.Gngine;
using static System.NumericsX.OpenStack.Gngine.Render.R;
using static System.NumericsX.OpenStack.OpenStack;
using static System.NumericsX.Platform;
using GlIndex = System.Int32;

namespace System.NumericsX.OpenStack.Gngine.Render
{
    public unsafe struct SrfCullInfo
    {
        // For each triangle a byte set to 1 if facing the light origin.
        public byte* facing;
        // For each vertex a byte with the bits [0-5] set if the vertex is at the back side of the corresponding clip plane. If the 'cullBits' pointer equals LIGHT_CULL_ALL_FRONT all vertices are at the front of all the clip planes.
        public byte* cullBits;
        // Clip planes in surface space used to calculate the cull bits.
        public Plane[] localClipPlanes;
    }

    public class SurfaceInteraction
    {
        // if lightTris == LIGHT_TRIS_DEFERRED, then the calculation of the lightTris has been deferred, and must be done if ambientTris is visible
        public SrfTriangles lightTris;
        // shadow volume triangle surface
        public SrfTriangles shadowTris;
        // so we can check ambientViewCount before adding lightTris, and get at the shared vertex and possibly shadowVertex caches
        public SrfTriangles ambientTris;
        public Material shader;
        public int expCulled;          // only for the experimental shadow buffer renderer
        public SrfCullInfo cullInfo;
    }

    public class AreaNumRef : BlockAllocElement<AreaNumRef>
    {
        public AreaNumRef next;
        public int areaNum;
    }

    public unsafe partial class IInteraction : BlockAllocElement<IInteraction>
    {
        public static readonly SrfTriangles LIGHT_TRIS_DEFERRED = new();
        public static readonly byte* LIGHT_CULL_ALL_FRONT = (byte*)(new IntPtr(-1));
        public const float LIGHT_CLIP_EPSILON = 0.1f;

        // this may be 0 if the light and entity do not actually intersect -1 = an untested interaction
        public int numSurfaces;

        // if there is a whole-entity optimized shadow hull, it will
        // be present as a surfaceInteraction_t with a null ambientTris, but
        // possibly having a shader to specify the shadow sorting order
        public SurfaceInteraction[] surfaces;

        // get space from here, if null, it is a pre-generated shadow volume from dmap
        public IRenderEntity entityDef;
        public IRenderLight lightDef;

        public IInteraction lightNext;               // for lightDef chains
        public IInteraction lightPrev;
        public IInteraction entityNext;              // for entityDef chains
        public IInteraction entityPrev;

        protected enum FrustumState
        {
            FRUSTUM_UNINITIALIZED,
            FRUSTUM_INVALID,
            FRUSTUM_VALID,
            FRUSTUM_VALIDAREAS,
        }

        protected FrustumState frustumState;
        protected Frustum frustum;              // frustum which contains the interaction
        protected AreaNumRef frustumAreas;         // numbers of the areas the frustum touches
        protected int dynamicModelFrameCount; // so we can tell if a callback model animated

        public IInteraction()
        {
            numSurfaces = 0;
            surfaces = null;
            entityDef = null;
            lightDef = null;
            lightNext = null;
            lightPrev = null;
            entityNext = null;
            entityPrev = null;
            dynamicModelFrameCount = 0;
            frustumState = FrustumState.FRUSTUM_UNINITIALIZED;
            frustumAreas = null;
        }

        public static IInteraction AllocAndLink(IRenderEntity edef, IRenderLight ldef)
        {
            if (edef == null || ldef == null) common.Error("Interaction::AllocAndLink: null parm");

            var renderWorld = edef.world;

            var interaction = renderWorld.interactionAllocator.Alloc();

            // link and initialize
            interaction.dynamicModelFrameCount = 0;

            interaction.lightDef = ldef;
            interaction.entityDef = edef;

            interaction.numSurfaces = -1;      // not checked yet
            interaction.surfaces = null;

            interaction.frustumState = FrustumState.FRUSTUM_UNINITIALIZED;
            interaction.frustumAreas = null;

            // link at the start of the entity's list
            interaction.lightNext = ldef.firstInteraction;
            interaction.lightPrev = null;
            ldef.firstInteraction = interaction;
            if (interaction.lightNext != null) interaction.lightNext.lightPrev = interaction;
            else ldef.lastInteraction = interaction;

            // link at the start of the light's list
            interaction.entityNext = edef.firstInteraction;
            interaction.entityPrev = null;
            edef.firstInteraction = interaction;
            if (interaction.entityNext != null) interaction.entityNext.entityPrev = interaction;
            else edef.lastInteraction = interaction;

            // update the interaction table
            if (renderWorld.interactionTable != null)
            {
                var index = ldef.index * renderWorld.interactionTableWidth + edef.index;
                if (renderWorld.interactionTable[index] != null) common.Error("Interaction::AllocAndLink: non null table entry");
                renderWorld.interactionTable[index] = interaction;
            }

            return interaction;
        }

        // unlinks from the entity and light, frees all surfaceInteractions, and puts it back on the free list
        // Removes links and puts it back on the free list.
        public void UnlinkAndFree()
        {
            // clear the table pointer
            var renderWorld = this.lightDef.world;
            if (renderWorld.interactionTable != null)
            {
                var index = this.lightDef.index * renderWorld.interactionTableWidth + this.entityDef.index;
                if (renderWorld.interactionTable[index] != this) common.Error("Interaction::UnlinkAndFree: interactionTable wasn't set");
                renderWorld.interactionTable[index] = null;
            }

            Unlink();

            FreeSurfaces();

            // free the interaction area references
            AreaNumRef area, nextArea;
            for (area = frustumAreas; area != null; area = nextArea) { nextArea = area.next; renderWorld.areaNumRefAllocator.Free(area); }

            // put it back on the free list
            renderWorld.interactionAllocator.Free(this);
        }

        // free the interaction surfaces
        // Frees the surfaces, but leaves the interaction linked in, so it will be regenerated automatically
        public void FreeSurfaces()
        {
            if (this.surfaces != null)
            {
                for (var i = 0; i < this.numSurfaces; i++)
                {
                    var sint = this.surfaces[i];
                    if (sint.lightTris != null)
                    {
                        if (sint.lightTris != LIGHT_TRIS_DEFERRED) R_FreeStaticTriSurf(sint.lightTris);
                        sint.lightTris = null;
                    }
                    if (sint.shadowTris != null)
                    {
                        // if it doesn't have an entityDef, it is part of a prelight model, not a generated interaction
                        if (this.entityDef != null) { R_FreeStaticTriSurf(sint.shadowTris); sint.shadowTris = null; }
                    }
                    R_FreeInteractionCullInfo(sint.cullInfo);
                }

                this.surfaces = null;
            }
            this.numSurfaces = -1;
        }

        // makes the interaction empty for when the light and entity do not actually intersect all empty interactions are linked at the end of the light's and entity's interaction list
        // Makes the interaction empty and links it at the end of the entity's and light's interaction lists.
        public void MakeEmpty()
        {
            // an empty interaction has no surfaces
            numSurfaces = 0;

            Unlink();

            // relink at the end of the entity's list
            this.entityNext = null;
            this.entityPrev = this.entityDef.lastInteraction;
            this.entityDef.lastInteraction = this;
            if (this.entityPrev != null) this.entityPrev.entityNext = this;
            else this.entityDef.firstInteraction = this;

            // relink at the end of the light's list
            this.lightNext = null;
            this.lightPrev = this.lightDef.lastInteraction;
            this.lightDef.lastInteraction = this;
            if (this.lightPrev != null) this.lightPrev.lightNext = this;
            else this.lightDef.firstInteraction = this;
        }

        // returns true if the interaction is empty
        public bool IsEmpty
            => numSurfaces == 0;

        // returns true if the interaction is not yet completely created
        public bool IsDeferred
            => numSurfaces == -1;

        // returns true if the interaction has shadows
        public bool HasShadows
            => !lightDef.parms.noShadows && !entityDef.parms.noShadow && lightDef.lightShader.LightCastsShadows;

        // counts up the memory used by all the surfaceInteractions, which will be used to determine when we need to start purging old interactions
        // Counts up the memory used by all the surfaceInteractions, which will be used to determine when we need to start purging old interactions.
        public int MemoryUsed
        {
            get
            {
                var total = 0;
                for (var i = 0; i < numSurfaces; i++)
                {
                    var inter = surfaces[i];
                    total += R_TriSurfMemory(inter.lightTris);
                    total += R_TriSurfMemory(inter.shadowTris);
                }
                return total;
            }
        }

        // unlink from entity and light lists
        protected void Unlink()
        {
            // unlink from the entity's list
            if (this.entityPrev != null) this.entityPrev.entityNext = this.entityNext;
            else this.entityDef.firstInteraction = this.entityNext;
            if (this.entityNext != null) this.entityNext.entityPrev = this.entityPrev;
            else this.entityDef.lastInteraction = this.entityPrev;
            this.entityNext = this.entityPrev = null;

            // unlink from the light's list
            if (this.lightPrev != null) this.lightPrev.lightNext = this.lightNext;
            else this.lightDef.firstInteraction = this.lightNext;
            if (this.lightNext != null) this.lightNext.lightPrev = this.lightPrev;
            else this.lightDef.lastInteraction = this.lightPrev;
            this.lightNext = this.lightPrev = null;
        }
    }

    unsafe partial class R
    {
        // Determines which triangles of the surface are facing towards the light origin.
        // The facing array should be allocated with one extra index than the number of surface triangles, which will be used to handle dangling edge silhouettes.
        static void R_CalcInteractionFacing(IRenderEntity ent, SrfTriangles tri, IRenderLight light, ref SrfCullInfo cullInfo)
        {
            if (cullInfo.facing != null) return;

            Vector3 localLightOrigin;
            R_GlobalPointToLocal(ent.modelMatrix, light.globalLightOrigin, out localLightOrigin);

            var numFaces = tri.numIndexes / 3;

            if (tri.facePlanes == null || !tri.facePlanesCalculated) R_DeriveFacePlanes(tri);

            cullInfo.facing = (byte*)R_StaticAlloc((numFaces + 1) * sizeof(byte));

            // calculate back face culling
            var planeSide = stackalloc float[numFaces + floatX.ALLOC16]; planeSide = (float*)_alloca16(planeSide);

            // exact geometric cull against face
            fixed (Plane* facePlanesP = tri.facePlanes) Simd.Dotcp(planeSide, localLightOrigin, facePlanesP, numFaces);
            Simd.CmpGE(cullInfo.facing, planeSide, 0f, numFaces);

            cullInfo.facing[numFaces] = 1;  // for dangling edges to reference
        }

        // We want to cull a little on the sloppy side, because the pre-clipping of geometry to the lights in dmap will give many cases that are right
        // at the border we throw things out on the border, because if any one vertex is clearly inside, the entire triangle will be accepted.
        static void R_CalcInteractionCullBits(IRenderEntity ent, SrfTriangles tri, IRenderLight light, ref SrfCullInfo cullInfo)
        {
            int i, frontBits;

            if (cullInfo.cullBits != null) return;

            frontBits = 0;

            // cull the triangle surface bounding box
            for (i = 0; i < 6; i++)
            {
                R_GlobalPlaneToLocal(ent.modelMatrix, -light.frustum[i], out cullInfo.localClipPlanes[i]);

                // get front bits for the whole surface
                if (tri.bounds.PlaneDistance(cullInfo.localClipPlanes[i]) >= IInteraction.LIGHT_CLIP_EPSILON) frontBits |= 1 << i;
            }

            // if the surface is completely inside the light frustum
            if (frontBits == ((1 << 6) - 1)) { cullInfo.cullBits = IInteraction.LIGHT_CULL_ALL_FRONT; return; }

            cullInfo.cullBits = (byte*)R_StaticAlloc(tri.numVerts * sizeof(byte));
            Simd.Memset(cullInfo.cullBits, 0, tri.numVerts * sizeof(byte));

            var planeSide = stackalloc float[tri.numVerts + floatX.ALLOC16]; planeSide = (float*)_alloca16(planeSide);

            for (i = 0; i < 6; i++)
            {
                // if completely infront of this clipping plane
                if ((frontBits & (1 << i)) != 0) continue;
                fixed (DrawVert* vertsD = tri.verts) Simd.Dotpd(planeSide, cullInfo.localClipPlanes[i], vertsD, tri.numVerts);
                Simd.CmpLTb(cullInfo.cullBits, (byte)i, planeSide, IInteraction.LIGHT_CLIP_EPSILON, tri.numVerts);
            }
        }

        public static void R_FreeInteractionCullInfo(SrfCullInfo cullInfo)
        {
            if (cullInfo.facing != null) { R_StaticFree(cullInfo.facing); cullInfo.facing = null; }
            if (cullInfo.cullBits != null)
            {
                if (cullInfo.cullBits != IInteraction.LIGHT_CULL_ALL_FRONT) R_StaticFree(cullInfo.cullBits);
                cullInfo.cullBits = null;
            }
        }

        unsafe struct ClipTri
        {
            public const int MAX_CLIPPED_POINTS = 20;
            public int numVerts;
            public Vector3 verts00; public Vector3 verts01; public Vector3 verts02; public Vector3 verts03; public Vector3 verts04;
            public Vector3 verts05; public Vector3 verts06; public Vector3 verts07; public Vector3 verts08; public Vector3 verts09;
            public Vector3 verts10; public Vector3 verts11; public Vector3 verts12; public Vector3 verts13; public Vector3 verts14;
            public Vector3 verts15; public Vector3 verts16; public Vector3 verts17; public Vector3 verts18; public Vector3 verts19;
            public ref Vector3 vertsGet(int index) { fixed (Vector3* p = &verts00) return ref p[index]; }
            public void vertsSet(int index, in Vector3 value) { fixed (Vector3* p = &verts00) p[index] = value; }
        }

        // Clips a triangle from one buffer to another, setting edge flags The returned buffer may be the same as inNum if no clipping is done If entirely clipped away, clipTris[returned].numVerts == 0
        // I have some worries about edge flag cases when polygons are clipped multiple times near the epsilon.
        static int R_ChopWinding(ClipTri* clipTris, int inNum, in Plane plane)
        {
            var dists = stackalloc float[ClipTri.MAX_CLIPPED_POINTS];
            var sides = stackalloc int[ClipTri.MAX_CLIPPED_POINTS];
            var counts = stackalloc int[3];
            float dot;
            int i, j;
            Vector3 mid;
            bool front;

            ref ClipTri in_ = ref clipTris[inNum];
            ref ClipTri o = ref clipTris[inNum ^ 1];
            counts[0] = counts[1] = counts[2] = 0;

            // determine sides for each point
            front = false;
            for (i = 0; i < in_.numVerts; i++)
            {
                dot = in_.vertsGet(i) * plane.Normal + plane[3];
                dists[i] = dot;
                if (dot < IInteraction.LIGHT_CLIP_EPSILON) sides[i] = Plane.SIDE_BACK; // slop onto the back
                else { sides[i] = Plane.SIDE_FRONT; if (dot > IInteraction.LIGHT_CLIP_EPSILON) front = true; }
                counts[sides[i]]++;
            }

            // if none in front, it is completely clipped away
            if (!front) { in_.numVerts = 0; return inNum; }
            if (counts[Plane.SIDE_BACK] == 0) return inNum;       // inout stays the same

            // avoid wrapping checks by duplicating first value to end
            sides[i] = sides[0];
            dists[i] = dists[0];
            in_.vertsSet(in_.numVerts, in_.verts00);

            o.numVerts = 0;
            for (i = 0; i < in_.numVerts; i++)
            {
                ref Vector3 p1 = ref in_.vertsGet(i);
                if (sides[i] == Plane.SIDE_FRONT) { o.vertsSet(o.numVerts, p1); o.numVerts++; }
                if (sides[i + 1] == sides[i]) continue;

                // generate a split point
                ref Vector3 p2 = ref in_.vertsGet(i + 1);

                dot = dists[i] / (dists[i] - dists[i + 1]);
                mid.x = p1.x + dot * (p2.x - p1.x);
                mid.y = p1.y + dot * (p2.y - p1.y);
                mid.z = p1.z + dot * (p2.z - p1.z);

                o.vertsSet(o.numVerts, mid);

                o.numVerts++;
            }

            return inNum ^ 1;
        }

        // Returns false if nothing is left after clipping
        static bool R_ClipTriangleToLight(in Vector3 a, in Vector3 b, in Vector3 c, int planeBits, Plane[] frustum)
        {
            int i, p; var pingPong = stackalloc ClipTri[2];

            pingPong[0].numVerts = 3;
            pingPong[0].verts00 = a;
            pingPong[0].verts01 = b;
            pingPong[0].verts02 = c;

            p = 0;
            for (i = 0; i < 6; i++)
                if ((planeBits & (1 << i)) != 0)
                {
                    p = R_ChopWinding(pingPong, p, frustum[i]);
                    if (pingPong[p].numVerts < 1) return false;
                }

            return true;
        }

        // The resulting surface will be a subset of the original triangles, it will never clip triangles, but it may cull on a per-triangle basis.
        static SrfTriangles R_CreateLightTris(IRenderEntity ent, SrfTriangles tri, IRenderLight light, Material shader, ref SrfCullInfo cullInfo)
        {
            int i, numIndexes;
            GlIndex[] indexes;
            SrfTriangles newTri;
            int c_backfaced, c_distance;
            Bounds bounds = default;
            bool includeBackFaces; int faceNum;
            var tri_verts = tri.verts; var tri_indexes = tri.indexes;

            tr.pc.c_createLightTris++;
            c_backfaced = 0;
            c_distance = 0;

            numIndexes = 0;
            indexes = null;

            // it is debatable if non-shadowing lights should light back faces. we aren't at the moment
            includeBackFaces = r_lightAllBackFaces.Bool || light.lightShader.LightEffectsBackSides || shader.ReceivesLightingOnBackSides || ent.parms.noSelfShadow || ent.parms.noShadow;

            // allocate a new surface for the lit triangles
            newTri = R_AllocStaticTriSurf();

            // save a reference to the original surface
            newTri.ambientSurface = tri;

            // the light surface references the verts of the ambient surface
            newTri.numVerts = tri.numVerts;
            R_ReferenceStaticTriSurfVerts(newTri, tri);

            // calculate cull information
            if (!includeBackFaces) R_CalcInteractionFacing(ent, tri, light, ref cullInfo);
            R_CalcInteractionCullBits(ent, tri, light, ref cullInfo);

            // if the surface is completely inside the light frustum
            if (cullInfo.cullBits == IInteraction.LIGHT_CULL_ALL_FRONT)
            {
                // if we aren't self shadowing, let back facing triangles get through so the smooth shaded bump maps light all the way around
                if (includeBackFaces)
                {
                    // the whole surface is lit so the light surface just references the indexes of the ambient surface
                    R_ReferenceStaticTriSurfIndexes(newTri, tri);
                    numIndexes = tri.numIndexes;
                    bounds = tri.bounds;
                }
                else
                {
                    // the light tris indexes are going to be a subset of the original indexes so we generally allocate too much memory here but we decrease the memory block when the number of indexes is known
                    R_AllocStaticTriSurfIndexes(newTri, tri.numIndexes);

                    // back face cull the individual triangles
                    indexes = newTri.indexes;
                    var facing = cullInfo.facing;
                    for (faceNum = 0, i = 0; i < tri.numIndexes; i += 3, faceNum++)
                    {
                        if (facing[faceNum] != 0) { c_backfaced++; continue; }
                        indexes[numIndexes + 0] = tri_indexes[i + 0];
                        indexes[numIndexes + 1] = tri_indexes[i + 1];
                        indexes[numIndexes + 2] = tri_indexes[i + 2];
                        numIndexes += 3;
                    }

                    // get bounds for the surface
                    fixed (DrawVert* vertsD = tri_verts)
                    fixed (GlIndex* indexesG = indexes)
                        Simd.MinMaxdi(out bounds.b0, out bounds.b1, vertsD, indexesG, numIndexes);

                    // decrease the size of the memory block to the size of the number of used indexes
                    R_ResizeStaticTriSurfIndexes(newTri, numIndexes);
                }
            }
            else
            {
                // the light tris indexes are going to be a subset of the original indexes so we generally
                // allocate too much memory here but we decrease the memory block when the number of indexes is known
                R_AllocStaticTriSurfIndexes(newTri, tri.numIndexes);

                // cull individual triangles
                indexes = newTri.indexes;
                var facing = cullInfo.facing;
                var cullBits = cullInfo.cullBits;
                for (faceNum = i = 0; i < tri.numIndexes; i += 3, faceNum++)
                {
                    int i1, i2, i3;

                    // if we aren't self shadowing, let back facing triangles get through so the smooth shaded bump maps light all the way around
                    if (!includeBackFaces && facing[faceNum] == 0) { c_backfaced++; continue; } // back face cull

                    i1 = tri_indexes[i + 0];
                    i2 = tri_indexes[i + 1];
                    i3 = tri_indexes[i + 2];

                    // fast cull outside the frustum. if all three points are off one plane side, it definately isn't visible
                    if (cullBits[i1] != 0 & cullBits[i2] != 0 & cullBits[i3] != 0) { c_distance++; continue; }

                    // do a precise clipped cull if none of the points is completely inside the frustum. note that we do not actually use the clipped triangle, which would have Z fighting issues.
                    if (r_usePreciseTriangleInteractions.Bool && cullBits[i1] != 0 && cullBits[i2] != 0 && cullBits[i3] != 0)
                    {
                        var cull = cullBits[i1] | cullBits[i2] | cullBits[i3];
                        if (!R_ClipTriangleToLight(tri_verts[i1].xyz, tri_verts[i2].xyz, tri_verts[i3].xyz, cull, cullInfo.localClipPlanes)) continue;
                    }

                    // add to the list
                    indexes[numIndexes + 0] = i1;
                    indexes[numIndexes + 1] = i2;
                    indexes[numIndexes + 2] = i3;
                    numIndexes += 3;
                }

                // get bounds for the surface
                fixed (DrawVert* vertsD = tri_verts)
                fixed (GlIndex* indexesG = indexes)
                    Simd.MinMaxdi(out bounds.b0, out bounds.b1, vertsD, indexesG, numIndexes);

                // decrease the size of the memory block to the size of the number of used indexes
                R_ResizeStaticTriSurfIndexes(newTri, numIndexes);
            }

            if (numIndexes == 0) { R_ReallyFreeStaticTriSurf(newTri); return null; }
            newTri.numIndexes = numIndexes;
            newTri.bounds = bounds;
            return newTri;
        }

        static void R_ShowInteractionMemory_f(CmdArgs args)
        {
            int total = 0, entities = 0,
                interactions = 0, deferredInteractions = 0, emptyInteractions = 0,
                lightTris = 0, lightTriVerts = 0, lightTriIndexes = 0,
                shadowTris = 0, shadowTriVerts = 0, shadowTriIndexes = 0;

            for (var i = 0; i < tr.primaryWorld.entityDefs.Count; i++)
            {
                var def = tr.primaryWorld.entityDefs[i];
                if (def == null) continue;
                if (def.firstInteraction == null) continue;
                entities++;

                for (var inter = (IInteraction)def.firstInteraction; inter != null; inter = (IInteraction)inter.entityNext)
                {
                    interactions++;
                    total += inter.MemoryUsed;

                    if (inter.IsDeferred) { deferredInteractions++; continue; }
                    if (inter.IsEmpty) { emptyInteractions++; continue; }

                    for (var j = 0; j < inter.numSurfaces; j++)
                    {
                        var srf = inter.surfaces[j];
                        if (srf.lightTris != null && srf.lightTris != IInteraction.LIGHT_TRIS_DEFERRED)
                        {
                            lightTris++;
                            lightTriVerts += srf.lightTris.numVerts;
                            lightTriIndexes += srf.lightTris.numIndexes;
                        }
                        if (srf.shadowTris != null)
                        {
                            shadowTris++;
                            shadowTriVerts += srf.shadowTris.numVerts;
                            shadowTriIndexes += srf.shadowTris.numIndexes;
                        }
                    }
                }
            }

            common.Printf($"{entities} entities with {interactions} total interactions totalling {total / 1024}k\n");
            common.Printf($"{deferredInteractions} deferred interactions, {emptyInteractions} empty interactions\n");
            common.Printf($"{lightTriIndexes,5} indexes {lightTriVerts,5} verts in {lightTris,5} light tris\n");
            common.Printf($"{shadowTriIndexes,5} indexes {shadowTriVerts,5} verts in {shadowTris,5} shadow tris\n");
        }
    }
}