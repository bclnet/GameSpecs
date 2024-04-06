using OpenStack.Graphics;
using System;
using System.IO;
using System.Runtime.InteropServices;
using static System.IO.Polyfill;

namespace GameX.Bethesda.Formats
{
    public interface IHaveEDID
    {
        STRVField EDID { get; }
    }

    public interface IHaveMODL
    {
        MODLGroup MODL { get; }
    }

    public class MODLGroup
    {
        public string Value;
        public float Bound;
        public byte[] Textures; // Texture Files Hashes
        public override string ToString() => $"{Value}";
        public MODLGroup(BinaryReader r, int dataSize) => Value = r.ReadYEncoding(dataSize);
        public void MODBField(BinaryReader r, int dataSize) => Bound = r.ReadSingle();
        public void MODTField(BinaryReader r, int dataSize) => Textures = r.ReadBytes(dataSize);
    }

    public struct FormId32<TRecord> where TRecord : Record
    {
        public readonly uint Id;
        public override string ToString() => $"{Type}:{Id}";
        public string Type => typeof(TRecord).Name.Substring(0, 4);
    }

    public struct FormId<TRecord> where TRecord : Record
    {
        public readonly uint Id;
        public readonly string Name;
        public string Type => typeof(TRecord).Name.Substring(0, 4);
        public override string ToString() => $"{Type}:{Name}{Id}";
        public FormId(uint id) { Id = id; Name = null; }
        public FormId(string name) { Id = 0; Name = name; }
        FormId(uint id, string name) { Id = id; Name = name; }
        public FormId<TRecord> AddName(string name) => new FormId<TRecord>(Id, name);
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ColorRef3 { public static (string, int) Struct = ("<3c", 3); public byte Red; public byte Green; public byte Blue; public override string ToString() => $"{Red}:{Green}:{Blue}"; }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ColorRef4 { public static (string, int) Struct = ("<4c", 4); public byte Red; public byte Green; public byte Blue; public byte Null; public override string ToString() => $"{Red}:{Green}:{Blue}"; public GXColor32 ToColor32() => new GXColor32(Red, Green, Blue, 255); }

    public static class ReaderExtension
    {
        public static INTVField ReadINTV(this BinaryReader r, int length)
        {
            return length switch
            {
                1 => new INTVField { Value = r.ReadByte() },
                2 => new INTVField { Value = r.ReadInt16() },
                4 => new INTVField { Value = r.ReadInt32() },
                8 => new INTVField { Value = r.ReadInt64() },
                _ => throw new NotImplementedException($"Tried to read an INTV subrecord with an unsupported size ({length})"),
            };
        }
        public static DATVField ReadDATV(this BinaryReader r, int length, char type)
        {
            return type switch
            {
                'b' => new DATVField { B = r.ReadInt32() != 0 },
                'i' => new DATVField { I = r.ReadInt32() },
                'f' => new DATVField { F = r.ReadSingle() },
                's' => new DATVField { S = r.ReadYEncoding(length) },
                _ => throw new InvalidOperationException($"{type}"),
            };
        }
        public static STRVField ReadSTRV(this BinaryReader r, int length) => new STRVField { Value = r.ReadYEncoding(length) };
        public static STRVField ReadSTRV_ZPad(this BinaryReader r, int length) => new STRVField { Value = r.ReadZString(length) };
        public static FILEField ReadFILE(this BinaryReader r, int length) => new FILEField { Value = r.ReadYEncoding(length) };
        public static BYTVField ReadBYTV(this BinaryReader r, int length) => new BYTVField { Value = r.ReadBytes(length) };
        public static UNKNField ReadUNKN(this BinaryReader r, int length) => new UNKNField { Value = r.ReadBytes(length) };
    }

    public struct STRVField { public string Value; public override string ToString() => Value; }
    public struct FILEField { public string Value; public override string ToString() => Value; }
    public struct INTVField { public static (string, int) Struct = ("<q", 8); public long Value; public override string ToString() => $"{Value}"; public UI16Field ToUI16Field() => new UI16Field { Value = (ushort)Value }; }
    public struct DATVField { public bool B; public int I; public float F; public string S; public override string ToString() => "DATV"; }
    public struct FLTVField { public static (string, int) Struct = ("<f", 4); public float Value; public override string ToString() => $"{Value}"; }
    public struct BYTEField { public static (string, int) Struct = ("<c", 1); public byte Value; public override string ToString() => $"{Value}"; }
    public struct IN16Field { public static (string, int) Struct = ("<h", 2); public short Value; public override string ToString() => $"{Value}"; }
    public struct UI16Field { public static (string, int) Struct = ("<H", 2); public ushort Value; public override string ToString() => $"{Value}"; }
    public struct IN32Field { public static (string, int) Struct = ("<i", 4); public int Value; public override string ToString() => $"{Value}"; }
    public struct UI32Field { public static (string, int) Struct = ("<I", 4); public uint Value; public override string ToString() => $"{Value}"; }
    public struct FMIDField<TRecord> where TRecord : Record
    {
        public FormId<TRecord> Value;
        public override string ToString() => $"{Value}";
        public FMIDField(BinaryReader r, int dataSize)
        {
            Value = dataSize == 4 ?
                new FormId<TRecord>(r.ReadUInt32()) :
                new FormId<TRecord>(r.ReadZString(dataSize));
        }
        public void AddName(string name) => Value = Value.AddName(name);
    }
    public struct FMID2Field<TRecord> where TRecord : Record
    {
        public FormId<TRecord> Value1;
        public FormId<TRecord> Value2;
        public override string ToString() => $"{Value1}x{Value2}";
        public FMID2Field(BinaryReader r, int dataSize)
        {
            Value1 = new FormId<TRecord>(r.ReadUInt32());
            Value2 = new FormId<TRecord>(r.ReadUInt32());
        }
    }
    public struct CREFField { public static (string, int) Struct = ("<4c", 4); public ColorRef4 Color; }
    public struct CNTOField
    {
        public uint ItemCount; // Number of the item
        public FormId<Record> Item; // The ID of the item
        public override string ToString() => $"{Item}";
        public CNTOField(BinaryReader r, int dataSize, BethesdaFormat format)
        {
            if (format == BethesdaFormat.TES3)
            {
                ItemCount = r.ReadUInt32();
                Item = new FormId<Record>(r.ReadZString(32));
                return;
            }
            Item = new FormId<Record>(r.ReadUInt32());
            ItemCount = r.ReadUInt32();
        }
    }
    public struct BYTVField { public byte[] Value; public override string ToString() => $"BYTS"; }
    public struct UNKNField { public byte[] Value; public override string ToString() => $"UNKN"; }
}