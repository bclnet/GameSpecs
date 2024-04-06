using System.Runtime.InteropServices;

namespace GameX.IW.Zone
{
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct Bounds
    {
        public vec3 midPoint;
        public vec3 halfSize;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct TriggerModel
    {
        public int contents;
        public ushort hullCount;
        public ushort firstHull;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct TriggerHull
    {
        public Bounds bounds;
        public int contents;
        public ushort slabCount;
        public ushort firstSlab;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct TriggerSlab
    {
        public vec3 dir;
        public float midPoint;
        public float halfSize;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct MapTriggers
    {
        public int modelCount;
        public TriggerModel* models; // sizeof 8
        public int hullCount;
        public TriggerHull* hulls; // sizeof 32
        public int slabCount;
        public TriggerSlab* slabs; // sizeof 20
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct Stage
    {
        public char* stageName;
        public fixed float offset[3];
        public int flags;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct MapEnts
    {
        public char* name;
        public char* entityString;
        public int numEntityChars;
        public MapTriggers trigger;
        public Stage* stageNames;
        public char stageCount;
        public fixed char pad[3];
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public unsafe partial struct BrushWrapper
    {
        public fixed float mins[3];
        public fixed float maxs[3];
        public cBrush brush;
        public int totalEdgeCount;
        public cPlane* planes;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe partial struct PhysGeomInfo
    {
        public BrushWrapper* brush;
        public int type;
        public fixed float orientation[3 * 3];
        public fixed float offset[3];
        public fixed float halfLengths[3];
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe partial struct PhysGeomList
    {
        public char* name;
        public uint count;
        public PhysGeomInfo* geoms;
        public fixed char unknown[0x18];
        public PhysMass mass;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct ComPrimaryLight
    {
        public char type;
        public char canUseShadowMap;
        public char exponent;
        public char unused;
        public fixed float color[3];
        public fixed float dir[3];
        public fixed float origin[3];
        public float radius;
        public float cosHalfFovOuter;
        public float cosHalfFovInner;
        public float cosHalfFovExpanded;
        public float rotationLimit;
        public float translationLimit;
        public char* name;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct ComWorld
    {
        public char* name;
        public int isInUse;
        public int lightCount;
        public ComPrimaryLight* lights;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct GameMap_Data
    {
        public void* unk1;
        public int unkCount1;
        public int unkCount2;
        public void* unk2;
        public fixed char pad[112];
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct GameMap_SP
    {
        public char* name;
        public fixed char pad[48];
        public GameMap_Data* data;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct GameMap_MP
    {
        public char* name;
        public GameMap_Data* data;
    }
}