using GameSpec.StoreManagers;
using System;

namespace GameSpec
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
                "Steam" => StoreManager_Steam.SteamPaths.TryGetValue(v, out var z) ? z : null,
                "GOG" => StoreManager_Gog.GogPaths.TryGetValue(v, out var z) ? z : null,
                "Blizzard" => StoreManager_Blizzard.BlizzardPaths.TryGetValue(v, out var z) ? z : null,
                "Epic" => StoreManager_Epic.EpicPaths.TryGetValue(v, out var z) ? z : null,
                "Ubisoft" => StoreManager_Ubisoft.UbisoftPaths.TryGetValue(v, out var z) ? z : null,
                "Unknown" => null,
                _ => throw new ArgumentOutOfRangeException(nameof(key), key),
            };
        }
    }
}
