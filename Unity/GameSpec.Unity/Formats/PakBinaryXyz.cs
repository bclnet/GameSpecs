using GameSpec.Formats;
using System;
using System.IO;
using System.Threading.Tasks;

namespace GameSpec.Unity.Formats
{
    /// <summary>
    /// PakBinaryXyz
    /// </summary>
    /// <seealso cref="GameSpec.Formats.PakBinary" />
    public class PakBinaryXyz : PakBinary
    {
        public static readonly PakBinary Instance = new PakBinaryXyz();
        
        readonly byte[] Key;

        public PakBinaryXyz(byte[] key = null) => Key = key;

        public override Task ReadAsync(BinaryPakFile source, BinaryReader r, ReadStage stage)
        {
            if (!(source is BinaryPakManyFile multiSource)) throw new NotSupportedException();
            if (stage != ReadStage.File) throw new ArgumentOutOfRangeException(nameof(stage), stage.ToString());

            //source.UseBinaryReader = false;
            //var files = multiSource.Files = new List<FileMetadata>();
            //var pak = (P4kFile)(source.Tag = new P4kFile(r.BaseStream) { Key = Key });
            //foreach (ZipEntry entry in pak)
            //{
            //    var metadata = new FileMetadata
            //    {
            //        Path = entry.Name.Replace('\\', '/'),
            //        Crypted = entry.IsCrypted,
            //        PackedSize = entry.CompressedSize,
            //        FileSize = entry.Size,
            //        Tag = entry,
            //    };
            //    files.Add(metadata);
            //}
            return Task.CompletedTask;
        }

        public override Task WriteAsync(BinaryPakFile source, BinaryWriter w, WriteStage stage)
        {
            if (!(source is BinaryPakManyFile multiSource)) throw new NotSupportedException();

            //source.UseBinaryReader = false;
            //var files = multiSource.Files;
            //var pak = (P4kFile)(source.Tag = new P4kFile(w.BaseStream) { Key = Key });
            //pak.BeginUpdate();
            //foreach (var file in files)
            //{
            //    var entry = (ZipEntry)(file.Tag = new ZipEntry(Path.GetFileName(file.Path)));
            //    pak.Add(entry);
            //    source.PakBinary.WriteDataAsync(source, w, file, null, null);
            //}
            //pak.CommitUpdate();
            return Task.CompletedTask;
        }

        public override Task<Stream> ReadDataAsync(BinaryPakFile source, BinaryReader r, FileMetadata file, DataOption option = 0, Action<FileMetadata, string> exception = null)
        {
            //var pak = (P4kFile)source.Tag;
            //var entry = (ZipEntry)file.Tag;
            //try
            //{
            //    using var input = pak.GetInputStream(entry);
            //    if (!input.CanRead) { Log($"Unable to read stream for file: {file.Path}"); exception?.Invoke(file, $"Unable to read stream for file: {file.Path}"); return Task.FromResult(System.IO.Stream.Null); }
            //    var s = new MemoryStream();
            //    input.CopyTo(s);
            //    s.Position = 0;
            //    return Task.FromResult((Stream)s);
            //}
            //catch (Exception e) { Log($"{file.Path} - Exception: {e.Message}"); exception?.Invoke(file, $"{file.Path} - Exception: {e.Message}"); return Task.FromResult(System.IO.Stream.Null); }
            return null;
        }

        public override Task WriteDataAsync(BinaryPakFile source, BinaryWriter w, FileMetadata file, Stream data, DataOption option = 0, Action<FileMetadata, string> exception = null)
        {
            //var pak = (P4kFile)source.Tag;
            //var entry = (ZipEntry)file.Tag;
            //try
            //{
            //    using var s = pak.GetInputStream(entry);
            //    data.CopyTo(s);
            //}
            //catch (Exception e) { exception?.Invoke(file, $"Exception: {e.Message}"); }
            return Task.CompletedTask;
        }
    }
}