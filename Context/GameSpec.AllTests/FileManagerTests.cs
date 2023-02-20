using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace GameSpec
{
    [TestClass]
    public class FileManagerTests
    {
        [TestMethod]
        public void MissingEstate_ShouldThrow()
            => Assert.ThrowsException<ArgumentOutOfRangeException>(() => FamilyManager.GetFamily("Missing"));

        [DataTestMethod]
        [DataRow("AC", "AC", "client_cell_1.dat")]
        [DataRow("Cry", "MWO", "GameData.pak")]
        [DataRow("Red", "Witcher", "2da00.bif")]
        [DataRow("Red", "Witcher2", "tutorial.dzip")]
        [DataRow("Red", "Witcher3", "metadata.store")]
        [DataRow("Rsi", "StarCitizen", "Data.p4k")]
        [DataRow("Tes", "Fallout4VR", "Fallout4 - *.ba2")]
        [DataRow("Tes", "Fallout4VR", "Fallout4 - Startup.ba2")]
        [DataRow("Origin", "UltimaOnline", "*.idx")]
        [DataRow("Origin", "UltimaOnline", "anim.idx")]
        [DataRow("Origin", "UltimaIX", "static/*.flx")]
        [DataRow("Origin", "UltimaIX", "static/activity.flx")]
        [DataRow("Valve", "Dota2", "core/pak01_dir.vpk")]
        public void FileManager(string familyName, string game, string searchPattern)
        {
            var family = FamilyManager.GetFamily(familyName);
            var fileManager = family.FileManager;
            Assert.IsTrue(fileManager.HasPaths);
            var abc0 = fileManager.FindGameFilePaths(family, null, family.GetGame(game), searchPattern);
            Assert.AreEqual(1, abc0.Length);
        }
    }
}
