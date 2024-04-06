//using GL_INDEX_TYPE = System.UInt32; // GL_UNSIGNED_INT
using System.Runtime.InteropServices;
using GlIndex = System.Int32;

namespace System.NumericsX.OpenStack.Gngine.Render
{
    public static partial class R
    {
        // shared between the renderer, game, and Maya export DLL
        public const string MD5_VERSION_STRING = "MD5Version";
        public const string MD5_MESH_EXT = "md5mesh";
        public const string MD5_ANIM_EXT = "md5anim";
        public const string MD5_CAMERA_EXT = "md5camera";
        public const int MD5_VERSION = 10;
    }

    public struct SilEdge
    {
        // NOTE: making this a glIndex is dubious, as there can be 2x the faces as verts
        public GlIndex p1, p2;                  // planes defining the edge
        public GlIndex v1, v2;                  // verts defining the edge
    }

    //// this is used for calculating unsmoothed normals and tangents for deformed models
    //public unsafe struct DominantTri
    //{
    //    public GlIndex v2, v3;
    //    public fixed float normalizationScale[3];
    //}

    public struct LightingCache
    {
        public Vector3 localLightVector;               // this is the statically computed vector to the light in texture space for cards without vertex programs
    }

    public struct ShadowCache
    {
        public const int ALLOC16 = 1;
        public Vector4 xyz;                            // we use homogenous coordinate tricks
    }

    // our only drawing geometry type
    public unsafe class SrfTriangles : BlockAllocElement<SrfTriangles>
    {
        public static int sizeOf = Marshal.SizeOf(new SrfTriangles());

        public Bounds bounds;                   // for culling

        public int ambientViewCount;            // if == tr.viewCount, it is visible this view

        public bool generateNormals;            // create normals from geometry, instead of using explicit ones
        public bool tangentsCalculated;         // set when the vertex tangents have been calculated
        public bool facePlanesCalculated;       // set when the face planes have been calculated
        public bool perfectHull;                // true if there aren't any dangling edges
        public bool deformedSurface;            // if true, indexes, silIndexes, mirrorVerts, and silEdges are pointers into the original surface, and should not be freed

        public int numVerts;                    // number of vertices
        public DrawVert[] verts;  // vertices, allocated with special allocator

        public int numIndexes;                  // for shadows, this has both front and rear end caps and silhouette planes
        public GlIndex[] indexes; // indexes, allocated with special allocator

        public GlIndex[] silIndexes;  // indexes changed to be the first vertex with same XYZ, ignoring normal and texcoords

        public int numMirroredVerts;            // this many verts at the end of the vert list are tangent mirrors
        public int[] mirroredVerts;             // tri->mirroredVerts[0] is the mirror of tri->numVerts - tri->numMirroredVerts + 0

        public int numDupVerts;                 // number of duplicate vertexes
        public int[] dupVerts;                  // pairs of the number of the first vertex and the number of the duplicate vertex

        public int numSilEdges;                 // number of silhouette edges
        public SilEdge[] silEdges;              // silhouette edges

        public Plane[] facePlanes;              // [numIndexes/3] plane equations

        public DominantTri[] dominantTris;      // [numVerts] for deformed surface fast tangent calculation

        public int numShadowIndexesNoFrontCaps; // shadow volumes with front caps omitted
        public int numShadowIndexesNoCaps;      // shadow volumes with the front and rear caps omitted

        public int shadowCapPlaneBits;          // bits 0-5 are set when that plane of the interacting light has triangles projected on it, which means that if the view is on the outside of that
                                                // plane, we need to draw the rear caps of the shadow volume turboShadows will have SHADOW_CAP_INFINITE

        public ShadowCache[] shadowVertexes;    // these will be copied to shadowCache when it is going to be drawn. these are NULL when vertex programs are available

        public SrfTriangles ambientSurface;       // for light interactions, point back at the original surface that generated the interaction, which we will get the ambientCache from

        public SrfTriangles nextDeferredFree;       // chain of tris to free next frame

        // data in vertex object space, not directly readable by the CPU
        public VertCache indexCache;                // int
        public VertCache ambientCache;              // DrawVert
        public VertCache lightingCache;             // lightingCache_t
        public VertCache shadowCache;               // shadowCache_t

        public SrfTriangles() { }
        public SrfTriangles(SrfTriangles clone) { }

        internal void memset()
        {
            throw new NotImplementedException();
        }
    }

    //List<SrfTriangles> TriList;

    public class ModelSurface
    {
        public int id;
        public Material shader;
        public SrfTriangles geometry;
    }

    public enum DynamicModel
    {
        DM_STATIC,      // never creates a dynamic model
        DM_CACHED,      // once created, stays constant until the entity is updated (animating characters)
        DM_CONTINUOUS   // must be recreated for every single view (time dependent things like particles)
    }

    public enum JointHandle
    {
        INVALID_JOINT = -1
    }

    public class MD5Joint
    {
        public MD5Joint() => parent = null;
        public string name;
        public MD5Joint parent;
    }

    // the init methods may be called again on an already created model when a reloadModels is issued
    public interface IRenderModel : IDisposable
    {
        // Loads static models only, dynamic models must be loaded by the modelManager
        void InitFromFile(string fileName);

        // renderBump uses this to load the very high poly count models, skipping the shadow and tangent generation, along with some surface cleanup to make it load faster
        void PartialInitFromFile(string fileName);

        // this is used for dynamically created surfaces, which are assumed to not be reloadable. It can be called again to clear out the surfaces of a dynamic model for regeneration.
        void InitEmpty(string name);

        // dynamic model instantiations will be created with this the geometry data will be owned by the model, and freed when it is freed
        // the geoemtry should be raw triangles, with no extra processing
        void AddSurface(ModelSurface surface);

        // cleans all the geometry and performs cross-surface processing like shadow hulls Creates the duplicated back side geometry for two sided, alpha tested, lit materials
        // This does not need to be called if none of the surfaces added with AddSurface require light interaction, and all the triangles are already well formed.
        void FinishSurfaces();

        // frees all the data, but leaves the class around for dangling references, which can regenerate the data with LoadModel()
        void PurgeModel();

        // resets any model information that needs to be reset on a same level load etc.. currently only implemented for liquids
        void Reset();

        // used for initial loads, reloadModel, and reloading the data of purged models Upon exit, the model will absolutely be valid, but possibly as a default model
        void LoadModel();

        // internal use
        bool IsLoaded { get; }
        bool IsLevelLoadReferenced { get; set; }

        // models that are already loaded at level start time will still touch their data to make sure they are kept loaded
        void TouchData();

        // dump any ambient caches on the model surfaces
        void FreeVertexCache();

        // returns the name of the model
        string Name { get; }

        // prints a detailed report on the model for printModel
        void Print();

        // prints a single line report for listModels
        void List();

        // reports the amount of memory (roughly) consumed by the model
        int Memory { get; }

        // for reloadModels
        DateTime Timestamp { get; }

        // returns the number of surfaces
        int NumSurfaces { get; }

        // NumBaseSurfaces will not count any overlays added to dynamic models
        int NumBaseSurfaces { get; }

        // get a pointer to a surface
        ModelSurface Surface(int surfaceNum);

        // Allocates surface triangles. Allocates memory for srfTriangles_t::verts and srfTriangles_t::indexes
        // The allocated memory is not initialized. srfTriangles_t::numVerts and srfTriangles_t::numIndexes are set to zero.
        SrfTriangles AllocSurfaceTriangles(int numVerts, int numIndexes);

        // Frees surfaces triangles.
        void FreeSurfaceTriangles(ref SrfTriangles tris);

        // created at load time by stitching together all surfaces and sharing the maximum number of edges.  This may be incorrect if a skin file
        // remaps surfaces between shadow casting and non-shadow casting, or if some surfaces are noSelfShadow and others aren't
        SrfTriangles ShadowHull { get; }

        // models of the form "_area*" may have a prelight shadow model associated with it
        bool IsStaticWorldModel { get; }

        // models parsed from inside map files or dynamically created cannot be reloaded by reloadmodels
        bool IsReloadable { get; }

        // md3, md5, particles, etc
        DynamicModel IsDynamicModel { get; }

        // if the load failed for any reason, this will return true
        bool IsDefaultModel { get; }

        // dynamic models should return a fast, conservative approximation static models should usually return the exact value
        Bounds Bounds(RenderEntity ent = null);

        // returns value != 0.0f if the model requires the depth hack
        float DepthHack { get; }

        // returns a static model based on the definition and view currently, this will be regenerated for every view, even though
        // some models, like character meshes, could be used for multiple (mirror) views in a frame, or may stay static for multiple frames (corpses)
        // The renderer will delete the returned dynamic model the next view This isn't const, because it may need to reload a purged model if it wasn't precached correctly.
        IRenderModel InstantiateDynamicModel(RenderEntity ent, ViewDef view, IRenderModel cachedModel);

        // Returns the number of joints or 0 if the model is not an MD5
        int NumJoints { get; }

        // Returns the MD5 joints or NULL if the model is not an MD5
        MD5Joint Joints { get; }

        // Returns the handle for the joint with the given name.
        JointHandle GetJointHandle(string name);

        // Returns the name for the joint with the given handle.
        string GetJointName(JointHandle handle);

        // Returns the default animation pose or NULL if the model is not an MD5.
        JointQuat DefaultPose { get; }

        // Returns number of the joint nearest to the given triangle.
        int NearestJoint(int surfaceNum, int a, int c, int b);

        // Writing to and reading from a demo file.
        void ReadFromDemoFile(VFileDemo f);
        void WriteToDemoFile(VFileDemo f);
    }
}