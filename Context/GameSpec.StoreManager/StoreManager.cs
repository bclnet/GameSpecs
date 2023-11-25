using GameSpec.StoreManagers;
using System;

namespace GameSpec
{
    /// <summary>
    /// StoreManager
    /// </summary>
    public static class StoreManager
    {
        public static bool TryGetPathByKey(string key, out string path)
        {
            var parts = key.Split(':', 2);
            return parts[0] switch
            {
                "Steam" => SteamStoreManager.TryGetPathByKey(parts[1], out path),
                "GOG" => GogStoreManager.TryGetPathByKey(parts[1], out path),
                "Blizzard" => BlizzardStoreManager.TryGetPathByKey(parts[1], out path),
                "Epic" => EpicStoreManager.TryGetPathByKey(parts[1], out path),
                "Unknown" => UnknownStoreManager.TryGetPathByKey(parts[1], out path),
                _ => throw new ArgumentOutOfRangeException(nameof(key), parts[0]),
            };
        }
    }
}
