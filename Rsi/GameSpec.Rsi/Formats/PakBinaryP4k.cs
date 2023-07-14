using GameSpec.Formats;
using ICSharpCode.SharpZipLib.Tar;
using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static OpenStack.Debug;

namespace GameSpec.Rsi.Formats
{
    /// <summary>
    /// PakBinaryP4k
    /// </summary>
    /// <seealso cref="GameSpec.Formats.PakBinary" />
    public unsafe class PakBinaryP4k : PakBinary
    {
        public static readonly PakBinary Instance = new PakBinaryP4k();
        static readonly byte[] DefaultKey = new byte[] { 0x5E, 0x7A, 0x20, 0x02, 0x30, 0x2E, 0xEB, 0x1A, 0x3B, 0xB6, 0x17, 0xC3, 0x0F, 0xDE, 0x1E, 0x47 };
        readonly byte[] Key;

        class SubPakFile : BinaryPakManyFile
        {
            public SubPakFile(FamilyGame game, string filePath, object tag = null) : base(game, filePath, Instance, tag) => Open();
        }

        PakBinaryP4k() => Key = DefaultKey;

        public override Task ReadAsync(BinaryPakFile source, BinaryReader r, ReadStage stage)
        {
            if (source is not BinaryPakManyFile multiSource) throw new NotSupportedException();
            if (stage != ReadStage.File) throw new ArgumentOutOfRangeException(nameof(stage), stage.ToString());
            source.UseBinaryReader = false;
            var files = multiSource.Files = new List<FileMetadata>();

            var pak = (P4kFile)(source.Tag = new P4kFile(r.BaseStream, Key));
            var parentByPath = new Dictionary<string, FileMetadata>();
            var partsByPath = new Dictionary<string, SortedList<string, FileMetadata>>();
            foreach (ZipEntry entry in pak)
            {
                var metadata = new FileMetadata
                {
                    Path = entry.Name.Replace('\\', '/'),
                    Crypted = entry.IsCrypted,
                    PackedSize = entry.CompressedSize,
                    FileSize = entry.Size,
                    Tag = entry,
                };
                if (metadata.Path.EndsWith(".pak", StringComparison.OrdinalIgnoreCase) || metadata.Path.EndsWith(".socpak", StringComparison.OrdinalIgnoreCase)) { } // metadata.Pak = new SubPakFile(source.Game, metadata.Path);
                else if (metadata.Path.EndsWith(".dds", StringComparison.OrdinalIgnoreCase) || metadata.Path.EndsWith(".dds.a", StringComparison.OrdinalIgnoreCase)) parentByPath.Add(metadata.Path, metadata);
                else if (metadata.Path[^8..].Contains(".dds.", StringComparison.OrdinalIgnoreCase))
                {
                    var parentPath = metadata.Path[..(metadata.Path.IndexOf(".dds", StringComparison.OrdinalIgnoreCase) + 4)];
                    if (metadata.Path.EndsWith("a")) parentPath += ".a";
                    var parts = partsByPath.TryGetValue(parentPath, out var z) ? z : null;
                    if (parts == null) partsByPath.Add(parentPath, parts = new SortedList<string, FileMetadata>());
                    parts.Add(metadata.Path, metadata);
                    continue;
                }
                files.Add(metadata);
            }

            // process links
            if (partsByPath.Count > 0)
                foreach (var kv in partsByPath) if (parentByPath.TryGetValue(kv.Key, out var parent)) parent.Parts = kv.Value.Values;
            return Task.CompletedTask;
        }

        public override Task WriteAsync(BinaryPakFile source, BinaryWriter w, WriteStage stage)
        {
            if (source is not BinaryPakManyFile multiSource) throw new NotSupportedException();
            source.UseBinaryReader = false;
            var files = multiSource.Files;

            var pak = (P4kFile)(source.Tag = new P4kFile(w.BaseStream, Key));
            pak.BeginUpdate();
            foreach (var file in files)
            {
                var entry = (ZipEntry)(file.Tag = new ZipEntry(Path.GetFileName(file.Path)));
                pak.Add(entry);
                source.PakBinary.WriteDataAsync(source, w, file, null, 0, null);
            }
            pak.CommitUpdate();
            return Task.CompletedTask;
        }

        public override Task<Stream> ReadDataAsync(BinaryPakFile source, BinaryReader r, FileMetadata file, DataOption option = 0, Action<FileMetadata, string> exception = null)
        {
            var pak = (P4kFile)source.Tag;
            var entry = (ZipEntry)file.Tag;
            try
            {
                using var input = pak.GetInputStream(entry);
                if (!input.CanRead) { Log($"Unable to read stream for file: {file.Path}"); exception?.Invoke(file, $"Unable to read stream for file: {file.Path}"); return Task.FromResult(System.IO.Stream.Null); }
                var s = new MemoryStream();
                input.CopyTo(s);
                if (file.Parts != null)
                    foreach (var part in file.Parts.Reverse())
                    {
                        var entry2 = (ZipEntry)part.Tag;
                        using var input2 = pak.GetInputStream(entry2);
                        if (!input2.CanRead) { Log($"Unable to read stream for file: {file.Path}"); exception?.Invoke(file, $"Unable to read stream for file: {part.Path}"); return Task.FromResult(System.IO.Stream.Null); }
                        input2.CopyTo(s);
                    }
                s.Position = 0;
                return Task.FromResult((Stream)s);
            }
            catch (Exception e) { Log($"{file.Path} - Exception: {e.Message}"); exception?.Invoke(file, $"{file.Path} - Exception: {e.Message}"); return Task.FromResult(System.IO.Stream.Null); }
        }

        public override Task WriteDataAsync(BinaryPakFile source, BinaryWriter w, FileMetadata file, Stream data, DataOption option = 0, Action<FileMetadata, string> exception = null)
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
            catch (Exception e) { exception?.Invoke(file, $"Exception: {e.Message}"); }
            return Task.CompletedTask;
        }
    }
}