using System.IO;
using System.Text;
using static GameSpec.Unreal.Formats.Core.UPackage.Gen;

namespace GameSpec.Unreal.Formats.Core
{
    static class ReaderExtensions
    {
        public static int ReadIndex(this BinaryReader r, int version)
        {
            if (version >= UPackage.VIndexDeprecated) return r.ReadInt32();
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

        public static string ReadUString(this BinaryReader r, int version, UPackage.BuildAttribute buildAttrib)
        {
            var unfixedSize = r.ReadIndex(version);
            if (buildAttrib.Gen == Vengeance && version >= 135) unfixedSize = -unfixedSize;
            var size = unfixedSize < 0 ? -unfixedSize : unfixedSize;
            if (unfixedSize > 0) // ANSI
            {
                var chars = new byte[size];
                for (var i = 0; i < chars.Length; ++i) chars[i] = r.ReadByte();
                return chars[size - 1] == '\0'
                    ? Encoding.ASCII.GetString(chars, 0, chars.Length - 1)
                    : Encoding.ASCII.GetString(chars, 0, chars.Length);
            }
            if (unfixedSize < 0) // UNICODE
            {
                var chars = new char[size];
                for (var i = 0; i < chars.Length; ++i) chars[i] = (char)r.ReadInt16();
                return chars[size - 1] == '\0' ? new string(chars, 0, chars.Length - 1) : new string(chars);
            }
            return string.Empty;
        }
    }
}