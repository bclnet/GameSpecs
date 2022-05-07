using GameSpec.AC.Formats.Entity;
using GameSpec.Explorer;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GameSpec.AC.Formats.FileTypes
{
    /// <summary>
    /// These are client_portal.dat files starting with 0x0D. 
    /// These are basically pre-fab regions for things like the interior of a dungeon.
    /// </summary>
    [PakFileType(PakFileType.Environment)]
    public class Environment : FileType, IGetExplorerInfo
    {
        public readonly Dictionary<uint, CellStruct> Cells;

        public Environment(BinaryReader r)
        {
            Id = r.ReadUInt32(); // this will match fileId
            Cells = r.ReadL32Many<uint, CellStruct>(sizeof(uint), x => new CellStruct(x));
        }

        //: FileTypes.Environment
        List<ExplorerInfoNode> IGetExplorerInfo.GetInfoNodes(ExplorerManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<ExplorerInfoNode> {
                new ExplorerInfoNode($"{nameof(Environment)}: {Id:X8}", items: Cells.Select(x => new ExplorerInfoNode($"{x.Key}", items: (x.Value as IGetExplorerInfo).GetInfoNodes()))),
            };
            return nodes;
        }
    }
}
