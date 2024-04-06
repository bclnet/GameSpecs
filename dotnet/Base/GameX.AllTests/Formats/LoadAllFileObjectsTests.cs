using GameX.Meta;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Threading.Tasks;

namespace GameX.Formats
{
    [TestClass]
    public class LoadAllFileObjectsTests
    {
        [DataTestMethod]
        [DataRow("AC:AC")]
        [DataRow("Arkane:D2")]
        [DataRow("Cry:MWO")]
        [DataRow("Cyanide:TheCouncil")]
        [DataRow("Origin:#UltimaOnline")]
        [DataRow("Origin:UltimaIX")]
        [DataRow("Rsi:StarCitizen")]
        [DataRow("Red:Witcher")]
        [DataRow("Red:Witcher2")]
        [DataRow("Red:Witcher3")]
        [DataRow("Tes:Morrowind")]
        [DataRow("Tes:Oblivion")]
        [DataRow("Tes:Skyrim")]
        [DataRow("Tes:SkyrimSE")]
        [DataRow("Tes:Fallout2")]
        [DataRow("Tes:Fallout3")]
        [DataRow("Tes:FalloutNV")]
        [DataRow("Tes:Fallout4")]
        [DataRow("Tes:Fallout4VR")]
        [DataRow("Tes:Fallout76")]
        [DataRow("Valve:Dota2")]
        public async Task LoadAllFileObjectsAsync(string pak)
        {
            var source = TestHelper.Paks[pak].Value;
            if (source is MultiPakFile multiPak)
                foreach (var p in multiPak.PakFiles)
                {
                    if (p is not BinaryPakFile z) throw new InvalidOperationException("multiPak not a BinaryPakFile");
                    await ExportAsync(z);
                }
            else await ExportAsync(source);
        }

        static Task ExportAsync(PakFile source)
        {
            if (source is not BinaryPakFile pak) throw new NotSupportedException();

            // write files
            Parallel.For(0, pak.Files.Count, new ParallelOptions { MaxDegreeOfParallelism = 1 }, async index =>
            {
                var file = pak.Files[index];

                // extract pak
                if (file.Pak != null) await ExportAsync(file.Pak);

                // skip empty file
                if (file.FileSize == 0 && file.PackedSize == 0) return;

                // skip large files
                //if (file.FileSize > 50000000) return;

                // extract file
                var obj = await pak.LoadFileObject<object>(file);
                if (obj is Stream stream)
                {
                    var value = MetaManager.GuessStringOrBytes(stream);
                }
                else if (obj is IDisposable disposable) disposable.Dispose();
            });

            return Task.CompletedTask;
        }
    }
}
