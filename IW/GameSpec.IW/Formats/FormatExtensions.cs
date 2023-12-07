using GameSpec.Formats;
using System;
using System.IO;
using System.Threading.Tasks;

namespace GameSpec.IW.Formats
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
                var x when x == ".cfg" || x == ".csv" || x == ".txt" => (0, BinaryTxt.Factory),
                //".roq" => (0, VIDEO.Factory),
                //".wav" => (0, BinaryWav.Factory),
                //".d3dbsp" => (0, BinaryD3dbsp.Factory),
                ".iwi" => (0, BinaryIwi.Factory),
                _ => (0, null),
            };
    }
}