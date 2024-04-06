using GameX.Crytek.Formats;
using GameX.Crytek.Transforms;
using GameX.Formats;
using GameX.Formats.Unknown;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;

namespace GameX.Crytek
{
    /// <summary>
    /// CrytekPakFile
    /// </summary>
    /// <seealso cref="GameX.Formats.BinaryPakFile" />
    public class CrytekPakFile : BinaryPakFile, ITransformFileObject<IUnknownFileModel>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CrytekPakFile" /> class.
        /// </summary>
        /// <param name="state">The state.</param>
        public CrytekPakFile(PakState state) : base(state, GetPakBinary(state.Game))
        {
            ObjectFactoryFactoryMethod = ObjectFactoryFactory;
        }

        #region Factories

        static readonly ConcurrentDictionary<string, PakBinary> PakBinarys = new ConcurrentDictionary<string, PakBinary>();

        static PakBinary GetPakBinary(FamilyGame game)
            => PakBinarys.GetOrAdd(game.Id, _ => PakBinaryFactory(game));

        static PakBinary PakBinaryFactory(FamilyGame game)
            => game.Engine switch
            {
                "ArcheAge" => new PakBinary_ArcheAge((byte[])game.Key),
                _ => new PakBinary_Cry3((byte[])game.Key),
            };

        public static (FileOption, Func<BinaryReader, FileSource, PakFile, Task<object>>) ObjectFactoryFactory(FileSource source, FamilyGame game)
            => Path.GetExtension(source.Path).ToLowerInvariant() switch
            {
                ".xml" => (0, CryXmlFile.Factory),
                ".dds" => (0, Binary_Dds.Factory),
                var x when x == ".cgf" || x == ".cga" || x == ".chr" || x == ".skin" || x == ".anim" => (0, CryFile.Factory),
                _ => (0, null),
            };

        #endregion

        #region Transforms

        bool ITransformFileObject<IUnknownFileModel>.CanTransformFileObject(PakFile transformTo, object source) => UnknownTransform.CanTransformFileObject(this, transformTo, source);
        Task<IUnknownFileModel> ITransformFileObject<IUnknownFileModel>.TransformFileObject(PakFile transformTo, object source) => UnknownTransform.TransformFileObjectAsync(this, transformTo, source);

        #endregion
    }
}