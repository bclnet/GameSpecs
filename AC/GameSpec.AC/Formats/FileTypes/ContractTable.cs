using GameSpec.AC.Formats.Entity;
using GameSpec.Metadata;
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
    public class ContractTable : FileType, IGetMetadataInfo
    {
        public const uint FILE_ID = 0x0E00001D;

        public readonly Dictionary<uint, Contract> Contracts;

        public ContractTable(BinaryReader r)
        {
            Id = r.ReadUInt32();
            Contracts = r.ReadL16Many<uint, Contract>(sizeof(uint), x => new Contract(x), offset: 2);
        }

        //: FileTypes.ContractTable
        List<MetadataInfo> IGetMetadataInfo.GetInfoNodes(MetadataManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<MetadataInfo> {
                new MetadataInfo($"{nameof(ContractTable)}: {Id:X8}", items: Contracts.Select(
                    x => new MetadataInfo($"{x.Key} - {x.Value.ContractName}", items: (x.Value as IGetMetadataInfo).GetInfoNodes(tag: tag))
                ))
            };
            return nodes;
        }
    }
}
