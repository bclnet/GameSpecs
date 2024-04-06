using GameX.Formats;
using GameX.Meta;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace GameX.Bioware.Formats
{
    public unsafe class Binary_Gff : IHaveMetaInfo
    {
        public static Task<object> Factory(BinaryReader r, FileSource f, PakFile s) => Task.FromResult((object)new Binary_Gff(r));

        // Headers
        #region Headers

        const uint GFF_VERSION3_2 = 0x322e3356; // literal string "V3.2".
        const uint GFF_VERSION3_3 = 0x332e3356; // literal string "V3.3".

        [StructLayout(LayoutKind.Sequential, Pack = 0x1)]
        unsafe struct GFF_Header
        {
            public uint Version;            // Version ("V3.3")
            public uint StructOffset;       // Offset of Struct array as bytes from the beginning of the file
            public uint StructCount;        // Number of elements in Struct array
            public uint FieldOffset;        // Offset of Field array as bytes from the beginning of the file
            public uint FieldCount;         // Number of elements in Field array
            public uint LabelOffset;        // Offset of Label array as bytes from the beginning of the file
            public uint LabelCount;         // Number of elements in Label array
            public uint FieldDataOffset;    // Offset of Field Data as bytes from the beginning of the file
            public uint FieldDataSize;      // Number of bytes in Field Data block
            public uint FieldIndicesOffset; // Offset of Field Indices array as bytes from the beginning of the file
            public uint FieldIndicesSize;   // Number of bytes in Field Indices array
            public uint ListIndicesOffset;  // Offset of List Indices array as bytes from the beginning of the file
            public uint ListIndicesSize;    // Number of bytes in List Indices array
        }

        [StructLayout(LayoutKind.Sequential, Pack = 0x1)]
        unsafe struct GFF_Struct
        {
            public uint Id;                 // Programmer-defined integer ID.
            public uint DataOrDataOffset;   // If FieldCount = 1, this is an index into the Field Array.
                                            // If FieldCount > 1, this is a byte offset into the Field Indices array.
            public uint FieldCount;         // Number of fields in this Struct.
        }

        [StructLayout(LayoutKind.Sequential, Pack = 0x1)]
        unsafe struct GFF_Field
        {
            public uint Type;               // Data type
            public uint LabelIndex;         // Index into the Label Array
            public uint DataOrDataOffset;   // If Type is a simple data type, then this is the value actual of the field.
                                            // If Type is a complex data type, then this is a byte offset into the Field Data block.
        }

        [StructLayout(LayoutKind.Sequential, Pack = 0x1)]
        unsafe struct GFF_Label
        {
            public fixed byte Name[0x10];     // Label
        }

        #endregion

        public enum DataType : uint
        {
            DLG = 0x20474c44,
            QDB = 0x20424451,
            QST = 0x20545351,
        }

        public DataType Type { get; private set; }
        public IDictionary<string, object> Root { get; private set; }
        public IDictionary<uint, object> Index { get; private set; }

        public class ResourceRef
        {
            public string Name;
        }

        public class LocalizedRef
        {
            public uint DialogID;
            public (uint id, string value)[] Values;
        }

        public Binary_Gff(BinaryReader r)
        {
            Type = (DataType)r.ReadUInt32();
            var header = r.ReadT<GFF_Header>(sizeof(GFF_Header));
            if (header.Version != GFF_VERSION3_2 && header.Version != GFF_VERSION3_3) throw new FormatException("BAD MAGIC");
            r.Seek(header.StructOffset);
            var headerStructs = r.ReadTArray<GFF_Struct>(sizeof(GFF_Struct), (int)header.StructCount);
            var index = new Dictionary<uint, object>();
            var structs = new IDictionary<string, object>[header.StructCount];
            for (var i = 0; i < structs.Length; i++)
            {
                var id = headerStructs[i].Id;
                var s = structs[i] = new Dictionary<string, object>();
                if (id == 0) continue;
                s.Add("_", id);
                index.Add(id, s);
            }
            r.Seek(header.FieldOffset);
            var headerFields = r.ReadTArray<GFF_Field>(sizeof(GFF_Field), (int)header.FieldCount).Select<GFF_Field, (uint label, object value)>(x =>
            {
                switch (x.Type)
                {
                    case 0: return (x.LabelIndex, (byte)x.DataOrDataOffset);    //: Byte
                    case 1: return (x.LabelIndex, (char)x.DataOrDataOffset);    //: Char
                    case 2: return (x.LabelIndex, (ushort)x.DataOrDataOffset);  //: Word
                    case 3: return (x.LabelIndex, (short)x.DataOrDataOffset);   //: Short
                    case 4: return (x.LabelIndex, x.DataOrDataOffset);          //: DWord
                    case 5: return (x.LabelIndex, (int)x.DataOrDataOffset);     //: Int
                    case 8: return (x.LabelIndex, BitConverter.ToSingle(BitConverter.GetBytes(x.DataOrDataOffset), 0)); //: Float
                    case 14: return (x.LabelIndex, structs[x.DataOrDataOffset]); //: Struct
                    case 15: //: List
                        r.Seek(header.ListIndicesOffset + x.DataOrDataOffset);
                        var list = new IDictionary<string, object>[(int)r.ReadUInt32()];
                        for (var i = 0; i < list.Length; i++)
                        {
                            var idx = r.ReadUInt32();
                            if (idx >= structs.Length) throw new Exception();
                            list[i] = structs[idx];
                        }
                        return (x.LabelIndex, list);
                }
                r.Seek(header.FieldDataOffset + x.DataOrDataOffset);
                switch (x.Type)
                {
                    case 6: return (x.LabelIndex, r.ReadUInt64());              //: DWord64
                    case 7: return (x.LabelIndex, r.ReadInt64());               //: Int64
                    case 9: return (x.LabelIndex, r.ReadDouble());              //: Double
                    case 10: return (x.LabelIndex, r.ReadL32Encoding());           //: CExoString
                    case 11: return (x.LabelIndex, new ResourceRef { Name = r.ReadL8Encoding() }); //: ResRef
                    case 12: //: CExoLocString
                        r.Skip(4);
                        var dialogID = r.ReadUInt32();
                        var values = new (uint id, string value)[r.ReadUInt32()];
                        for (var i = 0; i < values.Length; i++) values[i] = (r.ReadUInt32(), r.ReadL32Encoding());
                        return (x.LabelIndex, new LocalizedRef { DialogID = dialogID, Values = values });
                    case 13: return (x.LabelIndex, r.ReadBytes((int)r.ReadUInt32()));
                }
                throw new ArgumentOutOfRangeException(nameof(x.Type), x.Type.ToString());
            }).ToArray();
            r.Seek(header.LabelOffset);
            var headerLabels = r.ReadTArray<GFF_Label>(sizeof(GFF_Label), (int)header.LabelCount).Select(x => UnsafeX.FixedAString(x.Name, 0x10)).ToArray();
            // combine
            for (var i = 0; i < structs.Length; i++)
            {
                var fieldCount = headerStructs[i].FieldCount;
                var dataOrDataOffset = headerStructs[i].DataOrDataOffset;
                if (fieldCount == 1)
                {
                    var (label, value) = headerFields[dataOrDataOffset];
                    structs[i].Add(headerLabels[label], value);
                    continue;
                }
                var fields = structs[i];
                r.Seek(header.FieldIndicesOffset + dataOrDataOffset);
                foreach (var idx in r.ReadTArray<uint>(sizeof(uint), (int)fieldCount))
                {
                    var (label, value) = headerFields[idx];
                    fields.Add(headerLabels[label], value);
                }
            }
            Root = structs[0];
            Index = index;
        }

        // IHaveMetaInfo
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                new MetaInfo("BinaryGFF", items: new List<MetaInfo> {
                    new MetaInfo($"Type: {Type}"),
                })
            };
            return nodes;
        }
    }
}
