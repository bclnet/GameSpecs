using GameSpec.Formats;
using System;
using System.IO;
using System.Threading.Tasks;

namespace GameSpec.Crytek.Formats
{
    /// <summary>
    /// FormatExtensions
    /// </summary>
    public static class FormatExtensions
    {
        // object factory
        public static (FileOption, Func<BinaryReader, FileSource, PakFile, Task<object>>) GetObjectFactoryFactory(this FileSource source, FamilyGame game)
            => Path.GetExtension(source.Path).ToLowerInvariant() switch
            {
                ".xml" => (0, CryXmlFile.Factory),
                ".dds" => (0, BinaryDds.Factory),
                var x when x == ".cgf" || x == ".cga" || x == ".chr" || x == ".skin" || x == ".anim" => (0, CryFile.Factory),
                _ => (0, null),
            };
    }
}
