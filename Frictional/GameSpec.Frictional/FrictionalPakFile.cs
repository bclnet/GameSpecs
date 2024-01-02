using GameSpec.Formats;
using GameSpec.Formats.Unknown;
using GameSpec.Frictional.Formats;
using GameSpec.Frictional.Transforms;
using GameSpec.Transforms;
using System;
using System.IO;
using System.Threading.Tasks;

namespace GameSpec.Frictional
{
    /// <summary>
    /// FrictionalPakFile
    /// </summary>
    /// <seealso cref="GameSpec.Formats.BinaryPakFile" />
    public class FrictionalPakFile : BinaryPakFile, ITransformFileObject<IUnknownFileModel>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FrictionalPakFile" /> class.
        /// </summary>
        /// <param name="game">The game.</param>
        /// <param name="fileSystem">The file system.</param>
        /// <param name="filePath">The file path.</param>
        /// <param name="tag">The tag.</param>
        public FrictionalPakFile(FamilyGame game, IFileSystem fileSystem, string filePath, object tag = default) : base(game, fileSystem, filePath, PakBinary_Hpl.Instance, tag)
        {
            ObjectFactoryFactoryMethod = ObjectFactoryFactory;
        }

        #region Factories

        static (FileOption, Func<BinaryReader, FileSource, PakFile, Task<object>>) ObjectFactoryFactory(FileSource source, FamilyGame game)
            => Path.GetExtension(source.Path).ToLowerInvariant() switch
            {
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