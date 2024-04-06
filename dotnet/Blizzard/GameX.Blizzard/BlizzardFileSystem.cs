using Microsoft.Extensions.FileSystemGlobbing;
using System.Collections.Generic;
using System.IO;

namespace GameX.Blizzard
{
    /// <summary>
    /// BlizzardFileSystem
    /// </summary>
    /// <seealso cref="GameX.Family" />
    public class BlizzardFileSystem : IFileSystem
    {
        public IEnumerable<string> Glob(string path, string searchPattern)
        {
            var matcher = new Matcher();
            matcher.AddIncludePatterns(new[] { searchPattern });
            return matcher.GetResultsInFullPath(searchPattern);
        }

        public (string path, long length) FileInfo(string path)
        {
            throw new System.NotImplementedException();
        }

        public BinaryReader OpenReader(string path)
        {
            throw new System.NotImplementedException();
        }

        public BinaryWriter OpenWriter(string path)
        {
            throw new System.NotImplementedException();
        }

        public bool FileExists(string path)
        {
            throw new System.NotImplementedException();
        }
    }
}