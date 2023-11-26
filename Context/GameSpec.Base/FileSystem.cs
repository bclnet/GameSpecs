using Microsoft.Extensions.FileSystemGlobbing;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GameSpec
{
    /// <summary>
    /// IFileSystem
    /// </summary>
    public interface IFileSystem
    {
        //string[] GetDirectories(string path, string searchPattern, bool recursive);
        //string[] GetFiles(string path, string searchPattern);
        IEnumerable<string> Glob(string path, string searchPattern);
        string GetFile(string path);
        bool FileExists(string path);
        FileInfo GetFileInfo(string path);
        BinaryReader OpenReader(string path);
        BinaryWriter OpenWriter(string path);
    }

    /// <summary>
    /// StandardFileSystem
    /// </summary>
    internal class StandardFileSystem : IFileSystem
    {
        readonly string Root;
        readonly int Skip;
        public StandardFileSystem(string root) { Root = root; Skip = root.Length + 1; }
        public IEnumerable<string> Glob(string path, string searchPattern)
        {
            var matcher = new Matcher();
            matcher.AddIncludePatterns(new[] { searchPattern });
            return matcher.GetResultsInFullPath(Path.Combine(Root, path)).Select(x => x[Skip..]);
        }
        //public string[] GetDirectories(string path, string searchPattern, bool recursive) => Directory.GetDirectories(Path.Combine(Root, path), searchPattern, recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly).Select(x => x[Skip..]).ToArray();
        //public string[] GetFiles(string path, string searchPattern) => Directory.GetFiles(Path.Combine(Root, path), searchPattern).Select(x => x[Skip..]).ToArray();
        public string GetFile(string path) => File.Exists(path = Path.Combine(Root, path)) ? path[Skip..] : null;
        public bool FileExists(string path) => File.Exists(Path.Combine(Root, path));
        public FileInfo GetFileInfo(string path) => new FileInfo(Path.Combine(Root, path));
        public BinaryReader OpenReader(string path) => new BinaryReader(File.Open(Path.Combine(Root, path), FileMode.Open, FileAccess.Read, FileShare.Read));
        public BinaryWriter OpenWriter(string path) => new BinaryWriter(File.Open(Path.Combine(Root, path), FileMode.Open, FileAccess.Write, FileShare.Write));
    }

    /// <summary>
    /// HostFileSystem
    /// </summary>
    internal class HostFileSystem : IFileSystem
    {
        public HostFileSystem(Uri uri)
        {
            if (uri == null) throw new ArgumentNullException(nameof(uri));
            var pathOrPattern = uri.LocalPath;
            var searchPattern = Path.GetFileName(pathOrPattern);
            var path = Path.GetDirectoryName(pathOrPattern);
            // file
            if (!string.IsNullOrEmpty(searchPattern)) throw new ArgumentOutOfRangeException(nameof(pathOrPattern), pathOrPattern); //: Web single file access to supported.

            //options = PakOption.Stream;
            //searchPattern = Path.GetFileName(path);
            //path = Path.GetDirectoryName(path);
            //if (path.Contains('*')) throw new NotSupportedException("Web wildcard folder access");
            //host = new UriBuilder(uri) { Path = $"{path}/", Fragment = null }.Uri;
            //if (searchPattern.Contains('*'))
            //{
            //    var set = new HttpHost(host).GetSetAsync().Result ?? throw new NotSupportedException(".set not found. Web wildcard access");
            //    var pattern = $"^{Regex.Escape(searchPattern.Replace('*', '%')).Replace("_", ".").Replace("%", ".*")}$";
            //    return set.Where(x => Regex.IsMatch(x, pattern)).ToArray();
            //}
            //return new[] { searchPattern };
        }
        public IEnumerable<string> Glob(string path, string searchPattern)
        {
            var matcher = new Matcher();
            matcher.AddIncludePatterns(new[] { searchPattern });
            return matcher.GetResultsInFullPath(searchPattern);
        }
        //public string[] GetDirectories(string path, string searchPattern, bool recursive) => Directory.GetDirectories(path, searchPattern, recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
        //public string[] GetFiles(string path, string searchPattern) => Directory.GetFiles(path, searchPattern);
        public string GetFile(string path) => File.Exists(path) ? path : null;
        public bool FileExists(string path) => File.Exists(path);
        public FileInfo GetFileInfo(string path) => null;
        public BinaryReader OpenReader(string path) => null;
        public BinaryWriter OpenWriter(string path) => null;
    }

}