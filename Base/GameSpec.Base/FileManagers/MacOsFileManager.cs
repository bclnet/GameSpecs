using System.IO;
using System.Text.Json;

namespace GameSpec.FileManagers
{
    /// <summary>
    /// MacOSFileManager
    /// </summary>
    internal class MacOSFileManager : FileManager
    {
        #region Parse File-Manager

        public override FileManager ParseFileManager(JsonElement elem)
        {
            base.ParseFileManager(elem);
            if (!elem.TryGetProperty("macOS", out var z)) return this;
            elem = z;

            AddApplication(elem);
            AddDirect(elem);
            AddIgnores(elem);
            AddFilters(elem);

            return this;
        }

        protected static bool TryGetApplicationByKey(string key, JsonProperty prop, JsonElement? keyElem, out string path)
        {
            path = null;
            //path = GetRegistryExePath(new[] { $@"Wow6432Node\{key}", key });
            //if (keyElem == null) return !string.IsNullOrEmpty(path);
            //if (keyElem.Value.TryGetProperty("path", out var path2)) { path = Path.GetFullPath(PathWithSpecialFolders(path2.GetString(), path)); return !string.IsNullOrEmpty(path); }
            //else if (keyElem.Value.TryGetProperty("xml", out var xml)
            //    && keyElem.Value.TryGetProperty("xmlPath", out var xmlPath)
            //    && TryGetSingleFileValue(PathWithSpecialFolders(xml.GetString(), path), "xml", xmlPath.GetString(), out path))
            //    return !string.IsNullOrEmpty(path);
            return false;
        }

        protected void AddApplication(JsonElement elem)
        {
            if (!elem.TryGetProperty("application", out var z)) return;
            foreach (var prop in z.EnumerateObject())
                if (prop.Value.TryGetProperty("key", out z))
                    foreach (var key in z.GetStringOrArray())
                        if (TryGetApplicationByKey(key, prop, prop.Value.TryGetProperty(key, out z) ? z : null, out var path)) AddPath(prop, path);
        }

        #endregion
    }
}

