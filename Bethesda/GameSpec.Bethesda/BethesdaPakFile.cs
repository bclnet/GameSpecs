using GameSpec.Formats;
using GameSpec.Formats.Unknown;
using GameSpec.Metadata;
using GameSpec.Bethesda.Formats;
using GameSpec.Bethesda.Transforms;
using GameSpec.Transforms;
using OpenStack.Graphics;
using System;
using System.IO;
using System.Threading.Tasks;
using static OpenStack.Debug;

namespace GameSpec.Bethesda
{
    /// <summary>
    /// BethesdaPakFile
    /// </summary>
    /// <seealso cref="GameSpec.Formats.BinaryPakFile" />
    public class BethesdaPakFile : BinaryPakFile, ITransformFileObject<IUnknownFileModel>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BethesdaPakFile" /> class.
        /// </summary>
        /// <param name="game">The game.</param>
        /// <param name="fileSystem">The file system.</param>
        /// <param name="filePath">The file path.</param>
        /// <param name="tag">The tag.</param>
        public BethesdaPakFile(FamilyGame game, IFileSystem fileSystem, string filePath, object tag = default) : base(game, fileSystem, filePath, GetPakBinary(game, filePath), tag)
        {
            GetObjectFactoryFactory = FormatExtensions.GetObjectFactoryFactory;
            PathFinders.Add(typeof(ITexture), FindTexture);
        }

        #region PathFinders

        /// <summary>
        /// Finds the actual path of a texture.
        /// </summary>
        public string FindTexture(string path)
        {
            var textureName = Path.GetFileNameWithoutExtension(path);
            var textureNameInTexturesDir = $"textures/{textureName}";
            var filePath = $"{textureNameInTexturesDir}.dds";
            if (Contains(filePath)) return filePath;
            //filePath = $"{textureNameInTexturesDir}.tga";
            //if (Contains(filePath)) return filePath;
            var texturePathWithoutExtension = $"{Path.GetDirectoryName(path)}/{textureName}";
            filePath = $"{texturePathWithoutExtension}.dds";
            if (Contains(filePath)) return filePath;
            //filePath = $"{texturePathWithoutExtension}.tga";
            //if (Contains(filePath)) return filePath;
            Log($"Could not find file '{path}' in a PAK file.");
            return null;
        }

        #endregion

        #region GetPakBinary

        static PakBinary GetPakBinary(FamilyGame game, string filePath)
            => Path.GetExtension(filePath).ToLowerInvariant() switch
            {
                "" => PakBinary_Bsa.Instance,
                ".bsa" => PakBinary_Bsa.Instance,
                ".ba2" => PakBinary_Ba2.Instance,
                ".esm" => PakBinary_Esm.Instance,
                _ => throw new ArgumentOutOfRangeException(nameof(filePath)),
            };

        #endregion

        #region Transforms

        bool ITransformFileObject<IUnknownFileModel>.CanTransformFileObject(PakFile transformTo, object source) => UnknownTransform.CanTransformFileObject(this, transformTo, source);
        Task<IUnknownFileModel> ITransformFileObject<IUnknownFileModel>.TransformFileObjectAsync(PakFile transformTo, object source) => UnknownTransform.TransformFileObjectAsync(this, transformTo, source);

        #endregion
    }
}