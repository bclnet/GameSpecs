using GameSpec.Bioware.Formats;
using GameSpec.Bioware.Transforms;
using GameSpec.Formats;
using GameSpec.Formats.Unknown;
using GameSpec.Metadata;
using GameSpec.Transforms;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;

namespace GameSpec.Bioware
{
    /// <summary>
    /// BiowarePakFile
    /// </summary>
    /// <seealso cref="GameSpec.Formats.BinaryPakFile" />
    public class BiowarePakFile : BinaryPakManyFile, ITransformFileObject<IUnknownFileModel>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BiowarePakFile" /> class.
        /// </summary>
        /// <param name="game">The game.</param>
        /// <param name="filePath">The file path.</param>
        /// <param name="tag">The tag.</param>
        public BiowarePakFile(FamilyGame game, string filePath, object tag = null) : base(game, filePath, GetPackBinary(game, Path.GetExtension(filePath).ToLowerInvariant()), tag)
        {
            GetMetadataItems = StandardMetadataItem.GetPakFilesAsync;
            GetObjectFactoryFactory = FormatExtensions.GetObjectFactoryFactory;
            Open();
        }

        #region GetPackBinary

        static readonly ConcurrentDictionary<string, PakBinary> PakBinarys = new();

        static PakBinary GetPackBinary(FamilyGame game, string extension)
            => extension != ".zip"
            ? PakBinarys.GetOrAdd(game.Id, _ => PackBinaryFactory(game))
            : PakBinarySystemZip.Instance;

        static PakBinary PackBinaryFactory(FamilyGame game)
            => game.Engine switch
            {
                "HeroEngine" => PakBinaryMyp.Instance,
                _ => PakBinaryAurora.Instance,
            };

        #endregion

        #region Transforms

        bool ITransformFileObject<IUnknownFileModel>.CanTransformFileObject(PakFile transformTo, object source) => UnknownTransform.CanTransformFileObject(this, transformTo, source);
        Task<IUnknownFileModel> ITransformFileObject<IUnknownFileModel>.TransformFileObjectAsync(PakFile transformTo, object source) => UnknownTransform.TransformFileObjectAsync(this, transformTo, source);

        #endregion
    }
}