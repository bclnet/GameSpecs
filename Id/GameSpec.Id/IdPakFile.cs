using GameSpec.Id.Formats;
using GameSpec.Id.Transforms;
using GameSpec.Metadata;
using GameSpec.Formats;
using GameSpec.Formats.Unknown;
using GameSpec.Transforms;
using System;
using System.Threading.Tasks;

namespace GameSpec.Id
{
    /// <summary>
    /// IdPakFile
    /// </summary>
    /// <seealso cref="GameSpec.Formats.BinaryPakFile" />
    public class IdPakFile : BinaryPakManyFile, ITransformFileObject<IUnknownFileModel>
    {
        public static readonly PakBinary ZipInstance = new PakBinarySystemZip();

        /// <summary>
        /// Initializes a new instance of the <see cref="IdPakFile" /> class.
        /// </summary>
        /// <param name="family">The family.</param>
        /// <param name="game">The game.</param>
        /// <param name="filePath">The file path.</param>
        /// <param name="tag">The tag.</param>
        public IdPakFile(Family family, string game, string filePath, object tag = null)
            : base(family, game, filePath, filePath.EndsWith(".zip", StringComparison.OrdinalIgnoreCase) ? ZipInstance : PakBinaryId.Instance, tag)
        {
            GetMetadataItems = StandardMetadataItem.GetPakFilesAsync;
            GetObjectFactoryFactory = FormatExtensions.GetObjectFactoryFactory;
            Open();
        }

        #region Transforms

        bool ITransformFileObject<IUnknownFileModel>.CanTransformFileObject(PakFile transformTo, object source) => UnknownTransform.CanTransformFileObject(this, transformTo, source);
        Task<IUnknownFileModel> ITransformFileObject<IUnknownFileModel>.TransformFileObjectAsync(PakFile transformTo, object source) => UnknownTransform.TransformFileObjectAsync(this, transformTo, source);

        #endregion
    }
}