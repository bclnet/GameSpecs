using System.Runtime.InteropServices;

namespace GameX.IW.Zone
{
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct PhysMass
    {
        public fixed float centerOfMass[3];
        public fixed float momentsOfInertia[3];
        public fixed float productsOfInertia[3];
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct cPlane
    {
        public vec3 a;
        public float dist;
        public int type;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct cStaticModel
    {
        public XModel* xmodel;
        public fixed float origin[3];
        public fixed float invScaledAxis[3 * 3];
        public fixed float absmin[3];
        public fixed float absmax[3];
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct dMaterial
    {
        public char* name;
        public int unk;
        public int unk2;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct cNode
    {
        public cPlane* plane;
        public fixed short children[2];
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe partial struct cBrushSide
    {
        public cPlane* side;
        public short texInfo, dispInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct cBrush
    {
        public int count;
        public cBrushSide* brushSide;
        public char* brushEdge;
        public fixed char pad[24];
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct cLeaf
    {
        public ushort firstCollAabbIndex;
        public ushort collAabbCount;
        public int brushContents;
        public int terrainContents;
        public fixed float mins[3];
        public fixed float maxs[3];
        public int leafBrushNode;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct cLeafBrushNodeLeaf
    {
        public ushort* brushes;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct cLeafBrushNodeChildren
    {
        public fixed ushort childOffset[6];
    }

    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct cLeafBrushNodeData //: union 
    {
        [FieldOffset(0)] public cLeafBrushNodeLeaf leaf;
        [FieldOffset(0)] public cLeafBrushNodeChildren children;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct cLeafBrushNode
    {
        public char axis;
        public short leafBrushCount;
        public int contents;
        public cLeafBrushNodeData data;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct cModel
    {
        public fixed float mins[3];
        public fixed float maxs[3];
        public float radius;
        public cLeaf leaf;
    }

    public enum DynEntityType
    {
        DYNENT_TYPE_INVALID = 0x0,
        DYNENT_TYPE_CLUTTER = 0x1,
        DYNENT_TYPE_DESTRUCT = 0x2,
        DYNENT_TYPE_COUNT = 0x3,
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct GfxPlacement
    {
        public fixed float quat[4];
        public fixed float origin[3];
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct DynEntityDef
    {
        public DynEntityType type;
        public GfxPlacement pose;
        public XModel* xModel;
        public ushort brushModel;
        public ushort physicsBrushModel;
        public FxEffectDef* destroyFx;
        public PhysPreset* physPreset;
        public int health;
        public PhysMass mass;
        public int contents;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct DynEntityPose
    {
        public GfxPlacement pose;
        public float radius;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct DynEntityClient
    {
        public int physObjId;
        public ushort flags;
        public ushort lightingHandle;
        public int health;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct DynEntityColl
    {
        public ushort sector;
        public ushort nextEntInSector;
        public fixed float linkMins[2];
        public fixed float linkMaxs[2];
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct CollisionBorder
    {
        public fixed float distEq[3];
        public float zBase;
        public float zSlope;
        public float start;
        public float length;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct CollisionPartition
    {
        public char triCount;
        public char borderCount;
        public int firstTri;
        public CollisionBorder* borders;
    }

    [StructLayout(LayoutKind.Explicit)] //: union
    public unsafe struct CollisionAabbTreeIndex
    {
        [FieldOffset(0)] public int firstChildIndex;
        [FieldOffset(0)] public int partitionIndex;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct CollisionAabbTree
    {
        public vec3 origin;
        public ushort materialIndex;
        public ushort childCount;
        public vec3 halfSize;
        public CollisionAabbTreeIndex u;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct ClipMap
    {
        public char* name;
        public int unknown1; // +8
        public int numCPlanes; // +8
        public cPlane* cPlanes; // sizeof 20, +12
        public int numStaticModels; // +16
        public cStaticModel* staticModelList; // sizeof 76, +20
        public int numMaterials; // +24
        public dMaterial* materials; // sizeof 12 with a string (possibly name?), +28
        public int numCBrushSides; // +32
        public cBrushSide* cBrushSides; // sizeof 8, +36
        public int numCBrushEdges; // +40
        public char* cBrushEdges; // +44
        public int numCNodes; // +48
        public cNode* cNodes; // sizeof 8, +52
        public int numCLeaf; // +56
        public cLeaf* cLeaf; // +60
        public int numCLeafBrushNodes; // +64
        public cLeafBrushNode* cLeafBrushNodes; // +68
        public int numLeafBrushes; // +72
        public short* leafBrushes; // +76
        public int numLeafSurfaces; // +80
        public int* leafSurfaces; // +84
        public int numVerts; // +88
        public vec3* verts; // +92
        public int numTriIndices; // +96
        public short* triIndices; // +100
        public char* triEdgeIsWalkable; // +104
        public int numCollisionBorders; // +108
        public CollisionBorder* collisionBorders;// sizeof 28, +112
        public int numCollisionPartitions; // +116
        public CollisionPartition* collisionPartitions; // sizeof 12, +120
        public int numCollisionAABBTrees; // +124
        public CollisionAabbTree* collisionAABBTrees;// sizeof 32, +128
        public int numCModels; // +132
        public cModel[] cModels; // sizeof 68, +136
        public short numBrushes; // +140
        public short pad2; // +142
        public cBrush* brushes; // sizeof 36, +144
        public Bounds* brushBounds; // same count as cBrushes, +148
        public int* brushContents; // same count as cBrushes, +152
        public MapEnts* mapEnts; // +156
        public int unkCount4; // +160
        public void* unknown4; // +164
        public fixed ushort dynEntCount[2];
        public DynEntityDef* dynEntDefList0; public DynEntityDef* dynEntDefList1;
        public DynEntityPose* dynEntPoseList0; public DynEntityPose* dynEntPoseList1;
        public DynEntityClient* dynEntClientList0; public DynEntityClient* dynEntClientList1;
        public DynEntityColl* dynEntCollList0; public DynEntityColl* dynEntCollList1;
        public uint checksum;
        public fixed char unknown5[0x30];
    } // +256
}