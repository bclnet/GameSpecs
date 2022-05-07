using System;
using System.Collections.Generic;

namespace GameSpec.Unity
{
    public static class Helper
    {
        static readonly Family familyUnity = FamilyManager.GetFamily("Unity");

        public static readonly Dictionary<string, Lazy<PakFile>> Paks = new()
        {
            { "Rsi:StarCitizen", new Lazy<PakFile>(() => familyUnity.OpenPakFile(new Uri("game:/Data.p4k#StarCitizen"))) },
        };
    }
}
