using GameX.Formats;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace GameX.Crytek.Formats
{
    public class PakBinary_ArcheAge : PakBinary
    {
        readonly byte[] Key;

        public PakBinary_ArcheAge(byte[] key) => Key = key;

        // Header
        #region Header

        const uint AA_MAGIC = 0x4f424957; // Magic for Archeage, the literal string "WIBO".

        [StructLayout(LayoutKind.Sequential, Pack = 0x1)]
        struct AA_Header
        {
            public uint Magic;
            public uint Dummy1;
            public uint FileCount;
            public uint ExtraFiles;
            public uint Dummy2;
            public uint Dummy3;
            public uint Dummy4;
            public uint Dummy5;
        }

        #endregion

        public unsafe override Task Read(BinaryPakFile source, BinaryReader r, object tag)
        {
            FileSource[] files;

            var stream = r.BaseStream;
            using (var aes = Aes.Create())
            {
                aes.Key = Key;
                aes.IV = new byte[16];
                aes.Mode = CipherMode.CBC;
                r = new BinaryReader(new CryptoStream(stream, aes.CreateDecryptor(), CryptoStreamMode.Read));
                stream.Seek(stream.Length - 0x200, SeekOrigin.Begin);

                var header = r.ReadT<AA_Header>(sizeof(AA_Header));
                if (header.Magic != AA_MAGIC) throw new FormatException("BAD MAGIC");
                source.Magic = header.Magic;

                var totalSize = (header.FileCount + header.ExtraFiles) * 0x150;
                var infoOffset = stream.Length - 0x200;
                infoOffset -= totalSize;
                while (infoOffset >= 0)
                {
                    if ((infoOffset % 0x200) != 0) infoOffset -= 0x10;
                    else break;
                }

                // read-all files
                var fileIdx = 0U;
                source.Files = files = new FileSource[header.FileCount];
                for (var i = 0; i < header.FileCount; i++)
                {
                    stream.Seek(infoOffset, SeekOrigin.Begin);
                    r = new BinaryReader(new CryptoStream(stream, aes.CreateDecryptor(), CryptoStreamMode.Read));
                    var nameAsSpan = r.ReadBytes(0x108).AsSpan();
                    files[fileIdx++] = new FileSource
                    {
                        //.Replace('\\', '/')
                        Path = Encoding.ASCII.GetString(nameAsSpan[..nameAsSpan.IndexOf(byte.MinValue)]), //: name
                        Offset = r.ReadInt64(),   //: offset
                        FileSize = r.ReadInt64(),   //: size
                        PackedSize = r.ReadInt64(), //: xsize
                        Compressed = r.ReadInt32(), //: ysize
                    };
                    infoOffset += 0x150;
                }
            }
            return Task.CompletedTask;
        }

        public unsafe override Task<Stream> ReadData(BinaryPakFile source, BinaryReader r, FileSource file, FileOption option = default)
        {
            // position
            r.Seek(file.Offset);
            Stream fileData = new MemoryStream(r.ReadBytes((int)file.FileSize));
            return Task.FromResult(fileData);
        }
    }
}