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
        internal static (FileOption, Func<BinaryReader, FileSource, PakFile, Task<object>>) GetObjectFactoryFactory(this FileSource source, FamilyGame game)
            => Path.GetExtension(source.Path).ToLowerInvariant() switch
            {
                var x when x == ".txt" || x == ".ini" || x == ".asl" => (0, BinaryTxt.Factory),
                ".wav" => (0, BinarySnd.Factory),
                var x when x == ".bmp" || x == ".jpg" || x == ".tga" => (0, BinaryImg.Factory),
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