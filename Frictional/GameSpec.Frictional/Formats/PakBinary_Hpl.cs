using GameSpec.Formats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace GameSpec.Frictional.Formats
{
    public unsafe class PakBinary_Hpl : PakBinary
    {
        public static readonly PakBinary Instance = new PakBinary_Hpl();

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