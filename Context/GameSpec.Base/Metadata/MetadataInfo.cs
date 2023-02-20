using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace GameSpec.Metadata
{
    [DebuggerDisplay("{Name}, items: {Items.Count} [{Tag}]")]
    public class MetadataInfo
    {
        public string Name { get; set; }
        public object Tag { get; }
        public IEnumerable<MetadataInfo> Items { get; }
        public bool Clickable { get; set; }

        public MetadataInfo(string name, object tag = null, IEnumerable<MetadataInfo> items = null, bool clickable = false)
        {
            Name = name;
            Tag = tag;
            Items = items ?? Enumerable.Empty<MetadataInfo>();
            Clickable = clickable;
        }

        public static MetadataInfo WrapWithGroup<T>(IList<T> source, string groupName, IEnumerable<MetadataInfo> body)
            => source.Count == 0 ? null
            : source.Count == 1 ? body.First()
            : new MetadataInfo(groupName, body);
    }
}
