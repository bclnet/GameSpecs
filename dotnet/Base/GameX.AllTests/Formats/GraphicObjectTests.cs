using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace GameX.Formats
{
    [TestClass]
    public class GraphicObjectTests
    {
        [DataTestMethod]
        [DataRow("AC:AC", "client_highres.dat:Texture/060043BE.tex")]
        [DataRow("Cry:MWO", "GameData.pak:GameModeObjects.xml")]
        [DataRow("Rsi:StarCitizen", "Engine/default_cch.dds")]
        [DataRow("Red:Witcher", "main.key:2da00.bif")]
        [DataRow("Red:Witcher2", "base_scripts.dzip:globals/ch_credits_main.csv")]
        [DataRow("Red:Witcher3", "content0/bundles/xml.bundle:engine/physics/apexclothmaterialpresets.xml")]
        [DataRow("Red:Witcher3", "content0/collision.cache:engine/physics/apexclothmaterialpresets.xml")]
        [DataRow("Red:Witcher3", "content0/dep.cache:engine/physics/apexclothmaterialpresets.xml")]
        [DataRow("Tes:Morrowind", "meshes/x/ex_common_balcony_01.nif")]
        [DataRow("Tes:Oblivion", "Oblivion - Meshes.bsa:trees/treecottonwoodsu.spt")]
        [DataRow("Tes:Oblivion", "Oblivion - Textures - Compressed.bsa:textures/trees/canopyshadow.dds")]
        [DataRow("Tes:SkyrimSE", "Skyrim - Meshes0.bsa:meshes/scalegizmo.nif")]
        [DataRow("Tes:SkyrimSE", "Skyrim - Textures0.bsa:textures/actors/dog/dog.dds")]
        [DataRow("Tes:Fallout4VR", "Fallout4 - Startup.ba2:Textures/Water/WaterRainRipples.dds")]
        [DataRow("Tes:Fallout4VR", "Fallout4 - Textures8.ba2:Textures/Terrain/DiamondCity/DiamondCity.16.-2.-2.DDS")]
        [DataRow("Origin:UltimaOnline", "anim.idx:Engine/default_cch.dds")]
        [DataRow("Origin:UltimaIX", "static/activity.flx:Engine/default_cch.dds")]
        [DataRow("Valve:Dota2", "dota/pak01_dir.vpk:stringtokendatabase.txt")]
        public async Task GraphicObject(string pak, string sampleFile)
        {
            var source = TestHelper.Paks[pak].Value;
            Assert.IsTrue(source.Contains(sampleFile));
            var obj0 = await source.LoadFileObject<object>(sampleFile);
            Assert.IsNotNull(obj0);
            //Assert.Equal(sampleFileSize, pakFile.GetLoadFileDataAsync(sampleFile).Result.Length);
        }
    }
}
