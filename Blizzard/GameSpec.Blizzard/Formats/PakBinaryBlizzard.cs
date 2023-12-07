using GameSpec.Blizzard.Formats.Casc;
using GameSpec.Formats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace GameSpec.Blizzard.Formats
{
    public unsafe class PakBinaryBlizzard : PakBinary
    {
        public static readonly PakBinary Instance = new PakBinaryBlizzard();
        CascContext casc;

        public override Task ReadAsync(BinaryPakFile source, BinaryReader r, object tag)
        {
            if (!(source is BinaryPakManyFile multiSource)) throw new NotSupportedException();
            var files = multiSource.Files = new List<FileSource>();

            // load casc
            var editions = source.Game.Editions;
            var product = editions.First().Key;
            casc = new CascContext();
            casc.Read(source.FilePath, product, files);
            return Task.CompletedTask;
        }

        public override Task<Stream> ReadDataAsync(BinaryPakFile source, BinaryReader r, FileSource file, DataOption option = 0, Action<FileSource, string> exception = null)
            => Task.FromResult(casc.ReadData(file));
    }
}