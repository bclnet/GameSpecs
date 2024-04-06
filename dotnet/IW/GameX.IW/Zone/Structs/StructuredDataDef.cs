using System.Runtime.InteropServices;

namespace GameX.IW.Zone
{
    public enum StructuredDataTypeCategory
    {
        DATA_INT = 0x0,
        DATA_BYTE = 0x1,
        DATA_BOOL = 0x2,
        DATA_STRING = 0x3,
        DATA_ENUM = 0x4,
        DATA_STRUCT = 0x5,
        DATA_INDEXED_ARRAY = 0x6,
        DATA_ENUM_ARRAY = 0x7,
        DATA_FLOAT = 0x8,
        DATA_SHORT = 0x9,
        DATA_COUNT = 0xA,
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public unsafe struct StructuredDataEnumEntry
    {
        public char* name;
        public ushort index;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct StructuredDataEnum
    {
        public int entryCount;
        public int reservedEntryCount;
        public StructuredDataEnumEntry* entries;
    }

    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct StructuredDataTypeUnion //: union
    {
        [FieldOffset(0)] public uint stringDataLength;
        [FieldOffset(0)] public int enumIndex;
        [FieldOffset(0)] public int structIndex;
        [FieldOffset(0)] public int indexedArrayIndex;
        [FieldOffset(0)] public int enumedArrayIndex;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct StructuredDataType
    {
        public StructuredDataTypeCategory type;
        public StructuredDataTypeUnion u;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public unsafe struct StructuredDataStructProperty
    {
        public char* name;
        public StructuredDataType item;
        public int offset;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct StructuredDataStruct
    {
        public int propertyCount;
        public StructuredDataStructProperty* properties;
        public int size;
        public uint bitOffset;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct StructuredDataIndexedArray
    {
        public int arraySize;
        public StructuredDataType elementType;
        public uint elementSize;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct StructuredDataEnumedArray
    {
        public int enumIndex;
        public StructuredDataType elementType;
        public uint elementSize;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct StructuredDataDef
    {
        public int version;
        public uint formatChecksum;
        public int enumCount;
        public StructuredDataEnum* enums;
        public int structCount;
        public StructuredDataStruct* structs;
        public int indexedArrayCount;
        public StructuredDataIndexedArray* indexedArrays;
        public int enumedArrayCount;
        public StructuredDataEnumedArray* enumedArrays;
        public StructuredDataType rootType;
        public uint size;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct StructuredDataDefSet
    {
        public char* name;
        public uint defCount;
        public StructuredDataDef* defs;
    }
}