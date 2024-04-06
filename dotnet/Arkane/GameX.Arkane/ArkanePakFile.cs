using GameX.Arkane.Formats;
using GameX.Arkane.Formats.Danae;
using GameX.Arkane.Transforms;
using GameX.Formats;
using GameX.Formats.Unknown;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;

namespace GameX.Arkane
{
    /// <summary>
    /// ArkanePakFile
    /// </summary>
    /// <seealso cref="GameEstate.Formats.BinaryPakFile" />
    public class ArkanePakFile : BinaryPakFile, ITransformFileObject<IUnknownFileModel>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ArkanePakFile" /> class.
        /// </summary>
        /// <param name="state">The state.</param>
        public ArkanePakFile(PakState state) : base(state, GetPakBinary(state.Game, Path.GetExtension(state.Path).ToLowerInvariant()))
        {
            ObjectFactoryFactoryMethod = state.Game.Engine switch
            {
                "CryEngine" => Crytek.CrytekPakFile.ObjectFactoryFactory,
                "Unreal" => Epic.EpicPakFile.ObjectFactoryFactory,
                "Valve" => Valve.ValvePakFile.ObjectFactoryFactory,
                "idTech7" => Id.IdPakFile.ObjectFactoryFactory,
                _ => ObjectFactoryFactory,
            };
            UseFileId = true;
        }

        #region Factories

        static readonly ConcurrentDictionary<string, PakBinary> PakBinarys = new ConcurrentDictionary<string, PakBinary>();

        static PakBinary GetPakBinary(FamilyGame game, string extension)
            => PakBinarys.GetOrAdd(game.Id, _ => game.Engine switch
            {
                "Danae" => PakBinary_Danae.Instance,
                "Void" => PakBinary_Void.Instance,
                "CryEngine" => Crytek.Formats.PakBinary_Cry3.Instance,
                "Unreal" => Epic.Formats.PakBinary_Pck.Instance,
                "Valve" => Valve.Formats.PakBinary_Vpk.Instance,
                //"idTech7" => Id.Formats.PakBinaryVpk.Instance,
                _ => throw new ArgumentOutOfRangeException(nameof(game.Engine)),
            });

        internal static (FileOption, Func<BinaryReader, FileSource, PakFile, Task<object>>) ObjectFactoryFactory(FileSource source, FamilyGame game)
            => Path.GetExtension(source.Path).ToLowerInvariant() switch
            {
                var x when x == ".txt" || x == ".ini" || x == ".asl" => (0, Binary_Txt.Factory),
                ".wav" => (0, Binary_Snd.Factory),
                var x when x == ".bmp" || x == ".jpg" || x == ".tga" => (0, Binary_Img.Factory),
                ".dds" => (0, Binary_Dds.Factory),
                // Danae (AF)
                ".ftl" => (0, Binary_Ftl.Factory),
                ".fts" => (0, Binary_Fts.Factory),
                ".tea" => (0, Binary_Tea.Factory),
                //
                //".llf" => (0, Binary_Flt.Factory),
                //".dlf" => (0, Binary_Flt.Factory),
                _ => (0, null),
            };

        #endregion

        #region Transforms

        bool ITransformFileObject<IUnknownFileModel>.CanTransformFileObject(PakFile transformTo, object source) => UnknownTransform.CanTransformFileObject(this, transformTo, source);
        Task<IUnknownFileModel> ITransformFileObject<IUnknownFileModel>.TransformFileObject(PakFile transformTo, object source) => UnknownTransform.TransformFileObjectAsync(this, transformTo, source);

        #endregion
    }
}