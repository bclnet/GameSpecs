using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Text.Json;

namespace GameX
{
    [TestClass]
    public class FileManagerTests
    {
        static readonly Family Family = FamilyManager.Unknown;

        [DataTestMethod]
        [DataRow(null)]
        [DataRow("game:/#APP")]
        [DataRow("file:///C:/#APP")]
        [DataRow("https://localhost#APP")]
        public void ShouldParseResource(string uri)
        {
            Assert.IsNotNull(Family.ParseResource(uri != null ? new Uri(uri) : null, false));
        }

        [TestMethod]
        public void ShouldParseFileManager()
        {
            var fileManager = Family.FileManager;
            using var doc = JsonDocument.Parse(Some.FileManagerJson.Replace("'", "\""));
            var elem = doc.RootElement;
            //Assert.IsNotNull(fileManager.ParseFileManager(elem));
        }
    }
}
