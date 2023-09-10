using GameSpec.Unreal.Formats.Core;
using OpenStack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using static GameSpec.Unreal.Formats.Core.UPackage.Gen;

// https://eliotvu.com/page/unreal-package-file-format
// https://www.acordero.org/projects/
// https://paulbourke.net/dataformats/unreal/
namespace GameSpec.Unreal.Formats.Core
{
    [Flags]
    public enum PackageFlags : uint
    {
        // 028A0009 : A cooked and compressed package
        // 00280009 : A cooked package
        // 00020001 : A ordinary package

        /// <summary>
        /// Whether clients are allowed to download the package from the server.
        /// </summary>
        AllowDownload = 0x00000001U,

        /// <summary>
        /// Whether clients can skip downloading the package but still able to join the server.
        /// </summary>
        ClientOptional = 0x00000002U,

        /// <summary>
        /// Only necessary to load on the server.
        /// </summary>
        ServerSideOnly = 0x00000004U,

        BrokenLinks = 0x00000008U,      // @Redefined(UE3, Cooked)

        /// <summary>
        /// The package is cooked.
        /// </summary>
        Cooked = 0x00000008U,      // @Redefined

        /// <summary>
        /// ???
        /// <= UT
        /// </summary>
        Unsecure = 0x00000010U,

        /// <summary>
        /// The package is encrypted.
        /// <= UT
        /// </summary>
        Encrypted = 0x00000020U,

        /// <summary>
        /// Clients must download the package.
        /// </summary>
        Need = 0x00008000U,

        /// <summary>
        /// Unknown flags
        /// -   0x20000000  -- Probably means the package contains Content(Meshes, Textures)
        /// </summary>
        ///

        /// Package holds map data.
        Map = 0x00020000U,

        /// <summary>
        /// Package contains classes.
        /// </summary>
        Script = 0x00200000U,

        /// <summary>
        /// The package was build with -Debug
        /// </summary>
        Debug = 0x00400000U,
        Imports = 0x00800000U,

        Compressed = 0x02000000U,
        FullyCompressed = 0x04000000U,

        /// <summary>
        /// Whether package has metadata exported(anything related to the editor).
        /// </summary>
        NoExportsData = 0x20000000U,

        /// <summary>
        /// Package's source is stripped.
        /// </summary>
        Stripped = 0x40000000U,

        Protected = 0x80000000U,
    }

    public class UPackage
    {
        const int VSizePrefixDeprecated = 64;
        internal const int VIndexDeprecated = 178;
        const int VCookedPackages = 277;
        const int VEngineVersion = 245;
        const int VHeaderSize = 249;
        const int VGroup = 269;
        const int VCompression = 334;
        const int VPackageSource = 482;
        const int VAdditionalPackagesToCook = 516;
        const int VTextureAllocations = 767;
        const int VDependsOffset = 415;
        const int VImportExportGuidsOffset = 623;
        const int VThumbnailTableOffset = 584;

        public enum Gen
        {
            UE1, // Unreal Engine 1
            UE2, // Unreal Engine 2
            Thief, // Heavily modified Unreal Engine 2 by Ion Storm for Thief: Deadly Shadows
            UE2_5, // Unreal Engine 2 with some early UE3 upgrades.
            Vengeance, // Heavily modified Unreal Engine 2.5 for Vengeance: Tribes; also used by Swat4 and BioShock.
            Lead, // Heavily modified Unreal Engine 2.5 for Splinter Cell
            UE2X, // Modified Unreal Engine 2 for Xbox e.g. Unreal Championship 2: The Liandri Conflict
            UE3, // Unreal Engine 3
            RSS, // Rocksteady Studios - Heavily modified Unreal Engine 3 for the Arkham series
            HMS, // High Moon Studios - Heavily modified Unreal Engine 3 for Transformers and Deadpool etc
            UE4 // Unreal Engine 4
        }

        [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
        public class BuildAttribute : Attribute
        {
            readonly Range Version;
            readonly Range Licensee;
            public readonly Gen Gen;
            public BuildAttribute(int version, int licensee, Gen gen = 0) { Version = version..version; Licensee = licensee..licensee; Gen = gen; }
            public BuildAttribute(int minVersion, int maxVersion, int minLicensee, int maxLicensee, Gen gen = 0) { Version = minVersion..maxVersion; Licensee = minLicensee..maxLicensee; Gen = gen; }
            public bool Match(int version, int licensee) => version >= Version.Start.Value && version <= Version.End.Value && licensee >= Licensee.Start.Value && licensee <= Licensee.End.Value;

            public static (BuildName, BuildAttribute) Find(int version, int licenseeVersion)
            {
                foreach (var build in typeof(BuildName).GetFields())
                {
                    var s = build.GetCustomAttributes<BuildAttribute>(false).FirstOrDefault(x => x.Match(version, licenseeVersion));
                    if (s == null) continue;
                    return ((BuildName)Enum.Parse(typeof(BuildName), build.Name), s);
                }
                return (BuildName.Unknown, default);
            }
        }

        public enum BuildName
        {
            Unknown,
            [Build(061, 000)] Unreal1,                  // [061/000] Unreal1
            [Build(068, 069, 0, 0)] UT,                 // [068:069/000] Standard, Unreal Tournament & Deus Ex
            [Build(095, 069, Thief)] DeusEx_IW,         // [095/069+Thief] Deus Ex: Invisible War - Missing support for custom classes such as BitfieldProperty and BitfieldEnum among others.
            [Build(095, 133, Thief)] Thief_DS,          // [095/133+Thief] Thief: Deadly Shadows
            [Build(099, 117, 005, 008)] UT2003,         // [099:117/005:008] UT2003 - Latest patch? Same structure as UT2004's UE2.5
            [Build(100, 058)] XIII,                     // [100/058] XIII
            [Build(110, 2609)] Unreal2,                 // [110/2609] Unreal2
            [Build(118, 118, 011, 014)] R6RS,           // [118:118/011:014] Tom Clancy's Rainbow Six 3: Raven Shield
            [Build(126, 000)] Unreal2XMP,               // [126/000] Unreal II: eXpanded MultiPlayer
            [Build(118, 128, 025, 029, UE2_5)] UT2004,  // [118:128/025:029+UE2_5] UT2004 (Overlaps latest UT2003)
            [Build(128, 128, 032, 033, UE2_5)] AA2,     // [128:128/032:033+UE2_5] AA2 - Represents both AAO and AAA - Built on UT2004
                                                        // 129:143/027:059| IrrationalGames/Vengeance
            [Build(130, 027, Vengeance)] Tribes_VG,     // [130/027+Vengeance] Tribes: Vengeance
            [Build(129, 027, Vengeance)] Swat4,         // [129/027+Vengeance] Swat4
            [Build(130, 143, 056, 059, Vengeance)] BioShock,  // [130:143/056:059+Vengeance] BioShock 1 & 2
            [Build(159, 029, UE2_5)] Spellborn,         // [159/029+UE2_5] The Chronicles of Spellborn - Built on UT2004 - Comes with several new non-standard UnrealScript features, these are however not supported.
            [Build(369, 006)] RoboBlitz,                // [369/006] RoboBlitz
            [Build(421, 011)] MOHA,                     // [421/011] MOHA
            [Build(472, 046)] MKKE,                     // [472/046+ConsoleCooked] MKKE
            [Build(490, 009)] GoW1,                     // [490/009] GoW1
            [Build(512, 000)] UT3,                      // [512/000] UT3
            [Build(536, 043)] MirrorsEdge,              // [536/043] MirrorsEdge
            [Build(539, 091)] AlphaProtcol,             // [539/091] AlphaProtcol
            [Build(547, 547, 028, 032)] APB,            // [547:547/028:032] APB
            [Build(575, 000)] GoW2,                     // [575/000+XenonCooked] GoW2 - Xenon is enabled here, because the package is missing editor data, the editor data of UStruct is however still serialized.
            [Build(576, 005)] CrimeCraft,               // [576/005] CrimeCraft
            [Build(576, 021)] Batman1,                  // [576/021] Batman1 - No Special support, but there's no harm in recognizing this build.
            [Build(576, 100)] Homefront,                // [576/100] Homefront
            [Build(581, 058)] MOH,                      // [581/058+ConsoleCooked] Medal of Honor (2010) - Windows, PS3, Xbox 360 Defaulting to ConsoleCooked. XenonCooked is required to read the Xbox 360 packages.
            [Build(584, 058)] Borderlands,              // [584/058] Borderlands
            [Build(584, 126)] Singularity,              // [584/126] Singularity
            [Build(590, 001)] ShadowComplex,            // [590/001+XenonCooked] ShadowComplex
            [Build(610, 014)] Tera,                     // [610/014] Tera
            [Build(648, 6405)] DCUO,                    // [648/6405] DCUO
            [Build(687, 111)] DungeonDefenders2,        // [687/111] DungeonDefenders2
            [Build(727, 075)] Bioshock_Infinite,        // [727/075] Bioshock_Infinite
            [Build(742, 029)] BulletStorm,              // [742/029+ConsoleCooked] BulletStorm
            [Build(801, 030)] Dishonored,               // [801/030] Dishonored
            [Build(788, 001)]                           // [788/001+ConsoleCooked] InfinityBlade
            [Build(828, 000)] InfinityBlade,            // [828/000+ConsoleCooked] ...
            [Build(828, 000)] GoW3,                     // [828/000+ConsoleCooked] GoW3 - ever reached?
            [Build(832, 021)] RememberMe,               // [832/021] RememberMe
            [Build(832, 046)] Borderlands2,             // [832/046] Borderlands2
            [Build(842, 001)] InfinityBlade2,           // [842/001+ConsoleCooked] InfinityBlade2
            [Build(845, 059)] XCOM_EU,                  // [845/059] XCom
            [Build(845, 120)] XCOM2WotC,                // [845/120] XCom 2: War of The Chosen
            [Build(511, 039)]                           // [511/039] The Bourne Conspiracy
            [Build(511, 145)]                           // [511/145] Transformers: War for Cybertron (PC version)
            [Build(511, 144)]                           // [511/144] Transformers: War for Cybertron (PS3 and XBox 360 version)
            [Build(537, 174)]                           // [537/174] Transformers: Dark of the Moon
            [Build(846, 181, 002, 001)] Transformers,   // [846:181/002:001] Transformers: Fall of Cybertron // FIXME: The serialized version is false, needs to be adjusted.
            [Build(860, 004)] Hawken,                   // [860/004] Hawken
            [Build(805, 101, RSS)] Batman2,             // [805/101+RSS] Batman: Arkham City
            [Build(806, 103, RSS)]                      // [806/103+RSS] Batman: Arkham Origins
            [Build(807, 807, 137, 138, RSS)] Batman3,   // [807:807/137:138+RSS] ...
            [Build(807, 104, RSS)] Batman3MP,           // [807/104+RSS] Batman3MP
            [Build(863, 32995, RSS)] Batman4,           // [863/32995+RSS] Batman: Arkham Knight [OverridePackageVersion(863, 227)]
            [Build(867, 868, 009, 032)] RocketLeague,   // [867:868/009:032] RocketLeague - Requires third-party decompression and decryption
            [Build(904, 904, 009, 014)] SpecialForce2,  // [904:904/009:014] SpecialForce2
        }

        public struct UGeneration
        {
            const int VNetObjectsCount = 322;
            public int ExportsCount;
            public int NamesCount;
            public int NetObjectsCount;
            public UGeneration(BinaryReader r, int version)
            {
                ExportsCount = r.ReadInt32();
                NamesCount = r.ReadInt32();
                NetObjectsCount = version >= VNetObjectsCount ? r.ReadInt32() : 0;
            }
        }

        public class UName
        {
            /// <summary>
            /// Object Name
            /// </summary>
            public string Name = string.Empty;

            /// <summary>
            /// Object Flags, such as LoadForEdit, LoadForServer, LoadForClient
            /// </summary>
            /// <value>
            /// 32bit in UE2
            /// 64bit in UE3
            /// </value>
            public ulong Flags;

            public UName(BinaryReader r, BuildName build, BuildAttribute buildAttrib, int version, int licenseeVersion)
            {
                Name = DeserializeName(r, build, buildAttrib, version, licenseeVersion);
                Debug.Assert(Name.Length <= 1024, "Maximum name length exceeded! Possible corrupt or unsupported package.");
                if (build == BuildName.BioShock)
                {
                    Flags = r.ReadUInt64();
                    return;
                }
                Flags = version >= UExport.VObjectFlagsToUlong
                    ? r.ReadUInt64()
                    : r.ReadUInt32();


                //    var nameEntry = new UName { Offset = (int)stream.Position, Index = i };
                //    nameEntry.Deserialize(stream);
                //    nameEntry.Size = (int)(stream.Position - nameEntry.Offset);

            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            string DeserializeName(BinaryReader r, BuildName build, BuildAttribute buildAttrib, int version, int licenseeVersion)
            {
                // Very old packages use a simple Ansi encoding.
                if (version < VSizePrefixDeprecated) return r.ReadASCIIString();
                // Names are not encrypted in AAA/AAO 2.6 (LicenseeVersion 32)
                if (build == BuildName.AA2 && licenseeVersion >= 33 && Decoder is CryptoDecoderAA2)
                {
                    // Thanks to @gildor2, decryption code transpiled from https://github.com/gildor2/UEViewer, 
                    var length = r.ReadIndex(version); Debug.Assert(length < 0);
                    var size = -length;

                    const byte n = 5;
                    byte shift = n;
                    var buffer = new char[size];
                    for (var i = 0; i < size; i++)
                    {
                        var c = r.ReadUInt16();
                        ushort c2 = CryptoCore.RotateRight(c, shift);
                        Debug.Assert(c2 < byte.MaxValue);
                        buffer[i] = (char)(byte)c2;
                        shift = (byte)((c - n) & 0x0F);
                    }

                    var str = new string(buffer, 0, buffer.Length - 1);
                    // Part of name ?
                    var number = r.ReadIndex(version);
                    //Debug.Assert(number == 0, "Unknown value");
                    return str;
                }
                return r.ReadUString(version, buildAttrib);
            }
        }

        public class UImport
        {
            public UImport(BinaryReader r, int version)
            {
                //var imp = new UImport { Offset = (int)stream.Position, Index = i, Owner = this };
                //imp.Deserialize(stream);
                //imp.Size = (int)(stream.Position - imp.Offset);
            }
        }

        public class UExport
        {
            const int VArchetype = 220;
            internal const int VObjectFlagsToUlong = 195;
            const int VSerialSizeConditionless = 249;
            const int VNetObjects = 322;

            public UExport(BinaryReader r, int version)
            {
                //var exp = new UExport { Offset = (int)stream.Position, Index = i, Owner = this };
                //exp.Deserialize(stream);
                //exp.Size = (int)(stream.Position - exp.Offset);
            }
        }


        public struct CompressedChunk
        {
            public int UncompressedOffset;
            public int UncompressedSize;
            public int CompressedOffset;
            public int CompressedSize;
            public CompressedChunk(BinaryReader r, BuildName build, int licenseeVersion)
            {
                if (build == BuildName.RocketLeague && licenseeVersion >= 22)
                {
                    UncompressedOffset = (int)r.ReadInt64();
                    CompressedOffset = (int)r.ReadInt64();
                    goto streamStandardSize;
                }
                UncompressedOffset = r.ReadInt32();
                CompressedOffset = r.ReadInt32();
            streamStandardSize:
                UncompressedSize = r.ReadInt32();
                CompressedSize = r.ReadInt32();
            }
        }

        const uint MAGIC = 0x9e2a83c1;

        public uint Magic;
        public ushort Version; public ushort LicenseeVersion;
        public BuildName Build;
        public BuildAttribute BuildAttrib;
        public int HeaderSize; // Size of the Header. Basically points to the first Object in the package.
        public string Group; // The group the package is associated with in the Content Browser.
        public PackageFlags PackageFlags; // The bitflags of this package.

        // tables
        public int NamesCount;
        public int NamesOffset;
        public int ExportsCount;
        public int ExportsOffset;
        public int ImportsCount;
        public int ImportsOffset;
        public int DependsOffset;
        public int ImportExportGuidsOffset;
        public int ImportGuidsCount;
        public int ExportGuidsCount;
        public int ThumbnailTableOffset;
        //
        public IList<ushort> Heritages; // List of heritages. UE1 way of defining generations.
        public Guid Guid; // The guid of this package. Used to test if the package on a client is equal to the one on a server.
        public UGeneration[] Generations; // List of package generations.
        public int EngineVersion; // The Engine version the package was created with.
        public int CookerVersion; // The Cooker version the package was cooked with.
        public uint CompressionFlags; // The type of compression the package is compressed with.
        public CompressedChunk[] CompressedChunks; // List of compressed chunks throughout the package.

        public UName[] Names; // List of unique unreal names.
        public UExport[] Exports; // List of info about exported objects.
        public UImport[] Imports; // List of info about imported objects.

        public UPackage(BinaryReader r)
        {
            Magic = r.ReadUInt32();
            if (Magic != MAGIC) throw new FormatException("BAD MAGIC");
            var version = r.ReadUInt32(); Version = (ushort)(version & 0xFFFFU); LicenseeVersion = (ushort)(version >> 16); Debug.Log($"Package Version:{Version}/{LicenseeVersion}");
            (Build, BuildAttrib) = BuildAttribute.Find(Version, LicenseeVersion); Debug.Log($"Build: {Build}");
            if (Version >= VHeaderSize)
            {
                if (Build == BuildName.Bioshock_Infinite) r.Skip(4);
                else if (Build == BuildName.MKKE) r.Skip(8);
                else if (Build == BuildName.Transformers && LicenseeVersion >= 55)
                {
                    if (LicenseeVersion >= 181) r.Skip(16);
                    r.Skip(4);
                }
                HeaderSize = r.ReadInt32(); Debug.Log($"Header Size: {HeaderSize}");
            }
            if (Version >= VGroup) Group = r.ReadUString(Version, BuildAttrib); // UPK content category e.g. Weapons, Sounds or Meshes.
            PackageFlags = (PackageFlags)r.ReadUInt32(); Debug.Log($"Package Flags: {PackageFlags}"); // Bitflags such as AllowDownload.
            ReadTableCounts(r);
            ReadExtra(r);
            ReadTableData(r);
        }

        void ReadTableCounts(BinaryReader r)
        {
            if (Build == BuildName.Hawken && LicenseeVersion >= 2) r.Skip(4);
            NamesCount = r.ReadInt32();
            NamesOffset = r.ReadInt32();
            ExportsCount = r.ReadInt32();
            ExportsOffset = r.ReadInt32();
            if (Build == BuildName.APB && LicenseeVersion >= 28)
            {
                if (LicenseeVersion >= 29) r.Skip(4);
                r.Skip(20);
            }
            ImportsCount = r.ReadInt32();
            ImportsOffset = r.ReadInt32();
            Debug.Log($"Names Count: {NamesCount}, Names Offset: {NamesOffset}, Exports Count: {ExportsCount}, Exports Offset: {ExportsOffset}, Imports Count: {ImportsCount}, Imports Offset: {ImportsOffset}");
            if (Version >= VDependsOffset) DependsOffset = r.ReadInt32();
            if (Build == BuildName.Transformers && Version < 535) return;
            if (Version >= VImportExportGuidsOffset && Build != BuildName.Bioshock_Infinite && Build != BuildName.Transformers) // FIXME: Correct the output version of these games instead.
            {
                ImportExportGuidsOffset = r.ReadInt32();
                ImportGuidsCount = r.ReadInt32();
                ExportGuidsCount = r.ReadInt32();
            }
            if (Version >= VThumbnailTableOffset)
            {
                if (Build == BuildName.DungeonDefenders2) r.Skip(4);
                ThumbnailTableOffset = r.ReadInt32();
            }
        }

        void ReadExtra(BinaryReader r)
        {
            if (Version < 68)
            {
                var heritageCount = r.ReadInt32();
                var heritageOffset = r.ReadInt32();
                r.Seek(heritageOffset, SeekOrigin.Begin);
                Heritages = new List<ushort>(heritageCount);
                for (var i = 0; i < heritageCount; ++i) Heritages.Add(r.ReadUInt16());
            }
            else
            {
                if (Build == BuildName.Thief_DS || Build == BuildName.DeusEx_IW) r.Skip(4);
                else if (Build == BuildName.Borderlands) r.Skip(4);
                else if (Build == BuildName.MKKE) r.Skip(4);
                else if (Build == BuildName.Spellborn && Version >= 148) goto skipGuid;
                Guid = r.ReadGuid(); Debug.Log($"Guid: {Guid}");
            skipGuid:
                if (Build == BuildName.Tera) r.Skip(-4);
                if (Build != BuildName.MKKE)
                {
                    var generationCount = r.ReadInt32(); Debug.Log($"Generations Count: {generationCount}");
                    // Guid, however only serialized for the first generation item.
                    if (Build == BuildName.APB && LicenseeVersion >= 32) r.Skip(16);
                    Generations = r.ReadTArray(r => new UGeneration(r, Version), generationCount);
                }
                if (Version >= VEngineVersion) { EngineVersion = r.ReadInt32(); Debug.Log($"EngineVersion: {EngineVersion}"); } // The Engine Version this package was created with
                if (Version >= VCookedPackages) { CookerVersion = r.ReadInt32(); Debug.Log($"CookerVersion: {CookerVersion}"); } // The Cooker Version this package was cooked with

                // Read compressed info?
                if (Version >= VCompression)
                {
                    CompressionFlags = r.ReadUInt32(); Debug.Log($"CompressionFlags: {CompressionFlags}");
                    CompressedChunks = r.ReadTArray(r => new CompressedChunk(r, Build, LicenseeVersion), r.ReadIndex(Version));
                }

                if (Version >= VPackageSource) { var packageSource = r.ReadUInt32(); Debug.Log($"PackageSource: {packageSource}"); }

                if (Version >= VAdditionalPackagesToCook)
                {
                    var additionalPackagesToCook = r.ReadTArray(r => r.ReadUString(Version, BuildAttrib), r.ReadIndex(Version));
                    if (Build == BuildName.DCUO)
                    {
                        var realNameOffset = (int)r.BaseStream.Position;
                        Debug.Assert(realNameOffset <= NamesOffset, "realNameOffset is > the parsed name offset for a DCUO package, we don't know where to go now!");

                        var offsetDif = NamesOffset - realNameOffset;
                        NamesOffset -= offsetDif;
                        ImportsOffset -= offsetDif;
                        ExportsOffset -= offsetDif;
                        DependsOffset = 0; // not working
                        ImportExportGuidsOffset -= offsetDif;
                        ThumbnailTableOffset -= offsetDif;
                    }
                }

                if (Version >= VTextureAllocations)
                {
                    // TextureAllocations, TextureTypes
                    var count = r.ReadInt32();
                    for (var i = 0; i < count; i++)
                    {
                        r.ReadInt32();
                        r.ReadInt32();
                        r.ReadInt32();
                        r.ReadUInt32();
                        r.ReadUInt32();
                        int count2 = r.ReadInt32();
                        r.Skip(count2 * 4);
                    }
                }
            }
            if (Build == BuildName.RocketLeague && (PackageFlags & PackageFlags.Cooked) != 0 && Version >= VCookedPackages)
            {
                var garbageSize = r.ReadInt32(); Debug.Log($"GarbageSize: {garbageSize}");
                var compressedChunkInfoOffset = r.ReadInt32(); Debug.Log($"CompressedChunkInfoOffset: {compressedChunkInfoOffset}");
                var lastBlockSize = r.ReadInt32(); Debug.Log($"LastBlockSize: {lastBlockSize}");
                Debug.Assert(r.BaseStream.Position == NamesOffset, "There is more data before the NameTable");
                // Data after this is encrypted
            }
            // We can't continue without decompressing.
            if (CompressionFlags != 0 || (CompressedChunks != null && CompressedChunks.Any()))
            {
                // HACK: To fool UE Explorer
                //if (CompressedChunks.Capacity == 0) CompressedChunks.Capacity = 1;
                return;
            }
            // Note: Never true, AA2 is not a detected build for packages with LicenseeVersion 27 or less But we'll preserve this nonetheless
            if (Build == BuildName.AA2 && LicenseeVersion >= 19)
            {
                var isEncrypted = r.ReadInt32() > 0;
                if (isEncrypted)
                {
                    // TODO: Use a stream wrapper instead; but this is blocked by an overly intertwined use of PackageStream.
                    //if (LicenseeVersion >= 33) Decoder = new CryptoDecoderAA2();
                    //else
                    //{
                    //    var nonePosition = NamesOffset;
                    //    r.Seek(nonePosition, SeekOrigin.Begin);
                    //    var scrambledNoneLength = r.ReadByte();
                    //    var decoderKey = scrambledNoneLength;
                    //    r.Seek(nonePosition, SeekOrigin.Begin);
                    //    var unscrambledNoneLength = r.ReadByte();
                    //    Debug.Assert((unscrambledNoneLength & 0x3F) == 5);
                    //    Decoder = new CryptoDecoderWithKeyAA2(decoderKey);
                    //}
                }

                // Always one
                //int unkCount = stream.ReadInt32();
                //for (var i = 0; i < unkCount; i++)
                //{
                //    // All zero
                //    stream.Skip(24);
                //    // Always identical to the package's GUID
                //    var guid = stream.ReadGuid();
                //}

                //// Always one
                //int unk2Count = stream.ReadInt32();
                //for (var i = 0; i < unk2Count; i++)
                //{
                //    // All zero
                //    stream.Skip(12);
                //}
            }
        }

        void ReadTableData(BinaryReader r)
        {
            // Read the name table
            if (Build == BuildName.Tera) NamesCount = Generations.Last().NamesCount;
            if (NamesCount > 0)
            {
                r.Seek(NamesOffset, SeekOrigin.Begin);
                Names = r.ReadTArray(r => new UName(r, Build, BuildAttrib, Version, LicenseeVersion), NamesCount);
                if (Build == BuildName.Spellborn && Names[0].Name == "DRFORTHEWIN") Names[0].Name = "None";
            }

            // Read Import Table
            if (ImportsCount > 0)
            {
                r.Seek(ImportsOffset, SeekOrigin.Begin);
                Imports = r.ReadTArray(r => new UImport(r, Version), ImportsCount);
            }

            // Read Export Table
            if (ExportsCount > 0)
            {
                r.Seek(ExportsOffset, SeekOrigin.Begin);
                Exports = r.ReadTArray(r => new UExport(r, Version), ImportsCount);
                if (DependsOffset > 0)
                {
                    try
                    {
                        r.Seek(DependsOffset, SeekOrigin.Begin);
                        var dependsCount = ExportsCount;
                        // FIXME: Version?
                        if (Build == BuildName.Bioshock_Infinite) dependsCount = r.ReadInt32();
                        var dependsMap = new List<int[]>(dependsCount);
                        for (var i = 0; i < dependsCount; ++i)
                        {
                            // DependencyList, index to import table
                            var count = r.ReadInt32(); // -1 in DCUO?
                            var imports = new int[count];
                            for (var j = 0; j < count; ++j) imports[j] = r.ReadInt32();
                            dependsMap.Add(imports);
                        }
                    }
                    catch (Exception ex)
                    {
                        // Errors shouldn't be fatal here because this feature is not necessary for our purposes.
                        Console.Error.WriteLine("Couldn't parse DependenciesTable");
                        Console.Error.WriteLine(ex.ToString());
#if STRICT
                        throw new UnrealException("Couldn't parse DependenciesTable", ex);
#endif
                    }
                }
            }

            if (ImportExportGuidsOffset > 0)
            {
                try
                {
                    for (var i = 0; i < ImportGuidsCount; ++i)
                    {
                        var levelName = r.ReadUString(Version, BuildAttrib);
                        var guidCount = r.ReadInt32();
                        r.Skip(guidCount * 16);
                    }
                    for (var i = 0; i < ExportGuidsCount; ++i)
                    {
                        var objectGuid = r.ReadGuid();
                        var exportIndex = r.ReadInt32();
                    }
                }
                catch (Exception ex)
                {
                    // Errors shouldn't be fatal here because this feature is not necessary for our purposes.
                    Console.Error.WriteLine("Couldn't parse ImportExportGuidsTable");
                    Console.Error.WriteLine(ex.ToString());
#if STRICT
                        throw new UnrealException("Couldn't parse ImportExportGuidsTable", ex);
#endif
                }
            }

            if (ThumbnailTableOffset != 0)
            {
                try
                {
                    var thumbnailCount = r.ReadInt32();
                }
                catch (Exception ex)
                {
                    // Errors shouldn't be fatal here because this feature is not necessary for our purposes.
                    Console.Error.WriteLine("Couldn't parse ThumbnailTable");
                    Console.Error.WriteLine(ex.ToString());
#if STRICT
                    throw new UnrealException("Couldn't parse ThumbnailTable", ex);
#endif
                }
            }

            Debug.Assert(r.BaseStream.Position <= int.MaxValue);
            HeaderSize = (int)r.BaseStream.Position;
        }
    }
}