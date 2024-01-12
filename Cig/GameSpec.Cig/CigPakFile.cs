using GameSpec.Cig.Formats;
using GameSpec.Cig.Transforms;
using GameSpec.Crytek.Formats;
using GameSpec.Formats;
using GameSpec.Formats.Unknown;
using GameSpec.Transforms;
using System;
using System.IO;
using System.Threading.Tasks;

namespace GameSpec.Cig
{
    /// <summary>
    /// CigPakFile
    /// </summary>
    /// <seealso cref="GameEstate.Formats.BinaryPakFile" />
    public class CigPakFile : BinaryPakFile, ITransformFileObject<IUnknownFileModel>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CigPakFile" /> class.
        /// </summary>
        /// <param name="game">The game.</param>
        /// <param name="fileSystem">The file system.</param>
        /// <param name="filePath">The file path.</param>
        /// <param name="tag">The tag.</param>
        public CigPakFile(FamilyGame game, IFileSystem fileSystem, string filePath, object tag = default) : base(game, fileSystem, filePath, PakBinary_P4k.Instance, tag)
        {
            ObjectFactoryFactoryMethod = ObjectFactoryFactory;
        }

        #region Factories

        internal static (FileOption, Func<BinaryReader, FileSource, PakFile, Task<object>>) ObjectFactoryFactory(FileSource source, FamilyGame game)
            => Path.GetExtension(source.Path).ToLowerInvariant() switch
            {
                //".cfg" => (0, BinaryDcb.Factory),
                var x when x == ".cfg" || x == ".txt" => (0, Binary_Txt.Factory),
                var x when x == ".mtl" || x == ".xml" => (FileOption.Stream, CryXmlFile.Factory),
                ".dds" => (0, Binary_Dds.Factory),
                ".a" => (0, Binary_DdsA.Factory),
                ".dcb" => (0, Binary_Dcb.Factory),
                var x when x == ".soc" || x == ".cgf" || x == ".cga" || x == ".chr" || x == ".skin" || x == ".anim" => (FileOption.Model, CryFile.Factory),
                _ => (0, null),
            };

        #endregion

        #region Transforms

        bool ITransformFileObject<IUnknownFileModel>.CanTransformFileObject(PakFile transformTo, object source) => UnknownTransform.CanTransformFileObject(this, transformTo, source);
        Task<IUnknownFileModel> ITransformFileObject<IUnknownFileModel>.TransformFileObject(PakFile transformTo, object source) => UnknownTransform.TransformFileObjectAsync(this, transformTo, source);

        #endregion
    }
}