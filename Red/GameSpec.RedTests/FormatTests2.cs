using GameSpec.Aurora.Formats;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace GameSpec.Red
{
    [TestClass]
    public class FormatTests2
    {
        static readonly Family family = FamilyManager.GetFamily("Red");
        static PakFile main = family.OpenPakFile(new Uri("game:/main.key#Witcher"));

        [DataTestMethod]
        [DataRow("dialogues00.bif:09_ban2ban01.dlg")]
        public void DLG(string sampleFile) => LoadObject<BinaryGff>(main, sampleFile);

        [DataTestMethod]
        [DataRow("quests00.bif:act1.qdb")]
        public void QDB(string sampleFile) => LoadObject<BinaryGff>(main, sampleFile);

        [DataTestMethod]
        [DataRow("quests00.bif:q1000_act1_init.qst")]
        public void QST(string sampleFile) => LoadObject<BinaryGff>(main, sampleFile);

        //[DataTestMethod]
        //[DataRow("meshes00.bif/alpha_dummy.mdb")]
        //public void MDB(string sampleFile) => LoadObject<AuroraBinaryPak>(main, sampleFile);

        static void LoadObject<T>(PakFile source, string sampleFile)
        {
            Assert.IsTrue(source.Contains(sampleFile));
            var result = source.LoadFileObjectAsync<T>(sampleFile).Result;
        }
    }
}
