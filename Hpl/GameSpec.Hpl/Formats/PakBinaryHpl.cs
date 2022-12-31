using Compression;
using GameSpec.Formats;
using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Threading.Tasks;
using ZstdNet;
using static OpenStack.Debug;

namespace GameSpec.Hpl.Formats
{
    public unsafe class PakBinaryHpl : PakBinary
    {
        public static readonly PakBinary Instance = new PakBinaryHpl();
        PakBinaryHpl() { }

        public override Task ReadAsync(BinaryPakFile source, BinaryReader r, ReadStage stage)
        {
            if (!(source is BinaryPakManyFile multiSource)) throw new NotSupportedException();
            if (stage != ReadStage.File) throw new ArgumentOutOfRangeException(nameof(stage), stage.ToString());

            var extension = Path.GetExtension(source.FilePath);
            var files = multiSource.Files = new List<FileMetadata>();

            return Task.CompletedTask;
        }

        public override Task<Stream> ReadDataAsync(BinaryPakFile source, BinaryReader r, FileMetadata file, DataOption option = 0, Action<FileMetadata, string> exception = null)
        {
            throw new NotImplementedException();
        }
    }
}