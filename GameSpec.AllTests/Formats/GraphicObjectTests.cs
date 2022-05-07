using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace GameEstate.Formats
{
    [TestClass]
    public class GraphicObjectTests
    {
        [DataTestMethod]
        [DataRow("AC:AC:1", "Texture060043BE")]
        [DataRow("Cry:MWO:1", "GameModeObjects.xml")]
        [DataRow("Rsi:StarCitizen", "Engine/default_cch.dds")]
        [DataRow("Red:Witcher:1", "2da00.bif")]
        [DataRow("Red:Witcher2:1", "globals/ch_credits_main.csv")]
        [DataRow("Red:Witcher3:1", "engine/physics/apexclothmaterialpresets.xml")]
        [DataRow("Red:Witcher3:2", "engine/physics/apexclothmaterialpresets.xml")]
        [DataRow("Red:Witcher3:3", "engine/physics/apexclothmaterialpresets.xml")]
        [DataRow("Tes:Morrowind", "meshes/x/ex_common_balcony_01.nif")]
        [DataRow("Tes:Oblivion:1", "trees/treecottonwoodsu.spt")]
        [DataRow("Tes:Oblivion:2", "textures/trees/canopyshadow.dds")]
        [DataRow("Tes:SkyrimSE:1", "meshes/scalegizmo.nif")]
        [DataRow("Tes:SkyrimSE:2", "textures/actors/dog/dog.dds")]
        [DataRow("Tes:Fallout4VR:1", "Textures/Water/WaterRainRipples.dds")]
        [DataRow("Tes:Fallout4VR:2", "Textures/Terrain/DiamondCity/DiamondCity.16.-2.-2.DDS")]
        [DataRow("Origin:UltimaOnline:1", "Engine/default_cch.dds")]
        [DataRow("Origin:UltimaIX:1", "Engine/default_cch.dds")]
        [DataRow("Valve:Dota2:1", "stringtokendatabase.txt")]
        public async Task GraphicObject(string pak, string sampleFile) => await LoadGraphicObjectAsync(TestHelper.Paks[pak].Value, sampleFile);

        static async Task LoadGraphicObjectAsync(EstatePakFile source, string sampleFile)
        {
            Assert.IsTrue(source.Contains(sampleFile));
            var obj0 = await source.LoadFileObjectAsync<object>(sampleFile);
            Assert.IsNotNull(obj0);
            //Assert.Equal(sampleFileSize, pakFile.GetLoadFileDataAsync(sampleFile).Result.Length);
        }
    }
}
