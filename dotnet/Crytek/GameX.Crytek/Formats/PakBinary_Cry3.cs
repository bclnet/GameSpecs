using GameX.Formats;
using ICSharpCode.SharpZipLib.Zip;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using static OpenStack.Debug;

namespace GameX.Crytek.Formats
{
    /// <summary>
    /// PakBinaryCry3
    /// </summary>
    /// <seealso cref="GameX.Formats.PakBinary" />
    public unsafe class PakBinary_Cry3 : PakBinary<PakBinary_Cry3>
    {
        readonly byte[] Key;

        public PakBinary_Cry3() { }
        public PakBinary_Cry3(byte[] key = null) => Key = key;

        public override Task Read(BinaryPakFile source, BinaryReader r, object tag)
        {
            var files = source.Files = new List<FileSource>();
            source.UseReader = false;

            var pak = (Cry3File)(source.Tag = new Cry3File(r.BaseStream, Key));
            var parentByPath = new Dictionary<string, FileSource>();
            var partByPath = new Dictionary<string, SortedList<string, FileSource>>();
            foreach (ZipEntry entry in pak)
            {
                var metadata = new FileSource
                {
                    Path = entry.Name.Replace('\\', '/'),
                    Crypted = entry.IsCrypted,
                    PackedSize = entry.CompressedSize,
                    FileSize = entry.Size,
                    Tag = entry,
                };
                if (metadata.Path.EndsWith(".dds", StringComparison.OrdinalIgnoreCase)) parentByPath.Add(metadata.Path, metadata);
                else if (metadata.Path[^8..].Contains(".dds.", StringComparison.OrdinalIgnoreCase))
                {
                    var parentPath = metadata.Path[..(metadata.Path.IndexOf(".dds", StringComparison.OrdinalIgnoreCase) + 4)];
                    var parts = partByPath.TryGetValue(parentPath, out var z) ? z : null;
                    if (parts == null) partByPath.Add(parentPath, parts = new SortedList<string, FileSource>());
                    parts.Add(metadata.Path, metadata);
                    continue;
                }
                files.Add(metadata);
            }

            // process links
            if (partByPath.Count != 0)
                foreach (var kv in partByPath) if (parentByPath.TryGetValue(kv.Key, out var parent)) parent.Parts = kv.Value.Values;
            return Task.CompletedTask;
        }

        public override Task Write(BinaryPakFile source, BinaryWriter w, object tag)
        {
            source.UseReader = false;
            var files = source.Files;
            var pak = (Cry3File)(source.Tag = new Cry3File(w.BaseStream, Key));
            pak.BeginUpdate();
            foreach (var file in files)
            {
                var entry = (ZipEntry)(file.Tag = new ZipEntry(Path.GetFileName(file.Path)));
                pak.Add(entry);
                source.PakBinary.WriteData(source, w, file, null);
            }
            pak.CommitUpdate();
            return Task.CompletedTask;
        }

        public override Task<Stream> ReadData(BinaryPakFile source, BinaryReader r, FileSource file, FileOption option = default)
        {
            var pak = (Cry3File)source.Tag;
            var entry = (ZipEntry)file.Tag;
            try
            {
                using var input = pak.GetInputStream(entry);
                if (!input.CanRead) { HandleException(file, option, $"Unable to read stream for file: {file.Path}"); return Task.FromResult(System.IO.Stream.Null); }
                var s = new MemoryStream();
                input.CopyTo(s);
                s.Position = 0;
                return Task.FromResult((Stream)s);
            }
            catch (Exception e) { HandleException(file, option, $"{file.Path} - Exception: {e.Message}"); return Task.FromResult(System.IO.Stream.Null); }
        }

        public override Task WriteData(BinaryPakFile source, BinaryWriter w, FileSource file, Stream data, FileOption option = default)
        {
            var pak = (Cry3File)source.Tag;
            var entry = (ZipEntry)file.Tag;
            try
            {
                using var s = pak.GetInputStream(entry);
                data.CopyTo(s);
            }
            catch (Exception e) { HandleException(file, option, $"Exception: {e.Message}"); }
            return Task.CompletedTask;
        }
    }
}