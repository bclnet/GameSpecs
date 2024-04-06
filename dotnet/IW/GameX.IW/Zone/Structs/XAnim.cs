using System;
using System.Runtime.InteropServices;

namespace GameX.IW.Zone
{
    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct XAnimDynamicIndices //: union
    {
        [FieldOffset(0)] public fixed char _1[1];
        [FieldOffset(0)] public fixed ushort _2[1];
    }

    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct XAnimDynamicFrames //: union
    {
        [FieldOffset(0)] public void* _1; //: char (* _1)
        [FieldOffset(0)] public void* _2; //: ushort (* _2)
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct XAnimNotifyInfo
    {
        public ushort name;
        public float time;
    }

    [StructLayout(LayoutKind.Explicit)] //: union
    public unsafe struct XAnimIndices
    {
        [FieldOffset(0)] public char* _1;
        [FieldOffset(0)] public ushort* _2;
        [FieldOffset(0)] public void* data;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct XAnimPartTransFrames
    {
        public fixed float mins[3];
        public fixed float size[3];
        public XAnimDynamicFrames frames;
        public XAnimDynamicIndices indices;
    }

    [StructLayout(LayoutKind.Explicit)] //: union
    public unsafe struct XAnimPartTransData
    {
        [FieldOffset(0)] public XAnimPartTransFrames frames;
        [FieldOffset(0)] public fixed float frame0[3];
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct XAnimPartTrans
    {
        public ushort size;
        public char smallTrans;
        public XAnimPartTransData u;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct XAnimDeltaPartQuatDataFrames2
    {
        public short* frames;
        public fixed char indices[1];
    }

    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct XAnimDeltaPartQuatData2 //: union
    {
        [FieldOffset(0)] public XAnimDeltaPartQuatDataFrames2 frames;
        [FieldOffset(0)] public fixed short frame0[2];
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct XAnimDeltaPartQuat2
    {
        public ushort size;
        public XAnimDeltaPartQuatData2 u;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct XAnimDeltaPartQuatDataFrames
    {
        public short* frames;
        public fixed char indices[1];
    }

    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct XAnimDeltaPartQuatData //: union
    {
        [FieldOffset(0)] public XAnimDeltaPartQuatDataFrames frames;
        [FieldOffset(0)] public fixed short frame0[4];
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct XAnimDeltaPartQuat
    {
        public ushort size;
        public XAnimDeltaPartQuatData u;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct XAnimDeltaPart
    {
        public XAnimPartTrans* trans;
        public XAnimDeltaPartQuat2* quat2;
        public XAnimDeltaPartQuat* quat;
    }

    public enum XAnimPartType
    {
        PART_TYPE_NO_QUAT = 0x0,
        PART_TYPE_HALF_QUAT = 0x1,
        PART_TYPE_FULL_QUAT = 0x2,
        PART_TYPE_HALF_QUAT_NO_SIZE = 0x3,
        PART_TYPE_FULL_QUAT_NO_SIZE = 0x4,
        PART_TYPE_SMALL_TRANS = 0x5,
        PART_TYPE_TRANS = 0x6,
        PART_TYPE_TRANS_NO_SIZE = 0x7,
        PART_TYPE_NO_TRANS = 0x8,
        PART_TYPE_ALL = 0x9,
    }

    public enum XAnimFlags
    {
        XANIM_LOOP_SYNC_TIME = 0x1,
        XANIM_NONLOOP_SYNC_TIME = 0x2,
        XANIM_SYNC_ROOT = 0x4,
        XANIM_COMPLETE = 0x8,
        XANIM_ADDITIVE = 0x10,
        XANIM_CLIENT = 0x20,
        XANIM_SEPARATE = 0x40,
        XANIM_FORCELOAD = 0x80,
        XANIM_PROPOGATE_FLAGS = 0x63,
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe partial struct XAnim
    {
        public char* name; // 0
        public ushort dataByteCount; // 4
        public ushort dataShortCount; // 6
        public ushort dataIntCount; // 8
        public ushort randomDataByteCount; // 10 - 0xA
        public ushort randomDataIntCount;// 12 - 0xC
        public ushort framecount; // 14 - 0xE
        public char pad1; // 16
        public fixed char boneCount[10]; // 17
        public char notetrackCount; // 27
        public bool bLoop; // 28
        public bool bDelta; // 29
        public char assetType; // 30
        public char pad2; // 31
        public int randomDataShortCount; // 32 - 0x20
        public int indexcount; // 36 - 0x24
        public float framerate; // 40 - 0x28
        public float frequency; // 44 - 0x2C
        public short* tagnames; // 48 - 0x30
        public char* dataByte;// 52 - 0x34
        public short* dataShort; // 56 - 0x38
        public int* dataInt; // 60 - 0x3C
        public short* randomDataShort; // 64 - 0x40
        public char* randomDataByte; // 68 - 0x44
        public int* randomDataInt; // 72 - 0x48
        public XAnimIndices indices; // 76 - 0x4C
        public XAnimNotifyInfo* notetracks; // 80 - 0x50
        public XAnimDeltaPart* delta; // 84 - 0x54
                                      // 88 - 0x58
    }


    // used for loading only

    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct XAnimPartQuatFrames //: union
    {
        [FieldOffset(0)] public IntPtr frames_0; //: short(*frames)
        [FieldOffset(4)] public IntPtr frames_1;
        [FieldOffset(8)] public IntPtr frames_2;
        [FieldOffset(12)] public IntPtr frames_3;
        [FieldOffset(0)] public IntPtr frames2_0; //: short(*frames2)
        [FieldOffset(4)] public IntPtr frames2_1;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct XAnimPartQuatDataFrames
    {
        public XAnimPartQuatFrames u;
        public XAnimDynamicIndices indices;
    }

    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct XAnimPartQuatData //: union
    {
        [FieldOffset(0)] public XAnimPartQuatDataFrames frames;
        [FieldOffset(0)] public fixed short frame0[4];
        [FieldOffset(0)] public fixed short frame02[2];
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct XAnimPartQuat
    {
        public ushort size;
        public XAnimPartQuatData u;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct XAnimPartQuatPtr
    {
        public XAnimPartQuat* quat;
        public char partIndex;
        public char quatType;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct XAnimPartTransPtr
    {
        public XAnimPartTrans* trans;
        public char partIndex;
        public char transType;
    }
}