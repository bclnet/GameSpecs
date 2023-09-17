using System;
using System.IO;
using static GameSpec.Unreal.Formats.Core2.Game;
namespace GameSpec.Unreal.Formats.Core2
{
    struct FGenerationInfo
    {
        int ExportCount;
        int NameCount;
        int NetObjectCount;
        void Do(BinaryReader ar)
        {
            //    Ar << I.ExportCount << I.NameCount;
            //    if (Ar.Game >= GAME_UE4_BASE && Ar.ArVer >= 196 /*VER_UE4_REMOVE_NET_INDEX*/) return Ar;
            //    if (Ar.ArVer >= 322) Ar << I.NetObjectCount; // PACKAGE_V3
        }
    }

    struct FCompressedChunk
    {
        int UncompressedOffset;
        int UncompressedSize;
        int CompressedOffset;
        int CompressedSize;

        void Do(BinaryReader ar)
        {
            //    if ((Ar.Game == GAME_MK && Ar.ArVer >= 677) || (Ar.Game == GAME_RocketLeague && Ar.ArLicenseeVer >= 22))
            //    {
            //        // MK X and Rocket League has 64-bit file offsets
            //        int64 UncompressedOffset64, CompressedOffset64;
            //        Ar << UncompressedOffset64 << C.UncompressedSize << CompressedOffset64 << C.CompressedSize;
            //        C.UncompressedOffset = (int)UncompressedOffset64;
            //        C.CompressedOffset = (int)CompressedOffset64;
            //        return Ar;
            //    }

            //    Ar << C.UncompressedOffset << C.UncompressedSize << C.CompressedOffset << C.CompressedSize;

            //    if (Ar.Game == GAME_Bulletstorm && Ar.ArLicenseeVer >= 21)
            //    {
            //        int32 unk10;        // unused? could be 0 or 1
            //        Ar << unk10;
            //    }
            //    return Ar;
        }
    }

    struct FEngineVersion
    {
        ushort Major, Minor, Patch;
        int Changelist;
        string Branch;
        void Do(BinaryReader ar)
        {
            //return Ar << V.Major << V.Minor << V.Patch << V.Changelist << V.Branch;
        }
    }

    struct FCustomVersion
    {
        Guid Key;
        int Version;
        void Do(BinaryReader ar)
        {
            //return Ar << V.Key << V.Version;
        }
    }

    struct FCustomVersionContainer
    {
        FCustomVersion[] Versions;
        //void Serialize(FArchive& Ar, int LegacyVersion);
    }

    class FPackageFileSummary
    {
        Game Game;               // EGame
        Platform Platform;			// EPlatform
        Game GForceGame = UNKNOWN;

        const uint TAG = 0x9e2a83c1;
        const uint TAG_REV = 0xc1832a9e;

        uint Tag;
        uint LegacyVersion;
        bool IsUnversioned;
        FCustomVersionContainer CustomVersionContainer;
        ushort FileVersion;
        ushort LicenseeVersion;
        int PackageFlags;
        int NameCount, NameOffset;
        int ExportCount, ExportOffset;
        int ImportCount, ImportOffset;
        Guid Guid;
        FGenerationInfo[] Generations;
        int HeadersSize;        // used by UE3 for precaching name table
        string PackageGroup;       // "None" or directory name
        int DependsOffset;      // number of items = ExportCount
        int f38;
        int f3C;
        int f40;
        int EngineVersion;
        int CookerVersion;
        int CompressionFlags;
        FCompressedChunk[] CompressedChunks;
        int U3unk60;
        long BulkDataStartOffset;

        public FPackageFileSummary(BinaryReader r)
        {
            // read package tag
            Tag = r.ReadUInt32();
            switch (Tag) // some games has special tag constants
            {
                case TAG: break;
                case 0x9E2A83C2:   // Killing Floor
                case 0x7E4A8BCA:  // iStorm
                case 0xA94E6C81: break; // Nurien
                case 0xA1B2C93F: Game = BattleTerr; break;  // Battle Territory Online
                case 0xD58C3147: Game = Loco; break;  // Land of Chaos Online
                case 0xF2BAC156: Game = Berkanix; break;  // BERKANIX
                case 0xEA31928C: Game = Hawken; break;  // HAWKEN
                case 0x12345678: r.Skip(4); Game = TaoYuan; if (GForceGame == 0) GForceGame = TaoYuan; break;  // TAO_YUAN
                case 0xEC201133: var count = r.ReadByte(); r.Skip(count); break;  // STORMWAR
                case 0x879A4B41: Game = GunLegend; if (GForceGame == 0) GForceGame = GunLegend; break;  // GUNLEGEND
                case 0x4D4D4837: Game = UE3; break;  // Might & Magic Heroes 7 - version conflict with Guilty Gear Xrd
                case 0x7BC342F0: Game = DevilsThird; if (GForceGame == 0) GForceGame = DevilsThird; break;  // Devil's Third
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
                Game = UE4_BASE;
                Serialize4(r);
                //!! note: UE4 requires different DetectGame way, perhaps it's not possible at all (but can use PAK file names for game detection)
                return;
            }

            if (Version == TAG || Version == 0x20000)
                throw new Exception("Fully compressed package header?");

            FileVersion = (ushort)(Version & 0xFFFF);
            LicenseeVersion = (ushort)(Version >> 16);
            var ar = new FArchive();
            // store file version to archive (required for some structures, for UNREAL3 path)
            ar.ArVer = FileVersion;
            ar.ArLicenseeVer = LicenseeVersion;
            // detect game
            ar.DetectGame();
            //ar.OverrideVersion();

            // read other fields

            if (Game >= UE3) Serialize3(r);
            else Serialize2(r);

            Console.WriteLine($"EngVer:{EngineVersion} CookVer:{CookerVersion} CompF:{CompressionFlags} CompCh:{CompressedChunks.Length}");
            Console.WriteLine($"Names:{NameOffset}[{NameCount}] Exports:{ExportOffset}[{ExportCount}] Imports:{ImportOffset}[{ImportCount}]");
            Console.WriteLine($"HeadersSize:{HeadersSize} Group:{PackageGroup} DependsOffset:{DependsOffset} U60:{U3unk60}");

            return;
        }

        // Engine-specific serializers
        void Serialize2(BinaryReader r) { }
        void Serialize3(BinaryReader r) { }
        void Serialize4(BinaryReader r) { }
    }
}