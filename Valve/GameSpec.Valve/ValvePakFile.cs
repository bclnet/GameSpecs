using GameSpec.Metadata;
using GameSpec.Formats;
using GameSpec.Formats.Unknown;
using GameSpec.Transforms;
using GameSpec.Valve.Formats;
using GameSpec.Valve.Transforms;
using System;
using System.Threading.Tasks;
using static OpenStack.Debug;
using System.Collections.Concurrent;
using System.IO;

namespace GameSpec.Valve
{
    /// <summary>
    /// ValvePakFile
    /// </summary>
    /// <seealso cref="GameSpec.Formats.BinaryPakFile" />
    public class ValvePakFile : BinaryPakManyFile, ITransformFileObject<IUnknownFileModel>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ValvePakFile" /> class.
        /// </summary>
        /// <param name="game">The game.</param>
        /// <param name="filePath">The file path.</param>
        /// <param name="tag">The tag.</param>
        public ValvePakFile(FamilyGame game, IFileSystem fileSystem, string filePath, object tag = null) : base(game, fileSystem, filePath, GetPakBinary(game, filePath), tag)
        {
            GetMetadataItems = StandardMetadataItem.GetPakFilesAsync;
            GetObjectFactoryFactory = FormatExtensions.GetObjectFactoryFactory;
            PathFinders.Add(typeof(object), FindBinary);
        }

        #region GetPakBinary

        static readonly ConcurrentDictionary<string, PakBinary> PakBinarys = new ConcurrentDictionary<string, PakBinary>();

        static PakBinary GetPakBinary(FamilyGame game, string filePath)
            => PakBinarys.GetOrAdd(game.Id, _ => PakBinaryFactory(game, filePath != null ? Path.GetExtension(filePath).ToLowerInvariant() : null));

        static PakBinary PakBinaryFactory(FamilyGame game, string extension)
            => game.Engine switch
            {
                "Unity" => Unity.Formats.PakBinaryUnity.Instance,
                "Source" => PakBinaryVpk.Instance,
                "HL" => PakBinaryWad.Instance,
                _ => null,
                //_ => extension switch
                //{
                //    ".wad" => PakBinaryWad.Instance,
                //    _ => null,
                //}
            };

        #endregion

        #region PathFinders

        /// <summary>
        /// Finds the actual path of a texture.
        /// </summary>
        public string FindBinary(string path)
        {
            if (Contains(path)) return path;
            if (!path.EndsWith("_c", StringComparison.Ordinal)) path = $"{path}_c";
            if (Contains(path)) return path;
            Log($"Could not find file '{path}' in a PAK file.");
            return null;
        }

        #endregion

        #region Transforms

        bool ITransformFileObject<IUnknownFileModel>.CanTransformFileObject(PakFile transformTo, object source) => UnknownTransform.CanTransformFileObject(this, transformTo, source);
        Task<IUnknownFileModel> ITransformFileObject<IUnknownFileModel>.TransformFileObjectAsync(PakFile transformTo, object source) => UnknownTransform.TransformFileObjectAsync(this, transformTo, source);

        #endregion
    }
}