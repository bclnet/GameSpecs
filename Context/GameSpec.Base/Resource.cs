using static GameSpec.FileManager;

namespace GameSpec
{
    /// <summary>
    /// Resource
    /// </summary>
    public struct Resource
    {
        /// <summary>
        /// The filesystem.
        /// </summary>
        public IFileSystem FileSystem;
        /// <summary>
        /// The game.
        /// </summary>
        public FamilyGame Game;
        /// <summary>
        /// The search pattern.
        /// </summary>
        public string SearchPattern;
    }
}