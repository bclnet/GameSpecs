using GameSpec.Cryptic.Formats;
using GameSpec.Cryptic.Transforms;
using GameSpec.Metadata;
using GameSpec.Formats;
using GameSpec.Formats.Unknown;
using GameSpec.Transforms;
using System;
using System.Threading.Tasks;

namespace GameSpec.Cryptic
{
    /// <summary>
    /// CrypticPakFile
    /// </summary>
    /// <seealso cref="GameSpec.Formats.BinaryPakFile" />
    public class CrypticPakFile : BinaryPakManyFile, ITransformFileObject<IUnknownFileModel>
    {
        public static readonly PakBinary ZipInstance = new PakBinarySystemZip();

        /// <summary>
        /// Initializes a new instance of the <see cref="CrypticPakFile" /> class.
        /// </summary>
        /// <param name="family">The family.</param>
        /// <param name="game">The game.</param>
        /// <param name="filePath">The file path.</param>
        /// <param name="tag">The tag.</param>
        public CrypticPakFile(Family family, FamilyGame game, string filePath, object tag = null)
            : base(family, game, filePath, filePath.EndsWith(".zip", StringComparison.OrdinalIgnoreCase) ? ZipInstance : PakBinaryCryptic.Instance, tag)
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