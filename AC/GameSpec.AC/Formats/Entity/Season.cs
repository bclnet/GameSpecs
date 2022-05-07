using GameSpec.Explorer;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GameSpec.AC.Formats.Entity
{
    public class Season : IGetExplorerInfo
    {
        public readonly uint StartDate;
        public readonly string Name;

        public Season(BinaryReader r)
        {
            StartDate = r.ReadUInt32();
            Name = r.ReadL16String(Encoding.Default); r.AlignBoundary();
        }

        //: Entity.Season
        List<ExplorerInfoNode> IGetExplorerInfo.GetInfoNodes(ExplorerManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<ExplorerInfoNode> {
                new ExplorerInfoNode($"StartDate: {StartDate}"),
                new ExplorerInfoNode($"Name: {Name}"),
            };
            return nodes;
        }
    }
}
