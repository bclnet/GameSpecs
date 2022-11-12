using GameSpec.Formats;
using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using static OpenStack.Debug;

namespace GameSpec.IW.Formats
{
    // https://forum.xentax.com/viewtopic.php?t=12195
    // https://github.com/Scobalula/Greyhound/tree/master/src/WraithXCOD/WraithXCOD
    // https://github.com/Sohliramon/VulTure-CoD-CW-Trainer
    // https://github.com/XLabsProject/img-format-helper
    // https://github.com/DentonW/DevIL/blob/master/DevIL/src-IL/src/il_iwi.cpp
    // https://github.com/orgs/XLabsProject/repositories
    public unsafe class PakBinaryIW : PakBinary
    {
        public static readonly PakBinary Instance = new PakBinaryIW();
        PakBinaryIW() { }

        enum Magic
        {
            IWD,
            FF,
            PAK,
            XPAK,
            XSUB,
        }

        // Headers : IPAK (Black Ops 2)
        #region Headers : IPAK

        const uint IPAK_MAGIC = 0x12345678;

        [StructLayout(LayoutKind.Sequential, Pack = 0x1)]
        struct IPAK_Header
        {
            public uint Magic;
            public uint Version;
            public uint Size;
            public uint SegmentCount;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 0x1)]
        struct IPAK_Segment
        {
            public uint Type;
            public uint Offset;
            public uint Size;
            public uint EntryCount;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 0x1)]
        struct IPAK_DataHeader
        {
            public uint OffsetCount; // Count and offset are packed into a single integer
            public fixed uint Commands[31]; // The commands tell what each block of data does
            public uint Offset => OffsetCount << 8;
            public byte Count => (byte)(OffsetCount & 0xFF);
        }

        [StructLayout(LayoutKind.Sequential, Pack = 0x1)]
        struct IPAK_Entry
        {
            public ulong Key;
            public uint Offset;
            public uint Size;
        }

        #endregion

        // Headers : XPAK (Black Ops 3)
        #region Headers : XPAK

        const uint XPAK_MAGIC = 0x4950414b;

        [StructLayout(LayoutKind.Sequential, Pack = 0x1)]
        struct XPAK_Header
        {
            public uint Magic;
            public ushort Unknown1;
            public ushort Version;
            public ulong Unknown2;
            public ulong Size;
            public ulong FileCount;
            public ulong DataOffset;
            public ulong DataSize;
            public ulong HashCount;
            public ulong HashOffset;
            public ulong HashSize;
            public ulong Unknown3;
            public ulong UnknownOffset;
            public ulong Unknown4;
            public ulong IndexCount;
            public ulong IndexOffset;
            public ulong IndexSize;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 0x1)]
        struct XPAK_HashEntry
        {
            public ulong Key;
            public ulong Offset;
            public ulong Size;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 0x1)]
        struct XPAK_DataHeader
        {
            public uint Offset;
            public uint Count;
            public fixed uint Commands[31]; // The commands tell what each block of data does
        }

        class XSUB_PakFile : BinaryPakManyFile
        {
            public XSUB_PakFile(Family family, string game, string filePath, object tag = null) : base(family, game, filePath, Instance, tag) { Open(); }
        }

        #endregion

        // Headers : WWII (WWII)
        #region Headers : WWII

        const uint WWII_MAGIC = 0x12345678;

        [StructLayout(LayoutKind.Sequential, Pack = 0x1)]
        struct WWII_Header
        {
            public ulong Magic;
            public uint Version;
            public uint EntriesCount;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 0x1)]
        struct WWII_Segment
        {
            public fixed byte Hash[16];
            public ulong Offset;
            public uint Size;
            public ushort PackageIndex;
        }

        #endregion

        public unsafe override Task ReadAsync(BinaryPakFile source, BinaryReader r, ReadStage stage)
        {
            if (!(source is BinaryPakManyFile multiSource)) throw new NotSupportedException();
            if (stage != ReadStage.File) throw new ArgumentOutOfRangeException(nameof(stage), stage.ToString());

            var extension = Path.GetExtension(source.FilePath);
            var files = multiSource.Files = new List<FileMetadata>();

            switch (extension)
            {
                // IWD
                case ".iwd":
                    source.UseBinaryReader = false;
                    source.Magic = (int)Magic.IWD;

                    var pak = (ZipFile)(source.Tag = new ZipFile(r.BaseStream));
                    foreach (ZipEntry entry in pak)
                    {
                        if (entry.Size == 0) continue;
                        files.Add(new FileMetadata
                        {
                            Path = entry.Name.Replace('\\', '/'),
                            Crypted = entry.IsCrypted,
                            PackedSize = entry.CompressedSize,
                            FileSize = entry.Size,
                            Tag = entry,
                        });
                    }
                    return Task.CompletedTask;
                // FF
                case ".ff":
                    {
                        source.Magic = (int)Magic.FF;
                    }
                    return Task.CompletedTask;
                // PAK
                case ".pak":
                    {
                        source.Magic = (int)Magic.PAK;
                    }
                    return Task.CompletedTask;
                // XPAK
                case ".xpak":
                    {
                        source.Magic = (int)Magic.XPAK;
                        var header = r.ReadT<XPAK_Header>(sizeof(XPAK_Header));

                        // If MW4 we need to skip the new bytes
                        if (header.Version == 0xD)
                        {
                            throw new NotImplementedException();
                            //r.Seek(0);
                            //uint64_t Result;
                            //r.Read((uint8_t*)&Header, 24, Result);
                            //r.Advance(288);
                            //r.Read((uint8_t*)&Header + 24, 96, Result);
                        }

                        // Verify the magic and offset
                        if (header.Magic != XPAK_MAGIC || header.HashOffset >= (ulong)r.BaseStream.Length) throw new FormatException("Bad magic");

                        // Jump to hash offset
                        r.Seek((long)header.HashOffset);

                        var hashData = r.ReadTArray<XPAK_HashEntry>(sizeof(XPAK_HashEntry), (int)header.HashCount);
                        for (var i = 0; i < (int)header.HashCount; i++)
                        {
                            // Read it
                            ref XPAK_HashEntry entry = ref hashData[i];
                            files.Add(new FileMetadata
                            {
                                Id = (int)entry.Key,
                                Path = entry.Key.ToString(),
                                Position = (long)(header.DataOffset + entry.Offset),
                                PackedSize = (long)(entry.Size & 0xFFFFFFFFFFFFFF), // 0x80 in last 8 bits in some entries in new XPAKs
                                FileSize = 0,
                                Tag = entry,
                            });
                        }
                    }
                    return Task.CompletedTask;
                default:
                    return Task.CompletedTask;
            }
        }

        public unsafe override Task<Stream> ReadDataAsync(BinaryPakFile source, BinaryReader r, FileMetadata file, DataOption option = 0, Action<FileMetadata, string> exception = null)
        {
            switch ((Magic)source.Magic)
            {
                case Magic.IWD:
                    var pak = (ZipFile)source.Tag;
                    var entry = (ZipEntry)file.Tag;
                    try
                    {
                        using var input = pak.GetInputStream(entry);
                        if (!input.CanRead) { Log($"Unable to read stream for file: {file.Path}"); exception?.Invoke(file, $"Unable to read stream for file: {file.Path}"); return Task.FromResult(System.IO.Stream.Null); }
                        var s = new MemoryStream();
                        input.CopyTo(s);
                        s.Position = 0;
                        return Task.FromResult((Stream)s);
                    }
                    catch (Exception e) { Log($"{file.Path} - Exception: {e.Message}"); exception?.Invoke(file, $"{file.Path} - Exception: {e.Message}"); return Task.FromResult(System.IO.Stream.Null); }
                default: throw new NotImplementedException();
            }
        }
    }
}