using GameSpec.Cyanide.Formats;
using GameSpec.Cyanide.Transforms;
using GameSpec.Metadata;
using GameSpec.Formats;
using GameSpec.Formats.Unknown;
using GameSpec.Transforms;
using System.Threading.Tasks;

namespace GameSpec.Cyanide
{
    /// <summary>
    /// CyanidePakFile
    /// </summary>
    /// <seealso cref="GameSpec.Formats.BinaryPakFile" />
    public class CyanidePakFile : BinaryPakManyFile, ITransformFileObject<IUnknownFileModel>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CyanidePakFile" /> class.
        /// </summary>
        /// <param name="family">The family.</param>
        /// <param name="game">The game.</param>
        /// <param name="filePath">The file path.</param>
        /// <param name="tag">The tag.</param>
        public CyanidePakFile(Family family, string game, string filePath, object tag = null)
            : base(family, game, filePath, PakBinaryCyanide.Instance, tag)
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