using GameX.Formats.Unknown;
using GameX.Origin.Formats;
using GameX.Origin.Transforms;
using System;
using System.IO;
using System.Threading.Tasks;

namespace GameX.Origin
{
    /// <summary>
    /// OriginPakFile
    /// </summary>
    /// <seealso cref="GameX.Formats.BinaryPakFile" />
    public class OriginPakFile : BinaryPakFile, ITransformFileObject<IUnknownFileModel>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OriginPakFile" /> class.
        /// </summary>
        /// <param name="state">The state.</param>
        public OriginPakFile(PakState state) : base(state, GetPakBinary(state.Game))
        {
            ObjectFactoryFactoryMethod = ObjectFactoryFactory;
        }

        #region Factories

        static PakBinary GetPakBinary(FamilyGame game)
            => game.Id switch
            {
                "U8" => PakBinary_U8.Instance,
                "UO" => PakBinary_UO.Instance,
                "U9" => PakBinary_U9.Instance,
                _ => throw new ArgumentOutOfRangeException(),
            };

        static (FileOption, Func<BinaryReader, FileSource, PakFile, Task<object>>) ObjectFactoryFactory(FileSource source, FamilyGame game)
            => game.Id switch
            {
                "U8" => PakBinary_U8.ObjectFactoryFactory(source, game),
                "UO" => PakBinary_UO.ObjectFactoryFactory(source, game),
                "U9" => PakBinary_U9.ObjectFactoryFactory(source, game),
                _ => throw new ArgumentOutOfRangeException(),
            };

        #endregion

        #region Transforms

        bool ITransformFileObject<IUnknownFileModel>.CanTransformFileObject(PakFile transformTo, object source) => UnknownTransform.CanTransformFileObject(this, transformTo, source);
        Task<IUnknownFileModel> ITransformFileObject<IUnknownFileModel>.TransformFileObject(PakFile transformTo, object source) => UnknownTransform.TransformFileObjectAsync(this, transformTo, source);

        #endregion
    }
}