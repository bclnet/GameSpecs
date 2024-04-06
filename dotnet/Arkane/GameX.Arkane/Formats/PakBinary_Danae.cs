using GameX.Formats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace GameX.Arkane.Formats
{
    public unsafe class PakBinary_Danae : PakBinary<PakBinary_Danae>
    {
        public override Task Read(BinaryPakFile source, BinaryReader r, object tag)
        {
            var files = source.Files = new List<FileSource>();
            var key = Encoding.ASCII.GetBytes((string)source.Game.Key); int keyLength = key.Length, keyIndex = 0;

            int readInt32(ref byte* b)
            {
                var p = b;
                *(p + 0) = (byte)(*(p + 0) ^ key[keyIndex++]); if (keyIndex >= keyLength) keyIndex = 0;
                *(p + 1) = (byte)(*(p + 1) ^ key[keyIndex++]); if (keyIndex >= keyLength) keyIndex = 0;
                *(p + 2) = (byte)(*(p + 2) ^ key[keyIndex++]); if (keyIndex >= keyLength) keyIndex = 0;
                *(p + 3) = (byte)(*(p + 3) ^ key[keyIndex++]); if (keyIndex >= keyLength) keyIndex = 0;
                b += 4;
                return *(int*)p;
            }

            string readString(ref byte* b)
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
                    var dirPath = readString(ref c).Replace('\\', '/');
                    var numFiles = readInt32(ref c);
                    for (var i = 0; i < numFiles; i++)
                    {
                        var file = new FileSource
                        {
                            Path = dirPath + readString(ref c).Replace('\\', '/'),
                            Offset = readInt32(ref c),
                            Compressed = readInt32(ref c),
                            FileSize = readInt32(ref c),
                            PackedSize = readInt32(ref c),
                        };
                        if (file.Path.EndsWith(".FTL")) file.Compressed = 1;
                        else if (file.Compressed == 0) file.FileSize = file.PackedSize;
                        files.Add(file);
                    }
                }
            }
            return Task.CompletedTask;
        }

        public override Task<Stream> ReadData(BinaryPakFile source, BinaryReader r, FileSource file, FileOption option = default)
        {
            r.Seek(file.Offset);
            return Task.FromResult((Stream)new MemoryStream((file.Compressed & 1) != 0
                ? r.DecompressBlast((int)file.PackedSize, (int)file.FileSize)
                : r.ReadBytes((int)file.PackedSize)));
        }
    }
}