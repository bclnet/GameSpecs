using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace GameSpec.Metadata
{
    [DebuggerDisplay("{Name}, items: {Items.Count} [{Tag}]")]
    public class MetaInfo
    {
        public string Name { get; set; }
        public object Tag { get; }
        public IEnumerable<MetaInfo> Items { get; }
        public bool Clickable { get; set; }

        public MetaInfo(string name, object tag = null, IEnumerable<MetaInfo> items = null, bool clickable = false)
        {
            Name = name;
            Tag = tag;
            Items = items ?? Enumerable.Empty<MetaInfo>();
            Clickable = clickable;
        }

        public static MetaInfo WrapWithGroup<T>(IList<T> source, string groupName, IEnumerable<MetaInfo> body)
            => source.Count == 0 ? null
            : source.Count == 1 ? body.First()
            : new MetaInfo(groupName, body);
    }
}
