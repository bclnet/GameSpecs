using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace GameSpec.Formats
{
    [DebuggerDisplay("{Name}")]
    public abstract class BinaryPakManyFile : BinaryPakFile
    {
        [Flags]
        protected enum PakManyOptions
        {
            FilesById = 0x1,
        }

        protected PakManyOptions Options { get; set; }
        public override bool Valid => Files != null;
        public IList<FileSource> Files;
        public HashSet<string> FilesRawSet;
        public ILookup<int, FileSource> FilesById { get; private set; }
        public ILookup<string, FileSource> FilesByPath { get; private set; }
        public int VisualPathSkip;

        /// <summary>
        /// Initializes a new instance of the <see cref="BinaryPakManyFile"/> class.
        /// </summary>
        /// <param name="game">The game.</param>
        /// <param name="fileSystem">The file system.</param>
        /// <param name="filePath">The file path.</param>
        /// <param name="pakBinary">The pak binary.</param>
        /// <param name="tag">The tag.</param>
        public BinaryPakManyFile(FamilyGame game, IFileSystem fileSystem, string filePath, PakBinary pakBinary, object tag = null) : base(game, fileSystem, filePath, pakBinary, tag) { }

        /// <summary>
        /// Closes this instance.
        /// </summary>
        public override void Closing()
        {
            Files = null;
            FilesRawSet = null;
            FilesById = null;
            FilesByPath = null;
            base.Closing();
        }

        /// <summary>
        /// Determines whether the pak contains the specified file path.
        /// </summary>
        /// <param name="path">The file path.</param>
        /// <returns>
        ///   <c>true</c> if the specified file path contains file; otherwise, <c>false</c>.
        /// </returns>
        public override bool Contains(string path) => TryLookupPath(path ?? throw new ArgumentNullException(nameof(path)), out var pak, out var nextPath)
            ? pak.Contains(nextPath)
            : FilesByPath.Contains(path.Replace('\\', '/'));
        /// <summary>
        /// Determines whether this instance contains the object.
        /// </summary>
        /// <param name="fileId">The fileId.</param>
        /// <returns>
        ///   <c>true</c> if the specified file path contains file; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="System.InvalidOperationException">FileId not supported</exception>
        public override bool Contains(int fileId) => FilesById.Contains(fileId);

        /// <summary>Gets the count.</summary>
        /// <value>The count.</value>
        public override int Count => FilesByPath.Count;

        /// <summary>
        /// Loads the file data asynchronous.
        /// </summary>
        /// <param name="path">The file path.</param>
        /// <param name="option">The option.</param>
        /// <param name="exception">The exception.</param>
        /// <returns></returns>
        /// <exception cref="FileNotFoundException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public override Task<Stream> LoadFileDataAsync(string path, DataOption option = 0, Action<FileSource, string> exception = null)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));
            if (TryLookupPath(path, out var pak, out var nextFilePath)) return pak.LoadFileDataAsync(nextFilePath, option, exception);
            var files = FilesByPath[path.Replace('\\', '/')].ToArray();
            if (files.Length == 1) return LoadFileDataAsync(files[0], option, exception);
            exception?.Invoke(null, $"LoadFileDataAsync: {path} @ {files.Length}"); //Log($"LoadFileDataAsync: {filePath} @ {files.Length}");
            throw new FileNotFoundException(files.Length == 0 ? path : $"More then one file found for {path}");
        }
        /// <summary>
        /// Loads the file data asynchronous.
        /// </summary>
        /// <param name="fileId">The fileId.</param>
        /// <param name="option">The option.</param>
        /// <param name="exception">The exception.</param>
        /// <returns></returns>
        /// <exception cref="FileNotFoundException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public override Task<Stream> LoadFileDataAsync(int fileId, DataOption option = 0, Action<FileSource, string> exception = null)
        {
            var files = FilesById[fileId].ToArray();
            if (files.Length == 1) return LoadFileDataAsync(files[0], option, exception);
            exception?.Invoke(null, $"LoadFileDataAsync: {fileId}"); //Log($"LoadFileDataAsync: {fileId} @ {files.Length}");
            throw new FileNotFoundException(files.Length == 0 ? $"{fileId}" : $"More then one file found for {fileId}");
        }

        /// <summary>
        /// Loads the object asynchronous.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path">The file path.</param>
        /// <param name="exception">The exception.</param>
        /// <returns></returns>
        /// <exception cref="FileNotFoundException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public override Task<T> LoadFileObjectAsync<T>(string path, Action<FileSource, string> exception = null)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));
            if (TryLookupPath(path, out var pak, out var nextFilePath)) return pak.LoadFileObjectAsync<T>(nextFilePath, exception);
            if (PathFinders.Count > 0) path = FindPath<T>(path);
            var files = FilesByPath[path.Replace('\\', '/')].ToArray();
            if (files.Length == 1) return LoadFileObjectAsync<T>(files[0], exception);
            exception?.Invoke(null, $"LoadFileObjectAsync: {path} @ {files.Length}"); //Log($"LoadFileObjectAsync: {filePath} @ {files.Length}");
            throw new FileNotFoundException(files.Length == 0 ? path : $"More then one file found for {path}");
        }
        /// <summary>
        /// Loads the object asynchronous.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fileId">The fileId.</param>
        /// <param name="exception">The exception.</param>
        /// <returns></returns>
        /// <exception cref="FileNotFoundException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public override Task<T> LoadFileObjectAsync<T>(int fileId, Action<FileSource, string> exception = null)
        {
            var files = FilesById[fileId].ToArray();
            if (files.Length == 1) return LoadFileObjectAsync<T>(files[0], exception);
            exception?.Invoke(null, $"LoadFileObjectAsync: {fileId}"); //Log($"LoadFileObjectAsync: {fileId} @ {files.Length}");
            throw new FileNotFoundException(files.Length == 0 ? $"{fileId}" : $"More then one file found for {fileId}");
        }

        /// <summary>
        /// Processes this instance.
        /// </summary>
        public override void Process()
        {
            if ((Options & PakManyOptions.FilesById) != 0) FilesById = Files?.Where(x => x != null).ToLookup(x => x.Id);
            FilesByPath = Files?.Where(x => x != null).ToLookup(x => x.Path, StringComparer.OrdinalIgnoreCase);
            base.Process();
        }

        /// <summary>
        /// Adds the raw file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="message">The message.</param>
        public void AddRawFile(FileSource file, string message)
        {
            if (file == null) throw new ArgumentNullException(nameof(file));
            lock (this)
            {
                if (FilesRawSet == null) FilesRawSet = new HashSet<string>();
                FilesRawSet.Add(file.Path);
            }
        }

        bool TryLookupPath(string path, out BinaryPakFile pak, out string nextPath)
        {
            var paths = path.Split(new[] { ':' }, 2);
            pak = paths.Length == 1 ? null : FilesByPath[paths[0].Replace('\\', '/')].FirstOrDefault()?.Pak;
            if (pak != null) { nextPath = paths[1]; return true; }
            nextPath = null;
            return false;
        }
    }
}