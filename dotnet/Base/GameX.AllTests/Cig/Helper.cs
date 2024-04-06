using System;
using System.Collections.Generic;

namespace GameX.Cig
{
    public static class Helper
    {
        static readonly Family familyCig = FamilyManager.GetFamily("Cig");

        public static readonly Dictionary<string, Lazy<PakFile>> Paks = new()
        {
            { "Cig:StarCitizen", new Lazy<PakFile>(() => familyCig.OpenPakFile(new Uri("game:/Data.p4k#StarCitizen"))) },
        };
    }
}
