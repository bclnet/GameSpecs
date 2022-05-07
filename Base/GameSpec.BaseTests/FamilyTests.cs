using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace GameSpec
{
    [TestClass]
    public class FamilyTests
    {
        [TestMethod]
        public void Bootstrap_CanRegisterAnotherStartup()
        {
            lock (this)
            {
                FamilyPlatform.Startups.Clear();
                Assert.AreEqual(0, FamilyPlatform.Startups.Count, "None registered");
                FamilyPlatform.Startups.Add(SomePlatform.Startup);
                Family.Bootstrap();
                Assert.AreEqual(1, FamilyPlatform.Startups.Count, "Single Startup");
                Assert.AreEqual(SomePlatform.Startup, FamilyPlatform.Startups.First(), $"Default is {nameof(SomePlatform.Startup)}");
            }
        }

        [TestMethod]
        public void GetGame()
        {
            var family = Some.Family;
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => family.GetGame("Wrong"));
            Assert.IsNotNull(family.GetGame("Found"));
        }

        [TestMethod]
        public void OpenPakFile_Paths()
        {
            var family = Some.Family;
            Assert.ThrowsException<ArgumentNullException>(() => family.OpenPakFile(null, null));
            Assert.ThrowsException<ArgumentNullException>(() => family.OpenPakFile(null, "Wrong"));
            Assert.AreEqual(null, family.OpenPakFile(null, "Wrong", throwOnError: false));
            Assert.AreEqual(null, family.OpenPakFile(null, "Found"));
            Assert.IsNotNull(family.OpenPakFile(new string[] { "path" }, "Found"));
        }

        [TestMethod]
        public void OpenPakFile_Resource()
        {
            var family = Some.Family;
            Assert.ThrowsException<ArgumentNullException>(() => family.OpenPakFile(new Resource { }));
            Assert.AreEqual(null, family.OpenPakFile(new Resource { }, throwOnError: false));
            Assert.IsNotNull(family.OpenPakFile(new Resource { Paths = new[] { "path" }, Game = "Found" }));
        }

        [TestMethod]
        public void OpenPakFile_Uri()
        {
            var family = Some.Family;
            Assert.AreEqual(null, family.OpenPakFile(null));
            //// game-scheme
            //Assert.AreEqual(null, family.OpenPakFile(new Uri("game:/path#Found")));
            //// file-scheme
            //Assert.AreEqual(null, family.OpenPakFile(new Uri("file://path#Found")));
            //// network-scheme
            //Assert.AreEqual(null, family.OpenPakFile(new Uri("https://path#Found")));
        }
    }
}
