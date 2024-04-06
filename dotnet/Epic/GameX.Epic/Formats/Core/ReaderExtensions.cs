using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using static GameX.Epic.Formats.Core.Game;
namespace GameX.Epic.Formats.Core
{
    static class ReaderExtensions
    {
        public static byte ROL8(byte value, int count) => (byte)((value << count) | (value >> (8 - count)));
        public static byte ROR8(byte value, int count) => (byte)((value >> count) | (value << (8 - count)));
        public static ushort ROL16(ushort value, int count) => (ushort)((value << count) | (value >> (16 - count)));
        public static ushort ROR16(ushort value, int count) => (ushort)((value >> count) | (value << (16 - count)));
        public static uint ROL32(uint value, int count) => (value << count) | (value >> (32 - count));
        public static uint ROR32(uint value, int count) => (value >> count) | (value << (32 - count));

        static bool GameUsesCompactIndex(UPackage Ar)
            => Ar.Engine >= UE3 ? false
            : Ar.Engine == UE2X && Ar.ArVer >= 145 ? false
            : Ar.Game == Vanguard && Ar.ArVer >= 128 && Ar.ArLicenseeVer >= 25 ? false
            : true;

        public static T[] ReadArray<T>(this BinaryReader r, UPackage ar, Func<BinaryReader, T> factory) => r.ReadFArray(factory, GameUsesCompactIndex(ar) ? r.ReadCompactIndex(ar) : r.ReadInt32());
        public static Dictionary<TKey, TValue> ReadMap<TKey, TValue>(this BinaryReader r, UPackage ar, Func<BinaryReader, (TKey, TValue)> factory) => r.ReadArray(ar, factory).ToDictionary(x => x.Item1, x => x.Item2);

        public static int ReadCompactIndex(this BinaryReader r, UPackage ar)
        {
            if (ar.Engine >= UE3) throw new Exception("FCompactIndex is missing in UE3");
            var b = r.ReadByte();
            var sign = b & 0x80;    // sign bit
            var shift = 6;
            var r2 = b & 0x3F;
            if ((b & 0x40) != 0)           // has 2nd byte
                do
                {
                    b = r.ReadByte();
                    r2 |= (b & 0x7F) << shift;
                    shift += 7;
                } while ((b & 0x80) != 0); // has more bytes
            return sign != 0 ? -r2 : r2;
        }

        public static string ReadFString(this BinaryReader r, UPackage ar)
        {
            var len = ar.Game >= UE3 ? r.ReadInt32() // just a shortcut for UE3 and UE4
                : ar.Game == Bioshock ? -r.ReadCompactIndex(ar) // Bioshock serialized positive number, but it's string is always unicode
                : ar.Game == Vanguard ? r.ReadCompactIndex(ar)   // this game uses int for arrays, but FCompactIndex for strings
                : ar.GameUsesFCompactIndex ? r.ReadCompactIndex(ar)
                : r.ReadInt32();
            if (len == 0) return string.Empty;
            else if (len > 0) // ANSI
            {
                var b = r.ReadBytes(len);
                if (b.Length != len) throw new Exception("Short string");
                return Encoding.ASCII.GetString(b, 0, b.Length - (b[^1] == '\0' ? 1 : 0));
            }
            else // UNICODE
            {
                len = -len;
                var b = new char[len];
                for (var i = 0; i < len; i++)
                {
                    var c = (char)r.ReadUInt16();
                    if ((c & 0xFF00) != 0) c = '$';
                    b[i] = c;
                }
                // Xbox360 version of Mass Effect 3 is using little-endian strings
                if (ar.Game == MassEffect3 && ar.ReverseBytes) Array.Reverse(b, len, 2);
                return new string(b, 0, b.Length - (b[^1] == '\0' ? 1 : 0));
            }
        }
    }
}