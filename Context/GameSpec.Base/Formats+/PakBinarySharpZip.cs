using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using static OpenStack.Debug;

namespace GameSpec.Formats
{
    /// <summary>
    /// PakBinarySharpZip
    /// </summary>
    /// <seealso cref="GameEstate.Formats.PakBinary" />
    public class PakBinarySharpZip : PakBinary
    {
        static readonly PropertyInfo ZipFile_KeyProperty = typeof(ZipFile).GetProperty("Key", BindingFlags.NonPublic | BindingFlags.Instance);
        public static readonly PakBinary Instance = new PakBinarySharpZip();
        readonly byte[] Key;

        public PakBinarySharpZip(Family.ByteKey key = null) => Key = key?.Key;

        public override Task ReadAsync(BinaryPakFile source, BinaryReader r, object tag)
        {
            if (!(source is BinaryPakManyFile multiSource)) throw new NotSupportedException();

            source.UseBinaryReader = false;
            var files = multiSource.Files = new List<FileSource>();
            var pak = (ZipFile)(source.Tag = new ZipFile(r.BaseStream));
            ZipFile_KeyProperty.SetValue(pak, Key);
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
                files.Add(metadata);
            }
            return Task.CompletedTask;
        }

        public override Task WriteAsync(BinaryPakFile source, BinaryWriter w, object tag)
        {
            if (!(source is BinaryPakManyFile multiSource)) throw new NotSupportedException();

            source.UseBinaryReader = false;
            var files = multiSource.Files;
            var pak = (ZipFile)(source.Tag = new ZipFile(w.BaseStream));
            ZipFile_KeyProperty.SetValue(pak, Key);
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

        public override Task<Stream> ReadDataAsync(BinaryPakFile source, BinaryReader r, FileSource file, DataOption option = 0, Action<FileSource, string> exception = null)
        {
            var pak = (ZipFile)source.Tag;
            var entry = (ZipEntry)file.Tag;
            try
            {
                using var input = pak.GetInputStream(entry);
                if (!input.CanRead) { Log($"Unable to read stream for file: {file.Path}"); exception?.Invoke(file, $"Unable to read stream for file: {file.Path}"); return Task.FromResult(System.IO.Stream.Null); }
                var s = new MemoryStream();
                input.CopyTo(s);
                s.Position = 0;
                return Task.FromResult((Stream)s);
            }
            catch (Exception e) { Log($"{file.Path} - Exception: {e.Message}"); exception?.Invoke(file, $"{file.Path} - Exception: {e.Message}"); return Task.FromResult(System.IO.Stream.Null); }
        }

        public override Task WriteDataAsync(BinaryPakFile source, BinaryWriter w, FileSource file, Stream data, DataOption option = 0, Action<FileSource, string> exception = null)
        {
            var pak = (ZipFile)source.Tag;
            var entry = (ZipEntry)file.Tag;
            try
            {
                using var s = pak.GetInputStream(entry);
                data.CopyTo(s);
            }
            catch (Exception e) { exception?.Invoke(file, $"Exception: {e.Message}"); }
            return Task.CompletedTask;
        }
    }
}