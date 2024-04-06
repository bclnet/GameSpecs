using GameX.Formats;
using GameX.Meta;
using GameX.Unknown;
using OpenStack.Graphics;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static OpenStack.Debug;

namespace GameX
{
    #region FileOption

    [Flags]
    public enum FileOption
    {
        Hosting = Raw | Marker,
        None = 0x0,
        Raw = 0x1,
        Marker = 0x2,
        Stream = 0x4,
        Model = 0x8,
        Supress = 0x10,
    }

    #endregion

    #region ITransformFileObject

    /// <summary>
    /// ITransformFileObject
    /// </summary>
    public interface ITransformFileObject<T>
    {
        /// <summary>
        /// Determines whether this instance [can transform file object] the specified transform to.
        /// </summary>
        /// <param name="transformTo">The transform to.</param>
        /// <param name="source">The source.</param>
        /// <returns>
        ///   <c>true</c> if this instance [can transform file object] the specified transform to; otherwise, <c>false</c>.
        /// </returns>
        bool CanTransformFileObject(PakFile transformTo, object source);
        /// <summary>
        /// Transforms the file object asynchronous.
        /// </summary>
        /// <param name="transformTo">The transform to.</param>
        /// <param name="source">The source.</param>
        /// <returns></returns>
        Task<T> TransformFileObject(PakFile transformTo, object source);
    }

    #endregion

    #region PakState

    /// <summary>
    /// PakState
    /// </summary>
    public class PakState
    {
        /// <summary>
        /// Gets the filesystem.
        /// </summary>
        public readonly IFileSystem FileSystem;

        /// <summary>
        /// Gets the pak family game.
        /// </summary>
        public readonly FamilyGame Game;

        /// <summary>
        /// Gets the filesystem.
        /// </summary>
        public readonly FamilyGame.Edition Edition;

        /// <summary>
        /// Gets the path.
        /// </summary>
        public readonly string Path;

        /// <summary>
        /// Gets the tag.
        /// </summary>
        public object Tag;

        /// <param name="fileSystem">The file system.</param>
        /// <param name="game">The game.</param>
        /// <param name="edition">The edition.</param>
        /// <param name="path">The path.</param>
        /// <param name="tag">The tag.</param>
        public PakState(IFileSystem fileSystem, FamilyGame game, FamilyGame.Edition edition = null, string path = null, object tag = null)
        {
            FileSystem = fileSystem;
            Game = game;
            Edition = edition;
            Path = path ?? string.Empty;
            Tag = tag;
        }
    }

    #endregion

    #region PakFile

    /// <summary>
    /// PakFile
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    public abstract class PakFile : IDisposable
    {
        public delegate (FileOption option, Func<BinaryReader, FileSource, PakFile, Task<object>> factory) FuncObjectFactoryFactory(FileSource source, FamilyGame game);

        /// <summary>
        /// An empty family.
        /// </summary>
        public static PakFile Empty = new UnknownPakFile(new PakState(new StandardFileSystem(""), FamilyGame.Empty)) { Name = "Empty" };

        public enum PakStatus { Opening, Opened, Closing, Closed }

        /// <summary>
        /// Gets the status
        /// </summary>
        public volatile PakStatus Status = PakStatus.Closed;

        /// <summary>
        /// Gets the filesystem.
        /// </summary>
        public readonly IFileSystem FileSystem;

        /// <summary>
        /// Gets the pak family.
        /// </summary>
        public readonly Family Family;

        /// <summary>
        /// Gets the pak family game.
        /// </summary>
        public readonly FamilyGame Game;

        /// <summary>
        /// Gets the filesystem.
        /// </summary>
        public readonly FamilyGame.Edition Edition;

        /// <summary>
        /// Gets the pak path.
        /// </summary>
        public string PakPath;

        /// <summary>
        /// Gets the pak name.
        /// </summary>
        public string Name;

        /// <summary>
        /// Gets the tag.
        /// </summary>
        public object Tag;

        /// <summary>
        /// Gets the pak path finders.
        /// </summary>
        public readonly IDictionary<Type, Func<string, string>> PathFinders = new Dictionary<Type, Func<string, string>>();

        /// <summary>
        /// Initializes a new instance of the <see cref="PakFile" /> class.
        /// </summary>
        /// <param name="state">The state.</param>
        /// <param name="name">The name.</param>
        public PakFile(PakState state)
        {
            string z;
            FileSystem = state.FileSystem ?? throw new ArgumentNullException(nameof(state.FileSystem));
            Family = state.Game.Family ?? throw new ArgumentNullException(nameof(state.Game.Family));
            Game = state.Game ?? throw new ArgumentNullException(nameof(state.Game));
            Edition = state.Edition;
            PakPath = state.Path;
            Name = !string.IsNullOrEmpty(z = Path.GetFileName(state.Path)) ? z : Path.GetFileName(Path.GetDirectoryName(state.Path));
            Tag = state.Tag;
            Graphic = null;
        }

        /// <summary>
        /// Determines whether this instance is valid.
        /// </summary>
        public virtual bool Valid => true;

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Close();
            GC.SuppressFinalize(this);
        }
        ~PakFile() => Close();

        /// <summary>
        /// Closes this instance.
        /// </summary>
        public PakFile Close()
        {
            Status = PakStatus.Closing;
            Closing();
            if (Tag is IDisposable disposableTag) disposableTag.Dispose();
            Status = PakStatus.Closed;
            return this;
        }

        /// <summary>
        /// Closes this instance.
        /// </summary>
        public abstract void Closing();

        /// <summary>
        /// Opens this instance.
        /// </summary>
        public virtual PakFile Open(List<MetaItem> items = null, MetaManager manager = null)
        {
            if (Status != PakStatus.Closed) return this;
            Status = PakStatus.Opening;
            var watch = new Stopwatch();
            watch.Start();
            Opening();
            watch.Stop();
            Status = PakStatus.Opened;
            items?.AddRange(GetMetaItems(manager));
            Log($"Opened: {Name} @ {watch.ElapsedMilliseconds}ms");
            return this;
        }

        /// <summary>
        /// Opens this instance.
        /// </summary>
        public abstract void Opening();

        /// <summary>
        /// Determines whether this instance contains the item.
        /// </summary>
        /// <param name="path">The file path.</param>
        /// <returns>
        ///   <c>true</c> if [contains] [the specified file path]; otherwise, <c>false</c>.
        /// </returns>
        public abstract bool Contains(object path);

        /// <summary>
        /// Gets the pak item count.
        /// </summary>
        /// <value>
        /// The count.
        /// </value>
        public abstract int Count { get; }

        /// <summary>
        /// Finds the path.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        public string FindPath<T>(string path)
        {
            if (PathFinders.Count != 1) return PathFinders.TryGetValue(typeof(T), out var pathFinder) ? pathFinder(path) : path;
            var first = PathFinders.First();
            return first.Key == typeof(T) || first.Key == typeof(object) ? first.Value(path) : path;
        }

        /// <summary>
        /// Gets the graphic.
        /// </summary>
        /// <value>
        /// The graphic.
        /// </value>
        public IOpenGraphic Graphic { get; internal set; }

        /// <summary>
        /// Gets the file source.
        /// </summary>
        /// <param name="path">The file path.</param>
        /// <param name="throwOnError">Throws on error.</param>
        /// <returns></returns>
        /// <exception cref="FileNotFoundException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public abstract (PakFile, FileSource) GetFileSource(object path, bool throwOnError = true);

        /// <summary>
        /// Loads the file data asynchronous.
        /// </summary>
        /// <param name="path">The file path.</param>
        /// <param name="option">The option.</param>
        /// <param name="throwOnError">Throws on error.</param>
        /// <returns></returns>
        public abstract Task<Stream> LoadFileData(object path, FileOption option = default, bool throwOnError = true);

        /// <summary>
        /// Loads the object asynchronous.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path">The file path.</param>
        /// <param name="option">The option.</param>
        /// <param name="throwOnError">Throws on error.</param>
        /// <returns></returns>
        public abstract Task<T> LoadFileObject<T>(object path, FileOption option = default, bool throwOnError = true);

        /// Opens the family pak file.
        /// </summary>
        /// <param name="res">The res.</param>
        /// <param name="throwOnError">Throws on error.</param>
        /// <returns></returns>
        public PakFile OpenPakFile(object res, bool throwOnError = true)
            => res switch
            {
                string s => Game.CreatePakFile(FileSystem, Edition, s, throwOnError)?.Open(),
                _ => throw new ArgumentOutOfRangeException(nameof(res)),
            };

        #region Transform

        /// <summary>
        /// Loads the object transformed asynchronous.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path">The file path.</param>
        /// <param name="transformTo">The transformTo.</param>
        /// <returns></returns>
        public async Task<T> LoadFileObject<T>(object path, PakFile transformTo)
            => await TransformFileObject<T>(transformTo, await LoadFileObject<object>(path));

        /// <summary>
        /// Transforms the file object asynchronous.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="transformTo">The transformTo.</param>
        /// <param name="source">The source.</param>
        /// <returns></returns>
        Task<T> TransformFileObject<T>(PakFile transformTo, object source)
        {
            if (this is ITransformFileObject<T> left && left.CanTransformFileObject(transformTo, source)) return left.TransformFileObject(transformTo, source);
            else if (transformTo is ITransformFileObject<T> right && right.CanTransformFileObject(transformTo, source)) return right.TransformFileObject(transformTo, source);
            else throw new ArgumentOutOfRangeException(nameof(transformTo));
        }

        #endregion

        #region Metadata

        /// <summary>
        /// Gets the metadata item filters.
        /// </summary>
        /// <param name="manager">The resource.</param>
        /// <returns></returns>
        public virtual List<MetaItem.Filter> GetMetadataFilters(MetaManager manager)
            => Family.FileManager != null && Family.FileManager.Filters.TryGetValue(Game.Id, out var z) ? z.Select(x => new MetaItem.Filter(x.Key, x.Value)).ToList() : null;

        /// <summary>
        /// Gets the metadata infos.
        /// </summary>
        /// <param name="manager">The resource.</param>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        public virtual Task<List<MetaInfo>> GetMetaInfos(MetaManager manager, MetaItem item)
            => throw new NotImplementedException();

        /// <summary>
        /// Gets the metadata items.
        /// </summary>
        /// <param name="manager">The resource.</param>
        /// <returns></returns>
        public virtual List<MetaItem> GetMetaItems(MetaManager manager)
            => throw new NotImplementedException();

        #endregion
    }

    #endregion

    #region BinaryPakFile

    [DebuggerDisplay("{Name}")]
    public abstract class BinaryPakFile : PakFile
    {
        public readonly PakBinary PakBinary;
        readonly ConcurrentDictionary<string, GenericPool<BinaryReader>> Readers = new ConcurrentDictionary<string, GenericPool<BinaryReader>>();
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
        //internal protected Func<FileSource, FamilyGame, (FileOption option, Func<BinaryReader, FileSource, PakFile, Task<object>> factory)> ObjectFactoryFactoryMethod;
        public FuncObjectFactoryFactory ObjectFactoryFactoryMethod;

        // binary
        public IList<FileSource> Files;
        public HashSet<string> FilesRawSet;
        public ILookup<int, FileSource> FilesById { get; private set; }
        public ILookup<string, FileSource> FilesByPath { get; private set; }
        public int PathSkip;

        /// <summary>
        /// Initializes a new instance of the <see cref="BinaryPakFile" /> class.
        /// </summary>
        /// <param name="state">The state.</param>
        /// <param name="name">The name.</param>
        /// <param name="pakBinary">The pak binary.</param>
        /// <exception cref="ArgumentNullException">pakBinary</exception>
        public BinaryPakFile(PakState state, PakBinary pakBinary) : base(state)
            => PakBinary = pakBinary;

        /// <summary>
        /// Valid
        /// </summary>
        public override bool Valid => Files != null;

        /// <summary>
        /// Gets the binary reader.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        public GenericPool<BinaryReader> GetReader(string path = default, int retainInPool = 10)
            => Readers.GetOrAdd(path ?? PakPath, path => FileSystem.FileExists(path) ? new GenericPool<BinaryReader>(() => FileSystem.OpenReader(path), retainInPool) : default);

        //protected void DoRead(Func<BinaryReader, object, Task> func)
        //{
        //    if (UseReader) GetReader()?.Action(async r => await func(r, null));
        //    else func(null, null).GetAwaiter().GetResult();
        //}

        /// <summary>
        /// Opens this instance.
        /// </summary>
        public override void Opening()
        {
            //DoRead(Read);
            if (UseReader) GetReader()?.Action(async r => await Read(r));
            else Read(null).GetAwaiter().GetResult();
            Process();
        }

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
        {
            switch (path)
            {
                case null: throw new ArgumentNullException(nameof(path));
                case string s:
                    {
                        var (pak, s2) = FindPath(s);
                        return pak != null
                        ? pak.Contains(s2)
                        : FilesByPath != null && FilesByPath.Contains(s.Replace('\\', '/'));
                    }
                case int i: return FilesById != null && FilesById.Contains(i);
                default: throw new ArgumentOutOfRangeException(nameof(path));
            }
        }

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
        /// Gets the file source.
        /// </summary>
        /// <param name="path">The file path.</param>
        /// <param name="throwOnError">Throws on error.</param>
        /// <returns></returns>
        /// <exception cref="FileNotFoundException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public override (PakFile, FileSource) GetFileSource(object path, bool throwOnError = true)
        {
            switch (path)
            {
                case null: throw new ArgumentNullException(nameof(path));
                case FileSource f: return (this, f);
                case string s:
                    {
                        var (pak, s2) = FindPath(s);
                        if (pak != null) return pak.GetFileSource(s2);
                        var files = FilesByPath[s.Replace('\\', '/')].ToArray();
                        if (files.Length == 1) return (this, files[0]);
                        Log($"ERROR.LoadFileData: {s} @ {files.Length}");
                        if (throwOnError) throw new FileNotFoundException(files.Length == 0 ? s : $"More then one file found for {s}");
                        return (null, null);
                    }
                case int i:
                    {
                        var files = FilesById[i].ToArray();
                        if (files.Length == 1) return (this, files[0]);
                        Log($"ERROR.LoadFileData: {i} @ {files.Length}");
                        if (throwOnError) throw new FileNotFoundException(files.Length == 0 ? $"{i}" : $"More then one file found for {i}");
                        return (null, null);
                    }
                default: throw new ArgumentOutOfRangeException(nameof(path));
            }
        }

        /// <summary>
        /// Loads the file data asynchronous.
        /// </summary>
        /// <param name="path">The file path.</param>
        /// <param name="option">The option.</param>
        /// <param name="throwOnError">Throws on error.</param>
        /// <returns></returns>
        /// <exception cref="FileNotFoundException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public override Task<Stream> LoadFileData(object path, FileOption option = default, bool throwOnError = true)
        {
            if (path == null) return default;
            else if (!(path is FileSource))
            {
                var (p, f2) = GetFileSource(path, throwOnError);
                return p?.LoadFileData(f2, option, throwOnError);
            }
            var f = (FileSource)path;
            return UseReader
                ? GetReader().Func(r => ReadData(r, f, option))
                : ReadData(null, f, option);
        }

        /// <summary>
        /// Loads the file object asynchronous.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path">The file.</param>
        /// <param name="option">The option.</param>
        /// <param name="throwOnError">Throws on error.</param>
        /// <returns></returns>
        public override async Task<T> LoadFileObject<T>(object path, FileOption option = default, bool throwOnError = true)
        {
            if (path == null) return default;
            else if (!(path is FileSource))
            {
                var (p, f2) = GetFileSource(path, throwOnError);
                return await p.LoadFileObject<T>(f2, option, throwOnError);
            }
            var f = (FileSource)path;
            var type = typeof(T);
            var data = await LoadFileData(f, option, throwOnError);
            if (data == null) return default;
            var objectFactory = EnsureCachedObjectFactory(f);
            if (objectFactory != FileSource.EmptyObjectFactory)
            {
                var r = new BinaryReader(data);
                object value = null;
                Task<object> task = null;
                try
                {
                    task = objectFactory(r, f, this);
                    if (task != null)
                    {
                        value = await task;
                        return value is T z ? z
                            : value is IRedirected<T> y ? y.Value
                            : throw new InvalidCastException();
                    }
                }
                catch (Exception e) { Log(e.Message); throw e; }
                finally { if (task != null && !(value != null && value is IDisposable)) r.Dispose(); }
            }
            return type == typeof(Stream) || type == typeof(object)
                ? (T)(object)data
                : throw new ArgumentOutOfRangeException(nameof(T), $"Stream not returned for {f.Path} with {type.Name}");
        }

        /// <summary>
        /// Ensures the file object factory.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <returns></returns>
        public Func<BinaryReader, FileSource, PakFile, Task<object>> EnsureCachedObjectFactory(FileSource file)
        {
            if (file.CachedObjectFactory != null) return file.CachedObjectFactory;
            var factory = ObjectFactoryFactoryMethod(file, Game);
            file.CachedObjectOption = factory.option;
            file.CachedObjectFactory = factory.factory ?? FileSource.EmptyObjectFactory;
            return file.CachedObjectFactory;
        }

        /// <summary>
        /// Processes this instance.
        /// </summary>
        public virtual void Process()
        {
            if (UseFileId) FilesById = Files.Where(x => x != null).ToLookup(x => x.Id);
            FilesByPath = Files.Where(x => x != null).ToLookup(x => x.Path, StringComparer.OrdinalIgnoreCase);
            PakBinary?.Process(this);
        }

        (PakFile pak, string next) FindPath(string path)
        {
            var paths = path.Split(new[] { ':' }, 2);
            var p = paths[0].Replace('\\', '/');
            var pak = FilesByPath[p]?.FirstOrDefault()?.Pak?.Open();
            return (pak, pak != null && paths.Length > 1 ? paths[1] : null);
        }

        /// <summary>
        /// Adds the raw file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="message">The message.</param>
        //public void AddRawFile(FileSource file, string message)
        //{
        //    if (file == null) throw new ArgumentNullException(nameof(file));
        //    lock (this)
        //    {
        //        FilesRawSet ??= new HashSet<string>();
        //        FilesRawSet.Add(file.Path);
        //    }
        //}

        #region PakBinary

        /// <summary>
        /// Reads the asynchronous.
        /// </summary>
        /// <param name="r">The r.</param>
        /// <param name="tag">The tag.</param>
        /// <returns></returns>
        public virtual Task Read(BinaryReader r, object tag = default) => PakBinary.Read(this, r, tag);

        /// <summary>
        /// Reads the file data asynchronous.
        /// </summary>
        /// <param name="r">The r.</param>
        /// <param name="file">The file.</param>
        /// <param name="option">The option.</param>
        /// <returns></returns>
        public virtual Task<Stream> ReadData(BinaryReader r, FileSource file, FileOption option = default) => PakBinary.ReadData(this, r, file, option);

        /// <summary>
        /// Writes the asynchronous.
        /// </summary>
        /// <param name="w">The w.</param>
        /// <param name="tag">The tag.</param>
        /// <returns></returns>
        public virtual Task Write(BinaryWriter w, object tag = default) => PakBinary.Write(this, w, tag);

        /// <summary>
        /// Writes the file data asynchronous.
        /// </summary>
        /// <param name="w">The w.</param>
        /// <param name="file">The file.</param>
        /// <param name="data">The data.</param>
        /// <param name="option">The option.</param>
        /// <returns></returns>
        public virtual Task WriteData(BinaryWriter w, FileSource file, Stream data, FileOption option = default) => PakBinary.WriteData(this, w, file, data, option);

        #endregion

        #region Metadata

        /// <summary>
        /// Gets the explorer information nodes.
        /// </summary>
        /// <param name="manager">The manager.</param>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public override Task<List<MetaInfo>> GetMetaInfos(MetaManager manager, MetaItem item)
            => Valid ? MetaManager.GetMetaInfos(manager, this, item.Source as FileSource) : default;

        /// <summary>
        /// Gets the explorer item nodes.
        /// </summary>
        /// <param name="manager">The resource.</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public override List<MetaItem> GetMetaItems(MetaManager manager)
            => Valid ? MetaManager.GetMetaItems(manager, this) : default;

        #endregion
    }

    #endregion

    #region ManyPakFile

    public class ManyPakFile : BinaryPakFile
    {
        /// <summary>
        /// The paths
        /// </summary>
        public readonly string[] Paths;

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiPakFile" /> class.
        /// </summary>
        /// <param name="basis">The basis.</param>
        /// <param name="state">The state.</param>
        /// <param name="name">The name.</param>
        /// <param name="paths">The paths.</param>
        /// <param name="pathSkip">The pathSkip.</param>
        public ManyPakFile(PakFile basis, PakState state, string name, string[] paths, int pathSkip = 0) : base(state, null)
        {
            if (basis is BinaryPakFile b)
                ObjectFactoryFactoryMethod = b.ObjectFactoryFactoryMethod;
            Name = name;
            Paths = paths;
            PathSkip = pathSkip;
            UseReader = false;
        }

        #region PakBinary

        /// <summary>
        /// Reads the asynchronous.
        /// </summary>
        /// <param name="r">The r.</param>
        /// <param name="tag">The tag.</param>
        /// <returns></returns>
        public override Task Read(BinaryReader r, object tag = default)
        {
            Files = Paths.Select(s => new FileSource
            {
                Path = s.Replace('\\', '/'),
                Pak = Game.IsPakFile(s) ? (BinaryPakFile)Game.CreatePakFileType(new PakState(FileSystem, Game, Edition, s)) : default,
                FileSize = FileSystem.FileInfo(s).length,
            }).ToArray();
            return Task.CompletedTask;
        }

        public override Task<Stream> ReadData(BinaryReader r, FileSource file, FileOption option = default)
            => file.Pak != null
                ? file.Pak.ReadData(r, file, option)
                : Task.FromResult<Stream>(new MemoryStream(FileSystem.OpenReader(file.Path).ReadBytes((int)file.FileSize)));

        #endregion
    }

    #endregion

    #region MultiPakFile

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
        /// <param name="state">The state.</param>
        /// <param name="name">The name.</param>
        /// <param name="pakFiles">The packs.</param>
        /// <param name="tag">The tag.</param>
        public MultiPakFile(PakState state, string name, IList<PakFile> pakFiles) : base(state)
        {
            Name = name;
            PakFiles = pakFiles ?? throw new ArgumentNullException(nameof(pakFiles));
        }

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
                string s => FindPakFiles(s, out var next).Any(x => x.Valid && x.Contains(next)),
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

        IList<PakFile> FindPakFiles(string path, out string next)
        {
            var paths = path.Split(new[] { '\\', '/', ':' }, 2);
            if (paths.Length == 1) { next = path; return PakFiles; }
            path = paths[0]; next = paths[1];
            var pakFiles = PakFiles.Where(x => x.Name.StartsWith(path)).ToList();
            foreach (var pakFile in pakFiles) pakFile.Open();
            return pakFiles;
        }

        /// <summary>
        /// Gets the file source.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="throwOnError">Throws on error.</param>
        /// <returns></returns>
        /// <exception cref="System.IO.FileNotFoundException">Could not find file \"{path}\".</exception>
        public override (PakFile, FileSource) GetFileSource(object path, bool throwOnError = true)
            => path switch
            {
                null => throw new ArgumentNullException(nameof(path)),
                string s => (FindPakFiles(s, out var s2).FirstOrDefault(x => x.Valid && x.Contains(s2)) ?? throw new FileNotFoundException($"Could not find file \"{s}\"."))
                    .GetFileSource(s2, throwOnError),
                int i => (PakFiles.FirstOrDefault(x => x.Valid && x.Contains(i)) ?? throw new FileNotFoundException($"Could not find file \"{i}\"."))
                    .GetFileSource(i, throwOnError),
                _ => throw new ArgumentOutOfRangeException(nameof(path)),
            };

        /// <summary>
        /// Loads the file data asynchronous.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="option">The option.</param>
        /// <param name="throwOnError">Throws on error.</param>
        /// <returns></returns>
        /// <exception cref="System.IO.FileNotFoundException">Could not find file \"{path}\".</exception>
        public override Task<Stream> LoadFileData(object path, FileOption option = default, bool throwOnError = true)
            => path switch
            {
                null => throw new ArgumentNullException(nameof(path)),
                string s => (FindPakFiles(s, out var s2).FirstOrDefault(x => x.Valid && x.Contains(s2)) ?? throw new FileNotFoundException($"Could not find file \"{s}\"."))
                    .LoadFileData(s2, option, throwOnError),
                int i => (PakFiles.FirstOrDefault(x => x.Valid && x.Contains(i)) ?? throw new FileNotFoundException($"Could not find file \"{i}\"."))
                    .LoadFileData(i, option, throwOnError),
                _ => throw new ArgumentOutOfRangeException(nameof(path)),
            };

        /// <summary>
        /// Loads the object asynchronous.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="option">The option.</param>
        /// <param name="throwOnError">Throws on error.</param>
        /// <returns></returns>
        /// <exception cref="System.IO.FileNotFoundException">Could not find file \"{path}\".</exception>
        public override Task<T> LoadFileObject<T>(object path, FileOption option = default, bool throwOnError = true)
            => path switch
            {
                null => throw new ArgumentNullException(nameof(path)),
                string s => (FindPakFiles(s, out var s2).FirstOrDefault(x => x.Valid && x.Contains(s2)) ?? throw new FileNotFoundException($"Could not find file \"{s}\"."))
                    .LoadFileObject<T>(s2, option, throwOnError),
                int i => (PakFiles.FirstOrDefault(x => x.Valid && x.Contains(i)) ?? throw new FileNotFoundException($"Could not find file \"{i}\"."))
                    .LoadFileObject<T>(i, option, throwOnError),
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

    #endregion

    #region PakBinary

    public class PakBinary
    {
        /// <summary>
        /// The file
        /// </summary>
        public readonly static PakBinary Stream = new PakBinaryCanStream();

        /// <summary>
        /// Reads the asynchronous.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="r">The r.</param>
        /// <param name="tag">The tag.</param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException"></exception>
        public virtual Task Read(BinaryPakFile source, BinaryReader r, object tag = default) => throw new NotSupportedException();

        /// <summary>
        /// Reads the asynchronous.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="r">The r.</param>
        /// <param name="file">The file.</param>
        /// <param name="option">The option.</param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException"></exception>
        public virtual Task<Stream> ReadData(BinaryPakFile source, BinaryReader r, FileSource file, FileOption option = default) => throw new NotSupportedException();

        /// <summary>
        /// Writes the asynchronous.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="w">The w.</param>
        /// <param name="tag">The tag.</param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException"></exception>
        public virtual Task Write(BinaryPakFile source, BinaryWriter w, object tag = default) => throw new NotSupportedException();

        /// <summary>
        /// Writes the asynchronous.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="w">The w.</param>
        /// <param name="file">The file.</param>
        /// <param name="option">The option.</param>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException"></exception>
        public virtual Task WriteData(BinaryPakFile source, BinaryWriter w, FileSource file, Stream data, FileOption option = default) => throw new NotSupportedException();

        /// <summary>
        /// Processes this instance.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <exception cref="NotSupportedException"></exception>
        public virtual void Process(BinaryPakFile source) { }

        /// <summary>
        /// handles an exception.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="option">The option.</param>
        /// <param name="message">The message.</param>
        /// <exception cref="NotSupportedException"></exception>
        public static void HandleException(object source, FileOption option, string message)
        {
            Log(message);
            if ((option & FileOption.Supress) != 0) throw new Exception(message);
        }
    }

    #endregion

    #region PakBinaryT

    public class PakBinary<Self> : PakBinary where Self : PakBinary, new()
    {
        public static readonly PakBinary Instance = new Self();

        protected class SubPakFile : BinaryPakFile
        {
            FileSource File;
            BinaryPakFile Source;

            public SubPakFile(BinaryPakFile source, FileSource file, string path, object tag = null, PakBinary instance = null) : base(new PakState(source.FileSystem, source.Game, source.Edition, path, tag), instance ?? Instance)
            {
                File = file;
                Source = source;
                ObjectFactoryFactoryMethod = source.ObjectFactoryFactoryMethod;
                UseReader = file == null;
                //Open();
            }

            public async override Task Read(BinaryReader r, object tag = null)
            {
                if (UseReader) { await base.Read(r, tag); return; }
                using var r2 = await Source.GetReader().Func(async r => new BinaryReader(await ReadData(r, File)));
                if (r2 == null) throw new NotImplementedException();
                await PakBinary.Read(this, r2, tag);
            }
        }
    }

    #endregion
}