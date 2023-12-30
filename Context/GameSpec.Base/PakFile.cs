using GameSpec.Formats;
using GameSpec.Metadata;
using GameSpec.Transforms;
using GameSpec.Unknown;
using OpenStack.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static OpenStack.Debug;

namespace GameSpec
{
    /// <summary>
    /// PakFile
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    public abstract class PakFile : IDisposable
    {
        /// <summary>
        /// An empty family.
        /// </summary>
        public static PakFile Empty = new UnknownPakFile(FamilyGame.Empty, "Empty");

        public enum PakStatus { Opening, Opened, Closing, Closed }

        /// <summary>
        /// Gets the status
        /// </summary>
        public volatile PakStatus Status = PakStatus.Closed;

        /// <summary>
        /// Gets the pak family.
        /// </summary>
        public readonly Family Family;

        /// <summary>
        /// Gets the pak family game.
        /// </summary>
        public readonly FamilyGame Game;

        /// <summary>
        /// Gets the pak name.
        /// </summary>
        public readonly string Name;

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
        /// <param name="game">The game.</param>
        /// <param name="name">The name.</param>
        /// <param name="tag">The tag.</param>
        public PakFile(FamilyGame game, string name, object tag = null)
        {
            Family = game.Family ?? throw new ArgumentNullException(nameof(game.Family));
            Game = game ?? throw new ArgumentNullException(nameof(game));
            Name = name;
            Tag = tag;
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
        public virtual PakFile Open(List<MetadataItem> items = null, MetadataManager manager = null)
        {
            if (Status != PakStatus.Closed) return this;
            Status = PakStatus.Opening;
            var watch = new Stopwatch();
            watch.Start();
            Opening();
            watch.Stop();
            Status = PakStatus.Opened;
            items?.AddRange(GetMetadataItemsAsync(manager).Result);
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
        public abstract bool Contains(string path);

        /// <summary>
        /// Determines whether this instance contains the item.
        /// </summary>
        /// <param name="fileId">The fileId.</param>
        /// <returns>
        ///   <c>true</c> if [contains] [the specified file path]; otherwise, <c>false</c>.
        /// </returns>
        public abstract bool Contains(int fileId);

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
        /// Loads the file data asynchronous.
        /// </summary>
        /// <param name="path">The file path.</param>
        /// <param name="option">The option.</param>
        /// <param name="exception">The exception.</param>
        /// <returns></returns>
        public abstract Task<Stream> LoadFileDataAsync(string path, DataOption option = 0, Action<FileSource, string> exception = null);
        /// <summary>
        /// Loads the file data asynchronous.
        /// </summary>
        /// <param name="fileId">The fileId.</param>
        /// <param name="option">The option.</param>
        /// <param name="exception">The exception.</param>
        /// <returns></returns>
        public abstract Task<Stream> LoadFileDataAsync(int fileId, DataOption option = 0, Action<FileSource, string> exception = null);
        /// <summary>
        /// Loads the file data asynchronous.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="option">The option.</param>
        /// <param name="exception">The exception.</param>
        /// <returns></returns>
        public abstract Task<Stream> LoadFileDataAsync(FileSource file, DataOption option = 0, Action<FileSource, string> exception = null);

        /// <summary>
        /// Loads the object asynchronous.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path">The file path.</param>
        /// <param name="exception">The exception.</param>
        /// <returns></returns>
        public abstract Task<T> LoadFileObjectAsync<T>(string path, Action<FileSource, string> exception = null);
        /// <summary>
        /// Loads the object asynchronous.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fileId">The fileId.</param>
        /// <param name="exception">The exception.</param>
        /// <returns></returns>
        public abstract Task<T> LoadFileObjectAsync<T>(int fileId, Action<FileSource, string> exception = null);
        /// <summary>
        /// Loads the object asynchronous.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="file">The file.</param>
        /// <param name="exception">The exception.</param>
        /// <returns></returns>
        public abstract Task<T> LoadFileObjectAsync<T>(FileSource file, Action<FileSource, string> exception = null);

        /// <summary>
        /// Loads the object transformed asynchronous.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path">The file path.</param>
        /// <param name="transformTo">The transformTo.</param>
        /// <param name="exception">The exception.</param>
        /// <returns></returns>
        public async Task<T> LoadFileObjectAsync<T>(string path, PakFile transformTo, Action<FileSource, string> exception = null)
            => await TransformFileObjectAsync<T>(transformTo, await LoadFileObjectAsync<object>(path, exception));
        /// <summary>
        /// Loads the object transformed asynchronous.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fileId">The fileId.</param>
        /// <param name="transformTo">The transformTo.</param>
        /// <param name="exception">The exception.</param>
        /// <returns></returns>
        public async Task<T> LoadFileObjectAsync<T>(int fileId, PakFile transformTo, Action<FileSource, string> exception = null)
            => await TransformFileObjectAsync<T>(transformTo, await LoadFileObjectAsync<object>(fileId, exception));
        /// <summary>
        /// Loads the object transformed asynchronous.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="file">The file.</param>
        /// <param name="transformTo">The transformTo.</param>
        /// <param name="exception">The exception.</param>
        /// <returns></returns>
        public async Task<T> LoadFileObjectAsync<T>(FileSource fileId, PakFile transformTo, Action<FileSource, string> exception = null)
            => await TransformFileObjectAsync<T>(transformTo, await LoadFileObjectAsync<object>(fileId, exception));

        /// <summary>
        /// Transforms the file object asynchronous.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="transformTo">The transformTo.</param>
        /// <param name="source">The source.</param>
        /// <returns></returns>
        Task<T> TransformFileObjectAsync<T>(PakFile transformTo, object source)
        {
            if (this is ITransformFileObject<T> left && left.CanTransformFileObject(transformTo, source)) return left.TransformFileObjectAsync(transformTo, source);
            else if (transformTo is ITransformFileObject<T> right && right.CanTransformFileObject(transformTo, source)) return right.TransformFileObjectAsync(transformTo, source);
            else throw new ArgumentOutOfRangeException(nameof(transformTo));
        }

        /// <summary>
        /// Gets the graphic.
        /// </summary>
        /// <value>
        /// The graphic.
        /// </value>
        public IOpenGraphic Graphic { get; internal set; }

        #region Metadata

        /// <summary>
        /// Gets the metadata item filters.
        /// </summary>
        /// <param name="manager">The resource.</param>
        /// <returns></returns>
        public virtual Task<List<MetadataItem.Filter>> GetMetadataFiltersAsync(MetadataManager manager)
            => Task.FromResult(Family.FileManager != null && Family.FileManager.Filters.TryGetValue(Game.Id, out var z) ? z.Select(x => new MetadataItem.Filter(x.Key, x.Value)).ToList() : null);

        /// <summary>
        /// Gets the metadata infos.
        /// </summary>
        /// <param name="manager">The resource.</param>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        public virtual Task<List<MetadataInfo>> GetMetadataInfosAsync(MetadataManager manager, MetadataItem item)
            => throw new NotImplementedException();

        /// <summary>
        /// Gets the metadata items.
        /// </summary>
        /// <param name="manager">The resource.</param>
        /// <returns></returns>
        public virtual Task<List<MetadataItem>> GetMetadataItemsAsync(MetadataManager manager)
            => throw new NotImplementedException();

        #endregion
    }
}