using GameSpec.Formats;
using System;
using System.IO;
using System.Threading.Tasks;

namespace GameSpec.Cry.Formats
{
    /// <summary>
    /// FormatExtensions
    /// </summary>
    public static class FormatExtensions
    {
        // object factory
        public static (DataOption, Func<BinaryReader, FileMetadata, PakFile, Task<object>>) GetObjectFactoryFactory(this FileMetadata source, FamilyGame game)
            => Path.GetExtension(source.Path).ToLowerInvariant() switch
            {
                ".xml" => (0, CryXmlFile.Factory),
                ".dds" => (0, BinaryDds.Factory),
                ".cgf" or ".cga" or ".chr" or ".skin" or ".anim" => (0, CryFile.Factory),
                _ => (0, null),
            };
    }
}
