using GameSpec.Formats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace GameSpec.Arkane.Formats
{
    public unsafe class PakBinaryDanae : PakBinary
    {
        public static readonly PakBinary Instance = new PakBinaryDanae();

        public override Task ReadAsync(BinaryPakFile source, BinaryReader r, object tag)
        {
            if (!(source is BinaryPakManyFile multiSource)) throw new NotSupportedException();
            var files = multiSource.Files = new List<FileSource>();
            var key = source.Game.Key is Family.ByteKey z ? z.Key : null;
            int keyLength = key.Length, keyIndex = 0;

            int readFatInteger(ref byte* b)
            {
                var p = b;
                *(p + 0) = (byte)(*(p + 0) ^ key[keyIndex++]); if (keyIndex >= keyLength) keyIndex = 0;
                *(p + 1) = (byte)(*(p + 1) ^ key[keyIndex++]); if (keyIndex >= keyLength) keyIndex = 0;
                *(p + 2) = (byte)(*(p + 2) ^ key[keyIndex++]); if (keyIndex >= keyLength) keyIndex = 0;
                *(p + 3) = (byte)(*(p + 3) ^ key[keyIndex++]); if (keyIndex >= keyLength) keyIndex = 0;
                var r = *(int*)p;
                b += 4;
                return r;
            }

            string readFatString(ref byte* b)
            {
                var p = b;
                while (true)
                {
                    *p = (byte)(*p ^ key[keyIndex++]); if (keyIndex >= keyLength) keyIndex = 0;
                    if (*p == 0) break;
                    p++;
                }
                var length = (int)(p - b);
                var r = Encoding.ASCII.GetString(new ReadOnlySpan<byte>(b, length));
                b = p + 1;
                return r;
            }

            // move to fat table
            r.Seek(r.ReadUInt32());
            var fatSize = (int)r.ReadUInt32();
            var fatBytes = r.ReadBytes(fatSize);

            fixed (byte* _ = fatBytes)
            {
                byte* c = _, end = _ + fatSize;
                while (c < end)
                {
                    var dirPath = readFatString(ref c);
                    var numFiles = readFatInteger(ref c);
                    while (numFiles-- != 0)
                    {
                        var f = new FileSource
                        {
                            Path = dirPath + readFatString(ref c),
                            Position = readFatInteger(ref c),
                            Compressed = readFatInteger(ref c),
                            FileSize = readFatInteger(ref c),
                            PackedSize = readFatInteger(ref c),
                        };
                        if (f.Path.EndsWith(".FTL")) f.Compressed = 1;
                        else if (f.Compressed == 0) f.FileSize = f.PackedSize;
                        files.Add(f);
                    }
                }
            }
            return Task.CompletedTask;
        }

        public override Task<Stream> ReadDataAsync(BinaryPakFile source, BinaryReader r, FileSource file, DataOption option = 0, Action<FileSource, string> exception = null)
        {
            r.Seek(file.Position);
            return Task.FromResult((Stream)new MemoryStream((file.Compressed & 1) != 0
                ? r.DecompressBlast((int)file.PackedSize, (int)file.FileSize)
                : r.ReadBytes((int)file.PackedSize)));
        }
    }
}