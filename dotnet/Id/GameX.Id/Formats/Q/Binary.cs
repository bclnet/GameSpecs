using GameX.Meta;
using GameX.Platforms;
using OpenStack.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

// https://www.gamers.org/dEngine/quake/spec/quake-spec34/qkspec_3.htm
namespace GameX.Id.Formats.Q
{
    #region Binary_Lump

    public unsafe class Binary_Lump : IHaveMetaInfo, ITexture
    {
        public static Task<object> Factory(BinaryReader r, FileSource f, PakFile s) => Task.FromResult((object)new Binary_Lump(r, f, s));
        public static Binary_Lump Palette;
        public static Binary_Lump Colormap;

        #region Records

        public byte[][] PaletteRecords;
        public byte[][] ColormapRecords;

        public static byte[] ToLightPixel(int light, int pixel) => Palette.PaletteRecords[Colormap.ColormapRecords[(light >> 3) & 0x1F][pixel]];

        byte[] Pixels;
        static (object gl, object vulken, object unity, object unreal) Format = (
            (TextureGLFormat.Rgba, TextureGLPixelFormat.Bgra, TextureGLPixelType.UnsignedInt8888),
            (TextureGLFormat.Rgba, TextureGLPixelFormat.Bgra, TextureGLPixelType.UnsignedInt8888),
            TextureUnityFormat.Unknown,
            TextureUnrealFormat.Unknown);

        #endregion

        // file: PAK0.PAK:gfx/bigbox.lmp
        public Binary_Lump(BinaryReader r, FileSource f, PakFile s)
        {
            switch (Path.GetFileNameWithoutExtension(f.Path))
            {
                case "palette":
                    PaletteRecords = r.ReadFArray(s => s.ReadBytes(3).Concat(new byte[] { 0 }).ToArray(), 256);
                    Palette = this;
                    return;
                case "colormap":
                    ColormapRecords = r.ReadFArray(s => s.ReadBytes(256), 32);
                    Colormap = this;
                    return;
                default:
                    s.Game.Ensure();
                    var palette = Palette?.PaletteRecords ?? throw new NotImplementedException();
                    var width = Width = r.ReadInt32();
                    var height = Height = r.ReadInt32();
                    Pixels = r.ReadBytes(width * height).SelectMany(x => ToLightPixel(32 << 3, x)).ToArray();
                    return;
            }
        }

        public IDictionary<string, object> Data { get; } = null;
        public int Width { get; }
        public int Height { get; }
        public int Depth { get; } = 0;
        public int MipMaps { get; } = 1;
        public TextureFlags Flags { get; } = 0;

        public void Select(int id) { }
        public byte[] Begin(int platform, out object format, out Range[] ranges)
        {
            format = (Platform.Type)platform switch
            {
                Platform.Type.OpenGL => Format.gl,
                Platform.Type.Vulken => Format.vulken,
                Platform.Type.Unity => Format.unity,
                Platform.Type.Unreal => Format.unreal,
                _ => throw new ArgumentOutOfRangeException(nameof(platform), $"{platform}"),
            };
            ranges = null;
            return Pixels;
        }
        public void End() { }

        // IHaveMetaInfo
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
            => new List<MetaInfo> {
                new MetaInfo(null, new MetaContent { Type = "Texture", Name = Path.GetFileName(file.Path), Value = this }),
                new MetaInfo("Texture", items: new List<MetaInfo> {
                    new MetaInfo($"Width: {Width}"),
                    new MetaInfo($"Height: {Height}"),
                })
            };
    }

    #endregion

    #region Binary_Level

    public unsafe class Binary_Level : IHaveMetaInfo
    {
        public static Task<object> Factory(BinaryReader r, FileSource f, PakFile s) => Task.FromResult((object)new Binary_Level(r));

        #region Records

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct D_Entry
        {
            public int Offset;                // Offset to entry, in bytes, from start of file
            public int Size;                  // Size of entry in file, in bytes
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct BoundBox
        {
            public Vector3 Min;                // minimum values of X,Y,Z
            public Vector3 Max;                // maximum values of X,Y,Z
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct D_Header
        {
            public static (string, int) Struct = ("<I2i", sizeof(D_Header));
            public int Version;                 // Model version, must be 0x17 (23).
            public D_Entry Entities;            // List of Entities.
            public D_Entry Planes;              // Map Planes.
            public readonly int NumPlanes => Planes.Size / sizeof(D_Plane);
            public D_Entry Miptex;              // Wall Textures.
            public D_Entry Vertices;            // Map Vertices.
            public readonly int NumVertices => Vertices.Size / sizeof(Vector3);
            public D_Entry VisiList;            // Leaves Visibility lists.
            public D_Entry Nodes;               // BSP Nodes.
            public readonly int NumNodes => Nodes.Size / sizeof(D_Node);
            public D_Entry TexInfo;             // Texture Info for faces.
            public readonly int NumTexInfo => TexInfo.Size / sizeof(D_TexInfo);
            public D_Entry Faces;               // Faces of each surface.
            public readonly int NumFaces => Faces.Size / sizeof(D_Face);
            public D_Entry LightMaps;           // Wall Light Maps.
            public D_Entry ClipNodes;           // clip nodes, for Models.
            public readonly int NumClips => ClipNodes.Size / sizeof(D_ClipNode);
            public D_Entry Leaves;              // BSP Leaves.
            public readonly int NumLeaves => Leaves.Size / sizeof(D_Leaf);
            public D_Entry LFace;               // List of Faces.
            public D_Entry Edges;               // Edges of faces.
            public readonly int NumEdges => Edges.Size / sizeof(Vector2<ushort>);
            public D_Entry Ledges;              // List of Edges.
            public D_Entry Models;              // List of Models.
            public readonly int NumModels => Models.Size / sizeof(D_Model);
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct D_Model
        {
            public BoundBox Bound;              // The bounding box of the Model
            public Vector3 Origin;              // origin of model, usually (0,0,0)
            public int NodeId0;                 // index of first BSP node
            public int NodeId1;                 // index of the first Clip node
            public int NodeId2;                 // index of the second Clip node
            public int NodeId3;                 // usually zero
            public int NumLeafs;                // number of BSP leaves
            public int FaceId;                  // index of Faces
            public int FaceNum;                 // number of Faces
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct D_TexInfo
        {
            public Vector3 VectorS;             // S vector, horizontal in texture space)
            public float DistS;                 // horizontal offset in texture space
            public Vector3 VectorT;             // T vector, vertical in texture space
            public float DistT;                 // vertical offset in texture space
            public uint TextureId;              // Index of Mip Texture must be in [0,numtex[
            public uint Animated;               // 0 for ordinary textures, 1 for water 
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct D_Face
        {
            public ushort PlaneId;              // The plane in which the face lies: must be in [0,numplanes[ 
            public ushort Side;                 // 0 if in front of the plane, 1 if behind the plane
            public int LedgeId;                 // first edge in the List of edges: must be in [0,numledges[
            public ushort LedgeNum;             // number of edges in the List of edges
            public ushort TexinfoId;            // index of the Texture info the face is part of: must be in [0,numtexinfos[ 
            public byte TypeLight;              // type of lighting, for the face
            public byte BaseLight;              // from 0xFF (dark) to 0 (bright)
            public fixed byte Light[2];         // two additional light models  
            public int LightMap;                // Pointer inside the general light map, or -1
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct D_Node
        {
            public long PlaneId;                // The plane that splits the node: must be in [0,numplanes[
            public ushort Front;                // If bit15==0, index of Front child node: If bit15==1, ~front = index of child leaf
            public ushort Back;                 // If bit15==0, id of Back child node: If bit15==1, ~back =  id of child leaf
            public Vector2<short> Box;          // Bounding box of node and all childs
            public ushort FaceId;               // Index of first Polygons in the node
            public ushort FaceNum;              // Number of faces in the node
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct D_Leaf
        {
            public int Type;                    // Special type of leaf
            public int VisList;                 // Beginning of visibility lists: must be -1 or in [0,numvislist[
            Vector2<short> Bound;               // Bounding box of the leaf
            public ushort LFaceId;              // First item of the list of faces: must be in [0,numlfaces[
            public ushort LFaceNum;             // Number of faces in the leaf  
            public byte SndWater;               // level of the four ambient sounds:
            public byte SndSky;                 //   0    is no sound
            public byte SndSlime;               //   0xFF is maximum volume
            public byte SndLava;                //
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct D_Plane
        {
            public Vector3 Normal;              // Vector orthogonal to plane (Nx,Ny,Nz): with Nx2+Ny2+Nz2 = 1
            public float Dist;                  // Offset to plane, along the normal vector: Distance from (0,0,0) to the plane
            public int Type;                    // Type of plane, depending on normal vector.
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct D_ClipNode
        {
            public uint PlaneNum;               // The plane which splits the node
            public short Front;                 // If positive, id of Front child node: If -2, the Front part is inside the model: If -1, the Front part is outside the model
            public short Back;                  // If positive, id of Back child node: If -2, the Back part is inside the model: If -1, the Back part is outside the model
        }

        #endregion

        // file: xxxx.bsp
        public Binary_Level(BinaryReader r)
        {
        }

        // IHaveMetaInfo
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
            => new List<MetaInfo> {
                new MetaInfo(null, new MetaContent { Type = "Text", Name = Path.GetFileName(file.Path), Value = "BSP File" }),
                new MetaInfo("Level", items: new List<MetaInfo> {
                    //new MetaInfo($"Records: {Records.Length}"),
                })
            };
    }

    #endregion

    #region Binary_Model

    public unsafe class Binary_Model : IHaveMetaInfo
    {
        public static Task<object> Factory(BinaryReader r, FileSource f, PakFile s) => Task.FromResult((object)new Binary_Model(r));

        #region Records

        #endregion

        // file: xxxx.mdl
        public Binary_Model(BinaryReader r)
        {
        }

        // IHaveMetaInfo
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
            => new List<MetaInfo> {
                new MetaInfo(null, new MetaContent { Type = "Text", Name = Path.GetFileName(file.Path), Value = "Model File" }),
                new MetaInfo("Model", items: new List<MetaInfo> {
                    //new MetaInfo($"Records: {Records.Length}"),
                })
            };
    }

    #endregion

    #region Binary_Sprite

    public unsafe class Binary_Sprite : IHaveMetaInfo
    {
        public static Task<object> Factory(BinaryReader r, FileSource f, PakFile s) => Task.FromResult((object)new Binary_Sprite(r));

        #region Records

        const uint SPR_MAGIC = 0x1122; // IDSP

        enum SpriteType
        {
            ParallelUpright = 0, // vp parallel upright
            FaceUpright = 1, // facing upright
            Parallel = 2, // vp parallel
            Oriented = 3, // oriented
            ParallelOriented = 4, // vp parallel oriented
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct SPR_Header
        {
            public static (string, int) Struct = ("<I2i", sizeof(SPR_Header));
            public uint Magic;      // "IDSP"
            public int Version;     // Version = 1
            public int Type;        // See below
            public float Radius;           // Bounding Radius
            public int MaxWidth;           // Width of the largest frame
            public int MaxHeight;          // Height of the largest frame
            public int NumFrames;          // Number of frames
            public float BeamLength;       // 
            public int SynchType;          // 0=synchron 1=random
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct SPR_Picture
        {
            public static (string, int) Struct = ("<I2i", sizeof(SPR_Picture));
            public int OfsX;        // horizontal offset, in 3D space
            public int OfsY;        // vertical offset, in 3D space
            public int Width;       // width of the picture
            public int Height;      // height of the picture
        }

        #endregion
        // R_GetSpriteFrame

        // file: xxxx.spr
        public Binary_Sprite(BinaryReader r)
        {
            var header = r.ReadS<SPR_Header>();
        }

        // IHaveMetaInfo
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
            => new List<MetaInfo> {
                new MetaInfo(null, new MetaContent { Type = "Text", Name = Path.GetFileName(file.Path), Value = "Sprite File" }),
                new MetaInfo("Sprite", items: new List<MetaInfo> {
                    //new MetaInfo($"Records: {Records.Length}"),
                })
            };
    }

    #endregion
}
