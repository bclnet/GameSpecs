//#define NONULL

using GameX.Formats;
using GameX.Meta;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Obj = System.Collections.Generic.Dictionary<string, object>;

namespace GameX.Cig.Formats
{
    public unsafe partial class Binary_Dcb : IHaveMetaInfo
    {
        #region Base Types

        struct Header
        {
            public static (string, int) Struct = ("<5I9I8I4I", sizeof(Header));
            public int StructTypes;
            public int PropertyTypes;
            public int EnumTypes;
            public int DataMappings;
            public int RecordTypes;

            public int Booleans;
            public int Int8s;
            public int Int16s;
            public int Int32s;
            public int Int64s;
            public int UInt8s;
            public int UInt16s;
            public int UInt32s;
            public int UInt64s;

            public int Singles;
            public int Doubles;
            public int Guids;
            public int Strings;
            public int Locales;
            public int Enums;
            public int Strongs;
            public int Weaks;

            public int References;
            public int EnumOptions;
            public int TextLength;
            public int Unknown;
        }

        struct DataMapV1_
        {
            public static (string, int) Struct = ("<2I", sizeof(DataMapV1_));
            public uint StructCount;
            public uint StructIndex;
        }

        struct DataMapV0_
        {
            public static (string, int) Struct = ("<2H", sizeof(DataMapV0_));
            public ushort StructCount;
            public ushort StructIndex;
        }

        struct EnumType_
        {
            public static (string, int) Struct = ("<I2H", sizeof(EnumType_));
            public uint NameOffset; public string GetName(Dictionary<uint, string> map) => map[NameOffset];
            public ushort ValueCount;
            public ushort FirstValueIndex;
        }

        struct PropertyType_
        {
            public static (string, int) Struct = ("<I4H", sizeof(PropertyType_));
            public uint NameOffset; public string GetName(Dictionary<uint, string> map) => map[NameOffset];
            public ushort StructIndex;
            [MarshalAs(UnmanagedType.U2)] public EDataType DataType;
            [MarshalAs(UnmanagedType.U2)] public EConversionType ConversionType;
            public ushort Padding;
        }

        struct RecordTypeV1_
        {
            public static (string, int) Struct = ("<3I16x2H", sizeof(RecordTypeV1_));
            public uint NameOffset; public string GetName(Dictionary<uint, string> map) => map[NameOffset];
            public uint FileNameOffset;  // !Legacy
            public uint StructIndex;
            public Guid Hash;
            public ushort VariantIndex;
            public ushort OtherIndex;
        }

        struct RecordTypeV0_
        {
            public static (string, int) Struct = ("<2I16x2H", sizeof(RecordTypeV0_));
            public uint NameOffset; public string GetName(Dictionary<uint, string> map) => map[NameOffset];
            public uint StructIndex;
            public Guid Hash;
            public ushort VariantIndex;
            public ushort OtherIndex;
        }

        internal struct StructType_
        {
            public static (string, int) Struct = ("<2I2JO", sizeof(StructType_));
            public uint NameOffset; public string GetName(Dictionary<uint, string> map) => map[NameOffset];
            public uint ParentTypeIndex;
            public ushort AttributeCount;
            public ushort FirstAttributeIndex;
            public uint NodeType;
        }

        struct Pointer_
        {
            public static (string, int) Struct = ("<2I", sizeof(Pointer_));
            public uint StructType;
            public uint Index;
        }

        struct Reference_
        {
            public static (string, int) Struct = ("<I16c", sizeof(Reference_));
            public uint Item1;
            public Guid Value;
        }

        struct Lookup_
        {
            public static (string, int) Struct = ("<I", sizeof(Lookup_));
            public uint ValueOffset; public string GetValue(Dictionary<uint, string> map) => map[ValueOffset];
        }

        public enum EDataType : ushort
        {
            Reference = 0x0310,
            WeakPointer = 0x0210,
            StrongPointer = 0x0110,
            Class = 0x0010,
            Enum = 0x000F,
            Guid = 0x000E,
            Locale = 0x000D,
            Double = 0x000C,
            Single = 0x000B,
            String = 0x000A,
            UInt64 = 0x0009,
            UInt32 = 0x0008,
            UInt16 = 0x0007,
            Byte = 0x0006,
            Int64 = 0x0005,
            Int32 = 0x0004,
            Int16 = 0x0003,
            SByte = 0x0002,
            Boolean = 0x0001,
        }

        public enum EConversionType : ushort
        {
            Attribute = 0x00,
            ComplexArray = 0x01,
            SimpleArray = 0x02,
        }

        public enum StringSizeEnum
        {
            Int8 = 1,
            Int16 = 2,
            Int32 = 4,
        }

        #endregion

        public static Task<object> Factory(BinaryReader r, FileSource f, PakFile s) => Task.FromResult((object)new Binary_Dcb(r));

        public class Record
        {
            public string Name { get; set; }
            public string FileName { get; set; }
            public Obj Obj { get; set; }
            public int Other { get; set; }
            public Guid Hash { get; internal set; }
        }

        public Binary_Dcb(BinaryReader r)
        {
            var sw = new Stopwatch();
            sw.Start();

            r.Skip(4);
            FileVersion = r.ReadInt32();
            IsLegacy = r.BaseStream.Length < 0x0e2e00;

            if (!IsLegacy) r.Skip(2 << 2);

            var h = r.ReadT<Header>((IsLegacy ? 25 : 26) << 2);
            StructTypes = r.ReadTArray<StructType_>(sizeof(StructType_), h.StructTypes);
            PropertyTypes = r.ReadTArray<PropertyType_>(sizeof(PropertyType_), h.PropertyTypes);
            EnumTypes = r.ReadTArray<EnumType_>(sizeof(EnumType_), h.EnumTypes);
            if (FileVersion >= 5) DataMapV1s = r.ReadTArray<DataMapV1_>(sizeof(DataMapV1_), h.DataMappings);
            else DataMapV0s = r.ReadTArray<DataMapV0_>(sizeof(DataMapV0_), h.DataMappings);
            if (!IsLegacy) RecordTypeV1s = r.ReadTArray<RecordTypeV1_>(sizeof(RecordTypeV1_), h.RecordTypes);
            else RecordTypeV0s = r.ReadTArray<RecordTypeV0_>(sizeof(RecordTypeV0_), h.RecordTypes);

            // read values
            Value_Int8s = r.ReadTArray<sbyte>(sizeof(sbyte), h.Int8s);
            Value_Int16s = r.ReadTArray<short>(sizeof(short), h.Int16s);
            Value_Int32s = r.ReadTArray<int>(sizeof(int), h.Int32s);
            Value_Int64s = r.ReadTArray<long>(sizeof(long), h.Int64s);
            Value_UInt8s = r.ReadTArray<byte>(sizeof(byte), h.UInt8s);
            Value_UInt16s = r.ReadTArray<ushort>(sizeof(ushort), h.UInt16s);
            Value_UInt32s = r.ReadTArray<uint>(sizeof(uint), h.UInt32s);
            Value_UInt64s = r.ReadTArray<ulong>(sizeof(ulong), h.UInt64s);
            Value_Booleans = r.ReadTArray<bool>(sizeof(byte), h.Booleans);
            Value_Singles = r.ReadTArray<float>(sizeof(float), h.Singles);
            Value_Doubles = r.ReadTArray<double>(sizeof(double), h.Doubles);
            Value_Guids = r.ReadTArray<Guid>(sizeof(Guid), h.Guids);
            Value_Strings = r.ReadTArray<Lookup_>(sizeof(Lookup_), h.Strings);
            Value_Locales = r.ReadTArray<Lookup_>(sizeof(Lookup_), h.Locales);
            Value_Enums = r.ReadTArray<Lookup_>(sizeof(Lookup_), h.Enums);
            Value_Strongs = r.ReadTArray<Pointer_>(sizeof(Pointer_), h.Strongs);
            Value_Weaks = r.ReadTArray<Pointer_>(sizeof(Pointer_), h.Weaks);
            Value_References = r.ReadTArray<Reference_>(sizeof(Reference_), h.References);
            Value_EnumOptions = r.ReadTArray<Lookup_>(sizeof(Lookup_), h.EnumOptions);

            // read strings
            var b = new List<string>();
            var maxPosition = r.BaseStream.Position + h.TextLength;
            var startPosition = r.BaseStream.Position;
            while (r.BaseStream.Position < maxPosition)
            {
                var offset = r.BaseStream.Position - startPosition;
                var str = r.ReadCString();
                b.Add(str);
                ValueMap[(uint)offset] = str;
            }
            Values = b.ToArray();

            // read datamap
            if (DataMapV1s != null)
                foreach (var m in DataMapV1s)
                {
                    DataMap[m.StructIndex] = new List<Obj>();
                    ref StructType_ s = ref StructTypes[m.StructIndex];
                    for (var i = 0; i < m.StructCount; i++)
                    {
                        var node = ReadStruct(r, ref s);
                        node["__name"] = s.GetName(ValueMap);
                        DataMap[m.StructIndex].Add(node);
                        DataTable.Add(node);
                    }
                }
            else
                foreach (var m in DataMapV0s)
                {
                    DataMap[m.StructIndex] = new List<Obj>();
                    ref StructType_ s = ref StructTypes[m.StructIndex];
                    for (var i = 0; i < m.StructCount; i++)
                    {
                        var node = ReadStruct(r, ref s);
                        node["__name"] = s.GetName(ValueMap);
                        DataMap[m.StructIndex].Add(node);
                        DataTable.Add(node);
                    }
                }

            // remap datamap
            foreach (var m in Remap_Class)
                m.Map(m.StructIndex == 0xFFFF ? null
                    : DataMap.TryGetValue(m.StructIndex, out var z) && z.Count > m.Index ? (object)z
                    : new Obj {
                            { "__name", "bugged" },
                            { "__class", $"{m.StructIndex:X8}" },
                            { "__index", $"{m.Index:X8}" }
                    }, m.I);
            foreach (var m in Remap_Strong)
            {
                var strong = Value_Strongs[m.Index];
                if (strong.Index == 0xFFFFFFFF) m.Map(null, m.I);
                else m.Map(DataMap[strong.StructType][(int)strong.Index], m.I);
            }
            foreach (var m in Remap_Weak1)
            {
                var weak = Value_Weaks[m.Index];
                m.Map(weak.Index == 0xFFFFFFFF ? 0
                    : (object)new WeakReference(DataMap[weak.StructType][(int)weak.Index]), m.I);
            }
            foreach (var m in Remap_Weak2)
                m.Map(m.StructIndex == 0xFFFF ? null
                    : m.Index == -1 ? (object)new WeakReference(DataMap[m.StructIndex].FirstOrDefault())
                    : new WeakReference(DataMap[m.StructIndex][m.Index]), m.I);

            // read records
            if (RecordTypeV1s != null)
                foreach (var t in RecordTypeV1s)
                    RecordTable.Add(ReadRecord(t.NameOffset, t.FileNameOffset, t.StructIndex, t.Hash, t.VariantIndex, t.OtherIndex));
            else
                foreach (var t in RecordTypeV0s)
                    RecordTable.Add(ReadRecord(t.NameOffset, uint.MaxValue, t.StructIndex, t.Hash, t.VariantIndex, t.OtherIndex));

            sw.Stop();
            Debug.Write($"Elapsed={sw.Elapsed}");
        }

        Record ReadRecord(uint nameOffset, uint fileNameOffset, uint structIndex, Guid hash, ushort variantIndex, ushort otherIndex)
            => new Record
            {
                Name = ValueMap[nameOffset],
                FileName = fileNameOffset != uint.MaxValue ? ValueMap[fileNameOffset] : null,
                Hash = hash,
                Obj = DataMap[structIndex][variantIndex],
                Other = otherIndex,
            };

        Obj ReadStruct(BinaryReader r, ref StructType_ s)
        {
            // add properties
            ref StructType_ p = ref s;
            var properties = new List<PropertyType_>();
            properties.InsertRange(0, Enumerable.Range(p.FirstAttributeIndex, p.AttributeCount).Select(i => PropertyTypes[i]));
            while (p.ParentTypeIndex != 0xFFFFFFFF)
            {
                p = ref StructTypes[p.ParentTypeIndex];
                properties.InsertRange(0, Enumerable.Range(p.FirstAttributeIndex, p.AttributeCount).Select(i => PropertyTypes[i]));
            }

            Obj o;
            var obj = CreateObj();
            obj.Add("__type", p.GetName(ValueMap));
            if (s.ParentTypeIndex != 0xFFFFFFFF) obj.Add("__polyType", s.GetName(ValueMap));

            foreach (var node in properties)
            {
                var nodeName = node.GetName(ValueMap);
                var conversionType = (EConversionType)((int)node.ConversionType & 0xFF);
                if (conversionType == EConversionType.Attribute)
                {
                    if (node.DataType == EDataType.Class)
                    {
                        o = ReadStruct(r, ref StructTypes[node.StructIndex]);
                        obj.Add(nodeName, o);
                    }
                    else if (node.DataType == EDataType.StrongPointer)
                    {
                        Remap_Class.Add(new Remap { Map = (v, i) => obj.Add(nodeName, v), StructIndex = (ushort)r.ReadUInt32(), Index = (int)r.ReadUInt32() });
                    }
                    else
                    {
                        object value;
                        switch (node.DataType)
                        {
                            case EDataType.Reference: value = r.ReadS<Reference_>(); break;
                            case EDataType.Locale: value = ValueMap[r.ReadUInt32()]; break;
                            case EDataType.StrongPointer: Remap_Strong.Add(new Remap { Map = (v, i) => obj.Add(nodeName, v), StructIndex = (ushort)r.ReadUInt32(), Index = (int)r.ReadUInt32() }); continue;
                            case EDataType.WeakPointer: Remap_Weak2.Add(new Remap { Map = (v, i) => obj.Add(nodeName, v), StructIndex = (ushort)r.ReadUInt32(), Index = (int)r.ReadUInt32() }); continue;
                            case EDataType.String: value = ValueMap[r.ReadUInt32()]; break;
                            case EDataType.Boolean: value = r.ReadByte() != 0; break;
                            case EDataType.Single: value = r.ReadSingle(); break;
                            case EDataType.Double: value = r.ReadDouble(); break;
                            case EDataType.Guid: value = r.ReadGuid(); break;
                            case EDataType.SByte: value = r.ReadSByte(); break;
                            case EDataType.Int16: value = r.ReadInt16(); break;
                            case EDataType.Int32: value = r.ReadInt32(); break;
                            case EDataType.Int64: value = r.ReadInt64(); break;
                            case EDataType.Byte: value = r.ReadByte(); break;
                            case EDataType.UInt16: value = r.ReadUInt16(); break;
                            case EDataType.UInt32: value = r.ReadUInt32(); break;
                            case EDataType.UInt64: value = r.ReadUInt64(); break;
                            case EDataType.Enum: value = ValueMap[r.ReadUInt32()]; break;
                            default: throw new NotImplementedException();
                        }
                        obj.Add(nodeName, value);
                    }
                }
                else
                {
                    var arrayCount = r.ReadUInt32();
                    var firstIndex = r.ReadUInt32();
                    var value = new object[arrayCount];
                    for (var i = 0; i < arrayCount; i++)
                        switch (node.DataType)
                        {
                            case EDataType.Boolean: value[i] = Value_Booleans[firstIndex + i]; break;
                            case EDataType.Double: value[i] = Value_Doubles[firstIndex + i]; break;
                            case EDataType.Enum: value[i] = Value_Enums[firstIndex + i].GetValue(ValueMap); break;
                            case EDataType.Guid: value[i] = Value_Guids[firstIndex + i]; break;
                            case EDataType.Int16: value[i] = Value_Int16s[firstIndex + i]; break;
                            case EDataType.Int32: value[i] = Value_Int32s[firstIndex + i]; break;
                            case EDataType.Int64: value[i] = Value_Int64s[firstIndex + i]; break;
                            case EDataType.SByte: value[i] = Value_Int8s[firstIndex + i]; break;
                            case EDataType.Locale: value[i] = Value_Locales[firstIndex + i].GetValue(ValueMap); break;
                            case EDataType.Reference: value[i] = Value_References[firstIndex + i]; break;
                            case EDataType.Single: value[i] = Value_Singles[firstIndex + i]; break;
                            case EDataType.String: value[i] = Value_Strings[firstIndex + i].GetValue(ValueMap); break;
                            case EDataType.UInt16: value[i] = Value_UInt16s[firstIndex + i]; break;
                            case EDataType.UInt32: value[i] = Value_UInt32s[firstIndex + i]; break;
                            case EDataType.UInt64: value[i] = Value_UInt64s[firstIndex + i]; break;
                            case EDataType.Byte: value[i] = Value_UInt8s[firstIndex + i]; break;
                            case EDataType.Class:
                                Remap_Class.Add(new Remap { Map = (v, i) => value[i] = v, I = i, StructIndex = node.StructIndex, Index = (int)(firstIndex + i) });
                                break;
                            case EDataType.StrongPointer:
                                Remap_Strong.Add(new Remap { Map = (v, i) => value[i] = v, I = i, StructIndex = node.StructIndex, Index = (int)(firstIndex + i) });
                                break;
                            case EDataType.WeakPointer:
                                Remap_Weak1.Add(new Remap { Map = (v, i) => value[i] = v, I = i, StructIndex = node.StructIndex, Index = (int)(firstIndex + i) });
                                break;
                            default:
                                throw new NotImplementedException("HERE");
                                //var obj2 = CreateObj($"{node.DataType}");
                                //obj2.Add("x_child", (firstIndex + i).ToString());
                                //obj2.Add("x_parent", node.StructIndex.ToString());
                                //child[i] = obj2;
                                //break;
                        }
                    obj.Add(nodeName, value);
                }
            }
            return obj;
        }

        static Obj CreateObj() => new Obj { };
        static Obj CreateObj(string name) => new Obj { { "__name", name } };

        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                new MetaInfo(null, new MetaContent { EngineType = typeof(ICustomFormatter), Type = "DataForge", Name = Path.GetFileName(file.Path), Value = this }),
                new MetaInfo("DatabasePak", items: new List<MetaInfo> {
                    new MetaInfo($"FileVersion: {FileVersion}"),
                })
            };
            return nodes;
        }

        readonly bool IsLegacy;
        readonly int FileVersion;

        readonly StructType_[] StructTypes;
        readonly PropertyType_[] PropertyTypes;
        readonly EnumType_[] EnumTypes;
        readonly DataMapV0_[] DataMapV0s;
        readonly DataMapV1_[] DataMapV1s;
        readonly RecordTypeV0_[] RecordTypeV0s;
        readonly RecordTypeV1_[] RecordTypeV1s;
        readonly sbyte[] Value_Int8s;
        readonly short[] Value_Int16s;
        readonly int[] Value_Int32s;
        readonly long[] Value_Int64s;
        readonly byte[] Value_UInt8s;
        readonly ushort[] Value_UInt16s;
        readonly uint[] Value_UInt32s;
        readonly ulong[] Value_UInt64s;
        readonly bool[] Value_Booleans;
        readonly float[] Value_Singles;
        readonly double[] Value_Doubles;
        readonly Guid[] Value_Guids;
        readonly Lookup_[] Value_Strings;
        readonly Lookup_[] Value_Locales;
        readonly Lookup_[] Value_Enums;
        readonly Pointer_[] Value_Strongs;
        readonly Pointer_[] Value_Weaks;
        readonly Reference_[] Value_References;
        readonly Lookup_[] Value_EnumOptions;
        readonly string[] Values;

        readonly Dictionary<uint, string> ValueMap = new Dictionary<uint, string>();

        readonly Dictionary<uint, List<Obj>> DataMap = new Dictionary<uint, List<Obj>>();
        readonly List<Obj> DataTable = new List<Obj>();

        public readonly List<Record> RecordTable = new List<Record>();

        class Remap
        {
            public Action<object, int> Map;
            public int I;
            public ushort StructIndex;
            public int Index;
        }

        readonly List<Remap> Remap_Class = new List<Remap>();
        readonly List<Remap> Remap_Strong = new List<Remap>();
        readonly List<Remap> Remap_Weak1 = new List<Remap>();
        readonly List<Remap> Remap_Weak2 = new List<Remap>();
    }
}
