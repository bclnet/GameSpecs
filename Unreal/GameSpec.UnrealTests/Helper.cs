using System;
using System.Collections.Generic;

namespace GameSpec.Unreal
{
    public static class Helper
    {
        static readonly Family familyUnreal = FamilyManager.GetFamily("Unreal");

        public static readonly Dictionary<string, Lazy<PakFile>> Paks = new()
        {
            { "Rsi:StarCitizen", new Lazy<PakFile>(() => familyUnreal.OpenPakFile(new Uri("game:/Data.p4k#StarCitizen"))) },
        };
    }
}
