using GameSpec.Formats;
using GameSpec.Unreal.Formats.Core;
using System;
using System.IO;
using System.Threading.Tasks;

namespace GameSpec.Unreal.Formats
{
    /// <summary>
    /// PakBinaryPck
    /// </summary>
    /// <seealso cref="GameSpec.Formats.PakBinary" />
    public unsafe class PakBinaryPck : PakBinary
    {
        public static readonly PakBinary Instance = new PakBinaryPck();

        public override Task ReadAsync(BinaryPakFile source, BinaryReader r, ReadStage stage)
        {
            if (!(source is BinaryPakManyFile multiSource)) throw new NotSupportedException();
            if (stage != ReadStage.File) throw new ArgumentOutOfRangeException(nameof(stage), stage.ToString());

            var header = new UPackage(r);

            return Task.CompletedTask;
        }

        public override Task WriteAsync(BinaryPakFile source, BinaryWriter w, WriteStage stage)
            => throw new NotImplementedException();

        public override Task<Stream> ReadDataAsync(BinaryPakFile source, BinaryReader r, FileMetadata file, DataOption option = 0, Action<FileMetadata, string> exception = null)
        {
            return null;
        }

        public override Task WriteDataAsync(BinaryPakFile source, BinaryWriter w, FileMetadata file, Stream data, DataOption option = 0, Action<FileMetadata, string> exception = null)
            => throw new NotImplementedException();
    }
}