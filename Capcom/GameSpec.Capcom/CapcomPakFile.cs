using GameSpec.Formats;
using GameSpec.Formats.Unknown;
using GameSpec.Capcom.Formats;
using GameSpec.Capcom.Transforms;
using GameSpec.Metadata;
using GameSpec.Transforms;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System;

namespace GameSpec.Capcom
{
    /// <summary>
    /// CapcomPakFile
    /// </summary>
    /// <seealso cref="GameSpec.Formats.BinaryPakFile" />
    public class CapcomPakFile : BinaryPakManyFile, ITransformFileObject<IUnknownFileModel>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CapcomPakFile" /> class.
        /// </summary>
        /// <param name="game">The game.</param>
        /// <param name="filePath">The file path.</param>
        /// <param name="tag">The tag.</param>
        public CapcomPakFile(FamilyGame game, string filePath, object tag = null) : base(game, filePath, GetPackBinary(game), tag)
        {
            GetMetadataItems = StandardMetadataItem.GetPakFilesAsync;
            GetObjectFactoryFactory = game.Engine switch
            {
                "Unity" => Unity.Formats.FormatExtensions.GetObjectFactoryFactory,
                _ => FormatExtensions.GetObjectFactoryFactory,
            };
            Open();
        }

        #region GetPackBinary

        static readonly ConcurrentDictionary<string, PakBinary> PakBinarys = new ConcurrentDictionary<string, PakBinary>();

        static PakBinary GetPackBinary(FamilyGame game)
            => PakBinarys.GetOrAdd(game.Id, _ => PackBinaryFactory(game));

        static PakBinary PackBinaryFactory(FamilyGame game)
            => game.Engine switch
            {
                "Capcom" => PakBinaryCapcom.Instance,
                "Unity" => Unity.Formats.PakBinaryUnity.Instance,
                _ => throw new ArgumentOutOfRangeException(nameof(game.Engine)),
            };

        #endregion

        #region Transforms

        bool ITransformFileObject<IUnknownFileModel>.CanTransformFileObject(PakFile transformTo, object source) => UnknownTransform.CanTransformFileObject(this, transformTo, source);
        Task<IUnknownFileModel> ITransformFileObject<IUnknownFileModel>.TransformFileObjectAsync(PakFile transformTo, object source) => UnknownTransform.TransformFileObjectAsync(this, transformTo, source);

        #endregion
    }
}