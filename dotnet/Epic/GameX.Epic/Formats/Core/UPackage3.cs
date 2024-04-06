using System;
using System.Diagnostics;
using System.IO;
using static GameX.Epic.Formats.Core.Game;
using static GameX.Epic.Formats.Core.UPackage;

namespace GameX.Epic.Formats.Core
{
    partial class FPackageFileSummary
    {
        // Engine-specific serializers
        void Serialize3(BinaryReader r)
        {
            if (Ar.Game == Batman4) { Ar.ArLicenseeVer &= 0x7FFF; LicenseeVersion &= 0x7FFF; } // higher bit is used for something else, and it's set to 1
            else if (Ar.Game == R6Vegas2)
            {
                if (Ar.ArLicenseeVer >= 48) r.Skip(sizeof(int));
                if (Ar.ArLicenseeVer >= 49) r.Skip(sizeof(int));
            }
            else if (Ar.Game == Huxley && Ar.ArLicenseeVer >= 8)
            {
                r.Skip(sizeof(int)); // 0xFEFEFEFE
                if (Ar.ArLicenseeVer >= 17) r.Skip(sizeof(int)); // unknown used field
            }
            else if (Ar.Game == Transformers)
            {
                if (Ar.ArLicenseeVer >= 181) r.Skip(sizeof(int) * 4);
                if (Ar.ArLicenseeVer >= 55) r.Skip(sizeof(int)); // always 0x4BF1EB6B? (not true for later game versions)
            }
            else if (Ar.Game == MortalOnline && Ar.ArLicenseeVer >= 1) r.Skip(sizeof(int));
            else if (Ar.Game == Bioshock3 && Ar.ArLicenseeVer >= 66) r.Skip(sizeof(int));

            HeadersSize = Ar.ArVer >= 249 ? r.ReadInt32() : 0;

            // NOTE: A51 and MKVSDC has exactly the same code paths!
            var midwayVer = 0;
            if (Ar.Engine == MIDWAY3 && Ar.ArLicenseeVer >= 2)    //?? Wheelman not checked
            {
                // Tag == "A52 ", "MK8 ", "MK  ", "WMAN", "WOO " (Stranglehold), "EPIC", "TNA ", "KORE", "MK10"
                var tag = r.ReadUInt32();
                midwayVer = r.ReadInt32();
                if (Ar.Game == Strangle && midwayVer >= 256) r.Skip(sizeof(int));
                if (Ar.Game == MK && Ar.ArVer >= 668) r.Skip(sizeof(int)); // MK X
                if (Ar.Game == MK && Ar.ArVer >= 596) r.Skip(16);
            }

            if (Ar.ArVer >= 269) PackageGroup = r.ReadFString(Ar);

            PackageFlags = r.ReadInt32();

            if ((((Ar.Game == MassEffect3 || Ar.Game == MassEffectLE) && Ar.ArLicenseeVer >= 194)) && (PackageFlags & PKG_Cooked) != 0) r.Skip(sizeof(int)); // ME3 or ME3LE
            else if (Ar.Game == Hawken && Ar.ArLicenseeVer >= 2) r.Skip(sizeof(int));
            else if (Ar.Game == Gigantic && Ar.ArLicenseeVer >= 2) r.Skip(sizeof(int));
            else if (Ar.Game == MK && Ar.ArVer >= 677)
            {
                // MK X, no explicit version
                NameCount = r.ReadUInt32();
                var NameOffset64 = r.ReadUInt64();
                ExportCount = r.ReadUInt32();
                var ExportOffset64 = r.ReadUInt64();
                ImportCount = r.ReadUInt32();
                var ImportOffset64 = r.ReadUInt64();
                NameOffset = (uint)NameOffset64;
                ExportOffset = (uint)ExportOffset64;
                ImportOffset = (uint)ImportOffset64;
                r.ReadInt32();
                r.ReadInt64();
                r.ReadInt64();
                r.ReadInt64();
                r.ReadInt32();
                r.ReadInt32();
                r.ReadInt64();
                r.ReadGuid();
                r.ReadInt32();
                EngineVersion = r.ReadInt32();
                CompressionFlags = (COMPRESS)r.ReadInt32();
                CompressedChunks = r.ReadArray(Ar, r => new FCompressedChunk(r, Ar));
                // drop everything else in FPackageFileSummary
                return;
            }

            NameCount = r.ReadUInt32();
            NameOffset = r.ReadUInt32();
            ExportCount = r.ReadUInt32();
            ExportOffset = r.ReadUInt32();

            if (Ar.Game == APB)
            {
                if (Ar.ArLicenseeVer >= 29) r.Skip(sizeof(int));
                if (Ar.ArLicenseeVer >= 28) r.Skip(sizeof(float) * 5);
            }

            ImportCount = r.ReadUInt32();
            ImportOffset = r.ReadUInt32();

            if (Ar.Game == MK)
            {
                if (Ar.ArVer >= 524) r.Skip(sizeof(int));       // Injustice
                if (midwayVer >= 16) r.Skip(sizeof(int));
                if (Ar.ArVer >= 391) r.Skip(sizeof(int));
                if (Ar.ArVer >= 482) r.Skip(sizeof(int) * 3);       // Injustice
                if (Ar.ArVer >= 484) r.Skip(sizeof(int));        // Injustice
                if (Ar.ArVer >= 472)
                {
                    // Mortal Kombat, Injustice:
                    // - no DependsOffset
                    // - no generations (since version 446)
                    Guid = r.ReadGuid();
                    DependsOffset = 0;
                    goto engine_version;
                }
            }
            if (Ar.Game == Wheelman && midwayVer >= 23) r.Skip(sizeof(int));
            else if (Ar.Game == Strangle && Ar.ArVer >= 375) r.Skip(sizeof(int));
            // de-obfuscate NameCount for Tera
            else if (Ar.Game == Tera && (PackageFlags & PKG_Cooked) != 0) NameCount -= NameOffset;
            if (Ar.ArVer >= 415) DependsOffset = r.ReadInt32();
            if (Ar.Game == Bioshock3) { r.Skip(sizeof(int)); goto read_unk38; }
            else if (Ar.Game == DunDef && (PackageFlags & PKG_Cooked) != 0) r.Skip(sizeof(int));
            if (Ar.ArVer >= 623) { f38 = r.ReadInt32(); f3C = r.ReadInt32(); f40 = r.ReadInt32(); }
            if (Ar.Game == GoWU) goto guid;
            else if (Ar.Game == Transformers && Ar.ArVer >= 535) { r.Skip(sizeof(int)); goto read_unk38; }
            if (Ar.ArVer >= 584) r.Skip(sizeof(int));
            read_unk38:

        // GUID
        guid:
            Guid = r.ReadGuid();

            // Generations
            var GenerationCount = r.ReadInt32();
            if (Ar.Game == APB && Ar.ArLicenseeVer >= 32) r.Skip(16);
            Generations = r.ReadFArray(r => new FGenerationInfo(r, Ar), GenerationCount);
            if (Ar.Game == AliensCM) { r.Skip(sizeof(ushort) * 3); goto cooker_version; } // complex EngineVersion?

        engine_version:
            if (Ar.ArVer >= 245) { EngineVersion = r.ReadInt32(); }
        cooker_version:
            if (Ar.ArVer >= 277) CookerVersion = r.ReadInt32();

            // ... MassEffect has some additional structure here ...
            if (Ar.Game >= MassEffect && Ar.Game <= MassEffectLE)
            {
                if (Ar.ArLicenseeVer >= 16 && Ar.ArLicenseeVer < 136) r.Skip(sizeof(int));                 // random value, ME1&2
                if (Ar.ArLicenseeVer >= 32 && Ar.ArLicenseeVer < 136) r.Skip(sizeof(int));                 // unknown, ME1&2
                if (Ar.ArLicenseeVer >= 35 && Ar.ArLicenseeVer < 113)   // ME1
                {
                    throw new NotImplementedException();
                    //TMap<FString, TArray<FString>> unk5;
                    //Ar << unk5;
                }
                if (Ar.ArLicenseeVer >= 37) r.Skip(sizeof(int) * 2);   // 2 ints: 1, 0
                if (Ar.ArLicenseeVer >= 39 && Ar.ArLicenseeVer < 136) r.Skip(sizeof(int) * 2);   // 2 ints: -1, -1 (ME1&2)
            }
            if (Ar.ArVer >= 334)
            {
                CompressionFlags = (COMPRESS)r.ReadInt32();
                CompressedChunks = r.ReadArray(Ar, r => new FCompressedChunk(r, Ar));
            }
            if (Ar.ArVer >= 482) U3unk60 = r.ReadInt32();
            //if (Ar.ArVer >= 516) Ar << some array ... (U3unk70)
            //... MassEffect has additional field here ...
            //if (Ar.Game == MassEffect() && Ar.ArLicenseeVer >= 44) serialize 1*int
        }
    }

    partial class FObjectExport
    {
        void Serialize3(BinaryReader r, UPackage Ar)
        {
#if USE_COMPACT_PACKAGE_STRUCTS
            // locally declare FObjectImport data which are stripped
            int SuperIndex;
            uint ObjectFlags;
            uint ObjectFlags2;
            int Archetype;
            Guid Guid;
            int U3unk6C;
            int[] NetObjectCount;
#endif
            var AA3Obfuscator = 0;
            if (Ar.Game == AA3) AA3Obfuscator = r.ReadInt32();
            else if (Ar.Game == Wheelman)
            {
                // Wheelman has special code for quick serialization of FObjectExport struc
                // using a single Serialize(&S, 0x64) call
                // Ar.MidwayVer >= 22; when < 22 => standard version w/o ObjectFlags
                r.Skip(sizeof(int)); // 0 or 1
                ObjectName = new FName(r, Ar);
                PackageIndex = r.ReadInt32();
                ClassIndex = r.ReadInt32();
                SuperIndex = r.ReadInt32();
                Archetype = r.ReadInt32();
                ObjectFlags = r.ReadUInt32();
                ObjectFlags2 = r.ReadUInt32();
                SerialSize = r.ReadInt32();
                SerialOffset = r.ReadInt32();
                r.Skip(sizeof(int)); // zero ?
                r.Skip(sizeof(int)); // -1 ?
                r.Skip(0x14);  // skip raw version of ComponentMap
                ExportFlags = r.ReadUInt32();
                r.Skip(0xC); // skip raw version of NetObjectCount
                Guid = r.ReadGuid();
                return;
            }

            ClassIndex = r.ReadInt32();
            SuperIndex = r.ReadInt32();
            PackageIndex = r.ReadInt32();
            ObjectName = new FName(r, Ar);
            Archetype = Ar.ArVer >= 220 ? r.ReadInt32() : 0;

            if ((Ar.Game >= Batman2 && Ar.Game <= Batman4) && Ar.ArLicenseeVer >= 89) r.Skip(sizeof(int));
            if (Ar.Game == MK && Ar.ArVer >= 573)  // Injustice, version unknown
            {
                r.Skip(sizeof(int));
                if (Ar.ArVer >= 677) r.Skip(sizeof(int) * 4); // MK X, version unknown
            }

            ObjectFlags = r.ReadUInt32();
            if (Ar.ArVer >= 195) ObjectFlags2 = r.ReadUInt32();    // qword flags after version 195
            SerialSize = r.ReadInt32();
            if (SerialSize != 0 || Ar.ArVer >= 249) SerialOffset = r.ReadInt32();

            if (Ar.Game == GoWU)
            {
                var unk = r.ReadInt32();
                if (unk != 0) r.Seek(unk * 12);
            }
            else if (Ar.Game == Huxley && Ar.ArLicenseeVer >= 22) r.Skip(sizeof(int));
            else if (Ar.Game == AlphaProtocol && Ar.ArLicenseeVer >= 53) goto ue3_export_flags; // no ComponentMap
            else if (Ar.Game == Transformers && Ar.ArLicenseeVer >= 37) goto ue3_export_flags;  // no ComponentMap
            else if (Ar.Game == MK)
            {
                if (Ar.ArVer >= 677)
                {
                    // MK X has 64-bit SerialOffset, skip HIDWORD
                    var SerialOffsetUpper = r.ReadInt32();
                    Debug.Assert(SerialOffsetUpper == 0);
                }
                if (Ar.ArVer >= 573)
                {
                    var tmpComponentMap = r.ReadMap(Ar, r => (new FName(r, Ar), r.ReadInt32()));
                    goto ue3_export_flags; // Injustice, version unknown
                }
            }
            else if (Ar.Game == RocketLeague && Ar.ArLicenseeVer >= 22)
            {
                // Rocket League has 64-bit SerialOffset in LicenseeVer >= 22, skip HIDWORD
                var SerialOffsetUpper = r.ReadInt32();
                Debug.Assert(SerialOffsetUpper == 0);
            }
            if (Ar.ArVer < 543)
            {
                var tmpComponentMap = r.ReadMap(Ar, r => (new FName(r, Ar), r.ReadInt32()));
            }
        ue3_export_flags:
            if (Ar.ArVer >= 247) ExportFlags = r.ReadUInt32();
            if (Ar.Game == Transformers && Ar.ArLicenseeVer >= 116)
            {
                // version prior 116
                var someFlag = r.ReadByte();
                if (someFlag == 0) return;
                // else - continue serialization of remaining fields
            }
            else if (Ar.Game == MK && Ar.ArVer >= 446)
            {
                // removed generations (NetObjectCount)
                Guid = r.ReadGuid();
                return;
            }
            else if (Ar.Game == Bioshock3)
            {
                var flag = r.ReadInt32();
                if (flag == 0) return;              // stripped some fields
            }
            if (Ar.ArVer >= 322)
            {
                NetObjectCount = r.ReadArray(Ar, r => r.ReadInt32());
                Guid = r.ReadGuid();
            }
            if (Ar.Game == Undertow && Ar.ArVer >= 431) U3unk6C = r.ReadInt32(); // partially upgraded?
            else if (Ar.Game == ArmyOf2) return;
            if (Ar.ArVer >= 475) U3unk6C = r.ReadInt32();
            if (Ar.Game == AA3)
            {
                // deobfuscate data
                ClassIndex ^= AA3Obfuscator;
                SuperIndex ^= AA3Obfuscator;
                PackageIndex ^= AA3Obfuscator;
                Archetype ^= AA3Obfuscator;
                SerialSize ^= AA3Obfuscator;
                SerialOffset ^= AA3Obfuscator;
            }
            else if (Ar.Game == Thief4 && (ExportFlags & 8) != 0) r.Skip(sizeof(int));
        }
    }

    partial class UPackage
    {
        unsafe void LoadNames3(BinaryReader r, UPackage Ar)
        {
            string nameStr;
            for (var i = 0; i < Summary.NameCount; i++)
            {
                if (Game == DCUniverse)        // no version checking
                {
                    var buf = stackalloc char[MAX_FNAME_LEN];
                    var len = r.ReadInt32();
                    Debug.Assert(len > 0 && len < 0x3FF); // requires extra code
                    Debug.Assert(len < MAX_FNAME_LEN);
                    r.BaseStream.Read(new Span<byte>(buf, len));
                    Names[i] = new string(buf, 0, len);
                    goto qword_flags;
                }
                else if (Game == R6Vegas2 && ArLicenseeVer >= 71)
                {
                    var buf = stackalloc char[256];
                    var len = r.ReadByte();
                    r.BaseStream.Read(new Span<byte>(buf, len));
                    Names[i] = new string(buf, 0, len);
                    continue;
                }
                else if (Game == Transformers && ArLicenseeVer >= 181) // Transformers: Fall of Cybertron; no real version in code
                {
                    var buf = stackalloc char[MAX_FNAME_LEN];
                    var len = r.ReadInt32();
                    Debug.Assert(len < MAX_FNAME_LEN);
                    r.BaseStream.Read(new Span<byte>(buf, len));
                    Names[i] = new string(buf, 0, len);
                    goto qword_flags;
                }

                nameStr = r.ReadFString(Ar);
                VerifyName(ref nameStr, i);
                Names[i] = nameStr;

                if (Game == AVA)
                {
                    // Strange code - package contains some bytes:
                    // V(0) = len ^ 0x3E
                    // V(i) = V(i-1) + 0x48 ^ 0xE1
                    // Number of bytes = (len ^ 7) & 0xF
                    var skip = nameStr.Length;
                    skip = (skip ^ 7) & 0xF;
                    r.Seek(skip);
                }
                else if (Game == Wheelman) goto dword_flags;
                else if (Game >= MassEffect && Game <= MassEffectLE)
                {
                    if (ArLicenseeVer >= 142) continue;            // ME3, no flags
                    if (ArLicenseeVer >= 102) goto dword_flags;     // ME2
                }
                else if (Game == MK && ArVer >= 677) continue;      // no flags for MK X
                else if (Game == MetroConflict)
                {
                    var TrashLen = ArLicenseeVer < 3 ? 0
                        : ArLicenseeVer < 16 ? nameStr.Length ^ 7
                        : nameStr.Length ^ 6;
                    r.Skip(TrashLen & 0xF);
                }

                // Generic UE3
                if (ArVer >= 195) { goto qword_flags; } // Object flags are 64-bit in UE3
                else { goto dword_flags; }

            qword_flags:
                r.Skip(sizeof(ulong)); // 64-bit flags
                continue;
            dword_flags:
                r.Skip(sizeof(uint)); // 32-bit flags
            }
        }
    }
}