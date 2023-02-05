using System.Text.Json;

namespace GameSpec.FileManagers
{
    /// <summary>
    /// OsxFileManager
    /// </summary>
    internal class MacOsFileManager : FileManager
    {
        #region Parse File-Manager

        public override FileManager ParseFileManager(JsonElement elem)
        {
            base.ParseFileManager(elem);
            if (!elem.TryGetProperty("macos", out var z)) return this;
            elem = z;

            AddRegistry(elem);
            AddDirect(elem);
            AddIgnores(elem);
            AddFilters(elem);

            return this;
        }

        protected void AddRegistry(JsonElement elem)
        {
            if (!elem.TryGetProperty("registry", out var z)) return;
            foreach (var prop in z.EnumerateObject())
                if (prop.Value.TryGetProperty("key", out z))
                {
                    var keys = z.ValueKind switch
                    {
                        JsonValueKind.String => new[] { z.GetString() },
                        JsonValueKind.Array => z.EnumerateArray().Select(y => y.GetString()),
                        _ => throw new ArgumentOutOfRangeException(),
                    };
                    foreach (var key in keys)
                        if (TryGetRegistryByKey(key, prop, prop.Value.TryGetProperty(key, out z) ? (JsonElement?)z : null, out var path)) AddPath(prop, path);
                }
        }

        #endregion
    }
}

