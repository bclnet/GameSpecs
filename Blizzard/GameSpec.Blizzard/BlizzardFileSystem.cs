namespace GameSpec.Blizzard
{
    /// <summary>
    /// BlizzardFileSystem
    /// </summary>
    /// <seealso cref="GameSpec.Family" />
    public class BlizzardFileSystem : FileManager.IFileSystem
    {
        public string[] GetDirectories(string path, string searchPattern, bool recursive)
        {
            throw new System.NotImplementedException();
        }

        public string[] GetFiles(string path, string searchPattern)
        {
            throw new System.NotImplementedException();
        }
    }
}