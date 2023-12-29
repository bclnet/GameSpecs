﻿using GameSpec.Formats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace GameSpec.Epic.Formats
{
    /// <summary>
    /// PakBinaryPck
    /// </summary>
    /// <seealso cref="GameSpec.Formats.PakBinary" />
    public unsafe class PakBinary_Pck : PakBinary
    {
        public static readonly PakBinary Instance = new PakBinary_Pck();

        public override Task ReadAsync(BinaryPakFile source, BinaryReader r, object tag)
        {
            List<FileSource> files;
            source.Files = files = new List<FileSource>();
            var header = new Core.UPackage(r, source.FilePath);
            if (header.Exports == null) return Task.CompletedTask;
            var R = header.R;
            foreach (var item in header.Exports)
                files.Add(new FileSource
                {
                    Path = $"{header.GetClassNameFor(item)}/{item.ObjectName}",
                    Position = item.SerialOffset,
                    FileSize = item.SerialSize,
                    Tag = R,
                });
            return Task.CompletedTask;
        }

        public override Task<Stream> ReadDataAsync(BinaryPakFile source, BinaryReader r, FileSource file, DataOption option = default, Action<FileSource, string> exception = default)
        {
            var R = (BinaryReader)file.Tag;
            R.Seek(file.Position);
            return Task.FromResult((Stream)new MemoryStream(R.ReadBytes((int)file.FileSize)));
        }

        public override Task WriteDataAsync(BinaryPakFile source, BinaryWriter w, FileSource file, Stream data, DataOption option = default, Action<FileSource, string> exception = default)
            => throw new NotImplementedException();
    }
}