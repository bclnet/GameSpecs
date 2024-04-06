using Compression;
using Compression.Doboz;
using ICSharpCode.SharpZipLib.Zip.Compression;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using K4os.Compression.LZ4;
using lzo.net;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using ZstdNet;
using Decoder = SevenZip.Compression.LZMA.Decoder;

namespace GameX.Formats
{
    public static class Compression
    {
        const int BufferSize = 4096 * 10;
        public const int LZMAPropsSize = 5;

        #region Doboz
        public static byte[] DecompressDoboz(this BinaryReader r, int length, int newLength)
        {
            var fileData = r.ReadBytes(length);
            return DobozDecoder.Decode(fileData, 0, fileData.Length);
        }
        public static int DecompressDoboz(byte[] source, int sourceSize, byte[] target, int targetSize)
            => DobozDecoder.Decode(source, 0, sourceSize, target, 0, targetSize);
        #endregion

        #region Lz4
        public static byte[] DecompressLz4(this BinaryReader r, int length, int newLength)
        {
            var fileData = r.ReadBytes(length);
            var newFileData = new byte[newLength];
            LZ4Codec.Decode(fileData, newFileData);
            return newFileData;
        }
        public static int DecompressLz4(byte[] source, byte[] target) => LZ4Codec.Decode(source, 0, source.Length, target, 0, target.Length);
        #endregion

        #region Lzo
        public static byte[] DecompressLzo(this BinaryReader r, int length, int newLength)
        {
            var fs = new LzoStream(r.BaseStream, CompressionMode.Decompress);
            return fs.ReadBytes(newLength);
        }
        public static int DecompressLzo(byte[] source, byte[] target)
        {
            using var fs = new LzoStream(new MemoryStream(source), CompressionMode.Decompress);
            var r = fs.ReadBytes(target.Length);
            Buffer.BlockCopy(r, 0, target, 0, r.Length);
            return r.Length;
        }
        #endregion

        #region Lzf
        public static int DecompressLzf(this BinaryReader r, int length, byte[] buffer)
        {
            var fileData = r.ReadBytes(length);
            return Lzf.Decompress(fileData, buffer);
        }
        public static int DecompressLzf(byte[] source, byte[] target) => Lzf.Decompress(source, target);
        //public static byte[] DecompressLzm(this BinaryReader r, int length, int newLength)
        //{
        //    var data = r.ReadBytes(length);
        //    var inflater = new Inflater();
        //    inflater.SetInput(data, 0, data.Length);
        //    int count;
        //    var buffer = new byte[BufferSize];
        //    using (var s = new MemoryStream())
        //    {
        //        while ((count = Inflater.Inflate(buffer)) > 0) s.Write(buffer, 0, count);
        //        s.Position = 0;
        //        return s.ToArray();
        //    }
        //}
        #endregion

        #region Oodle
        public static byte[] DecompressOodle(this BinaryReader r, int length, int newLength)
        {
            var oodleCompression = r.ReadBytes(4);
            if (!(oodleCompression.SequenceEqual(new byte[] { 0x4b, 0x41, 0x52, 0x4b }))) throw new NotImplementedException();
            var size = r.ReadUInt32();
            if (size != newLength) throw new FormatException();
            var fileData = r.ReadBytes(length - 8);
            var newFileData = new byte[newLength];
            var unpackedSize = OodleLZ.Decompress(fileData, newFileData);
            if (unpackedSize != newLength) throw new FormatException($"Unpacked size does not match real size. {unpackedSize} vs {newLength}");
            return newFileData;
        }
        public static int DecompressOodle(byte[] source, byte[] target) => OodleLZ.Decompress(source, target);
        #endregion

        #region Snappy
        public static byte[] DecompressSnappy(this BinaryReader r, int length, int newLength) => throw new NotSupportedException();
        public static int DecompressSnappy(byte[] source, byte[] target) => throw new NotSupportedException();
        #endregion

        #region Zstd
        public static byte[] DecompressZstd(this BinaryReader r, int length, int newLength)
        {
            using var fs = new DecompressionStream(r.BaseStream);
            return fs.ReadBytes(newLength);
        }
        public static int DecompressZstd(byte[] source, byte[] target)
        {
            using var fs = new DecompressionStream(new MemoryStream(source));
            var r = fs.ReadBytes(target.Length);
            Buffer.BlockCopy(r, 0, target, 0, r.Length);
            return r.Length;
        }
        #endregion

        #region Zlib
        public static byte[] DecompressZlib(this BinaryReader r, int length, int newLength, bool noHeader = false)
        {
            var fileData = r.ReadBytes(length);
            var inflater = new Inflater(noHeader);
            inflater.SetInput(fileData, 0, fileData.Length);
            int count;
            var buffer = new byte[BufferSize];
            using var s = new MemoryStream();
            while ((count = inflater.Inflate(buffer)) > 0) s.Write(buffer, 0, count);
            return s.ToArray();
        }
        public static int DecompressZlib(byte[] source, byte[] target)
        {
            var inflater = new Inflater(false);
            inflater.SetInput(source, 0, source.Length);
            return inflater.Inflate(target, 0, target.Length);
        }
        public static byte[] DecompressZlib2(this BinaryReader r, int length, int newLength)
        {
            var fileData = r.ReadBytes(length);
            using var s = new InflaterInputStream(new MemoryStream(fileData), new Inflater(false), 4096);
            var newFileData = new byte[newLength];
            s.Read(newFileData, 0, newFileData.Length);
            return newFileData;
        }
        public static byte[] CompressZlib(byte[] source, int length)
        {
            var deflater = new Deflater(Deflater.BEST_COMPRESSION);
            deflater.SetInput(source, 0, length);
            int count;
            var buffer = new byte[BufferSize];
            using var s = new MemoryStream();
            while ((count = deflater.Deflate(buffer)) > 0) s.Write(buffer, 0, count);
            return s.ToArray();
        }
        #endregion

        #region Zlib2
        //public static ulong DecompressZlib_2(this Stream input, int length, Stream output)
        //{
        //    var written = 0UL;
        //    var data = new byte[length];
        //    input.Read(data, 0, data.Length);
        //    using (var ms = new MemoryStream(data, false))
        //    using (var lz4Stream = LZ4Decoder.Create(ms, LZ4StreamMode.Read))
        //    {
        //        var buffer = new byte[BufferSize];
        //        int count;
        //        while ((count = lz4Stream.Read(buffer, 0, buffer.Length)) > 0) { output.Write(buffer, 0, count); written += (ulong)count; }
        //    }
        //}
        #endregion

        #region Lzma
        public static byte[] DecompressLzma(this BinaryReader r, int length, int newLength)
        {
            var decoder = new Decoder();
            decoder.SetDecoderProperties(r.ReadBytes(5));
            var fileData = r.ReadBytes(length);
            using var s = new MemoryStream(fileData);
            using var os = new MemoryStream(newLength);
            decoder.Code(s, os, fileData.Length, newLength, null);
            return os.ToArray();
        }
        public static int DecompressLzma(byte[] source, byte[] target) => throw new NotImplementedException();
        #endregion

        #region Blast
        public static byte[] DecompressBlast(this BinaryReader r, int length, int newLength)
        {
            var decoder = new Blast();
            var fs = r.ReadBytes(length);
            //var os = new byte[newLength];
            using var os = new MemoryStream(newLength);
            decoder.Decompress(fs, os);
            return os.ToArray();
        }
        public static int DecompressBlast(byte[] source, byte[] target) => throw new NotImplementedException();
        #endregion

        #region Lzss
        public static byte[] DecompressLzss(this BinaryReader r, int length, int newLength)
        {
            using var stream = new Lzss.BinaryReaderE(new MemoryStream(r.ReadBytes(length)));
            var numArray = new Lzss(stream, newLength).Decompress();
            return numArray;
        }
        public static int DecompressLzss(byte[] source, byte[] target) => throw new NotImplementedException();
        #endregion
    }
}