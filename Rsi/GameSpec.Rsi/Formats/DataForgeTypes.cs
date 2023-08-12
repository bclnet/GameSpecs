using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace GameSpec.Rsi.Formats
{
    public partial class DataForgeFile
    {
        #region Base Types

        public abstract class Serializable_
        {
            public DataForgeFile Root { get; private set; }
            internal BinaryReader r;
            public Serializable_(DataForgeFile root) { Root = root; r = root.Reader; }
        }

        public enum EDataType : ushort
        {
            varReference = 0x0310,
            varWeakPointer = 0x0210,
            varStrongPointer = 0x0110,
            varClass = 0x0010,
            varEnum = 0x000F,
            varGuid = 0x000E,
            varLocale = 0x000D,
            varDouble = 0x000C,
            varSingle = 0x000B,
            varString = 0x000A,
            varUInt64 = 0x0009,
            varUInt32 = 0x0008,
            varUInt16 = 0x0007,
            varByte = 0x0006,
            varInt64 = 0x0005,
            varInt32 = 0x0004,
            varInt16 = 0x0003,
            varSByte = 0x0002,
            varBoolean = 0x0001,
        }

        public enum EConversionType : ushort
        {
            varAttribute = 0x00,
            varComplexArray = 0x01,
            varSimpleArray = 0x02,
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
            public Boolean_(DataForgeFile root) : base(root) => Value = r.ReadBoolean();
            public override string ToString() => string.Format("{0}", Value ? "1" : "0");
            public XmlElement Read()
            {
                var element = Root.CreateElement("Bool");
                var attribute = Root.CreateAttribute("value");
                attribute.Value = Value ? "1" : "0";
                element.Attributes.Append(attribute);
                return element;
            }
        }

        public class Double_ : Serializable_
        {
            public double Value { get; set; }
            public Double_(DataForgeFile root) : base(root) => Value = r.ReadDouble();
            public override string ToString() => string.Format("{0}", Value);
            public XmlElement Read()
            {
                var element = Root.CreateElement("Double");
                var attribute = Root.CreateAttribute("value");
                attribute.Value = Value.ToString();
                element.Attributes.Append(attribute);
                return element;
            }
        }

        public class Enum_ : Serializable_
        {
            uint _value;
            public string Value => Root.ValueMap[_value];
            public Enum_(DataForgeFile root) : base(root) => _value = r.ReadUInt32();
            public override string ToString() => Value;
            public XmlElement Read()
            {
                var element = Root.CreateElement("Enum");
                var attribute = Root.CreateAttribute("value");
                attribute.Value = Value;
                element.Attributes.Append(attribute);
                return element;
            }
        }

        public class Guid_ : Serializable_
        {
            public Guid Value { get; set; }
            public Guid_(DataForgeFile root) : base(root) => Value = r.ReadGuid(false).Value;
            public override string ToString() => Value.ToString();
            public XmlElement Read()
            {
                var element = Root.CreateElement("Guid");
                var attribute = Root.CreateAttribute("value");
                attribute.Value = Value.ToString();
                element.Attributes.Append(attribute);
                return element;
            }
        }

        public class Int16_ : Serializable_
        {
            public short Value { get; set; }
            public Int16_(DataForgeFile root) : base(root) => Value = r.ReadInt16();
            public override string ToString() => string.Format("{0}", Value);
            public XmlElement Read()
            {
                var element = Root.CreateElement("Int16");
                var attribute = Root.CreateAttribute("value");
                attribute.Value = Value.ToString();
                element.Attributes.Append(attribute);
                return element;
            }
        }

        public class Int32_ : Serializable_
        {
            public int Value { get; set; }
            public Int32_(DataForgeFile documentRoot) : base(documentRoot) => Value = r.ReadInt32();
            public override string ToString() => string.Format("{0}", this.Value);
            public XmlElement Read()
            {
                var element = Root.CreateElement("Int32");
                var attribute = Root.CreateAttribute("value");
                attribute.Value = Value.ToString();
                element.Attributes.Append(attribute);
                return element;
            }
        }

        public class Int64_ : Serializable_
        {
            public long Value { get; set; }
            public Int64_(DataForgeFile documentRoot) : base(documentRoot) => Value = r.ReadInt64();
            public override string ToString() => string.Format("{0}", Value);
            public XmlElement Read()
            {
                var element = Root.CreateElement("Int64");
                var attribute = Root.CreateAttribute("value");
                attribute.Value = Value.ToString();
                element.Attributes.Append(attribute);
                return element;
            }
        }

        public class Int8_ : Serializable_
        {
            public sbyte Value { get; set; }
            public Int8_(DataForgeFile root) : base(root) => Value = r.ReadSByte();
            public override string ToString() => string.Format("{0}", Value);
            public XmlElement Read()
            {
                var element = Root.CreateElement("Int8");
                var attribute = Root.CreateAttribute("value");
                attribute.Value = Value.ToString();
                element.Attributes.Append(attribute);
                return element;
            }
        }

        public class Locale_ : Serializable_
        {
            uint _value;
            public string Value => Root.ValueMap[_value];
            public Locale_(DataForgeFile root) : base(root) => _value = r.ReadUInt32();
            public override string ToString() => Value;
            public XmlElement Read()
            {
                var element = Root.CreateElement("LocID");
                var attribute = Root.CreateAttribute("value");
                attribute.Value = Value.ToString();
                element.Attributes.Append(attribute);
                return element;
            }
        }

        public class Pointer_ : Serializable_
        {
            public uint StructType { get; set; }
            public uint Index { get; set; }
            public Pointer_(DataForgeFile root) : base(root)
            {
                StructType = r.ReadUInt32();
                Index = r.ReadUInt32();
            }
            public override string ToString() => string.Format("0x{0:X8} 0x{1:X8}", StructType, Index);
            public XmlElement Read()
            {
                var element = Root.CreateElement("Pointer");
                var attribute = Root.CreateAttribute("typeIndex");
                attribute.Value = string.Format("{0:X4}", StructType);
                element.Attributes.Append(attribute);
                attribute = Root.CreateAttribute("firstIndex");
                attribute.Value = string.Format("{0:X4}", Index);
                element.Attributes.Append(attribute);
                return element;
            }
        }

        public class Reference_ : Serializable_
        {
            public uint Item1 { get; set; }
            public Guid Value { get; set; }
            public Reference_(DataForgeFile root) : base(root)
            {
                Item1 = r.ReadUInt32();
                Value = r.ReadGuid(false).Value;
            }
            public override string ToString() => string.Format("0x{0:X8} 0x{1}", Item1, Value);
            public XmlElement Read()
            {
                var element = Root.CreateElement("Reference");
                var attribute = Root.CreateAttribute("value");
                attribute.Value = Value.ToString();
                element.Attributes.Append(attribute);
                return element;
            }
        }

        public class Single_ : Serializable_
        {
            public float Value { get; set; }
            public Single_(DataForgeFile root) : base(root) => Value = r.ReadSingle();
            public override string ToString() => string.Format("{0}", this.Value);
            public XmlElement Read()
            {
                var element = Root.CreateElement("Single");
                var attribute = Root.CreateAttribute("value");
                attribute.Value = Value.ToString();
                element.Attributes.Append(attribute);
                return element;
            }
        }

        public class String_ : Serializable_
        {
            public string Value { get; set; }
            public String_(DataForgeFile root) : base(root) => Value = r.ReadCString();
            public override string ToString() => Value;
        }

        public class StringLookup_ : Serializable_
        {
            uint _value;
            public string Value => Root.ValueMap[_value];
            public StringLookup_(DataForgeFile root) : base(root) => _value = r.ReadUInt32();
            public override string ToString() => Value;
            public XmlElement Read()
            {
                var element = Root.CreateElement("String");
                var attribute = Root.CreateAttribute("value");
                attribute.Value = Value;
                element.Attributes.Append(attribute);
                return element;
            }
        }

        public class UInt16_ : Serializable_
        {
            public ushort Value { get; set; }
            public UInt16_(DataForgeFile root) : base(root) => Value = r.ReadUInt16();
            public override string ToString() => string.Format("{0}", Value);
            public XmlElement Read()
            {
                var element = Root.CreateElement("UInt16");
                var attribute = Root.CreateAttribute("value");
                attribute.Value = Value.ToString();
                element.Attributes.Append(attribute);
                return element;
            }
        }

        public class UInt32_ : Serializable_
        {
            public uint Value { get; set; }
            public UInt32_(DataForgeFile root) : base(root) => Value = r.ReadUInt32();
            public override string ToString() => string.Format("{0}", Value);
            public XmlElement Read()
            {
                var element = Root.CreateElement("UInt32");
                var attribute = Root.CreateAttribute("value");
                attribute.Value = Value.ToString();
                element.Attributes.Append(attribute);
                return element;
            }
        }

        public class UInt64_ : Serializable_
        {
            public ulong Value { get; set; }
            public UInt64_(DataForgeFile root) : base(root) => Value = r.ReadUInt64();
            public override string ToString() => string.Format("{0}", Value);
            public XmlElement Read()
            {
                var element = Root.CreateElement("UInt64");
                var attribute = Root.CreateAttribute("value");
                attribute.Value = Value.ToString();
                element.Attributes.Append(attribute);
                return element;
            }
        }

        public class UInt8_ : Serializable_
        {
            public byte Value { get; set; }
            public UInt8_(DataForgeFile root) : base(root) => Value = r.ReadByte();
            public override string ToString() => string.Format("{0}", Value);
            public XmlElement Read()
            {
                var element = Root.CreateElement("UInt8");
                var attribute = Root.CreateAttribute("value");
                attribute.Value = Value.ToString();
                element.Attributes.Append(attribute);
                return element;
            }
        }

        #endregion

        #region Complex Types

        public class DataMapping_ : Serializable_
        {
            public ushort StructIndex { get; set; }
            public ushort StructCount { get; set; }
            public uint NameOffset { get; set; }
            public string Name => Root.ValueMap[NameOffset];
            public DataMapping_(DataForgeFile root) : base(root)
            {
                StructCount = r.ReadUInt16();
                StructIndex = r.ReadUInt16();
                NameOffset = root.StructDefinitionTable[StructIndex].NameOffset;
            }
            public override string ToString() => string.Format("0x{1:X4} {2}[0x{0:X4}]", this.StructCount, this.StructIndex, this.Name);
        }

        public class EnumDefinition_ : Serializable_
        {
            public uint NameOffset { get; set; }
            public string Name => Root.ValueMap[NameOffset];
            public ushort ValueCount { get; set; }
            public ushort FirstValueIndex { get; set; }
            public EnumDefinition_(DataForgeFile root) : base(root)
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
            public uint NameOffset { get; set; }
            public string Name => Root.ValueMap[NameOffset];
            public ushort StructIndex { get; set; }
            public EDataType DataType { get; set; }
            public EConversionType ConversionType { get; set; }
            public ushort Padding { get; set; }
            public PropertyDefinition_(DataForgeFile root) : base(root)
            {
                NameOffset = r.ReadUInt32();
                StructIndex = r.ReadUInt16();
                DataType = (EDataType)r.ReadUInt16();
                ConversionType = (EConversionType)r.ReadUInt16();
                Padding = r.ReadUInt16();
            }
            public XmlAttribute Read()
            {
                var attribute = Root.CreateAttribute(Name);
                switch (DataType)
                {
                    case EDataType.varReference: attribute.Value = string.Format("{2}", DataType, r.ReadUInt32(), r.ReadGuid(false)); break;
                    case EDataType.varLocale: attribute.Value = string.Format("{1}", DataType, Root.ValueMap[r.ReadUInt32()]); break;
                    case EDataType.varStrongPointer: attribute.Value = string.Format("{0}:{1:X8} {2:X8}", DataType, r.ReadUInt32(), r.ReadUInt32()); break;
                    case EDataType.varWeakPointer:
                        var structIndex = r.ReadUInt32();
                        var itemIndex = r.ReadUInt32();
                        attribute.Value = string.Format("{0}:{1:X8} {1:X8}", DataType, structIndex, itemIndex);
                        Root.Require_WeakMapping2.Add(new Tuple<XmlAttribute, ushort, int>(attribute, (ushort)structIndex, (int)itemIndex));
                        break;
                    case EDataType.varString: attribute.Value = string.Format("{1}", DataType, Root.ValueMap[r.ReadUInt32()]); break;
                    case EDataType.varBoolean: attribute.Value = string.Format("{1}", DataType, r.ReadByte()); break;
                    case EDataType.varSingle: attribute.Value = string.Format("{1}", DataType, r.ReadSingle()); break;
                    case EDataType.varDouble: attribute.Value = string.Format("{1}", DataType, r.ReadDouble()); break;
                    case EDataType.varGuid: attribute.Value = string.Format("{1}", DataType, r.ReadGuid(false)); break;
                    case EDataType.varSByte: attribute.Value = string.Format("{1}", DataType, r.ReadSByte()); break;
                    case EDataType.varInt16: attribute.Value = string.Format("{1}", DataType, r.ReadInt16()); break;
                    case EDataType.varInt32: attribute.Value = string.Format("{1}", DataType, r.ReadInt32()); break;
                    case EDataType.varInt64: attribute.Value = string.Format("{1}", DataType, r.ReadInt64()); break;
                    case EDataType.varByte: attribute.Value = string.Format("{1}", DataType, r.ReadByte()); break;
                    case EDataType.varUInt16: attribute.Value = string.Format("{1}", DataType, r.ReadUInt16()); break;
                    case EDataType.varUInt32: attribute.Value = string.Format("{1}", DataType, r.ReadUInt32()); break;
                    case EDataType.varUInt64: attribute.Value = string.Format("{1}", DataType, r.ReadUInt64()); break;
                    case EDataType.varEnum:
                        var enumDefinition = Root.EnumDefinitionTable[StructIndex];
                        attribute.Value = string.Format("{1}", enumDefinition.Name, Root.ValueMap[r.ReadUInt32()]);
                        break;
                    default: throw new NotImplementedException();
                }
                return attribute;
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
            public Record_(DataForgeFile root) : base(root)
            {
                NameOffset = r.ReadUInt32();
                if (!Root.IsLegacy) FileNameOffset = r.ReadUInt32();
                StructIndex = r.ReadUInt32();
                Hash = r.ReadGuid(false);
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
            public StructDefinition_(DataForgeFile root) : base(root)
            {
                NameOffset = r.ReadUInt32();
                ParentTypeIndex = r.ReadUInt32();
                AttributeCount = r.ReadUInt16();
                FirstAttributeIndex = r.ReadUInt16();
                NodeType = r.ReadUInt32();
            }
            public XmlElement Read(string name = null)
            {
                XmlAttribute attribute;
                var baseStruct = this;
                var properties = new List<PropertyDefinition_> { };

                // TODO: Do we need to handle property overrides
                properties.InsertRange(0,
                    from index in Enumerable.Range(FirstAttributeIndex, AttributeCount)
                    let property = Root.PropertyDefinitionTable[index]
                    // where !properties.Select(p => p.Name).Contains(property.Name)
                    select property);

                while (baseStruct.ParentTypeIndex != 0xFFFFFFFF)
                {
                    baseStruct = Root.StructDefinitionTable[baseStruct.ParentTypeIndex];
                    properties.InsertRange(0,
                        from index in Enumerable.Range(baseStruct.FirstAttributeIndex, baseStruct.AttributeCount)
                        let property = Root.PropertyDefinitionTable[index]
                        // where !properties.Contains(property)
                        select property);
                }

                var element = Root.CreateElement(name ?? baseStruct.Name);
                foreach (var node in properties)
                {
                    node.ConversionType = (EConversionType)((int)node.ConversionType & 0xFF);
                    if (node.ConversionType == EConversionType.varAttribute)
                    {
                        if (node.DataType == EDataType.varClass)
                        {
                            var dataStruct = Root.StructDefinitionTable[node.StructIndex];
                            var child = dataStruct.Read(node.Name);
                            element.AppendChild(child);
                        }
                        else if (node.DataType == EDataType.varStrongPointer)
                        {
                            var parentSP = Root.CreateElement(node.Name);
                            var emptySP = Root.CreateElement(string.Format("{0}", node.DataType));
                            parentSP.AppendChild(emptySP);
                            element.AppendChild(parentSP);
                            Root.Require_ClassMapping.Add(new Tuple<XmlElement, ushort, int>(emptySP, (ushort)r.ReadUInt32(), (int)r.ReadUInt32()));
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
                        {
                            switch (node.DataType)
                            {
                                case EDataType.varBoolean: child.AppendChild(Root.Array_BooleanValues[firstIndex + i].Read()); break;
                                case EDataType.varDouble: child.AppendChild(Root.Array_DoubleValues[firstIndex + i].Read()); break;
                                case EDataType.varEnum: child.AppendChild(Root.Array_EnumValues[firstIndex + i].Read()); break;
                                case EDataType.varGuid: child.AppendChild(Root.Array_GuidValues[firstIndex + i].Read()); break;
                                case EDataType.varInt16: child.AppendChild(Root.Array_Int16Values[firstIndex + i].Read()); break;
                                case EDataType.varInt32: child.AppendChild(Root.Array_Int32Values[firstIndex + i].Read()); break;
                                case EDataType.varInt64: child.AppendChild(Root.Array_Int64Values[firstIndex + i].Read()); break;
                                case EDataType.varSByte: child.AppendChild(Root.Array_Int8Values[firstIndex + i].Read()); break;
                                case EDataType.varLocale: child.AppendChild(Root.Array_LocaleValues[firstIndex + i].Read()); break;
                                case EDataType.varReference: child.AppendChild(Root.Array_ReferenceValues[firstIndex + i].Read()); break;
                                case EDataType.varSingle: child.AppendChild(Root.Array_SingleValues[firstIndex + i].Read()); break;
                                case EDataType.varString: child.AppendChild(Root.Array_StringValues[firstIndex + i].Read()); break;
                                case EDataType.varUInt16: child.AppendChild(Root.Array_UInt16Values[firstIndex + i].Read()); break;
                                case EDataType.varUInt32: child.AppendChild(Root.Array_UInt32Values[firstIndex + i].Read()); break;
                                case EDataType.varUInt64: child.AppendChild(Root.Array_UInt64Values[firstIndex + i].Read()); break;
                                case EDataType.varByte: child.AppendChild(Root.Array_UInt8Values[firstIndex + i].Read()); break;
                                case EDataType.varClass:
                                    var emptyC = Root.CreateElement(string.Format("{0}", node.DataType));
                                    child.AppendChild(emptyC);
                                    Root.Require_ClassMapping.Add(new Tuple<XmlElement, ushort, int>(emptyC, node.StructIndex, (int)firstIndex + i));
                                    break;
                                case EDataType.varStrongPointer:
                                    var emptySP = Root.CreateElement(string.Format("{0}", node.DataType));
                                    child.AppendChild(emptySP);
                                    Root.Require_StrongMapping.Add(new Tuple<XmlElement, ushort, int>(emptySP, node.StructIndex, (int)firstIndex + i));
                                    break;
                                case EDataType.varWeakPointer:
                                    var weakPointerElement = Root.CreateElement("WeakPointer");
                                    var weakPointerAttribute = Root.CreateAttribute(node.Name);
                                    weakPointerElement.Attributes.Append(weakPointerAttribute);
                                    child.AppendChild(weakPointerElement);
                                    Root.Require_WeakMapping1.Add(new Tuple<XmlAttribute, ushort, int>(weakPointerAttribute, node.StructIndex, (int)firstIndex + i));
                                    break;
                                default:
                                    throw new NotImplementedException();
                                    // var tempe = Root.CreateElement(String.Format("{0}", node.DataType));
                                    // var tempa = Root.CreateAttribute("__child");
                                    // tempa.Value = (firstIndex + i).ToString();
                                    // tempe.Attributes.Append(tempa);
                                    // var tempb = Root.CreateAttribute("__parent");
                                    // tempb.Value = node.StructIndex.ToString();
                                    // tempe.Attributes.Append(tempb);
                                    // child.AppendChild(tempe);
                                    // break;
                            }
                        }
                        element.AppendChild(child);
                    }
                }
                attribute = Root.CreateAttribute("__type");
                attribute.Value = baseStruct.Name;
                element.Attributes.Append(attribute);
                if (ParentTypeIndex != 0xFFFFFFFF)
                {
                    attribute = Root.CreateAttribute("__polymorphicType");
                    attribute.Value = Name;
                    element.Attributes.Append(attribute);
                }
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
                        case EConversionType.varAttribute:
                            if (property.DataType == EDataType.varClass)
                            {
                                b.AppendFormat(@"        [XmlElement(ElementName = ""{0}"")]", property.Name);
                            }
                            else if (property.DataType == EDataType.varStrongPointer)
                            {
                                b.AppendFormat(@"        [XmlArray(ElementName = ""{0}"")]", property.Name);
                                arraySuffix = "[]";
                            }
                            else
                            {
                                b.AppendFormat(@"        [XmlAttribute(AttributeName = ""{0}"")]", property.Name);
                            }
                            break;
                        case EConversionType.varComplexArray:
                        case EConversionType.varSimpleArray:
                            b.AppendFormat(@"        [XmlArray(ElementName = ""{0}"")]", property.Name);
                            arraySuffix = "[]";
                            break;
                    }
                    b.AppendLine();

                    var arrayPrefix = "";
                    if (arraySuffix == "[]")
                    {
                        if (property.DataType == EDataType.varClass || property.DataType == EDataType.varStrongPointer)
                        {
                            b.Append(property.Export());
                        }
                        else if (property.DataType == EDataType.varEnum)
                        {
                            arrayPrefix = "_";
                            b.AppendFormat(@"        [XmlArrayItem(ElementName = ""Enum"", Type=typeof(_{0}))]\n", Root.EnumDefinitionTable[property.StructIndex].Name);
                        }
                        else if (property.DataType == EDataType.varSByte)
                        {
                            arrayPrefix = "_";
                            b.AppendFormat(@"        [XmlArrayItem(ElementName = ""Int8"", Type=typeof(_{0}))]\n", property.DataType.ToString().Replace("var", ""));
                        }
                        else if (property.DataType == EDataType.varByte)
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
                        case EDataType.varClass:
                        case EDataType.varStrongPointer:
                            b.AppendFormat("        public {0}{2} {1} {{ get; set; }}", Root.StructDefinitionTable[property.StructIndex].Name, propertyName, arraySuffix);
                            break;
                        case EDataType.varEnum:
                            b.AppendFormat("        public {3}{0}{2} {1} {{ get; set; }}", Root.EnumDefinitionTable[property.StructIndex].Name, propertyName, arraySuffix, arrayPrefix);
                            break;
                        case EDataType.varReference:
                            if (arraySuffix == "[]")
                                b.AppendFormat("        public {3}{0}{2} {1} {{ get; set; }}", property.DataType.ToString().Replace("var", ""), propertyName, arraySuffix, arrayPrefix);
                            else
                                b.AppendFormat("        public Guid{2} {1} {{ get; set; }}", Root.StructDefinitionTable[property.StructIndex].Name, propertyName, arraySuffix);
                            break;
                        case EDataType.varLocale:
                        case EDataType.varWeakPointer:
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
    }
}
