using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace GameX.Formats
{
    [TestClass]
    public class LoadFileDataTests
    {
        [DataTestMethod]
        [DataRow("AC:AC", ">client_highres.dat:Texture/060043BE.tex", 32792)]
        [DataRow("Arkane:AF", ">data.pak:GRAPH/particles/BOOM.jpg", 1923)]
        [DataRow("Arkane:DOM", ">depot_2101_dir.vpk:platform/config/server.vdf", 13)]
        [DataRow("Arkane:D", ">game1.index:strings/english_m.lang", 765258)]
        [DataRow("Arkane:D2", ">game1.index:strings/english_m.lang", 765258)]
        [DataRow("Arkane:P", ">game1.index:strings/english_m.lang", 765258)]
        [DataRow("Arkane:D:DOTO", ">game1.index:strings/english_m.lang", 765258)]
        [DataRow("Arkane:W:YB", ">game1.index:strings/english_m.lang", 765258)]
        [DataRow("Arkane:W:CP", ">game1.index:strings/english_m.lang", 765258)]
        [DataRow("Arkane:DL", ">game1.index:strings/english_m.lang", 765258)]
        //[DataRow("Cry:MWO", ">GameData.pak:GameModeObjects.xml", 153832)]
        //[DataRow("Cyanide:TC", ">Engine_Main_0.cpk:data/engine_0.prefab", 20704)]
        //[DataRow("Origin:UO", ">anim.idx:default_cch.dds", 16520)]
        //[DataRow("Origin:U9", ">activity.flx:Engine/default_cch.dds", 16520)]
        //[DataRow("Rsi:StarCitizen", ">Engine/default_cch.dds", 16520)]
        //[DataRow("Red:Witcher", ">main.key:2da00.bif", 887368)]
        //[DataRow("Red:Witcher2", ">base_scripts.dzip:globals/ch_credits_main.csv", 6716)]
        //[DataRow("Red:Witcher3", ">content0/bundles/xml.bundle:engine/physics/apexclothmaterialpresets.xml", 2512)]
        //[DataRow("Red:Witcher3", ">content0/collision.cache:engine/physics/apexclothmaterialpresets.xml", 2512)]
        //[DataRow("Red:Witcher3", ">content0/dep.cache:engine/physics/apexclothmaterialpresets.xml", 2512)]
        //[DataRow("Red:Witcher3", ">content0/texture.cache:environment/debug/debug-delete.xbm", 2512)]
        //[DataRow("Red:Witcher3", ">content0/texture.cache:environment/skyboxes/textures/clouds_noise_m.xbm", 2512)]
        //[DataRow("Tes:Morrowind", "textures/vfx_poison03.dds", 11040)]
        //[DataRow("Tes:Oblivion", ">Oblivion - Meshes.bsa:trees/treecottonwoodsu.spt", 6329)]
        //[DataRow("Tes:Oblivion", ">Oblivion - Textures - Compressed.bsa:textures/trees/canopyshadow.dds", 174904)]
        //[DataRow("Tes:SkyrimSE", ">Skyrim - Meshes0.bsa:meshes/scalegizmo.nif", 8137)]
        //[DataRow("Tes:SkyrimSE", ">Skyrim - Textures0.bsa:textures/actors/dog/dog.dds", 1398240)]
        //[DataRow("Tes:Fallout4VR", ">Fallout4 - Startup.ba2:Textures/Water/WaterRainRipples.dds", 349680)]
        //[DataRow("Tes:Fallout4VR", ">Fallout4 - Textures8.ba2:Textures/Terrain/DiamondCity/DiamondCity.16.-2.-2.DDS", 174904)]
        //[DataRow("Valve:Dota2", ">dota/pak01_dir.vpk:stringtokendatabase.txt", 35624)]
        public async Task LoadFileData(string pak, string sampleFile, int sampleFileSize)
        {
            var source = TestHelper.Paks[pak].Value;
            Assert.IsTrue(source.Contains(sampleFile));
            Assert.AreEqual(sampleFileSize, (await source.LoadFileData(sampleFile)).Length);
        }
    }
}
