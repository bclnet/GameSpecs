using GameX.Meta;
using GameX.Formats;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GameX.WbB.Formats.Entity
{
    public class Generator : IHaveMetaInfo
    {
        public readonly string Name;
        public readonly uint Id;
        public readonly Generator[] Items;

        public Generator(BinaryReader r)
        {
            Name = r.ReadL16StringObfuscated(); r.Align();
            Id = r.ReadUInt32();
            Items = r.ReadL32FArray(x => new Generator(x));
        }

        //: Entity.Generator
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                Id != 0 ? new MetaInfo($"Id: {Id}") : null,
                !string.IsNullOrEmpty(Name) ? new MetaInfo($"Name: {Name}") : null,
            };
            if (Items.Length > 0) nodes.AddRange(Items.Select(x => new MetaInfo(x.Id != 0 ? $"{x.Id} - {x.Name}" : x.Name, items: (x as IHaveMetaInfo).GetInfoNodes())));
            return nodes;
        }
    }
}
