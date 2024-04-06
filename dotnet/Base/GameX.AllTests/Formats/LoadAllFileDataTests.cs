using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Threading.Tasks;

namespace GameX.Formats
{
    [TestClass]
    public class LoadAllFileDataTests
    {
        [DataTestMethod]
        [DataRow("AC:AC")]
        [DataRow("Arkane:D2", 10000000)]
        [DataRow("Cry:MWO")]
        [DataRow("Cyanide:TheCouncil")]
        [DataRow("Origin:UltimaOnline")]
        [DataRow("Origin:UltimaIX")]
        [DataRow("Rsi:StarCitizen", 10000000)]
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
        [DataRow("Tes:Fallout76", 15000000)]
        [DataRow("Valve:Dota2", 15000000)]
        public async Task LoadAllFileData(string pak, long maxFileSize = 0)
        {
            var source = TestHelper.Paks[pak].Value;
            if (source is MultiPakFile multiPak)
                foreach (var p in multiPak.PakFiles)
                {
                    if (p is not BinaryPakFile z) throw new InvalidOperationException("multiPak not a BinaryPakFile");
                    await ExportAsync(z, maxFileSize);
                }
            else await ExportAsync(source, maxFileSize);
        }

        static Task ExportAsync(PakFile source, long maxSize)
        {
            if (source is not BinaryPakFile pak) throw new NotSupportedException();

            // write files
            Parallel.For(0, pak.Files.Count, new ParallelOptions { /*MaxDegreeOfParallelism = 1*/ }, async index =>
            {
                var file = pak.Files[index];

                // extract pak
                if (file.Pak != null) { await ExportAsync(file.Pak, maxSize); return; }
                // skip empty file
                if (file.FileSize == 0 && file.PackedSize == 0) return;
                // skip large files
                if (maxSize != 0 && file.FileSize > maxSize) return;

                // extract file
                using var s = await pak.LoadFileData(file);
                s.ReadAllBytes();
            });

            return Task.CompletedTask;
        }
    }
}
