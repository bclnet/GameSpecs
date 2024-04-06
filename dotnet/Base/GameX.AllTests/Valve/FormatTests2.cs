using GameX.Valve.Formats;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace GameX.Valve
{
    [TestClass]
    public class FormatTests2
    {
        static readonly Family family = FamilyManager.GetFamily("Valve");
        static readonly PakFile dota2 = family.OpenPakFile(new Uri("game:/dota/pak01_dir.vpk#Dota2"));

        [DataTestMethod]
        [DataRow("materials/models/courier/frog/frog_color_psd_15017e0b.vtex_c")]
        [DataRow("materials/models/courier/frog/frog_normal_psd_a5b783cb.vtex_c")]
        [DataRow("materials/models/courier/frog/frog_specmask_tga_a889a311.vtex_c")]
        public void AGRP(string sampleFile) => LoadObject<Binary_Pak>(dota2, sampleFile);

        [DataTestMethod]
        [DataRow("materials/models/courier/frog/frog_color_psd_15017e0b.vtex_c")]
        [DataRow("materials/models/courier/frog/frog_normal_psd_a5b783cb.vtex_c")]
        [DataRow("materials/models/courier/frog/frog_specmask_tga_a889a311.vtex_c")]
        public void DATATexture(string sampleFile) => LoadObject<Binary_Pak>(dota2, sampleFile);

        [DataTestMethod]
        [DataRow("materials/models/courier/frog/frog.vmat_c")]
        [DataRow("materials/vgui/800corner.vmat_c")]
        public void DATAMaterial(string sampleFile) => LoadObject<Binary_Pak>(dota2, sampleFile);

        static void LoadObject<T>(PakFile source, string sampleFile)
        {
            Assert.IsTrue(source.Contains(sampleFile));
            var result = source.LoadFileObject<T>(sampleFile).Result;
        }
    }
}
