using GameSpec.Formats;
using GameSpec.Formats.Unknown;
using GameSpec.Origin.Formats;
using GameSpec.Origin.Formats.UO;
using GameSpec.Origin.Transforms;
using GameSpec.Transforms;
using System;
using System.IO;
using System.Threading.Tasks;

namespace GameSpec.Origin
{
    /// <summary>
    /// OriginPakFile
    /// </summary>
    /// <seealso cref="GameSpec.Formats.BinaryPakFile" />
    public class OriginPakFile : BinaryPakFile, ITransformFileObject<IUnknownFileModel>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OriginPakFile" /> class.
        /// </summary>
        /// <param name="game">The game.</param>
        /// <param name="fileSystem">The file system.</param>
        /// <param name="filePath">The file path.</param>
        /// <param name="tag">The tag.</param>
        public OriginPakFile(FamilyGame game, IFileSystem fileSystem, string filePath, object tag = default) : base(game, fileSystem, filePath, GetPakBinary(game), tag)
        {
            ObjectFactoryFactoryMethod = ObjectFactoryFactory;
        }

        #region Factories

        static PakBinary GetPakBinary(FamilyGame game)
            => game.Id switch
            {
                "U8" => PakBinary_U8.Instance,
                "U9" => PakBinary_U9.Instance,
                "UO" => PakBinary_UO.Instance,
                _ => throw new ArgumentOutOfRangeException(),
            };

        static (FileOption, Func<BinaryReader, FileSource, PakFile, Task<object>>) ObjectFactoryFactory(FileSource source, FamilyGame game)
            => game.Id switch
            {
                "UO" => source.Path.ToLowerInvariant() switch
                {
                    var x when x.StartsWith("cliloc") => (0, Binary_Cliloc.Factory),
                    "verdata.mul" => (0, Binary_Verdata.Factory),
                    _ => Path.GetExtension(source.Path).ToLowerInvariant() switch
                    {
                        //".mul" => (0, Binary_Ignore.Factory($"refer to {source.Path[..^4]}.idx")),
                        _ => (0, null),
                    }
                },
                _ => Path.GetExtension(source.Path).ToLowerInvariant() switch
                {
                    ".dds" => (0, Binary_Dds.Factory),
                    _ => (0, null),
                }
            };

        #endregion

        #region Transforms

        bool ITransformFileObject<IUnknownFileModel>.CanTransformFileObject(PakFile transformTo, object source) => UnknownTransform.CanTransformFileObject(this, transformTo, source);
        Task<IUnknownFileModel> ITransformFileObject<IUnknownFileModel>.TransformFileObject(PakFile transformTo, object source) => UnknownTransform.TransformFileObjectAsync(this, transformTo, source);

        #endregion
    }
}