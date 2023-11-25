using System;
using System.Collections.Generic;

namespace GameSpec.Epic
{
    public static class Helper
    {
        static readonly Family familyEpic = FamilyManager.GetFamily("Epic");

        public static readonly Dictionary<string, Lazy<PakFile>> Paks = new()
        {
            { "Rsi:StarCitizen", new Lazy<PakFile>(() => familyEpic.OpenPakFile(new Uri("game:/Data.p4k#StarCitizen"))) },
        };
    }
}
