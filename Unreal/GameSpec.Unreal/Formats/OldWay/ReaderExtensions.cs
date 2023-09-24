using System.Collections.Generic;
using System.IO;
using System.Text;
using static GameSpec.Unreal.Formats.OldWay.UPackage;
using static GameSpec.Unreal.Formats.OldWay.UPackage.Gen;

namespace GameSpec.Unreal.Formats.OldWay
{
    static class ReaderExtensions
    {
        public static int ReadUIndex(this BinaryReader r, UPackage package)
        {
            if (package.Version >= VIndexDeprecated) return r.ReadInt32();
            const byte isIndiced = 0x40; // 7th bit
            const byte isNegative = 0x80; // 8th bit
            const byte value = 0xFF - isIndiced - isNegative; // 3F
            const byte isProceeded = 0x80; // 8th bit
            const byte proceededValue = 0xFF - isProceeded; // 7F
            var index = 0;
            var b0 = r.ReadByte();
            if ((b0 & isIndiced) != 0)
            {
                var b1 = r.ReadByte();
                if ((b1 & isProceeded) != 0)
                {
                    var b2 = r.ReadByte();
                    if ((b2 & isProceeded) != 0)
                    {
                        var b3 = r.ReadByte();
                        if ((b3 & isProceeded) != 0) { var b4 = r.ReadByte(); index = b4; }
                        index = (index << 7) + (b3 & proceededValue);
                    }
                    index = (index << 7) + (b2 & proceededValue);
                }
                index = (index << 7) + (b1 & proceededValue);
            }
            return (b0 & isNegative) != 0 // The value is negative or positive?.
                ? -((index << 6) + (b0 & value))
                : (index << 6) + (b0 & value);
        }

        public static UName ReadUNameReference(this BinaryReader r, UPackage package)
        {
            var index = r.ReadUIndex(package);
            if (package.Version >= VNameNumbered || package.Build == BuildName.BioShock)
            {
                var num = r.ReadInt32() - 1;
                return package.Names[index];
            }
            return package.Names[index];
        }
        //public static UName ReadUNameReference(this BinaryReader r, UPackage package, out int num)
        //{
        //    var index = r.ReadUIndex(package);
        //    if (package.Version >= VNameNumbered || package.Build == BuildName.BioShock)
        //    {
        //        num = r.ReadInt32() - 1;
        //        return package.Names[index];
        //    }
        //    num = -1;
        //    return package.Names[index];
        //}

        public static string ReadUString(this BinaryReader r, UPackage package)
        {
            var unfixedSize = r.ReadUIndex(package);
            if (package.BuildAttrib.Gen == Vengeance && package.Version >= 135) unfixedSize = -unfixedSize;
            var size = unfixedSize < 0 ? -unfixedSize : unfixedSize;
            if (unfixedSize > 0) // ANSI
            {
                var b = new byte[size];
                for (var i = 0; i < b.Length; ++i) b[i] = r.ReadByte();
                return b[size - 1] == '\0'
                    ? Encoding.ASCII.GetString(b, 0, b.Length - 1)
                    : Encoding.ASCII.GetString(b, 0, b.Length);
            }
            if (unfixedSize < 0) // UNICODE
            {
                var b = new char[size];
                for (var i = 0; i < b.Length; ++i) b[i] = (char)r.ReadInt16();
                return b[size - 1] == '\0' ? new string(b, 0, b.Length - 1) : new string(b);
            }
            return string.Empty;
        }

        public static string ReadUAnsi(this BinaryReader r)
        {
            var b = new List<byte>();
        nextChar:
            var c = r.ReadByte();
            if (c != '\0')
            {
                b.Add(c);
                goto nextChar;
            }
            return Encoding.UTF8.GetString(b.ToArray());
        }

        public static string ReadUUnicode(this BinaryReader r)
        {
            var b = new List<byte>();
        nextWord:
            var w = r.ReadInt16();
            if (w != 0)
            {
                b.Add((byte)(w >> 8));
                b.Add((byte)(w & 0x00FF));
                goto nextWord;
            }
            return Encoding.Unicode.GetString(b.ToArray());
        }
    }
}