using GameSpec.Formats;
using GameSpec.Formats.Unknown;
using GameSpec.Id.Formats;
using GameSpec.Id.Transforms;
using GameSpec.Metadata;
using GameSpec.Transforms;
using System.IO;
using System.Threading.Tasks;

namespace GameSpec.Id
{
    /// <summary>
    /// IdPakFile
    /// </summary>
    /// <seealso cref="GameSpec.Formats.BinaryPakFile" />
    public class IdPakFile : BinaryPakManyFile, ITransformFileObject<IUnknownFileModel>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IdPakFile" /> class.
        /// </summary>
        /// <param name="family">The family.</param>
        /// <param name="game">The game.</param>
        /// <param name="filePath">The file path.</param>
        /// <param name="tag">The tag.</param>
        public IdPakFile(Family family, FamilyGame game, string filePath, object tag = null)
            : base(family, game, filePath, GetPackBinary(Path.GetExtension(filePath).ToLowerInvariant()), tag)
        {
            GetMetadataItems = StandardMetadataItem.GetPakFilesAsync;
            GetObjectFactoryFactory = FormatExtensions.GetObjectFactoryFactory;
            Open();
        }

        #region GetPackBinary

        static PakBinary GetPackBinary(string extension)
            => extension != ".zip"
            ? PakBinaryId.Instance
            : PakBinarySystemZip.Instance;

        #endregion

        #region Transforms

        bool ITransformFileObject<IUnknownFileModel>.CanTransformFileObject(PakFile transformTo, object source) => UnknownTransform.CanTransformFileObject(this, transformTo, source);
        Task<IUnknownFileModel> ITransformFileObject<IUnknownFileModel>.TransformFileObjectAsync(PakFile transformTo, object source) => UnknownTransform.TransformFileObjectAsync(this, transformTo, source);

        #endregion
    }
}