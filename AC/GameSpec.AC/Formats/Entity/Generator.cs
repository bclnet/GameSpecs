using GameSpec.Explorer;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GameSpec.AC.Formats.Entity
{
    public class Generator : IGetExplorerInfo
    {
        public readonly string Name;
        public readonly uint Id;
        public readonly Generator[] Items;

        public Generator(BinaryReader r)
        {
            Name = r.ReadObfuscatedString(); r.AlignBoundary();
            Id = r.ReadUInt32();
            Items = r.ReadL32Array(x => new Generator(x));
        }

        //: Entity.Generator
        List<ExplorerInfoNode> IGetExplorerInfo.GetInfoNodes(ExplorerManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<ExplorerInfoNode> {
                Id != 0 ? new ExplorerInfoNode($"Id: {Id}") : null,
                !string.IsNullOrEmpty(Name) ? new ExplorerInfoNode($"Name: {Name}") : null,
            };
            if (Items.Length > 0) nodes.AddRange(Items.Select(x => new ExplorerInfoNode(x.Id != 0 ? $"{x.Id} - {x.Name}" : x.Name, items: (x as IGetExplorerInfo).GetInfoNodes())));
            return nodes;
        }
    }
}
