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
        /// <param name="game">The game.</param>
        /// <param name="name">The name.</param>
        public UnknownPakFile(FamilyGame game, string name) : base(game, name) { }
        public UnknownPakFile(FamilyGame game, IFileSystem fileSystem, string filePath, object tag = default) : base(game, "Unknown") { }
        //public override void Dispose() { }

        public override int Count => 0;
        public override void Closing() { }
        public override void Opening() { }
        public override bool Contains(string path) => false;
        public override bool Contains(int fileId) => false;
        public override Task<Stream> LoadFileDataAsync(string path, DataOption option = 0, Action<FileSource, string> exception = null) => throw new NotImplementedException();
        public override Task<Stream> LoadFileDataAsync(int fileId, DataOption option = 0, Action<FileSource, string> exception = null) => throw new NotImplementedException();
        public override Task<Stream> LoadFileDataAsync(FileSource file, DataOption option = 0, Action<FileSource, string> exception = null) => throw new NotImplementedException();
        public override Task<T> LoadFileObjectAsync<T>(string path, Action<FileSource, string> exception = null) => throw new NotImplementedException();
        public override Task<T> LoadFileObjectAsync<T>(int fileId, Action<FileSource, string> exception = null) => throw new NotImplementedException();
        public override Task<T> LoadFileObjectAsync<T>(FileSource file, Action<FileSource, string> exception = null) => throw new NotImplementedException();
    }
}