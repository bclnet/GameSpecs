using GameSpec.Formats;
using GameSpec.Formats.Unknown;
using GameSpec.Origin.Formats;
using GameSpec.Origin.Formats.UO;
using GameSpec.Origin.Transforms;
using System;
using System.IO;
using System.Threading.Tasks;

namespace GameSpec.Origin
{
    /// <summary>
    /// OriginPakFile
    /// </summary>
    /// <seealso cref="GameSpec.Formats.BinaryPakFile" />
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
                "U9" => PakBinary_U9.Instance,
                "UO" => PakBinary_UO.Instance,
                _ => throw new ArgumentOutOfRangeException(),
            };

        static (FileOption, Func<BinaryReader, FileSource, PakFile, Task<object>>) ObjectFactoryFactory(FileSource source, FamilyGame game)
            => game.Id switch
            {
                "UO" => Path.GetFileName(source.Path).ToLowerInvariant() switch
                {
                    "body.def" => (0, Binary_Body.Factory),
                    "bodyconv.def" => (0, Binary_BodyConv.Factory),
                    "bodytable.cfg" => (0, Binary_BodyTable.Factory),
                    var x when x.StartsWith("cliloc") => (0, Binary_Cliloc.Factory),
                    "containers.cfg" => (0, Binary_Container.Factory),
                    "animdata.mul" => (0, Binary_Effect.Factory),
                    "fonts.mul" => (0, Binary_Font.Factory), // includes unifont?.mul
                    "gump.def" => (0, Binary_Gump.Factory), // includes unifont?.mul
                    //
                    "verdata.mul" => (0, Binary_Verdata.Factory),
                    _ => Path.GetExtension(source.Path).ToLowerInvariant() switch
                    {
                        //".mul" => (0, Binary_Ignore.Factory($"refer to {source.Path[..^4]}.idx")),
                        _ => (0, null),
                    }
                },
                _ => Path.GetExtension(source.Path).ToLowerInvariant() switch
                {
                    ".dds" => (0, Binary_Dds.Factory),
                    _ => (0, null),
                }
            };

        #endregion

        #region Transforms

        bool ITransformFileObject<IUnknownFileModel>.CanTransformFileObject(PakFile transformTo, object source) => UnknownTransform.CanTransformFileObject(this, transformTo, source);
        Task<IUnknownFileModel> ITransformFileObject<IUnknownFileModel>.TransformFileObject(PakFile transformTo, object source) => UnknownTransform.TransformFileObjectAsync(this, transformTo, source);

        #endregion
    }
}