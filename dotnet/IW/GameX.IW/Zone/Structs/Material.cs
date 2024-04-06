using System.Runtime.InteropServices;

namespace GameX.IW.Zone
{
    public enum IWI_COMPRESSION
    {
        IWI_INVALID = 0x0,
        IWI_ARGB = 0x1,
        IWI_RGB8 = 0x2,
        IWI_DXT1 = 0xB,
        IWI_DXT3 = 0xC,
        IWI_DXT5 = 0xD,
    }

    public enum MaterialMapHashes : uint
    {
        HASH_COLORMAP = 0xa0ab1041,
        HASH_DETAILMAP = 0xeb529b4d,
        HASH_SPECULARMAP = 0x34ecccb3,
        HASH_NORMALMAP = 0x59d30d0f
    }

    public enum MaterialSemantic
    {
        SEMANTIC_2D = 0x0,
        SEMANTIC_FUNCTION = 0x1,
        SEMANTIC_COLOR_MAP = 0x2,
        SEMANTIC_NORMAL_MAP = 0x5,
        SEMANTIC_SPECULAR_MAP = 0x8,
        SEMANTIC_WATER_MAP = 0xB
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct _IWI
    {
        public fixed char magic[3]; //IWi
        public char version; // 8
        public int flags;
        public short format; // see above
        public short xsize;
        public short ysize;
        public short depth;
        public int mipAddr4;
        public int mipAddr3;
        public int mipAddr2;
        public int mipAddr1;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct GfxImageLoadDef // actually a IDirect3DTexture* but this is easier
    {
        public char mipLevels;
        public char flags;
        public fixed short dimensions[3];
        public int format; // usually the compression Magic
        public int dataSize; // set to zero to load from IWD
        public char* data;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct GfxImage
    {
        public GfxImageLoadDef* /*Direct3DTexture9**/ texture;
        public char mapType; // 5 is cube, 4 is 3d, 3 is 2d
        public char semantic;
        public char category;
        public char flags;
        public int cardMemory;
        public int dataLen1;
        public int dataLen2;
        public short height;
        public short width;
        public short depth;
        public bool loaded;
        public char pad;
        public char* name;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct Water
    {
        public float floatTime;
        public float* H0X;     // Count = M * N
        public float* H0Y;     // Count = M * N
        public float* wTerm;       // Count = M * N
        public int M;
        public int N;
        public float Lx;
        public float Lz;
        public float gravity;
        public float windvel;
        public fixed float winddir[2];
        public float amplitude;
        public fixed float codeConstant[4];
        public GfxImage* image;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct MaterialTextureDefInfo //: union 
    {
        public GfxImage* image;    // MaterialTextureDef->semantic != SEMANTIC_WATER_MAP
        public Water* water;     // MaterialTextureDef->semantic == SEMANTIC_WATER_MAP
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct MaterialTextureDef
    {
        public uint nameHash;
        public char nameStart;
        public char nameEnd;
        public char sampleState;
        public char semantic;
        public MaterialTextureDefInfo info;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct ShaderArgumentDef
    {
        public short type;
        public short dest;
        public short paramID;
        public short more;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct MaterialPass
    {
        public VertexDecl* vertexDecl;
        public VertexShader* vertexShader;
        public PixelShader* pixelShader;
        public char argCount1;
        public char argCount2;
        public char argCount3;
        public char unk;
        public ShaderArgumentDef* argumentDef;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct MaterialTechnique
    {
        public char* name;
        public short pad2;
        public short numPasses;
        public MaterialPass passes0;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct MaterialTechniqueSet
    {
        public char* name;
        public fixed char pad[4];
        public MaterialTechniqueSet* remappedTechniques;
        public MaterialTechnique* techniques00; public MaterialTechnique* techniques01; public MaterialTechnique* techniques02; public MaterialTechnique* techniques03; public MaterialTechnique* techniques04;
        public MaterialTechnique* techniques05; public MaterialTechnique* techniques06; public MaterialTechnique* techniques07; public MaterialTechnique* techniques08; public MaterialTechnique* techniques09;
        public MaterialTechnique* techniques10; public MaterialTechnique* techniques11; public MaterialTechnique* techniques12; public MaterialTechnique* techniques13; public MaterialTechnique* techniques14;
        public MaterialTechnique* techniques15; public MaterialTechnique* techniques16; public MaterialTechnique* techniques17; public MaterialTechnique* techniques18; public MaterialTechnique* techniques19;
        public MaterialTechnique* techniques20; public MaterialTechnique* techniques21; public MaterialTechnique* techniques22; public MaterialTechnique* techniques23; public MaterialTechnique* techniques24;
        public MaterialTechnique* techniques25; public MaterialTechnique* techniques26; public MaterialTechnique* techniques27; public MaterialTechnique* techniques28; public MaterialTechnique* techniques29;
        public MaterialTechnique* techniques30; public MaterialTechnique* techniques31; public MaterialTechnique* techniques32; public MaterialTechnique* techniques33; public MaterialTechnique* techniques34;
        public MaterialTechnique* techniques35; public MaterialTechnique* techniques36; public MaterialTechnique* techniques37; public MaterialTechnique* techniques38; public MaterialTechnique* techniques39;
        public MaterialTechnique* techniques40; public MaterialTechnique* techniques41; public MaterialTechnique* techniques42; public MaterialTechnique* techniques43; public MaterialTechnique* techniques44;
        public MaterialTechnique* techniques45; public MaterialTechnique* techniques46; public MaterialTechnique* techniques47;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct Material_old
    {
        public char* name;
        public char gameFlags;
        public char sortKey;
        public byte animationX; // amount of animation frames in X
        public byte animationY; // amount of animation frames in Y
        public void* drawSurf;
        public uint rendererIndex; // only for 3D models
        public fixed char unknown9[8];
        public uint unknown2; // 0xFFFFFFFF
        public uint unknown3; // 0xFFFFFF00
        public fixed char unknown4[40]; // 0xFF
        public char numMaps; // 0x01, possibly 'map count' (zone code confirms)
        public char unknown5; // 0x00
        public char stateMapCount; // 0x01, maybe map count actually
        public char unknown6; // 0x03
        public uint unknown7; // 0x04
        public MaterialTechniqueSet* techniqueSet; // '2d' techset; +80
        public MaterialTextureDef* maps; // map references
        public uint unknown8;
        public void* stateMap; // might be NULL, need to test
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct MaterialConstantDef
    {
        public int nameHash;
        public fixed char name[12];
        public vec4 literal;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe partial struct Material
    {
        public char* name;
        public char gameFlags;
        public char sortKey;
        public char textureAtlasRowCount;
        public char textureAtlasColumnCount;
        public fixed char drawSurf[12];
        public int surfaceTypeBits;
        public fixed char stateBitsEntry[48];
        public char textureCount;
        public char constantCount;
        public char stateBitsCount;
        public char stateFlags;
        public char cameraRegion;
        public MaterialTechniqueSet* techniqueSet;
        public MaterialTextureDef* textureTable;
        public MaterialConstantDef* constantTable;
        public void* stateBitTable;
    }
}