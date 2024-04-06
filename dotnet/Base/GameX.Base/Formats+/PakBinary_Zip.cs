using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using static OpenStack.Debug;
using ZipFile = ICSharpCode.SharpZipLib.Zip.ZipFile;

namespace GameX.Formats
{
    /// <summary>
    /// PakBinary_Zip
    /// </summary>
    /// <seealso cref="GameEstate.Formats.PakBinary" />
    public class PakBinary_Zip : PakBinary
    {
        static readonly PropertyInfo ZipFile_KeyProperty = typeof(ZipFile).GetProperty("Key", BindingFlags.NonPublic | BindingFlags.Instance);

        static readonly PakBinary Instance = new PakBinary_Zip();
        static readonly ConcurrentDictionary<object, PakBinary> PakBinarys = new ConcurrentDictionary<object, PakBinary>();
        public static PakBinary GetPakBinary(FamilyGame game) => game.Key == null ? Instance : PakBinarys.GetOrAdd(game.Key, x => new PakBinary_Zip(x));

        readonly object Key;
        bool UseSystem => Key == null;

        public PakBinary_Zip(object key = null) => Key = key;

        public override Task Read(BinaryPakFile source, BinaryReader r, object tag)
        {
            source.UseReader = false;
            if (UseSystem)
            {
                ZipArchive pak;
                source.Tag = pak = new ZipArchive(r.BaseStream, ZipArchiveMode.Read);
                source.Files = pak.Entries.Select(s => new FileSource
                {
                    Path = s.Name.Replace('\\', '/'),
                    PackedSize = s.CompressedLength,
                    FileSize = s.Length,
                    Tag = s
                }).ToArray();
            }
            else
            {
                ZipFile pak;
                source.Tag = pak = new ZipFile(r.BaseStream);
                if (Key == null) { }
                else if (Key is string s) pak.Password = s;
                else if (Key is byte[] z) ZipFile_KeyProperty.SetValue(pak, z);
                source.Files = pak.Cast<ZipEntry>().Where(x => x.IsFile).Select(s => new FileSource
                {
                    Path = s.Name.Replace('\\', '/'),
                    Crypted = s.IsCrypted,
                    PackedSize = s.CompressedSize,
                    FileSize = s.Size,
                    Tag = s,
                }).ToArray();
            }
            return Task.CompletedTask;
        }

        public override Task<Stream> ReadData(BinaryPakFile source, BinaryReader r, FileSource file, FileOption option = default)
        {
            try
            {
                using var input = UseSystem
                    ? ((ZipArchiveEntry)file.Tag).Open()
                    : ((ZipFile)source.Tag).GetInputStream((ZipEntry)file.Tag);
                if (!input.CanRead) { HandleException(file, option, $"Unable to read stream for file: {file.Path}"); return Task.FromResult(System.IO.Stream.Null); }
                var s = new MemoryStream();
                input.CopyTo(s);
                s.Position = 0;
                return Task.FromResult((Stream)s);
            }
            catch (Exception e) { HandleException(file, option, $"{file.Path} - Exception: {e.Message}"); return Task.FromResult(System.IO.Stream.Null); }
        }

        //public override Task WriteAsync(BinaryPakFile source, BinaryWriter w, object tag)
        //{
        //    source.UseReader = false;
        //    var files = source.Files;
        //    var pak = (ZipFile)(source.Tag = new ZipFile(w.BaseStream));
        //    ZipFile_KeyProperty.SetValue(pak, Key);
        //    pak.BeginUpdate();
        //    foreach (var file in files)
        //    {
        //        var entry = (ZipEntry)(file.Tag = new ZipEntry(Path.GetFileName(file.Path)));
        //        pak.Add(entry);
        //        source.PakBinary.WriteDataAsync(source, w, file, null, 0, null);
        //    }
        //    pak.CommitUpdate();
        //    return Task.CompletedTask;
        //}
        //public override Task WriteDataAsync(BinaryPakFile source, BinaryWriter w, FileSource file, Stream data, DataOption option = 0, Action<FileSource, string> exception = null)
        //{
        //    var pak = (ZipFile)source.Tag;
        //    var entry = (ZipEntry)file.Tag;
        //    try
        //    {
        //        using var s = pak.GetInputStream(entry);
        //        data.CopyTo(s);
        //    }
        //    catch (Exception e) { exception?.Invoke(file, $"Exception: {e.Message}"); }
        //    return Task.CompletedTask;
        //}
    }
}