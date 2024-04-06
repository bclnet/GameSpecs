using GameX.Blizzard.Formats.Casc;
using GameX.Formats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace GameX.Blizzard.Formats
{
    public unsafe class PakBinary_Blizzard : PakBinary<PakBinary_Blizzard>
    {
        CascContext casc;

        public override Task Read(BinaryPakFile source, BinaryReader r, object tag)
        {
            var files = source.Files = new List<FileSource>();

            // load casc
            var editions = source.Game.Editions;
            var product = editions.First().Key;
            casc = new CascContext();
            casc.Read(source.PakPath, product, files);
            return Task.CompletedTask;
        }

        public override Task<Stream> ReadData(BinaryPakFile source, BinaryReader r, FileSource file, FileOption option = default)
            => Task.FromResult(casc.ReadData(file));
    }
}