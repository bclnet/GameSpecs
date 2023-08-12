using GameSpec.Formats;
using GameSpec.Metadata;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;

namespace GameSpec.Rsi.Formats
{
    public partial class DataForgeFile : IDisposable, IGetMetadataInfo
    {
        public static Task<object> Factory(BinaryReader r, FileMetadata f, PakFile s) => Task.FromResult((object)new DataForgeFile(r));

        XmlDocument _xmlDocument = new XmlDocument();
        public XmlElement CreateElement(string name) => _xmlDocument.CreateElement(name);
        public XmlAttribute CreateAttribute(string name) => _xmlDocument.CreateAttribute(name);
        public string OuterXML => _xmlDocument.OuterXml;
        public XmlNodeList ChildNodes => _xmlDocument.DocumentElement.ChildNodes;

        public DataForgeFile(BinaryReader r)
        {
            Reader = r;
            IsLegacy = r.BaseStream.Length < 0x0e2e00;
            var temp00 = r.ReadInt32();
            FileVersion = r.ReadInt32();

            if (!IsLegacy) r.Skip(2 * 4);

            var structDefinitionCount = r.ReadUInt16(); var temp03 = r.ReadUInt16();  // 0x02d3
            var propertyDefinitionCount = r.ReadUInt16(); var temp04 = r.ReadUInt16();  // 0x0602
            var enumDefinitionCount = r.ReadUInt16(); var temp05 = r.ReadUInt16();  // 0x0041 : 0x0002 ??? 0xbbad
            var dataMappingCount = r.ReadUInt16(); var temp06 = r.ReadUInt16();  // 0x013c
            var recordDefinitionCount = r.ReadUInt16(); var temp07 = r.ReadUInt16();  // 0x0b35

            var int8ValueCount = r.ReadUInt16(); var temp08 = r.ReadUInt16();  // 0x0014 - Int8
            var int16ValueCount = r.ReadUInt16(); var temp09 = r.ReadUInt16();  // 0x0014 - Int16
            var int64ValueCount = r.ReadUInt16(); var temp10 = r.ReadUInt16();  // 0x0014 - Int32
            var int32ValueCount = r.ReadUInt16(); var temp11 = r.ReadUInt16();  // 0x0024 - Int64
            var uint8ValueCount = r.ReadUInt16(); var temp12 = r.ReadUInt16();  // 0x0014 - UInt8
            var uint16ValueCount = r.ReadUInt16(); var temp13 = r.ReadUInt16();  // 0x0014 - UInt16
            var uint64ValueCount = r.ReadUInt16(); var temp14 = r.ReadUInt16();  // 0x0014 - UInt32
            var uint32ValueCount = r.ReadUInt16(); var temp15 = r.ReadUInt16();  // 0x0014 - UInt64
            var booleanValueCount = r.ReadUInt16(); var temp16 = r.ReadUInt16();  // 0x0014 - Boolean
            var singleValueCount = r.ReadUInt16(); var temp17 = r.ReadUInt16();  // 0x003c - Single
            var doubleValueCount = r.ReadUInt16(); var temp18 = r.ReadUInt16();  // 0x0014 - Double
            var guidValueCount = r.ReadUInt16(); var temp19 = r.ReadUInt16();  // 0x0014 - Guid Array Values
            var stringValueCount = r.ReadUInt16(); var temp20 = r.ReadUInt16();  // 0x0076 - String Array Values
            var localeValueCount = r.ReadUInt16(); var temp21 = r.ReadUInt16();  // 0x0034 - Locale Array Values
            var enumValueCount = r.ReadUInt16(); var temp22 = r.ReadUInt16();  // 0x006e - Enum Array Values
            var strongValueCount = r.ReadUInt16(); var temp23 = r.ReadUInt16();  // 0x1cf3 - ??? Class Array Values
            var weakValueCount = r.ReadUInt16(); var temp24 = r.ReadUInt16();  // 0x079d - ??? Pointer Array Values

            var referenceValueCount = r.ReadUInt16(); var temp25 = r.ReadUInt16();  // 0x0026 - Reference Array Values
            var enumOptionCount = r.ReadUInt16(); var temp26 = r.ReadUInt16();  // 0x0284 - Enum Options
            var textLength = r.ReadUInt32(); var temp27 = IsLegacy ? 0 : r.ReadUInt32();  // 0x2e45 : 0x00066e4b

            StructDefinitionTable = ReadArray<StructDefinition_>(structDefinitionCount);
            PropertyDefinitionTable = ReadArray<PropertyDefinition_>(propertyDefinitionCount);
            EnumDefinitionTable = ReadArray<EnumDefinition_>(enumDefinitionCount);
            DataMappingTable = ReadArray<DataMapping_>(dataMappingCount);
            RecordDefinitionTable = ReadArray<Record_>(recordDefinitionCount);

            Array_Int8Values = ReadArray<Int8_>(int8ValueCount);
            Array_Int16Values = ReadArray<Int16_>(int16ValueCount);
            Array_Int32Values = ReadArray<Int32_>(int32ValueCount);
            Array_Int64Values = ReadArray<Int64_>(int64ValueCount);
            Array_UInt8Values = ReadArray<UInt8_>(uint8ValueCount);
            Array_UInt16Values = ReadArray<UInt16_>(uint16ValueCount);
            Array_UInt32Values = ReadArray<UInt32_>(uint32ValueCount);
            Array_UInt64Values = ReadArray<UInt64_>(uint64ValueCount);
            Array_BooleanValues = ReadArray<Boolean_>(booleanValueCount);
            Array_SingleValues = ReadArray<Single_>(singleValueCount);
            Array_DoubleValues = ReadArray<Double_>(doubleValueCount);
            Array_GuidValues = ReadArray<Guid_>(guidValueCount);
            Array_StringValues = ReadArray<StringLookup_>(stringValueCount);
            Array_LocaleValues = ReadArray<Locale_>(localeValueCount);
            Array_EnumValues = ReadArray<Enum_>(enumValueCount);
            Array_StrongValues = ReadArray<Pointer_>(strongValueCount);
            Array_WeakValues = ReadArray<Pointer_>(weakValueCount);

            Array_ReferenceValues = ReadArray<Reference_>(referenceValueCount);
            EnumOptionTable = ReadArray<StringLookup_>(enumOptionCount);

            var b = new List<String_>();
            var maxPosition = r.BaseStream.Position + textLength;
            var startPosition = r.BaseStream.Position;
            while (r.BaseStream.Position < maxPosition)
            {
                var offset = r.BaseStream.Position - startPosition;
                var dfString = new String_(this);
                b.Add(dfString);
                ValueMap[(uint)offset] = dfString.Value;
            }
            ValueTable = b.ToArray();

            foreach (var dataMapping in DataMappingTable)
            {
                DataMap[dataMapping.StructIndex] = new List<XmlElement>();
                var dataStruct = StructDefinitionTable[dataMapping.StructIndex];
                for (var i = 0; i < dataMapping.StructCount; i++)
                {
                    var node = dataStruct.Read(dataMapping.Name);
                    DataMap[dataMapping.StructIndex].Add(node);
                    DataTable.Add(node);
                }
            }

            foreach (var dataMapping in Require_ClassMapping)
                if (dataMapping.Item2 == 0xFFFF)
#if NONULL
                    dataMapping.Item1.ParentNode.RemoveChild(dataMapping.Item1);
#else
                    dataMapping.Item1.ParentNode.ReplaceChild(_xmlDocument.CreateElement("null"), dataMapping.Item1);
#endif
                else
                    dataMapping.Item1.ParentNode.ReplaceChild(DataMap[dataMapping.Item2][dataMapping.Item3], dataMapping.Item1);
        }

        U[] ReadArray<U>(int arraySize) where U : Serializable_
        {
            if (arraySize == -1) return null;
            return (from i in Enumerable.Range(0, arraySize)
                    let data = (U)Activator.CreateInstance(typeof(U), this)
                    // let hack = data._index = i
                    select data).ToArray();
        }

        public void Dispose()
        {
            Reader?.Dispose();
            Reader = null;
        }

        List<MetadataInfo> IGetMetadataInfo.GetInfoNodes(MetadataManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<MetadataInfo> {
                new MetadataInfo(null, new MetadataContent { EngineType = typeof(ICustomFormatter), Type = "DataForgeApp", Name = Path.GetFileName(file.Path), Value = this }),
                new MetadataInfo("DatabasePak", items: new List<MetadataInfo> {
                    new MetadataInfo($"FileSize: [FileSize]"),
                })
            };
            return nodes;
        }

        public BinaryReader Reader { get; private set; }
        public bool IsLegacy { get; private set; }
        public int FileVersion { get; private set; }

        public StructDefinition_[] StructDefinitionTable { get; private set; }
        public PropertyDefinition_[] PropertyDefinitionTable { get; private set; }
        public EnumDefinition_[] EnumDefinitionTable { get; private set; }
        public DataMapping_[] DataMappingTable { get; private set; }
        public Record_[] RecordDefinitionTable { get; private set; }
        public StringLookup_[] EnumOptionTable { get; private set; }
        public String_[] ValueTable { get; private set; }

        public Reference_[] Array_ReferenceValues { get; private set; }
        public Guid_[] Array_GuidValues { get; private set; }
        public StringLookup_[] Array_StringValues { get; private set; }
        public Locale_[] Array_LocaleValues { get; private set; }
        public Enum_[] Array_EnumValues { get; private set; }
        public Int8_[] Array_Int8Values { get; private set; }
        public Int16_[] Array_Int16Values { get; private set; }
        public Int32_[] Array_Int32Values { get; private set; }
        public Int64_[] Array_Int64Values { get; private set; }
        public UInt8_[] Array_UInt8Values { get; private set; }
        public UInt16_[] Array_UInt16Values { get; private set; }
        public UInt32_[] Array_UInt32Values { get; private set; }
        public UInt64_[] Array_UInt64Values { get; private set; }
        public Boolean_[] Array_BooleanValues { get; private set; }
        public Single_[] Array_SingleValues { get; private set; }
        public Double_[] Array_DoubleValues { get; private set; }
        public Pointer_[] Array_StrongValues { get; private set; }
        public Pointer_[] Array_WeakValues { get; private set; }

        public Dictionary<uint, string> ValueMap { get; private set; } = new Dictionary<uint, string>();
        public Dictionary<uint, List<XmlElement>> DataMap { get; private set; } = new Dictionary<uint, List<XmlElement>>();
        public List<Tuple<XmlElement, ushort, int>> Require_ClassMapping { get; private set; } = new List<Tuple<XmlElement, ushort, int>>();
        public List<Tuple<XmlElement, ushort, int>> Require_StrongMapping { get; private set; } = new List<Tuple<XmlElement, ushort, int>>();
        public List<Tuple<XmlAttribute, ushort, int>> Require_WeakMapping1 { get; private set; } = new List<Tuple<XmlAttribute, ushort, int>>();
        public List<Tuple<XmlAttribute, ushort, int>> Require_WeakMapping2 { get; private set; } = new List<Tuple<XmlAttribute, ushort, int>>();
        public List<XmlElement> DataTable { get; private set; } = new List<XmlElement>();
    }
}
