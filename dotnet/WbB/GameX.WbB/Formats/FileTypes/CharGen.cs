using GameX.Meta;
using GameX.WbB.Formats.Entity;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GameX.Formats;

namespace GameX.WbB.Formats.FileTypes
{
    [PakFileType(PakFileType.CharacterGenerator)]
    public class CharGen : FileType, IHaveMetaInfo
    {
        public const uint FILE_ID = 0x0E000002;

        public readonly StarterArea[] StarterAreas;
        public readonly IDictionary<uint, HeritageGroupCG> HeritageGroups;

        public CharGen(BinaryReader r)
        {
            Id = r.ReadUInt32();
            r.Skip(4);
            StarterAreas = r.ReadC32FArray(x => new StarterArea(x));
            // HERITAGE GROUPS -- 11 standard player races and 2 Olthoi.
            r.Skip(1); // Not sure what this byte 0x01 is indicating, but we'll skip it because we can.
            HeritageGroups = r.ReadC32TMany<uint, HeritageGroupCG>(sizeof(uint), x => new HeritageGroupCG(x));
        }

        //: FileTypes.CharGen
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                new MetaInfo($"{nameof(CharGen)}: {Id:X8}", items: new List<MetaInfo> {
                    new MetaInfo("Starter Areas", items: StarterAreas.Select(x => {
                        var items = (x as IHaveMetaInfo).GetInfoNodes();
                        var name = items[0].Name.Replace("Name: ", "");
                        items.RemoveAt(0);
                        return new MetaInfo(name, items: items);
                    })),
                    new MetaInfo("Heritage Groups", items: HeritageGroups.Select(x => {
                        var items = (x.Value as IHaveMetaInfo).GetInfoNodes();
                        var name = items[0].Name.Replace("Name: ", "");
                        items.RemoveAt(0);
                        return new MetaInfo(name, items: items);
                    })),
                })
            };
            return nodes;
        }
    }
}
