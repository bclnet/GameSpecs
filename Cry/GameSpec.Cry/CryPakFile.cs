using GameSpec.Cry.Formats;
using GameSpec.Cry.Transforms;
using GameSpec.Metadata;
using GameSpec.Formats;
using GameSpec.Formats.Unknown;
using GameSpec.Transforms;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
[assembly: InternalsVisibleTo("GameSpec.Rsi")]

namespace GameSpec.Cry
{
    /// <summary>
    /// CryPakFile
    /// </summary>
    /// <seealso cref="GameSpec.Formats.BinaryPakFile" />
    public class CryPakFile : BinaryPakManyFile, ITransformFileObject<IUnknownFileModel>
    {
        static readonly ConcurrentDictionary<string, PakBinary> PakBinarys = new ConcurrentDictionary<string, PakBinary>();

        /// <summary>
        /// Initializes a new instance of the <see cref="CryPakFile" /> class.
        /// </summary>
        /// <param name="family">The family.</param>
        /// <param name="game">The game.</param>
        /// <param name="filePath">The file path.</param>
        /// <param name="tag">The tag.</param>
        public CryPakFile(Family family, string game, string filePath, object tag = null)
            : base(family, game, filePath, GetPackBinary(family, game), tag)
        {
            GetMetadataItems = StandardMetadataItem.GetPakFilesAsync;
            GetObjectFactoryFactory = FormatExtensions.GetObjectFactoryFactory;
            Open();
        }

        #region GetPackBinary

        static PakBinary PackBinaryFactory(FamilyGame game)
        {
            var key = game.Key is Family.ByteKey z ? z.Key : null;
            return game.Id switch
            {
                "ArcheAge" => new PakBinaryBespoke(key),
                _ => new PakBinaryCry3(key),
            };
        }

        static PakBinary GetPackBinary(Family family, string game)
            => PakBinarys.GetOrAdd(game, _ => PackBinaryFactory(family.GetGame(game).game));

#endregion

        #region Transforms

        bool ITransformFileObject<IUnknownFileModel>.CanTransformFileObject(PakFile transformTo, object source) => UnknownTransform.CanTransformFileObject(this, transformTo, source);
        Task<IUnknownFileModel> ITransformFileObject<IUnknownFileModel>.TransformFileObjectAsync(PakFile transformTo, object source) => UnknownTransform.TransformFileObjectAsync(this, transformTo, source);

        #endregion
    }
}