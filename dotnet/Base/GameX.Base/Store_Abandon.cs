using System.Collections.Generic;
using System.IO;

namespace GameX.Store
{
    /// <summary>
    /// Store_Abandon
    /// </summary>
    internal static class Store_Abandon
    {
        internal static Dictionary<string, string> AbandonPaths = new Dictionary<string, string>();

        static Store_Abandon()
        {
            // get dbPath
            var root = GetPath();
            if (root == null || !Directory.Exists(root)) return;
            // # query games
            foreach (var s in Directory.EnumerateFiles(root))
                AbandonPaths.Add(Path.GetFileName(s), s);
        }

        static string GetPath() => @"G:\AbandonLibrary";
    }
}
