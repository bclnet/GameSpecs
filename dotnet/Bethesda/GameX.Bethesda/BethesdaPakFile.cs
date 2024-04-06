using GameX.Bethesda.Formats;
using GameX.Bethesda.Transforms;
using GameX.Formats;
using GameX.Formats.Unknown;
using OpenStack.Graphics;
using System;
using System.IO;
using System.Threading.Tasks;
using static OpenStack.Debug;

namespace GameX.Bethesda
{
    /// <summary>
    /// BethesdaPakFile
    /// </summary>
    /// <seealso cref="GameX.Formats.BinaryPakFile" />
    public class BethesdaPakFile : BinaryPakFile, ITransformFileObject<IUnknownFileModel>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BethesdaPakFile" /> class.
        /// </summary>
        /// <param name="state">The state.</param>
        public BethesdaPakFile(PakState state) : base(state, GetPakBinary(state.Game, Path.GetExtension(state.Path).ToLowerInvariant()))
        {
            ObjectFactoryFactoryMethod = ObjectFactoryFactory;
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

        #region Factories

        static PakBinary GetPakBinary(FamilyGame game, string extension)
            => extension switch
            {
                "" => PakBinary_Bsa.Instance,
                ".bsa" => PakBinary_Bsa.Instance,
                ".ba2" => PakBinary_Ba2.Instance,
                ".esm" => PakBinary_Esm.Instance,
                _ => throw new ArgumentOutOfRangeException(nameof(extension)),
            };

        static Task<object> NiFactory(BinaryReader r, FileSource f, PakFile s) { var file = new NiFile(Path.GetFileNameWithoutExtension(f.Path)); file.Read(r); return Task.FromResult((object)file); }

        static (FileOption, Func<BinaryReader, FileSource, PakFile, Task<object>>) ObjectFactoryFactory(FileSource source, FamilyGame game)
            => Path.GetExtension(source.Path).ToLowerInvariant() switch
            {
                ".dds" => (0, Binary_Dds.Factory),
                ".nif" => (0, NiFactory),
                _ => (0, null),
            };

        #endregion

        #region Transforms

        bool ITransformFileObject<IUnknownFileModel>.CanTransformFileObject(PakFile transformTo, object source) => UnknownTransform.CanTransformFileObject(this, transformTo, source);
        Task<IUnknownFileModel> ITransformFileObject<IUnknownFileModel>.TransformFileObject(PakFile transformTo, object source) => UnknownTransform.TransformFileObjectAsync(this, transformTo, source);

        #endregion
    }
}