using System;
using System.Diagnostics;
using System.IO;
using static GameX.Epic.Formats.Core.Game;
using static GameX.Epic.Formats.Core.UPackage;
using static GameX.Epic.Formats.Core.ReaderExtensions;

namespace GameX.Epic.Formats.Core
{
    partial class FPackageFileSummary
    {
        // Engine-specific serializers
        void Serialize2(BinaryReader r)
        {
            if (Ar.Game == SplinterCell && Ar.ArLicenseeVer >= 83) r.Skip(sizeof(int));

            PackageFlags = r.ReadInt32();

            if (Ar.Game == SplinterCellConv) r.Skip(sizeof(int));

            NameCount = r.ReadUInt32();
            NameOffset = r.ReadUInt32();
            ExportCount = r.ReadUInt32();
            ExportOffset = r.ReadUInt32();
            ImportCount = r.ReadUInt32();
            ImportOffset = r.ReadUInt32();

            if (Ar.Game == SplinterCellConv && Ar.ArLicenseeVer >= 48)
            {
                // this game has additional name table for some packages
                var ExtraNameCount = r.ReadInt32();
                var ExtraNameOffset = r.ReadInt32();
                if (ExtraNameOffset < ImportOffset) ExtraNameCount = 0;
                if (Ar.ArLicenseeVer >= 85) r.Skip(sizeof(int));
                goto generations;   // skip Guid
            }
            else if (Ar.Game == SplinterCell)
            {
                r.Skip(4); // 0xFF0ADDE
                var tmp2 = r.ReadArray(Ar, r => r.ReadByte());
            }
            if ((GForceGame == 0) && (PackageFlags & 0x10000) != 0 && (Ar.ArVer >= 0x80 && Ar.ArVer < 0x88)) //?? unknown upper limit; known lower limit: 0x80
            {
                // encrypted Ragnarok Online archive header (data taken by archive analysis)
                Ar.Game = Ragnarok2;
                NameCount ^= 0xD97790C7 ^ 0x1C;
                NameOffset ^= 0xF208FB9F ^ 0x40;
                ExportCount ^= 0xEBBDE077 ^ 0x04;
                ExportOffset ^= 0xE292EC62 ^ 0x03E9E1;
                ImportCount ^= 0x201DA87A ^ 0x05;
                ImportOffset ^= 0xA9B999DF ^ 0x003E9BE;
                return; // other data is useless for us, and they are encrypted too
            }
            else if (Ar.Game == EOS && Ar.ArLicenseeVer >= 49) goto generations;

            // Guid and generations
            if (Ar.ArVer < 68)
            {
                // old generations code
                var HeritageCount = r.ReadInt32();
                var HeritageOffset = r.ReadInt32();
                Generations = new[] { new FGenerationInfo(ExportCount, NameCount) };
            }
            else
            {
                Guid = r.ReadGuid();
                goto generations;
            }
            return;

        // current generations code
        generations:
            Generations = r.ReadL32FArray(r => new FGenerationInfo(r, Ar));
        }
    }

    partial class FObjectExport
    {
        void Serialize2(BinaryReader r, UPackage Ar)
        {
#if USE_COMPACT_PACKAGE_STRUCTS
            int SuperIndex;
            uint ObjectFlags;
#endif
            if (Ar.Game == Pariah)
            {
                ObjectName = new FName(r, Ar);
                SuperIndex = r.ReadCompactIndex(Ar);
                PackageIndex = r.ReadInt32();
                ObjectFlags = r.ReadUInt32();
                ClassIndex = r.ReadCompactIndex(Ar);
                SerialSize = r.ReadCompactIndex(Ar);
                if (SerialSize != 0) SerialOffset = r.ReadCompactIndex(Ar);
                return;
            }
            else if (Ar.Game == Bioshock)
            {
                ClassIndex = r.ReadCompactIndex(Ar);
                SuperIndex = r.ReadCompactIndex(Ar);
                PackageIndex = r.ReadInt32();
                if (Ar.ArVer >= 132) r.Skip(sizeof(int));            // unknown
                ObjectName = new FName(r, Ar);
                ObjectFlags = r.ReadUInt32();
                if (Ar.ArLicenseeVer >= 40) r.Skip(sizeof(int));   // qword flags
                SerialSize = r.ReadCompactIndex(Ar);
                if (SerialSize != 0) SerialOffset = r.ReadCompactIndex(Ar);
                if (Ar.ArVer >= 130) r.Skip(sizeof(int));           // unknown
                return;
            }
            else if (Ar.Game == RepCommando && Ar.ArVer >= 151)
            {
                ClassIndex = r.ReadCompactIndex(Ar);
                SuperIndex = r.ReadCompactIndex(Ar);
                PackageIndex = r.ReadInt32();
                if (Ar.ArVer >= 159) r.ReadCompactIndex(Ar);
                ObjectName = new FName(r, Ar);
                ObjectFlags = r.ReadUInt32();
                SerialSize = r.ReadInt32();
                SerialOffset = r.ReadInt32();
                return;
            }
            else if (Ar.Game == AA2)
            {
                SuperIndex = r.ReadCompactIndex(Ar);
                r.Skip(sizeof(int));
                ClassIndex = r.ReadCompactIndex(Ar);
                PackageIndex = r.ReadInt32();
                ObjectFlags = ~r.ReadUInt32(); // ObjectFlags are serialized in different order, ObjectFlags are negated
                ObjectName = new FName(r, Ar);
                SerialSize = r.ReadCompactIndex(Ar);
                if (SerialSize != 0) SerialOffset = r.ReadCompactIndex(Ar);
                return;
            }

            // generic UE1/UE2 code
            ClassIndex = r.ReadCompactIndex(Ar);
            SuperIndex = r.ReadCompactIndex(Ar);
            PackageIndex = r.ReadInt32();
            ObjectName = new FName(r, Ar);
            ObjectFlags = r.ReadUInt32();
            SerialSize = r.ReadCompactIndex(Ar);
            if (SerialSize != 0) SerialOffset = r.ReadCompactIndex(Ar);
        }

        void Serialize2X(BinaryReader r, UPackage Ar)
        {
#if USE_COMPACT_PACKAGE_STRUCTS
            int SuperIndex;
            uint ObjectFlags;
#endif
            ClassIndex = r.ReadInt32();
            SuperIndex = r.ReadInt32();
            PackageIndex = Ar.ArVer >= 150 ? r.ReadInt16() : r.ReadInt32();
            ObjectName = new FName(r, Ar);
            ObjectFlags = r.ReadUInt32();
            SerialSize = r.ReadInt32();
            if (SerialSize != 0) SerialOffset = r.ReadInt32();
            // UC2 has strange thing here: indices are serialized as 4-byte int (instead of AR_INDEX),
            // but stored into 2-byte shorts
            ClassIndex = (short)ClassIndex;
            SuperIndex = (short)SuperIndex;
        }
    }

    partial class UPackage
    {
        unsafe void LoadNames2(BinaryReader r, UPackage Ar)
        {
            var buf = stackalloc char[MAX_FNAME_LEN];
            // Korean games sometimes uses Unicode strings, so use FString for serialization
            string nameStr;
            for (var i = 0; i < Summary.NameCount; i++)
            {
                if (ArVer < 64) // UE1
                {
                    //var buf = stackalloc char[MAX_FNAME_LEN];
                    int len;
                    for (len = 0; len < MAX_FNAME_LEN; len++)
                    {
                        var c = (char)r.ReadByte();
                        buf[len] = c;
                        if (c == 0) break;
                    }
                    Debug.Assert(len < MAX_FNAME_LEN);
                    Names[i] = new string(buf, 0, len);
                    goto dword_flags;
                }
                else if ((Game == UC1 && ArLicenseeVer >= 28) || (Game == Pariah && (ArLicenseeVer & 0x3F) >= 28))
                {
                    // used uint16 + char[] instead of FString
                    //char* buf = stackalloc char[MAX_FNAME_LEN];
                    var len = r.ReadUInt16();
                    Debug.Assert(len < MAX_FNAME_LEN);
                    r.BaseStream.Read(new Span<byte>(buf, len + 1));
                    Names[i] = new string(buf, 0, len);
                    goto dword_flags;
                }
                else if (Game == SplinterCell && ArLicenseeVer >= 85)
                {
                    //char* buf = stackalloc char[256];
                    var len = r.ReadByte();
                    r.BaseStream.Read(new Span<byte>(buf, len + 1));
                    Names[i] = new string(buf, 0, len);
                    goto dword_flags;
                }
                else if (Game == SplinterCellConv && ArVer >= 68)
                {
                    //char* buf = stackalloc char[MAX_FNAME_LEN];
                    var len = r.ReadCompactIndex(Ar);
                    Debug.Assert(len < MAX_FNAME_LEN);
                    r.BaseStream.Read(new Span<byte>(buf, len));
                    Names[i] = new string(buf, 0, len);
                    continue;
                }
                else if (Game == AA2)
                {
                    //char* buf = stackalloc char[MAX_FNAME_LEN];
                    var len = r.ReadCompactIndex(Ar);
                    // read as unicode string and decrypt
                    Debug.Assert(len <= 0);
                    len = -len;
                    Debug.Assert(len < MAX_FNAME_LEN);
                    var d = buf;
                    var shift = (byte)5;
                    for (var j = 0; j < len; j++, d++)
                    {
                        var c = r.ReadUInt16();
                        var c2 = ROR16(c, shift);
                        Debug.Assert(c2 < 256);
                        *d = (char)(c2 & 0xFF);
                        shift = (byte)((c - 5) & 15);
                    }
                    Names[i] = new string(buf, 0, len);
                    var unk = r.ReadCompactIndex(Ar);
                    goto dword_flags;
                }

                nameStr = r.ReadFString(Ar);
                VerifyName(ref nameStr, i);
                Names[i] = nameStr;

                if (Game == Bioshock) { goto qword_flags; } // 64-bit flags, like in UE3
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
