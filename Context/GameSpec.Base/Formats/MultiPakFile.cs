using GameSpec.Metadata;
using System;
using System.Collections.Generic;
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
        /// <param name="fileSystem">The file system.</param>
        /// <param name="game">The game.</param>
        /// <param name="name">The name.</param>
        /// <param name="pakFiles">The packs.</param>
        /// <param name="tag">The tag.</param>
        public MultiPakFile(IFileSystem fileSystem, FamilyGame game, FamilyGame.Edition edition, string name, IList<PakFile> pakFiles, object tag = null)
            : base(fileSystem, game, edition, name, tag) => PakFiles = pakFiles ?? throw new ArgumentNullException(nameof(pakFiles));

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
                string s => FindPakFiles(s, out var nextPath).Any(x => x.Valid && x.Contains(nextPath)),
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

        IList<PakFile> FindPakFiles(string path, out string nextPath)
        {
            var paths = path.Split(new[] { '\\', '/', ':' }, 2);
            if (paths.Length == 1) { nextPath = path; return PakFiles; }
            path = paths[0]; nextPath = paths[1];
            var pakFiles = PakFiles.Where(x => x.Name.StartsWith(path)).ToList();
            foreach (var pakFile in pakFiles) pakFile.Open();
            return pakFiles;
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
                string s => (FindPakFiles(s, out var s2).FirstOrDefault(x => x.Valid && x.Contains(s2)) ?? throw new FileNotFoundException($"Could not find file \"{s}\"."))
                    .LoadFileData(s2, option),
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
                string s => (FindPakFiles(s, out var s2).FirstOrDefault(x => x.Valid && x.Contains(s2)) ?? throw new FileNotFoundException($"Could not find file \"{s}\"."))
                    .LoadFileObject<T>(s2, option),
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