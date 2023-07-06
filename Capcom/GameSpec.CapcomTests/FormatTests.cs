using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace GameSpec.Capcom
{
    [TestClass]
    public class FormatsTests
    {
        const string folderCOD2Url = "game:/iw_00.iwd#P:O"; const int ExpectedCellDatFileCount = 805003;
        const string localCOD2Url = "game:/localized_english_iw00.iwd#P:O";

        readonly Family family = FamilyManager.GetFamily("Capcom");

        [TestMethod]
        public void LoadCellDat_NoExceptions()
        {
            var dat = family.OpenPakFile(new Uri(folderCOD2Url));
            var count = dat.Count;
            Assert.IsTrue(ExpectedCellDatFileCount <= count, $"Insufficient files parsed from .dat. Expected: >= {ExpectedCellDatFileCount}, Actual: {count}");
        }
    }
}
