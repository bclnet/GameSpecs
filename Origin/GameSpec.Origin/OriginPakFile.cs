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
                "UO" => source.Path.ToLowerInvariant() switch
                {
                    "animdata.mul" => (0, Binary_Animdata.Factory),
                    "fonts.mul" => (0, Binary_AsciiFont.Factory),
                    "bodyconv.def" => (0, Binary_BodyConverter.Factory),
                    "body.def" => (0, Binary_BodyTable.Factory),
                    "calibration.cfg" => (0, Binary_CalibrationInfo.Factory),
                    "gump.def" => (0, Binary_GumpDef.Factory),
                    "hues.mul" => (0, Binary_Hues.Factory),
                    "mobtypes.txt" => (0, Binary_MobType.Factory),
                    var x when x == "multimap.rle" || x.StartsWith("facet") => (0, Binary_MultiMap.Factory),
                    "music/digital/config.txt" => (0, Binary_MusicDef.Factory),
                    "radarcol.mul" => (0, Binary_RadarColor.Factory),
                    "skillgrp.mul" => (0, Binary_SkillGroups.Factory),
                    "speech.mul" => (0, Binary_SpeechList.Factory),
                    "tiledata.mul" => (0, Binary_TileData.Factory),
                    var x when x.StartsWith("cliloc") => (0, Binary_StringTable.Factory),
                    "verdata.mul" => (0, Binary_Verdata.Factory),
                    // server
                    "data/containers.cfg" => (0, ServerBinary_Container.Factory),
                    "data/bodytable.cfg" => (0, ServerBinary_BodyTable.Factory),
                    _ => Path.GetExtension(source.Path).ToLowerInvariant() switch
                    {
                        ".anim" => (0, Binary_Anim.Factory),
                        ".tex" => (0, Binary_Gump.Factory),
                        ".land" => (0, Binary_Land.Factory),
                        ".light" => (0, Binary_Light.Factory),
                        ".art" => (0, Binary_Static.Factory),
                        ".multi" => (0, Binary_Multi.Factory),
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