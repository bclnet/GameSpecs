using System.Runtime.InteropServices;

namespace GameX.IW.Zone
{
    public static class FX
    {
        /* FxEffectDef::flags */
        public const uint FX_ELEM_LOOPING = 0x1;
        public const uint FX_ELEM_USE_RAND_COLOR = 0x2;
        public const uint FX_ELEM_USE_RAND_ALPHA = 0x4;
        public const uint FX_ELEM_USE_RAND_SIZE0 = 0x8;
        public const uint FX_ELEM_USE_RAND_SIZE1 = 0x10;
        public const uint FX_ELEM_USE_RAND_SCALE = 0x20;
        public const uint FX_ELEM_USE_RAND_ROT_DELTA = 0x40;
        public const uint FX_ELEM_MOD_COLOR_BY_ALPHA = 0x80;
        public const uint FX_ELEM_USE_RAND_VEL0 = 0x100;
        public const uint FX_ELEM_USE_RAND_VEL1 = 0x200;
        public const uint FX_ELEM_USE_BACK_COMPAT_VEL = 0x400;
        public const uint FX_ELEM_ABS_VEL0 = 0x800;
        public const uint FX_ELEM_ABS_VEL1 = 0x1000;
        public const uint FX_ELEM_PLAY_ON_TOUCH = 0x2000;
        public const uint FX_ELEM_PLAY_ON_DEATH = 0x4000;
        public const uint FX_ELEM_PLAY_ON_RUN = 0x8000;
        public const uint FX_ELEM_BOUNDING_SPHERE = 0x10000;
        public const uint FX_ELEM_USE_ITEM_CLIP = 0x20000;
        public const uint FX_ELEM_DISABLED = 0x80000000;
        public const uint FX_ELEM_DECAL_FADE_IN = 0x40000;

        /* FxElemDef::flags */
        public const uint FX_ELEM_SPAWN_RELATIVE_TO_EFFECT = 0x2;
        public const uint FX_ELEM_SPAWN_FRUSTUM_CULL = 0x4;
        public const uint FX_ELEM_RUNNER_USES_RAND_ROT = 0x8;
        public const uint FX_ELEM_SPAWN_OFFSET_NONE = 0x0;
        public const uint FX_ELEM_SPAWN_OFFSET_SPHERE = 0x10;
        public const uint FX_ELEM_SPAWN_OFFSET_CYLINDER = 0x20;
        public const uint FX_ELEM_SPAWN_OFFSET_MASK = 0x30;
        public const uint FX_ELEM_RUN_RELATIVE_TO_WORLD = 0x0;
        public const uint FX_ELEM_RUN_RELATIVE_TO_SPAWN = 0x40;
        public const uint FX_ELEM_RUN_RELATIVE_TO_EFFECT = 0x80;
        public const uint FX_ELEM_RUN_RELATIVE_TO_OFFSET = 0xC0;
        public const uint FX_ELEM_RUN_MASK = 0xC0;
        public const uint FX_ELEM_USE_COLLISION = 0x100;
        public const uint FX_ELEM_DIE_ON_TOUCH = 0x200;
        public const uint FX_ELEM_DRAW_PAST_FOG = 0x400;
        public const uint FX_ELEM_DRAW_WITH_VIEWMODEL = 0x800;
        public const uint FX_ELEM_BLOCK_SIGHT = 0x1000;
        public const uint FX_ELEM_HAS_VELOCITY_GRAPH_LOCAL = 0x1000000;
        public const uint FX_ELEM_HAS_VELOCITY_GRAPH_WORLD = 0x2000000;
        public const uint FX_ELEM_HAS_GRAVITY = 0x4000000;
        public const uint FX_ELEM_USE_MODEL_PHYSICS = 0x8000000;
        public const uint FX_ELEM_NONUNIFORM_SCALE = 0x10000000;
        public const uint FX_ELEM_CLOUD_SHAPE_CUBE = 0x0;
        public const uint FX_ELEM_CLOUD_SHAPE_SPHERE_LARGE = 0x20000000;
        public const uint FX_ELEM_CLOUD_SHAPE_SPHERE_MEDIUM = 0x40000000;
        public const uint FX_ELEM_CLOUD_SHAPE_SPHERE_SMALL = 0x60000000;
        public const uint FX_ELEM_CLOUD_MASK = 0x60000000;
        public const uint FX_ELEM_DISABLE_FOUNTAIN_COLLISION = 0x80000000;
        public const uint FX_ELEM_DRAW_IN_THERMAL_ONLY = 0x2000;
        public const uint FX_ELEM_TRAIL_ORIENT_BY_VELOCITY = 0x4000;
        public const uint FX_ELEM_EMIT_ORIENT_BY_ELEM = 0x8000;

        /* FxElemAtlas::behavior */
        public const uint FX_ATLAS_START_MASK = 0x3;
        public const uint FX_ATLAS_START_FIXED = 0x0;
        public const uint FX_ATLAS_START_RANDOM = 0x1;
        public const uint FX_ATLAS_START_INDEXED = 0x2;
        public const uint FX_ATLAS_PLAY_OVER_LIFE = 0x4;
        public const uint FX_ATLAS_LOOP_ONLY_N_TIMES = 0x8;
    }

    public enum FxElemType : byte
    {
        FX_ELEM_TYPE_SPRITE_BILLBOARD = 0x0,
        FX_ELEM_TYPE_SPRITE_ORIENTED = 0x1,
        FX_ELEM_TYPE_TAIL = 0x2,
        FX_ELEM_TYPE_TRAIL = 0x3,
        FX_ELEM_TYPE_CLOUD = 0x4,
        FX_ELEM_TYPE_SPARKCLOUD = 0x5,
        FX_ELEM_TYPE_SPARKFOUNTAIN = 0x6,
        FX_ELEM_TYPE_MODEL = 0x7,
        FX_ELEM_TYPE_OMNI_LIGHT = 0x8,
        FX_ELEM_TYPE_SPOT_LIGHT = 0x9,
        FX_ELEM_TYPE_SOUND = 0xA,
        FX_ELEM_TYPE_DECAL = 0xB,
        FX_ELEM_TYPE_RUNNER = 0xC,
        FX_ELEM_TYPE_COUNT = 0xD,
        FX_ELEM_TYPE_LAST_SPRITE = 0x3,
        FX_ELEM_TYPE_LAST_DRAWN = 0x9,
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct FxElemVec3Range
    {
        public fixed float @base[3];
        public fixed float amplitude[3];
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct FxIntRange
    {
        public int @base;
        public int amplitude;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct FxFloatRange
    {
        public float @base;
        public float amplitude;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct FxSpawnDefLooping
    {
        public int intervalMsec;
        public int count;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct FxSpawnDefOneShot
    {
        public FxIntRange count;
    }

    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct FxSpawnDef //: union
    {
        [FieldOffset(0)] public FxSpawnDefLooping looping;
        [FieldOffset(0)] public FxSpawnDefOneShot oneShot;
    }

    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct FxEffectDefRef //: union
    {
        [FieldOffset(0)] public FxEffectDef* handle;
        [FieldOffset(0)] public char* name;
    }

    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct FxElemVisuals //: union
    {
        [FieldOffset(0)] public void* anonymous;
        [FieldOffset(0)] public Material* material;
        [FieldOffset(0)] public XModel* xmodel;
        [FieldOffset(0)] public FxEffectDefRef* effectDef;
        [FieldOffset(0)] public char* soundName;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct FxElemMarkVisuals
    {
        public Material* data0; public Material* data1;
    }

    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct FxElemDefVisuals //: union
    {
        [FieldOffset(0)] public FxElemVisuals instance;
        //If parent FxElemDef::elemType == 0x7, use xmodel
        //If parent FxElemDef::elemType == 0xC, use effectDef
        //If parent FxElemDef::elemType == 0xA, use soundName
        //If parent FxElemDef::elemType != 0x9 || 0x8, use material
        [FieldOffset(0)] public FxElemVisuals* array;           //Total count = parent FxElemDef::visualCount
        [FieldOffset(0)] public FxElemMarkVisuals* markArray;       //Total count = parent FxElemDef::visualCount
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct FxTrailVertex
    {
        public fixed float pos[2];
        public fixed float normal[2];
        public fixed float texCoord[2];
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct FxTrailDef
    {
        public int scrollTimeMsec;
        public int repeatDist;
        public float splitArcDist;
        public int splitDist;
        public int splitTime;
        public int vertCount;
        public FxTrailVertex* verts;
        public int indCount;
        public ushort* inds;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct FxSparkFountain
    {
        public float sparkFountainGravity;
        public float sparkFountainBounceFrac;
        public float sparkFountainBounceRand;
        public float sparkFountainSparkSpacing;
        public float sparkFountainSparkLength;
        public int sparkFountainSparkCount;
        public float sparkFountainLoopTime;
        public float sparkFountainVelMin;
        public float sparkFountainVelMax;
        public float sparkFountainVelConeAngle;
        public float sparkFountainRestSpeed;
        public float sparkFountainBoostTime;
        public float sparkFountainBoostFactor;
    }

    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct unknownFxUnion //: union 
    {
        [FieldOffset(0)] public char* unknownBytes;
        [FieldOffset(0)] public FxSparkFountain* sparkFountain;
        [FieldOffset(0)] public FxTrailDef* trailDef;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct FxElemAtlas
    {
        public char behavior;
        public char index;
        public char fps;
        public char loopCount;
        public char colIndexBits;
        public char rowIndexBits;
        public short entryCount;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct FxElemVelStateInFrame
    {
        public FxElemVec3Range velocity;
        public FxElemVec3Range totalDelta;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct FxElemVelStateSample
    {
        public FxElemVelStateInFrame local;
        public FxElemVelStateInFrame world;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct FxElemVisualState
    {
        public fixed char color[4];
        public float rotationDelta;
        public float rotationTotal;
        public fixed float size[2];
        public float scale;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct FxElemVisStateSample
    {
        public FxElemVisualState @base;
        public FxElemVisualState amplitude;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct FxElemDef    // 0xFC
    {
        public int flags;
        public FxSpawnDef spawn;
        public FxFloatRange spawnRange;
        public FxFloatRange fadeInRange;
        public FxFloatRange fadeOutRange;
        public float spawnFrustumCullRadius;
        public FxIntRange spawnDelayMsec;
        public FxIntRange lifeSpanMsec;
        public FxFloatRange spawnOrigin0; public FxFloatRange spawnOrigin1; public FxFloatRange spawnOrigin2;
        public FxFloatRange spawnOffsetRadius;
        public FxFloatRange spawnOffsetHeight;
        public FxFloatRange spawnAngles0; public FxFloatRange spawnAngles1; public FxFloatRange spawnAngles2;
        public FxFloatRange angularVelocity0; public FxFloatRange angularVelocity1; public FxFloatRange angularVelocity2;
        public FxFloatRange initialRotation;
        public FxFloatRange gravity;
        public FxFloatRange reflectionFactor;
        public FxElemAtlas atlas;
        public char elemType;
        public char visualCount;
        public char velIntervalCount;
        public char visStateIntervalCount;
        public FxElemVelStateSample* velSamples;   // count = velIntervalCount
        public FxElemVisStateSample* visSamples;   // count = visStateIntervalCount
        public FxElemDefVisuals visuals;
        //If elemType is 0xB, then use markVisuals
        //If elemType is not 0xB and visualCount == 1, then use visual
        //If elemType is not 0xB and visualCount != 1, then use visualsArray
        public vec3 collMins;
        public vec3 collMaxs;
        public FxEffectDefRef* effectOnImpact;
        public FxEffectDefRef* effectOnDeath;
        public FxEffectDefRef* effectEmitted;
        public FxFloatRange emitDist;
        public FxFloatRange emitDistVariance;
        public unknownFxUnion* trailDef;
        //If elemType == 3, then use trailDef
        //If elemType == 6, then use sparkFountain
        //If elemType != 3 && elemType != 6 use unknownBytes (size = 1)
        public char sortOrder;
        public char lightingFrac;
        public fixed char unused[2];
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct FxEffectDef
    {
        public char* name;
        public int flags;
        public int totalSize;
        public int msecLoopingLife;
        public int elemDefCountLooping;
        public int elemDefCountOneShot;
        public int elemDefCountEmission;
        public FxElemDef* elemDefs;        //Count = elemDefCountOneShot + elemDefCountEmission + elemDefCountLooping
    }
}