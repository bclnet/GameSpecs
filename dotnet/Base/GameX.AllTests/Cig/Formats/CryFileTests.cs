using GameX.Crytek.Formats;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace GameX.Cig.Formats
{
    [TestClass]
    public class CryFileTests
    {
        [DataTestMethod]
        [DataRow("Cig:StarCitizen", "Data/Objects/animals/fish/CleanerFish_clean_prop_animal_01.chr")]
        //[DataRow("Cig:StarCitizen", "Data/Objects/buildingsets/human/hightech/prop/hydroponic/hydroponic_machine_1_incubator_01x01x02_a.cgf")]
        //[DataRow("Cig:StarCitizen", "Data/Objects/buildingsets/human/hightech/prop/hydroponic/hydroponic_machine_1_incubator_02x01x012_a.cgf")]
        //[DataRow("Cig:StarCitizen", "Data/Objects/buildingsets/human/hightech/prop/hydroponic/hydroponic_machine_1_incubator_rotary_025x01x0225_a/cgf")]
        //[DataRow("Cig:StarCitizen", "Data/Objects/Characters/Human/male_v7/armor/nvy/pilot_flightsuit/m_nvy_pilot_light_armor_helmet_01.skin")]
        public async Task LoadFileObjectAsync(string pak, string sampleFile) => await LoadFileObjectAsync(Helper.Paks[pak].Value, sampleFile);

        public async Task LoadFileObjectAsync(PakFile source, string sampleFile)
        {
            Assert.IsTrue(source.Contains(sampleFile));
            var file = await source.LoadFileObject<CryFile>(sampleFile);
        }
    }
}
