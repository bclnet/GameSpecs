using System;
using System.Collections.Generic;

namespace GameSpec.Rsi
{
    public static class Helper
    {
        static readonly Family familyRsi = FamilyManager.GetFamily("Rsi");

        public static readonly Dictionary<string, Lazy<PakFile>> Paks = new()
        {
            { "Rsi:StarCitizen", new Lazy<PakFile>(() => familyRsi.OpenPakFile(new Uri("game:/Data.p4k#StarCitizen"))) },
        };
    }
}
