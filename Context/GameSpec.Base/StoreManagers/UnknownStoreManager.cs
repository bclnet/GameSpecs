using System.Text.Json;

namespace GameSpec.StoreManagers
{
    /// <summary>
    /// UnknownStoreManager
    /// </summary>
    internal static class UnknownStoreManager
    {
        public static bool TryGetPathByKey(string key, out string path)
        {
            path = null;
            return false;
        }
    }
}

