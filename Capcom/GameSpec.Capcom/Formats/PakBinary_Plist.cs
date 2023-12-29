using GameSpec.Formats;
using GameSpec.Formats.Apple;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace GameSpec.Capcom.Formats
{
    public unsafe class PakBinary_Plist : PakBinary
    {
        public static readonly PakBinary Instance = new PakBinary_Plist();

        public override Task ReadAsync(BinaryPakFile source, BinaryReader r, object tag)
        {
            source.Files = ((Dictionary<object, object>)new PlistReader().ReadObject(r.BaseStream)).Select(x => new FileSource
            {
                Path = (string)x.Key,
                FileSize = ((byte[])x.Value).Length,
                Tag = x.Value,
            }).ToArray();
            return Task.CompletedTask;
        }

        public override Task<Stream> ReadDataAsync(BinaryPakFile source, BinaryReader r, FileSource file, DataOption option = 0, Action<FileSource, string> exception = null)
            => Task.FromResult<Stream>(new MemoryStream((byte[])file.Tag));
    }
}