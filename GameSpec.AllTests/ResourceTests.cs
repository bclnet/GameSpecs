using GameEstate.Formats;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using static GameEstate.Estate;

namespace GameEstate
{
    public class ResourceTests
    {
        const string GAME = "game:";
        const string FILE_Oblivion = "file:///D:/Program%20Files%20(x86)/Steam/steamapps/common/Oblivion";
        const string DIR_Oblivion = "file:////192.168.1.3/User/_SERVE/Assets/Oblivion";
        const string HTTP_Oblivion = "http://192.168.1.3/Estates/Oblivion";

        [DataTestMethod]
        [DataRow("Tes", $"{GAME}/Oblivion*.bsa/#Oblivion")]
        [DataRow("Tes", $"{HTTP_Oblivion}/Oblivion*.bsa#Oblivion")]
        public void ShouldThrow(string estateName, string uri)
            => Assert.ThrowsException<ArgumentOutOfRangeException>(() => EstateManager.GetEstate(estateName).ParseResource(new Uri(uri)));

        [DataTestMethod]
        [DataRow("Tes", $"game:/Oblivion*.bsa#Oblivion", "Oblivion", 0, 6, "Oblivion - Meshes.bsa", "trees/treeginkgo.spt", 2059)]
        [DataRow("Tes", $"{FILE_Oblivion}/Data/Oblivion*.bsa#Oblivion", "Oblivion", 0, 6, "Oblivion - Meshes.bsa", "trees/treeginkgo.spt", 2059)]
        [DataRow("Tes", $"{FILE_Oblivion}/Data/Oblivion%20-%20Meshes.bsa#Oblivion", "Oblivion", 0, 1, "Oblivion - Meshes.bsa", "trees/treeginkgo.spt", 2059)]
        //[DataRow("Tes", $"{DIR_Oblivion}/Oblivion*.bsa/#Oblivion", "Oblivion", PakOption.Stream, 6, "Oblivion - Meshes.bsa", "trees/treeginkgo.spt", 2059)]
        //[DataRow("Tes", $"{DIR_Oblivion}/Oblivion%20-%20Meshes.bsa/#Oblivion", "Oblivion", PakOption.Stream, 1, "Oblivion - Meshes.bsa", "trees/treeginkgo.spt", 2059)]
        [DataRow("Tes", $"{HTTP_Oblivion}/Oblivion*.bsa/#Oblivion", "Oblivion", PakOption.Stream, 6, "Oblivion - Meshes.bsa", "trees/treeginkgo.spt", 2059)]
        [DataRow("Tes", $"{HTTP_Oblivion}/Oblivion%20-%20Meshes.bsa/#Oblivion", "Oblivion", PakOption.Stream, 1, "Oblivion - Meshes.bsa", "trees/treeginkgo.spt", 2059)]
        public void Resource(string estateName, string uri, string game, PakOption options, int pathsFound, string firstPak, string sampleFile, int sampleFileSize)
        {
            var estate = EstateManager.GetEstate(estateName);
            var resource = estate.ParseResource(new Uri(uri));
            Assert.AreEqual(game, resource.Game);
            Assert.AreEqual(options, resource.Options);
            Assert.AreEqual(pathsFound, resource.Paths.Length);
            var pakFile = estate.OpenPakFile(new Uri(uri));
            if (pakFile is MultiPakFile multiPakFile)
            {
                Assert.AreEqual(pathsFound, multiPakFile.PakFiles.Count);
                pakFile = multiPakFile.PakFiles[0];
            }
            if (pakFile == null) throw new InvalidOperationException("pak not opened");
            Assert.AreEqual(firstPak, pakFile.Name);
            Assert.IsTrue(pakFile.Contains(sampleFile));
            Assert.AreEqual(sampleFileSize, pakFile.LoadFileDataAsync(sampleFile).Result.Length);
        }
    }
}
