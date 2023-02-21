using GameSpec.Arkane.Formats.Danae;
using GameSpec.Formats;
using System;
using System.IO;
using System.Threading.Tasks;

namespace GameSpec.Arkane.Formats
{
    /// <summary>
    /// FormatExtensions
    /// </summary>
    public static class FormatExtensions
    {
        // object factory
        internal static (DataOption, Func<BinaryReader, FileMetadata, PakFile, Task<object>>) GetObjectFactoryFactory(this FileMetadata source, FamilyGame game)
            => Path.GetExtension(source.Path).ToLowerInvariant() switch
            {
                ".txt" or ".ini" or ".asl" => (0, BinaryTxt.Factory),
                ".wav" => (0, BinarySnd.Factory),
                ".bmp" or ".jpg" or ".tga" => (0, BinaryImg.Factory),
                ".dds" => (0, BinaryDds.Factory),
                // Danae (AF)
                ".ftl" => (0, BinaryFtl.Factory),
                ".fts" => (0, BinaryFts.Factory),
                ".tea" => (0, BinaryTea.Factory),
                //
                //".llf" => (0, BinaryFlt.Factory),
                //".dlf" => (0, BinaryFlt.Factory),
                _ => (0, null),
            };
    }
}