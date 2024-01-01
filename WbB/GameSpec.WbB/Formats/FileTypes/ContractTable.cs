using GameSpec.WbB.Formats.Entity;
using GameSpec.Metadata;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GameSpec.WbB.Formats.FileTypes
{
    /// <summary>
    /// This is the client_portal.dat file 0x0E00001D
    /// </summary>
    [PakFileType(PakFileType.ContractTable)]
    public class ContractTable : FileType, IHaveMetaInfo
    {
        public const uint FILE_ID = 0x0E00001D;

        public readonly Dictionary<uint, Contract> Contracts;

        public ContractTable(BinaryReader r)
        {
            Id = r.ReadUInt32();
            Contracts = r.ReadL16Many<uint, Contract>(sizeof(uint), x => new Contract(x), offset: 2);
        }

        //: FileTypes.ContractTable
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                new MetaInfo($"{nameof(ContractTable)}: {Id:X8}", items: Contracts.Select(
                    x => new MetaInfo($"{x.Key} - {x.Value.ContractName}", items: (x.Value as IHaveMetaInfo).GetInfoNodes(tag: tag))
                ))
            };
            return nodes;
        }
    }
}
