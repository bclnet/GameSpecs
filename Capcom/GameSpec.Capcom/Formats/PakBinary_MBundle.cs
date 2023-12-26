using GameSpec.Formats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace GameSpec.Capcom.Formats
{
    public unsafe class PakBinary_MBundle : PakBinary
    {
        public static readonly PakBinary Instance = new PakBinary_MBundle();

        // Header : F1
        #region Header : F1
        // https://github.com/darkxex/Rune-Factory-4-Special-Mbundle-Extractor/blob/main/mbundle%20extractor/Program.cs

        const ulong B_MAGIC = 0x30307473696c7062;

        [StructLayout(LayoutKind.Sequential, Pack = 0x1)]
        struct M_Header
        {
            public static string Map = "B8B8";
            public long FirstTable; // XX
            public long SecondTable; // XX
        }

        #endregion
        // https://medium.com/@karaiskc/understanding-apples-binary-property-list-format-281e6da00dbd
        public override Task ReadAsync(BinaryPakFile source, BinaryReader r, object tag)
        {
            var header = r.ReadUInt64();
            if (header != B_MAGIC) throw new FormatException("Bad Magic");
            var maker = r.ReadByte();

            
            var files = source.Files = new List<FileSource>();
            return Task.CompletedTask;
        }

        public override Task<Stream> ReadDataAsync(BinaryPakFile source, BinaryReader r, FileSource file, DataOption option = 0, Action<FileSource, string> exception = null)
        {
            throw new NotImplementedException();
        }
    }
}