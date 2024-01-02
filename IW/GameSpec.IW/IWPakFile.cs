using GameSpec.Formats;
using GameSpec.Formats.Unknown;
using GameSpec.IW.Formats;
using GameSpec.IW.Transforms;
using GameSpec.Transforms;
using System;
using System.IO;
using System.Threading.Tasks;

namespace GameSpec.IW
{
    /// <summary>
    /// IWPakFile
    /// </summary>
    /// <seealso cref="GameSpec.Formats.BinaryPakFile" />
    public class IWPakFile : BinaryPakFile, ITransformFileObject<IUnknownFileModel>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IWPakFile" /> class.
        /// </summary>
        /// <param name="game">The game.</param>
        /// <param name="fileSystem">The file system.</param>
        /// <param name="filePath">The file path.</param>
        /// <param name="tag">The tag.</param>
        public IWPakFile(FamilyGame game, IFileSystem fileSystem, string filePath, object tag = default) : base(game, fileSystem, filePath, PakBinary_IW.Instance, tag)
        {
            ObjectFactoryFactoryMethod = ObjectFactoryFactory;
            UseReader = false;
        }

        #region Factories

        static (FileOption, Func<BinaryReader, FileSource, PakFile, Task<object>>) ObjectFactoryFactory(FileSource source, FamilyGame game)
            => Path.GetExtension(source.Path).ToLowerInvariant() switch
            {
                var x when x == ".cfg" || x == ".csv" || x == ".txt" => (0, Binary_Txt.Factory),
                //".roq" => (0, VIDEO.Factory),
                //".wav" => (0, BinaryWav.Factory),
                //".d3dbsp" => (0, BinaryD3dbsp.Factory),
                ".iwi" => (0, Binary_Iwi.Factory),
                _ => (0, null),
            };

        #endregion

        #region Transforms

        bool ITransformFileObject<IUnknownFileModel>.CanTransformFileObject(PakFile transformTo, object source) => UnknownTransform.CanTransformFileObject(this, transformTo, source);
        Task<IUnknownFileModel> ITransformFileObject<IUnknownFileModel>.TransformFileObjectAsync(PakFile transformTo, object source) => UnknownTransform.TransformFileObjectAsync(this, transformTo, source);

        #endregion
    }
}