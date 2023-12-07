using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace GameSpec.Formats
{
    public class ManyPakFile : BinaryPakFile
    {
        /// <summary>
        /// The paths
        /// </summary>
        public readonly string[] Paths;

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiPakFile" /> class.
        /// </summary>
        /// <param name="game">The game.</param>
        /// <param name="name">The name.</param>
        /// <param name="fileSystem">The file system.</param>
        /// <param name="paths">The paths.</param>
        /// <param name="tag">The tag.</param>
        public ManyPakFile(PakFile basis, FamilyGame game, string name, IFileSystem fileSystem, string[] paths, object tag = default) : base(game, fileSystem, name, null, tag)
        {
            if (basis is BinaryPakFile b)
            {
                GetMetadataItems = b.GetMetadataItems;
                GetObjectFactoryFactory = b.GetObjectFactoryFactory;
            }
            Paths = paths;
            Reader = false;
        }

        /// <summary>
        /// Reads the asynchronous.
        /// </summary>
        /// <param name="r">The r.</param>
        /// <param name="tag">The tag.</param>
        /// <returns></returns>
        public override Task ReadAsync(BinaryReader r, object tag = default)
        {
            Files = Paths.Select(s => (s, p: Game.IsPakFile(s), i: FileSystem.GetFileInfo(s)))
                .Select(s => new FileSource
                {
                    Path = s.s.Replace('/', '\\'),
                    Pak = s.p ? (BinaryPakFile)Game.CreatePakFileType(FileSystem, s.s, null) : default,
                    FileSize = s.i.Length,
                }).ToArray();
            return Task.CompletedTask;
        }

        public override Task<Stream> ReadFileDataAsync(BinaryReader r, FileSource file, DataOption option = default, Action<FileSource, string> exception = default)
            => Task.FromResult(file.Pak == null ? (Stream)new MemoryStream(FileSystem.OpenReader(file.Path).ReadBytes((int)file.FileSize)) : default);
    }
}