using GameSpec.Metadata;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GameSpec.AC.Formats.Entity
{
    public class Generator : IGetMetadataInfo
    {
        public readonly string Name;
        public readonly uint Id;
        public readonly Generator[] Items;

        public Generator(BinaryReader r)
        {
            Name = r.ReadObfuscatedString(); r.Align();
            Id = r.ReadUInt32();
            Items = r.ReadL32Array(x => new Generator(x));
        }

        //: Entity.Generator
        List<MetadataInfo> IGetMetadataInfo.GetInfoNodes(MetadataManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<MetadataInfo> {
                Id != 0 ? new MetadataInfo($"Id: {Id}") : null,
                !string.IsNullOrEmpty(Name) ? new MetadataInfo($"Name: {Name}") : null,
            };
            if (Items.Length > 0) nodes.AddRange(Items.Select(x => new MetadataInfo(x.Id != 0 ? $"{x.Id} - {x.Name}" : x.Name, items: (x as IGetMetadataInfo).GetInfoNodes())));
            return nodes;
        }
    }
}
