using GameSpec.Formats;
using GameSpec.Formats.Unknown;
using GameSpec.Metadata;
using GameSpec.Tes.Formats;
using GameSpec.Tes.Transforms;
using GameSpec.Transforms;
using Google.Protobuf;
using OpenStack.Graphics;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;
using static OpenStack.Debug;

namespace GameSpec.Tes
{
    /// <summary>
    /// TesPakFile
    /// </summary>
    /// <seealso cref="GameSpec.Formats.BinaryPakFile" />
    public class TesPakFile : BinaryPakManyFile, ITransformFileObject<IUnknownFileModel>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TesPakFile" /> class.
        /// </summary>
        /// <param name="game">The game.</param>
        /// <param name="filePath">The file path.</param>
        /// <param name="tag">The tag.</param>
        public TesPakFile(FamilyGame game, string filePath, object tag = null) : base(game, filePath, GetPackBinary(Path.GetExtension(filePath)), tag)
        {
            GetMetadataItems = StandardMetadataItem.GetPakFilesAsync;
            GetObjectFactoryFactory = FormatExtensions.GetObjectFactoryFactory;
            PathFinders.Add(typeof(ITexture), FindTexture);
            Open();
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

        #region GetPackBinary

        static PakBinary GetPackBinary(string extension)
            => extension.ToLowerInvariant() switch
            {
                ".bsa" => PakBinaryTesBsa.Instance,
                ".ba2" => PakBinaryTesBa2.Instance,
                ".dat" => PakBinaryTesDat.Instance,
                ".esm" => PakBinaryTesEsm.Instance,
                _ => throw new ArgumentOutOfRangeException(nameof(extension)),
            };

        #endregion

        #region Transforms

        bool ITransformFileObject<IUnknownFileModel>.CanTransformFileObject(PakFile transformTo, object source) => UnknownTransform.CanTransformFileObject(this, transformTo, source);
        Task<IUnknownFileModel> ITransformFileObject<IUnknownFileModel>.TransformFileObjectAsync(PakFile transformTo, object source) => UnknownTransform.TransformFileObjectAsync(this, transformTo, source);

        #endregion
    }
}