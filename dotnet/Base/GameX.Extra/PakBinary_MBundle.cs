using GameX.Formats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace GameX.Capcom.Formats
{
    public unsafe class PakBinary_MBundle : PakBinary
    {
        public static readonly PakBinary Instance = new PakBinary_MBundle();

        // Header : F1
        #region Header : F1
        // https://github.com/darkxex/Rune-Factory-4-Special-Mbundle-Extractor/blob/main/mbundle%20extractor/Program.cs

        [StructLayout(LayoutKind.Sequential, Pack = 0x1)]
        struct M_Header
        {
            public static string Map = "B8B8";
            public long FirstTable; // XX
            public long SecondTable; // XX
        }

        #endregion

        public override Task ReadAsync(BinaryPakFile source, BinaryReader r, object tag)
        {
            r.Seek(r.BaseStream.Length - sizeof(M_Header));
            var header = r.ReadTE<M_Header>(sizeof(M_Header), M_Header.Map);
            r.Seek(header.FirstTable + 10);
            var numFiles = r.ReadInt32E();
            var magic = r.ReadChars(8);

            var files = source.Files = new List<FileSource>();
            return Task.CompletedTask;
        }

        public override Task<Stream> ReadDataAsync(BinaryPakFile source, BinaryReader r, FileSource file, DataOption option = 0, Action<FileSource, string> exception = null)
        {
            throw new NotImplementedException();
        }
    }
}