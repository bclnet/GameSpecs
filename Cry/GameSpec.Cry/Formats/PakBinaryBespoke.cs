using GameSpec.Formats;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace GameSpec.Cry.Formats
{
    public class PakBinaryBespoke : PakBinary
    {
        public static readonly PakBinary Instance = new PakBinaryBespoke();
        readonly byte[] Key;

        public PakBinaryBespoke(byte[] key = null) => Key = key;

        // Header
        #region Archeage

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

        public unsafe override Task ReadAsync(BinaryPakFile source, BinaryReader r, ReadStage stage)
        {
            if (!(source is BinaryPakManyFile multiSource)) throw new NotSupportedException();
            if (stage != ReadStage.File) throw new ArgumentOutOfRangeException(nameof(stage), stage.ToString());
            FileMetadata[] files;

            switch (source.Game.Id)
            {
                case "ArcheAge":
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
                        multiSource.Files = files = new FileMetadata[header.FileCount];
                        for (var i = 0; i < header.FileCount; i++)
                        {
                            stream.Seek(infoOffset, SeekOrigin.Begin);
                            r = new BinaryReader(new CryptoStream(stream, aes.CreateDecryptor(), CryptoStreamMode.Read));
                            var nameAsSpan = r.ReadBytes(0x108).AsSpan();
                            files[fileIdx++] = new FileMetadata
                            {
                                //.Replace('\\', '/')
                                Path = Encoding.ASCII.GetString(nameAsSpan[..nameAsSpan.IndexOf(byte.MinValue)]), //: name
                                Position = r.ReadInt64(),   //: offset
                                FileSize = r.ReadInt64(),   //: size
                                PackedSize = r.ReadInt64(), //: xsize
                                Compressed = r.ReadInt32(), //: ysize
                            };
                            infoOffset += 0x150;
                        }
                    }
                    break;
                default: throw new InvalidOperationException("BAD MAGIC");
            }
            return Task.CompletedTask;
        }

        public unsafe override Task WriteAsync(BinaryPakFile source, BinaryWriter w, WriteStage stage)
            => throw new NotImplementedException();

        public unsafe override Task<Stream> ReadDataAsync(BinaryPakFile source, BinaryReader r, FileMetadata file, DataOption option = 0, Action<FileMetadata, string> exception = null)
        {
            Stream fileData = null;
            var magic = source.Magic;
            // AA
            if (magic == AA_MAGIC)
            {
                // position
                r.Position(file.Position);
                fileData = new MemoryStream(r.ReadBytes((int)file.FileSize));
            }
            else throw new InvalidOperationException("BAD MAGIC");
            return Task.FromResult(fileData);
        }
    }
}