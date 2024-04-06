using System.Runtime.InteropServices;

namespace GameX.IW.Zone
{
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct vec2
    {
        public float x, y;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct vec3
    {
        public float x, y, z;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct vec4
    {
        public float x, y, z, w;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe partial struct PhysPreset
    {
        public char* name;
        public int type;
        public float mass;
        public float bounce;
        public float friction;
        public float bulletForceScale;
        public float explosiveForceScale;
        public char* sndAliasPrefix;
        public float piecesSpreadFraction;
        public float piecesUpwardVelocity;
        public bool tempDefaultToCylinder;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct VertexDecl
    {
        public char* name;
        public int unknown;
        public fixed char pad[28];
        /*IDirect3DVertexDeclaration9**/
        public void* declarations00;
        public void* declarations01;
        public void* declarations02;
        public void* declarations03;
        public void* declarations04;
        public void* declarations05;
        public void* declarations06;
        public void* declarations07;
        public void* declarations08;
        public void* declarations09;
        public void* declarations10;
        public void* declarations11;
        public void* declarations12;
        public void* declarations13;
        public void* declarations14;
        public void* declarations15;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct PixelShader
    {
        public const int COD4_SizeOf = 115;
        public char* name;
        /*IDirect3DPixelShader9*/
        public void* shader;
        public uint* bytecode;
        public int codeLen;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct VertexShader
    {
        public char* name;
        public void* /*IDirect3DVertexShader9**/ shader;
        public uint* bytecode;
        public int codeLen;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct GfxLightImage
    {
        public GfxImage image;
        public char samplerState;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct GfxLightDef
    {
        public char* name;
        public GfxLightImage attenuation;
        public int lmapLookupStart;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct FontEntry
    {
        public ushort character;
        public byte padLeft;
        public byte padTop;
        public byte padRight;
        public byte width;
        public byte height;
        public byte const0;
        public float uvLeft;
        public float uvTop;
        public float uvRight;
        public float uvBottom;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct Font
    {
        public char* name;
        public int size;
        public int entries;
        public Material* image;
        public Material* glowImage;
        public FontEntry* characters;
    }

    //// we will leave menus out of this as they are compicated as hell

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct Localize
    {
        public char* localizedString;
        public char* name;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct FxImpactEntry
    {
        public FxEffectDef* nonflesh_00; public FxEffectDef* nonflesh_01; public FxEffectDef* nonflesh_02; public FxEffectDef* nonflesh_03; public FxEffectDef* nonflesh_04;
        public FxEffectDef* nonflesh_05; public FxEffectDef* nonflesh_06; public FxEffectDef* nonflesh_07; public FxEffectDef* nonflesh_08; public FxEffectDef* nonflesh_09;
        public FxEffectDef* nonflesh_10; public FxEffectDef* nonflesh_11; public FxEffectDef* nonflesh_12; public FxEffectDef* nonflesh_13; public FxEffectDef* nonflesh_14;
        public FxEffectDef* nonflesh_15; public FxEffectDef* nonflesh_16; public FxEffectDef* nonflesh_17; public FxEffectDef* nonflesh_18; public FxEffectDef* nonflesh_19;
        public FxEffectDef* nonflesh_20; public FxEffectDef* nonflesh_21; public FxEffectDef* nonflesh_22; public FxEffectDef* nonflesh_23; public FxEffectDef* nonflesh_24;
        public FxEffectDef* nonflesh_25; public FxEffectDef* nonflesh_26; public FxEffectDef* nonflesh_27; public FxEffectDef* nonflesh_28; public FxEffectDef* nonflesh_29;
        public FxEffectDef* nonflesh_30;
        public FxEffectDef* flesh0; public FxEffectDef* flesh1; public FxEffectDef* flesh2; public FxEffectDef* flesh3;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct FxImpactTable
    {
        public char* name;
        public FxImpactEntry* table;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct LbColumnDef
    {
        public char* title;
        public int id;
        public int propertyId;
        public int unk1;
        public char* statName;
        public int unk2;
        public int unk3;
        public int unk4;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct LeaderboardDef
    {
        public char* name;
        public int id;
        public int columnCount;
        public int xpColId;
        public int prestigeColId;
        public LbColumnDef* columns;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct Tracer
    {
        public char* name;
        public Material* material;
        public uint drawInterval;
        public float speed;
        public float beamLength;
        public float beamWidth;
        public float screwRadius;
        public float screwDist;
        public fixed float colors[5 * 4];
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct Rawfile
    {
        public char* name;
        public int sizeCompressed;
        public int sizeUnCompressed;
        public char* compressedData;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct StringTable
    {
        public char* name;
        public int columns;
        public int rows;
        public char** data;
    }
}