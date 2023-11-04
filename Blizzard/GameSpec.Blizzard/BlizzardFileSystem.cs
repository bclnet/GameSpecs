using System.IO;

namespace GameSpec.Blizzard
{
    /// <summary>
    /// BlizzardFileSystem
    /// </summary>
    /// <seealso cref="GameSpec.Family" />
    public class BlizzardFileSystem : IFileSystem
    {
        public string[] GetDirectories(string path, string searchPattern, bool recursive)
        {
            throw new System.NotImplementedException();
        }

        public string[] GetFiles(string path, string searchPattern)
        {
            throw new System.NotImplementedException();
        }
        public string GetFile(string path)
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

        public FileInfo GetFileInfo(string path)
        {
            throw new System.NotImplementedException();
        }
    }
}