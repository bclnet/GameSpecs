using GameSpec.Crytek.Formats;
using GameSpec.Crytek.Transforms;
using GameSpec.Formats;
using GameSpec.Formats.Unknown;
using GameSpec.Metadata;
using GameSpec.Transforms;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace GameSpec.Crytek
{
    /// <summary>
    /// CryPakFile
    /// </summary>
    /// <seealso cref="GameSpec.Formats.BinaryPakFile" />
    public class CryPakFile : BinaryPakFile, ITransformFileObject<IUnknownFileModel>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CryPakFile" /> class.
        /// </summary>
        /// <param name="game">The game.</param>
        /// <param name="fileSystem">The file system.</param>
        /// <param name="filePath">The file path.</param>
        /// <param name="tag">The tag.</param>
        public CryPakFile(FamilyGame game, IFileSystem fileSystem, string filePath, object tag = null) : base(game, fileSystem, filePath, GetPakBinary(game), tag)
        {
            GetObjectFactoryFactory = FormatExtensions.GetObjectFactoryFactory;
        }

        #region GetPakBinary

        static readonly ConcurrentDictionary<string, PakBinary> PakBinarys = new ConcurrentDictionary<string, PakBinary>();

        static PakBinary GetPakBinary(FamilyGame game)
            => PakBinarys.GetOrAdd(game.Id, _ => PakBinaryFactory(game));

        static PakBinary PakBinaryFactory(FamilyGame game)
            => game.Engine switch
            {
                "ArcheAge" => new PakBinaryArcheAge((byte[])game.Key),
                _ => new PakBinaryCry3((byte[])game.Key),
            };

        #endregion

        #region Transforms

        bool ITransformFileObject<IUnknownFileModel>.CanTransformFileObject(PakFile transformTo, object source) => UnknownTransform.CanTransformFileObject(this, transformTo, source);
        Task<IUnknownFileModel> ITransformFileObject<IUnknownFileModel>.TransformFileObjectAsync(PakFile transformTo, object source) => UnknownTransform.TransformFileObjectAsync(this, transformTo, source);

        #endregion
    }
}