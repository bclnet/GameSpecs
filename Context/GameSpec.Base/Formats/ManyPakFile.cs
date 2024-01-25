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
        /// <param name="basis">The basis.</param>
        /// <param name="fileSystem">The file system.</param>
        /// <param name="game">The game.</param>
        /// <param name="edition">The edition.</param>
        /// <param name="name">The name.</param>
        /// <param name="paths">The paths.</param>
        /// <param name="tag">The tag.</param>
        public ManyPakFile(PakFile basis, IFileSystem fileSystem, FamilyGame game, FamilyGame.Edition edition, string name, string[] paths, object tag = default)
            : base(fileSystem, game, edition, name, null, tag)
        {
            if (basis is BinaryPakFile b)
            {
                ObjectFactoryFactoryMethod = b.ObjectFactoryFactoryMethod;
            }
            Paths = paths;
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
                Path = s.Replace('/', '\\'),
                Pak = Game.IsPakFile(s) ? (BinaryPakFile)Game.CreatePakFileType(FileSystem, Edition, s) : default,
                FileSize = FileSystem.FileInfo(s).Length,
            }).ToArray();
            return Task.CompletedTask;
        }

        public override Task<Stream> ReadData(BinaryReader r, FileSource file, FileOption option = default)
            => Task.FromResult(file.Pak == null
                ? (Stream)new MemoryStream(FileSystem.OpenReader(file.Path).ReadBytes((int)file.FileSize))
                : default);

        #endregion
    }
}