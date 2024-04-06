using System.Collections.Generic;
using System.IO;
using static System.NumericsX.OpenStack.Gngine.Gngine;
using static System.NumericsX.OpenStack.Gngine.Render.R;
using static System.NumericsX.OpenStack.OpenStack;

namespace System.NumericsX.OpenStack.Gngine.Render
{
    public unsafe class RenderModelStatic : IRenderModel
    {
        public List<ModelSurface> surfaces;
        public Bounds bounds;
        public int overlaysAdded;

        protected int lastModifiedFrame;
        protected int lastArchivedFrame;

        protected string name;
        protected SrfTriangles shadowHull;
        protected bool isStaticWorldModel;
        protected bool defaulted;
        protected bool purged;                  // eventually we will have dynamic reloading
        protected bool fastLoad;                // don't generate tangents and shadow data
        protected bool reloadable;              // if not, reloadModels won't check timestamp
        protected bool levelLoadReferenced; // for determining if it needs to be freed
        protected DateTime timeStamp;

        protected static CVar r_mergeModelSurfaces = new("r_mergeModelSurfaces", "1", CVAR.BOOL | CVAR.RENDERER, "combine model surfaces with the same material");   // combine model surfaces with the same material
        protected static CVar r_slopVertex = new("r_slopVertex", "0.01", CVAR.RENDERER, "merge xyz coordinates this far apart");           // merge xyz coordinates this far apart
        protected static CVar r_slopTexCoord = new("r_slopTexCoord", "0.001", CVAR.RENDERER, "merge texture coordinates this far apart");         // merge texture coordinates this far apart
        protected static CVar r_slopNormal = new("r_slopNormal", "0.02", CVAR.RENDERER, "merge normals that dot less than this");           // merge normals that dot less than this

        // the inherited public interface
        public static IRenderModel Alloc() => throw new NotImplementedException();

        public RenderModelStatic()
        {
            name = "<undefined>";
            bounds.Clear();
            lastModifiedFrame = 0;
            lastArchivedFrame = 0;
            overlaysAdded = 0;
            shadowHull = null;
            isStaticWorldModel = false;
            defaulted = false;
            purged = false;
            fastLoad = false;
            reloadable = true;
            levelLoadReferenced = false;
            timeStamp = DateTime.MinValue;
        }
        public void Dispose()
            => PurgeModel();

        public virtual void InitFromFile(string fileName)
        {
            bool loaded;
            InitEmpty(fileName);

            var extension = Path.GetExtension(name).ToLowerInvariant();
            throw new NotImplementedException();
            //if (extension == ".ase") { loaded = LoadASE(name); reloadable = true; }
            //else if (extension == ".lwo") { loaded = LoadLWO(name); reloadable = true; }
            //else if (extension == ".flt") { loaded = LoadFLT(name); reloadable = true; }
            //else if (extension == ".ma") { loaded = LoadMA(name); reloadable = true; }
            //else { common.Warning($"RenderModelStatic::InitFromFile: unknown type for model: \'{name}\'"); loaded = false; }
            if (!loaded) { common.Warning($"Couldn't load model: '{name}'"); MakeDefaultModel(); return; }

            // it is now available for use
            purged = false;

            // create the bounds for culling and dynamic surface creation
            FinishSurfaces();
        }

        public virtual void PartialInitFromFile(string fileName)
        {
            fastLoad = true;
            InitFromFile(fileName);
        }

        public virtual void PurgeModel()
        {
            for (var i = 0; i < surfaces.Count; i++)
            {
                var surf = surfaces[i];
                if (surf.geometry != null) R_FreeStaticTriSurf(surf.geometry);
            }
            surfaces.Clear();

            purged = true;
        }

        public virtual void Reset() { }

        public virtual void LoadModel()
        {
            PurgeModel();
            InitFromFile(name);
        }

        public virtual bool IsLoaded
            => !purged;

        public virtual bool IsLevelLoadReferenced
        {
            get => levelLoadReferenced;
            set => levelLoadReferenced = value;
        }

        public virtual void TouchData()
        {
            for (var i = 0; i < surfaces.Count; i++)
            {
                var surf = surfaces[i];
                declManager.FindMaterial(surf.shader.Name); // re-find the material to make sure it gets added to the level keep list
            }
        }

        public virtual void InitEmpty(string name)
        {
            // model names of the form _area* are static parts of the world, and have already been considered for optimized shadows
            // other model names are inline entity models, and need to be shadowed normally
            isStaticWorldModel = name.StartsWith("_area");
            this.name = name;
            reloadable = false; // if it didn't come from a file, we can't reload it
            PurgeModel();
            purged = false;
            bounds.Zero();
        }

        public virtual void AddSurface(ModelSurface surface)
        {
            surfaces.Add(surface);
            if (surface.geometry != null) bounds += surface.geometry.bounds;
        }

        // The mergeShadows option allows surfaces with different textures to share silhouette edges for shadow calculation, instead of leaving shared edges hanging.
        // If any of the original shaders have the noSelfShadow flag set, the surfaces can't be merged, because they will need to be drawn in different order.
        // If there is only one surface, a separate merged surface won't be generated.
        // A model with multiple surfaces can't later have a skinned shader change the state of the noSelfShadow flag.
        //-----------------
        // Creates mirrored copies of two sided surfaces with normal maps, which would otherwise light funny.
        // Extends the bounds of deformed surfaces so they don't cull incorrectly at screen edges.
        public virtual void FinishSurfaces()
        {
            int i, totalVerts, totalIndexes;

            purged = false;

            // make sure we don't have a huge bounds even if we don't finish everything
            bounds.Zero();

            if (surfaces.Count == 0) return;

            // renderBump doesn't care about most of this
            if (fastLoad)
            {
                bounds.Zero();
                for (i = 0; i < surfaces.Count; i++)
                {
                    var surf = surfaces[i];
                    R_BoundTriSurf(surf.geometry);
                    bounds.AddBounds(surf.geometry.bounds);
                }
                return;
            }

            // cleanup all the final surfaces, but don't create sil edges
            totalVerts = 0;
            totalIndexes = 0;

            // decide if we are going to merge all the surfaces into one shadower
            var numOriginalSurfaces = surfaces.Count;

            // make sure there aren't any NULL shaders or geometry
            for (i = 0; i < numOriginalSurfaces; i++)
            {
                var surf = surfaces[i];
                if (surf.geometry == null || surf.shader == null) { MakeDefaultModel(); common.Error($"Model {name}, surface {i} had NULL geometry"); }
                if (surf.shader == null) { MakeDefaultModel(); common.Error("Model {name}, surface {i} had NULL shader"); }
            }

            // duplicate and reverse triangles for two sided bump mapped surfaces note that this won't catch surfaces that have their shaders dynamically
            // changed, and won't work with animated models. It is better to create completely separate surfaces, rather than
            // add vertexes and indexes to the existing surface, because the tangent generation wouldn't like the acute shared edges
            for (i = 0; i < numOriginalSurfaces; i++)
            {
                var surf = surfaces[i];
                if (surf.shader.ShouldCreateBackSides)
                {
                    var newTri = R_CopyStaticTriSurf(surf.geometry);
                    R_ReverseTriangles(newTri);
                    AddSurface(new ModelSurface { shader = surf.shader, geometry = newTri });
                }
            }

            // clean the surfaces
            for (i = 0; i < surfaces.Count; i++)
            {
                var surf = surfaces[i];
                R_CleanupTriangles(surf.geometry, surf.geometry.generateNormals, true, surf.shader.UseUnsmoothedTangents);
                if (surf.shader.SurfaceCastsShadow) { totalVerts += surf.geometry.numVerts; totalIndexes += surf.geometry.numIndexes; }
            }

            // add up the total surface area for development information
            for (i = 0; i < surfaces.Count; i++)
            {
                var surf = surfaces[i];
                var tri = surf.geometry;
                var tri_verts = tri.verts; var tri_indexes = tri.indexes;
                for (var j = 0; j < tri.numIndexes; j += 3)
                {
                    var area = Winding.TriangleArea(tri_verts[tri_indexes[j]].xyz, tri_verts[tri_indexes[j + 1]].xyz, tri_verts[tri_indexes[j + 2]].xyz);
                    surf.shader.AddToSurfaceArea(area);
                }
            }

            // calculate the bounds
            if (surfaces.Count == 0) bounds.Zero();
            else
            {
                bounds.Clear();
                for (i = 0; i < surfaces.Count; i++)
                {
                    var surf = surfaces[i];

                    // if the surface has a deformation, increase the bounds the amount here is somewhat arbitrary, designed to handle
                    // autosprites and flares, but could be done better with exact deformation information.
                    // Note that this doesn't handle deformations that are skinned in at run time...
                    if (surf.shader.Deform != DFRM.NONE)
                    {
                        var tri = surf.geometry;
                        var mid = (tri.bounds[1] + tri.bounds[0]) * 0.5f;
                        var radius = (tri.bounds[0] - mid).Length;
                        radius += 20f;

                        tri.bounds[0].x = mid.x - radius;
                        tri.bounds[0].y = mid.y - radius;
                        tri.bounds[0].z = mid.z - radius;

                        tri.bounds[1].x = mid.x + radius;
                        tri.bounds[1].y = mid.y + radius;
                        tri.bounds[1].z = mid.z + radius;
                    }

                    // add to the model bounds
                    bounds.AddBounds(surf.geometry.bounds);
                }
            }
        }

        // We are about to restart the vertex cache, so dump everything
        public virtual void FreeVertexCache()
        {
            for (var j = 0; j < surfaces.Count; j++)
            {
                var tri = surfaces[j].geometry;
                if (tri == null) continue;
                if (tri.ambientCache != null) { vertexCache.Free(ref tri.ambientCache); tri.ambientCache = null; }
                // static shadows may be present
                if (tri.shadowCache != null) { vertexCache.Free(ref tri.shadowCache); tri.shadowCache = null; }
            }
        }

        public virtual string Name
            => name;

        public virtual void Print()
        {
            common.Printf($"{name}\n");
            common.Printf("Static model.\n");
            common.Printf($"bounds: ({bounds[0].x} {bounds[0].y} {bounds[0].z}) to ({bounds[1].x} {bounds[1].y} {bounds[1].z})\n");
            common.Printf("    verts  tris material\n");
            for (var i = 0; i < NumSurfaces; i++)
            {
                var surf = Surface(i);
                var tri = surf.geometry;
                var material = surf.shader;
                if (tri == null) { common.Printf($"{i,2}: {material.Name}, NULL surface geometry\n"); continue; }
                common.Printf($"{i,2}: {tri.numVerts,5} {tri.numIndexes / 3,5} {material.Name}");
                common.Printf(tri.generateNormals ? " (smoothed)\n" : "\n");
            }
        }

        public virtual void List()
        {
            int totalTris = 0, totalVerts = 0, totalBytes = Memory;

            var closed = 'C';
            for (var j = 0; j < NumSurfaces; j++)
            {
                var surf = Surface(j);
                if (surf.geometry == null) continue;
                if (!surf.geometry.perfectHull) closed = ' ';
                totalTris += surf.geometry.numIndexes / 3;
                totalVerts += surf.geometry.numVerts;
            }
            common.Printf($"{closed}{totalBytes / 1024,4}k {NumSurfaces,3} {totalVerts,4} {totalTris,4} {Name}");

            if (IsDynamicModel == DynamicModel.DM_CACHED) common.Printf(" (DM_CACHED)");
            if (IsDynamicModel == DynamicModel.DM_CONTINUOUS) common.Printf(" (DM_CONTINUOUS)");
            if (defaulted) common.Printf(" (DEFAULTED)");
            if (bounds[0].x >= bounds[1].x) common.Printf(" (EMPTY BOUNDS)");
            if (bounds[1].x - bounds[0].x > 100000) common.Printf(" (HUGE BOUNDS)");
            common.Printf("\n");
        }

        public virtual int Memory
            => 0;

        public virtual DateTime Timestamp
            => timeStamp;

        public virtual int NumSurfaces
            => surfaces.Count;

        public virtual int NumBaseSurfaces
            => surfaces.Count - overlaysAdded;

        public virtual ModelSurface Surface(int surfaceNum)
            => surfaces[surfaceNum];

        public virtual SrfTriangles AllocSurfaceTriangles(int numVerts, int numIndexes)
        {
            var tri = R_AllocStaticTriSurf();
            R_AllocStaticTriSurfVerts(tri, numVerts);
            R_AllocStaticTriSurfIndexes(tri, numIndexes);
            return tri;
        }

        public virtual void FreeSurfaceTriangles(ref SrfTriangles tris)
            => R_FreeStaticTriSurf(tris);

        public virtual SrfTriangles ShadowHull
            => shadowHull;

        public virtual bool IsStaticWorldModel
            => isStaticWorldModel;

        public virtual DynamicModel IsDynamicModel
            => DynamicModel.DM_STATIC; // dynamic subclasses will override this

        public virtual bool IsDefaultModel
            => defaulted;

        public virtual bool IsReloadable
            => reloadable;

        public virtual IRenderModel InstantiateDynamicModel(RenderEntity ent, ViewDef view, IRenderModel cachedModel)
        {
            if (cachedModel != null) { cachedModel.Dispose(); cachedModel = null; }
            common.Error($"InstantiateDynamicModel called on static model '{name}'");
            return null;
        }

        public virtual int NumJoints
            => 0;

        public virtual MD5Joint Joints
            => default;

        public virtual JointHandle GetJointHandle(string name)
            => JointHandle.INVALID_JOINT;

        public virtual string GetJointName(JointHandle handle)
            => string.Empty;

        public virtual JointQuat DefaultPose
            => default;

        public virtual int NearestJoint(int surfaceNum, int a, int b, int c)
            => (int)JointHandle.INVALID_JOINT;

        public virtual Bounds Bounds(RenderEntity ent)
            => bounds;

        public virtual void ReadFromDemoFile(VFileDemo f)
        {
            PurgeModel();

            InitEmpty(f.ReadHashString());

            int i, j;
            f.ReadInt(out var numSurfaces);

            for (i = 0; i < numSurfaces; i++)
            {
                ModelSurface surf = new();
                surf.shader = declManager.FindMaterial(f.ReadHashString());

                var tri = R_AllocStaticTriSurf();
                var tri_verts = tri.verts; var tri_indexes = tri.indexes;
                f.ReadInt(out tri.numIndexes);
                R_AllocStaticTriSurfIndexes(tri, tri.numIndexes);
                for (j = 0; j < tri.numIndexes; ++j) f.ReadInt(out tri_indexes[j]);

                f.ReadInt(out tri.numVerts);
                R_AllocStaticTriSurfVerts(tri, tri.numVerts);
                for (j = 0; j < tri.numVerts; ++j)
                {
                    f.ReadVec3(out tri_verts[j].xyz);
                    f.ReadVec2(out tri_verts[j].st);
                    f.ReadVec3(out tri_verts[j].normal);
                    f.ReadVec3(out tri_verts[j].tangents0);
                    f.ReadVec3(out tri_verts[j].tangents1);
                    f.ReadUnsignedChar(out tri_verts[j].color0);
                    f.ReadUnsignedChar(out tri_verts[j].color1);
                    f.ReadUnsignedChar(out tri_verts[j].color2);
                    f.ReadUnsignedChar(out tri_verts[j].color3);
                }

                surf.geometry = tri;
                this.AddSurface(surf);
            }
            this.FinishSurfaces();
        }

        public virtual void WriteToDemoFile(VFileDemo f)
        {
            // note that it has been updated
            lastArchivedFrame = tr.frameCount;

            var data0 = (int)DC.DEFINE_MODEL;
            f.WriteInt(data0);
            f.WriteHashString(Name);

            int i, j;
            f.WriteInt(surfaces.Count);
            for (i = 0; i < surfaces.Count; i++)
            {
                var surf = surfaces[i];
                f.WriteHashString(surf.shader.Name);

                var tri = surf.geometry;
                var tri_verts = tri.verts; var tri_indexes = tri.indexes;
                f.WriteInt(tri.numIndexes);
                for (j = 0; j < tri.numIndexes; ++j) f.WriteInt(tri_indexes[j]);
                f.WriteInt(tri.numVerts);
                for (j = 0; j < tri.numVerts; ++j)
                {
                    f.WriteVec3(tri_verts[j].xyz);
                    f.WriteVec2(tri_verts[j].st);
                    f.WriteVec3(tri_verts[j].normal);
                    f.WriteVec3(tri_verts[j].tangents0);
                    f.WriteVec3(tri_verts[j].tangents1);
                    f.WriteUnsignedChar(tri_verts[j].color0);
                    f.WriteUnsignedChar(tri_verts[j].color1);
                    f.WriteUnsignedChar(tri_verts[j].color2);
                    f.WriteUnsignedChar(tri_verts[j].color3);
                }
            }
        }

        public virtual float DepthHack
            => 0f;

        static void AddCubeFace(SrfTriangles tri, Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4)
        {
            var tri_verts = tri.verts; var tri_indexes = tri.indexes;
            tri_verts[tri.numVerts + 0].Clear();
            tri_verts[tri.numVerts + 0].xyz = v1 * 8;
            tri_verts[tri.numVerts + 0].st.x = 0;
            tri_verts[tri.numVerts + 0].st.y = 0;

            tri_verts[tri.numVerts + 1].Clear();
            tri_verts[tri.numVerts + 1].xyz = v2 * 8;
            tri_verts[tri.numVerts + 1].st.x = 1;
            tri_verts[tri.numVerts + 1].st.y = 0;

            tri_verts[tri.numVerts + 2].Clear();
            tri_verts[tri.numVerts + 2].xyz = v3 * 8;
            tri_verts[tri.numVerts + 2].st.x = 1;
            tri_verts[tri.numVerts + 2].st.y = 1;

            tri_verts[tri.numVerts + 3].Clear();
            tri_verts[tri.numVerts + 3].xyz = v4 * 8;
            tri_verts[tri.numVerts + 3].st.x = 0;
            tri_verts[tri.numVerts + 3].st.y = 1;

            tri_indexes[tri.numIndexes + 0] = tri.numVerts + 0;
            tri_indexes[tri.numIndexes + 1] = tri.numVerts + 1;
            tri_indexes[tri.numIndexes + 2] = tri.numVerts + 2;
            tri_indexes[tri.numIndexes + 3] = tri.numVerts + 0;
            tri_indexes[tri.numIndexes + 4] = tri.numVerts + 2;
            tri_indexes[tri.numIndexes + 5] = tri.numVerts + 3;

            tri.numVerts += 4;
            tri.numIndexes += 6;
        }

        public void MakeDefaultModel()
        {
            defaulted = true;

            // throw out any surfaces we already have
            PurgeModel();

            // create one new surface
            ModelSurface surf = new();

            var tri = R_AllocStaticTriSurf();

            surf.shader = tr.defaultMaterial;
            surf.geometry = tri;

            R_AllocStaticTriSurfVerts(tri, 24);
            R_AllocStaticTriSurfIndexes(tri, 36);

            AddCubeFace(tri, new Vector3(-1, 1, 1), new Vector3(1, 1, 1), new Vector3(1, -1, 1), new Vector3(-1, -1, 1));
            AddCubeFace(tri, new Vector3(-1, 1, -1), new Vector3(-1, -1, -1), new Vector3(1, -1, -1), new Vector3(1, 1, -1));

            AddCubeFace(tri, new Vector3(1, -1, 1), new Vector3(1, 1, 1), new Vector3(1, 1, -1), new Vector3(1, -1, -1));
            AddCubeFace(tri, new Vector3(-1, -1, 1), new Vector3(-1, -1, -1), new Vector3(-1, 1, -1), new Vector3(-1, 1, 1));

            AddCubeFace(tri, new Vector3(-1, -1, 1), new Vector3(1, -1, 1), new Vector3(1, -1, -1), new Vector3(-1, -1, -1));
            AddCubeFace(tri, new Vector3(-1, 1, 1), new Vector3(-1, 1, -1), new Vector3(1, 1, -1), new Vector3(1, 1, 1));

            tri.generateNormals = true;

            AddSurface(surf);
            FinishSurfaces();
        }

        public bool DeleteSurfaceWithId(int id)
        {
            for (var i = 0; i < surfaces.Count; i++) if (surfaces[i].id == id) { R_FreeStaticTriSurf(surfaces[i].geometry); surfaces.RemoveAt(i); return true; }
            return false;
        }

        public void DeleteSurfacesWithNegativeId()
        {
            for (var i = 0; i < surfaces.Count; i++) if (surfaces[i].id < 0) { R_FreeStaticTriSurf(surfaces[i].geometry); surfaces.RemoveAt(i); i--; }
        }

        public bool FindSurfaceWithId(int id, out int surfaceNum)
        {
            for (var i = 0; i < surfaces.Count; i++) if (surfaces[i].id == id) { surfaceNum = i; return true; }
            surfaceNum = default;
            return false;
        }
    }
}