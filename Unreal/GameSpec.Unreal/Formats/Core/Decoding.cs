using System.IO;
using System.Runtime.CompilerServices;

namespace GameSpec.Unreal.Formats.Core
{
    public static class CryptoCore
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static byte RotateRight(byte value, int count) => (byte)((byte)(value >> count) | (byte)(value << (8 - count)));
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static ushort RotateRight(ushort value, int count) => (ushort)((ushort)(value >> count) | (ushort)(value << (16 - count)));
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static byte RotateLeft(byte value, int count) => (byte)((value << count) | (value >> (8 - count)));
    }

    public interface IBufferDecoder
    {
        void PreDecode(Stream stream);
        void DecodeBuild(Stream stream, UPackage.BuildName build);
        void DecodeRead(long position, byte[] buffer, int index, int count);
        unsafe void DecodeByte(long position, byte* b);
    }

    public class CryptoDecoderAA2 : IBufferDecoder
    {
        public void PreDecode(Stream stream) { }
        public void DecodeBuild(Stream stream, UPackage.BuildName build) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte DecryptByte(long position, byte scrambledByte)
        {
            long offsetScramble = (position >> 8) ^ position;
            scrambledByte ^= (byte)offsetScramble;
            return (offsetScramble & 0x02) != 0 ? CryptoCore.RotateLeft(scrambledByte, 1) : scrambledByte;
        }

        public void DecodeRead(long position, byte[] buffer, int index, int count)
        {
            for (int i = index; i < count; ++i) buffer[i] = DecryptByte(position + i, buffer[i]);
        }

        public unsafe void DecodeByte(long position, byte* b) => *b = DecryptByte(position, *b);
    }

    public class CryptoDecoderWithKeyAA2 : IBufferDecoder
    {
        public byte Key = 0x05;
        public CryptoDecoderWithKeyAA2(byte key) => Key = key;
        public void PreDecode(Stream stream) { }
        public void DecodeBuild(Stream stream, UPackage.BuildName build) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte DecryptByte(long position, byte scrambledByte)
        {
            long offsetScramble = (position >> 8) ^ position;
            scrambledByte ^= (byte)offsetScramble;
            if ((offsetScramble & 0x02) != 0)
            {
                if ((sbyte)scrambledByte < 0) scrambledByte = (byte)((scrambledByte << 1) | 1);
                else scrambledByte <<= 1;
            }
            return (byte)(Key ^ scrambledByte);
        }

        public void DecodeRead(long position, byte[] buffer, int index, int count)
        {
            for (int i = index; i < count; ++i) buffer[i] = DecryptByte(position + i, buffer[i]);
        }

        public unsafe void DecodeByte(long position, byte* b) => *b = DecryptByte(position, *b);
    }
}