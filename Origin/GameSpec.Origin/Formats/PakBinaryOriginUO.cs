using GameSpec.Formats;
using System;
using System.IO;
using System.Threading.Tasks;

namespace GameSpec.Origin.Formats
{
    public unsafe class PakBinaryOriginUO : PakBinary
    {
        public static readonly PakBinary Instance = new PakBinaryOriginUO();

        public override Task ReadAsync(BinaryPakFile source, BinaryReader r, object tag)
        {
            return Task.CompletedTask;
        }

        public override Task<Stream> ReadDataAsync(BinaryPakFile source, BinaryReader r, FileSource file, DataOption option = 0, Action<FileSource, string> exception = null)
        {
            r.Seek(file.Position);
            return Task.FromResult((Stream)new MemoryStream(r.ReadBytes((int)file.FileSize)));
        }
    }
}