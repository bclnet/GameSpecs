using GameX.Bioware.Formats;
using GameX.Bioware.Transforms;
using GameX.Formats;
using GameX.Formats.Unknown;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;

namespace GameX.Bioware
{
    /// <summary>
    /// BiowarePakFile
    /// </summary>
    /// <seealso cref="GameX.Formats.BinaryPakFile" />
    public class BiowarePakFile : BinaryPakFile, ITransformFileObject<IUnknownFileModel>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BiowarePakFile" /> class.
        /// </summary>
        /// <param name="state">The state.</param>
        public BiowarePakFile(PakState state) : base(state, GetPakBinary(state.Game, Path.GetExtension(state.Path).ToLowerInvariant()))
        {
            ObjectFactoryFactoryMethod = ObjectFactoryFactory;
        }

        #region Factories

        static readonly ConcurrentDictionary<string, PakBinary> PakBinarys = new ConcurrentDictionary<string, PakBinary>();

        static PakBinary GetPakBinary(FamilyGame game, string extension)
            => extension != ".zip"
                ? PakBinarys.GetOrAdd(game.Id, _ => PakBinaryFactory(game))
                : PakBinary_Zip.GetPakBinary(game);

        static PakBinary PakBinaryFactory(FamilyGame game)
            => game.Engine switch
            {
                //"Infinity" => PakBinary_Infinity.Instance,
                "Aurora" => PakBinary_Aurora.Instance,
                "HeroEngine" => PakBinary_Myp.Instance,
                _ => throw new ArgumentOutOfRangeException(nameof(game.Engine))
            };

        static (FileOption, Func<BinaryReader, FileSource, PakFile, Task<object>>) ObjectFactoryFactory(FileSource source, FamilyGame game)
            => Path.GetExtension(source.Path).ToLowerInvariant() switch
            {
                ".dds" => (0, Binary_Dds.Factory),
                var x when x == ".dlg" || x == ".qdb" || x == ".qst" => (0, Binary_Gff.Factory),
                _ => (0, null),
            };

        #endregion

        #region Transforms

        bool ITransformFileObject<IUnknownFileModel>.CanTransformFileObject(PakFile transformTo, object source) => UnknownTransform.CanTransformFileObject(this, transformTo, source);
        Task<IUnknownFileModel> ITransformFileObject<IUnknownFileModel>.TransformFileObject(PakFile transformTo, object source) => UnknownTransform.TransformFileObjectAsync(this, transformTo, source);

        #endregion
    }
}