using GameSpec.Formats;
using GameSpec.Formats.Unknown;
using GameSpec.Black.Formats;
using GameSpec.Black.Transforms;
using GameSpec.Metadata;
using GameSpec.Transforms;
using System;
using System.IO;
using System.Threading.Tasks;

namespace GameSpec.Black
{
    /// <summary>
    /// BlackPakFile
    /// </summary>
    /// <seealso cref="GameSpec.Formats.BinaryPakFile" />
    public class BlackPakFile : BinaryPakFile, ITransformFileObject<IUnknownFileModel>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BlackPakFile" /> class.
        /// </summary>
        /// <param name="game">The game.</param>
        /// <param name="fileSystem">The file system.</param>
        /// <param name="filePath">The file path.</param>
        /// <param name="tag">The tag.</param>
        public BlackPakFile(FamilyGame game, IFileSystem fileSystem, string filePath, object tag = default) : base(game, fileSystem, filePath, GetPakBinary(game, Path.GetExtension(filePath).ToLowerInvariant()), tag)
        {
            GetObjectFactoryFactory = FormatExtensions.GetObjectFactoryFactory;
        }

        #region GetPakBinary

        static PakBinary GetPakBinary(FamilyGame game, string extension)
            => PakBinary_Dat.Instance;

        //string.IsNullOrEmpty(extension)
        //? PakBinary_Dat.Instance
        //: extension switch
        //{
        //    ".dat" => PakBinary_Dat.Instance,
        //    _ => throw new ArgumentOutOfRangeException(nameof(extension)),
        //};

        #endregion

        #region Transforms

        bool ITransformFileObject<IUnknownFileModel>.CanTransformFileObject(PakFile transformTo, object source) => UnknownTransform.CanTransformFileObject(this, transformTo, source);
        Task<IUnknownFileModel> ITransformFileObject<IUnknownFileModel>.TransformFileObjectAsync(PakFile transformTo, object source) => UnknownTransform.TransformFileObjectAsync(this, transformTo, source);

        #endregion
    }
}