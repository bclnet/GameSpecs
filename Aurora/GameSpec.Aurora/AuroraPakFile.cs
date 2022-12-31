using GameSpec.Aurora.Formats;
using GameSpec.Aurora.Transforms;
using GameSpec.Formats;
using GameSpec.Formats.Unknown;
using GameSpec.Metadata;
using GameSpec.Transforms;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace GameSpec.Aurora
{
    /// <summary>
    /// AuroraPakFile
    /// </summary>
    /// <seealso cref="GameSpec.Formats.BinaryPakFile" />
    public class AuroraPakFile : BinaryPakManyFile, ITransformFileObject<IUnknownFileModel>
    {
        static readonly ConcurrentDictionary<string, PakBinary> PakBinarys = new ConcurrentDictionary<string, PakBinary>();
        public static readonly PakBinary ZipInstance = new PakBinarySystemZip();

        /// <summary>
        /// Initializes a new instance of the <see cref="AuroraPakFile" /> class.
        /// </summary>
        /// <param name="family">The family.</param>
        /// <param name="game">The game.</param>
        /// <param name="filePath">The file path.</param>
        /// <param name="tag">The tag.</param>
        public AuroraPakFile(Family family, string game, string filePath, object tag = null)
            : base(family, game, filePath, GetPackBinary(family, game, filePath), tag)
        {
            GetMetadataItems = StandardMetadataItem.GetPakFilesAsync;
            GetObjectFactoryFactory = FormatExtensions.GetObjectFactoryFactory;
            Open();
        }

        #region GetPackBinary

        static PakBinary PackBinaryFactory(FamilyGame game)
            => game.Game switch
            {
                "TOR" => PakBinaryMyp.Instance,
                _ => PakBinaryAurora.Instance,
            };

        static PakBinary GetPackBinary(Family family, string game, string filePath)
            => !filePath.EndsWith(".zip", StringComparison.OrdinalIgnoreCase)
            ? PakBinarys.GetOrAdd(game, _ => PackBinaryFactory(family.GetGame(game).game))
            : ZipInstance;

        #endregion

        #region Transforms

        bool ITransformFileObject<IUnknownFileModel>.CanTransformFileObject(PakFile transformTo, object source) => UnknownTransform.CanTransformFileObject(this, transformTo, source);
        Task<IUnknownFileModel> ITransformFileObject<IUnknownFileModel>.TransformFileObjectAsync(PakFile transformTo, object source) => UnknownTransform.TransformFileObjectAsync(this, transformTo, source);

        #endregion
    }
}