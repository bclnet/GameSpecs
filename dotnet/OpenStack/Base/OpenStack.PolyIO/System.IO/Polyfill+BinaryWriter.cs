using System.Text;

namespace System.IO
{
    public static partial class Polyfill
    {
        #region Endian

        public static void WriteE(this BinaryWriter source, byte[] bytes, int sizeOf, bool endian = true) { if (!endian) { source.Write(bytes); return; } for (var i = 0; i < bytes.Length; i += sizeOf) Array.Reverse(bytes, i, sizeOf); source.Write(bytes); }
        public static void WriteE(this BinaryWriter source, double value, bool endian = true) { if (!endian) { source.Write(value); return; } var bytes = BitConverter.GetBytes(value); Array.Reverse(bytes, 0, bytes.Length); source.Write(value); }
        public static void WriteE(this BinaryWriter source, short value, bool endian = true) { if (!endian) { source.Write(value); return; } var bytes = BitConverter.GetBytes(value); Array.Reverse(bytes, 0, bytes.Length); source.Write(value); }
        public static void WriteE(this BinaryWriter source, int value, bool endian = true) { if (!endian) { source.Write(value); return; } var bytes = BitConverter.GetBytes(value); Array.Reverse(bytes, 0, bytes.Length); source.Write(value); }
        public static void WriteE(this BinaryWriter source, long value, bool endian = true) { if (!endian) { source.Write(value); return; } var bytes = BitConverter.GetBytes(value); Array.Reverse(bytes, 0, bytes.Length); source.Write(value); }
        public static void WriteE(this BinaryWriter source, float value, bool endian = true) { if (!endian) { source.Write(value); return; } var bytes = BitConverter.GetBytes(value); Array.Reverse(bytes, 0, bytes.Length); source.Write(value); }
        public static void WriteE(this BinaryWriter source, ushort value, bool endian = true) { if (!endian) { source.Write(value); return; } var bytes = BitConverter.GetBytes(value); Array.Reverse(bytes, 0, bytes.Length); source.Write(value); }
        public static void WriteE(this BinaryWriter source, uint value, bool endian = true) { if (!endian) { source.Write(value); return; } var bytes = BitConverter.GetBytes(value); Array.Reverse(bytes, 0, bytes.Length); source.Write(value); }
        public static void WriteE(this BinaryWriter source, ulong value, bool endian = true) { if (!endian) { source.Write(value); return; } var bytes = BitConverter.GetBytes(value); Array.Reverse(bytes, 0, bytes.Length); source.Write(value); }

        #endregion

        #region Position

        public static void WriteAlign(this BinaryWriter source, int align = 4) { }
        public static long Position(this BinaryWriter source) => source.BaseStream.Position;

        #endregion

        #region Bytes

        //public static void WriteBytes(this BinaryWriter source, byte[] value) => source.Write(value, 0, value.Length);

        #endregion

        #region Other

        public static void Write32(this BinaryWriter source, bool value) => source.Write(value ? 1 : 0);
        public static void Write(this BinaryWriter source, Guid value) => source.Write(value.ToByteArray());
        public static void WriteT<T>(this BinaryWriter source, T value, int length) => throw new NotImplementedException(); // source.Write(UnsafeX.MarshalF(value, length));

        public static void WriteCompressed(this BinaryWriter source, uint value)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region String

        public static void WriteZ(this BinaryWriter source, string value, char endChar = '\0')
        {
            throw new NotImplementedException();
        }

        public static void WriteZASCII(this BinaryWriter source, string value, int length = int.MaxValue)
        {
            source.Write(Encoding.ASCII.GetBytes(value));
            source.Write((byte)0);
        }

        #endregion

        #region Array

        public static void WriteL8Array<T>(this BinaryWriter source, T[] value, int sizeOf) where T : struct { source.Write((byte)value.Length); WriteTArray(source, value, sizeOf); }
        public static void WriteL8Array<T>(this BinaryWriter source, T[] value, Action<BinaryWriter, T> factory) { source.Write((byte)value.Length); WriteTArray(source, value, factory); }

        public static void WriteL16Array<T>(this BinaryWriter source, T[] value, int sizeOf) where T : struct { source.Write((byte)value.Length); WriteTArray(source, value, sizeOf); }
        public static void WriteL16Array<T>(this BinaryWriter source, T[] value, Action<BinaryWriter, T> factory) { source.Write((byte)value.Length); WriteTArray(source, value, factory); }
        public static void WriteL32Array<T>(this BinaryWriter source, T[] value, int sizeOf) where T : struct { source.Write((byte)value.Length); WriteTArray(source, value, sizeOf); }
        public static void WriteL32Array<T>(this BinaryWriter source, T[] value, Action<BinaryWriter, T> factory) { source.Write((byte)value.Length); WriteTArray(source, value, factory); }
        public static void WriteC32Array<T>(this BinaryWriter source, T[] value, int sizeOf) where T : struct { source.WriteCompressed((uint)value.Length); WriteTArray(source, value, sizeOf); }
        public static void WriteC32Array<T>(this BinaryWriter source, T[] value, Action<BinaryWriter, T> factory) { source.WriteCompressed((uint)value.Length); WriteTArray(source, value, factory); }
        public static void WriteTArray<T>(this BinaryWriter source, T[] value, int sizeOf) where T : struct { if (value.Length == 0) return; var bytes = UnsafeX.MarshalTArray<T>(value, sizeOf); source.Write(bytes); }
        public static void WriteTArray<T>(this BinaryWriter source, T[] value, Action<BinaryWriter, T> factory) { for (var i = 0; i < value.Length; i++) factory(source, value[i]); }

        public static void WriteL16EArray<T>(this BinaryWriter source, T[] value, int sizeOf, bool endian = true) where T : struct { source.WriteE((ushort)value.Length, endian); WriteTEArray(source, value, sizeOf, endian); }
        public static void WriteL16EArray<T>(this BinaryWriter source, T[] value, Action<BinaryWriter, bool, T> factory, bool endian = true) { source.WriteE((ushort)value.Length, endian); WriteTEArray(source, value, factory, endian); }
        public static void WriteL32EArray<T>(this BinaryWriter source, T[] value, int sizeOf, bool endian = true) where T : struct { source.WriteE((uint)value.Length, endian); WriteTEArray(source, value, sizeOf, endian); }
        public static void WriteL32EArray<T>(this BinaryWriter source, T[] value, Action<BinaryWriter, bool, T> factory, bool endian = true) { source.WriteE((uint)value.Length, endian); WriteTEArray(source, value, factory, endian); }
        public static void WriteTEArray<T>(this BinaryWriter source, T[] value, int sizeOf, bool endian = true) where T : struct { if (value.Length == 0) return; var bytes = UnsafeX.MarshalTArray(value, sizeOf); source.WriteE(bytes, sizeOf, endian); }
        public static void WriteTEArray<T>(this BinaryWriter source, T[] value, Action<BinaryWriter, bool, T> factory, bool endian = true) { if (value.Length > 0) for (var i = 0; i < value.Length; i++) factory(source, endian, value[i]); }

        #endregion
    }
}