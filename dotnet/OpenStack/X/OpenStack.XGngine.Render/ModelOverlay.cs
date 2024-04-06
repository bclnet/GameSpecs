using System.Collections.Generic;
using System.Diagnostics;
using static System.NumericsX.OpenStack.Gngine.Render.R;
using static System.NumericsX.OpenStack.OpenStack;
using GlIndex = System.Int32;

namespace System.NumericsX.OpenStack.Gngine.Render
{
    public struct OverlayVertex
    {
        public int vertexNum;
        public float st0;
        public float st1;
    }

    public class OverlaySurface
    {
        public int surfaceNum;
        public int surfaceId;
        public int numIndexes;
        public GlIndex[] indexes;
        public int numVerts;
        public OverlayVertex[] verts;
    }

    public struct OverlayMaterial
    {
        public Material material;
        public List<OverlaySurface> surfaces;
    }

    public unsafe class RenderModelOverlay
    {
        const int MAX_OVERLAY_SURFACES = 16;

        readonly List<OverlayMaterial> materials = new();

        public void Dispose()
        {
            int i, k;

            for (k = 0; k < materials.Count; k++)
            {
                for (i = 0; i < materials[k].surfaces.Count; i++) FreeSurface(materials[k].surfaces[i]);
                materials[k].surfaces.Clear();
            }
            materials.Clear();
        }

        public static RenderModelOverlay Alloc()
            => new();

        public static void Free(ref RenderModelOverlay overlay)
            => overlay = null;

        void FreeSurface(OverlaySurface surface) { }

        // Projects an overlay onto deformable geometry and can be added to a render entity to allow decals on top of dynamic models.
        // This does not generate tangent vectors, so it can't be used with light interaction shaders. Materials for overlays should always
        // be clamped, because the projected texcoords can run well off the texture since no new clip vertexes are generated.
        public unsafe void CreateOverlay(IRenderModel model, Plane[] localTextureAxis, Material material)
        {
            int i, maxVerts, maxIndexes, surfNum;

            // count up the maximum possible vertices and indexes per surface
            maxVerts = 0;
            maxIndexes = 0;
            for (surfNum = 0; surfNum < model.NumSurfaces; surfNum++)
            {
                var surf = model.Surface(surfNum);
                if (surf.geometry.numVerts > maxVerts) maxVerts = surf.geometry.numVerts;
                if (surf.geometry.numIndexes > maxIndexes) maxIndexes = surf.geometry.numIndexes;
            }

            // make temporary buffers for the building process
            var overlayVerts = stackalloc OverlayVertex[maxVerts];
            var overlayIndexes = stackalloc GlIndex[maxIndexes];

            // pull out the triangles we need from the base surfaces
            for (surfNum = 0; surfNum < model.NumBaseSurfaces; surfNum++)
            {
                var surf = model.Surface(surfNum);
                if (surf.geometry == null || surf.shader == null) continue;

                // some surfaces can explicitly disallow overlays
                if (!surf.shader.AllowOverlays) continue;

                var stri = surf.geometry;
                var stri_verts = stri.verts.Value; var stri_indexes = stri.indexes.Value;

                // try to cull the whole surface along the first texture axis
                var d = stri.bounds.PlaneDistance(localTextureAxis[0]);
                if (d < 0f || d > 1f) continue;

                // try to cull the whole surface along the second texture axis
                d = stri.bounds.PlaneDistance(localTextureAxis[1]);
                if (d < 0f || d > 1f) continue;

                var cullBits = stackalloc byte[stri.numVerts];
                var texCoords = stackalloc Vector2[stri.numVerts];

                fixed (Plane* localTextureAxisP = localTextureAxis)
                fixed (DrawVert* vertsD = stri_verts)
                    Simd.OverlayPointCull(cullBits, texCoords, localTextureAxisP, vertsD, stri.numVerts);

                var vertexRemap = stackalloc GlIndex[stri.numVerts];
                Simd.Memset(vertexRemap, -1, sizeof(GlIndex) * stri.numVerts);

                // find triangles that need the overlay
                var numVerts = 0;
                var numIndexes = 0;
                var triNum = 0;
                for (var index = 0; index < stri.numIndexes; index += 3, triNum++)
                {
                    var v1 = stri_indexes[index + 0];
                    var v2 = stri_indexes[index + 1];
                    var v3 = stri_indexes[index + 2];

                    // skip triangles completely off one side
                    if ((cullBits[v1] & cullBits[v2] & cullBits[v3]) != 0) continue;

                    // we could do more precise triangle culling, like the light interaction does, if desired

                    // keep this triangle
                    for (var vnum = 0; vnum < 3; vnum++)
                    {
                        var ind = stri_indexes[index + vnum];
                        if (vertexRemap[ind] == -1)
                        {
                            vertexRemap[ind] = numVerts;

                            overlayVerts[numVerts].vertexNum = ind;
                            overlayVerts[numVerts].st0 = texCoords[ind].x;
                            overlayVerts[numVerts].st1 = texCoords[ind].y;

                            numVerts++;
                        }
                        overlayIndexes[numIndexes++] = vertexRemap[ind];
                    }
                }

                if (numIndexes == 0) continue;

                var s = new OverlaySurface
                {
                    surfaceNum = surfNum,
                    surfaceId = surf.id,
                    verts = new OverlayVertex[numVerts],
                    numVerts = numVerts,
                    indexes = new GlIndex[numIndexes],
                    numIndexes = numIndexes
                };

                for (i = 0; i < materials.Count; i++) if (materials[i].material == material) break;
                if (i < materials.Count) materials[i].surfaces.Add(s);
                else { var mat = new OverlayMaterial { material = material }; mat.surfaces.Add(s); materials.Add(mat); }
            }

            // remove the oldest overlay surfaces if there are too many per material
            for (i = 0; i < materials.Count; i++) while (materials[i].surfaces.Count > MAX_OVERLAY_SURFACES) { FreeSurface(materials[i].surfaces[0]); materials[i].surfaces.RemoveAt(0); }
        }

        // Creates new model surfaces for baseModel, which should be a static instantiation of a dynamic model.
        public void AddOverlaySurfacesToModel(IRenderModel baseModel)
        {
            int i, j, k, numVerts, numIndexes, surfaceNum;
            ModelSurface baseSurf;
            RenderModelStatic staticModel;
            OverlaySurface surf;
            SrfTriangles newTri;
            ModelSurface newSurf;

            if (baseModel == null || baseModel.IsDefaultModel) return;

            // md5 models won't have any surfaces when r_showSkel is set
            if (baseModel.NumSurfaces == 0) return;

            if (baseModel.IsDynamicModel != DynamicModel.DM_STATIC) common.Error("RenderModelOverlay::AddOverlaySurfacesToModel: baseModel is not a static model");

            Debug.Assert(baseModel is RenderModelStatic);
            staticModel = (RenderModelStatic)baseModel;

            staticModel.overlaysAdded = 0;

            if (materials.Count == 0) { staticModel.DeleteSurfacesWithNegativeId(); return; }

            for (k = 0; k < materials.Count; k++)
            {
                numVerts = numIndexes = 0;
                for (i = 0; i < materials[k].surfaces.Count; i++)
                {
                    numVerts += materials[k].surfaces[i].numVerts;
                    numIndexes += materials[k].surfaces[i].numIndexes;
                }

                if (staticModel.FindSurfaceWithId(-1 - k, out surfaceNum)) newSurf = staticModel.surfaces[surfaceNum];
                else staticModel.surfaces.Add(newSurf = new ModelSurface
                {
                    geometry = null,
                    shader = materials[k].material,
                    id = -1 - k
                });

                if (newSurf.geometry == null || newSurf.geometry.numVerts < numVerts || newSurf.geometry.numIndexes < numIndexes)
                {
                    R_FreeStaticTriSurf(newSurf.geometry);
                    newSurf.geometry = R_AllocStaticTriSurf();
                    R_AllocStaticTriSurfVerts(newSurf.geometry, numVerts);
                    R_AllocStaticTriSurfIndexes(newSurf.geometry, numIndexes);
                    fixed (DrawVert* vertsD = newSurf.geometry.verts) Simd.Memset(vertsD, 0, numVerts * sizeof(DrawVert));
                }
                else R_FreeStaticTriSurfVertexCaches(newSurf.geometry);

                newTri = newSurf.geometry;
                var newTri_verts = newTri.verts; var newTri_indexes = newTri.indexes;
                numVerts = numIndexes = 0;

                for (i = 0; i < materials[k].surfaces.Count; i++)
                {
                    surf = materials[k].surfaces[i];

                    // get the model surface for this overlay surface
                    baseSurf = surf.surfaceNum < staticModel.NumSurfaces
                        ? staticModel.Surface(surf.surfaceNum)
                        : null;

                    // if the surface ids no longer match
                    if (baseSurf == null || baseSurf.id != surf.surfaceId)
                    {
                        // find the surface with the correct id
                        if (staticModel.FindSurfaceWithId(surf.surfaceId, out surf.surfaceNum)) baseSurf = staticModel.Surface(surf.surfaceNum);
                        // the surface with this id no longer exists
                        else { FreeSurface(surf); materials[k].surfaces.RemoveAt(i); i--; continue; }
                    }

                    // copy indexes;
                    for (j = 0; j < surf.numIndexes; j++) newTri_indexes[numIndexes + j] = numVerts + surf.indexes[j];
                    numIndexes += surf.numIndexes;

                    // copy vertices
                    var baseSurf_geometry_verts = baseSurf.geometry.verts;
                    for (j = 0; j < surf.numVerts; j++)
                    {
                        var overlayVert = surf.verts[j];

                        newTri_verts[numVerts].st.x = overlayVert.st0;
                        newTri_verts[numVerts].st.y = overlayVert.st1;

                        if (overlayVert.vertexNum >= baseSurf.geometry.numVerts)
                        {
                            // This can happen when playing a demofile and a model has been changed since it was recorded, so just issue a warning and go on.
                            common.Warning("RenderModelOverlay::AddOverlaySurfacesToModel: overlay vertex out of range.  Model has probably changed since generating the overlay.");
                            //FreeSurface(ref surf);
                            materials[k].surfaces.RemoveAt(i);
                            staticModel.DeleteSurfaceWithId(newSurf.id);
                            return;
                        }
                        newTri_verts[numVerts].xyz = baseSurf_geometry_verts[overlayVert.vertexNum].xyz;
                        numVerts++;
                    }
                }

                newTri.numVerts = numVerts;
                newTri.numIndexes = numIndexes;
                R_BoundTriSurf(newTri);

                staticModel.overlaysAdded++;    // so we don't create an overlay on an overlay surface
            }
        }

        // Removes overlay surfaces from the model.
        public static void RemoveOverlaySurfacesFromModel(IRenderModel baseModel)
        {
            RenderModelStatic staticModel;

            Debug.Assert(baseModel is RenderModelStatic);
            staticModel = (RenderModelStatic)baseModel;

            staticModel.DeleteSurfacesWithNegativeId();
            staticModel.overlaysAdded = 0;
        }

        public void ReadFromDemoFile(VFileDemo f) { }
        public void WriteToDemoFile(VFileDemo f) { }
    }
}