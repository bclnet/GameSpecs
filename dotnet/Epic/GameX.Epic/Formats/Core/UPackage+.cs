using System;
using System.IO;
using static GameX.Epic.Formats.Core.Game;
using static GameX.Epic.Formats.Core.UPackage;
using static GameX.Epic.Formats.Core.GameDatabase;
using System.Diagnostics;

namespace GameX.Epic.Formats.Core
{
    [DebuggerDisplay("E:{ExportCount},N:{NameCount},O:{NetObjectCount}")]
    struct FGenerationInfo
    {
        public uint ExportCount;
        public uint NameCount;
        public uint NetObjectCount;
        public FGenerationInfo(uint exportCount, uint nameCount) { ExportCount = exportCount; NameCount = nameCount; NetObjectCount = 0; }
        public FGenerationInfo(BinaryReader r, UPackage Ar)
        {
            ExportCount = r.ReadUInt32();
            NameCount = r.ReadUInt32();
            NetObjectCount = 0;
            if (Ar.Game >= UE4_BASE && Ar.ArVer >= (int)VERUE4.REMOVE_NET_INDEX) return;
            if (Ar.ArVer >= 322) NetObjectCount = r.ReadUInt32(); // PACKAGE_V3
        }
    }

    class FCompressedChunk
    {
        public int UncompressedOffset;
        public int UncompressedSize;
        public int CompressedOffset;
        public int CompressedSize;
        public FCompressedChunk() { }
        public FCompressedChunk(BinaryReader r, UPackage Ar)
        {
            if ((Ar.Game == MK && Ar.ArVer >= 677) || (Ar.Game == RocketLeague && Ar.ArLicenseeVer >= 22))
            {
                // MK X and Rocket League has 64-bit file offsets
                var UncompressedOffset64 = r.ReadInt64();
                UncompressedSize = r.ReadInt32();
                var CompressedOffset64 = r.ReadInt64();
                CompressedSize = r.ReadInt32();
                UncompressedOffset = (int)UncompressedOffset64;
                CompressedOffset = (int)CompressedOffset64;
                return;
            }
            UncompressedOffset = r.ReadInt32();
            UncompressedSize = r.ReadInt32();
            CompressedOffset = r.ReadInt32();
            CompressedSize = r.ReadInt32();
            if (Ar.Game == Bulletstorm && Ar.ArLicenseeVer >= 21) r.Skip(4);
        }
        public override string ToString() => $"comp={CompressedOffset:X}+{CompressedSize:X}, uncomp={UncompressedOffset:X}+{UncompressedSize:X}";
    }

    class FCompressedChunkBlock
    {
        public int CompressedSize;
        public int UncompressedSize;
        public FCompressedChunkBlock() { }
        public FCompressedChunkBlock(BinaryReader r, UPackage Ar)
        {
            if (Ar.Game == MK && Ar.ArVer >= 677) goto int64_offsets;   // MK X
            if (Ar.Game >= UE4_BASE) goto int64_offsets;
            CompressedSize = r.ReadInt32();
            UncompressedSize = r.ReadInt32();
            return;
        int64_offsets:
            // UE4 has 64-bit values here
            var CompressedSize64 = r.ReadInt64();
            var UncompressedSize64 = r.ReadInt64();
            Debug.Assert((CompressedSize64 | UncompressedSize64) <= 0x7FFFFFFF); // we're using 32 bit values
            CompressedSize = (int)CompressedSize64;
            UncompressedSize = (int)UncompressedSize64;
        }
    }

    class FCompressedChunkHeader
    {
        public uint Tag;
        public int BlockSize;                       // maximal size of uncompressed block
        public FCompressedChunkBlock Sum;          // summary for the whole compressed block
        public FCompressedChunkBlock[] Blocks;
        public FCompressedChunkHeader() { }
        public FCompressedChunkHeader(BinaryReader r, UPackage Ar)
        {
            Tag = r.ReadUInt32();
            if (Tag == TAG_REV) Ar.ReverseBytes = !Ar.ReverseBytes;
            else if (Ar.Game == Berkanix && Tag == 0xF2BAC156) goto tag_ok;
            else if (Ar.Game == Hawken && Tag == 0xEA31928C) goto tag_ok;
            else if (/*Ar.Game == MMH7 && */ Tag == 0x4D4D4837) goto tag_ok;        // Might & Magic Heroes 7
            else if (Tag == 0x7E4A8BCA) goto tag_ok; // iStorm
            else Debug.Assert(Tag == TAG);
            if (Ar.Game == MK && Ar.ArVer >= 677) goto int64_offsets;  // MK X
            if (Ar.Game >= UE4_BASE) goto int64_offsets;
            goto tag_ok;

        int64_offsets:
            // Tag and BlockSize are really FCompressedChunkBlock, which has 64-bit integers here.
            var Pad = r.ReadInt32();
            var BlockSize64 = r.ReadInt64();
            Debug.Assert((Pad == 0) && (BlockSize64 <= 0x7FFFFFFF));
            BlockSize = (int)BlockSize64;
            goto summary;

        tag_ok:
            BlockSize = r.ReadInt32();

        summary:
            Sum = new FCompressedChunkBlock(r, Ar);
            BlockSize = 0x20000;
            Blocks = new FCompressedChunkBlock[(Sum.UncompressedSize + 0x20000 - 1) / 0x20000];   // optimized for block size 0x20000
            int i = 0, CompSize = 0, UncompSize = 0;
            while (CompSize < Sum.CompressedSize && UncompSize < Sum.UncompressedSize)
            {
                var Block = Blocks[i++] = new FCompressedChunkBlock(r, Ar);
                CompSize += Block.CompressedSize;
                UncompSize += Block.UncompressedSize;
            }
            // check header; seen one package where sum(Block.CompressedSize) < H.CompressedSize, but UncompressedSize is exact
            Debug.Assert(/*CompSize == CompressedSize &&*/ UncompSize == Sum.UncompressedSize);
            if (Blocks.Length > 1) BlockSize = Blocks[0].UncompressedSize;
        }
    }

    [DebuggerDisplay("{Major}.{Minor}.{Patch}")]
    struct FEngineVersion
    {
        ushort Major, Minor, Patch;
        int Changelist;
        string Branch;
        //public void FEngineVersion(BinaryReader r, FArchive Ar)
        //{
        //    //return Ar << V.Major << V.Minor << V.Patch << V.Changelist << V.Branch;
        //}
    }

    struct FCustomVersion
    {
        Guid Key;
        int Version;
        //void Do(BinaryReader ar)
        //{
        //    //return Ar << V.Key << V.Version;
        //}
    }

    struct FCustomVersionContainer
    {
        FCustomVersion[] Versions;
        //void Serialize(FArchive& Ar, int LegacyVersion);
    }

    partial class FPackageFileSummary
    {
        UPackage Ar;
        uint Tag;
        uint LegacyVersion;
        public bool IsUnversioned;
        FCustomVersionContainer CustomVersionContainer;
        ushort FileVersion;
        ushort LicenseeVersion;
        public int PackageFlags;
        public uint NameCount, NameOffset;
        public uint ExportCount, ExportOffset;
        public uint ImportCount, ImportOffset;
        Guid Guid;
        FGenerationInfo[] Generations;
        public int HeadersSize;        // used by UE3 for precaching name table
        string PackageGroup;       // "None" or directory name
        int DependsOffset;      // number of items = ExportCount
        int f38;
        int f3C;
        int f40;
        public int EngineVersion;
        int CookerVersion;
        public COMPRESS CompressionFlags;
        public FCompressedChunk[] CompressedChunks;
        int U3unk60;
        long BulkDataStartOffset;

        public FPackageFileSummary(BinaryReader r, UPackage ar)
        {
            Ar = ar;
            // read package tag
            Tag = r.ReadUInt32();
            switch (Tag) // some games has special tag constants
            {
                case TAG: break;
                case 0x9E2A83C2:   // Killing Floor
                case 0x7E4A8BCA:  // iStorm
                case 0xA94E6C81: break; // Nurien
                case 0xA1B2C93F: Ar.Game = BattleTerr; break;  // Battle Territory Online
                case 0xD58C3147: Ar.Game = Loco; break;  // Land of Chaos Online
                case 0xF2BAC156: Ar.Game = Berkanix; break;  // BERKANIX
                case 0xEA31928C: Ar.Game = Hawken; break;  // HAWKEN
                case 0x12345678: r.Skip(4); Ar.Game = TaoYuan; if (GForceGame == 0) GForceGame = TaoYuan; break;  // TAO_YUAN
                case 0xEC201133: var count = r.ReadByte(); r.Skip(count); break;  // STORMWAR
                case 0x879A4B41: Ar.Game = GunLegend; if (GForceGame == 0) GForceGame = GunLegend; break;  // GUNLEGEND
                case 0x4D4D4837: Ar.Game = UE3; break;  // Might & Magic Heroes 7 - version conflict with Guilty Gear Xrd
                case 0x7BC342F0: Ar.Game = DevilsThird; if (GForceGame == 0) GForceGame = DevilsThird; break;  // Devil's Third
                case TAG_REV: throw new Exception("ReverseBytes = true;");
                default: throw new Exception($"Wrong package tag ({Tag:x}) in file. Probably the file is encrypted.");
            }

            // read version
            var Version = r.ReadUInt32();

            // UE4 has negative version value, growing from -1 towards negative direction. This value is followed
            // by "UE3 Version", "UE4 Version" and "Licensee Version" (parsed in SerializePackageFileSummary4).
            // The value is used as some version for package header, and it's not changed frequently. We can't
            // expect these values to have large values in the future. The code below checks this value for
            // being less than zero, but allows UE1-UE3 LicenseeVersion up to 32767.
            if ((Version & 0xFFFFF000) == 0xFFFFF000)
            {
                LegacyVersion = Version;
                Ar.Game = UE4_BASE;
                Serialize4(r);
                //!! note: UE4 requires different DetectGame way, perhaps it's not possible at all (but can use PAK file names for game detection)
                return;
            }

            if (Version == TAG || Version == 0x20000) throw new Exception("Fully compressed package header?");

            FileVersion = (ushort)(Version & 0xFFFF);
            LicenseeVersion = (ushort)(Version >> 16);
            // store file version to archive (required for some structures, for UNREAL3 path)
            Ar.ArVer = FileVersion;
            Ar.ArLicenseeVer = LicenseeVersion;
            // detect game
            Ar.DetectGame();
            Ar.OverrideVersion();

            // read other fields

            if (Ar.Game >= UE3) Serialize3(r);
            else Serialize2(r);

            Console.WriteLine($"EngVer:{EngineVersion} CookVer:{CookerVersion} CompF:{CompressionFlags} CompCh:{CompressedChunks?.Length}");
            Console.WriteLine($"Names:{NameOffset}[{NameCount}] Exports:{ExportOffset}[{ExportCount}] Imports:{ImportOffset}[{ImportCount}]");
            Console.WriteLine($"HeadersSize:{HeadersSize} Group:{PackageGroup} DependsOffset:{DependsOffset} U60:{U3unk60}");

            return;
        }
    }

    public partial class FObjectExport
    {
        public int ClassIndex;              // object reference
        public int PackageIndex;            // object reference
        public FName ObjectName;
        public int SerialSize;
        public int SerialOffset;
        //UObject Object;                     // not serialized, filled by object loader
#if !USE_COMPACT_PACKAGE_STRUCTS
        int SuperIndex;                     // object reference
        uint ObjectFlags;
#endif
        public uint ExportFlags;            // EF_* flags
#if !USE_COMPACT_PACKAGE_STRUCTS
        uint ObjectFlags2;                  // really, 'uint64 ObjectFlags'
        int Archetype;
        //TMap<FName, int> ComponentMap;	-- this field was removed from UE3, so serialize it as a temporary variable when needed
        int[] NetObjectCount;               // generations
        Guid Guid;
        int PackageFlags;
        int U3unk6C;
        int TemplateIndex;                  // UE4
#endif
        // In UE4.26 IoStore package structure is different, 'ClassIndex' is replaced with global
        // script object index.
        public string ClassName_IO;
        // IoStore has reordered objects, but preserves "CookedSerialOffset" in export table.
        // We need to serialize data from the "read" offset, but set up the loader so it will
        // think that offset is like in original package.
        uint RealSerialOffset;

        public FObjectExport(BinaryReader r, UPackage Ar)
        {
            if (Ar.Game >= UE4_BASE) Serialize4(r, Ar);
            else if (Ar.Game >= UE3) Serialize3(r, Ar);
            else if (Ar.Engine == UE2X) Serialize2X(r, Ar);
            else Serialize2(r, Ar);
        }
        public override string ToString() => $"'{ObjectName}' offs={SerialOffset:08X} size={SerialSize:08X} parent={PackageIndex} exp_f={ExportFlags:08X}";
    }

    public partial class FObjectImport
    {
#if !USE_COMPACT_PACKAGE_STRUCTS
        FName ClassPackage;
#endif
        public FName ClassName;
        int PackageIndex;
        public FName ObjectName;
        bool Missing;                   // not serialized

        public FObjectImport(BinaryReader r, UPackage Ar)
        {
#if USE_COMPACT_PACKAGE_STRUCTS
            FName ClassPackage;
#endif
            if (Ar.Engine == UE2X && Ar.ArVer >= 150)
            {
                ClassPackage = new FName(r, Ar);
                ClassName = new FName(r, Ar);
                PackageIndex = r.ReadUInt16();
                ObjectName = new FName(r, Ar);
                return;
            }
            if (Ar.Game == Pariah)
            {
                PackageIndex = r.ReadInt32();
                ObjectName = new FName(r, Ar);
                ClassPackage = new FName(r, Ar);
                ClassName = new FName(r, Ar);
                return;
            }
            if (Ar.Game == AA2)
            {
                ClassPackage = new FName(r, Ar);
                ClassName = new FName(r, Ar);
                r.Skip(sizeof(byte)); // serialized length of ClassName string?
                ObjectName = new FName(r, Ar);
                PackageIndex = r.ReadInt32();
                return;
            }

            // this code is the same for all engine versions
            ClassPackage = new FName(r, Ar);
            ClassName = new FName(r, Ar);
            PackageIndex = r.ReadInt32();
            ObjectName = new FName(r, Ar);

            if (Ar.Game >= UE4_BASE && Ar.ArVer >= (int)VERUE4.NON_OUTER_PACKAGE_IMPORT && Ar.ContainsEditorData) { var PackageName = new FName(r, Ar); }
            if (Ar.Game == MK && Ar.ArVer >= 677) r.Skip(16); // MK X
        }

        public override string ToString() => $"{ClassName}'{ObjectName}'";
    }

    partial class UPackage
    {
        int GameSetCount;
        public void SetGame(Game value) { Game = value; GameSetCount++; }
        void CheckGameCollision()
        {
            if (GameSetCount > 1) throw new Exception($"DetectGame collision: detected {GameSetCount} titles, Ver={ArVer}, LicVer={ArLicenseeVer}");
        }

        public Game Engine => (Game)((int)Game & GAME_ENGINE);

        public bool GameUsesFCompactIndex =>
            Engine >= UE3 ? false
            : Engine == UE2X && ArVer >= 145 ? false
            : Game == Vanguard && ArVer >= 128 && ArLicenseeVer >= 25 ? false
            : true;

        public bool ContainsEditorData =>
            Game < UE4_BASE ? false
            : Summary.IsUnversioned ? false     // unversioned packages definitely has no editor data
            : (Summary.PackageFlags & PKG_FilterEditorOnly) != 0 ? false
            : true;

        public string GetName(int index)
        {
            if ((uint)index >= Summary.NameCount) throw new ArgumentOutOfRangeException(nameof(index), $"wrong name index {index}");
            return Names[index];
        }

        FObjectImport GetImport(int index)
        {
            if ((uint)index >= Summary.ImportCount) throw new ArgumentOutOfRangeException(nameof(index), $"wrong import index {index}");
            return Imports[index];
        }

        FObjectExport GetExport(int index)
        {
            if ((uint)index >= Summary.ExportCount) throw new ArgumentOutOfRangeException(nameof(index), $"wrong export index {index}");
            return Exports[index];
        }

        string GetObjectName(int PackageIndex)
            => PackageIndex < 0 ? GetImport(-PackageIndex - 1).ObjectName
            : PackageIndex > 0 ? GetExport(PackageIndex - 1).ObjectName
            : "Class";

        public string GetClassNameFor(FObjectExport exp) => exp.ClassName_IO ?? GetObjectName(exp.ClassIndex); // Allow to explicitly provide the class name
    }
}
