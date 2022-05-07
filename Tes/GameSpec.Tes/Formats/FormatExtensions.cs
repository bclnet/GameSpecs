using GameSpec.Formats;
using System;
using System.IO;
using System.Threading.Tasks;

namespace GameSpec.Tes.Formats
{
    /// <summary>
    /// FormatExtensions
    /// </summary>
    public static class FormatExtensions
    {
        // object factory
        internal static (DataOption, Func<BinaryReader, FileMetadata, PakFile, Task<object>>) GetObjectFactoryFactory(this FileMetadata source)
        {
            Task<object> NiFactory(BinaryReader r, FileMetadata f, PakFile s) { var file = new NiFile(Path.GetFileNameWithoutExtension(f.Path)); file.Read(r); return Task.FromResult((object)file); }
            return Path.GetExtension(source.Path).ToLowerInvariant() switch
            {
                ".dds" => (0, BinaryDds.Factory),
                ".nif" => (0, NiFactory),
                _ => (0, null),
            };
        }
    }
}