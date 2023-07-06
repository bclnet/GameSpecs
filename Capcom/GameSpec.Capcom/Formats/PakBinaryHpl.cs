﻿using GameSpec.Formats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace GameSpec.Capcom.Formats
{
    public unsafe class PakBinaryCapcom : PakBinary
    {
        public static readonly PakBinary Instance = new PakBinaryCapcom();

        public override Task ReadAsync(BinaryPakFile source, BinaryReader r, ReadStage stage)
        {
            if (source is not BinaryPakManyFile multiSource) throw new NotSupportedException();
            if (stage != ReadStage.File) throw new ArgumentOutOfRangeException(nameof(stage), stage.ToString());
            var files = multiSource.Files = new List<FileMetadata>();

            return Task.CompletedTask;
        }

        public override Task<Stream> ReadDataAsync(BinaryPakFile source, BinaryReader r, FileMetadata file, DataOption option = 0, Action<FileMetadata, string> exception = null)
        {
            throw new NotImplementedException();
        }
    }
}