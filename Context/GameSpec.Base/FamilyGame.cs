using GameSpec.Formats;
using GameSpec.Platforms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using static GameSpec.FamilyManager;
using static GameSpec.Util;

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

            /// <summary>
            /// Edition
            /// </summary>
            /// <param name="id"></param>
            /// <param name="elem"></param>
            /// <exception cref="ArgumentNullException"></exception>
            public Edition(string id, JsonElement elem)
            {
                Id = id;
                Name = _value(elem, "name") ?? id;
                Key = _method(elem, "key", ParseKey);
            }
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
            /// <summary>
            /// The Path
            /// </summary>
            public string Path { get; set; }

            /// <summary>
            /// DownloadableContent
            /// </summary>
            /// <param name="id"></param>
            /// <param name="elem"></param>
            /// <exception cref="ArgumentNullException"></exception>
            public DownloadableContent(string id, JsonElement elem)
            {
                Id = id;
                Name = _value(elem, "name") ?? id;
                Path = _value(elem, "path");
            }
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

            /// <summary>
            /// Locale
            /// </summary>
            /// <param name="id"></param>
            /// <param name="elem"></param>
            /// <exception cref="ArgumentNullException"></exception>
            public Locale(string id, JsonElement elem)
            {
                Id = id;
                Name = _value(elem, "name") ?? id;
            }
        }

        /// Gets or sets the game type.
        /// </summary>
        public Type GameType { get; set; }
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
        /// Gets or sets the game resource.
        /// </summary>
        public string Resource { get; set; }
        /// <summary>
        /// Gets or sets the game urls.
        /// </summary>
        public Uri[] Urls { get; set; }
        /// <summary>
        /// Gets or sets the game date.
        /// </summary>
        public DateTime Date { get; set; }
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
        /// Gets or sets the Status.
        /// </summary>
        //public string[] Status { get; set; }
        /// <summary>
        /// Gets or sets the Tags.
        /// </summary>
        public string[] Tags { get; set; }
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
        /// FamilyGame
        /// </summary>
        internal FamilyGame() { }
        /// <summary>
        /// FamilyGame
        /// </summary>
        /// <param name="family"></param>
        /// <param name="id"></param>
        /// <param name="elem"></param>
        /// <param name="dgame"></param>
        public FamilyGame(Family family, string id, JsonElement elem, FamilyGame dgame)
        {
            Family = family;
            Id = id;
            Ignore = _valueBool(elem, "n/a", dgame.Ignore);
            Name = _value(elem, "name");
            Engine = _value(elem, "engine", dgame.Engine);
            Resource = _value(elem, "resource", dgame.Resource);
            Urls = _list(elem, "url", x => new Uri(x));
            Date = _value(elem, "date", z => DateTime.Parse(z.GetString()));
            Option = _value(elem, "option", z => Enum.TryParse<GameOption>(z.GetString(), true, out var zT) ? zT : throw new ArgumentOutOfRangeException("option", $"Unknown option: {z}"), dgame.Option);
            Paks = _list(elem, "pak", x => new Uri(x), dgame.Paks);
            Dats = _list(elem, "dat", x => new Uri(x), dgame.Dats);
            Paths = _list(elem, "path", dgame.Paths);
            Key = _method(elem, "key", ParseKey, dgame.Key);
            //Status = _value(elem, "status");
            Tags = _value(elem, "tags", string.Empty).Split(' ');
            // interface
            FileSystemType = _value(elem, "fileSystemType", z => Type.GetType(z.GetString(), false) ?? throw new ArgumentOutOfRangeException("fileSystemType", $"Unknown type: {z}"), dgame.FileSystemType);
            SearchBy = _value(elem, "searchBy", z => Enum.TryParse<SearchBy>(z.GetString(), true, out var zS) ? zS : throw new ArgumentOutOfRangeException("searchBy", $"Unknown option: {z}"), dgame.SearchBy);
            PakFileType = _value(elem, "pakFileType", z => Type.GetType(z.GetString(), false) ?? throw new ArgumentOutOfRangeException("pakFileType", $"Unknown type: {z}"), dgame.PakFileType);
            PakExts = _list(elem, "pakExt", dgame.PakExts);
            // related
            Editions = _related(elem, "editions", (k, v) => new Edition(k, v));
            Dlcs = _related(elem, "dlcs", (k, v) => new DownloadableContent(k, v));
            Locales = _related(elem, "locals", (k, v) => new Locale(k, v));
        }

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
        public IList<Uri> ToPaks(string edition) => Paks.Select(x => new Uri($"{x}#{Id}")).ToList();

        /// <summary>
        /// Gets a family sample
        /// </summary>
        public FamilySample.File GetSample(string id)
        {
            if (!Family.Samples.TryGetValue(Id, out var samples) || samples.Count == 0) return null;
            var idx = id == "*" ? new Random((int)DateTime.Now.Ticks).Next(samples.Count) : int.Parse(id);
            return samples.Count > idx ? samples[idx] : null;
        }

        #region Pak

        /// <summary>
        /// Adds the platform graphic.
        /// </summary>
        /// <param name="pakFile">The pak file.</param>
        /// <returns></returns>
        static PakFile WithPlatformGraphic(PakFile pakFile)
        {
            if (pakFile != null) pakFile.Graphic = Platform.GraphicFactory?.Invoke(pakFile);
            return pakFile;
        }

        /// <summary>
        /// Create pak file.
        /// </summary>
        /// <param name="fileSystem">The fileSystem.</param>
        /// <param name="edition">The edition.</param>
        /// <param name="searchPattern">The search pattern.</param>
        /// <param name="throwOnError">Throws on error.</param>
        /// <returns></returns>
        internal PakFile CreatePakFile(IFileSystem fileSystem, Edition edition, string searchPattern, bool throwOnError)
        {
            if (fileSystem is HostFileSystem k) throw new NotImplementedException($"{k}"); //return new StreamPakFile(family.FileManager.HostFactory, game, path, fileSystem),
            searchPattern = CreateSearchPatterns(searchPattern);
            if (searchPattern == null) return default;
            var pakFiles = new List<PakFile>();
            foreach (var key in (new string[] { null }).Concat(Dlcs.Keys))
                foreach (var p in FindPaths(fileSystem, edition, key != null ? Dlcs[key] : null, searchPattern))
                    switch (SearchBy)
                    {
                        case SearchBy.Pak:
                            foreach (var path in p.paths)
                                if (IsPakFile(path)) pakFiles.Add(CreatePakFileObj(fileSystem, path));
                            break;
                        default:
                            pakFiles.Add(CreatePakFileObj(fileSystem, p));
                            break;
                    }
            return WithPlatformGraphic(CreatePakFileObj(fileSystem, pakFiles));
        }

        /// <summary>
        /// Create pak file.
        /// </summary>
        /// <param name="fileSystem">The fileSystem.</param>
        /// <param name="value">The value.</param>
        /// <param name="tag">The tag.</param>
        /// <returns></returns>
        public PakFile CreatePakFileObj(IFileSystem fileSystem, object value, object tag = null) => value switch
        {
            string s => IsPakFile(s)
                ? CreatePakFileType(fileSystem, s, tag)
                : throw new InvalidOperationException($"{Id} missing {s}"),
            ValueTuple<string, string[]> s => s.Item2.Length == 1 && IsPakFile(s.Item2[0])
                ? CreatePakFileObj(fileSystem, s.Item2[0], tag)
                : new ManyPakFile(CreatePakFileType(fileSystem, null, tag), this, s.Item1.Length > 0 ? s.Item1 : "Many", fileSystem, s.Item2)
                {
                    PathSkip = s.Item1.Length > 0 ? s.Item1.Length + 1 : 0
                },
            IList<PakFile> s => s.Count == 1
                ? s[0]
                : new MultiPakFile(this, "Multi", fileSystem, s, tag),
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
        public PakFile CreatePakFileType(IFileSystem fileSystem, string path, object tag = null) => (PakFile)Activator.CreateInstance(PakFileType ?? throw new InvalidOperationException($"{Id} missing PakFileType"), this, fileSystem, path, tag);

        /// <summary>
        /// Is pak file.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        public bool IsPakFile(string path) => PakExts != null && PakExts.Any(x => path.EndsWith(x, StringComparison.OrdinalIgnoreCase));

        /// <summary>
        /// Find the games paths.
        /// </summary>
        /// <param name="fileSystem">The fileSystem.</param>
        /// <param name="edition">The edition.</param>
        /// <param name="searchPattern">The search pattern.</param>
        /// <returns></returns>
        public IEnumerable<(string root, string[] paths)> FindPaths(IFileSystem fileSystem, Edition edition, DownloadableContent dlc, string searchPattern)
        {
            var gameIgnores = Family.FileManager.Ignores.TryGetValue(Id, out var z) ? z : null;
            foreach (var path in Paths ?? new[] { "" })
            {
                var searchPath = dlc?.Path != null ? Path.Join(path, dlc.Path) : path;
                var fileSearch = fileSystem.FindPaths(searchPath, searchPattern);
                if (gameIgnores != null) fileSearch = fileSearch.Where(x => !gameIgnores.Contains(Path.GetFileName(x)));
                yield return (path, fileSearch.ToArray());
            }
        }

        /// <summary>
        /// Creates the search patterns.
        /// </summary>
        /// <param name="searchPattern">The search pattern.</param>
        /// <returns></returns>
        public string CreateSearchPatterns(string searchPattern)
        {
            if (!string.IsNullOrEmpty(searchPattern)) return searchPattern;
            return SearchBy switch
            {
                SearchBy.None => null,
                SearchBy.Pak => PakExts == null || PakExts.Length == 0 ? ""
                    : PakExts.Length == 1 ? $"*{PakExts[0]}" : $"(*{string.Join(":*", PakExts)})",
                SearchBy.TopDir => "*",
                SearchBy.TwoDir => "*/*",
                SearchBy.AllDir => "**/*",
                _ => throw new ArgumentOutOfRangeException(nameof(SearchBy), $"{SearchBy}"),
            };
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