using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace GameEstate.Formats
{
    [TestClass]
    public class LoadFileDataTests
    {
        [DataTestMethod]
        [DataRow("AC:AC:1", "Texture060043BE", 32792)]
        [DataRow("Arkane:Dishonored2:1", "strings/english_m.lang", 765258)]
        [DataRow("Cry:MWO:1", "GameModeObjects.xml", 153832)]
        [DataRow("Cyanide:TheCouncil:1", "data/engine_0.prefab", 20704)]
        [DataRow("Origin:UltimaOnline:1", "Engine/default_cch.dds", 16520)]
        [DataRow("Origin:UltimaIX:1", "Engine/default_cch.dds", 16520)]
        [DataRow("Rsi:StarCitizen", "Engine/default_cch.dds", 16520)]
        [DataRow("Red:Witcher:1", "2da00.bif", 887368)]
        [DataRow("Red:Witcher2:1", "globals/ch_credits_main.csv", 6716)]
        [DataRow("Red:Witcher3:1", "engine/physics/apexclothmaterialpresets.xml", 2512)]
        [DataRow("Red:Witcher3:2", "engine/physics/apexclothmaterialpresets.xml", 2512)]
        [DataRow("Red:game:/content0/dep.cache#Witcher3:3", "engine/physics/apexclothmaterialpresets.xml", 2512)]
        [DataRow("Red:Witcher3:4", "environment/debug/debug-delete.xbm", 2512)]
        [DataRow("Red:Witcher3:4", "environment/skyboxes/textures/clouds_noise_m.xbm", 2512)]
        [DataRow("Tes:Morrowind", "textures/vfx_poison03.dds", 11040)]
        [DataRow("Tes:Oblivion:1", "trees/treecottonwoodsu.spt", 6329)]
        [DataRow("Tes:Oblivion:2", "textures/trees/canopyshadow.dds", 174904)]
        [DataRow("Tes:SkyrimSE:1", "meshes/scalegizmo.nif", 8137)]
        [DataRow("Tes:SkyrimSE:2", "textures/actors/dog/dog.dds", 1398240)]
        [DataRow("Tes:Fallout4VR:1", "Textures/Water/WaterRainRipples.dds", 349680)]
        [DataRow("Tes:Fallout4VR:2", "Textures/Terrain/DiamondCity/DiamondCity.16.-2.-2.DDS", 174904)]
        [DataRow("Valve:Dota2:1", "stringtokendatabase.txt", 35624)]
        public async Task LoadFileData(string pak, string sampleFile, int sampleFileSize) => await LoadFileDataAysc(TestHelper.Paks[pak].Value, sampleFile, sampleFileSize);

        static async Task LoadFileDataAysc(EstatePakFile source, string sampleFile, int sampleFileSize)
        {
            Assert.IsTrue(source.Contains(sampleFile));
            Assert.AreEqual(sampleFileSize, (await source.LoadFileDataAsync(sampleFile)).Length);
        }
    }
}
