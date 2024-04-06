using GameX.WbB.Formats.Entity;
using GameX.WbB.Formats.Props;
using GameX.Meta;
using GameX.Formats;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GameX.WbB.Formats.FileTypes
{
    /// <summary>
    /// SoundTable files contain a listing of which Wav types to play in response to certain events.
    /// They are located in the client_portal.dat and are files starting with 0x20
    /// </summary>
    [PakFileType(PakFileType.SoundTable)]
    public class SoundTable : FileType, IHaveMetaInfo
    {
        public readonly uint Unknown; // As the name implies, not sure what this is
        // Not quite sure what this is for, but it's the same in every file.
        public readonly SoundTableData[] SoundHash;
        // The uint key corresponds to an Enum.Sound
        public readonly IDictionary<uint, SoundData> Data;

        public SoundTable(BinaryReader r)
        {
            Id = r.ReadUInt32();
            Unknown = r.ReadUInt32();
            SoundHash = r.ReadL32FArray(x => new SoundTableData(x));
            Data = r.Skip(2).ReadL16TMany<uint, SoundData>(sizeof(uint), x => new SoundData(x));
        }

        //: FileTypes.SoundTable
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                new MetaInfo($"{nameof(SoundTable)}: {Id:X8}", items: new List<MetaInfo> {
                    new MetaInfo("SoundHash", items: SoundHash.Select(x => {
                        var items = (x as IHaveMetaInfo).GetInfoNodes();
                        var name = items[0].Name.Replace("Sound ID: ", "");
                        items.RemoveAt(0);
                        return new MetaInfo(name, items: items);
                    })),
                    new MetaInfo($"Sounds", items: Data.Select(x => new MetaInfo($"{(Sound)x.Key}", items: (x.Value as IHaveMetaInfo).GetInfoNodes()))),
                })
            };
            return nodes;
        }
    }
}
