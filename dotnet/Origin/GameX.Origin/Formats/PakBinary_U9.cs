using GameX.Origin.Formats.U9;
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace GameX.Origin.Formats
{
    public unsafe class PakBinary_U9 : PakBinary<PakBinary_U9>
    {
        #region Factories

        public static (FileOption, Func<BinaryReader, FileSource, PakFile, Task<object>>) ObjectFactoryFactory(FileSource source, FamilyGame game)
            => source.Path.ToLowerInvariant() switch
            {
                //"abc" => (0, Binary_Palette.Factory),
                _ => Path.GetExtension(source.Path).ToLowerInvariant() switch
                {
                    ".pal" => (0, Binary_Palette.Factory),
                    ".sfm" => (0, Binary_Music.Factory),
                    ".sfx" => (0, Binary_Sfx.Factory),
                    ".spk" => (0, Binary_Speech.Factory),
                    ".anim" => (0, Binary_Anim.Factory),
                    ".bmp" => (0, Binary_Bitmap.Factory),
                    ".book" => (0, Binary_Book.Factory),
                    ".str" => (0, Binary_Text.Factory),
                    ".mesh" => (0, Binary_Mesh.Factory),
                    ".tex" => (0, Binary_Texture.Factory),
                    ".type" => (0, Binary_Typename.Factory),
                    _ => (0, null),
                }
            };

        #endregion

        #region Headers
        // http://wiki.ultimacodex.com/wiki/Ultima_IX_Internal_Formats#FLX_Format

        [StructLayout(LayoutKind.Sequential, Pack = 0x1)]
        struct FlxRecord
        {
            public static (string, int) Struct = ("<2I", sizeof(FlxRecord));
            public uint Offset;     // Offset of the record from the start of the file, or 0 if there is no record in this entry.
            public uint FileSize;   // Length of the record in bytes.
        }

        #endregion

        static (string name, string ext)[] NameToExts = {
            ("music", ".sfm"),
            ("sfx", ".sfx"),
            ("speech", ".spk"),
            ("anim", ".anim"),
            ("bitmap", ".bmp"),
            ("book", ".book"),
            ("texture", ".tex"),
            ("text", ".str"),
            ("sappear", ".mesh"),
            ("typename", ".type"),
        };

        public override Task Read(BinaryPakFile source, BinaryReader r, object tag)
        {
            var fileName = Path.GetFileName(source.PakPath).ToLowerInvariant();
            var nameToExt = NameToExts.FirstOrDefault(x => fileName.Contains(x.name));
            var ext = nameToExt.ext ?? ".dat";

            source.Tag = Path.GetFileNameWithoutExtension(fileName)[^1];

            // read header
            r.Seek(0x50);
            var numFiles = r.ReadInt32();
            r.Seek(0x80);

            // read files
            var i = 0;
            source.Files = r.ReadSArray<FlxRecord>(numFiles).Select(s => new FileSource
            {
                Path = $"file{i++:x4}{ext}",
                FileSize = s.FileSize,
                Offset = s.Offset,
            }).Where(x => x.Offset != 0).ToList();
            return Task.CompletedTask;
        }

        public override Task<Stream> ReadData(BinaryPakFile source, BinaryReader r, FileSource file, FileOption option = default)
        {
            r.Seek(file.Offset);
            return Task.FromResult((Stream)new MemoryStream(r.ReadBytes((int)file.FileSize)));
        }
    }
}