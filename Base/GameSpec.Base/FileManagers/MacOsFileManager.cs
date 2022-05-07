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
            //elem = z;

            return this;
        }

        #endregion
    }
}

