//#define HTTPTEST

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using static GameX.FamilyManager;

namespace GameX
{
    [TestClass]
    public class ResourceTests
    {
        const string GAME = "game:";
        const string FILE_Oblivion = "file:///G:/SteamLibrary/steamapps/common/Oblivion";
        const string DIR_Oblivion = "file:////192.168.1.3/User/_SERVE/Assets/Oblivion";
#if HTTPTEST
        const string HTTP_Oblivion = "http://192.168.1.3/Estates/Oblivion";
#endif

        [DataTestMethod]
        [DataRow("Tes", $"{GAME}/Oblivion*.bsa/#Oblivion")]
#if HTTPTEST
        [DataRow("Tes", $"{HTTP_Oblivion}/Oblivion*.bsa#Oblivion")]
#endif
        public void ShouldThrow(string familyName, string uri)
            => Assert.ThrowsException<ArgumentOutOfRangeException>(() => FamilyManager.GetFamily(familyName).ParseResource(new Uri(uri)));

        [DataTestMethod]
        [DataRow("Tes", $"{GAME}/Oblivion*.bsa#Oblivion", "Oblivion", 0, 6, "Oblivion - Meshes.bsa", "trees/treeginkgo.spt", 6865)]
        [DataRow("Tes", $"{FILE_Oblivion}/Data/Oblivion*.bsa#Oblivion", "Oblivion", 0, 6, "Oblivion - Meshes.bsa", "trees/treeginkgo.spt", 6865)]
        [DataRow("Tes", $"{FILE_Oblivion}/Data/Oblivion%20-%20Meshes.bsa#Oblivion", "Oblivion", 0, 1, "Oblivion - Meshes.bsa", "trees/treeginkgo.spt", 6865)]
        //[DataRow("Tes", $"{DIR_Oblivion}/Oblivion*.bsa/#Oblivion", "Oblivion", PakOption.Stream, 6, "Oblivion - Meshes.bsa", "trees/treeginkgo.spt", 6865)]
        //[DataRow("Tes", $"{DIR_Oblivion}/Oblivion%20-%20Meshes.bsa/#Oblivion", "Oblivion", PakOption.Stream, 1, "Oblivion - Meshes.bsa", "trees/treeginkgo.spt", 6865)]
#if HTTPTEST
        [DataRow("Tes", $"{HTTP_Oblivion}/Oblivion*.bsa/#Oblivion", "Oblivion", PakOption.Stream, 6, "Oblivion - Meshes.bsa", "trees/treeginkgo.spt", 6865)]
        [DataRow("Tes", $"{HTTP_Oblivion}/Oblivion%20-%20Meshes.bsa/#Oblivion", "Oblivion", PakOption.Stream, 1, "Oblivion - Meshes.bsa", "trees/treeginkgo.spt", 6865)]
#endif
        public void Resource(string familyName, string uri, string game, GameOption options, int pathsFound, string firstPak, string sampleFile, int sampleFileSize)
        {
            var family = FamilyManager.GetFamily(familyName);
            var resource = family.ParseResource(new Uri(uri));
            Assert.AreEqual(game, resource.Game.Id);
            //Assert.AreEqual(options, resource.Options);
            //Assert.AreEqual(pathsFound, resource.Paths.Length);
            var pakFile = family.OpenPakFile(new Uri(uri));
            if (pakFile is MultiPakFile multiPakFile)
            {
                Assert.AreEqual(pathsFound, multiPakFile.PakFiles.Count);
                pakFile = multiPakFile.PakFiles[0];
            }
            if (pakFile == null) throw new InvalidOperationException("pak not opened");
            Assert.AreEqual(firstPak, pakFile.Name);
            Assert.IsTrue(pakFile.Contains(sampleFile));
            Assert.AreEqual(sampleFileSize, pakFile.LoadFileData(sampleFile).Result.Length);
        }
    }
}
