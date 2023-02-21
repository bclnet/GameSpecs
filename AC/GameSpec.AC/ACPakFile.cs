using GameSpec.AC.Formats;
using GameSpec.AC.Transforms;
using GameSpec.Metadata;
using GameSpec.Formats;
using GameSpec.Formats.Unknown;
using GameSpec.Transforms;
using System.Text;
using System.Threading.Tasks;

namespace GameSpec.AC
{
    /// <summary>
    /// ACPakFile
    /// </summary>
    /// <seealso cref="GameEstate.Formats.BinaryPakFile" />
    public class ACPakFile : BinaryPakManyFile, ITransformFileObject<IUnknownFileModel>
    {
        static ACPakFile() => Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        /// <summary>
        /// Initializes a new instance of the <see cref="ACPakFile" /> class.
        /// </summary>
        /// <param name="game">The game.</param>
        /// <param name="filePath">The file path.</param>
        /// <param name="tag">The tag.</param>
        public ACPakFile(FamilyGame game, string filePath, object tag = null) : base(game, filePath, PakBinaryAC.Instance, tag)
        {
            Options = PakManyOptions.FilesById;
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