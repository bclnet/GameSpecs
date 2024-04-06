using System;
using System.Diagnostics;
using System.IO;
using static GameX.Epic.Formats.Core.Game;

namespace GameX.Epic.Formats.Core
{
    public partial class UPackage
    {
        const int MAX_FNAME_LEN = 1024;
        public static Game GForceGame = UNKNOWN;
        public static int GForcePackageVersion = 0;
        public static Platform GForcePlatform = Platform.UNKNOWN;
        public static COMPRESS GForceCompMethod = 0;

        public Game Game;
        public Platform Platform;
        public int ArVer;
        public int ArLicenseeVer;
        public bool ReverseBytes;
        bool IsFullyCompressed;
        public BinaryReader R;

        // Package structures
        string Filename;
        FPackageFileSummary Summary;
        string[] Names;
        public FObjectImport[] Imports;
        public FObjectExport[] Exports;
        //FPackageObjectIndex ExportIndices_IOS;

        public UPackage(BinaryReader r, string path)
        {
            Filename = Path.GetFileName(path);
            r = CreateLoader(r);
            Summary = new FPackageFileSummary(r, this);

#if DEBUG_VERBOSE
            var packageLine = $"Loading package: {Filename} Ver: {ArVer}/{ArLicenseeVer} ";
            if (Game >= UE3)
            {
                packageLine += $"Engine: {Summary.EngineVersion} ";
                if (IsFullyCompressed) packageLine += $"[FullComp] ";
            }
            if (Game >= UE4_BASE && Summary.IsUnversioned) packageLine += "[Unversioned] ";
            Debug.WriteLine(packageLine);
            Debug.WriteLine($"Names: {Summary.NameCount} Exports: {Summary.ExportCount} Imports: {Summary.ImportCount} Game: {Game}");
            Debug.WriteLine($"Flags: {Summary.PackageFlags}, Name offset: {Summary.NameOffset}, Export offset: {Summary.ExportOffset}, Import offset: {Summary.ImportOffset}");
            for (var i = 0; i < Summary.CompressedChunks.Length; i++) Debug.WriteLine($"chunk[{i}]: {Summary.CompressedChunks[i]}");
#endif

            r = ReplaceLoader(r);

            LoadNames(r);
            LoadImports(r);
            LoadExports(r);

            ProcessEventDrivenFile(r);
            R = r;
        }

        bool VerifyName(ref string nameStr, int nameIndex)
        {
            // Verify name, some Korean games (B&S) has garbage in FName (unicode?)
            var goodName = true;
            var numBadChars = 0;
            foreach (var c in nameStr)
                if (c == 0) break; // end of line is included into FString unreadable character
                else if (c < ' ' || c > 0x7F) { goodName = false; break; }
                else if (c == '$') numBadChars++; // unicode characters replaced with '$' in FString serializer
            if (goodName && numBadChars != 0)
            {
                var nameLen = nameStr.Length;
                if (nameLen >= 64 || (numBadChars >= nameLen / 2 && nameLen > 16)) goodName = false;
            }
            if (!goodName)
            {
                // replace name
                Debug.WriteLine($"WARNING: XX: fixing name {nameIndex} ({nameStr})");
                nameStr = $"__name_{nameIndex}__";
            }
            return goodName;
        }

        BinaryReader CreateLoader(BinaryReader r)
        {
            const long MAX_FILE_SIZE32 = 1L << 31;		// 2Gb for int32
            // Verify the file size first, taking into account that it might be too large to open (requires 64-bit size support).
            var FileSize = r.BaseStream.Length;
            if (FileSize < 16 || FileSize >= MAX_FILE_SIZE32)
            {
                if (FileSize > 1024) Debug.WriteLine($"WARNING: package file {Filename} is too large ({FileSize >> 20} Mb), ignoring");
                // The file is too small, possibly invalid one.
                return null;
            }

            var checkDword = r.ReadUInt32(); // Pick 32-bit integer from archive to determine its type
            r.Seek(0); // Seek back to file start

            // LINEAGE2 || EXTEEL
            if (checkDword == ('L' | ('i' << 16)))  // unicode string "Lineage2Ver111"
            {
                // this is a Lineage2 package
                r.Seek(LineageStream.LINEAGE_HEADER_SIZE);
                // here is a encrypted by 'xor' standard FPackageFileSummary
                // to get encryption key, can check 1st byte
                var b = r.ReadByte();
                // for Ver111 XorKey==0xAC for Lineage or ==0x42 for Exteel, for Ver121 computed from filename
                var XorKey = (byte)(b ^ (TAG & 0xFF));
                Game = Lineage2;
                return new BinaryReader(new LineageStream(r, XorKey));
            }

            // BATTLE_TERR
            if (checkDword == 0x342B9CFC) { Game = BattleTerr; return new BinaryReader(new BattleTerrStream(r)); }

            // NURIEN
            if (checkDword == 0xB01F713F) return new BinaryReader(new NurienStream(r));

            // BLADENSOUL
            if (checkDword == 0xF84CEAB0)
            {
                if (GForceGame == 0) GForceGame = BladeNSoul;
                Game = BladeNSoul;
                return new BinaryReader(new BnSStream(r));
            }

            // Code for loading UE3 "fully compressed packages"
            var checkDword1 = r.ReadUInt32();
            if (checkDword1 == TAG_REV)
            {
                ReverseBytes = true;
                if (GForcePlatform == Platform.UNKNOWN) Platform = Platform.XBOX360;            // default platform for "ReverseBytes" mode is PLATFORM_XBOX360
            }
            else if (checkDword1 != TAG)
            {
                // fully compressed package always starts with package tag
                r.Seek(0);
                return r;
            }
            // Read 2nd dword after changing byte order in Loader
            var checkDword2 = r.ReadUInt32();
            r.Seek(0);

            // Check if this is a fully compressed package. UE3 by itself checks if there's .uncompressed_size with text contents
            // file exists next to the package file.
            if (checkDword2 == TAG || checkDword2 == 0x20000 || checkDword2 == 0x10000)    // seen 0x10000 in Enslaved/PS3
            {
                //!! NOTES: MKvsDC/X360 Core.u and Engine.u uses LZO instead of LZX (LZO and LZX are not auto-detected with COMPRESS_FIND)
                // this is a fully compressed package
                var H = new FCompressedChunkHeader(r, this);
                var Chunks = new[]{
                    new FCompressedChunk
                    {
                        UncompressedOffset = 0, UncompressedSize = H.Sum.UncompressedSize,
                        CompressedOffset = 0, CompressedSize = H.Sum.CompressedSize,
                    }
                };
                COMPRESS CompMethod = GForceCompMethod;
                if (CompMethod == 0) CompMethod = Platform == Platform.XBOX360 ? COMPRESS.LZX : COMPRESS.FIND;
                IsFullyCompressed = true;
                return new BinaryReader(new UE3Stream(r, this, CompMethod, Chunks));
            }

            return r;
        }

        BinaryReader ReplaceLoader(BinaryReader r)
        {
            // Current FArchive position is after FPackageFileSummary
            if ((Game == Bioshock) && (Summary.PackageFlags & 0x20000) != 0)
            {
                // Bioshock has a special flag indicating compression. Compression table follows the package summary.
                // Read compression tables.
                var NumChunks = r.ReadInt32();
                var Chunks = new FCompressedChunk[NumChunks];
                var UncompOffset = (int)r.Tell() - 4;              //?? there should be a flag signalling presence of compression structures, because of "Tell()-4"
                for (var i = 0; i < NumChunks; i++)
                {
                    var Offset = r.ReadInt32();
                    Chunks[i] = new FCompressedChunk
                    {
                        UncompressedOffset = UncompOffset,
                        UncompressedSize = 32768,
                        CompressedOffset = Offset,
                        CompressedSize = 0          //?? not used
                    };
                    UncompOffset += 32768;
                }
                // Replace Loader for reading compressed Bioshock archives.
                return new BinaryReader(new UE3Stream(r, this, COMPRESS.ZLIB, Chunks));
            }
            else if (Game == AA2)
            {
                // America's Army 2 has encryption after FPackageFileSummary
                if (ArLicenseeVer >= 19)
                {
                    var IsEncrypted = r.ReadInt32();
                    if (IsEncrypted != 0) r = new BinaryReader(new AA2Stream(r));
                }
                return r;
            }
            else if (Game == RocketLeague && (Summary.PackageFlags & PKG_Cooked) != 0)
            {
                throw new NotImplementedException();
                //    // Rocket League has an encrypted header after FPackageFileSummary containing the name/import/export tables and a compression table.
                //    TArray<FString> AdditionalPackagesToCook;
                //    *this << AdditionalPackagesToCook;

                //    // Array of unknown structs
                //    int32 NumUnknownStructs;
                //    *this << NumUnknownStructs;
                //    for (int i = 0; i < NumUnknownStructs; i++)
                //    {
                //        this->Seek(this->Tell() + sizeof(int32) * 5); // skip 5 int32 values
                //        TArray<int32> unknownArray;
                //        *this << unknownArray;
                //    }

                //    // Info related to encrypted buffer
                //    int32 GarbageSize, CompressedChunkInfoOffset, LastBlockSize;
                //    *this << GarbageSize << CompressedChunkInfoOffset << LastBlockSize;

                //    // Create a reader to decrypt the rest of Rocket League's header
                //    FFileReaderRocketLeague* RocketReader = new FFileReaderRocketLeague(Loader);
                //    RocketReader->SetupFrom(*this);
                //    RocketReader->EncryptionStart = Summary.NameOffset;
                //    RocketReader->EncryptionEnd = Summary.HeadersSize;

                //    // Create a UE3 compression reader with the chunk info contained in the encrypted RL header
                //    RocketReader->Seek(RocketReader->EncryptionStart + CompressedChunkInfoOffset);

                //    TArray<FCompressedChunk> Chunks;
                //    *RocketReader << Chunks;

                //    Loader = new UE3Stream(RocketReader, COMPRESS_ZLIB, Chunks);
                //    Loader->SetupFrom(*this);

                //    // The decompressed chunks will overwrite past CompressedChunkInfoOffset, so don't decrypt past that anymore
                //    RocketReader->EncryptionEnd = RocketReader->EncryptionStart + CompressedChunkInfoOffset;
                //    return r;
            }
            // Nurien has encryption in header, and no encryption after
            else if (r.BaseStream is NurienStream z)
            {
                z.Threshold = Summary.HeadersSize;
                return r;
            }
            else if (Game >= UE3 && Summary.CompressionFlags != 0 && Summary.CompressedChunks.Length != 0)
            {
                if (IsFullyCompressed) throw new Exception($"Fully compressed package {Filename} has additional compression table");
                return new BinaryReader(new UE3Stream(r, this, Summary.CompressionFlags, Summary.CompressedChunks)); // replace Loader with special reader for compressed UE3 archives
            }
            else return r;
        }

        void LoadNames(BinaryReader r)
        {
            if (Summary.NameCount == 0) return;
            r.Seek(Summary.NameOffset);
            Names = new string[Summary.NameCount];
            if (Game >= UE4_BASE) LoadNames4(r, this);
            else if (Game >= UE3) LoadNames3(r, this);
            else LoadNames2(r, this);
        }

        void LoadImports(BinaryReader r)
        {
            if (Summary.ImportCount == 0) return;
            r.Seek(Summary.ImportOffset);
            Imports = r.ReadFArray(r => new FObjectImport(r, this), (int)Summary.ImportCount);
#if DEBUG_VERBOSE
            for (var i = 0; i < Summary.ImportCount; i++) Debug.WriteLine($"Import[{i}]: {Imports[i]}");
#endif
        }

        void PatchBnSExports(FObjectExport[] exp, FPackageFileSummary summary) { }
        void PatchDunDefExports(FObjectExport[] exp, FPackageFileSummary summary) { }

        void LoadExports(BinaryReader r)
        {
            if (Summary.ExportCount == 0) return;
            r.Seek(Summary.ExportOffset);
            Exports = r.ReadFArray(r => new FObjectExport(r, this), (int)Summary.ExportCount);
            if (Game == BladeNSoul && (Summary.PackageFlags & 0x08000000) != 0) PatchBnSExports(Exports, Summary);
            if (Game == DunDef) PatchDunDefExports(Exports, Summary);
#if DEBUG_VERBOSE
            for (var i = 0; i < Summary.ExportCount; i++) Debug.WriteLine($"Export[{i}]: {GetClassNameFor(Exports[i])} {Exports[i]}");
#endif
        }

        // Process Event Driven Loader packages: such packages are split into 2 pieces: .uasset with headers
        // and .uexp with object's data. At this moment we already have FPackageFileSummary fully loaded,
        // so we can replace loader with .uexp file - with providing correct position offset.
        void ProcessEventDrivenFile(BinaryReader r)
        {
            //if (Game >= UE4_BASE && Summary.HeadersSize == Loader->GetFileSize())
            //{
            //    var name = $"{Path.GetFileNameWithoutExtension(filename)}.uexp";
            //    // When finding, explicitly tell to use the same folder where .uasset file exists
            //    const CGameFileInfo* expInfo = CGameFileInfo::Find(buf, FileInfo ? FileInfo->FolderIndex : -1);
            //    if (expInfo)
            //    {
            //        // Open .exp file
            //        FArchive* expLoader = expInfo->CreateReader();
            //        // Replace loader with this file, but add offset so it will work like it is part of original uasset
            //        Loader = new FReaderWrapper(expLoader, -Summary.HeadersSize);
            //    }
            //    else appPrintf("WARNING: it seems package %s has missing .uexp file\n", filename);
            //}
        }
    }
}