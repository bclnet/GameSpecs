using GameSpec.Blizzard.Formats;
using GameSpec.Blizzard.Transforms;
using GameSpec.Formats;
using GameSpec.Formats.Unknown;
using GameSpec.Transforms;
using System;
using System.IO;
using System.Threading.Tasks;

namespace GameSpec.Blizzard
{
    /// <summary>
    /// BlizzardPakFile
    /// </summary>
    /// <seealso cref="GameSpec.Formats.BinaryPakFile" />
    public class BlizzardPakFile : BinaryPakFile, ITransformFileObject<IUnknownFileModel>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BlizzardPakFile" /> class.
        /// </summary>
        /// <param name="game">The game.</param>
        /// <param name="fileSystem">The file system.</param>
        /// <param name="filePath">The file path.</param>
        /// <param name="tag">The tag.</param>
        public BlizzardPakFile(FamilyGame game, IFileSystem fileSystem, string filePath, object tag = null) : base(game, fileSystem, filePath, GetPakBinary(game, Path.GetExtension(filePath).ToLowerInvariant()), tag)
        {
            ObjectFactoryFactoryMethod = ObjectFactoryFactory;
            UseReader = false;
        }

        #region Factories

        static PakBinary GetPakBinary(FamilyGame game, string extension)
            => PakBinaryBlizzard.Instance;

        static (FileOption, Func<BinaryReader, FileSource, PakFile, Task<object>>) ObjectFactoryFactory(FileSource source, FamilyGame game)
            => Path.GetExtension(source.Path).ToLowerInvariant() switch
            {
                ".dds" => (0, Binary_Dds.Factory),
                var x when x == ".cfg" || x == ".csv" || x == ".txt" => (0, Binary_Txt.Factory),
                _ => (0, null),
            };

        #endregion

        #region Transforms

        bool ITransformFileObject<IUnknownFileModel>.CanTransformFileObject(PakFile transformTo, object source) => UnknownTransform.CanTransformFileObject(this, transformTo, source);
        Task<IUnknownFileModel> ITransformFileObject<IUnknownFileModel>.TransformFileObjectAsync(PakFile transformTo, object source) => UnknownTransform.TransformFileObjectAsync(this, transformTo, source);

        #endregion
    }
}