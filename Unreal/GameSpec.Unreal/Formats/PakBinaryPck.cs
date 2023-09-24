using GameSpec.Formats;
using GameSpec.Unreal.Formats.OldWay;
using GameSpec.Unreal.Formats.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

// https://www.gildor.org/en/projects/umodel
// https://github.com/gildor2/UEViewer
// https://www.gildor.org/smf/index.php/topic,297.0.html
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

            List<FileMetadata> files;
            multiSource.Files = files = new List<FileMetadata>();
            //var header1 = new UPackage(r); r.BaseStream.Position = 0;
            var header = new Core.UPackage(r, source.FilePath);
            if (header.Exports != null)
            {
                foreach (var item in header.Exports)
                    files.Add(new FileMetadata
                    {
                        Path = $"{header.GetClassNameFor(item)}/{item.ObjectName}",
                        Position = item.SerialOffset,
                        FileSize = item.SerialSize,
                    });
            }

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