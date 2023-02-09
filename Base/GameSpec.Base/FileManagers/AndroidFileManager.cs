using System.Text.Json;

namespace GameSpec.FileManagers
{
    /// <summary>
    /// AndroidFileManager
    /// </summary>
    internal class AndroidFileManager : FileManager
    {
        #region Parse File-Manager

        public override FileManager ParseFileManager(JsonElement elem)
        {
            base.ParseFileManager(elem);
            if (!elem.TryGetProperty("android", out var z)) return this;
            elem = z;
            AddDirect(elem);
            AddIgnores(elem);
            AddFilters(elem);
            return this;
        }

        #endregion
    }
}

