using GameX.Formats;
using GameX.Formats.Unknown;
using GameX.Ubisoft.Formats;
using GameX.Ubisoft.Transforms;
using System;
using System.IO;
using System.Threading.Tasks;

namespace GameX.Ubisoft
{
    /// <summary>
    /// UbisoftPakFile
    /// </summary>
    /// <seealso cref="GameX.Formats.BinaryPakFile" />
    public class UbisoftPakFile : BinaryPakFile, ITransformFileObject<IUnknownFileModel>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UbisoftPakFile" /> class.
        /// </summary>
        /// <param name="state">The state.</param>
        public UbisoftPakFile(PakState state) : base(state, GetPakBinary(state.Game, state.Path))
        {
            ObjectFactoryFactoryMethod = ObjectFactoryFactory;
        }

        #region Factories

        static PakBinary GetPakBinary(FamilyGame game, string filePath)
            => filePath == null || Path.GetExtension(filePath).ToLowerInvariant() != ".zip"
                ? PakBinary_Ubi.Instance
                : PakBinary_Zip.GetPakBinary(game);

        static (FileOption, Func<BinaryReader, FileSource, PakFile, Task<object>>) ObjectFactoryFactory(FileSource source, FamilyGame game)
           => Path.GetExtension(source.Path).ToLowerInvariant() switch
           {
               ".dds" => (0, Binary_Dds.Factory),
               _ => (0, null),
           };

        #endregion

        #region Transforms

        bool ITransformFileObject<IUnknownFileModel>.CanTransformFileObject(PakFile transformTo, object source) => UnknownTransform.CanTransformFileObject(this, transformTo, source);
        Task<IUnknownFileModel> ITransformFileObject<IUnknownFileModel>.TransformFileObject(PakFile transformTo, object source) => UnknownTransform.TransformFileObjectAsync(this, transformTo, source);

        #endregion
    }
}