using System.Text.Json;

namespace GameSpec.Base.FileManagers
{
    /// <summary>
    /// UnknownStoreManager
    /// </summary>
    internal static class UnknownStoreManager
    {
        public static bool TryGetPathByKey(string key, JsonProperty prop, JsonElement? keyElem, out string path)
        {
            path = null;
            return false;
        }
    }
}

