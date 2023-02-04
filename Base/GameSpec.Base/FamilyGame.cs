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
        /// Edition
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
        /// Edition
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
        /// The identifier
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// The name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// The engine
        /// </summary>
        public string Engine { get; set; }
        /// <summary>
        /// The paks
        /// </summary>
        public IList<Uri> Paks { get; set; }
        /// <summary>
        /// The dats
        /// </summary>
        public IList<Uri> Dats { get; set; }
        /// <summary>
        /// The key
        /// </summary>
        public object Key { get; set; }
        /// <summary>
        /// The has location
        /// </summary>
        public bool Found { get; set; }
        /// <summary>
        /// Gets the type of the file system.
        /// </summary>
        /// <value>
        /// The type of the file system.
        /// </value>
        public Type FileSystemType { get; set; }
        /// <summary>
        /// The Editions
        /// </summary>
        public IDictionary<string, Edition> Editions { get; set; }
        /// <summary>
        /// The Locales
        /// </summary>
        public IDictionary<string, Locale> Locales { get; set; }

        /// <summary>
        /// Gets the name of the displayed.
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
        /// File system factory.
        /// </summary>
        /// <param name="game">The game.</param>
        /// <returns></returns>
        public FileManager.IFileSystem CreateFileSystem() => FileSystemType != null ? (FileManager.IFileSystem)Activator.CreateInstance(FileSystemType) : null;

        #endregion
    }
}