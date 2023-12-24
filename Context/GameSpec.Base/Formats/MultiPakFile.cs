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
        /// <param name="game">The game.</param>
        /// <param name="name">The name.</param>
        /// <param name="fileSystem">The file system.</param>
        /// <param name="pakFiles">The packs.</param>
        /// <param name="tag">The tag.</param>
        public MultiPakFile(FamilyGame game, string name, IFileSystem fileSystem, IList<PakFile> pakFiles, object tag = null) : base(game, name, tag) => PakFiles = pakFiles;

        /// <summary>
        /// Closes this instance.
        /// </summary>
        public override void Closing()
        {
            if (PakFiles != null) foreach (var pakFile in PakFiles) pakFile.Close();
        }

        /// <summary>
        /// Opens this instance.
        /// </summary>
        public override void Opening()
        {
            if (PakFiles != null) foreach (var pakFile in PakFiles) pakFile.Open();
        }

        /// <summary>
        /// Determines whether the specified file path contains file.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>
        ///   <c>true</c> if the specified file path contains file; otherwise, <c>false</c>.
        /// </returns>
        public override bool Contains(string path) => FilterPakFiles(path ?? throw new ArgumentNullException(nameof(path)), out var nextPath)
            .Any(x => x.Valid && x.Contains(nextPath));

        /// <summary>
        /// Determines whether the specified fileId contains file.
        /// </summary>
        /// <param name="fileId">The fileId.</param>
        /// <returns>
        ///   <c>true</c> if the specified file path contains file; otherwise, <c>false</c>.
        /// </returns>
        public override bool Contains(int fileId) => PakFiles.Any(x => x.Valid && x.Contains(fileId));

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
        public override Task<Stream> LoadFileDataAsync(string path, DataOption option = 0, Action<FileSource, string> exception = null) =>
            (FilterPakFiles(path ?? throw new ArgumentNullException(nameof(path)), out var nextPath).FirstOrDefault(x => x.Valid && x.Contains(nextPath)) ?? throw new FileNotFoundException($"Could not find file \"{path}\"."))
            .LoadFileDataAsync(nextPath, option, exception);
        /// <summary>
        /// Loads the file data asynchronous.
        /// </summary>
        /// <param name="fileId">The fileId.</param>
        /// <param name="option">The option.</param>
        /// <param name="exception">The exception.</param>
        /// <returns></returns>
        /// <exception cref="System.IO.FileNotFoundException">Could not find file \"{fileId}\".</exception>
        public override Task<Stream> LoadFileDataAsync(int fileId, DataOption option = 0, Action<FileSource, string> exception = null) =>
            (PakFiles.FirstOrDefault(x => x.Valid && x.Contains(fileId)) ?? throw new FileNotFoundException($"Could not find file \"{fileId}\"."))
            .LoadFileDataAsync(fileId, option, exception);
        /// <summary>
        /// Loads the file data asynchronous.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="option">The option.</param>
        /// <param name="exception">The exception.</param>
        /// <returns></returns>
        /// <exception cref="System.IO.FileNotFoundException">Could not find file \"{path}\".</exception>
        public override Task<Stream> LoadFileDataAsync(FileSource file, DataOption option = 0, Action<FileSource, string> exception = null) => throw new NotSupportedException();

        /// <summary>
        /// Loads the object asynchronous.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path">The path.</param>
        /// <param name="exception">The exception.</param>
        /// <returns></returns>
        /// <exception cref="FileNotFoundException">Could not find file \"{path}\".</exception>
        public override Task<T> LoadFileObjectAsync<T>(string path, Action<FileSource, string> exception) =>
            (FilterPakFiles(path ?? throw new ArgumentNullException(nameof(path)), out var nextPath).FirstOrDefault(x => x.Valid && x.Contains(nextPath)) ?? throw new FileNotFoundException($"Could not find file \"{path}\"."))
            .LoadFileObjectAsync<T>(nextPath, exception);
        /// <summary>
        /// Loads the object asynchronous.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fileId">The fileId.</param>
        /// <param name="exception">The exception.</param>
        /// <returns></returns>
        /// <exception cref="FileNotFoundException">Could not find file \"{fileId}\".</exception>
        public override Task<T> LoadFileObjectAsync<T>(int fileId, Action<FileSource, string> exception) =>
            (PakFiles.FirstOrDefault(x => x.Valid && x.Contains(fileId)) ?? throw new FileNotFoundException($"Could not find file \"{fileId}\"."))
            .LoadFileObjectAsync<T>(fileId, exception);
        /// <summary>
        /// Loads the object asynchronous.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="file">The file.</param>
        /// <param name="exception">The exception.</param>
        /// <returns></returns>
        /// <exception cref="FileNotFoundException">Could not find file \"{path}\".</exception>
        public override Task<T> LoadFileObjectAsync<T>(FileSource file, Action<FileSource, string> exception) => throw new NotSupportedException();

        #region Metadata

        /// <summary>
        /// Gets the metadata items.
        /// </summary>
        /// <param name="manager">The resource.</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public override async Task<List<MetadataItem>> GetMetadataItemsAsync(MetadataManager manager)
        {
            var root = new List<MetadataItem>();
            foreach (var pakFile in PakFiles.Where(x => x.Valid))
                root.Add(new MetadataItem(pakFile, pakFile.Name, manager.PackageIcon, items: await pakFile.GetMetadataItemsAsync(manager)) { PakFile = pakFile });
            return root;
        }

        #endregion
    }
}