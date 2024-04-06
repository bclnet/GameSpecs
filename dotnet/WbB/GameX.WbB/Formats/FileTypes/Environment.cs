using GameX.WbB.Formats.Entity;
using GameX.Meta;
using GameX.Formats;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GameX.WbB.Formats.FileTypes
{
    /// <summary>
    /// These are client_portal.dat files starting with 0x0D. 
    /// These are basically pre-fab regions for things like the interior of a dungeon.
    /// </summary>
    [PakFileType(PakFileType.Environment)]
    public class Environment : FileType, IHaveMetaInfo
    {
        public readonly IDictionary<uint, CellStruct> Cells;

        public Environment(BinaryReader r)
        {
            Id = r.ReadUInt32(); // this will match fileId
            Cells = r.ReadL32TMany<uint, CellStruct>(sizeof(uint), x => new CellStruct(x));
        }

        //: FileTypes.Environment
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                new MetaInfo($"{nameof(Environment)}: {Id:X8}", items: Cells.Select(x => new MetaInfo($"{x.Key}", items: (x.Value as IHaveMetaInfo).GetInfoNodes()))),
            };
            return nodes;
        }
    }
}
