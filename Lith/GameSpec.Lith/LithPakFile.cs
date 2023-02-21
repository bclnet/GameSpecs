using GameSpec.Formats;
using GameSpec.Formats.Unknown;
using GameSpec.Lith.Formats;
using GameSpec.Lith.Transforms;
using GameSpec.Metadata;
using GameSpec.Transforms;
using System;
using System.IO;
using System.Threading.Tasks;

namespace GameSpec.Lith
{
    /// <summary>
    /// LithPakFile
    /// </summary>
    /// <seealso cref="GameSpec.Formats.BinaryPakFile" />
    public class LithPakFile : BinaryPakManyFile, ITransformFileObject<IUnknownFileModel>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LithPakFile" /> class.
        /// </summary>
        /// <param name="game">The game.</param>
        /// <param name="filePath">The file path.</param>
        /// <param name="tag">The tag.</param>
        public LithPakFile(FamilyGame game, string filePath, object tag = null) : base(game, filePath, GetPackBinary(Path.GetExtension(filePath).ToLowerInvariant()), tag)
        {
            GetMetadataItems = StandardMetadataItem.GetPakFilesAsync;
            GetObjectFactoryFactory = FormatExtensions.GetObjectFactoryFactory;
            Open();
        }

        #region GetPackBinary

        static PakBinary GetPackBinary(string filePath)
            => filePath != ".zip"
            ? PakBinaryLith.Instance
            : PakBinarySystemZip.Instance;

        #endregion

        #region Transforms

        bool ITransformFileObject<IUnknownFileModel>.CanTransformFileObject(PakFile transformTo, object source) => UnknownTransform.CanTransformFileObject(this, transformTo, source);
        Task<IUnknownFileModel> ITransformFileObject<IUnknownFileModel>.TransformFileObjectAsync(PakFile transformTo, object source) => UnknownTransform.TransformFileObjectAsync(this, transformTo, source);

        #endregion
    }
}