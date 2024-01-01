using GameSpec.Metadata;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Common;
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
        // metadata/factory
        protected Dictionary<string, Func<MetaManager, BinaryPakFile, FileSource, Task<List<MetaInfo>>>> MetaInfos = new Dictionary<string, Func<MetaManager, BinaryPakFile, FileSource, Task<List<MetaInfo>>>>();
        internal protected Func<FileSource, FamilyGame, (FileOption option, Func<BinaryReader, FileSource, PakFile, Task<object>> factory)> GetObjectFactoryFactory;
        // binary
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
        /// Valid
        /// </summary>
        public override bool Valid => Files != null;

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
        public override bool Contains(object path)
            => path switch
            {
                null => throw new ArgumentNullException(nameof(path)),
                string s => TryFindSubPak(s, out var pak, out var nextPath)
                    ? pak.Contains(nextPath)
                    : FilesByPath != null && FilesByPath.Contains(s.Replace('\\', '/')),
                int i => FilesById != null && FilesById.Contains(i),
                _ => throw new ArgumentOutOfRangeException(nameof(path))
            };

        /// <summary>Gets the count.</summary>
        /// <value>The count.</value>
        /// <exception cref="System.NotSupportedException"></exception>
        public override int Count => FilesByPath.Count;

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
        /// <returns></returns>
        /// <exception cref="FileNotFoundException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public override Task<Stream> LoadFileDataAsync(object path, FileOption option = default)
        {
            switch (path)
            {
                case null: throw new ArgumentNullException(nameof(path));
                case FileSource f:
                    {
                        return UseReader ? GetBinaryReader().Func(r => ReadDataAsync(r, f, option))
                            : ReadDataAsync(null, f, option);
                    }
                case string s:
                    {
                        if (TryFindSubPak(s, out var pak, out var nextPath)) return pak.LoadFileDataAsync(nextPath, option);
                        var files = FilesByPath[s.Replace('\\', '/')].ToArray();
                        if (files.Length == 1) return LoadFileDataAsync(files[0], option);
                        Log($"ERROR.LoadFileDataAsync: {s} @ {files.Length}");
                        throw new FileNotFoundException(files.Length == 0 ? s : $"More then one file found for {s}");
                    }
                case int i:
                    {
                        var files = FilesById[i].ToArray();
                        if (files.Length == 1) return LoadFileDataAsync(files[0], option);
                        Log($"ERROR.LoadFileDataAsync: {i} @ {files.Length}");
                        throw new FileNotFoundException(files.Length == 0 ? $"{i}" : $"More then one file found for {i}");
                    }
                default: throw new ArgumentOutOfRangeException(nameof(path));
            }
        }

        /// <summary>
        /// Loads the file object asynchronous.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path">The file.</param>
        /// <param name="option">The option.</param>
        /// <returns></returns>
        public override async Task<T> LoadFileObjectAsync<T>(object path, FileOption option = default)
        {
            switch (path)
            {
                case null: throw new ArgumentNullException(nameof(path));
                case FileSource f:
                    {
                        var type = typeof(T);
                        var data = await LoadFileDataAsync(f, option);
                        if (data == null) return default;
                        var objectFactory = EnsureCachedObjectFactory(f);
                        if (objectFactory == FileSource.EmptyObjectFactory)
                            return type == typeof(Stream) || type == typeof(object)
                                ? (T)(object)data
                                : throw new ArgumentOutOfRangeException(nameof(T), $"Stream not returned for {f.Path} with {type.Name}");
                        var r = new BinaryReader(data);
                        object value = null;
                        Task<object> task = null;
                        try
                        {
                            task = objectFactory(r, f, this);
                            if (task == null)
                                return type == typeof(Stream) || type == typeof(object)
                                    ? (T)(object)data
                                    : throw new ArgumentOutOfRangeException(nameof(T), $"Stream not returned for {f.Path} with {type.Name}");
                            value = await task;
                            return value is T z ? z
                                : value is IRedirected<T> y ? y.Value
                                : throw new InvalidCastException();
                        }
                        catch (Exception e) { Log(e.Message); throw e; }
                        finally { if (task != null && !(value != null && value is IDisposable)) r.Dispose(); }
                    }
                case string s:
                    {
                        if (TryFindSubPak(s, out var pak, out var nextPath)) return await pak.LoadFileObjectAsync<T>(nextPath);
                        if (PathFinders.Count > 0) path = FindPath<T>(s);
                        var files = FilesByPath[s.Replace('\\', '/')].ToArray();
                        if (files.Length == 1) return await LoadFileObjectAsync<T>(files[0], option);
                        Log($"ERROR.LoadFileObjectAsync: {s} @ {files.Length}");
                        throw new FileNotFoundException(files.Length == 0 ? s : $"More then one file found for {s}");
                    }
                case int i:
                    {
                        var files = FilesById[i].ToArray();
                        if (files.Length == 1) return await LoadFileObjectAsync<T>(files[0], option);
                        Log($"LoadFileObjectAsync: {i} @ {files.Length}");
                        throw new FileNotFoundException(files.Length == 0 ? $"{i}" : $"More then one file found for {i}");
                    }
                default: throw new ArgumentOutOfRangeException(nameof(path));
            }
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
        /// Processes this instance.
        /// </summary>
        public virtual void Process()
        {
            if (UseFileId) FilesById = Files?.Where(x => x != null).ToLookup(x => x.Id);
            FilesByPath = Files?.Where(x => x != null).ToLookup(x => x.Path, StringComparer.OrdinalIgnoreCase);
            PakBinary?.Process(this);
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

        bool TryFindSubPak(string path, out BinaryPakFile pak, out string nextPath)
        {
            var paths = path.Split(new[] { ':' }, 2);
            pak = paths.Length == 1 ? null : FilesByPath[paths[0].Replace('\\', '/')].FirstOrDefault()?.Pak;
            if (pak != null) { nextPath = paths[1]; return true; }
            nextPath = null;
            return false;
        }

        #region PakBinary

        /// <summary>
        /// Reads the asynchronous.
        /// </summary>
        /// <param name="r">The r.</param>
        /// <param name="tag">The tag.</param>
        /// <returns></returns>
        public virtual Task ReadAsync(BinaryReader r, object tag = default) => PakBinary.ReadAsync(this, r, tag);

        /// <summary>
        /// Reads the file data asynchronous.
        /// </summary>
        /// <param name="r">The r.</param>
        /// <param name="file">The file.</param>
        /// <param name="option">The option.</param>
        /// <returns></returns>
        public virtual Task<Stream> ReadDataAsync(BinaryReader r, FileSource file, FileOption option = default) => PakBinary.ReadDataAsync(this, r, file, option);

        /// <summary>
        /// Writes the asynchronous.
        /// </summary>
        /// <param name="w">The w.</param>
        /// <param name="tag">The tag.</param>
        /// <returns></returns>
        public virtual Task WriteAsync(BinaryWriter w, object tag = default) => PakBinary.WriteAsync(this, w, tag);

        /// <summary>
        /// Writes the file data asynchronous.
        /// </summary>
        /// <param name="w">The w.</param>
        /// <param name="file">The file.</param>
        /// <param name="data">The data.</param>
        /// <param name="option">The option.</param>
        /// <returns></returns>
        public virtual Task WriteDataAsync(BinaryWriter w, FileSource file, Stream data, FileOption option = default) => PakBinary.WriteDataAsync(this, w, file, data, option);

        #endregion

        #region Metadata

        /// <summary>
        /// Gets the explorer information nodes.
        /// </summary>
        /// <param name="manager">The manager.</param>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public override Task<List<MetaInfo>> GetMetaInfosAsync(MetaManager manager, MetaItem item)
            => Valid ? MetaManager.GetMetaInfosAsync(manager, this, item.Source as FileSource) : default;

        /// <summary>
        /// Gets the explorer item nodes.
        /// </summary>
        /// <param name="manager">The resource.</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public override Task<List<MetaItem>> GetMetaItemsAsync(MetaManager manager)
            => Valid ? MetaManager.GetMetaItemsAsync(manager, this) : default;

        #endregion
    }
}