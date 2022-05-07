using GameSpec.AC.Formats.Entity;
using GameSpec.AC.Formats.Props;
using GameSpec.Explorer;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GameSpec.AC.Formats.FileTypes
{
    /// <summary>
    /// SoundTable files contain a listing of which Wav types to play in response to certain events.
    /// They are located in the client_portal.dat and are files starting with 0x20
    /// </summary>
    [PakFileType(PakFileType.SoundTable)]
    public class SoundTable : FileType, IGetExplorerInfo
    {
        public readonly uint Unknown; // As the name implies, not sure what this is
        // Not quite sure what this is for, but it's the same in every file.
        public readonly SoundTableData[] SoundHash;
        // The uint key corresponds to an Enum.Sound
        public readonly Dictionary<uint, SoundData> Data;

        public SoundTable(BinaryReader r)
        {
            Id = r.ReadUInt32();
            Unknown = r.ReadUInt32();
            SoundHash = r.ReadL32Array(x => new SoundTableData(x));
            Data = r.ReadL16Many<uint, SoundData>(sizeof(uint), x => new SoundData(x), offset: 2);
        }

        //: FileTypes.SoundTable
        List<ExplorerInfoNode> IGetExplorerInfo.GetInfoNodes(ExplorerManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<ExplorerInfoNode> {
                new ExplorerInfoNode($"{nameof(SoundTable)}: {Id:X8}", items: new List<ExplorerInfoNode> {
                    new ExplorerInfoNode("SoundHash", items: SoundHash.Select(x => {
                        var items = (x as IGetExplorerInfo).GetInfoNodes();
                        var name = items[0].Name.Replace("Sound ID: ", "");
                        items.RemoveAt(0);
                        return new ExplorerInfoNode(name, items: items);
                    })),
                    new ExplorerInfoNode($"Sounds", items: Data.Select(x => new ExplorerInfoNode($"{(Sound)x.Key}", items: (x.Value as IGetExplorerInfo).GetInfoNodes()))),
                })
            };
            return nodes;
        }
    }
}
