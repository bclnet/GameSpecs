using GameSpec.WbB.Formats.Entity;
using GameSpec.Metadata;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GameSpec.WbB.Formats.FileTypes
{
    /// <summary>
    /// These are client_portal.dat files starting with 0x0D. 
    /// These are basically pre-fab regions for things like the interior of a dungeon.
    /// </summary>
    [PakFileType(PakFileType.Environment)]
    public class Environment : FileType, IGetMetadataInfo
    {
        public readonly Dictionary<uint, CellStruct> Cells;

        public Environment(BinaryReader r)
        {
            Id = r.ReadUInt32(); // this will match fileId
            Cells = r.ReadL32Many<uint, CellStruct>(sizeof(uint), x => new CellStruct(x));
        }

        //: FileTypes.Environment
        List<MetadataInfo> IGetMetadataInfo.GetInfoNodes(MetadataManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetadataInfo> {
                new MetadataInfo($"{nameof(Environment)}: {Id:X8}", items: Cells.Select(x => new MetadataInfo($"{x.Key}", items: (x.Value as IGetMetadataInfo).GetInfoNodes()))),
            };
            return nodes;
        }
    }
}
