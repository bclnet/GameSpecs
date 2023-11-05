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
        readonly ConcurrentDictionary<string, GenericPool<BinaryReader>> BinaryReaders = new ConcurrentDictionary<string, GenericPool<BinaryReader>>();
        public readonly IFileSystem FileSystem;
        public readonly string FilePath;
        public readonly PakBinary PakBinary;

        // state
        public bool UseBinaryReader = true;
        public Func<string, string> FileMask;
        public readonly Dictionary<string, string> Params = new Dictionary<string, string>();
        public uint Magic;
        public uint Version;
        public object CryptKey;

        // metadata
        internal protected Func<MetadataManager, BinaryPakFile, Task<List<MetadataItem>>> GetMetadataItems;
        protected Dictionary<string, Func<MetadataManager, BinaryPakFile, FileMetadata, Task<List<MetadataInfo>>>> MetadataInfos = new Dictionary<string, Func<MetadataManager, BinaryPakFile, FileMetadata, Task<List<MetadataInfo>>>>();

        // object-factory
        internal protected Func<FileMetadata, FamilyGame, (DataOption option, Func<BinaryReader, FileMetadata, PakFile, Task<object>> factory)> GetObjectFactoryFactory;

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
            FilePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
            PakBinary = pakBinary;
        }

        /// <summary>
        /// Opens this instance.
        /// </summary>
        public override void Opening()
        {
            if (UseBinaryReader) GetBinaryReader()?.Action(async r => await ReadAsync(r));
            else ReadAsync(null).GetAwaiter().GetResult();
            Process();
        }

        /// <summary>
        /// Gets the binary reader.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        public GenericPool<BinaryReader> GetBinaryReader(string path = default, int retainInPool = 10)
            => BinaryReaders.GetOrAdd(path ?? FilePath, filePath => FileSystem.FileExists(filePath) ? new GenericPool<BinaryReader>(() => FileSystem.OpenReader(filePath), retainInPool) : default);

        /// <summary>
        /// Closes this instance.
        /// </summary>
        public override void Closing()
        {
            foreach (var r in BinaryReaders.Values) r.Dispose();
            BinaryReaders.Clear();
        }

        /// <summary>
        /// Determines whether the pak contains the specified file path.
        /// </summary>
        /// <param name="path">The file path.</param>
        /// <returns>
        ///   <c>true</c> if the specified file path contains file; otherwise, <c>false</c>.
        /// </returns>
        public override bool Contains(string path) => throw new NotSupportedException();
        /// <summary>
        /// Determines whether the pak contains the specified file path.
        /// </summary>
        /// <param name="fileId">The fileId.</param>
        /// <returns>
        ///   <c>true</c> if the specified file path contains file; otherwise, <c>false</c>.
        /// </returns>
        public override bool Contains(int fileId) => throw new NotSupportedException();

        /// <summary>Gets the count.</summary>
        /// <value>The count.</value>
        /// <exception cref="System.NotSupportedException"></exception>
        public override int Count => throw new NotSupportedException();

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
        public override Task<Stream> LoadFileDataAsync(string path, DataOption option = default, Action<FileMetadata, string> exception = default) => throw new NotSupportedException();
        /// <summary>
        /// Loads the file data asynchronous.
        /// </summary>
        /// <param name="fileId">The fileId.</param>
        /// <param name="option">The option.</param>
        /// <param name="exception">The exception.</param>
        /// <returns></returns>
        /// <exception cref="FileNotFoundException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public override Task<Stream> LoadFileDataAsync(int fileId, DataOption option = default, Action<FileMetadata, string> exception = default) => throw new NotSupportedException();

        /// <summary>
        /// Loads the file data asynchronous.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="option">The file.</param>
        /// <param name="exception">The exception.</param>
        /// <returns></returns>
        public override Task<Stream> LoadFileDataAsync(FileMetadata file, DataOption option = default, Action<FileMetadata, string> exception = default)
            => UseBinaryReader
            ? GetBinaryReader().Func(r => ReadFileDataAsync(r, file, option, exception))
            : ReadFileDataAsync(null, file, option, exception);

        /// <summary>
        /// Loads the object asynchronous.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path">The file path.</param>
        /// <param name="exception">The exception.</param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException"></exception>
        public override Task<T> LoadFileObjectAsync<T>(string path, Action<FileMetadata, string> exception = default) => throw new NotSupportedException();
        /// <summary>
        /// Loads the object asynchronous.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fileId">The fileId.</param>
        /// <param name="option">The file.</param>
        /// <param name="exception">The exception.</param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException"></exception>
        public override Task<T> LoadFileObjectAsync<T>(int fileId, Action<FileMetadata, string> exception = default) => throw new NotSupportedException();

        /// <summary>
        /// Ensures the file object factory.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <returns></returns>
        public Func<BinaryReader, FileMetadata, PakFile, Task<object>> EnsureCachedObjectFactory(FileMetadata file)
        {
            if (file.CachedObjectFactory != null) return file.CachedObjectFactory;

            var factory = GetObjectFactoryFactory(file, Game);
            file.CachedDataOption = factory.option;
            file.CachedObjectFactory = factory.factory ?? FileMetadata.EmptyObjectFactory;
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
        public override async Task<T> LoadFileObjectAsync<T>(FileMetadata file, Action<FileMetadata, string> exception = default)
        {
            var type = typeof(T);
            var stream = await LoadFileDataAsync(file, 0, exception);
            if (stream == null) return default;
            var objectFactory = EnsureCachedObjectFactory(file);
            if (objectFactory == FileMetadata.EmptyObjectFactory)
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
        /// Reads the file data asynchronous.
        /// </summary>
        /// <param name="r">The r.</param>
        /// <param name="file">The file.</param>
        /// <param name="option">The option.</param>
        /// <param name="exception">The exception.</param>
        /// <returns></returns>
        public virtual Task<Stream> ReadFileDataAsync(BinaryReader r, FileMetadata file, DataOption option = default, Action<FileMetadata, string> exception = default) => PakBinary.ReadDataAsync(this, r, file, option, exception);

        /// <summary>
        /// Writes the file data asynchronous.
        /// </summary>
        /// <param name="w">The w.</param>
        /// <param name="file">The file.</param>
        /// <param name="data">The data.</param>
        /// <param name="option">The option.</param>
        /// <param name="exception">The exception.</param>
        /// <returns></returns>
        public virtual Task WriteFileDataAsync(BinaryWriter w, FileMetadata file, Stream data, DataOption option = default, Action<FileMetadata, string> exception = default) => PakBinary.WriteDataAsync(this, w, file, data, option, exception);

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
        /// Processes this instance.
        /// </summary>
        public virtual void Process() => PakBinary?.Process(this);

        #region Metadata

        /// <summary>
        /// Gets the explorer item nodes.
        /// </summary>
        /// <param name="manager">The resource.</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public override async Task<List<MetadataItem>> GetMetadataItemsAsync(MetadataManager manager) => Valid && GetMetadataItems != null ? await GetMetadataItems(manager, this) : default;

        /// <summary>
        /// Gets the explorer information nodes.
        /// </summary>
        /// <param name="manager">The resource.</param>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public override async Task<List<MetadataInfo>> GetMetadataInfosAsync(MetadataManager manager, MetadataItem item)
        {
            if (!(item.Source is FileMetadata file)) return null;
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

        #endregion
    }
}