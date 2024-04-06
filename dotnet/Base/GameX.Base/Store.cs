using GameX.Store;
using System;

namespace GameX
{
    /// <summary>
    /// StoreManager
    /// </summary>
    public static class StoreManager
    {
        public static string GetPathByKey(string key)
        {
            var parts = key.Split(':', 2);
            string k = parts[0], v = parts[1];
            return k switch
            {
                "Steam" => Store_Steam.SteamPaths.TryGetValue(v, out var z) ? z : null,
                "GOG" => Store_Gog.GogPaths.TryGetValue(v, out var z) ? z : null,
                "Blizzard" => Store_Blizzard.BlizzardPaths.TryGetValue(v, out var z) ? z : null,
                "Epic" => Store_Epic.EpicPaths.TryGetValue(v, out var z) ? z : null,
                "Ubisoft" => Store_Ubisoft.UbisoftPaths.TryGetValue(v, out var z) ? z : null,
                "Abandon" => Store_Abandon.AbandonPaths.TryGetValue(v, out var z) ? z : null,
                "Unknown" => null,
                _ => throw new ArgumentOutOfRangeException(nameof(key), key),
            };
        }
    }
}
