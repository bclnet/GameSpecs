using GameSpec.Arkane.Formats;
using GameSpec.Arkane.Transforms;
using GameSpec.Formats;
using GameSpec.Formats.Unknown;
using GameSpec.Metadata;
using GameSpec.Transforms;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace GameSpec.Arkane
{
    /// <summary>
    /// ArkanePakFile
    /// </summary>
    /// <seealso cref="GameEstate.Formats.BinaryPakFile" />
    public class ArkanePakFile : BinaryPakManyFile, ITransformFileObject<IUnknownFileModel>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ArkanePakFile" /> class.
        /// </summary>
        /// <param name="game">The game.</param>
        /// <param name="fileSystem">The file system.</param>
        /// <param name="filePath">The file path.</param>
        /// <param name="tag">The tag.</param>
        public ArkanePakFile(FamilyGame game, IFileSystem fileSystem, string filePath, object tag = null) : base(game, fileSystem, filePath, GetPakBinary(game), tag)
        {
            Options = PakManyOptions.FilesById;
            GetMetadataItems = StandardMetadataItem.GetPakFilesAsync;
            GetObjectFactoryFactory = game.Engine switch
            {
                "CryEngine" => Crytek.Formats.FormatExtensions.GetObjectFactoryFactory,
                "Unreal" => Epic.Formats.FormatExtensions.GetObjectFactoryFactory,
                "Valve" => Valve.Formats.FormatExtensions.GetObjectFactoryFactory,
                "idTech7" => Id.Formats.FormatExtensions.GetObjectFactoryFactory,
                _ => FormatExtensions.GetObjectFactoryFactory,
            };
        }

        #region GetPakBinary

        static readonly ConcurrentDictionary<string, PakBinary> PakBinarys = new ConcurrentDictionary<string, PakBinary>();

        static PakBinary GetPakBinary(FamilyGame game)
            => PakBinarys.GetOrAdd(game.Id, _ => PakBinaryFactory(game));

        static PakBinary PakBinaryFactory(FamilyGame game)
            => game.Engine switch
            {
                "Danae" => PakBinaryDanae.Instance,
                "Void" => PakBinaryVoid.Instance,
                "CryEngine" => Crytek.Formats.PakBinaryCry3.Instance,
                "Unreal" => Epic.Formats.PakBinaryPck.Instance,
                "Valve" => Valve.Formats.PakBinaryVpk.Instance,
                //"idTech7" => Id.Formats.PakBinaryVpk.Instance,
                _ => throw new ArgumentOutOfRangeException(nameof(game.Engine)),
            };

        #endregion

        #region Transforms

        bool ITransformFileObject<IUnknownFileModel>.CanTransformFileObject(PakFile transformTo, object source) => UnknownTransform.CanTransformFileObject(this, transformTo, source);
        Task<IUnknownFileModel> ITransformFileObject<IUnknownFileModel>.TransformFileObjectAsync(PakFile transformTo, object source) => UnknownTransform.TransformFileObjectAsync(this, transformTo, source);

        #endregion
    }
}