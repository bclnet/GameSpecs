using GameX.Formats;
using GameX.Formats.Apple;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace GameX.Capcom.Formats
{
    public unsafe class PakBinary_Plist : PakBinary<PakBinary_Plist>
    {
        public override Task Read(BinaryPakFile source, BinaryReader r, object tag)
        {
            source.Files = ((Dictionary<object, object>)new PlistReader().ReadObject(r.BaseStream)).Select(x => new FileSource
            {
                Path = (string)x.Key,
                FileSize = ((byte[])x.Value).Length,
                Tag = x.Value,
            }).ToArray();
            return Task.CompletedTask;
        }

        public override Task<Stream> ReadData(BinaryPakFile source, BinaryReader r, FileSource file, FileOption option = default)
            => Task.FromResult<Stream>(new MemoryStream((byte[])file.Tag));
    }
}