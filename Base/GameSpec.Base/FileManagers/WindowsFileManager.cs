using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using static Microsoft.Win32.Registry;

namespace GameSpec.FileManagers
{
    /// <summary>
    /// WindowsFileManager
    /// </summary>
    internal class WindowsFileManager : FileManager
    {
        #region Parse File-Manager

        public override FileManager ParseFileManager(JsonElement elem)
        {
            AddApplicationByRegistry(elem);
            base.ParseFileManager(elem);
            if (!elem.TryGetProperty("windows", out var z)) return this;
            elem = z;
            AddDirect(elem);
            AddIgnores(elem);
            AddFilters(elem);
            return this;
        }

        #endregion
    }
}

