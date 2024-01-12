using GameSpec.Metadata;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace GameSpec.Formats
{
    [DebuggerDisplay("Paks: {Paks.Count}")]
    public class MultiPakFile : PakFile
    {
        /// <summary>
        /// The paks
        /// </summary>
        public readonly IList<PakFile> PakFiles;

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiPakFile" /> class.
        /// </summary>
        /// <param name="game">The game.</param>
        /// <param name="name">The name.</param>
        /// <param name="fileSystem">The file system.</param>
        /// <param name="pakFiles">The packs.</param>
        /// <param name="tag">The tag.</param>
        public MultiPakFile(FamilyGame game, string name, IFileSystem fileSystem, IList<PakFile> pakFiles, object tag = null) : base(game, name, tag) => PakFiles = pakFiles ?? throw new ArgumentNullException(nameof(pakFiles));

        /// <summary>
        /// Closes this instance.
        /// </summary>
        public override void Closing()
        {
            foreach (var pakFile in PakFiles) pakFile.Close();
        }

        /// <summary>
        /// Opens this instance.
        /// </summary>
        public override void Opening()
        {
            foreach (var pakFile in PakFiles) pakFile.Open();
        }

        /// <summary>
        /// Determines whether the specified file path contains file.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>
        ///   <c>true</c> if the specified file path contains file; otherwise, <c>false</c>.
        /// </returns>
        public override bool Contains(object path)
            => path switch
            {
                null => throw new ArgumentNullException(nameof(path)),
                string s => FilterPakFiles(s, out var nextPath).Any(x => x.Valid && x.Contains(nextPath)),
                int i => PakFiles.Any(x => x.Valid && x.Contains(i)),
                _ => throw new ArgumentOutOfRangeException(nameof(path)),
            };

        /// <summary>
        /// Gets the count.
        /// </summary>
        /// <value>
        /// The count.
        /// </value>
        public override int Count
        {
            get { var count = 0; foreach (var pakFile in PakFiles) count += pakFile.Count; return count; }
        }

        IList<PakFile> FilterPakFiles(string path, out string nextPath)
        {
            if (!path.StartsWith('>')) { nextPath = path; return PakFiles; }
            var paths = path[1..].Split(new[] { ':' }, 2);
            if (paths.Length != 2) throw new ArgumentException("missing :", nameof(path));
            path = paths[0];
            nextPath = paths[1];
            return PakFiles.Where(x => x.Name.StartsWith(path)).ToList();
        }

        /// <summary>
        /// Loads the file data asynchronous.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="option">The option.</param>
        /// <param name="exception">The exception.</param>
        /// <returns></returns>
        /// <exception cref="System.IO.FileNotFoundException">Could not find file \"{path}\".</exception>
        public override Task<Stream> LoadFileData(object path, FileOption option = default)
            => path switch
            {
                null => throw new ArgumentNullException(nameof(path)),
                string s => (FilterPakFiles(s, out var nextPath).FirstOrDefault(x => x.Valid && x.Contains(nextPath)) ?? throw new FileNotFoundException($"Could not find file \"{s}\"."))
                    .LoadFileData(nextPath, option),
                int i => (PakFiles.FirstOrDefault(x => x.Valid && x.Contains(i)) ?? throw new FileNotFoundException($"Could not find file \"{i}\"."))
                    .LoadFileData(i, option),
                _ => throw new ArgumentOutOfRangeException(nameof(path)),
            };

        /// <summary>
        /// Loads the object asynchronous.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="option">The option.</param>
        /// <returns></returns>
        /// <exception cref="System.IO.FileNotFoundException">Could not find file \"{path}\".</exception>
        public override Task<T> LoadFileObject<T>(object path, FileOption option = default)
            => path switch
            {
                null => throw new ArgumentNullException(nameof(path)),
                string s => (FilterPakFiles(s, out var nextPath).FirstOrDefault(x => x.Valid && x.Contains(nextPath)) ?? throw new FileNotFoundException($"Could not find file \"{s}\"."))
                    .LoadFileObject<T>(nextPath, option),
                int i => (PakFiles.FirstOrDefault(x => x.Valid && x.Contains(i)) ?? throw new FileNotFoundException($"Could not find file \"{i}\"."))
                    .LoadFileObject<T>(i, option),
                _ => throw new ArgumentOutOfRangeException(nameof(path)),
            };

        #region Metadata

        /// <summary>
        /// Gets the metadata items.
        /// </summary>
        /// <param name="manager">The resource.</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public override List<MetaItem> GetMetaItems(MetaManager manager)
        {
            var root = new List<MetaItem>();
            foreach (var pakFile in PakFiles.Where(x => x.Valid))
                root.Add(new MetaItem(pakFile, pakFile.Name, manager.PackageIcon, pakFile: pakFile, items: pakFile.GetMetaItems(manager)));
            return root;
        }

        #endregion
    }
}