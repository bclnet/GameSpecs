using GameX.Formats;
using GameX.Formats.Unknown;
using GameX.Lucas.Formats;
using GameX.Lucas.Transforms;
using System;
using System.IO;
using System.Threading.Tasks;

namespace GameX.Lucas
{
    /// <summary>
    /// LucasPakFile
    /// </summary>
    /// <seealso cref="GameX.Formats.BinaryPakFile" />
    public class LucasPakFile : BinaryPakFile, ITransformFileObject<IUnknownFileModel>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LucasPakFile" /> class.
        /// </summary>
        /// <param name="state">The state.</param>
        public LucasPakFile(PakState state) : base(state, GetPakBinary(state.Game, Path.GetExtension(state.Path).ToLowerInvariant()))
        {
            ObjectFactoryFactoryMethod = ObjectFactoryFactory;
        }

        #region Factories

        static PakBinary GetPakBinary(FamilyGame game, string extension)
            => game.Engine switch
            {
                "SPUTM" => PakBinary_Scumm.Instance,
                "Jedi" => PakBinary_Jedi.Instance,
                _ => null,
            };

        static (FileOption, Func<BinaryReader, FileSource, PakFile, Task<object>>) ObjectFactoryFactory(FileSource source, FamilyGame game)
            => Path.GetExtension(source.Path).ToLowerInvariant() switch
            {
                var x when x == ".cfg" || x == ".csv" || x == ".txt" => (0, Binary_Txt.Factory),
                var x when x == ".bmp" => (0, Binary_Img.Factory),
                ".pcx" => (0, Binary_Pcx.Factory),
                ".wav" => (0, Binary_Snd.Factory),
                ".nwx" => (0, Binary_Nwx.Factory),
                ".san" => (0, Binary_San.Factory),
                _ => (0, null),
            };

        #endregion

        #region Transforms

        bool ITransformFileObject<IUnknownFileModel>.CanTransformFileObject(PakFile transformTo, object source) => UnknownTransform.CanTransformFileObject(this, transformTo, source);
        Task<IUnknownFileModel> ITransformFileObject<IUnknownFileModel>.TransformFileObject(PakFile transformTo, object source) => UnknownTransform.TransformFileObjectAsync(this, transformTo, source);

        #endregion
    }
}