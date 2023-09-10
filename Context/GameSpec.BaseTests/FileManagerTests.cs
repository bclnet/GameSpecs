using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text.Json;

namespace GameSpec
{
    [TestClass]
    public class FileManagerTests
    {
        static readonly Family Family = FamilyManager.Unknown;

        [TestMethod]
        public void ShouldParseResource()
        {
            var fileManager = Family.FileManager;
            Assert.IsNotNull(fileManager.ParseResource(Family, null, false));
        }

        [TestMethod]
        public void ShouldFindGameFilePaths()
        {
            var fileManager = Family.FileManager;
            Assert.IsFalse(fileManager.HasPaths);
            Assert.IsNull(fileManager.FindGameFilePaths(Family, null, Family.GetGame("CAT"), null));
        }

        [TestMethod]
        public void ShouldParseFileManager()
        {
            var fileManager = Family.FileManager;
            using var doc = JsonDocument.Parse(Some.FileManagerJson.Replace("'", "\""));
            var elem = doc.RootElement;
            Assert.IsNotNull(fileManager.ParseFileManager(elem));
        }
    }
}
