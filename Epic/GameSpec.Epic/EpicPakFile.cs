using GameSpec.Epic.Formats;
using GameSpec.Epic.Transforms;
using GameSpec.Formats;
using GameSpec.Formats.Unknown;
using GameSpec.Transforms;
using System.IO;
using System;
using System.Threading.Tasks;

namespace GameSpec.Epic
{
    /// <summary>
    /// EpicPakFile
    /// </summary>
    /// <seealso cref="GameSpec.Formats.BinaryPakFile" />
    public class EpicPakFile : BinaryPakFile, ITransformFileObject<IUnknownFileModel>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EpicPakFile" /> class.
        /// </summary>
        /// <param name="game">The game.</param>
        /// <param name="fileSystem">The file system.</param>
        /// <param name="filePath">The file path.</param>
        /// <param name="tag">The tag.</param>
        public EpicPakFile(FamilyGame game, IFileSystem fileSystem, string filePath, object tag = null) : base(game, fileSystem, filePath, PakBinary_Pck.Instance, tag)
        {
            ObjectFactoryFactoryMethod = ObjectFactoryFactory;
        }

        #region Factories

        // object factory
        public static (FileOption, Func<BinaryReader, FileSource, PakFile, Task<object>>) ObjectFactoryFactory(FileSource source, FamilyGame game)
            => Path.GetExtension(source.Path).ToLowerInvariant() switch
            {
                var x when x == ".cfg" || x == ".txt" => (0, Binary_Txt.Factory),
                ".dds" => (0, Binary_Dds.Factory),
                _ => (0, null),
            };

        #endregion

        #region Transforms

        bool ITransformFileObject<IUnknownFileModel>.CanTransformFileObject(PakFile transformTo, object source) => UnknownTransform.CanTransformFileObject(this, transformTo, source);
        Task<IUnknownFileModel> ITransformFileObject<IUnknownFileModel>.TransformFileObjectAsync(PakFile transformTo, object source) => UnknownTransform.TransformFileObjectAsync(this, transformTo, source);

        #endregion
    }
}