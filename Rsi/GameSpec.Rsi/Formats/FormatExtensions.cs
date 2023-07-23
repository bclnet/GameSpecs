using GameSpec.Cry.Formats;
using GameSpec.Formats;
using System;
using System.IO;
using System.Threading.Tasks;

namespace GameSpec.Rsi.Formats
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
                ".cfg" or ".txt" => (0, BinaryTxt.Factory),
                ".mtl" or ".xml" => (DataOption.Stream, CryXmlFile.Factory),
                ".dds" => (0, BinaryDds.Factory),
                ".a" => (0, BinaryDdsA.Factory),
                ".dcb" => (0, ForgeFile.Factory),
                ".soc" or ".cgf" or ".cga" or ".chr" or ".skin" or ".anim" => (DataOption.Model, CryFile.Factory),
                _ => (0, null),
            };
    }
}