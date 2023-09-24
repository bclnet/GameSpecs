using System;
using System.Diagnostics;
using System.IO;
using static GameSpec.Unreal.Formats.Core.Game;

namespace GameSpec.Unreal.Formats.Core
{
    [DebuggerDisplay("{Str}")]
    public class FName
    {
        public string Str = "None";
#if !USE_COMPACT_PACKAGE_STRUCTS
        int Index;
        int ExtraIndex;
#endif
        public FName(BinaryReader r, UPackage Ar)
        {
            // Declare aliases for FName.Index and ExtraIndex to allow USE_COMPACT_PACKAGE_STRUCTS to work
#if USE_COMPACT_PACKAGE_STRUCTS
            int Index = 0;
            int ExtraIndex = 0;
#endif
            if (Ar.Game == Bioshock)
            {
                Index = r.ReadCompactIndex(Ar);
                ExtraIndex = r.ReadInt32();
                Str = ExtraIndex == 0 ? Ar.GetName(Index) : $"{Ar.GetName(Index)}{ExtraIndex - 1}";  // without "_" char
                return;
            }
            if (Ar.Engine == UE2X && Ar.ArVer >= 145) Index = r.ReadInt32();
            else if (Ar.Game == SplinterCellConv && Ar.ArVer >= 64) Index = r.ReadInt32();
            else if (Ar.Engine >= UE3)
            {
                Index = r.ReadInt32();
                if (Ar.Game >= UE4_BASE) { ExtraIndex = r.ReadInt32(); goto extra_index; }
                if (Ar.Game == R6Vegas2)
                {
                    ExtraIndex = Index >> 19;
                    Index &= 0x7FFFF;
                }
                if (Ar.ArVer >= 343) ExtraIndex = r.ReadInt32();

            }
            // UE1 and UE2
            else Index = r.ReadCompactIndex(Ar);
            extra_index:

            // Convert name index to string
            Str = ExtraIndex == 0 ? Ar.GetName(Index) : $"{Ar.GetName(Index)}_{ExtraIndex - 1}";
        }
        public override string ToString() => Str;
        public static implicit operator string(FName d) => d.Str;
    }
}