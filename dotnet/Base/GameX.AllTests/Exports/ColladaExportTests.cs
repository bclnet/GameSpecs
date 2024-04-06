using GameX.Formats.Collada;
using GameX.Formats.Unknown;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace GameX.Exports
{
    [TestClass]
    public class ColladaExportTests
    {
        [DataTestMethod]
        //[DataRow("Rsi:StarCitizen", "Objects/animals/fish/CleanerFish_clean_prop_animal_01.chr")]
        [DataRow("Rsi:StarCitizen", "Objects/buildingsets/human/hightech/prop/hydroponic/hydroponic_machine_1_incubator_01x01x02_a.cgf")]
        //[DataRow("Rsi:StarCitizen", "Objects/buildingsets/human/hightech/prop/hydroponic/hydroponic_machine_1_incubator_02x01x012_a.cgf")]
        //[DataRow("Rsi:StarCitizen", "Objects/buildingsets/human/hightech/prop/hydroponic/hydroponic_machine_1_incubator_rotary_025x01x0225_a.cga")]
        //[DataRow("Rsi:StarCitizen", "Objects/buildingsets/human/hightech/prop/hydroponic/hydroponic_machine_1_incubator_rotary_025x01x0225_a.cgf")]
        public async Task ExportFileObjectAsync(string pak, string sampleFile) => await ExportFileObjectAsync(TestHelper.Paks[pak].Value, sampleFile);

        public async Task ExportFileObjectAsync(PakFile source, string sampleFile)
        {
            Assert.IsTrue(source.Contains(sampleFile));
            var file = await source.LoadFileObject<IUnknownFileModel>(sampleFile, FamilyManager.UnknownPakFile);
            var objFile = new ColladaFileWriter(file);
            objFile.Write(@"C:\T_\Models", false);
        }
    }
}
