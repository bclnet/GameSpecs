using GameX.Formats;
using GameX.Meta;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GameX.Valve.Formats.Extras
{
    public class ToolsAssetInfo : IHaveMetaInfo
    {
        public const uint MAGIC = 0xC4CCACE8;
        public const uint MAGIC2 = 0xC4CCACE9;
        public const uint GUARD = 0x049A48B2;

        public ToolsAssetInfo() { }
        public ToolsAssetInfo(BinaryReader r) => Read(r);

        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag) => new List<MetaInfo> {
            new MetaInfo(null, new MetaContent { Type = "Text", Name = "Text", Value = ToString() }),
            new MetaInfo("ToolsAssetInfo", items: new List<MetaInfo> {
                new MetaInfo($"Mods: {Mods.Count}"),
                new MetaInfo($"Directories: {Directories.Count}"),
                new MetaInfo($"Filenames: {Filenames.Count}"),
                new MetaInfo($"Extensions: {Extensions.Count}"),
                new MetaInfo($"EditInfoKeys: {EditInfoKeys.Count}"),
                new MetaInfo($"MiscStrings: {MiscStrings.Count}"),
                new MetaInfo($"ConstructedFilepaths: {ConstructedFilepaths.Count}"),
            }),
        };

        public readonly List<string> Mods = new List<string>();
        public readonly List<string> Directories = new List<string>();
        public readonly List<string> Filenames = new List<string>();
        public readonly List<string> Extensions = new List<string>();
        public readonly List<string> EditInfoKeys = new List<string>();
        public readonly List<string> MiscStrings = new List<string>();
        public readonly List<string> ConstructedFilepaths = new List<string>();
        public readonly List<string> UnknownSoundField1 = new List<string>();
        public readonly List<string> UnknownSoundField2 = new List<string>();

        public void Read(BinaryReader r)
        {
            var magic = r.ReadUInt32();
            if (magic != MAGIC && magic != MAGIC2) throw new InvalidDataException("Given file is not tools_asset_info.");

            var version = r.ReadUInt32();
            if (version < 9 || version > 13) throw new InvalidDataException($"Unsupported version: {version}");
            var fileCount = r.ReadUInt32();
            if (r.ReadUInt32() != 1) throw new InvalidDataException($"Invalid blockId");

            ReadStringsBlock(r, Mods);
            ReadStringsBlock(r, Directories);
            ReadStringsBlock(r, Filenames);
            ReadStringsBlock(r, Extensions);
            ReadStringsBlock(r, EditInfoKeys);
            ReadStringsBlock(r, MiscStrings);
            if (version >= 12)
            {
                ReadStringsBlock(r, UnknownSoundField1);
                ReadStringsBlock(r, UnknownSoundField2);
            }

            for (var i = 0; i < fileCount; i++)
            {
                var hash = r.ReadUInt64();
                var unk1 = (int)(hash >> 61) & 7;
                var addonIndex = (int)(hash >> 52) & 0x1FF;
                var directoryIndex = (int)(hash >> 33) & 0x7FFFF;
                var filenameIndex = (int)(hash >> 10) & 0x7FFFFF;
                var extensionIndex = (int)(hash & 0x3FF);
                //Console.WriteLine($"{unk1} {addonIndex} {directoryIndex} {filenameIndex} {extensionIndex}");
                var path = new StringBuilder();
                if (addonIndex != 0x1FF) { path.Append(Mods[addonIndex]); path.Append('/'); }
                if (directoryIndex != 0x7FFFF) { path.Append(Directories[directoryIndex]); path.Append('/'); }
                if (filenameIndex != 0x7FFFFF) { path.Append(Filenames[filenameIndex]); }
                if (extensionIndex != 0x3FF) { path.Append('.'); path.Append(Extensions[extensionIndex]); }
                ConstructedFilepaths.Add(path.ToString());
            }
        }

        static void ReadStringsBlock(BinaryReader r, ICollection<string> output)
        {
            var count = r.ReadUInt32();
            for (var i = 0U; i < count; i++) output.Add(r.ReadZUTF8());
        }

        public override string ToString()
        {
            var b = new StringBuilder();
            foreach (var str in ConstructedFilepaths) b.AppendLine(str);
            return b.ToString();
        }
    }
}
