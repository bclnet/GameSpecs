using CASCLib;
using GameX.Blizzard.Formats.Casc;
using GameX.Formats;
using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using static OpenStack.Debug;
using ZipFile = ICSharpCode.SharpZipLib.Zip.ZipFile;

namespace GameX.IW.Formats
{
    public unsafe class PakBinary_IW : PakBinary<PakBinary_IW>
    {
        CascContext casc;

        //class XSUB_PakFile : BinaryPakFile
        //{
        //    public XSUB_PakFile(FamilyGame game, IFileSystem fileSystem, string filePath, object tag = null) : base(game, fileSystem, filePath, Instance, tag) { Open(); }
        //}

        enum Magic
        {
            CASC,
            IWD,
            FF,
            PAK,
            IPAK,
            XPAK,
            XSUB,
        }

        // Headers : FF
        #region Headers : FF

        internal enum FF_MAGIC : uint
        {
            IWff = 0x66665749, // IWff
            S1ff = 0x66663153, // S1ff
            TAff = 0x66664154, // TAff
        }

        internal enum FF_FORMAT : uint
        {
            U100 = 0x30303175, // u100
            A100 = 0x30303161, // a100
            _100 = 0x30303130, // 0100
            _000 = 0x30303030, // 0000
        }

        internal enum FF_VERSION : uint
        {
            // IW X.0 : 2017 - Call of Duty: WWII
            CO4_WWII = 0x0005,  // IW 3.0 : 2007 - Call of Duty 4: Modern Warfare
            WaW = 0x0183,       // IW 3.0+: 2008 - Call of Duty: World at War
            MW2 = 0x0114,       // IW 4.0 : 2009 - Call of Duty: Modern Warfare 2
            BO = 0x01d9,        // IW 3.0 : 2010 - Call of Duty: Black Ops
            MW3 = 0x0001,       // IW 5.0 : 2011 - Call of Duty: Modern Warfare 3
            BO2 = 0x0093,       // IW 3.0m: 2012 - Call of Duty: Black Ops II
            Ghosts = 0x0235,    // IW X.0 : 2013 - Call of Duty: Ghosts
            AW = 0x072e,        // IW X.0 : 2014 - Call of Duty: Advanced Warfare
            BO3 = 0x0251,       // IW 3.0m: 2015 - Call of Duty: Black Ops III
            IW = 0x0653,        // IW X.0 : 2016 - Call of Duty: Infinite Warfare
            BO4 = 0x0,          // IW X.0 : 2018 - Call of Duty: Black Ops 4
            MW = 0x0042,        // IW X.0 : 2019 - Call of Duty: Modern Warfare
            BOCW = 0x0,         // IW X.0 : 2020 - Call of Duty: Black Ops Cold War
            Vanguard = 0x0,     // IW X.0 : 2021 - Call of Duty: Vanguard
            COD_MW2 = 0x0017,   // IW X.0: Call of Duty: Modern Warfare II
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct FF_Header
        {
            public static (string, int) Struct = ("<?", sizeof(FF_Header));
            [MarshalAs(UnmanagedType.U4)] public FF_MAGIC Magic;
            [MarshalAs(UnmanagedType.U4)] public FF_FORMAT Format;
            [MarshalAs(UnmanagedType.U4)] public FF_VERSION Version;
        }

        #endregion

        // Headers : IPAK (Black Ops 2)
        #region Headers : IPAK

        const uint IPAK_MAGIC = 0x12345678;

        [StructLayout(LayoutKind.Sequential, Pack = 0x1)]
        struct IPAK_Header
        {
            public static (string, int) Struct = ("<?", sizeof(IPAK_Header));
            public uint Magic;
            public uint Version;
            public uint Size;
            public uint SegmentCount;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 0x1)]
        struct IPAK_Segment
        {
            public static (string, int) Struct = ("<?", sizeof(IPAK_Segment));
            public uint Type;
            public uint Offset;
            public uint Size;
            public uint EntryCount;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 0x1)]
        struct IPAK_DataHeader
        {
            public static (string, int) Struct = ("<?", sizeof(IPAK_DataHeader));
            public uint OffsetCount; // Count and offset are packed into a single integer
            public fixed uint Commands[31]; // The commands tell what each block of data does
            public uint Offset => OffsetCount << 8;
            public byte Count => (byte)(OffsetCount & 0xFF);
        }

        [StructLayout(LayoutKind.Sequential, Pack = 0x1)]
        struct IPAK_Entry
        {
            public static (string, int) Struct = ("<?", sizeof(IPAK_Entry));
            public ulong Key;
            public uint Offset;
            public uint Size;
        }

        #endregion

        // Headers : XPAK (Black Ops 3)
        #region Headers : XPAK

        const uint XPAK_MAGIC = 0x4950414b;

        [StructLayout(LayoutKind.Sequential, Pack = 0x1)]
        struct XPAK_Header //: BO3XPakHeader, VGXPAKHeader
        {
            public static (string, int) Struct = ("<?", sizeof(XPAK_Body));
            public uint Magic; // KAPI / IPAK
            public ushort Zero;
            public ushort Version;
            public ulong Unknown2;
            //public ulong Type;
            ///*16*/ public ulong Size;
            //public fixed byte UnknownHashes[1896];
            //public ulong FileCount;     /*24*/
        }

        [StructLayout(LayoutKind.Sequential, Pack = 0x1)]
        struct XPAK_Body //: BO3XPakHeader, VGXPAKHeader
        {
            public static (string, int) Struct = ("<?", sizeof(XPAK_Body));
            public ulong DataOffset;    /*00*/
            public ulong DataSize;      /*08*/
            public ulong HashCount;     /*16*/
            public ulong HashOffset;    /*24*/
            public ulong HashSize;      /*32*/
            public ulong Unknown3;      /*40*/
            public ulong UnknownOffset; /*48*/
            public ulong Unknown4;      /*56*/
            public ulong IndexCount;    /*64*/
            public ulong IndexOffset;   /*72*/
            public ulong IndexSize;     /*80*/
        }

        [StructLayout(LayoutKind.Sequential, Pack = 0x1)]
        struct XPAK_HeaderVG //: VGXPAKHeader
        {
            public static (string, int) Struct = ("<?", sizeof(XPAK_HeaderVG));
            public uint Magic; // KAPI / IPAK
            public ushort Zero;
            public ushort Version;
            public ulong Unknown2;
            public ulong Type;
            public ulong Size;
            public fixed byte UnknownHashes[1896];

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
        struct XPAK_HashEntry //: BO3XPakHashEntry
        {
            public static (string, int) Struct = ("<?", sizeof(XPAK_HashEntry));
            public ulong Key;
            public ulong Offset;
            public ulong Size;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 0x1)]
        struct XPAK_HashEntryVG //: VGXPAKHashEntry
        {
            public static (string, int) Struct = ("<?", sizeof(XPAK_HashEntryVG));
            public ulong Key;
            public ulong PackedInfo;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 0x1)]
        struct XPAK_DataHeader //: BO3XPakDataHeader
        {
            public static (string, int) Struct = ("<?", sizeof(XPAK_DataHeader));
            public uint Offset;
            public uint Count;
            public fixed uint Commands[31]; // The commands tell what each block of data does
        }

        #endregion

        // Headers : WWII (WWII)
        #region Headers : WWII

        const uint WWII_MAGIC = 0x12345678;

        [StructLayout(LayoutKind.Sequential, Pack = 0x1)]
        struct WWII_Header
        {
            public static (string, int) Struct = ("<?", sizeof(WWII_Header));
            public ulong Magic;
            public uint Version;
            public uint EntriesCount;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 0x1)]
        struct WWII_Segment
        {
            public static (string, int) Struct = ("<?", sizeof(WWII_Segment));
            public fixed byte Hash[16];
            public ulong Offset;
            public uint Size;
            public ushort PackageIndex;
        }

        #endregion

        public override Task Read(BinaryPakFile source, BinaryReader r, object tag)
        {
            var files = source.Files = new List<FileSource>();
            var extension = Path.GetExtension(source.PakPath);

            switch (source.Game.Id)
            {
                case "BO4":
                case "BOCW":
                case "Vanguard":
                    source.Magic = (int)Magic.CASC;
                    var editions = source.Game.Editions;
                    var product = editions.First().Key;
                    casc = new CascContext();
                    casc.Read(source.PakPath, product, files);
                    return Task.CompletedTask;
            }

            switch (extension)
            {
                // IWD
                case ".iwd":
                    {
                        source.UseReader = false;
                        source.Magic = (int)Magic.IWD;

                        var pak = (ZipFile)(source.Tag = new ZipFile(r.BaseStream));
                        foreach (ZipEntry entry in pak)
                            if (entry.Size != 0)
                                files.Add(new FileSource
                                {
                                    Path = entry.Name.Replace('\\', '/'),
                                    Crypted = entry.IsCrypted,
                                    PackedSize = entry.CompressedSize,
                                    FileSize = entry.Size,
                                    Tag = entry,
                                });
                        return Task.CompletedTask;
                    }
                // FF
                case ".ff":
                    {
                        source.Magic = (int)Magic.FF;

                        var header = r.ReadS<FF_Header>();
                        if (header.Magic != FF_MAGIC.IWff && header.Magic != FF_MAGIC.S1ff && header.Magic != FF_MAGIC.TAff) throw new FormatException($"Bad magic {header.Magic}");
                        if (header.Format != FF_FORMAT.U100 && header.Format != FF_FORMAT._100 && header.Format != FF_FORMAT.A100 && header.Format != FF_FORMAT._000) throw new FormatException($"Bad format {header.Format}");

                        return Task.CompletedTask;
                        //var assets = FastFile.GetAssets(source, r, cryptKey, ref header);
                        //if (assets != null)
                        //    foreach (var asset in assets)
                        //        files.Add(new FileMetadata
                        //        {
                        //            Id = asset.Id,
                        //            Path = asset.Path,
                        //            Position = asset.Position,
                        //            FileSize = asset.FileSize,
                        //        });

                        //return Task.CompletedTask;
                    }
                // PAK
                case ".pak":
                    {
                        source.Magic = (int)Magic.PAK;
                        return Task.CompletedTask;
                    }
                // IPAK
                case ".ipak":
                    {
                        source.Magic = (int)Magic.IPAK;
                        return Task.CompletedTask;
                    }
                // XPAK
                case ".xpak":
                    {
                        source.Magic = (int)Magic.XPAK;
                        var header = r.ReadS<XPAK_Header>();
                        // Verify the magic and offset
                        if (header.Magic != XPAK_MAGIC) throw new FormatException("Bad magic");
                        var type = header.Version == 0x1 ? r.ReadUInt64() : 0;
                        var size = r.ReadUInt64();
                        var unknownHashes = header.Version == 0x1 ? r.ReadBytes(1896) : null;
                        var fileCount = r.ReadUInt64();
                        if (header.Version == 0xD) r.Skip(288); // If MW4 we need to skip the new bytes
                        var body = r.ReadS<XPAK_Body>();
                        // Verify the magic and offset
                        if (body.HashOffset >= (ulong)r.BaseStream.Length) throw new FormatException("Bad magic");

                        // Jump to hash offset
                        r.Seek((long)body.IndexOffset);
                        //var indexHeader = r.ReadBytes(16); //<XPAK_DataHeader>(sizeof(XPAK_HashEntry), (int)header.HashCount);
                        //var abc = r.ReadCString(); //<XPAK_DataHeader>(sizeof(XPAK_HashEntry), (int)header.HashCount);

                        // Read hash entries
                        r.Seek((long)body.HashOffset);
                        var entries = r.ReadTArray<XPAK_HashEntry>(sizeof(XPAK_HashEntry), (int)body.HashCount);
                        for (var i = 0; i < (int)body.HashCount; i++)
                        {
                            // Read it
                            ref XPAK_HashEntry entry = ref entries[i];
                            files.Add(new FileSource
                            {
                                Id = (int)entry.Key,
                                Path = entry.Key.ToString(),
                                Offset = (long)(body.DataOffset + entry.Offset), //: Offset
                                PackedSize = (long)(entry.Size & 0xFFFFFFFFFFFFFF), //: CompressedSize, 0x80 in last 8 bits in some entries in new XPAKs
                                FileSize = 0, //: UncompressedSize
                                Tag = entry,
                            });
                        }
                        return Task.CompletedTask;
                    }
                default: return Task.CompletedTask;
            }
        }

        public override Task<Stream> ReadData(BinaryPakFile source, BinaryReader r, FileSource file, FileOption option = default)
        {
            switch ((Magic)source.Magic)
            {
                case Magic.CASC:
                    return Task.FromResult(casc.ReadData(file));
                case Magic.IWD:
                    var pak = (ZipFile)source.Tag;
                    var entry = (ZipEntry)file.Tag;
                    try
                    {
                        using var input = pak.GetInputStream(entry);
                        if (!input.CanRead) { HandleException(file, option, $"Unable to read stream for file: {file.Path}"); return Task.FromResult(System.IO.Stream.Null); }
                        var s = new MemoryStream();
                        input.CopyTo(s);
                        s.Position = 0;
                        return Task.FromResult((Stream)s);
                    }
                    catch (Exception e) { HandleException(file, option, $"{file.Path} - Exception: {e.Message}"); return Task.FromResult(System.IO.Stream.Null); }
                case Magic.FF:
                    {
                        var s = new MemoryStream();
                        s.Position = 0;
                        return Task.FromResult((Stream)s);
                    }
                default: throw new NotImplementedException();
            }
        }
    }
}