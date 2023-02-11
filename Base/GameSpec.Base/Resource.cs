using System;

namespace GameSpec
{
    /// <summary>
    /// Resource
    /// </summary>
    public struct Resource
    {
        [Flags]
        public enum PakOption
        {
            Paths = 0x1,
            Stream = 0x2,
        }

        /// <summary>
        /// The pak options
        /// </summary>
        public PakOption Options;
        /// <summary>
        /// The host
        /// </summary>
        public Uri Host;
        /// <summary>
        /// The paths
        /// </summary>
        public string[] Paths;
        /// <summary>
        /// The game
        /// </summary>
        public FamilyGame Game;
    }
}