using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace GameSpec
{
    [TestClass]
    public class FileManagerTests
    {
        [DataTestMethod]
        [DataRow("Android", "GameSpec.AndroidFileManager, GameSpec.Base")]
        [DataRow("Linux", "GameSpec.LinuxFileManager, GameSpec.Base")]
        [DataRow("MacOs", "GameSpec.MacOsFileManager, GameSpec.Base")]
        [DataRow("Windows", "GameSpec.WindowsFileManager, GameSpec.Base")]
        public void ShouldParse(string id, string fileManagerType)
        {
            var fileManager = (FileManager)Activator.CreateInstance(Type.GetType(fileManagerType));
        }
    }
}
