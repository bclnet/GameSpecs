using GameSpec.Formats;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace GameSpec.Origin.Formats
{
    public unsafe class PakBinaryOriginU9 : PakBinary
    {
        public static readonly PakBinary Instance = new PakBinaryOriginU9();
        PakBinaryOriginU9() { }

        // Headers
        #region Headers
        // http://wiki.ultimacodex.com/wiki/Ultima_IX_Internal_Formats#FLX_Format

        [StructLayout(LayoutKind.Sequential, Pack = 0x1)]
        struct FLX_HeaderFile
        {
            public uint Position;
            public uint FileSize;
        }

        #endregion

        public override Task ReadAsync(BinaryPakFile source, BinaryReader r, ReadStage stage)
        {
            if (!(source is BinaryPakManyFile multiSource)) throw new NotSupportedException();
            if (stage != ReadStage.File) throw new ArgumentOutOfRangeException(nameof(stage), stage.ToString());

            var fileName = Path.GetFileNameWithoutExtension(source.FilePath).ToLowerInvariant();
            var prefix
                = fileName.Contains("bitmap") ? "bitmap"
                : fileName.Contains("texture") ? "texture"
                : fileName.Contains("sdinfo") ? "sdinfo"
                : fileName;
            r.Position(0x50);
            var numFiles = r.ReadInt32();
            r.Position(0x80);
            var headerFiles = r.ReadTArray<FLX_HeaderFile>(sizeof(FLX_HeaderFile), numFiles);
            var files = multiSource.Files = new FileMetadata[numFiles];
            for (var i = 0; i < files.Count; i++)
            {
                var headerFile = headerFiles[i];
                files[i] = new FileMetadata
                {
                    Path = $"{prefix}/{i}",
                    FileSize = headerFile.FileSize,
                    Position = headerFile.Position,
                };
            }
            return Task.CompletedTask;
        }

        public override Task<Stream> ReadDataAsync(BinaryPakFile source, BinaryReader r, FileMetadata file, DataOption option = 0, Action<FileMetadata, string> exception = null)
        {
            r.Position(file.Position);
            return Task.FromResult((Stream)new MemoryStream(r.ReadBytes((int)file.FileSize)));
        }
    }
}