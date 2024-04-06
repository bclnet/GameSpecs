using GameX.Capcom.Formats;
using GameX.Capcom.Transforms;
using GameX.Formats;
using GameX.Formats.Unknown;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;

namespace GameX.Capcom
{
    /// <summary>
    /// CapcomPakFile
    /// </summary>
    /// <seealso cref="GameX.Formats.BinaryPakFile" />
    public class CapcomPakFile : BinaryPakFile, ITransformFileObject<IUnknownFileModel>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CapcomPakFile" /> class.
        /// </summary>
        /// <param name="state">The state.</param>
        public CapcomPakFile(PakState state) : base(state, GetPakBinary(state.Game, Path.GetExtension(state.Path).ToLowerInvariant()))
        {
            ObjectFactoryFactoryMethod = state.Game.Engine switch
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