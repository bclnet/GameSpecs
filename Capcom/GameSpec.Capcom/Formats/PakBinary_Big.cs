using GameSpec.Formats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace GameSpec.Capcom.Formats
{
    public unsafe class PakBinary_Big : PakBinary
    {
        public static readonly PakBinary Instance = new PakBinary_Big();

        public override Task ReadAsync(BinaryPakFile source, BinaryReader r, object tag)
        {
            var files = source.Files = new List<FileSource>();

            return Task.CompletedTask;
        }

        public override Task<Stream> ReadDataAsync(BinaryPakFile source, BinaryReader r, FileSource file, DataOption option = 0, Action<FileSource, string> exception = null)
        {
            throw new NotImplementedException();
        }
    }
}