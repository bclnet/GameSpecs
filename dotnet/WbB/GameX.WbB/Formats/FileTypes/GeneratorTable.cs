using GameX.WbB.Formats.Entity;
using GameX.Meta;
using GameX.Formats;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GameX.WbB.Formats.FileTypes
{
    /// <summary>
    /// Class for reading the File 0x0E00000D from the portal.dat.
    /// Thanks alot Widgeon of Leafcull for his ACDataTools which helped understanding this structure.
    /// And thanks alot to Pea as well whos hard work surely helped in the creation of those Tools too.
    /// </summary>
    [PakFileType(PakFileType.ObjectHierarchy)]
    public class GeneratorTable : FileType, IHaveMetaInfo
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
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                new MetaInfo($"{nameof(GeneratorTable)}: {Id:X8}", items: new List<MetaInfo> {
                    new MetaInfo("Generators", items: (Generators as IHaveMetaInfo).GetInfoNodes(tag: tag)),
                    new MetaInfo("PlayDayItems", items: PlayDayItems.Select(x => new MetaInfo(x.Id != 0 ? $"{x.Id} - {x.Name}" : $"{x.Name}", items: (x as IHaveMetaInfo).GetInfoNodes(tag: tag)))),
                    new MetaInfo("WeenieObjectsItems", items: WeenieObjectsItems.Select(x => new MetaInfo(x.Id != 0 ? $"{x.Id} - {x.Name}" : $"{x.Name}", items: (x as IHaveMetaInfo).GetInfoNodes(tag: tag)))),
                })
            };
            return nodes;
        }
    }
}
