using GameSpec.Bioware.Formats;
using GameSpec.Formats;
using GameSpec.Formats.Unknown;
using GameSpec.Red.Formats;
using GameSpec.Red.Transforms;
using GameSpec.Transforms;
using System;
using System.IO;
using System.Threading.Tasks;

namespace GameSpec.Red
{
    /// <summary>
    /// RedPakFile
    /// </summary>
    /// <seealso cref="GameSpec.Formats.BinaryPakFile" />
    public class RedPakFile : BinaryPakFile, ITransformFileObject<IUnknownFileModel>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RedPakFile" /> class.
        /// </summary>
        /// <param name="game">The game.</param>
        /// <param name="fileSystem">The file system.</param>
        /// <param name="filePath">The file path.</param>
        /// <param name="tag">The tag.</param>
        public RedPakFile(FamilyGame game, IFileSystem fileSystem, string filePath, object tag = default) : base(game, fileSystem, filePath, PakBinary_Red.Instance, tag)
        {
            ObjectFactoryFactoryMethod = ObjectFactoryFactory;
        }

        #region Factories

        static (FileOption, Func<BinaryReader, FileSource, PakFile, Task<object>>) ObjectFactoryFactory(FileSource source, FamilyGame game)
            => Path.GetExtension(source.Path).ToLowerInvariant() switch
            {
                ".dds" => (0, Binary_Dds.Factory),
                // witcher 1
                var x when x == ".dlg" || x == ".qdb" || x == ".qst" => (0, Binary_Gff.Factory),
                _ => (0, null),
            };

        #endregion

        #region Transforms

        bool ITransformFileObject<IUnknownFileModel>.CanTransformFileObject(PakFile transformTo, object source) => UnknownTransform.CanTransformFileObject(this, transformTo, source);
        Task<IUnknownFileModel> ITransformFileObject<IUnknownFileModel>.TransformFileObjectAsync(PakFile transformTo, object source) => UnknownTransform.TransformFileObjectAsync(this, transformTo, source);

        #endregion
    }
}