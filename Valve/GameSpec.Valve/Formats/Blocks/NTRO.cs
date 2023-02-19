using System.Collections.Generic;
using System.IO;

namespace GameSpec.Valve.Formats.Blocks
{
    /// <summary>
    /// "NTRO" block. CResourceIntrospectionManifest.
    /// </summary>
    public class NTRO : Block
    {
        public enum DataType
        {
            Struct = 1,
            Enum = 2, // TODO: not verified with resourceinfo
            ExternalReference = 3,
            String4 = 4, // TODO: not verified with resourceinfo
            SByte = 10,
            Byte = 11,
            Int16 = 12,
            UInt16 = 13,
            Int32 = 14,
            UInt32 = 15,
            Int64 = 16, // TODO: not verified with resourceinfo
            UInt64 = 17,
            Float = 18,
            Matrix2x4 = 21, // TODO: FourVectors2D
            Vector = 22,
            Vector4D = 23,
            Quaternion = 25,
            Fltx4 = 27,
            Color = 28, // TODO: not verified with resourceinfo
            Boolean = 30,
            String = 31,
            Matrix3x4 = 33,
            Matrix3x4a = 36,
            CTransform = 40,
            Vector4D_44 = 44,
        }

        public class ResourceDiskStruct
        {
            public class Field
            {
                public string FieldName { get; set; }
                public short Count { get; set; }
                public short OnDiskOffset { get; set; }
                public List<byte> Indirections { get; private set; } = new List<byte>();
                public uint TypeData { get; set; }
                public DataType Type { get; set; }
                public ushort Unknown { get; set; }

                public void WriteText(IndentedTextWriter w)
                {
                    w.WriteLine("CResourceDiskStructField {"); w.Indent++;
                    w.WriteLine($"CResourceString m_pFieldName = \"{FieldName}\"");
                    w.WriteLine($"int16 m_nCount = {Count}");
                    w.WriteLine($"int16 m_nOnDiskOffset = {OnDiskOffset}");
                    w.WriteLine($"uint8[{Indirections.Count}] m_Indirection = ["); w.Indent++;
                    foreach (var dep in Indirections) w.WriteLine("{0:D2}", dep);
                    w.Indent--; w.WriteLine("]");
                    w.WriteLine($"uint32 m_nTypeData = 0x{TypeData:X8}");
                    w.WriteLine($"int16 m_nType = {(int)Type}");
                    w.Indent--; w.WriteLine("}");
                }
            }

            public uint IntrospectionVersion { get; set; }
            public uint Id { get; set; }
            public string Name { get; set; }
            public uint DiskCrc { get; set; }
            public int UserVersion { get; set; }
            public ushort DiskSize { get; set; }
            public ushort Alignment { get; set; }
            public uint BaseStructId { get; set; }
            public byte StructFlags { get; set; }
            public ushort Unknown { get; set; }
            public byte Unknown2 { get; set; }
            public List<Field> FieldIntrospection { get; private set; } = new List<Field>();

            public void WriteText(IndentedTextWriter w)
            {
                w.WriteLine("CResourceDiskStruct {"); w.Indent++;
                w.WriteLine($"uint32 m_nIntrospectionVersion = 0x{IntrospectionVersion:X8}");
                w.WriteLine($"uint32 m_nId = 0x{Id:X8}");
                w.WriteLine($"CResourceString m_pName = \"{Name}\"");
                w.WriteLine($"uint32 m_nDiskCrc = 0x{DiskCrc:X8}");
                w.WriteLine($"int32 m_nUserVersion = {UserVersion}");
                w.WriteLine($"uint16 m_nDiskSize = 0x{DiskSize:X4}");
                w.WriteLine($"uint16 m_nAlignment = 0x{Alignment:X4}");
                w.WriteLine($"uint32 m_nBaseStructId = 0x{BaseStructId:X8}");
                w.WriteLine($"Struct m_FieldIntrospection[{FieldIntrospection.Count}] = ["); w.Indent++;
                foreach (var field in FieldIntrospection) field.WriteText(w);
                w.Indent--; w.WriteLine("]");
                w.WriteLine($"uint8 m_nStructFlags = 0x{StructFlags:X2}");
                w.Indent--; w.WriteLine("}");
            }
        }

        public class ResourceDiskEnum
        {
            public class Value
            {
                public string EnumValueName { get; set; }
                public int EnumValue { get; set; }

                public void WriteText(IndentedTextWriter w)
                {
                    w.WriteLine("CResourceDiskEnumValue {"); w.Indent++;
                    w.WriteLine("CResourceString m_pEnumValueName = \"{EnumValueName}\"");
                    w.WriteLine("int32 m_nEnumValue = {EnumValue}");
                    w.Indent--; w.WriteLine("}");
                }
            }

            public uint IntrospectionVersion { get; set; }
            public uint Id { get; set; }
            public string Name { get; set; }
            public uint DiskCrc { get; set; }
            public int UserVersion { get; set; }
            public List<Value> EnumValueIntrospection { get; private set; } = new List<Value>();

            public void WriteText(IndentedTextWriter w)
            {
                w.WriteLine("CResourceDiskEnum {"); w.Indent++;
                w.WriteLine($"uint32 m_nIntrospectionVersion = 0x{IntrospectionVersion:X8}");
                w.WriteLine($"uint32 m_nId = 0x{Id:X8}");
                w.WriteLine($"CResourceString m_pName = \"{Name}\"");
                w.WriteLine($"uint32 m_nDiskCrc = 0x{DiskCrc:X8}");
                w.WriteLine($"int32 m_nUserVersion = {UserVersion}");
                w.WriteLine($"Struct m_EnumValueIntrospection[{EnumValueIntrospection.Count}] = ["); w.Indent++;
                foreach (var value in EnumValueIntrospection) value.WriteText(w);
                w.Indent--; w.WriteLine("]");
                w.Indent--; w.WriteLine("}");
            }
        }

        public uint IntrospectionVersion { get; private set; }

        public List<ResourceDiskStruct> ReferencedStructs { get; } = new List<ResourceDiskStruct>();
        public List<ResourceDiskEnum> ReferencedEnums { get; } = new List<ResourceDiskEnum>();

        public override void Read(BinaryPak parent, BinaryReader r)
        {
            r.Seek(Offset);
            IntrospectionVersion = r.ReadUInt32();
            ReadStructs(r);
            r.BaseStream.Position = Offset + 12; // skip 3 ints
            ReadEnums(r);
        }

        void ReadStructs(BinaryReader r)
        {
            var entriesOffset = r.ReadUInt32();
            var entriesCount = r.ReadUInt32();
            if (entriesCount == 0) return;

            r.BaseStream.Position += entriesOffset - 8; // offset minus 2 ints we just read
            for (var i = 0; i < entriesCount; i++)
            {
                var diskStruct = new ResourceDiskStruct
                {
                    IntrospectionVersion = r.ReadUInt32(),
                    Id = r.ReadUInt32(),
                    Name = r.ReadO32UTF8(),
                    DiskCrc = r.ReadUInt32(),
                    UserVersion = r.ReadInt32(),
                    DiskSize = r.ReadUInt16(),
                    Alignment = r.ReadUInt16(),
                    BaseStructId = r.ReadUInt32()
                };

                var fieldsOffset = r.ReadUInt32();
                var fieldsSize = r.ReadUInt32();
                if (fieldsSize > 0)
                {
                    var prev = r.BaseStream.Position;
                    r.BaseStream.Position += fieldsOffset - 8; // offset minus 2 ints we just read

                    for (var y = 0; y < fieldsSize; y++)
                    {
                        var field = new ResourceDiskStruct.Field
                        {
                            FieldName = r.ReadO32UTF8(),
                            Count = r.ReadInt16(),
                            OnDiskOffset = r.ReadInt16()
                        };

                        var indirectionOffset = r.ReadUInt32();
                        var indirectionSize = r.ReadUInt32();
                        if (indirectionSize > 0)
                        {
                            // jump to indirections
                            var prev2 = r.BaseStream.Position;
                            r.BaseStream.Position += indirectionOffset - 8; // offset minus 2 ints we just read
                            for (var x = 0; x < indirectionSize; x++)
                                field.Indirections.Add(r.ReadByte());
                            r.BaseStream.Position = prev2;
                        }
                        field.TypeData = r.ReadUInt32();
                        field.Type = (DataType)r.ReadInt16();
                        field.Unknown = r.ReadUInt16();
                        diskStruct.FieldIntrospection.Add(field);
                    }
                    r.BaseStream.Position = prev;
                }

                diskStruct.StructFlags = r.ReadByte();
                diskStruct.Unknown = r.ReadUInt16();
                diskStruct.Unknown2 = r.ReadByte();
                ReferencedStructs.Add(diskStruct);
            }
        }

        void ReadEnums(BinaryReader r)
        {
            var entriesOffset = r.ReadUInt32();
            var entriesCount = r.ReadUInt32();
            if (entriesCount == 0) return;

            r.BaseStream.Position += entriesOffset - 8; // offset minus 2 ints we just read
            for (var i = 0; i < entriesCount; i++)
            {
                var diskEnum = new ResourceDiskEnum
                {
                    IntrospectionVersion = r.ReadUInt32(),
                    Id = r.ReadUInt32(),
                    Name = r.ReadO32UTF8(),
                    DiskCrc = r.ReadUInt32(),
                    UserVersion = r.ReadInt32()
                };

                var fieldsOffset = r.ReadUInt32();
                var fieldsSize = r.ReadUInt32();
                if (fieldsSize > 0)
                {
                    var prev = r.BaseStream.Position;
                    r.BaseStream.Position += fieldsOffset - 8; // offset minus 2 ints we just read
                    for (var y = 0; y < fieldsSize; y++) diskEnum.EnumValueIntrospection.Add(new ResourceDiskEnum.Value { EnumValueName = r.ReadO32UTF8(), EnumValue = r.ReadInt32() });
                    r.BaseStream.Position = prev;
                }
                ReferencedEnums.Add(diskEnum);
            }
        }

        public override void WriteText(IndentedTextWriter w)
        {
            w.WriteLine("CResourceIntrospectionManifest {"); w.Indent++;
            w.WriteLine($"uint32 m_nIntrospectionVersion = 0x{IntrospectionVersion:x8}");
            w.WriteLine($"Struct m_ReferencedStructs[{ReferencedStructs.Count}] = ["); w.Indent++;
            foreach (var refStruct in ReferencedStructs) refStruct.WriteText(w);
            w.Indent--; w.WriteLine("]");
            w.WriteLine($"Struct m_ReferencedEnums[{ReferencedEnums.Count}] = ["); w.Indent++;
            foreach (var refEnum in ReferencedEnums) refEnum.WriteText(w);
            w.Indent--; w.WriteLine("]");
            w.Indent--; w.WriteLine("}");
        }
    }
}
