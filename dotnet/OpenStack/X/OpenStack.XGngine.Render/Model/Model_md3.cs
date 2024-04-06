using System.NumericsX;
using System.Runtime.InteropServices;
using static Gengine.Lib;
using static Gengine.Render.TR;
using static System.NumericsX.OpenStack.OpenStack;
using static System.NumericsX.Platform;

namespace Gengine.Render
{
    static unsafe class ModelXMd3
    {
        public const int MD3_IDENT = ('3' << 24) + ('P' << 16) + ('D' << 8) + 'I';
        public const int MD3_VERSION = 15;

        // surface geometry should not exceed these limits
        public const int SHADER_MAX_VERTEXES = 1000;
        public const int SHADER_MAX_INDEXES = 6 * SHADER_MAX_VERTEXES;

        // limits
        public const int MD3_MAX_LODS = 4;
        public const int MD3_MAX_TRIANGLES = 8192; // per surface
        public const int MD3_MAX_VERTS = 4096; // per surface
        public const int MD3_MAX_SHADERS = 256;        // per surface
        public const int MD3_MAX_FRAMES = 1024;    // per model
        public const int MD3_MAX_SURFACES = 32;        // per model
        public const int MD3_MAX_TAGS = 16;        // per frame
        public const int MAX_MD3PATH = 64;		// from quake3

        // vertex scales
        public const float MD3_XYZ_SCALE = 1f / 64;
    }

    #region Records

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    class Md3Frame
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)] public Vector3[] bounds;
        public Vector3 localOrigin;
        public float radius;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)] public string name;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    class Md3Tag
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = ModelXMd3.MAX_MD3PATH)] public string name; // tag name
        public Vector3 origin;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)] public Vector3[] axis;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    class Md3Surface
    {
        public static readonly int SizeOf = (int)Marshal.OffsetOf(typeof(Md3Surface), "e_lfanew");

        public int ident;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = ModelXMd3.MAX_MD3PATH)] public string name; // polyset name

        public int flags;
        public int numFrames;          // all surfaces in a model should have the same

        public int numShaders;         // all surfaces in a model should have the same
        public int numVerts;

        public int numTriangles;
        public int ofsTriangles;

        public int ofsShaders;         // offset from start of md3Surface_t
        public int ofsSt;              // texture coords are common for all frames
        public int ofsXyzNormals;      // numVerts * numFrames

        public int ofsEnd;             // next surface follows

        // data
        public Md3Shader[] shaders;
        public Md3Triangle[] tris;
        public Md3St[] sts;
        public Md3XyzNormal[] xyzs;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    struct Md3Shader
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = ModelXMd3.MAX_MD3PATH)] public string name;
        public Material shader;         // for in-game use
    }

    [StructLayout(LayoutKind.Sequential)]
    struct Md3Triangle
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)] public int[] indexes;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct Md3St
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)] public float[] st;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct Md3XyzNormal
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)] public short[] xyz;
        public short normal;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    class Md3Header
    {
        public static readonly int SizeOf = (int)Marshal.OffsetOf(typeof(Md3Surface), "frames");

        public int ident;
        public int version;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = ModelXMd3.MAX_MD3PATH)] public string name; // model name

        public int flags;

        public int numFrames;
        public int numTags;
        public int numSurfaces;

        public int numSkins;

        public int ofsFrames;          // offset for first frame
        public int ofsTags;            // numFrames * numTags
        public int ofsSurfaces;        // first surface, others follow

        public int ofsEnd;             // end of file

        // data
        public Md3Frame[] frames;
        public Md3Tag[] tags;
        public Md3Surface[] surfaces;
    }

    #endregion

    public unsafe class RenderModelMD3 : RenderModelStatic
    {
        int index;          // model = tr.models[model.index]
        int dataSize;       // just for listing purposes
        Md3Header md3;            // only if type == MOD_MESH
        int numLods;

        public unsafe override void InitFromFile(string fileName)
        {
            int i, j;

            name = fileName;

            var size = fileSystem.ReadFile(fileName, out var buffer, out var _);
            if (size <= 0) return;

            var pinmodel = UnsafeX.ReadTSize<Md3Header>(Md3Header.SizeOf, buffer);
            var version = LittleInt(pinmodel.version);
            if (version != ModelXMd3.MD3_VERSION) { fileSystem.FreeFile(buffer); common.Warning($"InitFromFile: {fileName} has wrong version ({version} should be {ModelXMd3.MD3_VERSION})"); return; }

            size = LittleInt(pinmodel.ofsEnd);
            dataSize += size;

            md3 = UnsafeX.ReadTSize<Md3Header>(size, buffer);
            LittleInt(ref md3.ident);
            LittleInt(ref md3.version);
            LittleInt(ref md3.numFrames);
            LittleInt(ref md3.numTags);
            LittleInt(ref md3.numSurfaces);
            LittleInt(ref md3.ofsFrames);
            LittleInt(ref md3.ofsTags);
            LittleInt(ref md3.ofsSurfaces);
            LittleInt(ref md3.ofsEnd);

            if (md3.numFrames < 1) { common.Warning($"InitFromFile: {fileName} has no frames"); fileSystem.FreeFile(buffer); return; }

            // swap all the frames
            pinmodel.frames = UnsafeX.ReadTArray<Md3Frame>(buffer, pinmodel.ofsFrames, pinmodel.numFrames);
            for (i = 0; i < pinmodel.frames.Length; i++)
            {
                ref Md3Frame frame = ref pinmodel.frames[i];
                LittleFloat(ref frame.radius);
                LittleVector3(ref frame.bounds[0]);
                LittleVector3(ref frame.bounds[1]);
                LittleVector3(ref frame.localOrigin);
            }

            // swap all the tags
            pinmodel.tags = UnsafeX.ReadTArray<Md3Tag>(buffer, pinmodel.ofsTags, pinmodel.numTags * pinmodel.numFrames);
            for (i = 0; i < pinmodel.tags.Length; i++)
            {
                ref Md3Tag tag = ref pinmodel.tags[i];
                LittleVector3(ref tag.origin);
                LittleVector3(ref tag.axis[0]);
                LittleVector3(ref tag.axis[1]);
                LittleVector3(ref tag.axis[2]);
            }

            // swap all the surfaces
            pinmodel.surfaces = new Md3Surface[pinmodel.numSurfaces];
            var surfOfs = pinmodel.ofsSurfaces;
            for (i = 0; i < pinmodel.surfaces.Length; i++)
            {
                pinmodel.surfaces[i] = UnsafeX.ReadTSize<Md3Surface>(Md3Surface.SizeOf, buffer, surfOfs);
                ref Md3Surface surf = ref pinmodel.surfaces[i];
                LittleInt(ref surf.ident);
                LittleInt(ref surf.flags);
                LittleInt(ref surf.numFrames);
                LittleInt(ref surf.numShaders);
                LittleInt(ref surf.numTriangles);
                LittleInt(ref surf.ofsTriangles);
                LittleInt(ref surf.numVerts);
                LittleInt(ref surf.ofsShaders);
                LittleInt(ref surf.ofsSt);
                LittleInt(ref surf.ofsXyzNormals);
                LittleInt(ref surf.ofsEnd);

                if (surf.numVerts > ModelXMd3.SHADER_MAX_VERTEXES) common.Error($"InitFromFile: {fileName} has more than {ModelXMd3.SHADER_MAX_VERTEXES} verts on a surface ({surf.numVerts})");
                if (surf.numTriangles * 3 > ModelXMd3.SHADER_MAX_INDEXES) common.Error($"InitFromFile: {fileName} has more than {ModelXMd3.SHADER_MAX_INDEXES / 3} triangles on a surface ({surf.numTriangles})");

                // change to surface identifier
                surf.ident = 0;    //SF_MD3;

                // lowercase the surface name so skin compares are faster
                surf.name = surf.name.ToLowerInvariant();

                // strip off a trailing _1 or _2 this is a crutch for q3data being a mess
                j = surf.name.Length;
                if (j > 2 && surf.name[j - 2] == '_') surf.name = surf.name.Remove(j - 2);

                // register the shaders
                surf.shaders = UnsafeX.ReadTArray<Md3Shader>(buffer, surfOfs + surf.ofsShaders, surf.numShaders);
                for (j = 0; j < surf.shaders.Length; j++)
                {
                    ref Md3Shader shader = ref surf.shaders[j];
                    var sh = declManager.FindMaterial(shader.name);
                    shader.shader = sh;
                }

                // swap all the triangles
                surf.tris = UnsafeX.ReadTArray<Md3Triangle>(buffer, surfOfs + surf.ofsTriangles, surf.numTriangles);
                for (j = 0; j < surf.tris.Length; j++)
                {
                    ref Md3Triangle tri = ref surf.tris[j];
                    LittleInt(ref tri.indexes[0]);
                    LittleInt(ref tri.indexes[1]);
                    LittleInt(ref tri.indexes[2]);
                }

                // swap all the ST
                surf.sts = UnsafeX.ReadTArray<Md3St>(buffer, surfOfs + surf.ofsSt, surf.numVerts);
                for (j = 0; j < surf.sts.Length; j++)
                {
                    ref Md3St st = ref surf.sts[j];
                    LittleFloat(ref st.st[0]);
                    LittleFloat(ref st.st[1]);
                }

                // swap all the XyzNormals
                surf.xyzs = UnsafeX.ReadTArray<Md3XyzNormal>(buffer, surfOfs + surf.ofsXyzNormals, surf.numVerts * surf.numFrames);
                for (j = 0; j < surf.xyzs.Length; j++)
                {
                    ref Md3XyzNormal xyz = ref surf.xyzs[j];
                    LittleShort(ref xyz.xyz[0]);
                    LittleShort(ref xyz.xyz[1]);
                    LittleShort(ref xyz.xyz[2]);

                    LittleShort(ref xyz.normal);
                }

                // find the next surface
                surfOfs += surf.ofsEnd;
            }

            fileSystem.FreeFile(buffer);
        }

        public override DynamicModel IsDynamicModel
            => DynamicModel.DM_CACHED;

        void LerpMeshVertexes(SrfTriangles tri, Md3Surface surf, float backlerp, int frame, int oldframe)
        {
            float oldXyzScale, newXyzScale; int vertNum, numVerts;

            ref Md3XyzNormal newXyz = ref surf.xyzs[frame];
            newXyzScale = ModelXMd3.MD3_XYZ_SCALE * (1f - backlerp);

            numVerts = surf.numVerts;

            // just copy the vertexes
            if (backlerp == 0)
                for (vertNum = 0; vertNum < numVerts; vertNum++)
                {
                    var outvert = tri.verts[tri.numVerts];

                    outvert.xyz.x = newXyz.xyz[0] * newXyzScale;
                    outvert.xyz.y = newXyz.xyz[1] * newXyzScale;
                    outvert.xyz.z = newXyz.xyz[2] * newXyzScale;

                    tri.numVerts++;
                }
            // interpolate and copy the vertexes
            else
            {
                ref Md3XyzNormal oldXyz = ref surf.xyzs[oldframe];
                oldXyzScale = ModelXMd3.MD3_XYZ_SCALE * backlerp;

                for (vertNum = 0; vertNum < numVerts; vertNum++)
                {
                    var outvert = tri.verts[tri.numVerts];

                    // interpolate the xyz
                    outvert.xyz.x = oldXyz.xyz[0] * oldXyzScale + newXyz.xyz[0] * newXyzScale;
                    outvert.xyz.y = oldXyz.xyz[1] * oldXyzScale + newXyz.xyz[1] * newXyzScale;
                    outvert.xyz.z = oldXyz.xyz[2] * oldXyzScale + newXyz.xyz[2] * newXyzScale;

                    tri.numVerts++;
                }
            }
        }

        public override IRenderModel InstantiateDynamicModel(RenderEntity ent, ViewDef view, IRenderModel cachedModel)
        {
            int i, j;
            float backlerp;
            int[] triangles;
            float[] texCoords;
            int indexes, numVerts;
            Md3Surface surface;
            int frame, oldframe;
            RenderModelStatic staticModel;

            if (cachedModel != null) cachedModel = null;

            staticModel = new RenderModelStatic();
            staticModel.bounds.Clear();

            // TODO: these need set by an entity
            frame = (int)ent.shaderParms[IRenderWorld.SHADERPARM_MD3_FRAME];         // probably want to keep frames < 1000 or so
            oldframe = (int)ent.shaderParms[IRenderWorld.SHADERPARM_MD3_LASTFRAME];
            backlerp = ent.shaderParms[IRenderWorld.SHADERPARM_MD3_BACKLERP];

            for (i = 0; i < md3.numSurfaces; i++)
            {
                surface = md3.surfaces[i];

                var tri = R_AllocStaticTriSurf();
                R_AllocStaticTriSurfVerts(tri, surface.numVerts);
                R_AllocStaticTriSurfIndexes(tri, surface.numTriangles * 3);
                tri.bounds.Clear();

                ModelSurface surf = new();

                surf.geometry = tri;
                surf.shader = surface.shaders[0].shader;

                LerpMeshVertexes(tri, surface, backlerp, frame, oldframe);

                triangles = surface.tris[0].indexes;
                indexes = surface.numTriangles * 3;
                for (j = 0; j < indexes; j++) tri.indexes[j] = triangles[j];
                tri.numIndexes += indexes;

                texCoords = surface.sts;

                numVerts = surface.numVerts;
                for (j = 0; j < numVerts; j++)
                {
                    ref DrawVert stri = ref tri.verts[j];
                    stri.st[0] = texCoords[j * 2 + 0];
                    stri.st[1] = texCoords[j * 2 + 1];
                }

                R_BoundTriSurf(tri);

                staticModel.AddSurface(surf);
                staticModel.bounds.AddPoint(surf.geometry.bounds[0]);
                staticModel.bounds.AddPoint(surf.geometry.bounds[1]);
            }

            return staticModel;
        }

        public override Bounds Bounds(RenderEntity ent)
        {
            Bounds ret = new();

            ret.Clear();

            if (ent == null || md3 == null)
            {
                // just give it the editor bounds
                ret.AddPoint(new Vector3(-10, -10, -10));
                ret.AddPoint(new Vector3(10, 10, 10));
                return ret;
            }

            var frame = md3.frames[0];

            ret.AddPoint(frame.bounds[0]);
            ret.AddPoint(frame.bounds[1]);

            return ret;
        }
    }
}