using GameX.Formats;
using GameX.Formats.Unknown;
using GameX.WbB.Formats;
using GameX.WbB.Formats.FileTypes;
using GameX.WbB.Transforms;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Environment = GameX.WbB.Formats.FileTypes.Environment;

namespace GameX.WbB
{
    /// <summary>
    /// WbBPakFile
    /// </summary>
    /// <seealso cref="GameEstate.Formats.BinaryPakFile" />
    public class WbBPakFile : BinaryPakFile, ITransformFileObject<IUnknownFileModel>
    {
        static WbBPakFile() => Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        /// <summary>
        /// Initializes a new instance of the <see cref="WbBPakFile" /> class.
        /// </summary>
        /// <param name="state">The state.</param>
        public WbBPakFile(PakState state) : base(state, PakBinary_AC.Instance)
        {
            ObjectFactoryFactoryMethod = ObjectFactoryFactory;
            UseFileId = true;
        }

        #region Factories

        internal static string GetPath(FileSource source, BinaryReader r, PakType pakType, out PakFileType? fileType)
        {
            if ((uint)source.Id == Iteration.FILE_ID) { fileType = null; return "Iteration"; }
            var (type, ext) = GetFileType(source, pakType);
            if (type == 0) { fileType = null; return $"{source.Id:X8}"; }
            fileType = type;
            return ext switch
            {
                null => $"{fileType}/{source.Id:X8}",
                string extension => $"{fileType}/{source.Id:X8}.{extension}",
                Func<FileSource, BinaryReader, string> func => $"{fileType}/{source.Id:X8}.{func(source, r)}",
                _ => throw new ArgumentOutOfRangeException(nameof(ext), ext.ToString()),
            };
        }

        static (FileOption, Func<BinaryReader, FileSource, PakFile, Task<object>>) ObjectFactoryFactory(FileSource source, FamilyGame game)
        {
            var (pakType, type) = ((PakType, PakFileType?))source.ExtraArgs;
            if ((uint)source.Id == Iteration.FILE_ID) return (0, (r, m, s) => Task.FromResult((object)new Iteration(r)));
            else if (type == null) return (0, null);
            else return type.Value switch
            {
                PakFileType.LandBlock => (0, (r, m, s) => Task.FromResult((object)new Landblock(r))),
                PakFileType.LandBlockInfo => (0, (r, m, s) => Task.FromResult((object)new LandblockInfo(r))),
                PakFileType.EnvCell => (0, (r, m, s) => Task.FromResult((object)new EnvCell(r))),
                //PakFileType.LandBlockObjects => (0, null),
                //PakFileType.Instantiation => (0, null),
                PakFileType.GraphicsObject => (0, (r, m, s) => Task.FromResult((object)new GfxObj(r))),
                PakFileType.Setup => (0, (r, m, s) => Task.FromResult((object)new SetupModel(r))),
                PakFileType.Animation => (0, (r, m, s) => Task.FromResult((object)new Animation(r))),
                //PakFileType.AnimationHook => (0, null),
                PakFileType.Palette => (0, (r, m, s) => Task.FromResult((object)new Palette(r))),
                PakFileType.SurfaceTexture => (0, (r, m, s) => Task.FromResult((object)new SurfaceTexture(r))),
                PakFileType.Texture => (0, (r, m, s) => Task.FromResult((object)new Texture(r, game))),
                PakFileType.Surface => (0, (r, m, s) => Task.FromResult((object)new Surface(r))),
                PakFileType.MotionTable => (0, (r, m, s) => Task.FromResult((object)new MotionTable(r))),
                PakFileType.Wave => (0, (r, m, s) => Task.FromResult((object)new Wave(r))),
                PakFileType.Environment => (0, (r, m, s) => Task.FromResult((object)new Environment(r))),
                PakFileType.ChatPoseTable => (0, (r, m, s) => Task.FromResult((object)new ChatPoseTable(r))),
                PakFileType.ObjectHierarchy => (0, (r, m, s) => Task.FromResult((object)new GeneratorTable(r))), //: Name wayoff
                PakFileType.BadData => (0, (r, m, s) => Task.FromResult((object)new BadData(r))),
                PakFileType.TabooTable => (0, (r, m, s) => Task.FromResult((object)new TabooTable(r))),
                PakFileType.FileToId => (0, null),
                PakFileType.NameFilterTable => (0, (r, m, s) => Task.FromResult((object)new NameFilterTable(r))),
                PakFileType.MonitoredProperties => (0, null),
                PakFileType.PaletteSet => (0, (r, m, s) => Task.FromResult((object)new PaletteSet(r))),
                PakFileType.Clothing => (0, (r, m, s) => Task.FromResult((object)new ClothingTable(r))),
                PakFileType.DegradeInfo => (0, (r, m, s) => Task.FromResult((object)new GfxObjDegradeInfo(r))),
                PakFileType.Scene => (0, (r, m, s) => Task.FromResult((object)new Scene(r))),
                PakFileType.Region => (0, (r, m, s) => Task.FromResult((object)new RegionDesc(r))),
                PakFileType.KeyMap => (0, null),
                PakFileType.RenderTexture => (0, (r, m, s) => Task.FromResult((object)new RenderTexture(r))),
                PakFileType.RenderMaterial => (0, null),
                PakFileType.MaterialModifier => (0, null),
                PakFileType.MaterialInstance => (0, null),
                PakFileType.SoundTable => (0, (r, m, s) => Task.FromResult((object)new SoundTable(r))),
                PakFileType.UILayout => (0, null),
                PakFileType.EnumMapper => (0, (r, m, s) => Task.FromResult((object)new EnumMapper(r))),
                PakFileType.StringTable => (0, (r, m, s) => Task.FromResult((object)new StringTable(r))),
                PakFileType.DidMapper => (0, (r, m, s) => Task.FromResult((object)new DidMapper(r))),
                PakFileType.ActionMap => (0, null),
                PakFileType.DualDidMapper => (0, (r, m, s) => Task.FromResult((object)new DualDidMapper(r))),
                PakFileType.String => (0, (r, m, s) => Task.FromResult((object)new LanguageString(r))), //: Name wayoff
                PakFileType.ParticleEmitter => (0, (r, m, s) => Task.FromResult((object)new ParticleEmitterInfo(r))),
                PakFileType.PhysicsScript => (0, (r, m, s) => Task.FromResult((object)new PhysicsScript(r))),
                PakFileType.PhysicsScriptTable => (0, (r, m, s) => Task.FromResult((object)new PhysicsScriptTable(r))),
                PakFileType.MasterProperty => (0, null),
                PakFileType.Font => (0, (r, m, s) => Task.FromResult((object)new Font(r))),
                PakFileType.FontLocal => (0, null),
                PakFileType.StringState => (0, (r, m, s) => Task.FromResult((object)new LanguageInfo(r))), //: Name wayoff
                PakFileType.DbProperties => (0, null),
                PakFileType.RenderMesh => (0, null),
                PakFileType.WeenieDefaults => (0, null),
                PakFileType.CharacterGenerator => (0, (r, m, s) => Task.FromResult((object)new CharGen(r))),
                PakFileType.SecondaryAttributeTable => (0, (r, m, s) => Task.FromResult((object)new SecondaryAttributeTable(r))),
                PakFileType.SkillTable => (0, (r, m, s) => Task.FromResult((object)new SkillTable(r))),
                PakFileType.SpellTable => (0, (r, m, s) => Task.FromResult((object)new SpellTable(r))),
                PakFileType.SpellComponentTable => (0, (r, m, s) => Task.FromResult((object)new SpellComponentTable(r))),
                PakFileType.TreasureTable => (0, null),
                PakFileType.CraftTable => (0, null),
                PakFileType.XpTable => (0, (r, m, s) => Task.FromResult((object)new XpTable(r))),
                PakFileType.Quests => (0, null),
                PakFileType.GameEventTable => (0, null),
                PakFileType.QualityFilter => (0, (r, m, s) => Task.FromResult((object)new QualityFilter(r))),
                PakFileType.CombatTable => (0, (r, m, s) => Task.FromResult((object)new CombatManeuverTable(r))),
                PakFileType.ItemMutation => (0, null),
                PakFileType.ContractTable => (0, (r, m, s) => Task.FromResult((object)new ContractTable(r))),
                _ => (0, null),
            };
        }

        public static (PakFileType fileType, object ext) GetFileType(FileSource source, PakType pakType)
        {
            var objectId = (uint)source.Id;
            if (pakType == PakType.Cell)
            {
                if ((objectId & 0xFFFF) == 0xFFFF) return (PakFileType.LandBlock, "land");
                else if ((objectId & 0xFFFF) == 0xFFFE) return (PakFileType.LandBlockInfo, "lbi");
                else return (PakFileType.EnvCell, "cell");
            }
            else if (pakType == PakType.Portal)
            {
                switch (objectId >> 24)
                {
                    case 0x01: return (PakFileType.GraphicsObject, "obj");
                    case 0x02: return (PakFileType.Setup, "set");
                    case 0x03: return (PakFileType.Animation, "anm");
                    case 0x04: return (PakFileType.Palette, "pal");
                    case 0x05: return (PakFileType.SurfaceTexture, "texture");
                    case 0x06: return (PakFileType.Texture, "tex"); // new PakFileExtensionAttribute(typeof(FormatExtensions), "TextureExtensionLookup").Value);
                    case 0x08: return (PakFileType.Surface, "surface");
                    case 0x09: return (PakFileType.MotionTable, "dsc");
                    case 0x0A: return (PakFileType.Wave, "wav");
                    case 0x0D: return (PakFileType.Environment, "env");
                    case 0x0F: return (PakFileType.PaletteSet, "pst");
                    case 0x10: return (PakFileType.Clothing, "clo");
                    case 0x11: return (PakFileType.DegradeInfo, "deg");
                    case 0x12: return (PakFileType.Scene, "scn");
                    case 0x13: return (PakFileType.Region, "rgn");
                    case 0x14: return (PakFileType.KeyMap, "keymap");
                    case 0x15: return (PakFileType.RenderTexture, "rtexture");
                    case 0x16: return (PakFileType.RenderMaterial, "mat");
                    case 0x17: return (PakFileType.MaterialModifier, "mm");
                    case 0x18: return (PakFileType.MaterialInstance, "mi");
                    case 0x20: return (PakFileType.SoundTable, "stb");
                    case 0x22: return (PakFileType.EnumMapper, "emp");
                    case 0x25: return (PakFileType.DidMapper, "imp");
                    case 0x26: return (PakFileType.ActionMap, "actionmap");
                    case 0x27: return (PakFileType.DualDidMapper, "dimp");
                    case 0x30: return (PakFileType.CombatTable, null);
                    case 0x31: return (PakFileType.String, "str");
                    case 0x32: return (PakFileType.ParticleEmitter, "emt");
                    case 0x33: return (PakFileType.PhysicsScript, "pes");
                    case 0x34: return (PakFileType.PhysicsScriptTable, "pet");
                    case 0x39: return (PakFileType.MasterProperty, "mpr");
                    case 0x40: return (PakFileType.Font, "font");
                    case 0x78: return (PakFileType.DbProperties, new PakFileExtensionAttribute(typeof(WbBPakFile), "DbPropertyExtensionLookup").Value);
                }
                switch (objectId >> 16)
                {
                    case 0x0E01: return (PakFileType.QualityFilter, null);
                    case 0x0E02: return (PakFileType.MonitoredProperties, "monprop");
                }
                if (objectId == 0x0E000002) return (PakFileType.CharacterGenerator, null);
                else if (objectId == 0x0E000003) return (PakFileType.SecondaryAttributeTable, null);
                else if (objectId == 0x0E000004) return (PakFileType.SkillTable, null);
                else if (objectId == 0x0E000007) return (PakFileType.ChatPoseTable, "cps");
                else if (objectId == 0x0E00000D) return (PakFileType.ObjectHierarchy, "hrc");
                else if (objectId == 0x0E00000E) return (PakFileType.SpellTable, "cps");
                else if (objectId == 0x0E00000F) return (PakFileType.SpellComponentTable, "cps");
                else if (objectId == 0x0E000018) return (PakFileType.XpTable, "cps");
                else if (objectId == 0xE00001A) return (PakFileType.BadData, "bad");
                else if (objectId == 0x0E00001D) return (PakFileType.ContractTable, null);
                else if (objectId == 0x0E00001E) return (PakFileType.TabooTable, "taboo");
                else if (objectId == 0x0E00001F) return (PakFileType.FileToId, null);
                else if (objectId == 0x0E000020) return (PakFileType.NameFilterTable, "nft");
            }
            if (pakType == PakType.Language)
                switch (objectId >> 24)
                {
                    case 0x21: return (PakFileType.UILayout, null);
                    case 0x23: return (PakFileType.StringTable, null);
                    case 0x41: return (PakFileType.StringState, null);
                }
            Console.WriteLine($"Unknown file type: {objectId:X8}");
            return (0, null);
        }

        static string DbPropertyExtensionLookup(FileSource source, BinaryReader r)
            => 0 switch
            {
                0 => "dbpc",
                1 => "pmat",
                _ => throw new ArgumentOutOfRangeException(),
            };

        #endregion

        #region Transforms

        bool ITransformFileObject<IUnknownFileModel>.CanTransformFileObject(PakFile transformTo, object source) => UnknownTransform.CanTransformFileObject(this, transformTo, source);
        Task<IUnknownFileModel> ITransformFileObject<IUnknownFileModel>.TransformFileObject(PakFile transformTo, object source) => UnknownTransform.TransformFileObjectAsync(this, transformTo, source);

        #endregion
    }
}