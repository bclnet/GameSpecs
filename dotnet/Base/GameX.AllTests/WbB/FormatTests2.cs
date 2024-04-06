using GameX.WbB.Formats;
using GameX.WbB.Formats.FileTypes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Threading.Tasks;
using Environment = GameX.WbB.Formats.FileTypes.Environment;

namespace GameX.WbB
{
    [TestClass]
    public class FormatTests2
    {
        static readonly Family family = FamilyManager.GetFamily("WbB");
        static readonly PakFile portal = family.OpenPakFile(new Uri("game:/client_portal.dat#AC"));
        static readonly PakFile cell = family.OpenPakFile(new Uri("game:/client_cell_1.dat#AC"));
        static readonly PakFile localEnglish = family.OpenPakFile(new Uri("game:/client_local_English.dat#AC"));

        [DataTestMethod]
        [DataRow("0E000002")]
        [DataRow("0E000003")]
        [DataRow("0E000004")]
        [DataRow("0E00000E")]
        [DataRow("0E00000F")]
        [DataRow("0E000018")]
        [DataRow("0E00001D")]
        [DataRow("0E001001")]
        [DataRow("0E001002")]
        [DataRow("30000000")]
        [DataRow("3000004D")]
        [DataRow("FFFF0001")]
        public void Unknown(string sampleFile) => LoadObject<object>(portal, sampleFile);

        [DataTestMethod]
        [DataRow("Landblock/0000FFFF.land")]
        public void LandBlock(string sampleFile) => LoadObject<Landblock>(cell, sampleFile);

        [DataTestMethod]
        [DataRow("LandblockInfo/00E1FFFE.lbi")]
        public void LandBlockInfo(string sampleFile) => LoadObject<LandblockInfo>(cell, sampleFile);

        [DataTestMethod]
        [DataRow("EnvCell/00010100.cell")]
        public void EnvCell(string sampleFile) => LoadObject<EnvCell>(cell, sampleFile);

        [DataTestMethod]
        [DataRow("GfxObj/01000001.obj")]
        [DataRow("GfxObj/01004E59.obj")]
        public void GraphicsObject(string sampleFile) => LoadObject<GfxObj>(portal, sampleFile);

        [DataTestMethod]
        [DataRow("SetupModel/02000001.set")]
        [DataRow("SetupModel/02001C56.set")]
        public void Setup(string sampleFile) => LoadObject<SetupModel>(portal, sampleFile);

        [DataTestMethod]
        [DataRow("Animation/03000001.anm")]
        [DataRow("Animation/03000E24.anm")]
        public void Animation(string sampleFile) => LoadObject<Animation>(portal, sampleFile);

        [DataTestMethod]
        [DataRow("Palette/0400007E.pal")]
        [DataRow("Palette/04002059.pal")]
        public void Palette(string sampleFile) => LoadObject<Palette>(portal, sampleFile);

        [DataTestMethod]
        [DataRow("SurfaceTexture/0500000C.texture")]
        [DataRow("SurfaceTexture/05003358.texture")]
        public void SurfaceTexture(string sampleFile) => LoadObject<SurfaceTexture>(portal, sampleFile);

        [DataTestMethod]
        [DataRow("Texture/06000133.jpg")]
        [DataRow("Texture/06007576.jpg")]
        public void Texture(string sampleFile) => LoadObject<Texture>(portal, sampleFile);

        [DataTestMethod]
        [DataRow("Surface/08000000.surface")]
        [DataRow("Surface/0800194D.surface")]
        public void Surface(string sampleFile) => LoadObject<Surface>(portal, sampleFile);

        [DataTestMethod]
        [DataRow("MotionTable/09000001.dsc")]
        [DataRow("MotionTable/09000231.dsc")]
        public void MotionTable(string sampleFile) => LoadObject<MotionTable>(portal, sampleFile);

        [DataTestMethod]
        [DataRow("Wave/0A000002.wav")]
        [DataRow("Wave/0A0005B5.wav")]
        public void Wave(string sampleFile) => LoadObject<Wave>(portal, sampleFile);

        [DataTestMethod]
        [DataRow("Environment/0D000002.env")]
        [DataRow("Environment/0D00063F.env")]
        public void Environment(string sampleFile) => LoadObject<Environment>(portal, sampleFile);

        [DataTestMethod]
        [DataRow("ChatPoseTable/0E000007.cps")]
        public void ChatPoseTable(string sampleFile) => LoadObject<ChatPoseTable>(portal, sampleFile);

        [DataTestMethod]
        [DataRow("GeneratorTable/0E00000D.hrc")]
        public void ObjectHierarchy(string sampleFile) => LoadObject<GeneratorTable>(portal, sampleFile);

        [DataTestMethod]
        [DataRow("BadData/0E00001A.bad")]
        public void BadData(string sampleFile) => LoadObject<BadData>(portal, sampleFile);

        [DataTestMethod]
        [DataRow("TabooTable/0E00001E.taboo")]
        public void TabooTable(string sampleFile) => LoadObject<TabooTable>(portal, sampleFile);

        [DataTestMethod]
        [DataRow("0E00001F")] //??
        public void FileToId(string sampleFile) => LoadObject<object>(portal, sampleFile);

        [DataTestMethod]
        [DataRow("NameFilterTable/0E000020.nft")]
        public void NameFilterTable(string sampleFile) => LoadObject<NameFilterTable>(portal, sampleFile);

        [DataTestMethod]
        [DataRow("0E020000.monprop")] //? where?
        public void MonitoredProperties(string sampleFile) => LoadObject<object>(portal, sampleFile);

        [DataTestMethod]
        [DataRow("PaletteSet/0F000001.pst")]
        [DataRow("PaletteSet/0F000B6B.pst")]
        public void PaletteSet(string sampleFile) => LoadObject<PaletteSet>(portal, sampleFile);

        [DataTestMethod]
        [DataRow("ClothingTable/10000001.clo")]
        [DataRow("ClothingTable/1000086C.clo")]
        public void Clothing(string sampleFile) => LoadObject<ClothingTable>(portal, sampleFile);

        [DataTestMethod]
        [DataRow("GfxObjDegradeInfo/11000000.deg")]
        [DataRow("GfxObjDegradeInfo/110010BF.deg")]
        public void DegradeInfo(string sampleFile) => LoadObject<GfxObjDegradeInfo>(portal, sampleFile);

        [DataTestMethod]
        [DataRow("Scene/12000009.scn")]
        [DataRow("Scene/120002C6.scn")]
        public void Scene(string sampleFile) => LoadObject<Scene>(portal, sampleFile);

        [DataTestMethod]
        [DataRow("RegionDesc/13000000.rgn")]
        public void Region(string sampleFile) => LoadObject<RegionDesc>(portal, sampleFile);

        [DataTestMethod]
        [DataRow("14000000.keymap")]
        [DataRow("14000002.keymap")]
        public void KeyMap(string sampleFile) => LoadObject<object>(portal, sampleFile);

        [DataTestMethod]
        [DataRow("RenderTexture/15000000.rtexture")]
        [DataRow("RenderTexture/15000001.rtexture")]
        public void RenderTexture(string sampleFile) => LoadObject<RenderTexture>(portal, sampleFile);

        [DataTestMethod]
        [DataRow("16000000.mat")]
        public void RenderMaterial(string sampleFile) => LoadObject<object>(portal, sampleFile);

        [DataTestMethod]
        [DataRow("17000000.mm")]
        public void MaterialModifier(string sampleFile) => LoadObject<object>(portal, sampleFile);

        [DataTestMethod]
        [DataRow("18000000.mi")]
        public void MaterialInstance(string sampleFile) => LoadObject<object>(portal, sampleFile);

        [DataTestMethod]
        [DataRow("SoundTable/20000001.stb")]
        [DataRow("SoundTable/200000DA.stb")]
        public void SoundTable(string sampleFile) => LoadObject<SoundTable>(portal, sampleFile);

        [DataTestMethod]
        [DataRow("21000000.uil")]
        [DataRow("21000075.uil")]
        public void UILayout(string sampleFile) => LoadObject<object>(localEnglish, sampleFile);

        [DataTestMethod]
        [DataRow("EnumMapper/22000005.emp")] //? where?
        public void EnumMapper(string sampleFile) => LoadObject<EnumMapper>(portal, sampleFile);

        [DataTestMethod]
        [DataRow("StringTable/23000005.stt")]
        [DataRow("StringTable/2300004A.stt")]
        public void StringTable(string sampleFile) => LoadObject<StringTable>(portal, sampleFile);

        [DataTestMethod]
        [DataRow("StringTable/23000001.stt")]
        [DataRow("StringTable/23000010.stt")]
        public void StringTable2(string sampleFile) => LoadObject<StringTable>(localEnglish, sampleFile);

        [DataTestMethod]
        [DataRow("25000000.imp")]
        [DataRow("25000015.imp")]
        public void DidMapper(string sampleFile) => LoadObject<DidMapper>(portal, sampleFile);

        [DataTestMethod]
        [DataRow("26000000.actionmap")]
        public void ActionMap(string sampleFile) => LoadObject<object>(portal, sampleFile);

        [DataTestMethod]
        [DataRow("27000000.dimp")]
        [DataRow("27000004.dimp")]
        public void DualDidMapper(string sampleFile) => LoadObject<DualDidMapper>(portal, sampleFile);

        [DataTestMethod]
        [DataRow("31000001.str")]
        [DataRow("31000025.str")]
        public void String(string sampleFile) => LoadObject<LanguageString>(portal, sampleFile);

        [DataTestMethod]
        [DataRow("32000A83.emt")]
        public void ParticleEmitter(string sampleFile) => LoadObject<ParticleEmitterInfo>(portal, sampleFile);

        [DataTestMethod]
        [DataRow("33000007.pes")]
        [DataRow("3300139F.pes")]
        public void PhysicsScript(string sampleFile) => LoadObject<PhysicsScript>(portal, sampleFile);

        [DataTestMethod]
        [DataRow("34000004.pet")]
        [DataRow("340000D7.pet")]
        public void PhysicsScriptTable(string sampleFile) => LoadObject<PhysicsScriptTable>(portal, sampleFile);

        [DataTestMethod]
        [DataRow("39000001.mpr")]
        public void MasterProperty(string sampleFile) => LoadObject<object>(portal, sampleFile);

        [DataTestMethod]
        [DataRow("40000000.font")]
        [DataRow("40000032.font")]
        public void Font(string sampleFile) => LoadObject<Font>(portal, sampleFile);

        [DataTestMethod]
        [DataRow("LanguageInfo/41000000.sti")]
        public void StringState(string sampleFile) => LoadObject<LanguageInfo>(localEnglish, sampleFile);

        [DataTestMethod]
        [DataRow("78000000.dbpc")]
        [DataRow("78000001.dbpc")]
        public void DbProperties(string sampleFile) => LoadObject<object>(portal, sampleFile);

        static void LoadObject<T>(PakFile source, string sampleFile)
        {
            Assert.IsTrue(source.Contains(sampleFile));
            var result = source.LoadFileObject<T>(sampleFile).Result;
        }
    }
}
