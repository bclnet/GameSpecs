using GameSpec.Formats;
using System;
using System.IO;
using System.Threading.Tasks;

namespace GameSpec.Black.Formats
{
    /// <summary>
    /// FormatExtensions
    /// </summary>
    public static class FormatExtensions
    {
        // object factory
        internal static (DataOption, Func<BinaryReader, FileSource, PakFile, Task<object>>) GetObjectFactoryFactory(this FileSource source, FamilyGame game)
            => Path.GetExtension(source.Path).ToLowerInvariant() switch
            {
                var x when x.StartsWith(".fr") => (0, BinaryFrm.Factory),
                ".pal" => (0, BinaryPal.Factory),
                ".rix" => (0, BinaryRix.Factory),
                _ => (0, null),
            };
    }
}