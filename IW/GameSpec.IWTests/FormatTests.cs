using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace GameSpec.IW
{
    [TestClass]
    public class FormatsTests
    {
        const string folderCOD2Url = "game:/iw_00.iwd#COD2"; const int ExpectedCellDatFileCount = 805003;
        const string localCOD2Url = "game:/localized_english_iw00.iwd#COD2";

        readonly Family family = FamilyManager.GetFamily("IW");

        [TestMethod]
        public void LoadCellDat_NoExceptions()
        {
            var dat = family.OpenPakFile(new Uri(folderCOD2Url));
            var count = dat.Count;
            Assert.IsTrue(ExpectedCellDatFileCount <= count, $"Insufficient files parsed from .dat. Expected: >= {ExpectedCellDatFileCount}, Actual: {count}");
        }

        // https://github.com/mauserzjeh/iwi
        // https://github.com/mauserzjeh/iwi2dds
        // https://github.com/mauserzjeh/iwi2dds/blob/master/main.go
        // https://github.com/mauserzjeh/cod-asset-importer
        // https://wiki.zeroy.com/index.php?title=Call_of_Duty_4:_d3dbsp
    }
}
