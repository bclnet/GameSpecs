#define NONULL

using GameX.Formats;
using GameX.Meta;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace GameX.Cig.Formats
{
    public partial class Binary_Dcb_LNG : IHaveMetaInfo
    {
        #region Base Types

        public class ClassMapping
        {
            public XmlNode Node { get; set; }
            public ushort StructIndex { get; set; }
            public int RecordIndex { get; set; }
        }

        public abstract class Serializable_
        {
            public Binary_Dcb_LNG Root { get; private set; }
            internal BinaryReader r;
            public Serializable_(Binary_Dcb_LNG root) { Root = root; r = root.R; }

            public Guid? ReadGuid(bool nullable = false)
            {
                var isNull = nullable && r.ReadInt32() == -1;
                var c = r.ReadInt16();
                var b = r.ReadInt16();
                var a = r.ReadInt32();
                var k = r.ReadByte();
                var j = r.ReadByte();
                var i = r.ReadByte();
                var h = r.ReadByte();
                var g = r.ReadByte();
                var f = r.ReadByte();
                var e = r.ReadByte();
                var d = r.ReadByte();
                if (isNull) return null;
                return new Guid(a, b, c, d, e, f, g, h, i, j, k);
            }
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

        #region Simple Types

        public class Boolean_ : Serializable_
        {
            public bool Value { get; set; }
            public Boolean_(Binary_Dcb_LNG root) : base(root) => Value = r.ReadBoolean();
            public override string ToString() => string.Format("{0}", Value ? "1" : "0");
            public XmlElement Read() => Root.CreateElement("Bool", Root.CreateAttribute("value", Value ? "1" : "0"));
        }

        public class Double_ : Serializable_
        {
            public double Value { get; set; }
            public Double_(Binary_Dcb_LNG root) : base(root) => Value = r.ReadDouble();
            public override string ToString() => string.Format("{0}", Value);
            public XmlElement Read() => Root.CreateElement("Double", Root.CreateAttribute("value", Value.ToString()));
        }

        public class Enum_ : Serializable_
        {
            uint _value;
            public string Value => Root.ValueMap[_value];
            public Enum_(Binary_Dcb_LNG root) : base(root) => _value = r.ReadUInt32();
            public override string ToString() => Value;
            public XmlElement Read() => Root.CreateElement("Enum", Root.CreateAttribute("value", Value));
        }

        public class Guid_ : Serializable_
        {
            public Guid Value { get; set; }
            public Guid_(Binary_Dcb_LNG root) : base(root) => Value = ReadGuid(false).Value;
            public override string ToString() => Value.ToString();
            public XmlElement Read() => Root.CreateElement("Guid", Root.CreateAttribute("value", Value.ToString()));
        }

        public class Int16_ : Serializable_
        {
            public short Value { get; set; }
            public Int16_(Binary_Dcb_LNG root) : base(root) => Value = r.ReadInt16();
            public override string ToString() => string.Format("{0}", Value);
            public XmlElement Read() => Root.CreateElement("Int16", Root.CreateAttribute("value", Value.ToString()));
        }

        public class Int32_ : Serializable_
        {
            public int Value { get; set; }
            public Int32_(Binary_Dcb_LNG documentRoot) : base(documentRoot) => Value = r.ReadInt32();
            public override string ToString() => string.Format("{0}", Value);
            public XmlElement Read() => Root.CreateElement("Int32", Root.CreateAttribute("value", Value.ToString()));
        }

        public class Int64_ : Serializable_
        {
            public long Value { get; set; }
            public Int64_(Binary_Dcb_LNG documentRoot) : base(documentRoot) => Value = r.ReadInt64();
            public override string ToString() => string.Format("{0}", Value);
            public XmlElement Read() => Root.CreateElement("Int64", Root.CreateAttribute("value", Value.ToString()));
        }

        public class Int8_ : Serializable_
        {
            public sbyte Value { get; set; }
            public Int8_(Binary_Dcb_LNG root) : base(root) => Value = r.ReadSByte();
            public override string ToString() => string.Format("{0}", Value);
            public XmlElement Read() => Root.CreateElement("Int8", Root.CreateAttribute("value", Value.ToString()));
        }

        public class Locale_ : Serializable_
        {
            uint _value;
            public string Value => Root.ValueMap[_value];
            public Locale_(Binary_Dcb_LNG root) : base(root) => _value = r.ReadUInt32();
            public override string ToString() => Value;
            public XmlElement Read() => Root.CreateElement("LocID", Root.CreateAttribute("value", Value.ToString()));
        }

        public class Pointer_ : Serializable_
        {
            public uint StructType { get; set; }
            public uint Index { get; set; }
            public Pointer_(Binary_Dcb_LNG root) : base(root) { StructType = r.ReadUInt32(); Index = r.ReadUInt32(); }
            public override string ToString() => string.Format("0x{0:X8} 0x{1:X8}", StructType, Index);
            public XmlElement Read() => Root.CreateElement("Pointer", Root.CreateAttribute("typeIndex", string.Format("{0:X4}", StructType)),
                Root.CreateAttribute("firstIndex", string.Format("{0:X4}", Index)));
        }

        public class Reference_ : Serializable_
        {
            public uint Item1 { get; set; }
            public Guid Value { get; set; }
            public Reference_(Binary_Dcb_LNG root) : base(root) { Item1 = r.ReadUInt32(); Value = ReadGuid().Value; }
            public override string ToString() => string.Format("0x{0:X8} 0x{1}", Item1, Value);
            public XmlElement Read() => Root.CreateElement("Reference", Root.CreateAttribute("item1", string.Format("{0:X4}", Item1)),
                Root.CreateAttribute("value", Value.ToString()));
        }

        public class Single_ : Serializable_
        {
            public float Value { get; set; }
            public Single_(Binary_Dcb_LNG root) : base(root) => Value = r.ReadSingle();
            public override string ToString() => string.Format("{0}", Value);
            public XmlElement Read() => Root.CreateElement("Single", Root.CreateAttribute("value", Value.ToString()));
        }

        public class String_ : Serializable_
        {
            public string Value { get; set; }
            public String_(Binary_Dcb_LNG root) : base(root) => Value = r.ReadCString();
            public override string ToString() => Value;
            public XmlElement Read() => Root.CreateElement("String", Root.CreateAttribute("value", Value));
        }

        public class StringLookup_ : Serializable_
        {
            uint _value;
            public string Value => Root.ValueMap[_value];
            public StringLookup_(Binary_Dcb_LNG root) : base(root) => _value = r.ReadUInt32();
            public override string ToString() => Value;
            public XmlElement Read() => Root.CreateElement("String", Root.CreateAttribute("value", Value));
        }

        public class UInt16_ : Serializable_
        {
            public ushort Value { get; set; }
            public UInt16_(Binary_Dcb_LNG root) : base(root) => Value = r.ReadUInt16();
            public override string ToString() => string.Format("{0}", Value);
            public XmlElement Read() => Root.CreateElement("UInt16", Root.CreateAttribute("value", Value.ToString()));
        }

        public class UInt32_ : Serializable_
        {
            public uint Value { get; set; }
            public UInt32_(Binary_Dcb_LNG root) : base(root) => Value = r.ReadUInt32();
            public override string ToString() => string.Format("{0}", Value);
            public XmlElement Read() => Root.CreateElement("UInt32", Root.CreateAttribute("value", Value.ToString()));
        }

        public class UInt64_ : Serializable_
        {
            public ulong Value { get; set; }
            public UInt64_(Binary_Dcb_LNG root) : base(root) => Value = r.ReadUInt64();
            public override string ToString() => string.Format("{0}", Value);
            public XmlElement Read() => Root.CreateElement("UInt64", Root.CreateAttribute("value", Value.ToString()));
        }

        public class UInt8_ : Serializable_
        {
            public byte Value { get; set; }
            public UInt8_(Binary_Dcb_LNG root) : base(root) => Value = r.ReadByte();
            public override string ToString() => string.Format("{0}", Value);
            public XmlElement Read() => Root.CreateElement("UInt8", Root.CreateAttribute("value", Value.ToString()));
        }

        #endregion

        #region Complex Types

        public class DataMapping_ : Serializable_
        {
            public uint StructIndex { get; set; }
            public uint StructCount { get; set; }
            public string Name => Root.ValueMap[NameOffset]; public uint NameOffset { get; set; }
            public DataMapping_(Binary_Dcb_LNG root) : base(root)
            {
                if (Root.FileVersion >= 5)
                {
                    StructCount = r.ReadUInt32();
                    StructIndex = r.ReadUInt32();
                }
                else
                {
                    StructCount = r.ReadUInt16();
                    StructIndex = r.ReadUInt16();
                }
                NameOffset = root.StructDefinitionTable[StructIndex].NameOffset;
            }
            public override string ToString() => string.Format("0x{1:X4} {2}[0x{0:X4}]", StructCount, StructIndex, Name);
        }

        public class EnumDefinition_ : Serializable_
        {
            public string Name => Root.ValueMap[NameOffset]; public uint NameOffset { get; set; }
            public ushort ValueCount { get; set; }
            public ushort FirstValueIndex { get; set; }
            public EnumDefinition_(Binary_Dcb_LNG root) : base(root)
            {
                NameOffset = r.ReadUInt32();
                ValueCount = r.ReadUInt16();
                FirstValueIndex = r.ReadUInt16();
            }
            public override string ToString() => string.Format("<{0} />", Name);

            public string Export()
            {
                var b = new StringBuilder();
                b.AppendFormat(@"    public enum {0}\n", Name);
                b.AppendLine(@"    {");
                for (uint i = FirstValueIndex, j = (uint)(FirstValueIndex + ValueCount); i < j; i++)
                {
                    b.AppendFormat(@"        [XmlEnum(Name = ""{0}"")]\n", Root.EnumOptionTable[i].Value);
                    b.AppendFormat(@"        _{0},\n", Root.EnumOptionTable[i].Value);
                }
                b.AppendLine(@"    }");
                b.AppendLine();
                b.AppendFormat(@"    public class _{0}\n", Name);
                b.AppendLine(@"    {");
                b.AppendFormat(@"        public {0} Value {{ get; set; }}\n", Name);
                b.AppendLine(@"    }");
                return b.ToString();
            }
        }

        public class PropertyDefinition_ : Serializable_
        {
            public string Name => Root.ValueMap[NameOffset]; public uint NameOffset { get; set; }
            public ushort StructIndex { get; set; }
            public EDataType DataType { get; set; }
            public EConversionType ConversionType { get; set; }
            public ushort Padding { get; set; }
            public PropertyDefinition_(Binary_Dcb_LNG root) : base(root)
            {
                NameOffset = r.ReadUInt32();
                StructIndex = r.ReadUInt16();
                DataType = (EDataType)r.ReadUInt16();
                ConversionType = (EConversionType)r.ReadUInt16();
                Padding = r.ReadUInt16();
            }
            public XmlAttribute Read()
            {
                string value;
                switch (DataType)
                {
                    case EDataType.Reference: value = string.Format("{2}", DataType, r.ReadUInt32(), ReadGuid(false)); break;
                    case EDataType.Locale: value = string.Format("{1}", DataType, Root.ValueMap[r.ReadUInt32()]); break;
                    case EDataType.StrongPointer: value = string.Format("{0}:{1:X8} {2:X8}", DataType, r.ReadUInt32(), r.ReadUInt32()); break;
                    case EDataType.WeakPointer:
                        var structIndex = r.ReadUInt32();
                        var itemIndex = r.ReadUInt32();
                        value = string.Format("{0}:{1:X8} {1:X8}", DataType, structIndex, itemIndex);
                        var attribute = Root.CreateAttribute(Name, value);
                        Root.Require_WeakMapping2.Add(new ClassMapping { Node = attribute, StructIndex = (ushort)structIndex, RecordIndex = (int)itemIndex });
                        return attribute;
                    case EDataType.String: value = string.Format("{1}", DataType, Root.ValueMap[r.ReadUInt32()]); break;
                    case EDataType.Boolean: value = string.Format("{1}", DataType, r.ReadByte()); break;
                    case EDataType.Single: value = string.Format("{1}", DataType, r.ReadSingle()); break;
                    case EDataType.Double: value = string.Format("{1}", DataType, r.ReadDouble()); break;
                    case EDataType.Guid: value = string.Format("{1}", DataType, ReadGuid()); break;
                    case EDataType.SByte: value = string.Format("{1}", DataType, r.ReadSByte()); break;
                    case EDataType.Int16: value = string.Format("{1}", DataType, r.ReadInt16()); break;
                    case EDataType.Int32: value = string.Format("{1}", DataType, r.ReadInt32()); break;
                    case EDataType.Int64: value = string.Format("{1}", DataType, r.ReadInt64()); break;
                    case EDataType.Byte: value = string.Format("{1}", DataType, r.ReadByte()); break;
                    case EDataType.UInt16: value = string.Format("{1}", DataType, r.ReadUInt16()); break;
                    case EDataType.UInt32: value = string.Format("{1}", DataType, r.ReadUInt32()); break;
                    case EDataType.UInt64: value = string.Format("{1}", DataType, r.ReadUInt64()); break;
                    case EDataType.Enum: value = string.Format("{1}", Root.EnumDefinitionTable[StructIndex].Name, Root.ValueMap[r.ReadUInt32()]); break;
                    default: throw new NotImplementedException();
                }
                return Root.CreateAttribute(Name, value);
            }
            public override string ToString() => string.Format("<{0} />", Name);
            public string Export()
            {
                var b = new StringBuilder();
                b.AppendFormat(@"        [XmlArrayItem(Type = typeof({0}))]\n", Root.StructDefinitionTable[StructIndex].Name);
                foreach (var structDefinition in Root.StructDefinitionTable)
                {
                    var allowed = false;
                    var baseStruct = structDefinition;
                    while (baseStruct.ParentTypeIndex != 0xFFFFFFFF && !allowed)
                    {
                        allowed |= baseStruct.ParentTypeIndex == StructIndex;
                        baseStruct = Root.StructDefinitionTable[baseStruct.ParentTypeIndex];
                    }
                    if (allowed) b.AppendFormat(@"        [XmlArrayItem(Type = typeof({0}))]\n", structDefinition.Name);
                }
                return b.ToString();
            }
        }

        public class Record_ : Serializable_
        {
            public string Name => Root.ValueMap[NameOffset]; public uint NameOffset { get; set; }
            public string FileName => Root.ValueMap[FileNameOffset]; public uint FileNameOffset { get; set; }
            public string __structIndex => string.Format("{0:X4}", StructIndex); public uint StructIndex { get; set; }
            public Guid? Hash { get; set; }
            public string __variantIndex => string.Format("{0:X4}", VariantIndex); public ushort VariantIndex { get; set; }
            public string __otherIndex => string.Format("{0:X4}", OtherIndex); public ushort OtherIndex { get; set; }
            public Record_(Binary_Dcb_LNG root) : base(root)
            {
                NameOffset = r.ReadUInt32();
                if (!Root.IsLegacy) FileNameOffset = r.ReadUInt32();
                StructIndex = r.ReadUInt32();
                Hash = ReadGuid();
                VariantIndex = r.ReadUInt16();
                OtherIndex = r.ReadUInt16();
            }
            public override string ToString() => string.Format("<{0} {1:X4} />", Name, StructIndex);
        }

        public class StructDefinition_ : Serializable_
        {
            public string Name => Root.ValueMap[NameOffset]; public uint NameOffset { get; set; }
            public string __parentTypeIndex => string.Format("{0:X4}", ParentTypeIndex); public uint ParentTypeIndex { get; set; }
            public string __attributeCount => string.Format("{0:X4}", AttributeCount); public ushort AttributeCount { get; set; }
            public string __firstAttributeIndex => string.Format("{0:X4}", FirstAttributeIndex); public ushort FirstAttributeIndex { get; set; }
            public string __nodeType => string.Format("{0:X4}", NodeType); public uint NodeType { get; set; }
            public StructDefinition_(Binary_Dcb_LNG root) : base(root)
            {
                NameOffset = r.ReadUInt32();
                ParentTypeIndex = r.ReadUInt32();
                AttributeCount = r.ReadUInt16();
                FirstAttributeIndex = r.ReadUInt16();
                NodeType = r.ReadUInt32();
            }
            public XmlElement Read(string name = null)
            {
                //Debug.Print(".");
                var baseStruct = this;
                var properties = new List<PropertyDefinition_>();

                // TODO: Do we need to handle property overrides
                properties.InsertRange(0,
                    from index in Enumerable.Range(FirstAttributeIndex, AttributeCount)
                    let property = Root.PropertyDefinitionTable[index]
                    select property);

                while (baseStruct.ParentTypeIndex != 0xFFFFFFFF)
                {
                    baseStruct = Root.StructDefinitionTable[baseStruct.ParentTypeIndex];
                    properties.InsertRange(0,
                        from index in Enumerable.Range(baseStruct.FirstAttributeIndex, baseStruct.AttributeCount)
                        let property = Root.PropertyDefinitionTable[index]
                        select property);
                }

                var element = Root.CreateElement(name ?? baseStruct.Name);
                foreach (var node in properties)
                {
                    //Debug.Print($"{node.Name} : {r.BaseStream.Position}");
                    node.ConversionType = (EConversionType)((int)node.ConversionType & 0xFF);
                    if (node.ConversionType == EConversionType.Attribute)
                    {
                        if (node.DataType == EDataType.Class)
                        {
                            var dataStruct = Root.StructDefinitionTable[node.StructIndex];
                            var child = dataStruct.Read(node.Name);
                            element.AppendChild(child);
                        }
                        else if (node.DataType == EDataType.StrongPointer)
                        {
                            var parentSP = Root.CreateElement(node.Name);
                            var emptySP = Root.CreateElement(string.Format("{0}", node.DataType));
                            parentSP.AppendChild(emptySP);
                            element.AppendChild(parentSP);
                            Root.Require_ClassMapping.Add(new ClassMapping { Node = emptySP, StructIndex = (ushort)r.ReadUInt32(), RecordIndex = (int)r.ReadUInt32() });
                        }
                        else
                        {
                            var childAttribute = node.Read();
                            element.Attributes.Append(childAttribute);
                        }
                    }
                    else
                    {
                        var arrayCount = r.ReadUInt32();
                        var firstIndex = r.ReadUInt32();
                        var child = Root.CreateElement(node.Name);
                        for (var i = 0; i < arrayCount; i++)
                            switch (node.DataType)
                            {
                                case EDataType.Boolean: child.AppendChild(Root.Array_BooleanValues[firstIndex + i].Read()); break;
                                case EDataType.Double: child.AppendChild(Root.Array_DoubleValues[firstIndex + i].Read()); break;
                                case EDataType.Enum: child.AppendChild(Root.Array_EnumValues[firstIndex + i].Read()); break;
                                case EDataType.Guid: child.AppendChild(Root.Array_GuidValues[firstIndex + i].Read()); break;
                                case EDataType.Int16: child.AppendChild(Root.Array_Int16Values[firstIndex + i].Read()); break;
                                case EDataType.Int32: child.AppendChild(Root.Array_Int32Values[firstIndex + i].Read()); break;
                                case EDataType.Int64: child.AppendChild(Root.Array_Int64Values[firstIndex + i].Read()); break;
                                case EDataType.SByte: child.AppendChild(Root.Array_Int8Values[firstIndex + i].Read()); break;
                                case EDataType.Locale: child.AppendChild(Root.Array_LocaleValues[firstIndex + i].Read()); break;
                                case EDataType.Reference: child.AppendChild(Root.Array_ReferenceValues[firstIndex + i].Read()); break;
                                case EDataType.Single: child.AppendChild(Root.Array_SingleValues[firstIndex + i].Read()); break;
                                case EDataType.String: child.AppendChild(Root.Array_StringValues[firstIndex + i].Read()); break;
                                case EDataType.UInt16: child.AppendChild(Root.Array_UInt16Values[firstIndex + i].Read()); break;
                                case EDataType.UInt32: child.AppendChild(Root.Array_UInt32Values[firstIndex + i].Read()); break;
                                case EDataType.UInt64: child.AppendChild(Root.Array_UInt64Values[firstIndex + i].Read()); break;
                                case EDataType.Byte: child.AppendChild(Root.Array_UInt8Values[firstIndex + i].Read()); break;
                                case EDataType.Class:
                                    var emptyC = Root.CreateElement(string.Format("{0}", node.DataType));
                                    child.AppendChild(emptyC);
                                    Root.Require_ClassMapping.Add(new ClassMapping { Node = emptyC, StructIndex = node.StructIndex, RecordIndex = (int)(firstIndex + i) });
                                    break;
                                case EDataType.StrongPointer:
                                    var emptySP = Root.CreateElement(string.Format("{0}", node.DataType));
                                    child.AppendChild(emptySP);
                                    Root.Require_StrongMapping.Add(new ClassMapping { Node = emptySP, StructIndex = node.StructIndex, RecordIndex = (int)(firstIndex + i) });
                                    break;
                                case EDataType.WeakPointer:
                                    var weakPointerElement = Root.CreateElement("WeakPointer");
                                    var weakPointerAttribute = Root.CreateAttribute(node.Name);
                                    weakPointerElement.Attributes.Append(weakPointerAttribute);
                                    child.AppendChild(weakPointerElement);
                                    Root.Require_WeakMapping1.Add(new ClassMapping { Node = weakPointerAttribute, StructIndex = node.StructIndex, RecordIndex = (Int32)(firstIndex + i) });
                                    break;
                                default:
                                    throw new NotImplementedException();

                                    // var tempe = Root.CreateElement(String.Format("{0}", node.DataType));
                                    // tempe.Attributes.Append(Root.CreateAttribute("__child", (firstIndex + i).ToString()));
                                    // tempe.Attributes.Append(Root.CreateAttribute("__parent", node.StructIndex.ToString()));
                                    // child.AppendChild(tempe);
                                    // break;
                            }
                        element.AppendChild(child);
                    }
                }

                element.Attributes.Append(Root.CreateAttribute("__type", baseStruct.Name));
                if (ParentTypeIndex != 0xFFFFFFFF) element.Attributes.Append(Root.CreateAttribute("__polymorphicType", Name));
                return element;
            }

            public string Export(string assemblyName = "DataForge")
            {
                var b = new StringBuilder();
                b.AppendLine(@"using System;");
                b.AppendLine(@"using System.Xml.Serialization;");
                b.AppendLine(@"");
                b.AppendFormat(@"namespace {0}\n", assemblyName);
                b.AppendLine(@"{");
                b.AppendFormat(@"    [XmlRoot(ElementName = ""{0}"")]\n", Name);
                b.AppendFormat(@"    public partial class {0}", Name);
                if (ParentTypeIndex != 0xFFFFFFFF) b.AppendFormat(" : {0}\n", Root.StructDefinitionTable[ParentTypeIndex].Name);
                b.AppendLine(@"    {");
                for (uint i = FirstAttributeIndex, j = (uint)(FirstAttributeIndex + AttributeCount); i < j; i++)
                {
                    var property = Root.PropertyDefinitionTable[i];
                    property.ConversionType = (EConversionType)((int)property.ConversionType | 0x6900);
                    var arraySuffix = string.Empty;
                    switch (property.ConversionType)
                    {
                        case EConversionType.Attribute:
                            if (property.DataType == EDataType.Class)
                            {
                                b.AppendFormat(@"        [XmlElement(ElementName = ""{0}"")]", property.Name);
                            }
                            else if (property.DataType == EDataType.StrongPointer)
                            {
                                b.AppendFormat(@"        [XmlArray(ElementName = ""{0}"")]", property.Name);
                                arraySuffix = "[]";
                            }
                            else
                            {
                                b.AppendFormat(@"        [XmlAttribute(AttributeName = ""{0}"")]", property.Name);
                            }
                            break;
                        case EConversionType.ComplexArray:
                        case EConversionType.SimpleArray:
                            b.AppendFormat(@"        [XmlArray(ElementName = ""{0}"")]", property.Name);
                            arraySuffix = "[]";
                            break;
                    }
                    b.AppendLine();

                    var arrayPrefix = "";
                    if (arraySuffix == "[]")
                    {
                        if (property.DataType == EDataType.Class || property.DataType == EDataType.StrongPointer)
                        {
                            b.Append(property.Export());
                        }
                        else if (property.DataType == EDataType.Enum)
                        {
                            arrayPrefix = "_";
                            b.AppendFormat(@"        [XmlArrayItem(ElementName = ""Enum"", Type=typeof(_{0}))]\n", Root.EnumDefinitionTable[property.StructIndex].Name);
                        }
                        else if (property.DataType == EDataType.SByte)
                        {
                            arrayPrefix = "_";
                            b.AppendFormat(@"        [XmlArrayItem(ElementName = ""Int8"", Type=typeof(_{0}))]\n", property.DataType.ToString().Replace("var", ""));
                        }
                        else if (property.DataType == EDataType.Byte)
                        {
                            arrayPrefix = "_";
                            b.AppendFormat(@"        [XmlArrayItem(ElementName = ""UInt8"", Type=typeof(_{0}))]\n", property.DataType.ToString().Replace("var", ""));
                        }
                        else
                        {
                            arrayPrefix = "_";
                            b.AppendFormat(@"        [XmlArrayItem(ElementName = ""{0}"", Type=typeof(_{0}))]\n", property.DataType.ToString().Replace("var", ""));
                        }
                    }

                    var keywords = new HashSet<string>
                    {
                        "Dynamic",
                        "Int16",
                        "Int32",
                        "Int64",
                        "UInt16",
                        "UInt32",
                        "UInt64",
                        "Double",
                        "Single",
                    };
                    var propertyName = property.Name;
                    propertyName = string.Format("{0}{1}", propertyName[0].ToString().ToUpper(), propertyName.Substring(1));
                    if (keywords.Contains(propertyName)) propertyName = string.Format("@{0}", propertyName);
                    switch (property.DataType)
                    {
                        case EDataType.Class:
                        case EDataType.StrongPointer:
                            b.AppendFormat("        public {0}{2} {1} {{ get; set; }}", Root.StructDefinitionTable[property.StructIndex].Name, propertyName, arraySuffix);
                            break;
                        case EDataType.Enum:
                            b.AppendFormat("        public {3}{0}{2} {1} {{ get; set; }}", Root.EnumDefinitionTable[property.StructIndex].Name, propertyName, arraySuffix, arrayPrefix);
                            break;
                        case EDataType.Reference:
                            if (arraySuffix == "[]")
                                b.AppendFormat("        public {3}{0}{2} {1} {{ get; set; }}", property.DataType.ToString().Replace("var", ""), propertyName, arraySuffix, arrayPrefix);
                            else
                                b.AppendFormat("        public Guid{2} {1} {{ get; set; }}", Root.StructDefinitionTable[property.StructIndex].Name, propertyName, arraySuffix);
                            break;
                        case EDataType.Locale:
                        case EDataType.WeakPointer:
                            if (arraySuffix == "[]")
                                b.AppendFormat("        public {3}{0}{2} {1} {{ get; set; }}", property.DataType.ToString().Replace("var", ""), propertyName, arraySuffix, arrayPrefix);
                            else
                                b.AppendFormat("        public String{2} {1} {{ get; set; }}", Root.StructDefinitionTable[property.StructIndex].Name, propertyName, arraySuffix);
                            break;
                        default:
                            b.AppendFormat("        public {3}{0}{2} {1} {{ get; set; }}", property.DataType.ToString().Replace("var", ""), propertyName, arraySuffix, arrayPrefix);
                            break;
                    }
                    b.AppendLine();
                    b.AppendLine();
                }
                b.AppendLine(@"    }");
                b.AppendLine(@"}");
                return b.ToString();
            }

            public override string ToString() => string.Format("<{0} />", Name);
        }

        #endregion

        public static Task<object> Factory(BinaryReader r, FileSource f, PakFile s) => Task.FromResult((object)new Binary_Dcb_LNG(r));

        XmlDocument _xmlDocument = new XmlDocument();
        internal XmlElement CreateElement(string name) => _xmlDocument.CreateElement(name);
        internal XmlElement CreateElement(string name, XmlAttribute attribute) { var element = _xmlDocument.CreateElement(name); element.Attributes.Append(attribute); return element; }
        internal XmlElement CreateElement(string name, XmlAttribute attribute, XmlAttribute attribute2) { var element = _xmlDocument.CreateElement(name); element.Attributes.Append(attribute); element.Attributes.Append(attribute2); return element; }
        internal XmlAttribute CreateAttribute(string name) => _xmlDocument.CreateAttribute(name);
        internal XmlAttribute CreateAttribute(string name, string value) { var attribute = _xmlDocument.CreateAttribute(name); attribute.Value = value; return attribute; }
        internal string OuterXML => _xmlDocument.OuterXml;
        internal XmlNodeList ChildNodes => _xmlDocument.DocumentElement.ChildNodes;

        public Binary_Dcb_LNG(BinaryReader r)
        {
            var sw = new Stopwatch();
            sw.Start();

            R = r;
            r.Skip(4);
            FileVersion = r.ReadInt32();
            IsLegacy = r.BaseStream.Length < 0x0e2e00;

            if (!IsLegacy) r.Skip(2 * 4);

            var structDefinitionCount = r.ReadInt32();
            var propertyDefinitionCount = r.ReadInt32();
            var enumDefinitionCount = r.ReadInt32();
            var dataMappingCount = r.ReadInt32();
            var recordDefinitionCount = r.ReadInt32();

            var booleanValueCount = r.ReadInt32();
            var int8ValueCount = r.ReadInt32();
            var int16ValueCount = r.ReadInt32();
            var int32ValueCount = r.ReadInt32();
            var int64ValueCount = r.ReadInt32();
            var uint8ValueCount = r.ReadInt32();
            var uint16ValueCount = r.ReadInt32();
            var uint32ValueCount = r.ReadInt32();
            var uint64ValueCount = r.ReadInt32();

            var singleValueCount = r.ReadInt32();
            var doubleValueCount = r.ReadInt32();
            var guidValueCount = r.ReadInt32();
            var stringValueCount = r.ReadInt32();
            var localeValueCount = r.ReadInt32();
            var enumValueCount = r.ReadInt32();
            var strongValueCount = r.ReadInt32();
            var weakValueCount = r.ReadInt32();

            var referenceValueCount = r.ReadInt32();
            var enumOptionCount = r.ReadInt32();
            var textLength = r.ReadUInt32();
            var unknown = IsLegacy ? 0 : r.ReadUInt32();

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
                if (dataMapping.StructIndex == 0xFFFF)
#if NONULL
                    dataMapping.Node.ParentNode.RemoveChild(dataMapping.Node);
#else
                    dataMapping.Node.ParentNode.ReplaceChild(_xmlDocument.CreateElement("null"), dataMapping.Node);
#endif
                else if (DataMap.ContainsKey(dataMapping.StructIndex) && DataMap[dataMapping.StructIndex].Count > dataMapping.RecordIndex)
                    dataMapping.Node.ParentNode.ReplaceChild(DataMap[dataMapping.StructIndex][dataMapping.RecordIndex], dataMapping.Node);
                else
                {
                    var bugged = _xmlDocument.CreateElement("bugged");
                    bugged.Attributes.Append(_xmlDocument.CreateAttribute("__class", $"{dataMapping.StructIndex:X8}"));
                    bugged.Attributes.Append(_xmlDocument.CreateAttribute("__index", $"{dataMapping.RecordIndex:X8}"));
                    dataMapping.Node.ParentNode.ReplaceChild(bugged, dataMapping.Node);
                }
            sw.Stop();
            Debug.Write($"Elapsed={sw.Elapsed}");
        }

        U[] ReadArray<U>(int arraySize) where U : Serializable_
        {
            if (arraySize == -1) return null;
            return (from i in Enumerable.Range(0, arraySize)
                    let data = (U)Activator.CreateInstance(typeof(U), this)
                    select data).ToArray();
        }

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

        internal BinaryReader R;

        public bool IsLegacy { get; }
        public int FileVersion { get; }

        public StructDefinition_[] StructDefinitionTable { get; }
        public PropertyDefinition_[] PropertyDefinitionTable { get; }
        public EnumDefinition_[] EnumDefinitionTable { get; }
        public DataMapping_[] DataMappingTable { get; }
        public Record_[] RecordDefinitionTable { get; }
        public StringLookup_[] EnumOptionTable { get; }
        public String_[] ValueTable { get; }

        public Reference_[] Array_ReferenceValues { get; }
        public Guid_[] Array_GuidValues { get; }
        public StringLookup_[] Array_StringValues { get; }
        public Locale_[] Array_LocaleValues { get; }
        public Enum_[] Array_EnumValues { get; }
        public Int8_[] Array_Int8Values { get; }
        public Int16_[] Array_Int16Values { get; }
        public Int32_[] Array_Int32Values { get; }
        public Int64_[] Array_Int64Values { get; }
        public UInt8_[] Array_UInt8Values { get; }
        public UInt16_[] Array_UInt16Values { get; }
        public UInt32_[] Array_UInt32Values { get; }
        public UInt64_[] Array_UInt64Values { get; }
        public Boolean_[] Array_BooleanValues { get; }
        public Single_[] Array_SingleValues { get; }
        public Double_[] Array_DoubleValues { get; }
        public Pointer_[] Array_StrongValues { get; }
        public Pointer_[] Array_WeakValues { get; }

        public Dictionary<uint, string> ValueMap { get; } = new Dictionary<uint, string>();
        public Dictionary<uint, List<XmlElement>> DataMap { get; } = new Dictionary<uint, List<XmlElement>>();
        public List<ClassMapping> Require_ClassMapping { get; } = new List<ClassMapping>();
        public List<ClassMapping> Require_StrongMapping { get; } = new List<ClassMapping>();
        public List<ClassMapping> Require_WeakMapping1 { get; } = new List<ClassMapping>();
        public List<ClassMapping> Require_WeakMapping2 { get; } = new List<ClassMapping>();
        public List<XmlElement> DataTable { get; } = new List<XmlElement>();
    }
}
