using Microsoft.VisualStudio.TestTools.UnitTesting;
using static GameX.FamilyManager;
using static OpenStack.Debug;

namespace GameX
{
    [TestClass]
    public class FileDataTests
    {
        [DataTestMethod]
        [DataRow("Arkane", "game:/#AF", "sample:0")]
        public void Resource(string familyName, string file0, string file1)
        {
            // get family
            var family = GetFamily(familyName);
            Log($"studio: {family.Studio}");

            // get pak with game:/uri
            var pakFile = family.OpenPakFile(file0);
            var sample = file1.StartsWith("sample") ? pakFile.Game.GetSample(file1[7..]).Path : file1;
            Log($"pak: {pakFile}, {sample}");

            // get file
            var data = pakFile.LoadFileData(sample).Result;
            Log($"dat: {data}");
        }
    }
}
