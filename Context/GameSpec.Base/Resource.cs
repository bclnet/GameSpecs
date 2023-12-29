namespace GameSpec
{
    /// <summary>
    /// Resource
    /// </summary>
    public struct Resource
    {
        /// <summary>
        /// The game edition.
        /// </summary>
        public FamilyGame.Edition Edition;
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