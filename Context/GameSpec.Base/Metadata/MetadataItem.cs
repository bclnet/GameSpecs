using GameSpec.Formats;
using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace GameSpec.Metadata
{
    [DebuggerDisplay("{Name}, items: {Items.Count}")]
    public class MetadataItem
    {
        [DebuggerDisplay("{Name}")]
        public class Filter
        {
            public string Name;
            public string Description;

            public Filter(string name, string description = "")
            {
                Name = name;
                Description = description;
            }

            public override string ToString() => Name;
        }

        public object Source { get; }
        public string Name { get; }
        public object Icon { get; }
        public object Tag { get; }
        public List<MetadataItem> Items { get; private set; }
        public PakFile PakFile { get; set; }

        public MetadataItem(object source, string name, object icon, object tag = null, List<MetadataItem> children = null)
        {
            Source = source;
            Name = name;
            Icon = icon;
            Tag = tag;
            Items = children ?? new List<MetadataItem>();
        }

        public MetadataItem Search(Func<MetadataItem, bool> predicate)
        {
            // if node is a leaf
            if (Items == null || Items.Count == 0) return predicate(this) ? this : null;
            // Otherwise if node is not a leaf
            else
            {
                var results = Items.Select(i => i.Search(predicate)).Where(i => i != null).ToList();
                if (results.Any())
                {
                    var result = (MetadataItem)MemberwiseClone();
                    result.Items = results;
                    return result;
                }
                return null;
            }
        }

        public MetadataItem FindByPath(string path, MetadataManager manager)
        {
            var paths = path.Split(new[] { '\\', '/', ':' }, 2);
            var node = Items.FirstOrDefault(x => x.Name == paths[0]);
            if (node != null && node.Source is FileSource z) z.Pak?.Open(node.Items, manager);
            return node == null || paths.Length == 1 ? node : node.FindByPath(paths[1], manager);
        }
    }
}
