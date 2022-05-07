using GameSpec.AC.Formats.Entity;
using GameSpec.Explorer;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GameSpec.AC.Formats.FileTypes
{
    /// <summary>
    /// This is the client_portal.dat file 0x0E00001D
    /// </summary>
    [PakFileType(PakFileType.ContractTable)]
    public class ContractTable : FileType, IGetExplorerInfo
    {
        public const uint FILE_ID = 0x0E00001D;

        public readonly Dictionary<uint, Contract> Contracts;

        public ContractTable(BinaryReader r)
        {
            Id = r.ReadUInt32();
            Contracts = r.ReadL16Many<uint, Contract>(sizeof(uint), x => new Contract(x), offset: 2);
        }

        //: FileTypes.ContractTable
        List<ExplorerInfoNode> IGetExplorerInfo.GetInfoNodes(ExplorerManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<ExplorerInfoNode> {
                new ExplorerInfoNode($"{nameof(ContractTable)}: {Id:X8}", items: Contracts.Select(
                    x => new ExplorerInfoNode($"{x.Key} - {x.Value.ContractName}", items: (x.Value as IGetExplorerInfo).GetInfoNodes(tag: tag))
                ))
            };
            return nodes;
        }
    }
}
