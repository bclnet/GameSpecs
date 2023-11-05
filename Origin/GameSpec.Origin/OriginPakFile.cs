using GameSpec.Formats;
using GameSpec.Formats.Unknown;
using GameSpec.Metadata;
using GameSpec.Origin.Formats;
using GameSpec.Origin.Transforms;
using GameSpec.Transforms;
using System.Threading.Tasks;

namespace GameSpec.Origin
{
    /// <summary>
    /// OriginPakFile
    /// </summary>
    /// <seealso cref="GameSpec.Formats.BinaryPakManyFile" />
    public class OriginPakFile : BinaryPakManyFile, ITransformFileObject<IUnknownFileModel>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OriginPakFile" /> class.
        /// </summary>
        /// <param name="game">The game.</param>
        /// <param name="fileSystem">The file system.</param>
        /// <param name="filePath">The file path.</param>
        /// <param name="tag">The tag.</param>
        public OriginPakFile(FamilyGame game, IFileSystem fileSystem, string filePath, object tag = default) : base(game, fileSystem, filePath, GetPakBinary(game), tag)
        {
            GetMetadataItems = StandardMetadataItem.GetPakFilesAsync;
            GetObjectFactoryFactory = FormatExtensions.GetObjectFactoryFactory;
        }

        #region GetPakBinary

        static PakBinary GetPakBinary(FamilyGame game)
            => game.Id == "UO"
            ? PakBinaryOriginUO.Instance
            : PakBinaryOriginU9.Instance;

        #endregion

        #region Transforms

        bool ITransformFileObject<IUnknownFileModel>.CanTransformFileObject(PakFile transformTo, object source) => UnknownTransform.CanTransformFileObject(this, transformTo, source);
        Task<IUnknownFileModel> ITransformFileObject<IUnknownFileModel>.TransformFileObjectAsync(PakFile transformTo, object source) => UnknownTransform.TransformFileObjectAsync(this, transformTo, source);

        #endregion
    }
}