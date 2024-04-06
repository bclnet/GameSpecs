using GameX.Formats;
using GameX.Meta;
using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static OpenStack.Debug;

namespace GameX.Cig.Formats
{
    /// <summary>
    /// PakBinaryP4k
    /// </summary>
    /// <seealso cref="GameX.Formats.PakBinary" />
    public class PakBinary_P4k : PakBinary<PakBinary_P4k>
    {
        readonly byte[] Key = new byte[] { 0x5E, 0x7A, 0x20, 0x02, 0x30, 0x2E, 0xEB, 0x1A, 0x3B, 0xB6, 0x17, 0xC3, 0x0F, 0xDE, 0x1E, 0x47 };

        protected class SubPakFileP4k : BinaryPakFile
        {
            P4kFile Pak;

            public SubPakFileP4k(BinaryPakFile source, P4kFile pak, string path, object tag) : base(new PakState(source.FileSystem, source.Game, source.Edition, path, tag), Instance)
            {
                Pak = pak;
                ObjectFactoryFactoryMethod = source.ObjectFactoryFactoryMethod;
                UseReader = false;
                //Open();
            }

            public async override Task Read(BinaryReader r, object tag)
            {
                var entry = (P4kEntry)Tag;
                var stream = Pak.GetInputStream(entry.ZipFileIndex);
                using var r2 = new BinaryReader(stream);
                await PakBinary.Read(this, r2, tag);
            }
        }

        public override Task Read(BinaryPakFile source, BinaryReader r, object tag)
        {
            source.UseReader = false;
            var files = source.Files = new List<FileSource>();

            var pak = (P4kFile)(source.Tag = new P4kFile(r.BaseStream, Key));
            var parentByPath = new Dictionary<string, FileSource>();
            var partsByPath = new Dictionary<string, SortedList<string, FileSource>>();
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
                var metadataPath = metadata.Path;
                if (metadataPath.EndsWith(".pak", StringComparison.OrdinalIgnoreCase) || metadataPath.EndsWith(".socpak", StringComparison.OrdinalIgnoreCase)) metadata.Pak = new SubPakFileP4k(source, pak, metadataPath, metadata.Tag);
                else if (metadataPath.EndsWith(".dds", StringComparison.OrdinalIgnoreCase) || metadataPath.EndsWith(".dds.a", StringComparison.OrdinalIgnoreCase)) parentByPath.Add(metadataPath, metadata);
                else if (metadataPath.Length > 8 && metadataPath[^8..].Contains(".dds.", StringComparison.OrdinalIgnoreCase))
                {
                    var parentPath = metadataPath[..(metadataPath.IndexOf(".dds", StringComparison.OrdinalIgnoreCase) + 4)];
                    if (metadataPath.EndsWith("a")) parentPath += ".a";
                    var parts = partsByPath.TryGetValue(parentPath, out var z) ? z : null;
                    if (parts == null) partsByPath.Add(parentPath, parts = new SortedList<string, FileSource>());
                    parts.Add(metadataPath, metadata);
                    continue;
                }
                files.Add(metadata);
            }

            // process links
            if (partsByPath.Count > 0)
                foreach (var kv in partsByPath) if (parentByPath.TryGetValue(kv.Key, out var parent)) parent.Parts = kv.Value.Values;
            return Task.CompletedTask;
        }

        public override Task<Stream> ReadData(BinaryPakFile source, BinaryReader r, FileSource file, FileOption option = default)
        {
            var pak = (P4kFile)source.Tag;
            var entry = (ZipEntry)file.Tag;
            try
            {
                using var input = pak.GetInputStream(entry.ZipFileIndex);
                if (!input.CanRead) { HandleException(file, option, $"Unable to read stream for file: {file.Path}"); return Task.FromResult(System.IO.Stream.Null); }
                var s = new MemoryStream();
                input.CopyTo(s);
                if (file.Parts != null)
                    foreach (var part in file.Parts.Reverse())
                    {
                        var entry2 = (ZipEntry)part.Tag;
                        using var input2 = pak.GetInputStream(entry2.ZipFileIndex);
                        if (!input2.CanRead) { HandleException(file, option, $"Unable to read stream for file: {part.Path}"); return Task.FromResult(System.IO.Stream.Null); }
                        input2.CopyTo(s);
                    }
                s.Position = 0;
                return Task.FromResult((Stream)s);
            }
            catch (Exception e) { HandleException(file, option, $"{file.Path} - Exception: {e.Message}"); return Task.FromResult(System.IO.Stream.Null); }
        }

        #region Write

        public override Task Write(BinaryPakFile source, BinaryWriter w, object tag)
        {
            source.UseReader = false;
            var files = source.Files;

            var pak = (P4kFile)(source.Tag = new P4kFile(w.BaseStream, Key));
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

        public override Task WriteData(BinaryPakFile source, BinaryWriter w, FileSource file, Stream data, FileOption option = default)
        {
            var pak = (P4kFile)source.Tag;
            var entry = (ZipEntry)file.Tag;
            try
            {
                using var s = pak.GetInputStream(entry);
                data.CopyTo(s);
                if (file.Parts != null)
                    foreach (var part in file.Parts.Reverse())
                    {
                        var entry2 = (ZipEntry)part.Tag;
                        using var s2 = pak.GetInputStream(entry);
                        data.CopyTo(s2);
                    }
            }
            catch (Exception e) { HandleException(file, option, $"Exception: {e.Message}"); }
            return Task.CompletedTask;
        }

        #endregion
    }
}