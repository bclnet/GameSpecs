using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace GameX.IW
{
    [TestClass]
    public class FormatTests2
    {
        static readonly Family family = FamilyManager.GetFamily("IW");
        static PakFile main = family.OpenPakFile(new Uri("game:/xxx#MW2"));

        //[DataTestMethod]
        //[DataRow("dialogues00.bif:09_ban2ban01.dlg")]
        //public void DLG(string sampleFile) => LoadObject<BinaryGff>(main, sampleFile);

        //[DataTestMethod]
        //[DataRow("quests00.bif:act1.qdb")]
        //public void QDB(string sampleFile) => LoadObject<BinaryGff>(main, sampleFile);

        //[DataTestMethod]
        //[DataRow("quests00.bif:q1000_act1_init.qst")]
        //public void QST(string sampleFile) => LoadObject<BinaryGff>(main, sampleFile);

        //[DataTestMethod]
        //[DataRow("meshes00.bif/alpha_dummy.mdb")]
        //public void MDB(string sampleFile) => LoadObject<BiowareBinaryPak>(main, sampleFile);

        static void LoadObject<T>(PakFile source, string sampleFile)
        {
            Assert.IsTrue(source.Contains(sampleFile));
            var result = source.LoadFileObject<T>(sampleFile).Result;
        }
    }
}
