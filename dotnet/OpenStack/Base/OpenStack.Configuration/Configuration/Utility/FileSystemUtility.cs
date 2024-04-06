using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Xml.Linq;

namespace OpenStack.Configuration
{
    internal static class FileSystemUtility
    {
        internal static XDocument GetOrCreateDocument(XDocument content, string fullPath)
        {
            if (File.Exists(fullPath))
                try { return XmlUtility.Load(fullPath, LoadOptions.PreserveWhitespace); }
                catch (FileNotFoundException) { }

            AddFile(fullPath, content.Save);
            return content;
        }

        internal static void AddFile(string fullPath, Action<Stream> writeToStream)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
            using (Stream outputStream = File.Create(fullPath))
                writeToStream(outputStream);

            if (!NuGet.Common.RuntimeEnvironmentHelper.IsWindows)
            {
                var mode = Convert.ToInt32("600", 8);
                if (chmod(fullPath, mode) == -1)
                {
                    // it's very unlikely we can't set the permissions of a file we just wrote
                    var errno = Marshal.GetLastWin32Error(); // fetch the errno before running any other operation
                    throw new InvalidOperationException($"Unable to set permission while creating {fullPath}, errno={errno}.");
                }
            }
        }

        /// <summary>Only to be used for setting nuget.config permissions on Linux/Mac. Do not use elsewhere.</summary>
        [DllImport("libc", SetLastError = true, CharSet = CharSet.Ansi)]
        private static extern int chmod(string pathname, int mode);

        internal static bool IsPathAFile(string path)
            => string.Equals(path, Path.GetFileName(path), StringComparison.OrdinalIgnoreCase);

        internal static bool DoesFileExistIn(string root, string file)
            => File.Exists(Path.Combine(root, file));

        internal static IEnumerable<string> GetFilesRelativeToRoot(string root, string path = "", string[] filters = null, SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            path = EnsureTrailingSlash(Path.Combine(root, path));
            if (filters == null || !filters.Any())
                filters = new[] { "*.*" };
            try
            {
                if (!Directory.Exists(path)) return Enumerable.Empty<string>();
                var files = new HashSet<string>();

                foreach (var filter in filters)
                {
                    var enumerateFiles = Directory.EnumerateFiles(path, filter, searchOption);
                    files.UnionWith(enumerateFiles);
                }
                return files.Select(f => GetRelativePath(root, f));
            }
            catch (UnauthorizedAccessException) { }
            catch (DirectoryNotFoundException) { }
            return Enumerable.Empty<string>();
        }

        internal static string GetRelativePath(string root, string fullPath)
            => fullPath.Substring(root.Length).TrimStart(Path.DirectorySeparatorChar);

        internal static string EnsureTrailingSlash(string path)
            => EnsureTrailingCharacter(path, Path.DirectorySeparatorChar);

        internal static string EnsureTrailingForwardSlash(string path)
            => EnsureTrailingCharacter(path, '/');

        static string EnsureTrailingCharacter(string path, char trailingCharacter)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));

            // if the path is empty, we want to return the original string instead of a single trailing character.
            return path.Length == 0 || path[path.Length - 1] == trailingCharacter ? path : path + trailingCharacter;
        }
    }
}
