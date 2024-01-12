using GameSpec.Capcom.Formats;
using GameSpec.Capcom.Transforms;
using GameSpec.Formats;
using GameSpec.Formats.Unknown;
using GameSpec.Transforms;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;

namespace GameSpec.Capcom
{
    /// <summary>
    /// CapcomPakFile
    /// </summary>
    /// <seealso cref="GameSpec.Formats.BinaryPakFile" />
    public class CapcomPakFile : BinaryPakFile, ITransformFileObject<IUnknownFileModel>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CapcomPakFile" /> class.
        /// </summary>
        /// <param name="game">The game.</param>
        /// <param name="fileSystem">The file system.</param>
        /// <param name="filePath">The file path.</param>
        /// <param name="tag">The tag.</param>
        public CapcomPakFile(FamilyGame game, IFileSystem fileSystem, string filePath, object tag = null) : base(game, fileSystem, filePath, filePath != null ? GetPakBinary(game, Path.GetExtension(filePath).ToLowerInvariant()) : null, tag)
        {
            ObjectFactoryFactoryMethod = game.Engine switch
            {
                "Unity" => Unity.UnityPakFile.ObjectFactoryFactory,
                _ => ObjectFactoryFactory,
            };
        }

        #region Factories

        static readonly ConcurrentDictionary<string, PakBinary> PakBinarys = new ConcurrentDictionary<string, PakBinary>();

        static PakBinary GetPakBinary(FamilyGame game, string extension) => PakBinarys.GetOrAdd(game.Id, _ => PakBinaryFactory(game, extension));

        static PakBinary PakBinaryFactory(FamilyGame game, string extension)
            => game.Engine switch
            {
                "Zip" => PakBinary_Zip.GetPakBinary(game),
                "Unity" => Unity.Formats.PakBinary_Unity.Instance,
                _ => extension switch
                {
                    ".pak" => PakBinary_Kpka.Instance,
                    ".arc" => PakBinary_Arc.Instance,
                    ".big" => PakBinary_Big.Instance,
                    ".bundle" => PakBinary_Bundle.Instance,
                    ".mbundle" => PakBinary_Plist.Instance,
                    _ => throw new ArgumentOutOfRangeException(nameof(extension)),
                },
            };

        static (FileOption, Func<BinaryReader, FileSource, PakFile, Task<object>>) ObjectFactoryFactory(FileSource source, FamilyGame game)
            => Path.GetExtension(source.Path).ToLowerInvariant() switch
            {
                ".png" => (0, Binary_Img.Factory),
                var x when x == ".cfg" || x == ".csv" || x == ".txt" => (0, Binary_Txt.Factory),
                _ => (0, null),
            };

        #endregion

        #region Transforms

        bool ITransformFileObject<IUnknownFileModel>.CanTransformFileObject(PakFile transformTo, object source) => UnknownTransform.CanTransformFileObject(this, transformTo, source);
        Task<IUnknownFileModel> ITransformFileObject<IUnknownFileModel>.TransformFileObject(PakFile transformTo, object source) => UnknownTransform.TransformFileObjectAsync(this, transformTo, source);

        #endregion
    }
}