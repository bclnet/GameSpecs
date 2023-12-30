using GameSpec.Metadata;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OpenStack.Debug;

namespace GameSpec.Formats
{
    [DebuggerDisplay("{Name}")]
    public abstract class BinaryPakFile : PakFile
    {
        readonly ConcurrentDictionary<string, GenericPool<BinaryReader>> Readers = new ConcurrentDictionary<string, GenericPool<BinaryReader>>();
        public readonly IFileSystem FileSystem;
        public readonly string FilePath;
        public readonly PakBinary PakBinary;
        // options
        public bool UseReader = true;
        public bool UseFileId = false;

        // state
        public Func<string, string> FileMask;
        public readonly Dictionary<string, string> Params = new Dictionary<string, string>();
        public uint Magic;
        public uint Version;
        public object CryptKey;

        // metadata/factory
        internal protected Func<MetadataManager, BinaryPakFile, Task<List<MetadataItem>>> GetMetadataItemsMethod = StandardMetadataItem.GetPakFilesAsync;
        protected Dictionary<string, Func<MetadataManager, BinaryPakFile, FileSource, Task<List<MetadataInfo>>>> MetadataInfos = new Dictionary<string, Func<MetadataManager, BinaryPakFile, FileSource, Task<List<MetadataInfo>>>>();
        internal protected Func<FileSource, FamilyGame, (DataOption option, Func<BinaryReader, FileSource, PakFile, Task<object>> factory)> GetObjectFactoryFactory;

        // From: BinaryPakManyFile

        public override bool Valid => Files != null;
        public IList<FileSource> Files;
        public HashSet<string> FilesRawSet;
        public ILookup<int, FileSource> FilesById { get; private set; }
        public ILookup<string, FileSource> FilesByPath { get; private set; }
        public int PathSkip;

        /// <summary>
        /// Initializes a new instance of the <see cref="BinaryPakFile" /> class.
        /// </summary>
        /// <param name="game">The game.</param>
        /// <param name="filePath">The file path.</param>
        /// <param name="pakBinary">The pak binary.</param>
        /// <param name="tag">The tag.</param>
        /// <exception cref="ArgumentNullException">pakBinary</exception>
        public BinaryPakFile(FamilyGame game, IFileSystem fileSystem, string filePath, PakBinary pakBinary, object tag = default)
            : base(game, !string.IsNullOrEmpty(Path.GetFileName(filePath)) ? Path.GetFileName(filePath) : Path.GetFileName(Path.GetDirectoryName(filePath)), tag)
        {
            FileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
            FilePath = filePath;
            PakBinary = pakBinary;
        }

        /// <summary>
        /// Opens this instance.
        /// </summary>
        public override void Opening()
        {
            if (UseReader) GetBinaryReader()?.Action(async r => await ReadAsync(r));
            else ReadAsync(null).GetAwaiter().GetResult();
            Process();
        }

        /// <summary>
        /// Gets the binary reader.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        public GenericPool<BinaryReader> GetBinaryReader(string path = default, int retainInPool = 10)
            => Readers.GetOrAdd(path ?? FilePath, filePath => FileSystem.FileExists(filePath) ? new GenericPool<BinaryReader>(() => FileSystem.OpenReader(filePath), retainInPool) : default);

        /// <summary>
        /// Closes this instance.
        /// </summary>
        public override void Closing()
        {
            Files = null;
            FilesRawSet = null;
            FilesById = null;
            FilesByPath = null;
            foreach (var r in Readers.Values) r.Dispose();
            Readers.Clear();
        }

        /// <summary>
        /// Determines whether the pak contains the specified file path.
        /// </summary>
        /// <param name="path">The file path.</param>
        /// <returns>
        ///   <c>true</c> if the specified file path contains file; otherwise, <c>false</c>.
        /// </returns>
        public override bool Contains(string path) => TryFindSubPak(path ?? throw new ArgumentNullException(nameof(path)), out var pak, out var nextPath)
            ? pak.Contains(nextPath)
            : FilesByPath.Contains(path.Replace('\\', '/'));
        /// <summary>
        /// Determines whether the pak contains the specified file path.
        /// </summary>
        /// <param name="fileId">The fileId.</param>
        /// <returns>
        ///   <c>true</c> if the specified file path contains file; otherwise, <c>false</c>.
        /// </returns>
        public override bool Contains(int fileId) => FilesById.Contains(fileId);

        /// <summary>Gets the count.</summary>
        /// <value>The count.</value>
        /// <exception cref="System.NotSupportedException"></exception>
        public override int Count => FilesByPath.Count;

        // string or bytes
        /// <summary>
        /// The get string or bytes encoding
        /// </summary>
        public Encoding GetStringOrBytesEncoding = Encoding.UTF8;

        /// <summary>
        /// Gets the string or bytes.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <returns></returns>
        public virtual object GetStringOrBytes(Stream stream, bool dispose = true)
        {
            using var ms = new MemoryStream();
            stream.Position = 0;
            stream.CopyTo(ms);
            var bytes = ms.ToArray();
            if (dispose) stream.Dispose();
            return !bytes.Contains<byte>(0x00)
                ? (object)GetStringOrBytesEncoding.GetString(bytes)
                : bytes;
        }

        /// <summary>
        /// Finds the texture.
        /// </summary>
        /// <param name="path">The texture path.</param>
        /// <returns></returns>
        //public override string FindTexture(string path) => Contains(path) ? path : null;

        /// <summary>
        /// Loads the file data asynchronous.
        /// </summary>
        /// <param name="path">The file path.</param>
        /// <param name="option">The option.</param>
        /// <param name="exception">The exception.</param>
        /// <returns></returns>
        /// <exception cref="FileNotFoundException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public override Task<Stream> LoadFileDataAsync(string path, DataOption option = default, Action<FileSource, string> exception = default)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));
            if (TryFindSubPak(path, out var pak, out var nextPath)) return pak.LoadFileDataAsync(nextPath, option, exception);
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
        public override Task<Stream> LoadFileDataAsync(int fileId, DataOption option = default, Action<FileSource, string> exception = default)
        {
            var files = FilesById[fileId].ToArray();
            if (files.Length == 1) return LoadFileDataAsync(files[0], option, exception);
            exception?.Invoke(null, $"LoadFileDataAsync: {fileId}"); //Log($"LoadFileDataAsync: {fileId} @ {files.Length}");
            throw new FileNotFoundException(files.Length == 0 ? $"{fileId}" : $"More then one file found for {fileId}");
        }

        /// <summary>
        /// Loads the file data asynchronous.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="option">The file.</param>
        /// <param name="exception">The exception.</param>
        /// <returns></returns>
        public override Task<Stream> LoadFileDataAsync(FileSource file, DataOption option = default, Action<FileSource, string> exception = default)
            => UseReader
            ? GetBinaryReader().Func(r => ReadDataAsync(r, file, option, exception))
            : ReadDataAsync(null, file, option, exception);

        /// <summary>
        /// Loads the object asynchronous.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path">The file path.</param>
        /// <param name="exception">The exception.</param>
        /// <returns></returns>
        /// <exception cref="FileNotFoundException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public override Task<T> LoadFileObjectAsync<T>(string path, Action<FileSource, string> exception = default)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));
            if (TryFindSubPak(path, out var pak, out var nextPath)) return pak.LoadFileObjectAsync<T>(nextPath, exception);
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
        public override Task<T> LoadFileObjectAsync<T>(int fileId, Action<FileSource, string> exception = default)
        {
            var files = FilesById[fileId].ToArray();
            if (files.Length == 1) return LoadFileObjectAsync<T>(files[0], exception);
            exception?.Invoke(null, $"LoadFileObjectAsync: {fileId}"); //Log($"LoadFileObjectAsync: {fileId} @ {files.Length}");
            throw new FileNotFoundException(files.Length == 0 ? $"{fileId}" : $"More then one file found for {fileId}");
        }

        /// <summary>
        /// Ensures the file object factory.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <returns></returns>
        public Func<BinaryReader, FileSource, PakFile, Task<object>> EnsureCachedObjectFactory(FileSource file)
        {
            if (file.CachedObjectFactory != null) return file.CachedObjectFactory;

            var factory = GetObjectFactoryFactory(file, Game);
            file.CachedDataOption = factory.option;
            file.CachedObjectFactory = factory.factory ?? FileSource.EmptyObjectFactory;
            return file.CachedObjectFactory;
        }

        /// <summary>
        /// Loads the file data asynchronous.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="file">The file.</param>
        /// <param name="option">The option.</param>
        /// <param name="exception">The exception.</param>
        /// <returns></returns>
        public override async Task<T> LoadFileObjectAsync<T>(FileSource file, Action<FileSource, string> exception = default)
        {
            var type = typeof(T);
            var stream = await LoadFileDataAsync(file, 0, exception);
            if (stream == null) return default;
            var objectFactory = EnsureCachedObjectFactory(file);
            if (objectFactory == FileSource.EmptyObjectFactory)
                return type == typeof(Stream) || type == typeof(object)
                    ? (T)(object)stream
                    : throw new ArgumentOutOfRangeException(nameof(T), $"Stream not returned for {file.Path} with {type.Name}");
            var r = new BinaryReader(stream);
            object value = null;
            Task<object> task = null;
            try
            {
                task = objectFactory(r, file, this);
                if (task == null)
                    return type == typeof(Stream) || type == typeof(object)
                        ? (T)(object)stream
                        : throw new ArgumentOutOfRangeException(nameof(T), $"Stream not returned for {file.Path} with {type.Name}");
                value = await task;
                return value is T z ? z
                    : value is IRedirected<T> y ? y.Value
                    : throw new InvalidCastException();
            }
            catch (Exception e) { Log(e.Message); throw e; }
            finally { if (task != null && !(value != null && value is IDisposable)) r.Dispose(); }
        }

        /// <summary>
        /// Processes this instance.
        /// </summary>
        public virtual void Process()
        {
            if (UseFileId) FilesById = Files?.Where(x => x != null).ToLookup(x => x.Id);
            FilesByPath = Files?.Where(x => x != null).ToLookup(x => x.Path, StringComparer.OrdinalIgnoreCase);
            PakBinary?.Process(this);
        }

        /// <summary>
        /// Reads the asynchronous.
        /// </summary>
        /// <param name="r">The r.</param>
        /// <param name="tag">The tag.</param>
        /// <returns></returns>
        public virtual Task ReadAsync(BinaryReader r, object tag = default) => PakBinary.ReadAsync(this, r, tag);

        /// <summary>
        /// Writes the asynchronous.
        /// </summary>
        /// <param name="w">The w.</param>
        /// <param name="tag">The tag.</param>
        /// <returns></returns>
        public virtual Task WriteAsync(BinaryWriter w, object tag = default) => PakBinary.WriteAsync(this, w, tag);

        /// <summary>
        /// Reads the file data asynchronous.
        /// </summary>
        /// <param name="r">The r.</param>
        /// <param name="file">The file.</param>
        /// <param name="option">The option.</param>
        /// <param name="exception">The exception.</param>
        /// <returns></returns>
        public virtual Task<Stream> ReadDataAsync(BinaryReader r, FileSource file, DataOption option = default, Action<FileSource, string> exception = default) => PakBinary.ReadDataAsync(this, r, file, option, exception);

        /// <summary>
        /// Writes the file data asynchronous.
        /// </summary>
        /// <param name="w">The w.</param>
        /// <param name="file">The file.</param>
        /// <param name="data">The data.</param>
        /// <param name="option">The option.</param>
        /// <param name="exception">The exception.</param>
        /// <returns></returns>
        public virtual Task WriteDataAsync(BinaryWriter w, FileSource file, Stream data, DataOption option = default, Action<FileSource, string> exception = default) => PakBinary.WriteDataAsync(this, w, file, data, option, exception);

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

        bool TryFindSubPak(string path, out BinaryPakFile pak, out string nextPath)
        {
            var paths = path.Split(new[] { ':' }, 2);
            pak = paths.Length == 1 ? null : FilesByPath[paths[0].Replace('\\', '/')].FirstOrDefault()?.Pak;
            if (pak != null) { nextPath = paths[1]; return true; }
            nextPath = null;
            return false;
        }

        #region Metadata

        /// <summary>
        /// Gets the explorer information nodes.
        /// </summary>
        /// <param name="manager">The resource.</param>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public override async Task<List<MetadataInfo>> GetMetadataInfosAsync(MetadataManager manager, MetadataItem item)
        {
            if (!(item.Source is FileSource file)) return null;
            List<MetadataInfo> nodes = null;
            var obj = await LoadFileObjectAsync<object>(file);
            if (obj == null) return null;
            else if (obj is IGetMetadataInfo info) nodes = info.GetInfoNodes(manager, file);
            else if (obj is Stream stream)
            {
                var value = GetStringOrBytes(stream);
                nodes = value is string text ? new List<MetadataInfo> {
                        new MetadataInfo(null, new MetadataContent { Type = "Text", Name = "Text", Value = text }),
                        new MetadataInfo("Text", items: new List<MetadataInfo> {
                            new MetadataInfo($"Length: {text.Length}"),
                        }) }
                    : value is byte[] bytes ? new List<MetadataInfo> {
                        new MetadataInfo(null, new MetadataContent { Type = "Hex", Name = "Hex", Value = new MemoryStream(bytes) }),
                        new MetadataInfo("Bytes", items: new List<MetadataInfo> {
                            new MetadataInfo($"Length: {bytes.Length}"),
                        }) }
                    : throw new ArgumentOutOfRangeException(nameof(value), value.GetType().Name);
            }
            else if (obj is IDisposable disposable) disposable.Dispose();
            if (nodes == null) return null;
            nodes.Add(new MetadataInfo("File", items: new List<MetadataInfo> {
                new MetadataInfo($"Path: {file.Path}"),
                new MetadataInfo($"FileSize: {file.FileSize}"),
                file.Parts != null
                    ? new MetadataInfo("Parts", items: file.Parts.Select(part => new MetadataInfo($"{part.FileSize}@{part.Path}")))
                    : null
            }));
            //nodes.Add(new MetadataInfo(null, new MetadataContent { Type = "Hex", Name = "TEST", Value = new MemoryStream() }));
            //nodes.Add(new MetadataInfo(null, new MetadataContent { Type = "Image", Name = "TEST", MaxWidth = 500, MaxHeight = 500, Value = null }));
            return nodes;
        }

        /// <summary>
        /// Gets the explorer item nodes.
        /// </summary>
        /// <param name="manager">The resource.</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public override async Task<List<MetadataItem>> GetMetadataItemsAsync(MetadataManager manager)
            => Valid && GetMetadataItemsMethod != null ? await GetMetadataItemsMethod(manager, this) : default;

        #endregion
    }
}