using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace GameSpec
{
    [TestClass]
    public class EstateManagerTests
    {
        [TestMethod]
        public void EstatesIsZero()
        {
            Assert.AreEqual(0, FamilyManager.Families.Count);
        }

        [TestMethod]
        public void GetEstate()
        {
            Assert.ThrowsException<ArgumentNullException>(() => FamilyManager.GetFamily(null));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => FamilyManager.GetFamily("Missing"));
        }

        [TestMethod]
        public void ParseEstate()
        {
            Assert.ThrowsException<ArgumentNullException>(() => FamilyManager.ParseFamily(null));
            Assert.IsNotNull(FamilyManager.ParseFamily(Some.FamilyJson.Replace("'", "\"")));
        }
    }
}
