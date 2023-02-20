using System;
using System.Collections.Generic;

namespace GameSpec
{
    /// <summary>
    /// FamilyGame
    /// </summary>
    public class FamilyGame
    {
        /// <summary>
        /// The game edition.
        /// </summary>
        public class Edition
        {
            /// <summary>
            /// The identifier
            /// </summary>
            public string Id { get; set; }
            /// <summary>
            /// The name
            /// </summary>
            public string Name { get; set; }
            /// <summary>
            /// The key
            /// </summary>
            public object Key { get; set; }
        }

        /// <summary>
        /// The game DLC.
        /// </summary>
        public class DownloadableContent
        {
            /// <summary>
            /// The identifier
            /// </summary>
            public string Id { get; set; }
            /// <summary>
            /// The name
            /// </summary>
            public string Name { get; set; }
        }

        /// <summary>
        /// The game locale.
        /// </summary>
        public class Locale
        {
            /// <summary>
            /// The identifier
            /// </summary>
            public string Id { get; set; }
            /// <summary>
            /// The name
            /// </summary>
            public string Name { get; set; }
        }

        /// <summary>
        /// Gets or sets the game identifier.
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// Gets or sets the game name.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Gets or sets the game engine.
        /// </summary>
        public string Engine { get; set; }
        /// <summary>
        /// Gets or sets the paks.
        /// </summary>
        public IList<Uri> Paks { get; set; }
        /// <summary>
        /// Gets or sets the dats.
        /// </summary>
        public IList<Uri> Dats { get; set; }
        /// <summary>
        /// Gets or sets the Paths.
        /// </summary>
        public IList<string> Paths { get; set; }
        /// <summary>
        /// Gets or sets the key.
        /// </summary>
        public object Key { get; set; }
        /// <summary>
        /// Determines if the game has been found.
        /// </summary>
        public bool Found { get; set; }
        /// <summary>
        /// Gets or sets the type of the file system.
        /// </summary>
        /// <value>
        /// Gets or sets the file system type.
        /// </value>
        public Type FileSystemType { get; set; }
        /// <summary>
        /// Gets the game editions.
        /// </summary>
        public IDictionary<string, Edition> Editions { get; set; }
        /// <summary>
        /// Gets the game dlcs.
        /// </summary>
        public IDictionary<string, DownloadableContent> Dlc { get; set; }
        /// <summary>
        /// Gets the game locales.
        /// </summary>
        public IDictionary<string, Locale> Locales { get; set; }
        /// <summary>
        /// Gets the displayed game name.
        /// </summary>
        /// <value>
        /// The name of the displayed.
        /// </value>
        public string DisplayedName => $"{Name}{(Found ? " - found" : null)}";

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString() => Name;

        #region FileSystem

        /// <summary>
        /// Creates the game file system.
        /// </summary>
        /// <returns></returns>
        public FileManager.IFileSystem CreateFileSystem() => FileSystemType != null ? (FileManager.IFileSystem)Activator.CreateInstance(FileSystemType) : null;

        #endregion
    }
}