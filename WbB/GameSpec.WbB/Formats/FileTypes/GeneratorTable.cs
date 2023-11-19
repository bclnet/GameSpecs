using GameSpec.WbB.Formats.Entity;
using GameSpec.Metadata;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GameSpec.WbB.Formats.FileTypes
{
    /// <summary>
    /// Class for reading the File 0x0E00000D from the portal.dat.
    /// Thanks alot Widgeon of Leafcull for his WbBDataTools which helped understanding this structure.
    /// And thanks alot to Pea as well whos hard work surely helped in the creation of those Tools too.
    /// </summary>
    [PakFileType(PakFileType.ObjectHierarchy)]
    public class GeneratorTable : FileType, IGetMetadataInfo
    {
        public const uint FILE_ID = 0x0E00000D;

        public readonly Generator Generators;
        /// <summary>
        /// This is just a shortcut to Generators.Items[0].Items
        /// </summary>
        public readonly Generator[] PlayDayItems;
        /// <summary>
        /// This is just a shortcut to Generators.Items[1].Items
        /// </summary>
        public readonly Generator[] WeenieObjectsItems;

        public GeneratorTable(BinaryReader r)
        {
            Id = r.ReadUInt32();
            Generators = new Generator(r);
            PlayDayItems = Generators.Items[0].Items;
            WeenieObjectsItems = Generators.Items[1].Items;
        }

        //: FileTypes.GeneratorTable
        List<MetadataInfo> IGetMetadataInfo.GetInfoNodes(MetadataManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<MetadataInfo> {
                new MetadataInfo($"{nameof(GeneratorTable)}: {Id:X8}", items: new List<MetadataInfo> {
                    new MetadataInfo("Generators", items: (Generators as IGetMetadataInfo).GetInfoNodes(tag: tag)),
                    new MetadataInfo("PlayDayItems", items: PlayDayItems.Select(x => new MetadataInfo(x.Id != 0 ? $"{x.Id} - {x.Name}" : $"{x.Name}", items: (x as IGetMetadataInfo).GetInfoNodes(tag: tag)))),
                    new MetadataInfo("WeenieObjectsItems", items: WeenieObjectsItems.Select(x => new MetadataInfo(x.Id != 0 ? $"{x.Id} - {x.Name}" : $"{x.Name}", items: (x as IGetMetadataInfo).GetInfoNodes(tag: tag)))),
                })
            };
            return nodes;
        }
    }
}
