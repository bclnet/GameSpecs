using System.Diagnostics;
using System.Runtime.InteropServices;
using static System.NumericsX.OpenStack.Gngine.Gngine;
using static System.NumericsX.OpenStack.OpenStack;
using static System.NumericsX.Platform;
using GlIndex = System.Int32;

namespace System.NumericsX.OpenStack.Gngine.Render
{
    [StructLayout(LayoutKind.Sequential)]
    public struct DecalProjectionInfo
    {
        public Vector3 projectionOrigin;
        public Bounds projectionBounds;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)] public Plane[] boundingPlanes;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)] public Plane[] fadePlanes;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)] public Plane[] textureAxis;
        public Material material;
        public bool parallel;
        public float fadeDepth;
        public int startTime;
        public bool force;
    }

    public unsafe class RenderModelDecal
    {
        const int NUM_DECAL_BOUNDING_PLANES = 6;

        const int MAX_DECAL_VERTS = 40;
        const int MAX_DECAL_INDEXES = 60;

        Material material;
        SrfTriangles tri;
        DrawVert[] verts = new DrawVert[MAX_DECAL_VERTS];
        float[] vertDepthFade = new float[MAX_DECAL_VERTS];
        GlIndex[] indexes = new GlIndex[MAX_DECAL_INDEXES];
        int[] indexStartTime = new int[MAX_DECAL_INDEXES];
        RenderModelDecal nextDecal;

        public RenderModelDecal()
        {
            tri = new();
            tri.verts = verts;
            tri.indexes = indexes;
            material = null;
            nextDecal = null;
        }

        public static RenderModelDecal Alloc()
            => new();

        public static void Free(ref RenderModelDecal decal)
            => decal = null;

        // Creates decal projection info.
        public static bool CreateProjectionInfo(out DecalProjectionInfo info, in FixedWinding winding, in Vector3 projectionOrigin, bool parallel, float fadeDepth, Material material, int startTime)
        {
            if (winding.NumPoints != NUM_DECAL_BOUNDING_PLANES - 2) { common.Printf($"RenderModelDecal::CreateProjectionInfo: winding must have {NUM_DECAL_BOUNDING_PLANES - 2} points\n"); info = default; return false; }
            Debug.Assert(material != null);
            info.boundingPlanes = default;
            info.fadePlanes = default;
            info.textureAxis = default;

            info.projectionOrigin = projectionOrigin;
            info.material = material;
            info.parallel = parallel;
            info.fadeDepth = fadeDepth;
            info.startTime = startTime;
            info.force = false;

            // get the winding plane and the depth of the projection volume
            winding.GetPlane(out var windingPlane);
            var depth = windingPlane.Distance(projectionOrigin);

            // find the bounds for the projection
            winding.GetBounds(out info.projectionBounds);
            if (parallel) info.projectionBounds.ExpandSelf(depth);
            else info.projectionBounds.AddPoint(projectionOrigin);

            // calculate the world space projection volume bounding planes, positive sides face outside the decal
            if (parallel)
                for (var i = 0; i < winding.NumPoints; i++)
                {
                    var edge = winding[(i + 1) % winding.NumPoints].ToVec3() - winding[i].ToVec3();
                    info.boundingPlanes[i].Normal.Cross(windingPlane.Normal, edge);
                    info.boundingPlanes[i].Normalize();
                    info.boundingPlanes[i].FitThroughPoint(winding[i].ToVec3());
                }
            else for (var i = 0; i < winding.NumPoints; i++) info.boundingPlanes[i].FromPoints(projectionOrigin, winding[i].ToVec3(), winding[(i + 1) % winding.NumPoints].ToVec3());
            info.boundingPlanes[NUM_DECAL_BOUNDING_PLANES - 2] = windingPlane;
            info.boundingPlanes[NUM_DECAL_BOUNDING_PLANES - 2].d -= depth;
            info.boundingPlanes[NUM_DECAL_BOUNDING_PLANES - 1] = -windingPlane;

            // fades will be from these plane
            info.fadePlanes[0] = windingPlane; info.fadePlanes[0].d -= fadeDepth;
            info.fadePlanes[1] = -windingPlane; info.fadePlanes[1].d += depth - fadeDepth;

            // calculate the texture vectors for the winding
            float len, texArea, inva; Vector3 temp; Vector5 d0, d1;

            ref Vector5 a = ref winding[0];
            ref Vector5 b = ref winding[1];
            ref Vector5 c = ref winding[2];

            d0 = new Vector5(b.ToVec3() - a.ToVec3()) { s = b.s - a.s, t = b.t - a.t };
            d1 = new Vector5(c.ToVec3() - a.ToVec3()) { s = c.s - a.s, t = c.t - a.t };
            texArea = (d0.s * d1.t) - (d0.t * d1.s);
            inva = 1f / texArea;

            temp.x = (d0.x * d1.t - d0[4] * d1.x) * inva;
            temp.y = (d0.y * d1.t - d0[4] * d1.y) * inva;
            temp.z = (d0.z * d1.t - d0[4] * d1.z) * inva;
            len = temp.Normalize();
            info.textureAxis[0].Normal = temp * (1f / len);
            info.textureAxis[0].d = winding[0].s - (winding[0].ToVec3() * info.textureAxis[0].Normal);

            temp.x = (d0.s * d1.x - d0.x * d1.s) * inva;
            temp.y = (d0.s * d1.y - d0.y * d1.s) * inva;
            temp.z = (d0.s * d1.z - d0.z * d1.s) * inva;
            len = temp.Normalize();
            info.textureAxis[1].Normal = temp * (1f / len);
            info.textureAxis[1].d = winding[0].t - (winding[0].ToVec3() * info.textureAxis[1].Normal);

            return true;
        }

        // Transform the projection info from global space to local.
        public static void GlobalProjectionInfoToLocal(out DecalProjectionInfo localInfo, in DecalProjectionInfo info, in Vector3 origin, in Matrix3x3 axis)
        {
            var modelMatrix = new float[16];
            localInfo.boundingPlanes = default;
            localInfo.fadePlanes = default;
            localInfo.textureAxis = default;

            R_AxisToModelMatrix(axis, origin, modelMatrix);

            for (var j = 0; j < NUM_DECAL_BOUNDING_PLANES; j++) R_GlobalPlaneToLocal(modelMatrix, info.boundingPlanes[j], out localInfo.boundingPlanes[j]);
            R_GlobalPlaneToLocal(modelMatrix, info.fadePlanes[0], out localInfo.fadePlanes[0]);
            R_GlobalPlaneToLocal(modelMatrix, info.fadePlanes[1], out localInfo.fadePlanes[1]);
            R_GlobalPlaneToLocal(modelMatrix, info.textureAxis[0], out localInfo.textureAxis[0]);
            R_GlobalPlaneToLocal(modelMatrix, info.textureAxis[1], out localInfo.textureAxis[1]);
            R_GlobalPointToLocal(modelMatrix, info.projectionOrigin, out localInfo.projectionOrigin);
            localInfo.projectionBounds = info.projectionBounds;
            localInfo.projectionBounds.TranslateSelf(-origin);
            localInfo.projectionBounds.RotateSelf(axis.Transpose());
            localInfo.material = info.material;
            localInfo.parallel = info.parallel;
            localInfo.fadeDepth = info.fadeDepth;
            localInfo.startTime = info.startTime;
            localInfo.force = info.force;
        }

        // Creates a deal on the given model.
        public void CreateDecal(IRenderModel model, in DecalProjectionInfo localInfo)
        {
            // check all model surfaces
            for (var surfNum = 0; surfNum < model.NumSurfaces; surfNum++)
            {
                var surf = model.Surface(surfNum);

                // if no geometry or no shader
                if (surf.geometry == null || surf.shader == null) continue;

                // decals and overlays use the same rules
                if (!localInfo.force && !surf.shader.AllowOverlays) continue;

                var stri = surf.geometry;
                var stri_verts = stri.verts; var stri_indexes = stri.indexes; var stri_facePlanes = stri.facePlanes;

                // if the triangle bounds do not overlap with projection bounds
                if (!localInfo.projectionBounds.IntersectsBounds(stri.bounds)) continue;

                // allocate memory for the cull bits
                byte* cullBits = stackalloc byte[stri.numVerts + byteX.ALLOC16]; cullBits = (byte*)_alloca16(cullBits);

                // catagorize all points by the planes
                fixed (Plane* boundingPlanesP = localInfo.boundingPlanes)
                fixed (DrawVert* vertsD = stri_verts)
                    Simd.DecalPointCull(cullBits, boundingPlanesP, vertsD, stri.numVerts);

                // find triangles inside the projection volume
                for (int triNum = 0, index = 0; index < stri.numIndexes; index += 3, triNum++)
                {
                    var v1 = stri_indexes[index + 0];
                    var v2 = stri_indexes[index + 1];
                    var v3 = stri_indexes[index + 2];

                    // skip triangles completely off one side
                    if (cullBits[v1] != 0 & cullBits[v2] != 0 & cullBits[v3] != 0) continue;

                    // skip back facing triangles
                    if (stri.facePlanes != null && stri.facePlanesCalculated && stri_facePlanes[triNum].Normal * localInfo.boundingPlanes[NUM_DECAL_BOUNDING_PLANES - 2].Normal < -0.1f) continue;

                    // create a winding with texture coordinates for the triangle
                    FixedWinding fw = default;
                    fw.NumPoints = 3;
                    if (localInfo.parallel)
                        for (var j = 0; j < 3; j++)
                            fw[j] = new Vector5(stri_verts[stri_indexes[index + j]].xyz)
                            {
                                s = localInfo.textureAxis[0].Distance(fw[j].ToVec3()),
                                t = localInfo.textureAxis[1].Distance(fw[j].ToVec3()),
                            };
                    else
                        for (var j = 0; j < 3; j++)
                        {
                            fw[j] = new Vector5(stri_verts[stri_indexes[index + j]].xyz);
                            var dir = fw[j].ToVec3() - localInfo.projectionOrigin;
                            if (!localInfo.boundingPlanes[NUM_DECAL_BOUNDING_PLANES - 1].RayIntersection(fw[j].ToVec3(), dir, out var scale)) scale = 0f;
                            dir = fw[j].ToVec3() + scale * dir;
                            fw[j].s = localInfo.textureAxis[0].Distance(dir);
                            fw[j].t = localInfo.textureAxis[1].Distance(dir);
                        }

                    var orBits = cullBits[v1] | cullBits[v2] | cullBits[v3];

                    // clip the exact surface triangle to the projection volume
                    for (var j = 0; j < NUM_DECAL_BOUNDING_PLANES; j++) if ((orBits & (1 << j)) != 0 && !fw.ClipInPlace(-localInfo.boundingPlanes[j])) break;

                    if (fw.NumPoints == 0) continue;

                    AddDepthFadedWinding(fw, localInfo.material, localInfo.fadePlanes, localInfo.fadeDepth, localInfo.startTime);
                }
            }
        }

        // Remove decals that are completely faded away.
        public static RenderModelDecal RemoveFadedDecals(RenderModelDecal decals, int time)
        {
            int i, j, minTime, newNumIndexes, newNumVerts;
            DecalInfo decalInfo; RenderModelDecal nextDecal;

            if (decals == null) return null;

            // recursively free any next decals
            decals.nextDecal = RemoveFadedDecals(decals.nextDecal, time);

            // free the decals if no material set
            if (decals.material == null) { nextDecal = decals.nextDecal; Free(ref decals); return nextDecal; }

            decalInfo = decals.material.DecalInfo;
            minTime = time - (decalInfo.stayTime + decalInfo.fadeTime);

            newNumIndexes = 0;
            for (i = 0; i < decals.tri.numIndexes; i += 3)
                if (decals.indexStartTime[i] > minTime)
                {
                    // keep this triangle
                    if (newNumIndexes != i)
                        for (j = 0; j < 3; j++)
                        {
                            decals.tri.indexes[newNumIndexes + j] = decals.tri.indexes[i + j];
                            decals.indexStartTime[newNumIndexes + j] = decals.indexStartTime[i + j];
                        }
                    newNumIndexes += 3;
                }

            // free the decals if all trianges faded away
            if (newNumIndexes == 0) { nextDecal = decals.nextDecal; Free(ref decals); return nextDecal; }

            decals.tri.numIndexes = newNumIndexes;

            var inUse = stackalloc int[MAX_DECAL_VERTS];
            for (i = 0; i < decals.tri.numIndexes; i++) inUse[decals.tri.indexes[i]] = 1;

            newNumVerts = 0;
            for (i = 0; i < decals.tri.numVerts; i++)
            {
                if (inUse[i] == 0) continue;
                var decals_tri_verts = decals.tri.verts;
                decals_tri_verts[newNumVerts] = decals_tri_verts[i];
                decals.vertDepthFade[newNumVerts] = decals.vertDepthFade[i];
                inUse[i] = newNumVerts;
                newNumVerts++;
            }
            decals.tri.numVerts = newNumVerts;

            for (i = 0; i < decals.tri.numIndexes; i++) { var decals_tri_indexes = decals.tri.indexes; decals_tri_indexes[i] = inUse[decals_tri_indexes[i]]; }

            return decals;
        }

        // Updates the vertex colors, removing any faded indexes, then copy the verts to temporary vertex cache and adds a drawSurf.
        public void AddDecalDrawSurf(ViewEntity space)
        {
            int i, j, maxTime; float f; DecalInfo decalInfo;

            if (tri.numIndexes == 0) return;
            var tri_verts = tri.verts; var tri_indexes = tri.indexes;

            // fade down all the verts with time
            decalInfo = material.DecalInfo;
            maxTime = decalInfo.stayTime + decalInfo.fadeTime;

            // set vertex colors and remove faded triangles
            for (i = 0; i < tri.numIndexes; i += 3)
            {
                var deltaTime = tr.viewDef.renderView.time - indexStartTime[i];

                if (deltaTime > maxTime && deltaTime <= decalInfo.stayTime) continue;

                deltaTime -= decalInfo.stayTime;
                f = (float)deltaTime / decalInfo.fadeTime;

                for (j = 0; j < 3; j++)
                {
                    var ind = tri_indexes[i + j];

                    for (var k = 0; k < 4; k++)
                    {
                        var fcolor = decalInfo.start[k] + (decalInfo.end[k] - decalInfo.start[k]) * f;
                        var icolor = MathX.FtoiFast(fcolor * vertDepthFade[ind] * 255f);
                        if (icolor < 0) icolor = 0;
                        else if (icolor > 255) icolor = 255;
                        tri_verts[ind].SetColor(k, (byte)icolor);
                    }
                }
            }

            // copy the tri and indexes to temp heap memory, because if we are running multi-threaded, we wouldn't be able to reorganize the index list
            //var newTri = R_FrameAllocT<SrfTriangles>();
            var newTri = new SrfTriangles(tri);
            // copy the current vertexes to temp vertex cache
            fixed (DrawVert* vertsD = tri_verts) newTri.ambientCache = vertexCache.AllocFrameTemp(vertsD, tri.numVerts * sizeof(DrawVert), false);
            fixed (GlIndex* indexesG = tri_indexes) newTri.indexCache = vertexCache.AllocFrameTemp(indexesG, tri.numIndexes * sizeof(GlIndex), true);

            // create the drawsurf
            R_AddDrawSurf(newTri, space, space.entityDef.parms, material, space.scissorRect);
        }

        // Returns the next decal in the chain.
        public RenderModelDecal Next()
            => nextDecal;

        public void ReadFromDemoFile(VFileDemo f) { }
        public void WriteToDemoFile(VFileDemo f) { }

        // Adds the winding triangles to the appropriate decal in the chain, creating a new one if necessary.
        void AddWinding(in Winding w, Material decalMaterial, Plane[] fadePlanes, float fadeDepth, int startTime)
        {
            int i; float invFadeDepth, fade; DecalInfo decalInfo;
            var tri_verts = tri.verts; var tri_indexes = tri.indexes;

            if ((material == null || material == decalMaterial) && tri.numVerts + w.NumPoints < MAX_DECAL_VERTS && tri.numIndexes + (w.NumPoints - 2) * 3 < MAX_DECAL_INDEXES)
            {
                material = decalMaterial;

                // add to this decal
                decalInfo = material.DecalInfo;
                invFadeDepth = -1f / fadeDepth;

                for (i = 0; i < w.NumPoints; i++)
                {
                    fade = fadePlanes[0].Distance(w[i].ToVec3()) * invFadeDepth;
                    if (fade < 0f) fade = fadePlanes[1].Distance(w[i].ToVec3()) * invFadeDepth;
                    if (fade < 0f) fade = 0f;
                    else if (fade > 0.99f) fade = 1f;
                    fade = 1f - fade;
                    vertDepthFade[tri.numVerts + i] = fade;
                    tri_verts[tri.numVerts + i].xyz = w[i].ToVec3();
                    tri_verts[tri.numVerts + i].st.x = w[i].s;
                    tri_verts[tri.numVerts + i].st.y = w[i].t;
                    for (var k = 0; k < 4; k++)
                    {
                        var icolor = MathX.FtoiFast(decalInfo.start[k] * fade * 255f);
                        if (icolor < 0) icolor = 0;
                        else if (icolor > 255) icolor = 255;
                        tri_verts[tri.numVerts + i].SetColor(k, (byte)icolor);
                    }
                }
                for (i = 2; i < w.NumPoints; i++)
                {
                    tri_indexes[tri.numIndexes + 0] = tri.numVerts;
                    tri_indexes[tri.numIndexes + 1] = tri.numVerts + i - 1;
                    tri_indexes[tri.numIndexes + 2] = tri.numVerts + i;
                    indexStartTime[tri.numIndexes] = indexStartTime[tri.numIndexes + 1] = indexStartTime[tri.numIndexes + 2] = startTime;
                    tri.numIndexes += 3;
                }
                tri.numVerts += w.NumPoints;
                return;
            }

            // if we are at the end of the list, create a new decal
            if (nextDecal == null) nextDecal = RenderModelDecal.Alloc();
            // let the next decal on the chain take a look
            nextDecal.AddWinding(w, decalMaterial, fadePlanes, fadeDepth, startTime);
        }

        // Adds depth faded triangles for the winding to the appropriate decal in the chain, creating a new one if necessary.
        // The part of the winding at the front side of both fade planes is not faded. The parts at the back sides of the fade planes are faded with the given depth.
        void AddDepthFadedWinding(Winding w, Material decalMaterial, Plane[] fadePlanes, float fadeDepth, int startTime)
        {
            FixedWinding front, back = default;

            front = (FixedWinding)w;
            if (front.Split(back, fadePlanes[0], 0.1f) == Plane.SIDE_CROSS) AddWinding(back, decalMaterial, fadePlanes, fadeDepth, startTime);
            if (front.Split(back, fadePlanes[1], 0.1f) == Plane.SIDE_CROSS) AddWinding(back, decalMaterial, fadePlanes, fadeDepth, startTime);
            AddWinding(front, decalMaterial, fadePlanes, fadeDepth, startTime);
        }
    }
}