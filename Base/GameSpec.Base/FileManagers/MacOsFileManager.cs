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
            AddDirect(elem);
            AddIgnores(elem);
            AddFilters(elem);
            return this;
        }

        #endregion
    }
}

