using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace GameX
{
    [DebuggerDisplay("{Path}")]
    public class FileSource
    {
        internal static readonly Func<BinaryReader, FileSource, PakFile, Task<object>> EmptyObjectFactory = (a, b, c) => null;
        
        // common
        public int Id;
        public string Path;
        public int Compressed;
        public long Offset;
        public long FileSize;
        public long PackedSize;
        public bool Crypted;
        public ulong Hash;
        public BinaryPakFile Pak;
        public IList<FileSource> Parts;
        public object Tag;
        // extra
        public object FileInfo;
        public byte[] Extra;
        public object ExtraArgs;
        // cached
        internal Func<BinaryReader, FileSource, PakFile, Task<object>> CachedObjectFactory;
        internal FileOption CachedObjectOption;
    }
}