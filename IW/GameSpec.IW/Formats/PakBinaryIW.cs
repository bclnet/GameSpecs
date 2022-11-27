using Compression;
using GameSpec.Formats;
using GameSpec.IW.Zone;
using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Threading.Tasks;
using ZstdNet;
using static OpenStack.Debug;
using ZipFile = ICSharpCode.SharpZipLib.Zip.ZipFile;

namespace GameSpec.IW.Formats
{
    // https://forum.xentax.com/viewtopic.php?t=12195 - COD:AW
    // https://github.com/Scobalula/Greyhound/tree/master/src/WraithXCOD/WraithXCOD - Hooking
    // https://github.com/orgs/XLabsProject/repositories
    // http://tom-crowley.co.uk/downloads/
    // https://wiki.zeroy.com/index.php?title=Call_of_Duty_4:_Skinning
    // https://github.com/RagdollPhysics/zonebuilder - Hooking
    // https://gist.github.com/Scobalula/a0fd08197497336f67b7ff551b2db404 - S1ff 0x42|0x72e 
    // https://github.com/SE2Dev/PyCoD - read binaries
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

        // Headers : FF
        #region Headers : FF

        const uint FF_MAGIC_IW = 0x66665749; // IWff
        const uint FF_MAGIC_S1 = 0x66663153; // S1ff
        const uint FF_MAGIC_TA = 0x66664154; // TAff

        const uint FF_FORMAT_u100 = 0x30303175; // u100
        const uint FF_FORMAT_a100 = 0x30303161; // a100
        const uint FF_FORMAT_0100 = 0x30303130; // 0100
        const uint FF_FORMAT_0000 = 0x30303030; // 0000

        [StructLayout(LayoutKind.Explicit)]
        struct FF_Header
        {
            [FieldOffset(0)] public uint Magic; // IWff
            [FieldOffset(4)] public uint Format; // u100
            [FieldOffset(8)] public uint Version;
            //[FieldOffset(12)] public fixed byte Unknown3[12];
            // BO2
            //[FieldOffset(12)] public byte BO2_Unknown2;
            //[FieldOffset(13)] public uint BO2_dwHighDateTime;
            //[FieldOffset(17)] public uint BO2_dwLowDateTime;
            //[FieldOffset(21)] public fixed byte Unknown3[3];
            // BO3
            [FieldOffset(12)] public byte BO3_Unknown;
            [FieldOffset(13)] public byte BO3_FlagsZLIB;
            [FieldOffset(14)] public byte BO3_FlagsPC;
            [FieldOffset(15)] public byte BO3_FlagsEncrypted;
            [FieldOffset(20)] public uint End;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct FF_BO3BlockHeader
        {
            public int CompressedSize;
            public int DecompressedSize;
            public int Size;
            public int Position;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct FF_Asset32
        {
            public uint namePtr;
            public int size;
            public uint dataPtr;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct FF_Asset64
        {
            public ulong namePtr;
            public long size;
            public ulong dataPtr;
        }

        [StructLayout(LayoutKind.Explicit)]
        struct FF_ZoneHeader
        {
            [FieldOffset(0)] public uint Size;
            //
            [FieldOffset(44)] public ushort CO_Args;
            [FieldOffset(52)] public int CO_Assets;
            //
            [FieldOffset(36)] public ushort BO_Args;
            [FieldOffset(44)] public int BO_Assets;
            //
            [FieldOffset(36)] public int AW_Args;
            [FieldOffset(44)] public int AW_Assets;
            //
            [FieldOffset(40)] public int BO2_Args;
            [FieldOffset(9)] public int BO2_Assets;
            //
            [FieldOffset(0)] public int BO3_Args;
            [FieldOffset(32)] public int BO3_Assets;

            (int seek, int argCount, int assetCount, int endSkip) GetArgsAndAssetsOffsets(uint version)
                => version switch
                {
                    0x5 => (0x3C, CO_Args, CO_Assets, 0),       // Call of Duty 4: Modern Warfare
                    0x1d9 => (0x34, BO_Args, BO_Assets, 0),     // Black Ops Fast File
                    0x93 => (0x40, BO2_Args, BO2_Assets, 0),    // Black Ops II Fast File
                    0x183 => (0x34, AW_Args, AW_Assets, -2),    // Call of Duty: Advanced Warfare
                    0x251 => (0x40, BO3_Args, BO3_Assets, 0),   // Black Ops III Fast File
                    _ => throw new FormatException($"Unknown Version: {version}"),
                };

            public (Dictionary<string, long> args, ASSET_TYPE[] assetInfos) GetArgsAndAssetInfos(BinaryReader r, ref FF_Header header)
            {
                var version = header.Version;
                var (seek, argCount, assetCount, endSkip) = GetArgsAndAssetsOffsets(version);
                var args = new Dictionary<string, long>();
                ASSET_TYPE[] assetInfos = null;
                r.Seek(seek);
                if (version >= 0x251)
                {
                    if (argCount > 0)
                    {
                        var argsValues = r.ReadTArray<long>(sizeof(long), argCount);
                        var argsNames = r.ReadTArray(r => r.ReadCString(), argCount);
                        if (argsNames[argCount - 1] == "\u0005") { argCount--; r.Skip(-3); }
                        for (var i = 0; i < argCount; i++) args[argsNames[i] ?? $"${i}"] = argsValues[i];
                    }
                    if (assetCount > 0)
                    {
                        var assetType = r.ReadTArray<long>(sizeof(long), assetCount * 2);
                        //assetInfos = assetType.Select(x => (ASSET_TYPE)(x << 32)).ToArray();
                    }
                }
                else
                {
                    if (argCount > 0)
                    {
                        var argsValues = r.ReadTArray<int>(sizeof(int), argCount);
                        var argsNames = r.ReadTArray(r => r.ReadCString(), argCount);
                        if (argsNames[argCount - 1] == "\u0005") { argCount--; r.Skip(-2); }
                        for (var i = 0; i < argCount; i++) args[argsNames[i] ?? $"${i}"] = argsValues[i];
                    }
                    if (assetCount > 0)
                    {
                        var assetType = r.ReadTArray<long>(sizeof(long), assetCount);
                        assetInfos = assetType.Select(x => (ASSET_TYPE)x).ToArray();
                    }
                }
                r.Skip(endSkip);
                return (args, assetInfos);
            }
        }

        static string GetZoneFile(string filePath, byte[] cryptKey, BinaryReader r, ref FF_Header header)
        {
            var zonePath = Path.Combine(Path.GetDirectoryName(filePath), Path.GetFileNameWithoutExtension(filePath) + ".ff~");
            if (File.Exists(zonePath)) return zonePath;

            static byte[] CreateIVTable_BO(byte[] source)
            {
                // Init tables
                var ivTable = new byte[0xFB0];

                // Build table
                var ptr = 0;
                for (var i = 0; i < 200; i++)
                    for (var x = 0; x < 5; x++)
                    {
                        // Check next byte
                        if (source[ptr] == 0x00)
                            ptr = 0;

                        // Copy 4 times
                        ivTable[(i * 20) + (x * 4)] = source[ptr];
                        ivTable[(i * 20) + (x * 4) + 1] = source[ptr];
                        ivTable[(i * 20) + (x * 4) + 2] = source[ptr];
                        ivTable[(i * 20) + (x * 4) + 3] = source[ptr];
                        ptr++;
                    }

                // Copy BlockNums
                Array.Copy(new byte[] { 1, 0, 0, 0 }, 0, ivTable, 0xFA0, 4);
                Array.Copy(new byte[] { 1, 0, 0, 0 }, 0, ivTable, 0xFA4, 4);
                Array.Copy(new byte[] { 1, 0, 0, 0 }, 0, ivTable, 0xFA8, 4);
                Array.Copy(new byte[] { 1, 0, 0, 0 }, 0, ivTable, 0xFAC, 4);

                // Return table
                return ivTable;
            }

            static byte[] CreateIVTable(byte[] source)
            {
                var ivTable = new byte[16000];
                int addDiv = 0, nameKeyLength = Array.FindIndex(source, b => b == 0);
                for (var i = 0; i < ivTable.Length; i += nameKeyLength * 4)
                    for (var x = 0; x < nameKeyLength * 4; x += 4)
                    {
                        if ((i + addDiv) >= ivTable.Length || i + x >= ivTable.Length) return ivTable;
                        addDiv = x > 0 ? x / 4 : 0;
                        for (var y = 0; y < 4; y++) ivTable[i + x + y] = source[addDiv];
                    }
                return ivTable;
            }

            static void UpdateIVTable(int index, byte[] hash, byte[] ivTable, int[] ivCounter)
            {
                for (var i = 0; i < 20; i += 5)
                {
                    var value = (index + 4 * ivCounter[index]) % 800 * 5;
                    for (var x = 0; x < 5; x++) ivTable[4 * value + x + i] ^= hash[i + x];
                }
                ivCounter[index]++;
            }

            static byte[] GetIV(int index, byte[] ivTable, int[] ivCounter)
            {
                var iv = new byte[8];
                var arrayIndex = (index + 4 * (ivCounter[index] - 1)) % 800 * 20;
                Array.Copy(ivTable, arrayIndex, iv, 0, 8);
                return iv;
            }

            // extract zone
            using (var zoneStream = File.Create(zonePath))
                try
                {
                    switch (header.Version)
                    {
                        // https://gist.github.com/Scobalula/a0fd08197497336f67b7ff551b2db404
                        case 0x42:
                        case 0x72e:
                            {
                            }
                            break;
                        // Call of Duty 4: Modern Warfare - 0x5
                        // Black Ops Fast File - 0x1d9
                        // https://wiki.zeroy.com/images/b/b4/Unpack.png
                        // Call of Duty: Advanced Warfare - 0x183
                        case 0x5:
                        case 0x1d9:
                        case 0x183:
                            {
                                r.Seek(0x0E);
                                var decryptedData = r.ReadBytes((int)(r.BaseStream.Length - r.BaseStream.Position));
                                using (var s = new MemoryStream(decryptedData))
                                using (var decompressor = new DeflateStream(s, CompressionMode.Decompress))
                                    decompressor.CopyTo(zoneStream);
                                zoneStream.Flush();
                            }
                            break;
                        // Black Ops II Fast File
                        case 0x93:
                            {
                                // Get IV Table
                                r.Seek(0x18);
                                var ivCount = Enumerable.Repeat(1, 4).ToArray();
                                var ivTable = CreateIVTable(r.ReadBytes(0x20));
                                r.Skip(0x100); // Skip the RSA sig.
                                var salsa = new Salsa20 { Key = cryptKey };

                                var sectionIndex = 0;
                                while (true)
                                {
                                    // Read section size.
                                    var size = r.ReadInt32();

                                    // Check that we've reached the last section.
                                    if (size == 0) break;

                                    // Decrypt and update IVtable
                                    salsa.IV = GetIV(sectionIndex % 4, ivTable, ivCount);
                                    var decryptedData = salsa.CreateDecryptor().TransformFinalBlock(r.ReadBytes(size), 0, size);
                                    using (var sha1 = SHA1.Create()) UpdateIVTable(sectionIndex % 4, sha1.ComputeHash(decryptedData), ivTable, ivCount);

                                    // Uncompress the decrypted data.
                                    try
                                    {
                                        using (var s = new MemoryStream(decryptedData))
                                        using (var decompressor = new DeflateStream(s, CompressionMode.Decompress))
                                            decompressor.CopyTo(zoneStream);
                                        zoneStream.Flush();
                                    }
                                    catch
                                    {
                                        Console.WriteLine("Error Decoding");
                                        return zonePath;
                                    }
                                    sectionIndex++;
                                }
                                break;
                            }
                        // Black Ops III Fast File
                        case 0x251:
                            {
                                // Validate the flags, we only support ZLIB, PC, and Non-Encrypted FFs
                                if (header.BO3_FlagsZLIB != 1) throw new Exception("Invalid Fast File Compression. Only ZLIB Fast Files are supported.");
                                if (header.BO3_FlagsPC != 0) throw new Exception("Invalid Fast File Platform. Only PC Fast Files are supported.");
                                if (header.BO3_FlagsEncrypted != 0) throw new Exception("Encrypted Fast Files are not supported");

                                // get file size
                                r.Seek(0x90);
                                var size = r.ReadInt64();

                                // decode blocks
                                r.Seek(0x248);
                                var consumed = 0;
                                while (consumed < size)
                                {
                                    // Read Block Header & validate the block position, it should match 
                                    var block = r.ReadT<FF_BO3BlockHeader>(sizeof(FF_BO3BlockHeader));
                                    if (block.Position != r.BaseStream.Position - 16) throw new Exception("Block Position does not match Stream Position.");

                                    // Check for padding blocks
                                    if (block.DecompressedSize == 0)
                                    {
                                        r.Align(0x800000); //r.Skip(Utility.ComputePadding((int)r.BaseStream.Position, 0x800000));
                                        continue;
                                    }

                                    // Uncompress the decrypted data.
                                    r.Skip(2);
                                    var decryptedData = r.ReadBytes(block.CompressedSize - 2);
                                    using (var s = new MemoryStream(decryptedData))
                                    using (var decompressor = new DeflateStream(s, CompressionMode.Decompress))
                                        decompressor.CopyTo(zoneStream);
                                    zoneStream.Flush();
                                    consumed += block.DecompressedSize;

                                    // Sinze Fast Files are aligns, we must skip the full block
                                    r.Seek(block.Position + 16 + block.Size);
                                }
                                break;
                            }

                        default: throw new FormatException($"Unknown Version: {header.Version}");
                    }
                    return zonePath;
                }
                catch
                {
                    zoneStream.Close();
                    File.Delete(zonePath);
                    return null;
                }
        }

        #endregion

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
        struct XPAK_Header //: BO3XPakHeader, VGXPAKHeader
        {
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
            public ulong Key;
            public ulong Offset;
            public ulong Size;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 0x1)]
        struct XPAK_HashEntryVG //: VGXPAKHashEntry
        {
            public ulong Key;
            public ulong PackedInfo;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 0x1)]
        struct XPAK_DataHeader //: BO3XPakDataHeader
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

        static readonly byte[] FF_Stop8 = { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };
        static readonly byte[] FF_Stop4 = { 0xFF, 0xFF, 0xFF, 0xFF };

        public unsafe override Task ReadAsync(BinaryPakFile source, BinaryReader r, ReadStage stage)
        {
            if (!(source is BinaryPakManyFile multiSource)) throw new NotSupportedException();
            if (stage != ReadStage.File) throw new ArgumentOutOfRangeException(nameof(stage), stage.ToString());

            var extension = Path.GetExtension(source.FilePath);
            var files = multiSource.Files = new List<FileMetadata>();
            var cryptKey = source.Family.Games[source.Game].Key is Family.AesKey aes ? aes.Key : null;

            switch (extension)
            {
                // IWD
                case ".iwd":
                    {
                        source.UseBinaryReader = false;
                        source.Magic = (int)Magic.IWD;

                        var pak = (ZipFile)(source.Tag = new ZipFile(r.BaseStream));
                        foreach (ZipEntry entry in pak)
                            if (entry.Size != 0)
                                files.Add(new FileMetadata
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
                // https://gist.github.com/Scobalula/a0fd08197497336f67b7ff551b2db404
                // https://www.itsmods.com/forum/Thread-Release-FF-decompiler.html
                // https://www.se7ensins.com/forums/threads/release-ff-explorer.933419/ - BO
                // https://cabconmodding.com/threads/black-ops-ii-fast-file-explorer-v1-1-by-master131-download.79/ - BO2
                // https://www.itsmods.com/forum/Thread-Release-Black-Ops-2-FastFile-decrypter.html - BO2
                case ".ff":
                    {
                        source.Magic = (int)Magic.FF;

                        var header = r.ReadT<FF_Header>(sizeof(FF_Header));
                        if (header.Magic != FF_MAGIC_IW && header.Magic != FF_MAGIC_S1 && header.Magic != FF_MAGIC_TA) throw new FormatException($"Bad magic {header.Magic}");
                        if (header.Format != FF_FORMAT_u100 && header.Format != FF_FORMAT_0100 && header.Format != FF_FORMAT_a100 && header.Format != FF_FORMAT_0000) throw new FormatException($"Bad format {header.Format}");

                        var zonePath = GetZoneFile(source.FilePath, cryptKey, r, ref header);
                        if (zonePath == null) return Task.CompletedTask;

                        // Create new streams for the zone file.
                        r = new BinaryReader(new FileStream(zonePath, FileMode.Open, FileAccess.Read, FileShare.Read));
                        var zone = r.ReadT<FF_ZoneHeader>(sizeof(FF_ZoneHeader));
                        var (args, assetInfos) = zone.GetArgsAndAssetInfos(r, ref header);

                        for (var i = 0; i < assetInfos.Length; i++)
                        {
                            files.Add(new FileMetadata
                            {
                                Id = i,
                                Path = $"{i}.{assetInfos[i]}",
                                Position = 0,
                                FileSize = 0,
                            });
                        }

                        //var needle = FF_Stop8; // header.Version == 0x251 ? FF_Stop8 : FF_Stop4;
                        //var indexs = r.FindBytes(needle);
                        //for (var i = 0; i < indexs.Length; i++)
                        //{
                        //    r.Seek(indexs[i] + needle.Length);
                        //    var path = r.ReadZASCII(128);
                        //    var position = r.Position();
                        //    var size = indexs.Length > i + 1 ? indexs[i + 1] - position : 0;
                        //    files.Add(new FileMetadata
                        //    {
                        //        Id = i,
                        //        Path = path,
                        //        Position = position,
                        //        FileSize = size,
                        //        //Tag = assetTypes[i],
                        //    });
                        //}

                        //foreach (var index in indexs)
                        //{
                        //    r.Seek(index);
                        //    if (header.Version == 0x251)
                        //    {
                        //        var asset = r.ReadT<FF_Asset64>(sizeof(FF_Asset64));
                        //        if (asset.namePtr != 0xFFFFFFFFFFFFFFFF || asset.dataPtr != 0xFFFFFFFFFFFFFFFF || asset.size > uint.MaxValue) continue;
                        //        var name = r.ReadZASCII(128);
                        //    }
                        //    else
                        //    {
                        //        var asset = r.ReadT<FF_Asset32>(sizeof(FF_Asset32));
                        //        if (asset.namePtr != 0xFFFFFFFF || asset.dataPtr != 0xFFFFFFFF || asset.size > int.MaxValue) continue;
                        //        var name = r.ReadZASCII(128);
                        //    }
                        //}
                        return Task.CompletedTask;
                    }
                // PAK
                case ".pak":
                    {
                        source.Magic = (int)Magic.PAK;
                        return Task.CompletedTask;
                    }
                // XPAK
                case ".xpak":
                    {
                        source.Magic = (int)Magic.XPAK;
                        var header = r.ReadT<XPAK_Header>(sizeof(XPAK_Header));
                        // Verify the magic and offset
                        if (header.Magic != XPAK_MAGIC) throw new FormatException("Bad magic");
                        var type = header.Version == 0x1 ? r.ReadUInt64() : 0;
                        var size = r.ReadUInt64();
                        var unknownHashes = header.Version == 0x1 ? r.ReadBytes(1896) : null;
                        var fileCount = r.ReadUInt64();
                        if (header.Version == 0xD) r.Skip(288); // If MW4 we need to skip the new bytes
                        var body = r.ReadT<XPAK_Body>(sizeof(XPAK_Body));
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
                            files.Add(new FileMetadata
                            {
                                Id = (int)entry.Key,
                                Path = entry.Key.ToString(),
                                Position = (long)(body.DataOffset + entry.Offset), //: Offset
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