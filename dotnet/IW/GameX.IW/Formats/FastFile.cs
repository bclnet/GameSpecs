using Compression;
using GameX.Formats;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using FF_Header = GameX.IW.Formats.PakBinary_IW.FF_Header;
using FF_VERSION = GameX.IW.Formats.PakBinary_IW.FF_VERSION;

namespace GameX.IW.Formats
{
    public unsafe static partial class FastFile
    {
        [StructLayout(LayoutKind.Sequential)]
        struct FF_BO3BlockHeader
        {
            public static (string, int) Struct = ("<?", sizeof(FF_BO3BlockHeader));
            public int CompressedSize;
            public int DecompressedSize;
            public int Size;
            public int Position;
        }

        //[StructLayout(LayoutKind.Sequential)]
        //struct FF_Asset32
        //{
        //    public static (string, int) Struct = ("<?", sizeof(FF_Asset32));
        //    public uint namePtr;
        //    public int size;
        //    public uint dataPtr;
        //}

        //[StructLayout(LayoutKind.Sequential)]
        //struct FF_Asset64
        //{
        //    public static (string, int) Struct = ("<?", sizeof(FF_Asset64));
        //    public ulong namePtr;
        //    public long size;
        //    public ulong dataPtr;
        //}

        [StructLayout(LayoutKind.Explicit)]
        struct FF_ZoneHeader
        {
            public static (string, int) Struct = ("<?", sizeof(FF_ZoneHeader));
            [FieldOffset(0)] public uint Size;                  // decompressed fastfile size minus 44 (0x2C)
            [FieldOffset(4)] public uint ReferenceSize;         // about the total size of referenced data, e.g. the required memory for IWI textures if material files are in the ff
            //
            [FieldOffset(44)] public ushort CO_Args;            // equal to number of entries in "1st index" and amount of (model tag/joint/notetrack) strings (times 4 for index length)
            [FieldOffset(48)] public int CO_ArgSeparator;       // separator? (FF FF FF FF)
            [FieldOffset(52)] public int CO_Assets;             // number of records* ("2nd index", times 8 for index length)
            [FieldOffset(56)] public long CO_AssetsSeparator;   // separator? (FF FF FF FF FF FF FF FF)
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

            // https://en.wikipedia.org/wiki/IW_(game_engine)
            (int seek, int argCount, int assetCount, int endSkip) GetArgsAndAssetsOffsets(FF_VERSION version)
                => version switch
                {
                    FF_VERSION.CO4_WWII => (0x3C, CO_Args, CO_Assets, 0),       // IW 3.0: Call of Duty 4: Modern Warfare
                    FF_VERSION.WaW => (0x34, AW_Args, AW_Assets, 0),     // IW 3.0+: Call of Duty: World at War
                    FF_VERSION.MW2 => (0x3C, CO_Args, CO_Assets, 0),     // IW 4.0: Call of Duty: Modern Warfare 2
                    FF_VERSION.BO => (0x34, BO_Args, BO_Assets, 0),     // IW 3.0: Call of Duty: Black Ops
                    FF_VERSION.BO2 => (0x40, BO2_Args, BO2_Assets, 0),    // IW 3.0m: Call of Duty: Black Ops II
                    FF_VERSION.BO3 => (0x40, BO3_Args, BO3_Assets, 0),   // IW 3.0m: Call of Duty: Black Ops III
                    _ => throw new FormatException($"Unknown Version: {version}"),
                };

            public (Dictionary<string, long> args, string[] assetInfos) GetArgsAndAssetInfos(BinaryReader r, ref FF_Header header)
            {
                var version = header.Version;
                var (seek, argCount, assetCount, endSkip) = GetArgsAndAssetsOffsets(version);
                var args = new Dictionary<string, long>();
                string[] assetInfos = null;
                r.Seek(seek);
                //if (version >= 0x251)
                //{
                //    if (argCount > 0)
                //    {
                //        var argsValues = r.ReadTArray<long>(sizeof(long), argCount);
                //        var argsNames = r.ReadTArray(r => r.ReadCString(), argCount);
                //        if (argsNames[argCount - 1] == "\u0005") { argCount--; r.Skip(-3); }
                //        for (var i = 0; i < argCount; i++) args[argsNames[i] ?? $"${i}"] = argsValues[i];
                //    }
                //    if (assetCount > 0)
                //    {
                //        var assetType = r.ReadTArray<long>(sizeof(long), assetCount * 2);
                //        //assetInfos = assetType.Select(x => (CodAssetType)(x << 32)).ToArray();
                //    }
                //}
                //else
                {
                    if (argCount > 0)
                    {
                        var argsValues = r.ReadTArray<int>(sizeof(int), argCount);
                        var argsNames = r.ReadFArray(r => r.ReadZAString(), argCount);
                        if (argsNames[argCount - 1] == "\u0005") { argCount--; r.Skip(-2); }
                        for (var i = 0; i < argCount; i++) args[argsNames[i] ?? $"${i}"] = argsValues[i];
                    }
                    if (assetCount > 0)
                    {
                        var assetType = r.ReadTArray<long>(sizeof(long), assetCount);
                        switch (version)
                        {
                            case FF_VERSION.CO4_WWII: assetInfos = assetType.Select(x => ((IW3XAssetType)x).ToString()).ToArray(); break; // Call of Duty 4: Modern Warfare
                            case FF_VERSION.WaW: assetInfos = assetType.Select(x => ((IW3XAssetType)x).ToString()).ToArray(); break; // Call of Duty: World at War
                            case FF_VERSION.BO: assetInfos = assetType.Select(x => ((Oth_AssetType)x).ToString()).ToArray(); break; // Call of Duty: Black Ops
                            case FF_VERSION.BO2: assetInfos = assetType.Select(x => ((Oth_AssetType)x).ToString()).ToArray(); break; // Call of Duty: Black Ops II
                            case FF_VERSION.BO3: assetInfos = assetType.Select(x => ((Oth_AssetType)x).ToString()).ToArray(); break; // Call of Duty: Black Ops III
                        }
                    }
                }
                r.Skip(endSkip);
                if (r.ReadInt32() != -1) throw new FormatException($"Bad End of Index");
                return (args, assetInfos);
            }
        }

        static readonly byte[] FF_Stop8 = { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };
        static readonly byte[] FF_Stop4 = { 0xFF, 0xFF, 0xFF, 0xFF };

        static string GetZoneFile(string filePath, byte[] cryptKey, BinaryReader r, ref FF_Header header)
        {
            var zonePath = Path.Combine(Path.GetDirectoryName(filePath), Path.GetFileNameWithoutExtension(filePath) + ".ff~");
            //if (File.Exists(zonePath)) return zonePath;

            //static byte[] CreateIVTable_BO(byte[] source)
            //{
            //    // Init tables
            //    var ivTable = new byte[0xFB0];

            //    // Build table
            //    var ptr = 0;
            //    for (var i = 0; i < 200; i++)
            //        for (var x = 0; x < 5; x++)
            //        {
            //            // Check next byte
            //            if (source[ptr] == 0x00)
            //                ptr = 0;

            //            // Copy 4 times
            //            ivTable[(i * 20) + (x * 4)] = source[ptr];
            //            ivTable[(i * 20) + (x * 4) + 1] = source[ptr];
            //            ivTable[(i * 20) + (x * 4) + 2] = source[ptr];
            //            ivTable[(i * 20) + (x * 4) + 3] = source[ptr];
            //            ptr++;
            //        }

            //    // Copy BlockNums
            //    Array.Copy(new byte[] { 1, 0, 0, 0 }, 0, ivTable, 0xFA0, 4);
            //    Array.Copy(new byte[] { 1, 0, 0, 0 }, 0, ivTable, 0xFA4, 4);
            //    Array.Copy(new byte[] { 1, 0, 0, 0 }, 0, ivTable, 0xFA8, 4);
            //    Array.Copy(new byte[] { 1, 0, 0, 0 }, 0, ivTable, 0xFAC, 4);

            //    // Return table
            //    return ivTable;
            //}

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
                        case FF_VERSION.MW:
                        case FF_VERSION.AW:
                            {
                            }
                            break;
                        case FF_VERSION.CO4_WWII:
                        case FF_VERSION.BO:
                        case FF_VERSION.WaW:
                            {
                                r.Seek(0x0E);
                                var decryptedData = r.ReadBytes((int)(r.BaseStream.Length - r.BaseStream.Position));
                                using (var s = new MemoryStream(decryptedData))
                                using (var decompressor = new DeflateStream(s, CompressionMode.Decompress))
                                    decompressor.CopyTo(zoneStream);
                                zoneStream.Flush();
                            }
                            break;
                        case FF_VERSION.MW2:
                            {
                                r.Seek(0x0E);
                                var decryptedData = r.ReadBytes((int)(r.BaseStream.Length - r.BaseStream.Position));
                                using (var s = new MemoryStream(decryptedData))
                                using (var decompressor = new DeflateStream(s, CompressionMode.Decompress))
                                    decompressor.CopyTo(zoneStream);
                                zoneStream.Flush();
                            }
                            break;
                        case FF_VERSION.BO2:
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
                        case FF_VERSION.BO3:
                            {
                                var unknown = r.ReadByte();
                                var flagsZLIB = r.ReadByte();
                                var flagsPC = r.ReadByte();
                                var flagsEncrypted = r.ReadByte();
                                // Validate the flags, we only support ZLIB, PC, and Non-Encrypted FFs
                                if (flagsZLIB != 1) throw new Exception("Invalid Fast File Compression. Only ZLIB Fast Files are supported.");
                                if (flagsPC != 0) throw new Exception("Invalid Fast File Platform. Only PC Fast Files are supported.");
                                if (flagsEncrypted != 0) throw new Exception("Encrypted Fast Files are not supported");

                                // get file size
                                r.Seek(0x90);
                                var size = r.ReadInt64();

                                // decode blocks
                                r.Seek(0x248);
                                var consumed = 0;
                                while (consumed < size)
                                {
                                    // Read Block Header & validate the block position, it should match 
                                    var block = r.ReadS<FF_BO3BlockHeader>();
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

        internal class FileHeader
        {
            public int Id;
            public string Path;
            public long Position;
            public long FileSize;
        }

        struct COD_Material
        {
            public short Size;
            public byte B1;
            public byte B2;

            public COD_Material(BinaryReader r)
            {
                Size = r.ReadInt16();
                var end = r.Tell() + Size;
                B1 = r.ReadByte();
                B2 = r.ReadByte();
                var moreHeader = r.ReadBytes(0x10);
                var separator = r.ReadInt32();     // Separator
            }
        }

        struct COD_Shader
        {
            public int[] Pointers;
            public string Name;

            public COD_Shader(BinaryReader r)
            {
                Pointers = r.ReadTArray<int>(sizeof(int), 36);  // 36 pointers (0x90 bytes) (bytes 0x19 up to 0x22 say if we get any content at all?)
                Name = r.ReadZAString();                          // Then techset file name (0x00 termination) if pointer is 0xFFFFFFFF?

                // start of shader pack
                while (r.ReadInt32() != -1) // pointer (-1)
                {
                    var packOption = r.ReadInt32();             // 1 dword of some character-length options / flags ?
                    var packPointer2 = r.ReadTArray<int>(sizeof(int), 3); // 3 pointers
                    var packSizes = r.ReadInt32();              // 1 dword of some character-length options / flags ?
                    var packSeparator = r.ReadInt32();          // Separator
                    var packPointer3 = r.ReadTArray<short>(sizeof(short), 0x32); // 0x64 bytes of some short length options / flags? If the second pointer above is 0xFFFFFFFF

                    // start of shader
                    while (true)
                    {
                        var shadePointer1 = r.ReadInt32();      // pointer, if not 0xFFFFFFFF, no filename is further on
                        var shadePointer2 = r.ReadInt32();      // pointer, often 00 00 00 00
                        var shadeSeparator = r.ReadInt32();     // Separator
                        var shadeSize = r.ReadInt16() * 4;      // 2 shorts, first short multiplied by 4 gives the length of the shader, second is a flag?
                        var shadeFlag = r.ReadInt16();
                        break;
                    }
                    break;
                }
            }
        }

        // https://www.itsmods.com/forum/Thread-Release-FF-decompiler.html
        // https://www.se7ensins.com/forums/threads/release-ff-explorer.933419/ - BO
        // https://cabconmodding.com/threads/black-ops-ii-fast-file-explorer-v1-1-by-master131-download.79/ - BO2
        // https://www.itsmods.com/forum/Thread-Release-Black-Ops-2-FastFile-decrypter.html - BO2
        // https://gist.github.com/Scobalula/a0fd08197497336f67b7ff551b2db404 - S1ff 0x42|0x72e 
        // https://wiki.zeroy.com/index.php?title=Call_of_Duty_4:_FastFile_Format - COD4 FF
        internal static List<FileHeader> GetAssets(BinaryPakFile source, BinaryReader r, byte[] cryptKey, ref FF_Header header)
        {
            var zonePath = GetZoneFile(source.PakPath, cryptKey, r, ref header);
            if (zonePath == null) return null;
            var headers = new List<FileHeader>();

            // Create new streams for the zone file.
            r = new BinaryReader(new FileStream(zonePath, FileMode.Open, FileAccess.Read, FileShare.Read));
            var zone = r.ReadS<FF_ZoneHeader>();
            var (args, assetInfos) = zone.GetArgsAndAssetInfos(r, ref header);

            // foreach asset
            string path = null;
            for (var i = 0; i < assetInfos.Length; i++)
            {
                var info = assetInfos[i];
                switch (info)
                {
                    case "Material":
                        {
                            var mat = new COD_Material(r);
                            break;
                        }
                    case "PixelShader":
                        {
                            var mat = new COD_Shader(r);
                            //path = r.ReadZASCII(128);
                            //var ps = r.ReadT<PixelShader>(PixelShader.COD4_SizeOf);
                            //if (r.ReadInt32() != -1) throw new FormatException($"Bad End of Index");
                            break;
                        }
                    default:
                        path = $"{i}.{assetInfos[i]}";
                        break;
                }
                headers.Add(new FileHeader
                {
                    Id = i,
                    Path = path,
                    Position = 0,
                    FileSize = 0,
                });
            }

            return null;
        }
    }
}


// find start
//r.SeekNeedles(FF_Stop4).First();
//if (r.ReadInt32() != -1) throw new FormatException($"Bad End of Index");

//// foreach asset
//string path;
//var i = 0;
//var position = r.Position();
//object value;
//foreach (var needle in r.SeekNeedles(FF_Stop8).Take(20))
//{
//    var info = assetInfos[i++];
//    r.Seek(position);
//    switch (info)
//    {
//        case CodAssetType.localize:
//            value = r.ReadZASCII(128);
//            path = r.ReadZASCII(128);
//            break;
//        case CodAssetType.rawfile:
//            path = r.ReadZASCII(128);
//            position = r.Position();
//            break;
//        default:
//            position = r.Position();
//            path = $"{info}";
//            break;
//    }
//    //files.Add(new FileMetadata
//    //{
//    //    Id = i,
//    //    Path = path,
//    //    Position = position,
//    //    FileSize = needle - position,
//    //});
//    position = needle + 8;
//}

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