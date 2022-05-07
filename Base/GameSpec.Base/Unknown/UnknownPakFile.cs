using GameSpec.Formats;
using System;
using System.IO;
using System.Threading.Tasks;

namespace GameSpec.Unknown
{
    /// <summary>
    /// UnknownPakFile
    /// </summary>
    /// <seealso cref="GameSpec.Formats.PakFile" />
    public class UnknownPakFile : PakFile
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UnknownPakFile" /> class.
        /// </summary>
        /// <param name="family">The family.</param>
        /// <param name="game">The game.</param>
        /// <param name="name">The name.</param>
        public UnknownPakFile(Family family, string game, string name)
            : base(family, game, name) { }
        public override void Dispose() { }

        public override int Count => 0;
        public override void Close() { }
        public override bool Contains(string path) => false;
        public override bool Contains(int fileId) => false;
        public override Task<Stream> LoadFileDataAsync(string path, DataOption option = 0, Action<FileMetadata, string> exception = null) => throw new NotImplementedException();
        public override Task<Stream> LoadFileDataAsync(int fileId, DataOption option = 0, Action<FileMetadata, string> exception = null) => throw new NotImplementedException();
        public override Task<Stream> LoadFileDataAsync(FileMetadata file, DataOption option = 0, Action<FileMetadata, string> exception = null) => throw new NotImplementedException();
        public override Task<T> LoadFileObjectAsync<T>(string path, Action<FileMetadata, string> exception = null) => throw new NotImplementedException();
        public override Task<T> LoadFileObjectAsync<T>(int fileId, Action<FileMetadata, string> exception = null) => throw new NotImplementedException();
        public override Task<T> LoadFileObjectAsync<T>(FileMetadata file, Action<FileMetadata, string> exception = null) => throw new NotImplementedException();
    }
}