using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace GameX
{
    [TestClass]
    public class FileManagerTests
    {
        [TestMethod]
        public void MissingEstate_ShouldThrow()
            => Assert.ThrowsException<ArgumentOutOfRangeException>(() => FamilyManager.GetFamily("Missing"));

        [DataTestMethod]
        [DataRow("AC", "AC", "*.dat", 4)]
        [DataRow("Arkane", "AF", "*.pak", 7)]
        [DataRow("Arkane", "DOM", "*_dir.vpk", 9)]
        [DataRow("Arkane", "D", "*TOC.txt", 1)]
        [DataRow("Arkane", "D2", "*.index", 28)]
        [DataRow("Arkane", "P", "*.pak", 14)]
        [DataRow("Arkane", "D:DOTO", "*.index", 10)]
        [DataRow("Arkane", "W:YB", "*", 56)]
        [DataRow("Arkane", "W:CP", "*", 25)]
        [DataRow("Arkane", "DL", "*.index", 5)]
        //[DataRow("Arkane", "RF", "*.index", 1)] //: future
        [DataRow("Bioware", "SS", "*", 77)]
        [DataRow("Bioware", "BG", "*.key", 1)]
        [DataRow("Bioware", "MDK2", "*.zip", 4)]
        [DataRow("Bioware", "BG2", "*.key", 1)]
        [DataRow("Bioware", "NWN", "*.key", 1)]
        [DataRow("Bioware", "KotOR", "*.key", 1)]
        [DataRow("Bioware", "JE", "*.key", 1)]
        [DataRow("Bioware", "ME", "*.key", 0)]
        [DataRow("Bioware", "NWN2", "*.zip", 81)]
        [DataRow("Bioware", "DA:O", "*", 2)]
        [DataRow("Bioware", "ME2", "*", 3)]
        [DataRow("Bioware", "DA2", "*", 3)]
        [DataRow("Bioware", "SWTOR", "*.tor", 101)]
        [DataRow("Bioware", "ME3", "*", 3)]
        [DataRow("Bioware", "DA:I", "*", 8)]
        [DataRow("Bioware", "ME:A", "*", 48)]
        [DataRow("Bioware", "A", "*", 12)]
        [DataRow("Bioware", "ME:LE", "*", 23230)]
        //[DataRow("Bioware", "DA:D", "*", 1)] //: future
        //[DataRow("Bioware", "ME5", "*", 1)] //: future
        [DataRow("Blizzard", "SC", "*", 5)]
        [DataRow("Blizzard", "D2R", "*", 10)]
        [DataRow("Blizzard", "W3", "*", 0)]
        [DataRow("Blizzard", "WOW", "*", 5)]
        [DataRow("Blizzard", "WOWC", "*", 0)]
        [DataRow("Blizzard", "SC2", "*", 7)]
        [DataRow("Blizzard", "D3", "*", 56)]
        [DataRow("Blizzard", "HS", "*", 25)]
        [DataRow("Blizzard", "HOTS", "*", 5)]
        [DataRow("Blizzard", "CB", "*", 6)]
        [DataRow("Blizzard", "DI", "*", 4)]
        [DataRow("Blizzard", "OW2", "*", 5)]
        [DataRow("Blizzard", "D4", "*", 40)]
        [DataRow("Capcom", "BionicCommando", "*.bundle", 3)]
        [DataRow("Cry", "ArcheAge", "*.pak", 5)]
        [DataRow("Cry", "Hunt", "*.pak", 69)]
        [DataRow("Cry", "Warface", "*.pak", 51)]
        [DataRow("Cry", "Wolcen", "*.pak", 134)]
        [DataRow("Cry", "Crysis", "*.pak", 21)]
        [DataRow("Cry", "Ryse", "*.pak", 24)]
        [DataRow("Cry", "Robinson", "*.pak", 15)]
        [DataRow("Cry", "Snow", "*.pak", 7)]
        [DataRow("Red", "Witcher", "*.bif", 0)]
        [DataRow("Red", "Witcher2", "*.dzip", 0)]
        [DataRow("Red", "Witcher3", "*.store", 0)]
        [DataRow("Rsi", "StarCitizen", "Data.p4k", 0)]
        [DataRow("Tes", "Fallout4VR", "Fallout4 - *.ba2", 0)]
        [DataRow("Origin", "UltimaOnline", "*.idx", 0)]
        [DataRow("Origin", "UltimaIX", "static/*.flx", 0)]
        [DataRow("Valve", "Dota2", "core/pak01_dir.vpk", 0)]
        public void FileManager(string familyName, string game, string searchPattern, int count)
        {
            var family = FamilyManager.GetFamily(familyName);
            var fileManager = family.FileManager;
            Assert.IsTrue(fileManager.HasPaths);
            //var paths = fileManager.FindGameFilePaths(family, null, family.GetGame(game), searchPattern);
            //Assert.AreEqual(count, paths.Length);
        }
    }
}
