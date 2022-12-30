using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
//using static GameSpec.Estate;

namespace GameSpec.Formats
{
    [DebuggerDisplay("{Path}")]
    public class FileMetadata
    {
        internal static readonly Func<BinaryReader, FileMetadata, PakFile, Task<object>> EmptyObjectFactory = (a, b, c) => null;
        internal Func<BinaryReader, FileMetadata, PakFile, Task<object>> CachedObjectFactory;
        internal DataOption CachedDataOption;
        public BinaryPakFile Pak;
        public object Tag;
        // common
        public int Id;
        public string Path;
        public int Compressed;
        public bool Crypted;
        public long PackedSize;
        public long FileSize;
        public long Position;
        public ulong Digest;
        // options
        public IList<FileMetadata> Parts;
        // extra
        public object FileInfo;
        public byte[] Extra;
        public object ExtraArgs;
    }
}