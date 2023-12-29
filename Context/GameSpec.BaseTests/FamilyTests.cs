using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using static GameSpec.FamilyGame;

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
        public void ShouldGetGame()
        {
            var family = Some.Family;
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => family.GetGame("Wrong", out var _));
            Assert.IsNotNull(family.GetGame("Missing", out var _));
            Assert.IsNotNull(family.GetGame("Found", out var _));
        }

        [TestMethod]
        public void ShouldParseResource()
        {
            var family = Some.Family;
            Assert.IsNotNull(family.ParseResource(null, true));
        }

        [TestMethod]
        public void ShouldOpenPakFile_Paths()
        {
            var family = Some.Family;
            Assert.ThrowsException<ArgumentNullException>(() => family.OpenPakFile(null, null, null, null));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => family.OpenPakFile(family.GetGame("Missing", out var edition), edition, null, null));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => family.OpenPakFile(family.GetGame("Found", out var edition), edition, null, null));
            Assert.IsNull(family.OpenPakFile(family.GetGame("Missing", out var edition), edition, null, null, throwOnError: false));
            Assert.IsNotNull(family.OpenPakFile(family.GetGame("Found", out edition), edition, "path", null));
        }

        [TestMethod]
        public void ShouldOpenPakFile_Resource()
        {
            var family = Some.Family;
            Assert.ThrowsException<ArgumentNullException>(() => family.OpenPakFile(new Resource { }));
            //Assert.IsNull(family.OpenPakFile(new Resource { Paths = null, Game = FamilyGame.Empty }, throwOnError: false));
            //Assert.IsNotNull(family.OpenPakFile(new Resource { Paths = new[] { "path" }, Game = family.GetGame("Found") }));
        }

        [TestMethod]
        public void ShouldOpenPakFile_Uri()
        {
            var family = Some.Family;
            Assert.IsNull(family.OpenPakFile(null, throwOnError: false));
            //// game-scheme
            //Assert.IsNull(null, family.OpenPakFile(new Uri("game:/path#Found")));
            //// file-scheme
            //Assert.IsNull(null, family.OpenPakFile(new Uri("file://path#Found")));
            //// network-scheme
            //Assert.IsNull(null, family.OpenPakFile(new Uri("https://path#Found")));
        }
    }
}
