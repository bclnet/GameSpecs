using GameSpec.Formats;
using System;
using System.Collections.Generic;
using System.Linq;
using static GameSpec.FamilyManager;

namespace GameSpec
{
    /// <summary>
    /// FamilyGame
    /// </summary>
    public class FamilyGame
    {
        /// <summary>
        /// An empty family game.
        /// </summary>
        public static readonly FamilyGame Empty = new FamilyGame
        {
            Family = Family.Empty,
            Id = "Empty",
            Name = "Empty",
        };

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
        /// Gets or sets the family.
        /// </summary>
        public Family Family { get; set; }
        /// <summary>
        /// Gets or sets the game identifier.
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// Gets or sets if ignored.
        /// </summary>
        public bool Ignore { get; set; }
        /// <summary>
        /// Gets or sets the game name.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Gets or sets the game engine.
        /// </summary>
        public string Engine { get; set; }
        /// <summary>
        /// Gets or sets the game urls.
        /// </summary>
        public Uri[] Urls { get; set; }
        /// <summary>
        /// Gets or sets the game date.
        /// </summary>
        public DateTime Date { get; set; }
        /// <summary>
        /// Gets or sets the game type.
        /// </summary>
        public Type GameType { get; set; }
        /// <summary>
        /// Gets or sets the search by.
        /// </summary>
        public SearchBy SearchBy { get; set; }
        /// <summary>
        /// Gets or sets the pak option.
        /// </summary>
        public GameOption Option { get; set; }
        /// <summary>
        /// Gets or sets the pakFile type.
        /// </summary>
        public Type PakFileType { get; set; }
        /// <summary>
        /// Gets or sets the pak etxs.
        /// </summary>
        public string[] PakExts { get; set; }
        /// <summary>
        /// Gets or sets the paks.
        /// </summary>
        public Uri[] Paks { get; set; }
        /// <summary>
        /// Gets or sets the dats.
        /// </summary>
        public Uri[] Dats { get; set; }
        /// <summary>
        /// Gets or sets the Paths.
        /// </summary>
        public string[] Paths { get; set; }
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
        public IDictionary<string, DownloadableContent> Dlcs { get; set; }
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

        /// <summary>
        /// Ensures this instance.
        /// </summary>
        public virtual FamilyGame Ensure() => this;

        /// <summary>
        /// Converts the Paks to Application Paks.
        /// </summary>
        public IList<Uri> ToPaks() => Paks.Select(x => new Uri($"{x}#{Id}")).ToList();

        #region FileSystem

        /// <summary>
        /// Creates the game file system.
        /// </summary>
        /// <param name="paths">The paths.</param>
        /// <returns></returns>
        public IFileSystem CreateFileSystem(string root) => FileSystemType != null ? (IFileSystem)Activator.CreateInstance(FileSystemType, root) : new StandardFileSystem(root);

        /// <summary>
        /// Create pak file.
        /// </summary>
        /// <param name="fileSystem">The fileSystem.</param>
        /// <param name="value">The value.</param>
        /// <param name="tag">The tag.</param>
        /// <returns></returns>
        public PakFile CreatePakFile(IFileSystem fileSystem, object value, object tag = null) => value switch
        {
            string s when IsPakFile(s) => CreatePakFile(fileSystem, s, tag),
            string s => throw new InvalidOperationException($"{Id} missing {s}"),
            ValueTuple<string, string[]> s when s.Item2.Length == 1 && IsPakFile(s.Item2[0]) => CreatePakFile(fileSystem, (object)s.Item2[0], tag),
            ValueTuple<string, string[]> s => new ManyPakFile(CreatePakFile(fileSystem, "", tag), this, s.Item1.Length > 0 ? s.Item1 : "Many", fileSystem, s.Item2) { VisualPathSkip = s.Item1.Length > 0 ? s.Item1.Length + 1 : 0 },
            IList<PakFile> s when s.Count == 1 => s[0],
            IList<PakFile> s => new MultiPakFile(this, "Multi", fileSystem, s, tag),
            null => null,
            _ => throw new ArgumentOutOfRangeException(nameof(value), $"{value}"),
        };

        /// <summary>
        /// Create pak file.
        /// </summary>
        /// <param name="fileSystem">The fileSystem.</param>
        /// <param name="path">The path.</param>
        /// <param name="tag">The tag.</param>
        /// <returns></returns>
        public PakFile CreatePakFile(IFileSystem fileSystem, string path, object tag = null) => (PakFile)Activator.CreateInstance(PakFileType ?? throw new InvalidOperationException($"{Id} missing PakFileType"), this, fileSystem, path, tag);

        /// <summary>
        /// Is pak file.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        public bool IsPakFile(string path) => PakExts.Any(x => path.EndsWith(x, StringComparison.OrdinalIgnoreCase));

        /// <summary>
        /// Creates the search patterns.
        /// </summary>
        /// <param name="searchPattern">The search pattern.</param>
        /// <returns></returns>
        public string CreateSearchPatterns(string searchPattern)
        {
            if (string.IsNullOrEmpty(searchPattern))
            {
                if (PakExts == null || PakExts.Length == 0) return null;
                return SearchBy switch
                {
                    SearchBy.Pak => PakExts.Length == 1 ? $"*{PakExts[0]}" : $"(*{string.Join(":", PakExts)})",
                    SearchBy.TopDir => "*/*",
                    SearchBy.AllDir => "**/*",
                    _ => throw new ArgumentOutOfRangeException(nameof(SearchBy), $"{SearchBy}"),
                };
            }
            return null;

            //string ext;
            //if (string.IsNullOrEmpty(searchPattern)) {
            //    string.Join(PakEtxs).Select(x => x)
            //        }
            //    : !string.IsNullOrEmpty(ext = Path.GetExtension(searchPattern)) ? PakEtxs?.Select(x => x + ext).ToArray()
            //    : new string[] { searchPattern };
        }

        #endregion
    }
}