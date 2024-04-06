using System.Runtime.InteropServices;

namespace GameX.IW.Zone
{
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct XModelDrawInfo
    {
        public ushort lod;
        public ushort surfId;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct GfxSceneDynModel
    {
        public XModelDrawInfo info;
        public ushort dynEntId;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct BModelDrawInfo
    {
        public ushort surfId;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct GfxSceneDynBrush
    {
        public BModelDrawInfo info;
        public ushort dynEntId;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct GfxStreamingAabbTree
    {
        public ushort firstItem;
        public ushort itemCount;
        public ushort firstChild;
        public ushort childCount;
        public fixed float mins[3];
        public fixed float maxs[3];
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct GfxWorldStreamInfo
    {
        public int aabbTreeCount;
        public GfxStreamingAabbTree* aabbTrees;
        public int leafRefCount;
        public int* leafRefs;
    }

    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct GfxColor //: union
    {
        [FieldOffset(0)] public uint packed;
        [FieldOffset(0)] public fixed char array[4];
    }

    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct PackedUnitVec //: union
    {
        [FieldOffset(0)] public uint packed;
        [FieldOffset(0)] public fixed char array[4];
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct GfxWorldVertex
    {
        public fixed float xyz[3];
        public float binormalSign;
        public GfxColor color;
        public fixed float texCoord[2];
        public fixed float lmapCoord[2];
        public PackedUnitVec normal;
        public PackedUnitVec tangent;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct GfxWorldVertexData
    {
        public GfxWorldVertex* vertices;
        public IDirect3DVertexBuffer9* worldVb;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct GfxLight
    {
        public char type;
        public char canUseShadowMap;
        public fixed char unused[2];
        public fixed float color[3];
        public fixed float dir[3];
        public fixed float origin[3];
        public float radius;
        public float cosHalfFovOuter;
        public float cosHalfFovInner;
        public int exponent;
        public uint spotShadowIndex;
        public GfxLightDef* def;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct GfxReflectionProbe
    {
        public fixed float offset[3];
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct GfxWorldDpvsPlanes
    {
        public int cellCount;
        public cplane_s* planes;
        public ushort* nodes;
        public uint* sceneEntCellBits; //Size = cellCount << 11
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct GfxAabbTree
    {
        public fixed float mins[3];
        public fixed float maxs[3];
        public ushort childCount;
        public ushort surfaceCount;
        public ushort startSurfIndex;
        public ushort smodelIndexCount;
        public ushort* smodelIndexes;
        public int childrenOffset;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct GfxLightGridEntry
    {
        public ushort colorsIndex;
        public char primaryLightIndex;
        public char needsTrace;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct GfxLightGridColors
    {
        public fixed char rgb[56 * 3];
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct GfxStaticModelInst
    {
        public fixed float mins[3];
        public fixed float maxs[3];
        public GfxColor groundLighting;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct srfTriangles_t
    {
        public int vertexLayerData;
        public int firstVertex;
        public ushort vertexCount;
        public ushort triCount;
        public int baseIndex;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct GfxSurface
    {
        public srfTriangles_t tris;
        public Material* material;
        public char lightmapIndex;
        public char reflectionProbeIndex;
        public char primaryLightIndex;
        public bool castsSunShadow;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct GfxCullGroup
    {
        public fixed float mins[3];
        public fixed float maxs[3];
        public int surfaceCount;
        public int startSurfIndex;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct GfxDrawSurfFields
    {
        public long _bf0;
    }

    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct GfxDrawSurf //: union
    {
        [FieldOffset(0)] public GfxDrawSurfFields fields;
        [FieldOffset(0)] public ulong packed;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct GfxWorldDpvsStatic
    {
        public uint smodelCount;
        public uint staticSurfaceCount;
        public uint litSurfsBegin;
        public uint litSurfsEnd;
        public fixed char unknown1[0x20];
        public char* smodelVisData0; public char* smodelVisData1; public char* smodelVisData2;
        public char* surfaceVisData0; public char* surfaceVisData1; public char* surfaceVisData2;
        public ushort* sortedSurfIndex;
        public GfxStaticModelInst* smodelInsts;
        public GfxSurface* surfaces;
        public GfxCullGroup* cullGroups;
        public GfxStaticModelDrawInst* smodelDrawInsts;
        public GfxDrawSurf* surfaceMaterials;
        public uint* surfaceCastsSunShadow;
        public volatile int usageCount;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public unsafe struct GfxPackedPlacement
    {
        public fixed float origin[3];
        public PackedUnitVec axis0; public PackedUnitVec axis1; public PackedUnitVec axis2;
        public float scale;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public unsafe struct GfxStaticModelDrawInst
    {
        public GfxPackedPlacement placement;
        public XModel* model;
        public float cullDist;
        public char reflectionProbeIndex;
        public char primaryLightIndex;
        public ushort lightingHandle;
        public char flags;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public unsafe struct cplane_s
    {
        public fixed float normal[3];
        public float dist;
        public char type;
        public char signbits;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public unsafe struct GfxPortalWritable
    {
        public bool isQueued;
        public bool isAncestor;
        public char recursionDepth;
        public char hullPointCount;
        public float** hullPoints; //:float (* hullPoints)[2]
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public unsafe struct DpvsPlane
    {
        public fixed float coeffs[4];
        public fixed char side[3];
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public unsafe struct GfxPortal
    {
        public GfxPortalWritable writable;
        public DpvsPlane plane;
        public float** vertices; //:float (* vertices)[3]
        public fixed char unknown[2];
        public char vertexCount;
        public fixed float hullAxis[2 * 3];
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public unsafe struct GfxCell
    {
        public fixed float mins[3];
        public fixed float maxs[3];
        public int portalCount;
        public GfxPortal* portals;
        public char reflectionProbeCount;
        public char* reflectionProbes;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public unsafe struct GfxLightmapArray
    {
        public GfxImage* primary;
        public GfxImage* secondary;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public unsafe struct GfxLightGrid
    {
        public bool hasLightRegions;
        public uint sunPrimaryLightIndex;
        public fixed ushort mins[3];
        public fixed ushort maxs[3];
        public uint rowAxis;
        public uint colAxis;
        public ushort* rowDataStart;
        public uint rawRowDataSize;
        public char* rawRowData;
        public uint entryCount;
        public GfxLightGridEntry* entries;
        public uint colorCount;
        public GfxLightGridColors* colors;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public unsafe struct GfxBrushModelWritable
    {
        public fixed float mins[3];
        public fixed float maxs[3];
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public unsafe struct GfxBrushModel
    {
        public GfxBrushModelWritable writable;
        public fixed float bounds[2 * 3];
        public uint surfaceCount;
        public uint startSurfIndex;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public unsafe struct MaterialMemory
    {
        public Material* material;
        public int memory;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public unsafe struct sunflare_t
    {
        public bool hasValidData;
        public Material* spriteMaterial;
        public Material* flareMaterial;
        public float spriteSize;
        public float flareMinSize;
        public float flareMinDot;
        public float flareMaxSize;
        public float flareMaxDot;
        public float flareMaxAlpha;
        public int flareFadeInTime;
        public int flareFadeOutTime;
        public float blindMinDot;
        public float blindMaxDot;
        public float blindMaxDarken;
        public int blindFadeInTime;
        public int blindFadeOutTime;
        public float glareMinDot;
        public float glareMaxDot;
        public float glareMaxLighten;
        public int glareFadeInTime;
        public int glareFadeOutTime;
        public fixed float sunFxPosition[3];
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public unsafe struct GfxShadowGeometry
    {
        public ushort surfaceCount;
        public ushort smodelCount;
        public ushort* sortedSurfIndex;
        public ushort* smodelIndex;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public unsafe struct GfxLightRegionAxis
    {
        public fixed float dir[3];
        public float midPoint;
        public float halfSize;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public unsafe struct GfxLightRegionHull
    {
        public fixed float kdopMidPoint[9];
        public fixed float kdopHalfSize[9];
        public uint axisCount;
        public GfxLightRegionAxis* axis;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public unsafe struct GfxLightRegion
    {
        public uint hullCount;
        public GfxLightRegionHull* hulls;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public unsafe struct GfxWorldDpvsDynamic
    {
        public fixed uint dynEntClientWordCount[2];
        public fixed uint dynEntClientCount[2];
        //public fixed uint* dynEntCellBits[2];
        //public fixed char* dynEntVisData[2*3];
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public unsafe struct SunLightParseParams
    {
        public fixed char name[64];
        public float ambientScale;
        public fixed float ambientColor[3];
        public float diffuseFraction;
        public float sunLight;
        public fixed float sunColor[3];
        public fixed float diffuseColor[3];
        public bool diffuseColorHasBeenSet;
        public fixed float angles[3];
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public unsafe struct GfxWorldVertexLayerData
    {
        public char* data;
        public IDirect3DVertexBuffer9* layerVb;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public unsafe struct GfxWorldDraw
    {
        public uint reflectionProbeCount;
        public GfxImage** reflectionImages;
        public GfxReflectionProbe* reflectionProbes;
        public fixed char reflectionProbeTextures[0x34]; //: GfxTexture //Count = refelctionProbeCount
        public int lightmapCount;
        public GfxLightmapArray* lightmaps;
        public fixed char lightmapPrimaryTextures[0x34]; //: GfxTexture //Count = lightmapCount
        public fixed char lightmapSecondaryTextures[0x34]; //: GfxTexture //Count = lightmapCount
        public GfxImage* skyImage;
        public GfxImage* outdoorImage;
        public uint vertexCount;
        public GfxWorldVertexData vd;
        public uint vertexLayerDataSize;
        public GfxWorldVertexLayerData vld;
        public int indexCount;
        public ushort* indices;
        public IDirect3DIndexBuffer9* indexBuffer;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public unsafe struct unknownGfxWorldStruct2
    {
        public int unknownCount;
        public int* unknownArray;
        public GfxImage* unknownImage;
        public int unknown;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public unsafe struct GfxWorld
    {
        public char* name;
        public char* baseName;
        public int planeCount;
        public int nodeCount;
        public int unknown2;
        public uint unknownCount1;
        public unknownGfxWorldStruct2* unknownStructs1; //Count = unknownCount1;
        public fixed char unknown1[0x18];
        public GfxWorldDpvsPlanes dpvsPlanes; //The following rely on the count in this
        public char* unknown3;
        public GfxAabbTree* aabbTree;
        public GfxCell* cells;
        public GfxWorldDraw worldDraw;
        public GfxLightGrid lightGrid;
        public int modelCount;
        public GfxBrushModel* models;
        public fixed float mins[3];
        public fixed float maxs[3];
        public uint checksum;
        public int materialMemoryCount;
        public MaterialMemory* materialMemory;
        public sunflare_t sun;
        public uint* cellCasterBits0; public uint* cellCasterBits1;
        public GfxSceneDynModel* sceneDynModel;
        public GfxSceneDynBrush* sceneDynBrush;
        public uint* primaryLightEntityShadowVis;
        public uint* primaryLightDynEntShadowVis0; public uint* primaryLightDynEntShadowVis1;
        public char* primaryLightForModelDynEnt;
        public GfxShadowGeometry* shadowGeom;
        public GfxLightRegion* lightRegion;
        public GfxWorldDpvsStatic dpvs;
        public GfxWorldDpvsDynamic dpvsDyn;
        public uint unknownCount2;
        public char* unknown4; //Size = unknownCount2 * 0x38
        public int unknown5;
    }
}