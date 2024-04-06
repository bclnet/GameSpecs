using System;
using System.Collections.Generic;

namespace GameX.Unity
{
    public static class Helper
    {
        static readonly Family familyUnity = FamilyManager.GetFamily("Unity");

        public static readonly Dictionary<string, Lazy<PakFile>> Paks = new()
        {
            { "Unity:AmongUs", new Lazy<PakFile>(() => familyUnity.OpenPakFile(new Uri("game:/resources.assets#AmongUs"))) },
        };
    }
}
