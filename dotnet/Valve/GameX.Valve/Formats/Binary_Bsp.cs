using GameX.Meta;
using OpenStack.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace GameX.Formats
{
    // https://hlbsp.sourceforge.net/index.php?content=bspdef
    // https://github.com/bernhardmgruber/hlbsp/tree/master/src
    public unsafe class Binary_Bsp : IHaveMetaInfo
    {
        public static Task<object> Factory(BinaryReader r, FileSource f, PakFile s) => Task.FromResult((object)new Binary_Bsp(r, f));

        // Headers
        #region Headers

        [StructLayout(LayoutKind.Sequential)]
        struct BSP_Header
        {
            public static (string, int) Struct = ("<31i", sizeof(BSP_Header));
            public int Version;
            public BSP_Lump Entities;
            public BSP_Lump Planes;
            public BSP_Lump Textures;
            public BSP_Lump Vertices;
            public BSP_Lump Visibility;
            public BSP_Lump Nodes;
            public BSP_Lump TexInfo;
            public BSP_Lump Faces;
            public BSP_Lump Lighting;
            public BSP_Lump ClipNodes;
            public BSP_Lump Leaves;
            public BSP_Lump MarkSurfaces;
            public BSP_Lump Edges;
            public BSP_Lump SurfEdges;
            public BSP_Lump Models;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct BSP_Lump
        {
            public static (string, int) Struct = ("<2i", sizeof(BSP_Lump));
            public int Offset;
            public int Length;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct SPR_Frame
        {
            public static (string, int) Struct = ("<5i", sizeof(SPR_Frame));
            public int Group;
            public int OriginX;
            public int OriginY;
            public int Width;
            public int Height;
        }

        const int MAX_MAP_HULLS = 4;

        const int MAX_MAP_MODELS = 400;
        const int MAX_MAP_BRUSHES = 4096;
        const int MAX_MAP_ENTITIES = 1024;
        const int MAX_MAP_ENTSTRING = (128 * 1024);

        const int MAX_MAP_PLANES = 32767;
        const int MAX_MAP_NODES = 32767;
        const int MAX_MAP_CLIPNODES = 32767;
        const int MAX_MAP_LEAFS = 8192;
        const int MAX_MAP_VERTS = 65535;
        const int MAX_MAP_FACES = 65535;
        const int MAX_MAP_MARKSURFACES = 65535;
        const int MAX_MAP_TEXINFO = 8192;
        const int MAX_MAP_EDGES = 256000;
        const int MAX_MAP_SURFEDGES = 512000;
        const int MAX_MAP_TEXTURES = 512;
        const int MAX_MAP_MIPTEX = 0x200000;
        const int MAX_MAP_LIGHTING = 0x200000;
        const int MAX_MAP_VISIBILITY = 0x200000;

        const int MAX_MAP_PORTALS = 65536;

        #endregion

        public Binary_Bsp(BinaryReader r, FileSource f)
        {
            // read file
            var header = r.ReadS<BSP_Header>();
            if (header.Version != 30) throw new FormatException("BAD VERSION");
        }

        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag) => new List<MetaInfo> {
            //new MetaInfo(null, new MetaContent { Type = "Texture", Name = Path.GetFileName(file.Path), Value = this }),
            new MetaInfo("Bsp", items: new List<MetaInfo> {
                //new MetaInfo($"Width: {Width}"),
                //new MetaInfo($"Height: {Height}"),
                //new MetaInfo($"Mipmaps: {MipMaps}"),
            }),
        };
    }
}
