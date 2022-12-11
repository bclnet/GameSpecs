using GameSpec.Formats;
using System;
using System.IO;
using System.Threading.Tasks;

namespace GameSpec.Blizzard.Formats
{
    public unsafe class PakBinaryBlizzard : PakBinary
    {
        public static readonly PakBinary Instance = new PakBinaryBlizzard();
        PakBinaryBlizzard() { }

        public unsafe override Task ReadAsync(BinaryPakFile source, BinaryReader r, ReadStage stage)
        {
            if (!(source is BinaryPakManyFile multiSource)) throw new NotSupportedException();
            if (stage != ReadStage.File) throw new ArgumentOutOfRangeException(nameof(stage), stage.ToString());

            return Task.CompletedTask;
        }

        public unsafe override Task<Stream> ReadDataAsync(BinaryPakFile source, BinaryReader r, FileMetadata file, DataOption option = 0, Action<FileMetadata, string> exception = null)
        {
           throw new NotImplementedException();
        }
    }
}